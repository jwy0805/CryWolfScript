using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;
using Google.Protobuf.Protocol;

public class MothLunaController : TowerController
{
    protected override void Init()
    {
        SkillSubject = GameObject.Find("Subject").GetComponent<SkillSubject>();
        SkillSubject.AddObserver(this);
        ObjectType = GameObjectType.Tower;
        
        // Instantiate Health bar
        Managers.Resource.Instantiate("WorldObjects/HealthSlider", transform);
        Util.GetOrAddComponent<UI_HealthBar>(gameObject);
        
        ObjectType = GameObjectType.Tower;
        UnitId = UnitId.MothLuna;
        AttackAnimValue = 2 / 3f;
    }
}
