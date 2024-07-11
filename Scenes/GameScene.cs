using UnityEngine;

public class GameScene : BaseScene
{
    protected override void Awake()
    {
        base.Init();
        SceneType = Define.Scene.Game;

        var mapNumber = Managers.Map.MapId;
        Managers.Map.LoadMap(mapNumber);
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
