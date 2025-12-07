using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class WolfResourceController : ResourceController
{
    protected override void Init()
    {
        var wolfPlayer = Managers.Object.Find(obj =>
            obj != null &&                                      
            obj.TryGetComponent<PlayerController>(out var pc) && 
            pc.Faction == Faction.Wolf);

        if (wolfPlayer == null)
        {
            Debug.LogWarning("WolfResourceController: Wolf PlayerController 오브젝트를 찾지 못했습니다.");
            DestPos = transform.position;
        }
        else
        {
            DestPos = wolfPlayer.transform.position;
        }

        InitPos = transform.position;
    }
}
