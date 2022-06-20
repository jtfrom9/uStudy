#nullable enable

using UnityEngine;

namespace Hedwig.Runtime
{
    public class DummyEffectFactory : IEffectFactory
    {
        #region DummyEffectFactory
        public IDamageEffect? CreateDamageEffect(Transform parent, int damage)
        {
            return null;
        }
        public IHitEffect? CreateHitEffect(Transform parent, Vector3 position, Vector3 normal)
        {
            return null;
        }
        #endregion
    }
}
