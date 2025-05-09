using System;
using System.Linq;
using UnityEngine;

public static class DebugHelper
{
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    // static void ListAssemblies()
    // {
    //     foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
    //     {
    //         Debug.Log($"[AOT] Loaded Assembly: {asm.GetName().Name}");
    //     }
    // }
    //
    // [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    // static void ListProtocolTypes()
    // {
    //     var protocols = AppDomain.CurrentDomain
    //         .GetAssemblies()
    //         .SelectMany(a => a.GetTypes())
    //         .Where(t => typeof(Microsoft.AspNetCore.SignalR.Protocol.IHubProtocol).IsAssignableFrom(t))
    //         .ToList();
    //     Debug.Log($"[AOT] IHubProtocol impl count: {protocols.Count}");
    //     foreach (var t in protocols)
    //         Debug.Log($"[AOT] Protocol Type: {t.FullName}");
    // }
}
