using Google.Protobuf.Protocol;
using UnityEngine;

public class StarFallController : EffectController
{
    protected override void Init()
    {
        
    }

    protected override void Update()
    {
        // transform.position = parent.transform.position + Vector3.up * 0.5f;
        if (Time.time > LastSendTime + SendTick)
        {
            LastSendTime = Time.time;
            Vector3 pos = transform.position + Vector3.up * 0.5f;
            C_Move movePacket = new C_Move { ObjectId = Id, PosX = pos.x, PosY = pos.y, PosZ = pos.z};
            Managers.Network.Send(movePacket);
        }
    }
}
