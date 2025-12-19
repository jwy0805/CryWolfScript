using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class FenceController : CreatureController
{
    private readonly float _destroySpeed = 1f;
    
    protected override async void Init()
    {
        try
        {
            ObjectType = GameObjectType.Fence;
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
    
    public override void OnDead(float time = 2f)
    {
        StartCoroutine(Despawn(gameObject, time));
        _ = Managers.Sound.PlaySfx3D("InGame/fence_down", transform.position);
    }
}
