using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GreenGateController : EffectController
{
    protected override void Init()
    {
        StartTime = Time.time;
        Duration = 2f;
        DamageTime = 1.5f;
    }
    
    protected override void Update()
    {
        transform.position = parent.transform.position + Vector3.up * 1.0f;
        transform.rotation = Quaternion.Euler(0, 0, 0);
        base.Update();
    }
}
