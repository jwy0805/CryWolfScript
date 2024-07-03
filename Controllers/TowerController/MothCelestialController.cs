using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class MothCelestialController : MothMoonController
{
    private readonly ProjectileId _originalAttack = ProjectileId.MothMoonProjectile;
    private readonly ProjectileId _upgradedAttack = ProjectileId.MothCelestialPoison;
    
    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Tower;
        UnitId = UnitId.MothCelestial;
        CurrentAttack = _originalAttack;
    }
    
    public override void OnSkillUpdated(int id, GameObjectType type, SkillType skillType, int step)
    {
        if (id == (int)UnitId.MothCelestial && type == GameObjectType.Tower) CurrentAttack = _upgradedAttack;
    }
}
