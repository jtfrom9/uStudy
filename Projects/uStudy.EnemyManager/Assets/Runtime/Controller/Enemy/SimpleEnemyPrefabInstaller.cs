#nullable enable

using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Cysharp.Threading.Tasks;
using VContainer;

namespace Hedwig.Runtime
{
    public class SimpleEnemyPrefabInstaller : MonoBehaviour
    {
        [Inject]
        IEnemyManager? enemyManager;

        public void Awake()
        {
            var children = transform.GetComponentsInChildren<SimpleEnemyController>();
            foreach(var enemy in children) {
                enemyManager?.AddEnemy(enemy);
            }
        }
    }
}
