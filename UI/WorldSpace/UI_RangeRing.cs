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
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _cc = gameObject.GetComponentInParent<CreatureController>();
        if (_cc == null) return;
        
        if (AboutAttack)
        {
            _spriteRenderer.color = Color.cyan;
            transform.localScale = 
                new Vector3(_cc.AttackRange * 0.75f, _cc.AttackRange * 0.75f, _cc.AttackRange * 0.75f);
        }
        
        if (AboutSkill)
        {
            _spriteRenderer.color = Color.magenta;
            transform.localScale = _cc.SkillRange == 0 
                ? new Vector3(0, 0, 0) 
                : new Vector3(_cc.SkillRange * 0.75f, _cc.SkillRange * 0.75f, _cc.SkillRange * 0.75f);
        }
    }

    public void SetScale(float range)
    {
        if (_spriteRenderer == null) _spriteRenderer = GetComponent<SpriteRenderer>();

        if (AboutAttack)
        {
            _spriteRenderer.color = Color.cyan;
            transform.localScale = 
                new Vector3(range * 0.75f, range * 0.75f, range * 0.75f);
        }

        if (AboutSkill)
        {
            _spriteRenderer.color = Color.magenta;
            transform.localScale =
                new Vector3(range * 0.75f, range * 0.75f, range * 0.75f);
        }
    }
}
