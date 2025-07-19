using System;
using Google.Protobuf.Protocol;
using UnityEngine;

public class GameScene : BaseScene
{
    protected override async void Awake()
    {
        try
        {
            base.Init();
            QualitySettings.shadowDistance  = 80f;
            QualitySettings.shadowCascades  = 2;
            SceneType = Define.Scene.Game;

            var mapNumber = Managers.Map.MapId;
            await Managers.Map.LoadMap(mapNumber);
            InitObjects();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    public override void Clear()
    {
        
    }

    private void InitObjects()
    {
        var subject = new GameObject { name = "Subject", tag = "SkillSubject" };
        subject.GetOrAddComponent<SkillSubject>();
        
        Managers.Network.Send(new C_StartGameScene());
    }
}
