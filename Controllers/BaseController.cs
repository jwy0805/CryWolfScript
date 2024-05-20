using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public abstract class BaseController : MonoBehaviour
{ 
    private PositionInfo _positionInfo = new();
    private DestVector _destVec = new();

    protected double MoveDir;
    protected const float SendTick = 0.099f;
    protected const float Tolerance = 0.005f;
    protected SkillSubject SkillSubject;
    protected Animator Anim;
    protected readonly int AnimAttackSpeed = Animator.StringToHash("AttackSpeed");
    
    public int Id { get; set; }
    public SpawnWay Way { get; set; }
    public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
    public Vector3 DestPos { get; set; }
    public State NextState { get; set; }
    public bool SetKnockBackDest { get; set; }
    public Queue<Vector3> DestQueue { get; set; }
    public Queue<double> DirQueue { get; set; }
    public DestVector DestVec // Client에서 new 를 이용한 객체 생성 불가
    {
        get => _destVec;
        set
        {
            _destVec.X = value.X;
            _destVec.Y = value.Y;
            _destVec.Z = value.Z;
        }
    }
    public Vector3 CellPos
    {
        get => new(PosInfo.PosX, PosInfo.PosY, PosInfo.PosZ);
        set
        {
            if (Math.Abs(PosInfo.PosX - value.x) < 0.1 && 
                Math.Abs(PosInfo.PosY - value.y) < 0.1 &&
                Math.Abs(PosInfo.PosZ - value.z) < 0.1)
                return;

            Vector3 v = Util.NearestCell(new Vector3(value.x, value.y, value.z));
            PosInfo.PosX = v.x;
            PosInfo.PosY = v.y;
            PosInfo.PosZ = v.z;
        }
    }
    public PositionInfo PosInfo
    {
        get => _positionInfo;
        set
        {
            if (_positionInfo.Equals(value)) return;
            CellPos = new Vector3(value.PosX, value.PosY, value.PosZ);
            State = value.State;
            _positionInfo.Dir = value.Dir;
        }
    }
    public virtual State State
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
                case State.Standby:
                default:
                    break;
            }
        }
    }
    
    private void Start()
    {
        Init();
    }

    protected virtual void Update()
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
            case State.Faint:
                break;
        }
    }

    protected virtual void Init() { }
    protected virtual void UpdateDie() { }
    protected virtual void UpdateMoving() { }
    protected virtual void UpdateIdle() { }
    protected virtual void UpdateRush() { }

    protected virtual void UpdateAttack()
    {
        Quaternion quaternion = Quaternion.Euler(new Vector3(0, PosInfo.Dir, 0));
        transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, 20 * Time.deltaTime);
    }
    
    protected virtual void UpdateSkill()
    {
        Quaternion quaternion = Quaternion.Euler(new Vector3(0, PosInfo.Dir, 0));
        transform.rotation = Quaternion.Lerp(transform.rotation, quaternion, 20 * Time.deltaTime);
    }
    
    protected virtual void UpdateSkill2() { }
    protected virtual void UpdateKnockBack() { }
    protected virtual void UpdateFaint() { }

    public IEnumerator Despawn(GameObject go, float animPlayTime)
    {
        yield return new WaitForSeconds(animPlayTime);
        Managers.Game.Despawn(go);
    }

    public virtual void OnAnimSpeedUpdated(float param)
    {
        Anim.SetFloat(AnimAttackSpeed, param);
    }
}
