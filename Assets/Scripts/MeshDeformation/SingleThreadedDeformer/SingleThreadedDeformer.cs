using UnityEngine;

namespace MeshDeformation.SingleThreadedDeformer
{
    public class SingleThreadedDeformer : BaseDeformer
    {
        private Vector3[] _vertices;

        private void Awake()
        {
            Mesh = GetComponent<MeshFilter>().mesh;
            _vertices = Mesh.vertices;
        }

        private void Update()
        {
            Deform();
        }

        private void Deform()
        {
            for (var i = 0; i < _vertices.Length; i++)
            {
                var position = _vertices[i];
                var distance = DeformerUtilities.CalculateDistance(position);
                position.y = DeformerUtilities.CalculateDisplacement(distance, Time.time, _speed, _amplitude);
                _vertices[i] = position;
            }

            Mesh.MarkDynamic();
            Mesh.vertices = _vertices;
            Mesh.RecalculateNormals();
        }
    }
}