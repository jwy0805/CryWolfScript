using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class HauntController : SoulController
{
    protected ProjectileId CurrentAttack;
    protected ProjectileId OriginalAttack;
    protected ProjectileId UpgradedAttack;
        
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.Haunt;
    }
}
