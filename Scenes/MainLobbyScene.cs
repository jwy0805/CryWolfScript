using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLobbyScene : BaseScene
{
    protected override async void Init()
    {
        try
        {
            base.Init();

            SceneType = Define.Scene.MainLobby;
            await Managers.UI.ShowSceneUI<UI_MainLobby>();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    public override void Clear() { }
}
