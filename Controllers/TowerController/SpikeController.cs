using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;
using Google.Protobuf.Protocol;

public class SpikeController : ShellController
{
    protected ProjectileId CurrentAttack = ProjectileId.SpikeProjectile;
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Spike;
        AttackAnimValue = 5 / 6f;
    }
}
