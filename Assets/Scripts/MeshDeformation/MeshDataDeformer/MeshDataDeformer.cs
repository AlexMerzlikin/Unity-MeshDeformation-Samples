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

        private NativeArray<VertexData> _vertexData;
        private Vector3 _positionToDeform;
        private Mesh.MeshDataArray _meshDataArray;
        private Mesh.MeshDataArray _meshDataArrayOutput;
        private VertexAttributeDescriptor[] _layout;
        private SubMeshDescriptor _subMeshDescriptor;
        private DeformMeshDataJob _job;
        private JobHandle _jobHandle;
        private bool _scheduled;

        protected override void Awake()
        {
            base.Awake();
            CreateMeshData();
        }

        private void CreateMeshData()
        {
            _meshDataArray = Mesh.AcquireReadOnlyMeshData(Mesh);
            _layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position,
                    _meshDataArray[0].GetVertexAttributeFormat(VertexAttribute.Position), 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal,
                    _meshDataArray[0].GetVertexAttributeFormat(VertexAttribute.Normal), 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                    _meshDataArray[0].GetVertexAttributeFormat(VertexAttribute.TexCoord0), 2),
            };
            _subMeshDescriptor =
                new SubMeshDescriptor(0, _meshDataArray[0].GetSubMesh(0).indexCount, MeshTopology.Triangles)
                {
                    firstVertex = 0, vertexCount = _meshDataArray[0].vertexCount
                };
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
            _meshDataArray = Mesh.AcquireReadOnlyMeshData(Mesh);
            var meshData = _meshDataArray[0];
            outputMesh.SetIndexBufferParams(meshData.GetSubMesh(0).indexCount, meshData.indexFormat);
            outputMesh.SetVertexBufferParams(meshData.vertexCount, _layout);
            _vertexData = meshData.GetVertexData<VertexData>();
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
            var outputIndexData = meshData.GetIndexData<ushort>();
            _meshDataArray[0].GetIndexData<ushort>().CopyTo(outputIndexData);
            _meshDataArray.Dispose();
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0,
                _subMeshDescriptor,
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontResetBoneBounds |
                MeshUpdateFlags.DontNotifyMeshUsers);
            Mesh.MarkDynamic();
            Mesh.ApplyAndDisposeWritableMeshData(
                _meshDataArrayOutput,
                Mesh,
                MeshUpdateFlags.DontRecalculateBounds |
                MeshUpdateFlags.DontValidateIndices |
                MeshUpdateFlags.DontResetBoneBounds |
                MeshUpdateFlags.DontNotifyMeshUsers);
            Mesh.RecalculateNormals();
        }
    }
}