using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class StatueController : CreatureController
{
    // protected override void Init()
    // {
    //     ObjectType = GameObjectType.MonsterStatue;
    // }

    // 개별 UnitTest
    protected override void Init()
    {
        ObjectType = GameObjectType.MonsterStatue;
        var unitId = (UnitId)Enum.Parse(typeof(UnitId), name.Replace("Statue", ""));
        var position = transform.position;
        
        var posInfo = new PositionInfo
        {
            PosX = position.x,
            PosY = position.y,
            PosZ = position.z - 2
        };
        
        Managers.Network.Send(new C_Spawn
        {
            Num = (int)unitId,
            PosInfo = posInfo,
            Register = false,
            Type = ObjectType,
            Way = SpawnWay.North,
        });
    }

    protected override void Update() { }
}
