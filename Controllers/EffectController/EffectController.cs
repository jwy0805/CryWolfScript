using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

public class EffectController : MonoBehaviour
{
    protected float StartTime;
    protected float DiffY;
    
    public int Id { get; set; }
    [CanBeNull] public GameObject Master { get; set; }
    public bool TrailingMaster { get; set; }
    public float Duration { get; set; } = 2.0f;
    
    protected virtual void Start()
    {
        Init();
    }

    protected virtual void Init()
    {
        StartTime = Time.time;
        if (TrailingMaster && Master != null) DiffY = transform.position.y - Master.transform.position.y;
    }
    
    protected virtual void Update()
    {
        if (Time.time < StartTime + Duration) return;
        Managers.Object.Remove(Id);
        Managers.Resource.Destroy(gameObject);

        if (TrailingMaster && Master != null)
        {
            if (Master.TryGetComponent(out CreatureController cc))
            {
                if (cc.Stat.Hp > 0) return;
                Managers.Object.Remove(Id);
                Managers.Resource.Destroy(gameObject);
            }
            
            transform.position = Master.transform.position + new Vector3(0, DiffY, 0);
        }
    }
}
