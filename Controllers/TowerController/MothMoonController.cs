using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class MothMoonController : MothLunaController
{
    protected ProjectileId CurrentAttack;
    
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.MothMoon;
        State = State.Idle;
        CurrentAttack = ProjectileId.MothMoonProjectile;
    }
}
