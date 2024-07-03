using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class LurkerController : MonsterController
{
    protected ProjectileId CurrentAttack;
    
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Lurker;
        CurrentAttack = ProjectileId.BasicProjectile;
    }
}
