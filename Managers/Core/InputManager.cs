using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputManager
{
    public Action<Define.MouseEvent> MouseAction;
    
    private bool _pressed;
    private float _pressedTime;

    public void OnUpdate()
    {
        if (EventSystem.current.IsPointerOverGameObject(-1) || 
            EventSystem.current.IsPointerOverGameObject(0) || MouseAction == null)
            return;
        
        if (Input.GetMouseButton(0))
        {
            if (!_pressed)
            {
                MouseAction.Invoke(Define.MouseEvent.PointerDown);
                _pressedTime = Time.deltaTime;
            }
            MouseAction.Invoke(Define.MouseEvent.Press);
            _pressed = true;
        }
        else
        {
            if (_pressed)
            {
                if (Time.time < _pressedTime + 0.2f)
                    MouseAction.Invoke(Define.MouseEvent.Click);
                MouseAction.Invoke(Define.MouseEvent.PointerUp);
            }

            _pressed = false;
            _pressedTime = 0.0f;
        }
    }

    public void Clear()
    {
        MouseAction = null;
    }
}
