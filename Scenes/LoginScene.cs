using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoginScene : BaseScene
{
    private UI_Login _sceneUI;
    
    private float _verticalSpeed = 500.0f;
    private float _destPos;
    private RectTransform _rtBoard;

    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Login;
        _sceneUI = Managers.UI.ShowSceneUI<UI_Login>();
        
        _rtBoard = GameObject.Find("LoginBoard").GetComponent<RectTransform>();
        _destPos = _rtBoard.anchoredPosition.y;
        _rtBoard.anchoredPosition = Vector2.up * _rtBoard.rect.height;
    }

    void Update()
    {
        if (_rtBoard.anchoredPosition.y > _destPos)
        {
            _rtBoard.anchoredPosition += Vector2.down * (_verticalSpeed * Time.deltaTime);
        }
    }

    public override void Clear()
    {
        
    }
}
