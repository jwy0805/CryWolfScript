using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;

public class SunfloraPixieController : SunflowerFairyController
{
    private ProjectileId _currentAttack;
    private readonly ProjectileId _originalAttack = ProjectileId.SunfloraPixieProjectile;
    private readonly ProjectileId _upgradedAttack = ProjectileId.SunfloraPixieFire;
    
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.SunfloraPixie;
        _currentAttack = _originalAttack;
        AttackAnimValue = 2 / 3f;
    }
    
    public override void OnSkillUpdated(int id, GameObjectType type, SkillType skillType, int step)
    {
        if (id == (int)UnitId.SunfloraPixie && type == GameObjectType.Tower) _currentAttack = _upgradedAttack;
    }
}
