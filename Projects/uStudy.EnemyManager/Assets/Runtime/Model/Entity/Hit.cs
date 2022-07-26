#nullable enable

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Hedwig.Runtime
{
    public class HitTag
    {
        public const string Environment = "Environment";
        public const string Character = "Character";
        public const string Projectile = "Projectile";
    }
}