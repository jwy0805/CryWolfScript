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
        base.Init();
        // Instantiate Health bar
        Destroy(GetComponent<UI_HealthCircle>());
        var healthCircle = Util.FindChild(gameObject, "HealthCircle(Clone)", true, true);
        Managers.Game.Despawn(healthCircle);
        Instantiate(Resources.Load<GameObject>("Prefabs/WorldObjects/HealthSlider"), transform);
        Util.GetOrAddComponent<UI_HealthBar>(gameObject);
        
        UnitId = UnitId.MosquitoBug;
        AttackAnimValue = 4 / 5f;
    }
}
