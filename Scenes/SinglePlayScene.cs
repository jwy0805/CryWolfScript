using System;
using UnityEngine;

public class SinglePlayScene : BaseScene
{
    protected override async void Init()
    {
        try
        {
            base.Init();

            SceneType = Define.Scene.SinglePlay;
            await Managers.UI.ShowSceneUI<UI_SinglePlay>();
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
