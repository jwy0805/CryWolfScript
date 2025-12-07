using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SheepResourceController : ResourceController
{
    protected override void Init()
    {
        var sheepPlayer = Managers.Object.Find(obj =>
            obj != null && 
            obj.TryGetComponent(out PlayerController pc) &&
            pc.Faction == Faction.Sheep);

        if (sheepPlayer == null)
        {
            Debug.LogWarning("SheepResourceController: Cant find Sheep PlayerController");
            DestPos = transform.position;
        }
        else
        {
            DestPos = sheepPlayer.transform.position;
        }
        
        InitPos = transform.position;
    }
}
