using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Zenject;

public class LoginScene : BaseScene
{
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            SceneType = Define.Scene.Login;
            await Managers.UI.ShowSceneUI<UI_Login>();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    public override void Clear()
    {
        
    }
}
