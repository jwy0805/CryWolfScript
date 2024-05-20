using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class EffectController : MonoBehaviour
{
    public GameObject parent;
    protected float LastSendTime = 0;
    protected readonly float SendTick = 0.099f;
    protected bool IsHit = false;
    protected float StartTime;
    protected float DamageTime;
    protected float Duration;
    
    public int Id { get; set; }
    
    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Update()
    {
        if (Time.time > StartTime + DamageTime && IsHit == false)
        {
            SendSkillPacket();
            IsHit = true;
        }
        
        if (Time.time > StartTime + Duration)
        {
            Managers.Resource.Destroy(gameObject);
            Managers.Object.Remove(Id);
        }
    }

    protected virtual void Init()
    {
        
    }

    protected virtual void SendSkillPacket()
    {
        Managers.Network.Send(new C_EffectActivate { ObjectId = Id });
    }

}
