using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamegeEffectTest : MonoBehaviour
{
    public async void Show() {
        var factory = FindObjectOfType<Effect.EffectFactory>();
        var effect = factory.Create(Vector3.zero, Camera.main.transform.position);

        await effect.Play();
        effect.Dispose();
    }
}
