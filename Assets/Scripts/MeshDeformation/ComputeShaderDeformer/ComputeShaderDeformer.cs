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
        private ComputeBuffer _computeBuffer;
        private AsyncGPUReadbackRequest _request;
        private NativeArray<VertexData> _vertexData;
        private readonly int _timePropertyId = Shader.PropertyToID("_Time");

        protected override void Awake()
        {
            base.Awake();
            if (!SystemInfo.supportsAsyncGPUReadback)
            {
                gameObject.SetActive(false);
                return;
            }

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
            var meshVertexCount = Mesh.vertexCount;
            _vertexData = new NativeArray<VertexData>(meshVertexCount, Allocator.Temp);
            var meshVertices = Mesh.vertices;
            var meshNormals = Mesh.normals;
            var meshUV = Mesh.uv;
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
                    Mesh.GetVertexAttributeFormat(VertexAttribute.Position), 3),
                new VertexAttributeDescriptor(VertexAttribute.Normal,
                    Mesh.GetVertexAttributeFormat(VertexAttribute.Normal), 3),
                new VertexAttributeDescriptor(VertexAttribute.TexCoord0,
                    Mesh.GetVertexAttributeFormat(VertexAttribute.TexCoord0), 2),
            };
            Mesh.SetVertexBufferParams(Mesh.vertexCount, layout);
        }

        private void SetComputeShaderValues()
        {
            _kernel = _computeShader.FindKernel("CSMain");
            _computeShader.GetKernelThreadGroupSizes(_kernel, out var threadX, out _, out _);
            _dispatchCount = Mathf.CeilToInt(Mesh.vertexCount / threadX + 1);
            _computeShader.SetBuffer(_kernel, "_VertexBuffer", _computeBuffer);
            _computeShader.SetFloat("_Speed", _speed);
            _computeShader.SetFloat("_Amplitude", _amplitude);
        }

        private ComputeBuffer CreateComputeBuffer()
        {
            var computeBuffer = new ComputeBuffer(Mesh.vertexCount, 32);
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
            Mesh.MarkDynamic();
            Mesh.SetVertexBufferData(_vertexData, 0, 0, _vertexData.Length);
            Mesh.RecalculateNormals();
        }

        private void OnDestroy()
        {
            _computeBuffer?.Release();
            _vertexData.Dispose();
        }
    }
}