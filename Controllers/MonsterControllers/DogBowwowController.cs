using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class DogBowwowController : DogBarkController
{
    protected override void Init()
    {
        base.Init();
        UnitId = UnitId.DogBowwow;
        AttackAnimValue = 5 / 6f;
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
            case State.Attack:
            case State.Attack2:
                UpdateAttack();
                break;
            case State.Skill:
                UpdateSkill();
                break;
            case State.Skill2:
                UpdateSkill2();
                break;
            case State.Faint:
                break;
        }
    }
}
    
