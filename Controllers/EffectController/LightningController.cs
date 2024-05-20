using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightningController : EffectController
{
    protected override void Init()
    {
        StartTime = Time.time;
        Duration = 1f;
    }
}
