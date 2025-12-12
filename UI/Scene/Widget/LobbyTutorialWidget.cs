using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class LobbyTutorialWidget
{
    private readonly TutorialViewModel _tutorialVm;
    private readonly Camera _tutorialCamera1;
    private readonly Camera _tutorialCamera2;

    public LobbyTutorialWidget(TutorialViewModel tutorialVm, GameObject[] tutorialCameras)
    {
        _tutorialVm = tutorialVm;
        _tutorialCamera1 = tutorialCameras.First(go => go.name == "TutorialCamera1").GetComponent<Camera>();
        _tutorialCamera2 = tutorialCameras.First(go => go.name == "TutorialCamera2").GetComponent<Camera>();
        
        BindEvents();
    }

    private void BindEvents()
    {
        _tutorialVm.OnInitTutorialCamera1 += InitTutorialMainCamera1;
        _tutorialVm.OnInitTutorialCamera2 += InitTutorialMainCamera2;
    }

    public async Task ProcessTutorial(UserTutorialInfo tutorialInfo)
    {
        // Case 1: Both tutorials are completed
        if (tutorialInfo.WolfTutorialDone && tutorialInfo.SheepTutorialDone)
        {
            if (tutorialInfo.ChangeFactionTutorialDone) return;
            await Managers.UI.ShowPopupUI<UI_ChangeFactionPopup>();
            return;
        }

        // Case 2: One of the tutorials is completed, Succeed in the other tutorial
        if (tutorialInfo.WolfTutorialDone)
        {
            _tutorialVm.CompleteTutorialWolf();
        }
        else if (tutorialInfo.SheepTutorialDone)
        {
            _tutorialVm.CompleteTutorialSheep();
        }
        // Case 3: Both tutorials are not completed -> First time to play
        else
        {
            await Managers.UI.ShowPopupUI<UI_TutorialMainPopup>();
        }
    }
    
    private void InitTutorialMainCamera1(Vector3 npcPos, Vector3 cameraPos)
    {
        _tutorialCamera1.transform.position = cameraPos;
        _tutorialCamera1.transform.LookAt(npcPos);
    }
    
    private void InitTutorialMainCamera2(Vector3 npcPos, Vector3 cameraPos)
    {
        _tutorialCamera2.transform.position = cameraPos;
        _tutorialCamera2.transform.LookAt(npcPos);
    }
    
    public void Dispose()
    {
        _tutorialVm.OnInitTutorialCamera1 -= InitTutorialMainCamera1;
        _tutorialVm.OnInitTutorialCamera2 -= InitTutorialMainCamera2;
    }
}