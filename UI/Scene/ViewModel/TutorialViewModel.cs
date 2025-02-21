using System;
using System.Collections.Generic;
using UnityEngine;

public class TutorialViewModel
{
    public readonly Dictionary<string, Action> MainEventDict = new();
    
    public event Action<Vector3, Vector3> OnInitTutorialCamera1;
    public event Action<Vector3, Vector3> OnInitTutorialCamera2;
    public event Action OnShowSpeaker;
    public event Action OnShowNewSpeaker;
    public event Action OnChangeSpeaker;
    public event Action OnShowFactionSelectPopup;
    public event Action OnChangeFaceCry;
    public event Action OnChangeFaceHappy;
    public event Action OnChangeFaceNormal;
    
    public void InitTutorialMain(Vector3 npc1Position, Vector3 camera1Position, Vector3 npc2Position, Vector3 camera2Position)
    {
        OnInitTutorialCamera1?.Invoke(npc1Position, camera1Position);
        OnInitTutorialCamera2?.Invoke(npc2Position, camera2Position);
        
        MainEventDict.Add("ShowSpeaker", OnShowSpeaker);
        MainEventDict.Add("ShowNewSpeaker", OnShowNewSpeaker);
        MainEventDict.Add("ChangeSpeaker", OnChangeSpeaker);
        MainEventDict.Add("ShowFactionSelectPopup", OnShowFactionSelectPopup);
        MainEventDict.Add("ChangeFaceCry", OnChangeFaceCry);
        MainEventDict.Add("ChangeFaceHappy", OnChangeFaceHappy);
        MainEventDict.Add("ChangeFaceNormal", OnChangeFaceNormal);
    }
}
