using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectWithRotationController : EffectController
{
    protected override void Init()
    {
        base.Init();
        transform.rotation = Parent.transform.rotation;
    }
}
