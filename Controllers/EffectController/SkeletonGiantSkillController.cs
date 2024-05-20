using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SkeletonGiantSkillController : EffectController
{
    protected override void Init()
    {
        StartTime = Time.time;
        Duration = 1.0f;
        transform.position += Vector3.up * 0.1f;
    }
    
    protected override void Update()
    {
        if (Time.time > StartTime + Duration == false) return;
        Managers.Resource.Destroy(gameObject);
        Managers.Object.Remove(Id);
    }
}
