using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshDeformation.MeshDataDeformer
{
    /// <summary>
    /// Jobified mesh deformation using MeshData API
    /// </summary>
    public class MeshDataDeformer : BaseDeformer
    {
        [SerializeField] private int _innerloopBatchCount = 64;

        private Mesh _mesh;
        private NativeArray<VertexData> _vertexData;
        private Vector3 _positionToDeform;
        private Mesh.MeshDataArray _meshDataArray;
        private Mesh.MeshDataArray _meshDataArrayOutput;
        private VertexAttributeDescriptor[] _layout;
        private SubMeshDescriptor _subMeshDescriptor;
        private DeformMeshDataJob _job;
        private JobHandle _jobHandle;
        private bool _scheduled;
        private NativeArray<ushort> _sourceIndexData;
        private NativeArray<ushort> _outputIndexData;

        private void Awake()
        {
            _mesh = GetComponent<MeshFilter>().mesh;
            CreateVertexDataArray();
            CreateMeshData();
        }

        private void CreateMeshData()
        {
            _meshDataArray = Mesh.AcquireReadOnlyMeshData(_mesh);
            _layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position,
                    _meshDataArray[0].GetVertexAttributeFormat(VertexAttribute.Position), 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal,
                    _meshDataArray[0].GetVertexAttributeFormat(VertexAttribute.Normal), 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                    _meshDataArray[0].GetVertexAttributeFormat(VertexAttribute.TexCoord0), 2),
            };
            _sourceIndexData = _meshDataArray[0].GetIndexData<ushort>();
            _subMeshDescriptor =
                new SubMeshDescriptor(0, _meshDataArray[0].GetSubMesh(0).indexCount, MeshTopology.Triangles)
                {
                    firstVertex = 0, vertexCount = _meshDataArray[0].vertexCount
                };
        }

        private void CreateVertexDataArray()
        {
            _vertexData = new NativeArray<VertexData>(_mesh.vertexCount, Allocator.Persistent);
            for (var i = 0; i < _mesh.vertexCount; ++i)
            {
                var v = new VertexData
                {
                    Position = _mesh.vertices[i],
                    Normal = _mesh.normals[i],
                    Uv = _mesh.uv[i]
                };
                _vertexData[i] = v;
            }
        }

        private void Update()
        {
            ScheduleJob();
        }

        private void LateUpdate()
        {
            CompleteJob();
        }

        private void ScheduleJob()
        {
            if (_scheduled)
            {
                return;
            }

            _scheduled = true;
            _meshDataArrayOutput = Mesh.AllocateWritableMeshData(1);
            var outputMesh = _meshDataArrayOutput[0];
            var meshData = _meshDataArray[0];
            outputMesh.SetIndexBufferParams(meshData.GetSubMesh(0).indexCount, meshData.indexFormat);
            outputMesh.SetVertexBufferParams(meshData.vertexCount, _layout);
            _job = new DeformMeshDataJob(
                _vertexData,
                outputMesh,
                _speed,
                _amplitude,
                Time.time
            );

            _jobHandle = _job.Schedule(meshData.vertexCount, _innerloopBatchCount);
        }

        private void CompleteJob()
        {
            if (!_scheduled)
            {
                return;
            }

            _jobHandle.Complete();
            UpdateMesh(_job.OutputMesh);
            _scheduled = false;
        }

        private void UpdateMesh(Mesh.MeshData meshData)
        {
            _outputIndexData = meshData.GetIndexData<ushort>();
            _sourceIndexData.CopyTo(_outputIndexData);
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,
                _subMeshDescriptor,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontNotifyMeshUsers);
            _mesh.MarkDynamic();
            Mesh.ApplyAndDisposeWritableMeshData(
                _meshDataArrayOutput,
                _mesh,
                MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices);
            _mesh.RecalculateNormals();
        }

        private void OnDestroy()
        {
            _vertexData.Dispose();
            _meshDataArray.Dispose();
            _sourceIndexData.Dispose();
            _outputIndexData.Dispose();
        }
    }
}