using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEffectController : EffectController
{
    protected override void Update()
    {
        if (parent == null)
        {
            Managers.Resource.Destroy(gameObject);
            return;
        }
        transform.position = parent.transform.position + Vector3.up * 0.5f;
    }
}
