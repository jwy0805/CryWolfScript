using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Google.Protobuf.Protocol;
using UnityEngine.Serialization;

public class TowerController : CreatureController
{
    protected override void Init()
    {
        base.Init();
        ObjectType = GameObjectType.Tower;
    }
    
    protected override void UpdateIdle()
    {
        if (TryGetComponent(out UI_CanSpawn uiCanSpawn)) uiCanSpawn.background.color = new Color(0, 0, 0, 0f);
    }
}
