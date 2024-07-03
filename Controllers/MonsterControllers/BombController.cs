using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class BombController : MonsterController
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
                    Anim.CrossFade("RUN", 0.01f);
                    break;
                case State.Attack:
                    Anim.CrossFade("ATTACK", 0.01f, -1, 0.0f);
                    break;
                case State.Skill:
                    Anim.CrossFade("SKILL", 0.01f, -1, 0.0f);
                    break;
                case State.Faint:
                    Anim.CrossFade("FAINT", 0.01f);
                    break;
                case State.GoingToExplode:
                    Anim.CrossFade("GOING_TO_EXPLODE", 0.01f);
                    break;
                case State.Explode:
                    Anim.CrossFade("EXPLODE", 0.01f);
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
        UnitId = UnitId.Bomb;
    }

    protected override void UpdateRush()
    {
        UpdateMoving();
    }
}
