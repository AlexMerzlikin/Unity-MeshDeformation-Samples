using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MeshDeformation.JobSystemDeformer
{
    public class JobSystemDeformer : BaseDeformer
    {
        private NativeArray<Vector3> _vertices;
        private bool _scheduled;
        private DeformerJob _job;
        private JobHandle _handle;

        protected override void Awake()
        {
            base.Awake();
            _vertices = new NativeArray<Vector3>(Mesh.vertices, Allocator.Persistent);
        }

        private void Update()
        {
            ScheduleJob();
        }

        private void LateUpdate()
        {
            CompleteJob();
        }

        private void OnDestroy()
        {
            _vertices.Dispose();
        }

        private void ScheduleJob()
        {
            if (_scheduled)
            {
                return;
            }

            _scheduled = true;
            _job = new DeformerJob(_speed, _amplitude, Time.time, _vertices);
            _handle = _job.Schedule(_vertices.Length, 64);
        }

        private void CompleteJob()
        {
            if (!_scheduled)
            {
                return;
            }

            _handle.Complete();
            Mesh.MarkDynamic();
            Mesh.SetVertices(_vertices);
            _scheduled = false;
        }
    }
}