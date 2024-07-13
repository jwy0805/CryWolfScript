using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MatchMakingScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        
        SceneType = Define.Scene.MatchMaking;
        Managers.UI.ShowSceneUI<UI_MatchMaking>();
    }
    
    public override void Clear() { }
}
