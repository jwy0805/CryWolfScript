using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class EffectController : MonoBehaviour
{
    protected float LastSendTime = 0;
    protected readonly float SendTick = 0.099f;
    protected float StartTime;
    
    public int Id { get; set; }
    public GameObject Parent { get; set; }
    public bool TrailingParent { get; set; }
    public float Duration { get; set; } = 2.0f;
    
    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Update()
    {
        if (Time.time < StartTime + Duration) return;
        Managers.Object.Remove(Id);
        Managers.Resource.Destroy(gameObject);
    }

    protected virtual void Init()
    {
        StartTime = Time.time;
    }
}
