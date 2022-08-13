using Unity.Burst;
using UnityEngine;

namespace MeshDeformation
{
    public static class DeformerUtilities
    {
        [BurstCompile]
        public static float CalculateDistance(Vector3 position) => 6f - Vector3.Distance(position, Vector3.zero);

        [BurstCompile]
        public static float CalculateDisplacement(float distance, float time, float speed, float amplitude) => Mathf.Sin(time * speed + distance) * amplitude;

    }
}