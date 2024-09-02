using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public interface ISubResourceWindow
{
    
}

public class SubResourceWindow : UI_Popup, ISubResourceWindow
{
    private enum Images
    {
        SubResourceWindowPanel,
    }
    
    private GameViewModel _gameVm;
    
    [Inject]
    public void Construct(GameViewModel gameViewModel)
    {
        _gameVm = gameViewModel;
    }
    
    protected override void Init()
    {
        base.Init();
        _gameVm.SubResourceWindow = this;
        
        BindObjects();
        InitUI();
        SetBaseSkillPanel();
    }

    private void SetBaseSkillPanel()
    {
        // Load the base skill panel according to the camp
        var camp = Util.Camp;
        var path = $"UI/InGame/SkillPanel/{camp.ToString()}BaseSkillPanel";
        var panel = Managers.Resource.Instantiate(path);
        panel.transform.SetParent(GetImage((int)Images.SubResourceWindowPanel).transform);
        
        // Set the base skill buttons
        var skillButtons = panel.GetComponentsInChildren<Button>();
        foreach (var skillButton in skillButtons)
        {
            skillButton.gameObject.BindEvent(OnSkillButtonClicked);
        }
    }

    private void OnSkillButtonClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI<UI_UpgradePopup>();
        if (_gameVm.CurrentSelectedSkillButton != null)
        {
            var oldImage = GetImageFromButton(_gameVm.CurrentSelectedSkillButton);
            oldImage.color = Color.green;
        }

        var selectedSkillButton = data.pointerPress.GetComponent<ISkillButton>();
        if (selectedSkillButton == null) return;
        
        _gameVm.CurrentSelectedSkillButton = selectedSkillButton;
        var newImage = GetImageFromButton(_gameVm.CurrentSelectedSkillButton);
        if (newImage != null)
        {
            newImage.color = Color.blue;
        }
        
        Managers.UI.ShowPopupUiInGame<UI_UpgradePopup>();
        var skillName = _gameVm.CurrentSelectedSkillButton.Name.Replace("Button", "");
        if (Enum.TryParse(skillName, out Skill skill))
        {
            Managers.Network.Send(new C_SetUpgradePopup { SkillId = (int)skill });
        }
    }
    
    private Image GetImageFromButton(ISkillButton button)
    {
        if (button is not MonoBehaviour mono) return null;
        var go = mono.gameObject;
        return go.transform.parent.parent.GetChild(1).GetComponent<Image>();
    }
    
    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
    }
}
