using UnityEngine;
using Zenject;

public class UI_TutorialBattleWolfPopup : UI_Popup
{
    private TutorialViewModel _tutorialVm;

    private enum Images
    {
        
    }

    private enum Buttons
    {
        
    }

    private enum Texts
    {
        
    }
    
    [Inject]
    public void Construct(TutorialViewModel tutorialViewModel)
    {
        _tutorialVm = tutorialViewModel;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        BindActions();
        InitButtonEvents();
        InitUI();
    }
    
    protected override void BindObjects()
    {
        
    }
    
    private void BindActions()
    {
        
    }
    
    protected override void InitButtonEvents()
    {
        
    }

    protected override void InitUI()
    {
        
    }
}
