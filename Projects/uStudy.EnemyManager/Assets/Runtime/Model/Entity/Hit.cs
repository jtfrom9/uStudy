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

    public enum HitObjectType
    {
        Single,
        Range
    }

    public interface IHitObject
    {
        HitObjectType Type { get; }
        float weight { get; }
        float power { get; }
        float speed { get; }
        Vector3 direction { get; }
        Vector3 position{ get; }
    }

    public interface IHitHandler
    {
        void OnHit(IHitObject hitObject);
    }
}