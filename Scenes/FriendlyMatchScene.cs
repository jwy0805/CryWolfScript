using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyMatchScene : BaseScene
{
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            SceneType = Define.Scene.FriendlyMatch;
            await Managers.UI.ShowSceneUI<UI_FriendlyMatch>();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    public override void Clear() { }
}
