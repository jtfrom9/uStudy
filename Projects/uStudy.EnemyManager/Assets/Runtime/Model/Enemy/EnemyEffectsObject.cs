#nullable enable

using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/Effects", fileName = "EnemyEffects")]
    public class EnemyEffectsObject : ScriptableObject
    {
        public List<DamageEffect> damageEffects = new List<DamageEffect>();
        public List<HitEffect> hitEffects = new List<HitEffect>();

        IEnumerable<IEffect?> createEffects(IEnemy enemy, IHitObject? hitObject, DamageEvent e)
        {
            foreach (var damageEffect in damageEffects)
            {
                yield return damageEffect.Create(enemy.controller, e.damage);
            }
            foreach (var hitEffect in hitEffects)
            {
                yield return hitEffect.Create(enemy.controller,
                    hitObject?.position ?? enemy.controller.transform.Position,
                    Vector3.zero);
            }
        }

        public IEffect[] CreateEffects(IEnemy enemy, IHitObject? hitObject, in DamageEvent e)
            => createEffects(enemy, hitObject, e)
                .WhereNotNull()
                .ToArray();
    }
}