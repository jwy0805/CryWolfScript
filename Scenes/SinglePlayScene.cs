using UnityEngine;

public class SinglePlayScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.SinglePlay;
        Managers.UI.ShowSceneUI<UI_SinglePlay>();
    }

    public override void Clear()
    {
        
    }
}
