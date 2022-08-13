using UnityEngine;

namespace MeshDeformation
{
    [RequireComponent(typeof(MeshFilter))]
    public abstract class BaseDeformer : MonoBehaviour
    {
        [SerializeField] protected float _speed = 2.0f;
        [SerializeField] protected float _amplitude = 0.25f;
        protected Mesh Mesh;
    }
}