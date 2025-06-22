using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_UpgradePopupNoCost : UI_Popup
{
    private GameViewModel _gameVm;
    
    private string _skillName;
    private GameObject _currentSkillButton;

    public Define.PopupType PopupType { get; set; }
    
    private enum Buttons
    {
        AcceptButton,
        DenyButton,
    }

    private enum Texts
    {
        SkillInfoText,
    }

    [Inject]
    public void Construct(GameViewModel gameViewModel)
    {
        _gameVm = gameViewModel;
    }
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        PopupType = Define.PopupType.UpgradePopup;
    }

    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.AcceptButton).gameObject.BindEvent(OnAcceptClicked);
        GetButton((int)Buttons.DenyButton).gameObject.BindEvent(OnDenyClicked);
    }

    public async void SetPopup(S_SetUpgradePopup packet)
    {
        try
        {
            var skillText = GetText((int)Texts.SkillInfoText);
            var skillId = packet.SkillInfo.Id;
            await Managers.Localization.BindLocalizedBaseSkillText(skillText, skillId);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private void OnAcceptClicked(PointerEventData data)
    {
        _gameVm.UpgradeBaseSkill();
        _gameVm.SetBaseSkillCostRequired();
    }
    
    private void OnDenyClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
