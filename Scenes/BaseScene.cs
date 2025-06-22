using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Object = System.Object;

public abstract class BaseScene : MonoBehaviour
{
    public static Define.Scene SceneType { get; protected set; } = Define.Scene.Unknown;

    protected virtual void Awake()
    {
        Init();
    }

    protected virtual async void Init()
    {
        try
        {
            var obj = FindAnyObjectByType(typeof(EventSystem));
            if (obj == null)
            {
                var eventSystem = await Managers.Resource.Instantiate("EventSystem");
                eventSystem.name = "@EventSystem";
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public abstract void Clear();
}
