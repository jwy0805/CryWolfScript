using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SeedProjectileController : ProjectileController
{
    protected override void Init()
    {
        base.Init();
        Speed = 15.0f;
    }
}
