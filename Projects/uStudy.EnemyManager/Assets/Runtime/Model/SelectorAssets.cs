#nullable enable

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Hedwig.Runtime
{
    [CreateAssetMenu(menuName = "Hedwig/SelectorAssets", fileName = "SelectorAssets")]
    public class SelectorAssets : ScriptableObject, ISelectorFactory
    {
        [SerializeField, InterfaceType(typeof(ISelector))]
        Component? selectorPrefab;

        #region ISelectorFactory
        ISelector? ISelectorFactory.Create(ICharactor charactor)
        {
            if (selectorPrefab == null)
            {
                return null;
            }
            var selector = Instantiate(selectorPrefab) as ISelector;
            selector?.Initialize(charactor.transform, charactor.distanceToGround);
            return selector;
        }
        #endregion
    }
}
