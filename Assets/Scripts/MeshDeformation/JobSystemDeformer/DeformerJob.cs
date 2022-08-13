using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MeshDeformation.JobSystemDeformer
{
    [BurstCompile]
    public struct DeformerJob : IJobParallelFor
    {
        private NativeArray<Vector3> _vertices;
        [ReadOnly] private readonly float _speed;
        [ReadOnly] private readonly float _amplitude;
        [ReadOnly] private readonly float _time;

        public DeformerJob(float speed, float amplitude, float time, NativeArray<Vector3> vertices)
        {
            _vertices = vertices;
            _speed = speed;
            _amplitude = amplitude;
            _time = time;
        }

        public void Execute(int index)
        {
            var position = _vertices[index];
            var distance = DeformerUtilities.CalculateDistance(position);
            position.y = DeformerUtilities.CalculateDisplacement(distance, _time, _speed, _amplitude);
            _vertices[index] = position;
        }
    }
}