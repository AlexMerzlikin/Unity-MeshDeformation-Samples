using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

namespace MeshDeformation.ComputeShaderDeformer
{
    public class ComputeShaderDeformer : BaseDeformer
    {
        [SerializeField] private ComputeShader _computeShader;
        private int _kernel;
        private int _dispatchCount;
        private bool _isDispatched;
        private Mesh _mesh;
        private ComputeBuffer _computeBuffer;
        private AsyncGPUReadbackRequest _request;
        private NativeArray<VertexData> _vertexData;
        private readonly int _timePropertyId = Shader.PropertyToID("_Time");

        private void Awake()
        {
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                gameObject.SetActive(false);
                return;
            }

            var meshFilter = GetComponent<MeshFilter>();
            _mesh = meshFilter.mesh;
            CreateVertexData();
            SetMeshVertexBufferParams();
            _computeBuffer = CreateComputeBuffer();
            SetComputeShaderValues();
        }

        private void Update()
        {
            Request();
        }

        private void LateUpdate()
        {
            TryGetResult();
        }

        private void CreateVertexData()
        {
            var meshVertexCount = _mesh.vertexCount;
            _vertexData = new NativeArray<VertexData>(meshVertexCount, Allocator.Temp);
            var meshVertices = _mesh.vertices;
            var meshNormals = _mesh.normals;
            var meshUV = _mesh.uv;
            for (var i = 0; i < meshVertexCount; ++i)
            {
                var v = new VertexData
                {
                    Position = meshVertices[i],
                    Normal = meshNormals[i],
                    Uv = meshUV[i]
                };
                _vertexData[i] = v;
            }
        }

        private void SetMeshVertexBufferParams()
        {
            var layout = new[]
            {
                new VertexAttributeDescriptor(VertexAttribute.Position,
                    _mesh.GetVertexAttributeFormat(VertexAttribute.Position), 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal,
                    _mesh.GetVertexAttributeFormat(VertexAttribute.Normal), 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                    _mesh.GetVertexAttributeFormat(VertexAttribute.TexCoord0), 2),
            };
            _mesh.SetVertexBufferParams(_mesh.vertexCount, layout);
        }

        private void SetComputeShaderValues()
        {
            _kernel = _computeShader.FindKernel("CSMain");
            _computeShader.GetKernelThreadGroupSizes(_kernel, out var threadX, out _, out _);
            _dispatchCount = Mathf.CeilToInt(_mesh.vertexCount / threadX + 1);
            _computeShader.SetBuffer(_kernel, "_VertexBuffer", _computeBuffer);
            _computeShader.SetFloat("_Speed", _speed);
            _computeShader.SetFloat("_Amplitude", _amplitude);
        }

        private ComputeBuffer CreateComputeBuffer()
        {
            var computeBuffer = new ComputeBuffer(_mesh.vertexCount, 32);
            if (_vertexData.IsCreated)
            {
                computeBuffer.SetData(_vertexData);
            }

            return computeBuffer;
        }

        private void Request()
        {
            if (_isDispatched)
            {
                return;
            }

            _isDispatched = true;
            _computeShader.SetFloat(_timePropertyId, Time.time);
            _computeShader.Dispatch(_kernel, _dispatchCount, 1, 1);
            _request = AsyncGPUReadback.Request(_computeBuffer);
        }

        private void TryGetResult()
        {
            if (!_isDispatched || !_request.done)
            {
                return;
            }

            _isDispatched = false;
            if (_request.hasError)
            {
                return;
            }

            _vertexData = _request.GetData<VertexData>();
            _mesh.MarkDynamic();
            _mesh.SetVertexBufferData(_vertexData, 0, 0, _vertexData.Length);
            _mesh.RecalculateNormals();
        }

        private void OnDestroy()
        {
            _computeBuffer?.Release();
            _vertexData.Dispose();
        }
    }
}