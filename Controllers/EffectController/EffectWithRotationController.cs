using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectWithRotationController : EffectController
{
    protected override void Init()
    {
        base.Init();
        if (Master != null) transform.rotation = Master.transform.rotation;
    }
}
