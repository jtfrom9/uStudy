using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    public class ChildEnemiesRepository : Controller, IEnemyControllerRepository
    {
        IEnemyController[] IEnemyControllerRepository.GetEnemyController()
        {
            var list = new List<IEnemyController>();
            foreach(var enemyController in transform.GetComponentsInChildren<SimpleEnemyController>()) {
                list.Add(enemyController);
            }
            return list.ToArray();
        }
    }
}
