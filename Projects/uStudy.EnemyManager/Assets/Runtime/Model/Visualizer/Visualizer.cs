#nullable enable

using System;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Hedwig.Runtime
{
    public interface IFreeCursorVisualizer : ITransformProvider
    {
        void Initialize();
        void Move(Vector3 pos);
    }

    public interface IGlobalVisualizerFactory
    {
        IFreeCursorVisualizer CreateFreeCursor();
    }

    public interface ITargetVisualizer: IDisposable
    {
        void Initialize(ITransformProvider target);
    }

    public interface ITargetVisualizerFactory
    {
        IEnumerable<ITargetVisualizer> CreateVisualizers(ITransformProvider target);
    }
}