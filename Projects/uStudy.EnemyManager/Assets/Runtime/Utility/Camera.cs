#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public interface ICameraTransform : ITransform
    {
        Camera camera { get; }
    }
}
