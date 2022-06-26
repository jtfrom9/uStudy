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

        ITransform? _start;
        ITransform? _end;

        CompositeDisposable disposables = new CompositeDisposable();

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
        }

        void redraw()
        {
            if (lineRenderer == null)
                return;
            if (!_visible || this._start == null || this._end == null || this._config == null)
            {
                lineRenderer.positionCount = 0;
                lineRenderer.enabled = false;
                return;
            }
            var points = new Vector3[] {
                this._start.Position,
                this._end.Position
            };
            lineRenderer.enabled = true;
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
        }

        void clearHandler()
        {
            disposables.Clear();
        }

        void setupHandler()
        {
            if (this._start != null)
            {
                disposables.Add(this._start.OnPositionChanged
                    .Subscribe(
                        _ => redraw(),
                        () => { // onComplete
                            this._start = null;
                            redraw();
                        }
                ));
            }
            if (this._end != null)
            {
                disposables.Add(this._end.OnPositionChanged
                    .Subscribe(
                        _ => redraw(),
                        () => {  // onComplete
                            this._end = null;
                            redraw();
                        }
                ));
            }
        }

        #region ITrajectoryVisualizer
        bool ITrajectoryVisualizer.visible { get => _visible; }

        void ITrajectoryVisualizer.SetStartTarget(ITransform? target)
        {
            this.clearHandler();
            this._start = target;
            this.setupHandler();
            this.redraw();
        }
        void ITrajectoryVisualizer.SetEndTarget(ITransform? target)
        {
            this.clearHandler();
            this._end = target;
            this.setupHandler();
            this.redraw();
        }
        void ITrajectoryVisualizer.SetConfig(ProjectileConfig? config)
        {
            this._config = config;
            redraw();
        }
        void ITrajectoryVisualizer.Show(bool v)
        {
            _visible = v;
            redraw();
        }
        #endregion
    }
}