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

public class UI_UpgradePopup : UI_Popup
{
    private string _skillName;
    // private SkillSubject _skillSubject;
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
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        SetButtonEvents();
        PopupType = Define.PopupType.UpgradePopup;
    }

    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void SetButtonEvents()
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
        var ui = GameObject.FindWithTag("UI").GetComponent<UI_Mediator>();
        var currentSkillButton = ui.CurrentSelectedSkill;
        _skillName = currentSkillButton.name.Replace("Button", "");
        var skill = (Skill)Enum.Parse(typeof(Skill), _skillName);
        Managers.Network.Send(new C_SkillUpgrade { Skill = skill });
        Managers.UI.ClosePopupUI();
    }
    
    private void OnDenyClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}

