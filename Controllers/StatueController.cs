using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class StatueController : CreatureController
{
    // 개별 UnitTest
    protected override void Init()
    {
        ObjectType = GameObjectType.MonsterStatue;
        // Instantiate Health bar
        Instantiate(Resources.Load<GameObject>("Prefabs/WorldObjects/HealthSlider"), transform);
        Util.GetOrAddComponent<UI_HealthBar>(gameObject);
    }
}
