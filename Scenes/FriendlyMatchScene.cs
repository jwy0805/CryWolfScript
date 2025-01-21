using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FriendlyMatchScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        
        SceneType = Define.Scene.FriendlyMatch;
        Managers.UI.ShowSceneUI<UI_FriendlyMatch>();
    }
    
    public override void Clear() { }
}
