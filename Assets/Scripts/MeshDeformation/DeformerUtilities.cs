using Unity.Burst;
using UnityEngine;

namespace MeshDeformation
{
    public static class DeformerUtilities
    {
        [BurstCompile]
        public static float CalculateDisplacement(Vector3 position, float time, float speed, float amplitude)
        {
            var distance = 6f - Vector3.Distance(position, Vector3.zero);
            return Mathf.Sin(time * speed + distance) * amplitude;
        }
    }
}