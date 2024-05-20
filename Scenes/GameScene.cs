using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        SceneType = Define.Scene.Game;
        
        Managers.Map.LoadMap();
        Managers.UI.ShowSceneUI<UI_Game>();
        InitObjects();
        Managers.Network.ConnectGameSession();
    }
    
    public override void Clear()
    {
        
    }

    private void InitObjects()
    {
        var subject = new GameObject { name = "Subject", tag = "SkillSubject" };
        subject.GetOrAddComponent<SkillSubject>();
    }
}
