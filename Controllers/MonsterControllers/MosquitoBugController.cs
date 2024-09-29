using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Google.Protobuf.Protocol;
using UnityEngine.Diagnostics;

public class MosquitoBugController : MonsterController
{
    protected override void Init()
    {
        SkillSubject = GameObject.Find("Subject").GetComponent<SkillSubject>();
        SkillSubject.AddObserver(this);
        ObjectType = GameObjectType.Monster;
        
        // Instantiate Health bar
        Managers.Resource.Instantiate("WorldObjects/HealthSlider", transform);
        Util.GetOrAddComponent<UI_HealthBar>(gameObject);
        
        UnitId = UnitId.MosquitoBug;
        AttackAnimValue = 4 / 5f;
    }
}
