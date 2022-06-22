using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public class ChildEnemiesRepository : Controller, IEnemyRepository
    {
        IEnemy[] IEnemyRepository.GetEnemies()
        {
            var list = new List<IEnemy>();
            foreach(var enemy in transform.GetComponentsInChildren<SimpleEnemyController>()) {
                list.Add(enemy);
            }
            return list.ToArray();
        }
    }
}
