using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class StatueController : CreatureController
{
    private readonly float _destroySpeed = 2f;
    
    // 개별 UnitTest
    protected override async void Init()
    {
        try
        {
            ObjectType = GameObjectType.MonsterStatue;
            // Instantiate Health bar
            await Managers.Resource.Instantiate("WorldObjects/HealthSlider", transform);
            Util.GetOrAddComponent<UI_HealthBar>(gameObject);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    protected override void UpdateDie()
    {
        transform.position += Vector3.down * (_destroySpeed * Time.deltaTime);
    }
}
