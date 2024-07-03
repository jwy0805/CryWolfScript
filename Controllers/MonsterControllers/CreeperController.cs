using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

public class CreeperController : LurkerController
{
    private Vector3 divisionStartPos = new();
    private float duration = 0.8f;
    private float elapsedTime = 0f;
    
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
                case State.Divide:
                    Anim.CrossFade("RUSH", 0.01f);
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
        UnitId = UnitId.Creeper;
        AttackAnimValue = 5 / 6f;
        divisionStartPos = transform.position;
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
            case State.Divide:
                UpdateDivide();
                break;
            case State.Faint:
                break;
        }
    }
    
    protected override void UpdateRush()
    {
        UpdateMoving();
    }
    
    protected override void UpdateKnockBack()
    {
        if (PathQueue.Count <= 1) return;
        // Path 설정
        Vector3 cellPos = PathQueue.Peek();
        float step = TotalMoveSpeed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, cellPos, step);
        // Dir 설정
        double moveDir = DirQueue.Peek();
        Quaternion quaternion = Quaternion.Euler(0, (float)moveDir, step);
        transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, step);
        // Queue 정리
        if (Vector3.Distance(transform.position, cellPos) > 0.05f) return;
        DirQueue.Dequeue();
        PathQueue.Dequeue();
    }

    private void UpdateDivide()
    {
        if (DestPos == new Vector3(0, 0, 0)) return;
        elapsedTime += Time.deltaTime;
        if (elapsedTime <= duration)
        {
            float t = elapsedTime / duration;
            float height = CalculateHeight(t);
            Vector3 currentPos = Vector3.Lerp(divisionStartPos, DestPos, t);
            float deltaHeight = Mathf.Lerp(divisionStartPos.y, DestPos.y, t) + height;
            currentPos.y = Mathf.Clamp( deltaHeight, 6, deltaHeight);
            transform.position = currentPos;
        }
        else
        {
            transform.position = DestPos;
        }
    }

    private float CalculateHeight(float t)
    {   // 0에서 max로 올라갔다가 min으로 떨어지는 포물선
        float min = -0.75f;
        float max = 0.75f;
        if (t <= 0.25f)
        {   // 0에서 max로 상승
            return 2 * t * max;
        }
        else
        {   // max에서 min로 하강
            return (-min - 2 * (t - 0.5f)) * 1.25f + min;
        }
    }
}
