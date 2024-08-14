using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultScene : BaseScene
{
    private UI_Result _sceneUI;
    
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.Result;
        Managers.UI.ShowSceneUI<UI_Result>();
    }

    public override void Clear()
    {
        
    }
}
