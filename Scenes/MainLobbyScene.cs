using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.MainLobby;
        Managers.UI.ShowSceneUI<UI_MainLobby>();
    }
    
    public override void Clear() { }
}
