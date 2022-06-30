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

        // Vector3 SampleCurve(Vector3 start, Vector3 end, Vector3 control, float t)
        // {
        //     // Interpolate along line S0: control - start;
        //     Vector3 Q0 = Vector3.Lerp(start, control, t);
        //     // Interpolate along line S1: S1 = end - control;
        //     Vector3 Q1 = Vector3.Lerp(control, end, t);
        //     // Interpolate along line S2: Q1 - Q0
        //     Vector3 Q2 = Vector3.Lerp(Q0, Q1, t);
        //     return Q2; // Q2 is a point on the curve at time t
        // }

        // Vector3[] makePoints(Vector3 start, Vector3 end)
        // {
        //     var control = ((start + end) / 2) + Vector3.up * 5;
        //     var points = new List<Vector3>() {
        //         start
        //     };
        //     for (var i = 1; i <= 10; i++)
        //     {
        //         var mid = SampleCurve(start, end, control, (float)i / 10);
        //         points.Add(mid);
        //     }
        //     points.Add(end);
        //     return points.ToArray();
        // }

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
            // var points = new Vector3[] {
            //     this._start.Position,
            //     this._end.Position
            // };
            var points = _config.trajectory?.MakePoints(this._start.Position, this._end.Position) ?? new Vector3[] { };
            // var points = makePoints(this._start.Position, this._end.Position);
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