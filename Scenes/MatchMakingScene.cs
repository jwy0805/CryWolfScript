using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchMakingScene : BaseScene
{
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            SceneType = Define.Scene.MatchMaking;
            await Managers.UI.ShowSceneUI<UI_MatchMaking>();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    public override void Clear() { }
}
