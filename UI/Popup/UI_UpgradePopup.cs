using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

public class UI_UpgradePopup : UI_Popup
{
    private GameViewModel _gameVm;
    
    private string _skillName;
    private GameObject _currentSkillButton;
    private int _cost;

    public Define.PopupType PopupType { get; set; }
    
    private enum Buttons
    {
        AcceptButton,
        DenyButton,
    }

    private enum Texts
    {
        SkillInfoText,
        CostText,
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

    public void SetPopup(S_SetUpgradePopup packet)
    {
        GetText((int)Texts.SkillInfoText).gameObject.GetComponent<TextMeshProUGUI>().text = packet.SkillInfo.Explanation;
        GetText((int)Texts.CostText).gameObject.GetComponent<TextMeshProUGUI>().text = packet.SkillInfo.Cost.ToString();
    }

    private void OnAcceptClicked(PointerEventData data)
    {
        _gameVm.UpgradeSkill();
    }
    
    private void OnDenyClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}

