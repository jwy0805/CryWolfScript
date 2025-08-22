using System;
using System.Collections.Concurrent;
using UnityEngine;

public class MainThreadDispatcher
{
    private readonly ConcurrentQueue<Action> _queue = new();
    public void Enqueue(Action action) => _queue.Enqueue(action);

    public void Update()
    {
        while (_queue.TryDequeue(out var action))
        {
            action?.Invoke();
        }
    }
}
