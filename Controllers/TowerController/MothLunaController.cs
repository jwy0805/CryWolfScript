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
        base.Init();
        // Instantiate Health bar
        var healthCircle = Util.FindChild(gameObject, "HealthCircle", true, true);
        var uIHealthCircle = Util.GetOrAddComponent<UI_HealthCircle>(healthCircle);
        Managers.Game.Despawn(healthCircle);
        Destroy(uIHealthCircle);
        Instantiate(Resources.Load<GameObject>("Prefabs/WorldObjects/HealthSlider"), transform);
        Util.GetOrAddComponent<UI_HealthBar>(gameObject);
        
        ObjectType = GameObjectType.Tower;
        UnitId = UnitId.MothLuna;
    }
}
