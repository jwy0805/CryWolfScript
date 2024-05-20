using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TouchToStartScene : BaseScene
{
    private readonly float _verticalSpeed = 500.0f;
    private float _destPos;
    private bool _move = false;
    private RectTransform _rtTitle;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Start;
        
        _rtTitle = GameObject.Find("CryWolf").GetComponent<RectTransform>();
        _destPos = _rtTitle.anchoredPosition.y - _rtTitle.rect.height;
    }

    private void Update()
    {
        if (Input.GetMouseButtonUp(0))
        {
            _move = true;
        }

        if (_move)
        {
            _rtTitle.anchoredPosition += Vector2.down * (_verticalSpeed * Time.deltaTime);
        }
        
        if (_rtTitle.anchoredPosition.y <= _destPos)
        {
            _move = false;
            SceneManager.LoadScene("Scenes/Login");
        }
        
    }

    public override void Clear()
    {
        
    }
}
