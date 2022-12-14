using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MeshDeformation.MeshDataDeformer
{
    [BurstCompile]
    public struct DeformMeshDataJob : IJobParallelFor
    {
        public Mesh.MeshData OutputMesh;
        [ReadOnly] private Mesh.MeshData _inputMesh;
        [ReadOnly] private readonly float _speed;
        [ReadOnly] private readonly float _amplitude;
        [ReadOnly] private readonly float _time;

        public DeformMeshDataJob(
            Mesh.MeshData inputMesh,
            Mesh.MeshData outputMesh,
            float speed,
            float amplitude,
            float time)
        {
            _inputMesh = inputMesh;
            OutputMesh = outputMesh;
            _speed = speed;
            _amplitude = amplitude;
            _time = time;
        }

        public void Execute(int index)
        {
            var inputVertexData = _inputMesh.GetVertexData<VertexData>();
            var outputVertexData = OutputMesh.GetVertexData<VertexData>();
            var vertexData = inputVertexData[index];
            var position = vertexData.Position;
            position.y = DeformerUtilities.CalculateDisplacement(position, _time, _speed, _amplitude);
            outputVertexData[index] = new VertexData
            {
                Position = position,
                Normal = vertexData.Normal,
                Uv = vertexData.Uv
            };
        }
    }
}