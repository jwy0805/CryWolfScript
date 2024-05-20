using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class BurrowController : MonsterController
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
                case State.Skill:
                    Anim.CrossFade("SKILL", 0.01f, -1, 0.0f);
                    break;
                case State.Skill2:
                    Anim.CrossFade("SKILL2", 0.01f, -1, 0.0f);
                    break;
                case State.KnockBack:
                    Anim.CrossFade("RUSH", 0.01f);
                    break;
                case State.Faint:
                    Anim.CrossFade("FAINT", 0.01f);
                    break;
                case State.IdleToRush:
                    Anim.CrossFade("IDLE_TO_RUSH", 0.01f);
                    break;
                case State.RushToIdle:
                    Anim.CrossFade("RUSH_TO_IDLE", 0.01f);
                    break;
                case State.IdleToUnderground:
                    Anim.CrossFade("IDLE_TO_UNDERGROUND", 0.01f);
                    break;
                case State.UndergroundToIdle:
                    Anim.CrossFade("UNDERGROUND_TO_IDLE", 0.01f);
                    break;
                case State.Underground:
                    Anim.CrossFade("UNDERGROUND", 0.01f);
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
        UnitId = UnitId.Burrow;
    }

    protected override void UpdateRush()
    {
        base.UpdateMoving();
    }
}
