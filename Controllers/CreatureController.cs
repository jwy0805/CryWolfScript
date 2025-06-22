using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.Serialization;

public class CreatureController : BaseController, ISkillObserver
{
    private readonly StatInfo _stat = new();
    
    public UnitId UnitId { get; set; }

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
            _stat.Mp = value.Mp;
            _stat.MaxMp = value.MaxMp;
            _stat.Level = value.Level;
            _stat.MoveSpeed = value.MoveSpeed;
            _stat.AttackRange = value.AttackRange;
            _stat.SkillRange = value.SkillRange;
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

    public int ShieldAdd { get; set; }
    
    public int ShieldRemain { get; set; }
    
    public int MaxMp
    {
        get => Stat.MaxMp;
        set => Stat.MaxMp = value;
    }
    
    public int Mp
    {
        get => Stat.Mp;
        set => Stat.Mp = value;
    }

    public float AttackRange
    {
        get => Stat.AttackRange;
        set => Stat.AttackRange = value;
    }
    
    public float SkillRange
    {
        get => Stat.SkillRange;
        set => Stat.SkillRange = value;
    }
    
    public int Level => Stat.Level;
    public float TotalMoveSpeed { get; set; }

    protected override async void Init()
    {
        try
        {
            base.Init();
            SkillSubject = GameObject.Find("Subject").GetComponent<SkillSubject>();
            SkillSubject.AddObserver(this);
            // Instantiate Health Circle
            await Managers.Resource.Instantiate("WorldObjects/HealthCircle", transform);
            Util.GetOrAddComponent<UI_HealthCircle>(gameObject);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override void UpdateIdle()
    {
        PathQueue?.Clear();
        DirQueue?.Clear();
    }

    public virtual void OnPathReceived(S_SetPath pathPacket)
    {
        var pathQueue = new Queue<Vector3>();
        var dirQueue = new Queue<double>();
        foreach (var v in pathPacket.Path) pathQueue.Enqueue(new Vector3(v.X, v.Y, v.Z));
        foreach (var v in pathPacket.Dir) dirQueue.Enqueue(v);
        TotalMoveSpeed = pathPacket.MoveSpeed;
        PathQueue = pathQueue;
        DirQueue = dirQueue;
    }

    public virtual void OnDead(float time = 2f)
    {
        StartCoroutine(Despawn(gameObject, time));
    }
    
    public virtual void OnSkillUpdated(int id, GameObjectType type, SkillType skillType, int step) { }
    
    // Animation Event
    protected virtual void OnHitEvent() { }
    protected virtual void OnSkillEvent() { }
    protected virtual void OnEndEvent() { }
}
