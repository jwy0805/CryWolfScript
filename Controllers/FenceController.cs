using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class FenceController : CreatureController
{
    protected override void Init()
    {
        ObjectType = GameObjectType.Fence;
        // Instantiate Health bar
        Instantiate(Resources.Load<GameObject>("Prefabs/WorldObjects/HealthSlider"), transform);
        Util.GetOrAddComponent<UI_HealthBar>(gameObject);
    }
}
