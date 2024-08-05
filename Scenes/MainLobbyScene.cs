using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainLobbyScene : BaseScene
{
    protected override void Init()
    {
        base.Init();

        SceneType = Define.Scene.MainLobby;
        Managers.UI.ShowSceneUI<UI_MainLobby>();
        // TODO: web packet을 통해 보유한 컬렉션 목록 DB 긁어오기
        // 테스트 단계에서는 모두 보유한 것으로 생성
    }
    
    public override void Clear() { }
}
