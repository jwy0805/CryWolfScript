using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class MeteorController : EffectController
{
    protected override void Init()
    {
        StartTime = Time.time;
        Duration = 2f;
        DamageTime = 0.75f;
    }
}
