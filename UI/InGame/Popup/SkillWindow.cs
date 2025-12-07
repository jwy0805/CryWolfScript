using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    Task InitUIAsync(UnitId unitId);
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
        SkillWindowUpgradeText,
        SkillWindowUpgradeGoldText,
    }
    
    private GameViewModel _gameVm;
    private TutorialViewModel _tutorialVm;
    
    private GameObject _skillPanel;
    private Button[] _skillButtons;
    
    [Inject]
    public void Construct(GameViewModel gameViewModel, TutorialViewModel tutorialViewModel)
    {
        _gameVm = gameViewModel;
        _tutorialVm = tutorialViewModel;
    }

    protected override async void Init()
    {
        try
        {
            base.Init();
            _gameVm.SkillWindow = this;
        
            BindObjects();
            InitButtonEvents();
            InitUpgradeButton();
            await InitUIAsync(_gameVm.CurrentSelectedPortrait.UnitId);
        
            // Tutorial
            if (_tutorialVm.CurrentTag.Contains("CheckSkillTree")) 
            {
                _tutorialVm.StepTutorialByClickingUI();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public async Task InitUIAsync(UnitId unitId)
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
        
        _skillPanel = await Managers.Resource.Instantiate($"UI/InGame/SkillPanel/{unitId.ToString()}SkillPanel");
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
        var currentNameText = GetText((int)Texts.CurrentName);
        currentNameText.text = await Managers.Localization.BindLocalizedText(currentNameText, unitId.ToString());
    }
    
    // Update skill button when a skill is upgraded
    public void UpdateSkillButton()
    {
        foreach (var skillButton in _skillButtons)
        {
            var skillName = skillButton.name.Replace("Button", "");
            if (_gameVm.SkillsUpgraded.Contains(skillName.ToEnum<Skill>()))
            {
                Util.SetAlpha(skillButton.gameObject.GetComponent<Image>(), 1f);
            }
        }
        
        _gameVm.UpdateUpgradeCostRequired();
    }

    // Update the cost(gold text) in the upgrade button
    public void UpdateUpgradeCost(int cost)
    {
        GetText((int)Texts.SkillWindowUpgradeGoldText).text = cost.ToString();
    }

    #region Button Events

    private void OnUpgradeClicked(PointerEventData data)
    {
        if (_tutorialVm.CurrentTag.Contains("CheckUpgrade"))
        {
            _tutorialVm.SendHoldPacket(false);
        }
        
        _gameVm.OnUnitUpgradeClicked();
    }

    private async Task OnSkillButtonClicked(PointerEventData data)
    {
        Managers.UI.ClosePopupUI<UI_UpgradePopup>();
        foreach (var skillButton in _skillButtons)
        {
            Managers.Resource.GetFrameFromCardButton(skillButton.GetComponent<UI_Skill>()).color = Color.green;
        }

        var selectedSkillButton = data.pointerPress.GetComponent<UI_Skill>();
        if (selectedSkillButton == null) return;
        
        _gameVm.CurrentSelectedSkillButton = selectedSkillButton;
        Managers.Resource.GetFrameFromCardButton(_gameVm.CurrentSelectedSkillButton).color = Color.cyan;
        
        await _gameVm.ShowUpgradePopup();
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
        SetObjectSize(button.gameObject);
        if (_gameVm.CurrentSelectedPortrait == null) return;
        
        var unitLevel = _gameVm.GetLevelFromUiObject(_gameVm.CurrentSelectedPortrait.UnitId);
        var buyObject = Util.FindChild(button.gameObject, "BuyPanel", false, true);
        var completeObject = Util.FindChild(button.gameObject, "CompletePanel", false, true);
        
        if (unitLevel == 3)
        {
            button.interactable = false;
            buyObject.SetActive(false);
            completeObject.SetActive(true);
        }
        else
        {
            var unitIds = Util.Faction == Faction.Sheep
                ? User.Instance.DeckSheep.UnitsOnDeck.Select(info => info.Id).ToArray()
                : User.Instance.DeckWolf.UnitsOnDeck.Select(info => info.Id).ToArray();
            var availableUnits = new List<int>();
            availableUnits.AddRange(
                unitIds.SelectMany(id =>
                {
                    var level = id % 100 % 3;
                    return level switch
                    {
                        0 => new[] { id, id - 1, id - 2 },
                        1 => new[] { id },
                        2 => new[] { id, id - 1 },
                        _ => Array.Empty<int>()
                    };
                })
            );
            
            var isOwned = availableUnits.Contains((int)_gameVm.CurrentSelectedPortrait.UnitId + 1);
            
            button.interactable = isOwned;
            buyObject.SetActive(isOwned);
            completeObject.SetActive(!isOwned);

            if (isOwned)
            {
                _gameVm.UpdateUpgradeCostRequired();
            }
        }
    }
}
