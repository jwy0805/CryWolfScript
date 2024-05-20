using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;

public class PoisonBeltController : EffectController
{
    protected override void Init()
    {
        StartTime = Time.time;
        Duration = 2f;
        DamageTime = 1f;
        Transform t = transform;
        t.position = parent.transform.position + Vector3.up * 0.5f;
        t.localScale *= 1.2f;
    }
    
    protected override void Update()
    {
        transform.position = parent.transform.position + Vector3.up * 0.5f;
        if (Time.time > LastSendTime + SendTick)
        {
            LastSendTime = Time.time;
            Vector3 pos = transform.position + Vector3.up * 0.5f;
            C_Move movePacket = new C_Move { ObjectId = Id, PosX = pos.x, PosY = pos.y, PosZ = pos.z};
            Managers.Network.Send(movePacket);
        }
        
        base.Update();
    }
}
