using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class StatueController : CreatureController
{
    private readonly float _destroySpeed = 2f;
    
    // 개별 UnitTest
    protected override void Init()
    {
        ObjectType = GameObjectType.MonsterStatue;
        // Instantiate Health bar
        Managers.Resource.Instantiate("WorldObjects/HealthSlider", transform);
        Util.GetOrAddComponent<UI_HealthBar>(gameObject);
    }
    
    protected override void UpdateDie()
    {
        transform.position += Vector3.down * (_destroySpeed * Time.deltaTime);
    }
}
