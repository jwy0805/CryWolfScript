using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class MoleRatKingController : MoleRatController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.MoleRatKing;
    }

    protected override void Update()
    {
        switch (State)
        {
            case State.Die:
                UpdateDie();
                break;
            case State.Moving:
                UpdateMoving();
                break;
            case State.Idle:
                UpdateIdle();
                break;
            case State.Underground:
                UpdateUnderground();
                break;
            case State.Rush:
                UpdateRush();
                break;
            case State.Attack:
                UpdateAttack();
                break;
            case State.Skill:
                UpdateSkill();
                break;
            case State.Skill2:
                UpdateSkill2();
                break;
            case State.KnockBack:
                UpdateKnockBack();
                break;
            case State.Faint:
                break;
        }
    }

    private void UpdateUnderground()
    {
        UpdateMoving();
    }
}
