using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OneTimeEffectController : EffectController
{
    protected override void Init()
    {
        StartTime = Time.time;
        Duration = 2.0f;
    }
}
