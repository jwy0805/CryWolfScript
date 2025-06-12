using UnityEngine;

public class LoadingScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        
        SceneType = Define.Scene.Loading;
        Managers.UI.ShowSceneUI<UI_Loading>();
    }

    public override void Clear()
    {
        
    }
}
