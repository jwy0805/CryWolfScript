using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class SkillSubject : MonoBehaviour, ISkillSubject
{
    public List<ISkillObserver> Observers = new();
    private int _id;
    private float _param;
    private GameObjectType _type;
    private SkillType _skillType;
    private int _step;
    
    public void AddObserver(ISkillObserver observer)
    {
        Observers.Add(observer);
    }

    public void RemoveObserver(ISkillObserver observer)
    {
        if (Observers.IndexOf(observer) > 0) Observers.Remove(observer);
    }

    public void Notify()
    {
        foreach (var observer in Observers) observer.OnSkillUpdated(_id, _type, _skillType, _step);
    }
    
    public void SkillUpdated(int id, GameObjectType type, SkillType skillType, int step = 0)
    {
        _id = id;
        _type = type;
        _skillType = skillType;
        _step = step;
        Notify();
    }
}
