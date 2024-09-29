using System;
using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Button = UnityEngine.UI.Button;

public interface ISkillWindow
{
    void InitUI(IPortrait portrait);
    void InitUpgradeButton();
    void UpdateSkillButton();
    void UpdateUpgradeCost(int cost);
}

public class SkillWindow : UI_Popup, ISkillWindow
{
    private enum Buttons
    {
        UpgradeButton,
    }
    
    private enum Images
    {
        SkillPanel,
    }

    private enum Texts
    {
        CurrentName,
        CurrentPercent,
        GoldText,
    }
    
    private GameViewModel _gameVm;
    
    private GameObject _skillPanel;
    private Button[] _skillButtons;
    
    [Inject]
    public void Construct(GameViewModel gameViewModel)
    {
        _gameVm = gameViewModel;
    }

    protected override void Init()
    {
        base.Init();
        _gameVm.SkillWindow = this;
        
        BindObjects();
        InitButtonEvents();
        InitUpgradeButton();
        InitUI(_gameVm.CurrentSelectedPortrait);
    }

    public void InitUI(IPortrait portrait)
    {
        // Destroy the previous skill panel
        if (_skillPanel != null)
        {
            Destroy(_skillPanel);
        }
        
        var rect = GetComponent<RectTransform>();
        transform.SetParent(transform);
        rect.anchoredPosition = new Vector2(0, 0);
        rect.sizeDelta = new Vector2(0, 0);
        
        var unitId = portrait.UnitId;
        _skillPanel = Managers.Resource.Instantiate($"UI/InGame/SkillPanel/{unitId.ToString()}SkillPanel");
        _skillPanel.transform.SetParent(Util.FindChild(gameObject, "SkillPanel", true).transform);
        var panelRect = _skillPanel.GetComponent<RectTransform>();
        panelRect.anchoredPosition = new Vector2(0, 0);
        panelRect.sizeDelta = new Vector2(0, 0);
        
        // Set Skill Buttons on the skill panel
        _skillButtons = _skillPanel.GetComponentsInChildren<Button>();
        foreach (var skillButton in _skillButtons)
        {
            skillButton.gameObject.BindEvent(OnSkillButtonClicked);
            SetObjectSize(skillButton.gameObject, 0.22f);
            Util.SetAlpha(skillButton.gameObject.GetComponent<Image>(), 0.6f);
            UpdateSkillButton();
        }
        
        // Set Current Unit Name
        GetText((int)Texts.CurrentName).text = unitId.ToString();
    }
    
    // Update skill button when a skill is upgraded
    public void UpdateSkillButton()
    {
        foreach (var skillButton in _skillButtons)
        {
            var skillName = skillButton.name.Replace("Button", "");
            if (_gameVm.SkillsUpgraded.Contains((Skill)Enum.Parse(typeof(Skill), skillName)))
            {
                Util.SetAlpha(skillButton.gameObject.GetComponent<Image>(), 1f);
            }
        }
        
        _gameVm.UpdateUpgradeCostRequired();
    }

    // Update the cost(gold text) in the upgrade button
    public void UpdateUpgradeCost(int cost)
    {
        GetText((int)Texts.GoldText).text = cost.ToString();
    }

    #region Button Events

    private void OnUpgradeClicked(PointerEventData data)
    {
        _gameVm.OnUpgradeButtonClicked();
    }

    private void OnSkillButtonClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI<UI_UpgradePopup>();
        foreach (var skillButton in _skillButtons)
        {
            Util.GetFrameFromButton(skillButton.GetComponent<UI_Skill>()).color = Color.green;
        }

        var selectedSkillButton = data.pointerPress.GetComponent<UI_Skill>();
        if (selectedSkillButton == null) return;
        
        _gameVm.CurrentSelectedSkillButton = selectedSkillButton;
        Util.GetFrameFromButton(_gameVm.CurrentSelectedSkillButton).color = Color.cyan;
        
        _gameVm.ShowUpgradePopup();
    }

    #endregion
    
    protected override void BindObjects()
    {
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.UpgradeButton).gameObject.BindEvent(OnUpgradeClicked);
    }

    public void InitUpgradeButton()
    {
        var button = GetButton((int)Buttons.UpgradeButton);
        SetObjectSize(button.gameObject, 0.95f);
        if (_gameVm.CurrentSelectedPortrait == null) return;

        if (_gameVm.GetLevelFromUiObject(_gameVm.CurrentSelectedPortrait.UnitId) == 3)
        {
            button.interactable = false;
            Util.FindChild(button.gameObject, "BuyPanel", false, true).SetActive(false);
            Util.FindChild(button.gameObject, "CompletePanel", false, true).SetActive(true);
        }
        else
        {
            button.interactable = true;
            Util.FindChild(button.gameObject, "CompletePanel", false, true).SetActive(false);
            Util.FindChild(button.gameObject, "BuyPanel", false, true).SetActive(true);
            
            _gameVm.UpdateUpgradeCostRequired();
        }
    }
}
