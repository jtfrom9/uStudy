#nullable enable
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public class DummySelectorFactory : ISelectorFactory
    {
        ISelector? ISelectorFactory.Create(ICharactor charactor)
        {
            return null;
        }
    }
}