using System;
using System.Collections.Generic;

public class EventManager
{
    private readonly Dictionary<string, Action<object>> _eventDictionary = new();

    public void StartListening(string eventName, Action<object> listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent += listener;
            _eventDictionary[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            _eventDictionary.Add(eventName, thisEvent);
        }
    }

    public void StopListening(string eventName, Action<object> listener)
    {
        if (_eventDictionary.TryGetValue(eventName, out var thisEvent) == false) return;
        
        thisEvent -= listener;
        if (thisEvent == null)
        {
            _eventDictionary.Remove(eventName);
        }
        else
        {
            _eventDictionary[eventName] = thisEvent;
        }
    }

    public void TriggerEvent(string eventName, object eventData = null)
    {
        if (_eventDictionary.TryGetValue(eventName, out var thisEvent))
        {
            thisEvent.Invoke(eventData);
        }
    }
}