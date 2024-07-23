using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject go) where T : UnityEngine.Component
    {
        return Util.GetOrAddComponent<T>(go);
    }

    public static void BindEvent(this GameObject gameObject, Action<PointerEventData> action,
        Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.BindEvent(gameObject, action, type);
    }
    
    public static void BindEvent(this GameObject gameObject, Func<PointerEventData, Task> action,
        Define.UIEvent type = Define.UIEvent.Click)
    {
        UI_Base.BindEvent(gameObject, Action, type);
        return;
        async void Action(PointerEventData data) => await action(data);
    }
}
