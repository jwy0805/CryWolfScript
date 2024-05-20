using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RollPoisonController : EffectController
{
    protected override void Init()
    {
        StartTime = Time.time;
        Duration = 1.5f;
        DamageTime = 0.5f;
        Transform t = transform;
        t.position = parent.transform.position;
        t.rotation = parent.transform.rotation;   
    }
}
