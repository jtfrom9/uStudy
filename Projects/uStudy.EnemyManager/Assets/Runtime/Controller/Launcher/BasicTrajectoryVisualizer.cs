#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Hedwig.Runtime
{
    public class BasicTrajectoryVisualizer : Controller, ITrajectoryVisualizer
    {
        LineRenderer? lineRenderer;
        bool _visible;
        Transform? _start;
        Transform? _end;
        ProjectileConfig? _config;

        void Awake()
        {
            var child = new GameObject("BasicTrajectory");
            lineRenderer = child.AddComponent<LineRenderer>();
            lineRenderer.positionCount = 0;
            child.transform.SetParent(transform, false);
        }

        void Start()
        {
            if (lineRenderer == null) return;

            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;

            this.UpdateAsObservable().Where(_ => _visible).Subscribe(_ =>
            {
                _update(lineRenderer);
            }).AddTo(this);
        }

        void _update(LineRenderer lineRenderer)
        {
            if (this._start == null || this._end == null || this._config==null)
                return;
            var points = new Vector3[] {
                this._start.position,
                this._end.position
            };
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }

        #region ITrajectoryVisualizer
        bool ITrajectoryVisualizer.visible { get => _visible; }

        void ITrajectoryVisualizer.SetStartTarget(Transform target)
        {
            this._start = target;
        }
        void ITrajectoryVisualizer.SetEndTarget(Transform target)
        {
            this._end = target;
        }
        void ITrajectoryVisualizer.SetConfig(ProjectileConfig? config)
        {
            this._config = config;
        }

        void ITrajectoryVisualizer.Show(bool v) {
            _visible = v;
            if (lineRenderer != null)
            {
                if (v)
                {
                    lineRenderer.enabled = true;
                } else {
                    lineRenderer.positionCount = 0;
                    lineRenderer.enabled = false;
                }
            }
        }
        #endregion
    }
}