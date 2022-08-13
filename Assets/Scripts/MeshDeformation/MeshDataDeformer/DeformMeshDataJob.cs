using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MeshDeformation.MeshDataDeformer
{
    [BurstCompile]
    public struct DeformMeshDataJob : IJobParallelFor
    {
        public NativeArray<VertexData> VertexData;
        public Mesh.MeshData OutputMesh;
        [ReadOnly] private readonly float _speed;
        [ReadOnly] private readonly float _amplitude;
        [ReadOnly] private readonly float _time;

        public DeformMeshDataJob(NativeArray<VertexData> vertexData,
            Mesh.MeshData outputMesh,
            float speed,
            float amplitude,
            float time)
        {
            VertexData = vertexData;
            OutputMesh = outputMesh;
            _speed = speed;
            _amplitude = amplitude;
            _time = time;
        }

        public void Execute(int index)
        {
            var outputVertices = OutputMesh.GetVertexData<VertexData>();
            var vertexData = VertexData[index];
            var position = vertexData.Position;
            var distance = Vector3.Distance(position, Vector3.zero);
            position.y = Mathf.Sin(_time * _speed + distance) * _amplitude;
            outputVertices[index] = new VertexData
            {
                Position = position,
                Normal = vertexData.Normal,
                Uv = vertexData.Uv
            };

            VertexData[index] = outputVertices[index];
        }
    }
}