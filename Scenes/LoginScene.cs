using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Zenject;

public class LoginScene : BaseScene
{

    protected override void Init()
    {
        base.Init();
        
        SceneType = Define.Scene.Login;
        Managers.UI.ShowSceneUI<UI_Login>();
    }
    
    public override void Clear()
    {
        
    }
}
