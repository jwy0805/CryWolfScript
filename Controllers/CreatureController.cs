using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class CreatureController : BaseController, ISkillObserver
{
    private readonly StatInfo _stat = new();
    protected float LastSendTime;
    
    public UnitId UnitId { get; protected set; }

    public virtual StatInfo Stat
    {
        get => _stat;
        set
        {
            if (_stat.Equals(value)) return;

            _stat.SizeX = value.SizeX;
            _stat.SizeY = value.SizeY;
            _stat.SizeZ = value.SizeZ;
            _stat.Hp = value.Hp;
            _stat.MaxHp = value.MaxHp;
            _stat.Level = value.Level;
            _stat.AttackSpeed = value.AttackSpeed;
            _stat.MoveSpeed = value.MoveSpeed;
        }
    }

    public int MaxHp 
    {
        get => Stat.MaxHp;
        set => Stat.MaxHp = value;
    }

    public int Hp
    {
        get => Stat.Hp;
        set => Stat.Hp = value;
    }

    public int Mp
    {
        get => Stat.Mp;
        set => Stat.Mp = value;
    }

    public int Level => Stat.Level;
    public float TotalMoveSpeed { get; set; }

    protected override void Init()
    {
        base.Init();
        SkillSubject = GameObject.Find("Subject").GetComponent<SkillSubject>();
        SkillSubject.AddObserver(this);
    }

    protected override void UpdateIdle()
    {
        PathQueue?.Clear();
        DirQueue?.Clear();
    }

    public virtual void OnPathReceived(S_SetPath packet)
    {
        var pathQueue = new Queue<Vector3>();
        var dirQueue = new Queue<double>();
        foreach (var v in packet.Path) pathQueue.Enqueue(new Vector3(v.X, v.Y, v.Z));
        foreach (var v in packet.Dir) dirQueue.Enqueue(v);
        TotalMoveSpeed = packet.MoveSpeed;
        PathQueue = pathQueue;
        DirQueue = dirQueue;
    }

    public virtual void OnDead()
    {
        StartCoroutine(base.Despawn(gameObject, 2f));
    }
    
    public virtual void OnSkillUpdated(int id, GameObjectType type, SkillType skillType, int step) { }
    
    // Animation Event
    protected virtual void OnHitEvent()
    {
        // Managers.Network.Send(new C_Attack { ObjectId = Id, AttackMethod = AttackMethod.NormalAttack });
    }
    
    protected virtual void OnSkillEvent() { }

    protected virtual void OnMotionEvent()
    {
        Managers.Network.Send(new C_Motion { ObjectId = Id, State = State });
    }

    protected virtual void OnEndEvent()
    {
        State = NextState;
    }
}
