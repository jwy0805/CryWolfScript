using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuffEffectController : EffectController
{
    private float _diffY = 0;
    
    protected override void Init()
    {
        StartTime = Time.time;
        if (TrailingParent && Parent != null) _diffY = transform.position.y - Parent.transform.position.y;
    }
    
    protected override void Update()
    {
        base.Update();
        
        if (Parent == null)
        {
            Managers.Object.Remove(Id);
            Managers.Resource.Destroy(gameObject);
            return;
        }

        if (Parent.TryGetComponent(out CreatureController cc))
        {
            if (cc.Stat.Hp <= 0)
            {
                Managers.Object.Remove(Id);
                Managers.Resource.Destroy(gameObject);
                return;
            }
        }

        if (TrailingParent) transform.position = Parent.transform.position + new Vector3(0, _diffY, 0);
    }
}
