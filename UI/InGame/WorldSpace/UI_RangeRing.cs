using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_RangeRing : MonoBehaviour
{
    private CreatureController _cc;
    private SpriteRenderer _spriteRenderer;
    
    public bool AboutAttack { get; set; }
    public bool AboutSkill { get; set; }
    
    private void Start()
    {
        // _spriteRenderer = GetComponent<SpriteRenderer>();
        // _cc = gameObject.GetComponentInParent<CreatureController>();
        // if (_cc == null) return;
        //
        // if (AboutAttack)
        // {
        //     _spriteRenderer.color = Color.cyan;
        //     transform.localScale = new Vector3(_cc.AttackRange, _cc.AttackRange, _cc.AttackRange);
        //     Debug.Log(_cc.AttackRange);
        // }
        //
        // if (AboutSkill)
        // {
        //     _spriteRenderer.color = Color.magenta;
        //     transform.localScale = _cc.SkillRange == 0 
        //         ? new Vector3(0, 0, 0) 
        //         : new Vector3(_cc.SkillRange, _cc.SkillRange, _cc.SkillRange);
        // }
    }

    public void SetScale(float range)
    {
        var scaledRange = range * 2;
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

        if (AboutAttack)
        {
            _spriteRenderer.color = Color.cyan;
            transform.localScale = new Vector3(scaledRange, scaledRange, scaledRange);
        }

        if (AboutSkill)
        {
            _spriteRenderer.color = Color.magenta;
            transform.localScale = new Vector3(scaledRange, scaledRange, scaledRange);

        }
    }
}
