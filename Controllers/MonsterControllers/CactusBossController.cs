using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class CactusBossController : CactusController
{
    public override State State
    {
        get => PosInfo.State;
        set
        {
            PosInfo.State = value;
            if (TryGetComponent(out Animator anim) == false) return;
            Anim = anim;
            switch (PosInfo.State)
            {
                case State.Die:
                    Anim.CrossFade("DIE", 0.01f);
                    break;
                case State.Idle:
                    Anim.CrossFade("IDLE", 0.01f);
                    break;
                case State.Moving:
                    Anim.CrossFade("RUN", 0.01f);
                    break;
                case State.Rush:
                    Anim.CrossFade("RUSH", 0.01f);
                    break;
                case State.Attack:
                    Anim.CrossFade("ATTACK", 0.01f, -1, 0.0f);
                    break;
                case State.Attack2:
                    Anim.CrossFade("ATTACK2", 0.01f, -1, 0.0f);
                    break;
                case State.Attack3:
                    Anim.CrossFade("SMASH", 0.01f, -1, 0.0f);
                    break;
                case State.Skill:
                    Anim.CrossFade("BREATH", 0.01f, -1, 0.0f);
                    break;
                case State.Skill2:
                    Anim.CrossFade("BREATH", 0.01f, -1, 0.0f);
                    break;
                case State.KnockBack:
                    Anim.CrossFade("RUSH", 0.01f);
                    break;
                case State.Faint:
                    Anim.CrossFade("FAINT", 0.01f);
                    break;
                case State.Standby:
                default:
                    break;
            }
        }
    }
    
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.CactusBoss;
        AttackAnimValue = 5 / 6f;
    }

    protected override void UpdateRush()
    {
        base.UpdateMoving();
    }
}
