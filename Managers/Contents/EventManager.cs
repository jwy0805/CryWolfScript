using System;
using System.Collections.Generic;

public enum GameEventKey
{
    // GameViewModel
    UpdateUnitDeleteCost,
    UpdateUnitRepairCost,
    UpdateBaseSkillCost,
    UpdateLinePos,
    SendWarning,
    
    // TutorialViewModel
    RunTutorialTag,
    ReceiveTutorialReward,
}

public class EventManager
{
    private readonly Dictionary<GameEventKey, Action<object>> _eventDictionary = new();

    public void StartListening(GameEventKey key, Action<object> listener)
    {
        if (_eventDictionary.TryGetValue(key, out var thisEvent))
        {
            thisEvent += listener;
            _eventDictionary[key] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            _eventDictionary.Add(key, thisEvent);
        }
    }
    
    public void StopListening(GameEventKey key, Action<object> listener)
    {
        if (_eventDictionary.TryGetValue(key, out var thisEvent) == false) return;
        
        thisEvent -= listener;
        if (thisEvent == null)
        {
            _eventDictionary.Remove(key);
        }
        else
        {
            _eventDictionary[key] = thisEvent;
        }
    }

    public void TriggerEvent(GameEventKey key, object eventData = null)
    {
        if (_eventDictionary.TryGetValue(key, out var thisEvent))
        {
            thisEvent.Invoke(eventData);
        }
    }
}