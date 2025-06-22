using System;
using System.Threading.Tasks;
using UnityEngine;

public class LoadingScene : BaseScene
{
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            SceneType = Define.Scene.Loading;
            await Managers.UI.ShowSceneUI<UI_Loading>();
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
