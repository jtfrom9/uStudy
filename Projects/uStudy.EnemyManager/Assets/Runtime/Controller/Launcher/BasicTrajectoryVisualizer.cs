#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public class BasicTrajectoryVisualizer : Controller, ITrajectoryVisualizer
    {
        LineRenderer? lineRenderer;
        bool _visible;
        ProjectileConfig? _config;

        Vector3? _start;
        Vector3? _end;

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

            // this.UpdateAsObservable().Where(_ => _visible).Subscribe(_ =>
            // {
            //     _update(lineRenderer);
            // }).AddTo(this);

            // var task1 = this._start.ObserveEveryValueChanged(t => t.position).ToUniTask();
            // var task2 = this._end.ObserveEveryValueChanged(t => t.position).ToUniTask();
            // UniTask.WhenAny(task1, task2).ToObservable()
            //     .Subscribe(_ => _update(lineRenderer)).AddTo(this);
        }

        void _update()
        {
            if (this._start == null || this._end == null || this._config==null)
                return;
            if(lineRenderer==null)
                return;
            var points = new Vector3[] {
                this._start.Value,
                this._end.Value
            };
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }

        #region ITrajectoryVisualizer
        bool ITrajectoryVisualizer.visible { get => _visible; }

        void ITrajectoryVisualizer.SetStartTarget(Vector3 pos)
        {
            this._start = pos;
            _update();
        }
        void ITrajectoryVisualizer.SetEndTarget(Vector3 pos)
        {
            this._end = pos;
            _update();
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