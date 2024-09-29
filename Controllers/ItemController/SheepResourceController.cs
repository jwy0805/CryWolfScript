using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SheepResourceController : ResourceController
{
    protected override void Init()
    {
        DestPos = Managers.Object.Find(obj => 
            obj.TryGetComponent(out PlayerController pc) && pc.Faction == Faction.Sheep).transform.position;
        InitPos = transform.position;
    }
}
