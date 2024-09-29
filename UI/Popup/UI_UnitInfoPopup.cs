using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using DG.Tweening;
// ReSharper disable SpecifyACultureInStringConversionExplicitly

/* Last Modified : 24. 09. 12
 * Version : 0.02
 */

public class UI_UnitInfoPopup : UI_Popup
{
    private bool _isAnimating;
    private bool _showDetails;
    private Button _selectedButton;
    private UnitInfo _unitInfo;
    private Contents.UnitData _unitData;
    private RectTransform _skillInfoPanelRect;
    private GameObject _statDetailPanel;
    private GameObject _skillDescriptionPanel;
    private GameObject _currentSkillPanel;
    private GameObject _mainSkillTextPanel;
    private GameObject _mainSkillPanel;
    private GameObject _mainSkillIcon;
    private readonly Dictionary<int, Button> _levelButtons = new();

    public Card SelectedCard { get; set; }
    
    public bool ShowDetails
    {
        get => _showDetails;
        set
        {
            _showDetails = value;
            var detailButtonText = GetText((int)Texts.DetailButtonText).GetComponent<TextMeshProUGUI>();
            detailButtonText.text = _showDetails ? "Summary" : "Detail";
            ToggleStatDetailPanel();
        }
    }

    public Button SelectedButton
    {
        set
        {
            _selectedButton = value;
            var buttonImage = _selectedButton.GetComponent<Image>(); 
            buttonImage.color = new Color(171f/255f, 140f/255f, 64f/255f) ;
            _selectedButton.interactable = false;
        }
    }
    
    #region Enums
    
    private enum Images
    {
        ScrollView,
        CardPanel,
        StatPanel,
        StatDetailPanel,
        
        UnitClassPanel,
        UnitRegionPanel,
        UnitRolePanel,
        UnitLocationPanel,
        UnitTypePanel,
        UnitAttackTypePanel,
        
        UnitClassFrame,
        UnitRegionFrame,
        UnitRoleFrame,
        UnitLocationFrame,
        UnitTypeFrame,
        UnitAttackTypeFrame,
        
        UnitClassImage,
        UnitRegionImage,
        UnitRoleImage,
        UnitLocationImage,
        UnitTypeImage,
        UnitAttackTypeImage,
        
        SkillInfoPanel,
        SkillPanel,
        SkillTreeTextPanel,
        SkillDescriptionPanel,
        SkillDescriptionGoldImage,
        MainSkillTextPanel,
        MainSkillPanel,
        MainSkillImage
    }

    private enum Buttons
    {
        ExitButton,
        EnterButton,
        
        LevelButton1,
        LevelButton2,
        LevelButton3,
        
        UpgradeButton,
        DetailButton
    }

    private enum Texts
    {
        UnitNameText,
        DetailButtonText,
        
        UnitClassText,
        UnitRegionText,
        UnitRoleText,
        UnitLocationText,
        UnitTypeText,
        UnitAttackTypeText,
        
        SkillDescriptionText,
        SkillDescriptionGoldText,
        MainSkillDescriptionText,
        
        UnitHpText,
        UnitMpText,
        UnitAttackText,
        UnitMagicalAttackText,
        UnitAttackSpeedText,
        UnitMoveSpeedText,
        UnitDefenceText,
        UnitMagicalDefenceText,
        UnitFireResistText,
        UnitPoisonResistText,
        UnitAttackRangeText,
        UnitSkillRangeText,
    }
    
    #endregion
    
    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        GetUnitInfo((UnitId)SelectedCard.Id);
        InitUI();
        SetLevelButton(_unitInfo);
    }

    protected override void BindObjects()
    {
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        Bind<TextMeshProUGUI>(typeof(Texts));
        
        _levelButtons.Add(1, GetButton((int)Buttons.LevelButton1));
        _levelButtons.Add(2, GetButton((int)Buttons.LevelButton2));
        _levelButtons.Add(3, GetButton((int)Buttons.LevelButton3));
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(CloseAllPopup);
        GetButton((int)Buttons.EnterButton).gameObject.BindEvent(CloseAllPopup);
        GetButton((int)Buttons.LevelButton1).gameObject.BindEvent(OnLevelButtonClicked);
        GetButton((int)Buttons.LevelButton2).gameObject.BindEvent(OnLevelButtonClicked);
        GetButton((int)Buttons.LevelButton3).gameObject.BindEvent(OnLevelButtonClicked);
        
        GetButton((int)Buttons.DetailButton).gameObject.BindEvent(OnDetailButtonClicked);
    }

    protected override void InitUI()
    { 
        var skillTreeTextPanel = GetImage((int)Images.SkillTreeTextPanel);
        _mainSkillTextPanel = GetImage((int)Images.MainSkillTextPanel).gameObject;
        _mainSkillPanel = GetImage((int)Images.MainSkillPanel).gameObject;
        _skillInfoPanelRect = GetImage((int)Images.SkillInfoPanel).GetComponent<RectTransform>();
        _statDetailPanel = GetImage((int)Images.StatDetailPanel).gameObject;

        var statDetailPanelRect = _statDetailPanel.GetComponent<RectTransform>();
        SetDetailStat();
        statDetailPanelRect.anchorMax = new Vector2(0.43f, 1);
        statDetailPanelRect.anchorMin = new Vector2(0.43f, 0.12f);
        _statDetailPanel.gameObject.SetActive(false);

        AdjustLayoutElement(_mainSkillTextPanel, 0.12f, 0.5f);
        AdjustLayoutElement(_mainSkillPanel, 0.12f, 0.96f);
        AdjustLayoutElement(skillTreeTextPanel.gameObject, 0.12f, 0.5f);
    }
    
    private void GetUnitInfo(UnitId unitId)
    {
        Managers.Data.UnitDict.TryGetValue((int)unitId, out var unitData);
        if (unitData == null) return;
        
        _unitData = unitData;
        _unitInfo = new UnitInfo
        {
            Id = unitData.Id,
            Level = unitData.Stat.Level,
            Faction = unitData.Faction,
            Class = unitData.UnitClass,
            Role = unitData.UnitRole,
            Species = (int)unitData.UnitSpecies,
            Region = unitData.Region
        };
    }
    
    private void SetLevelButton(UnitInfo unitInfo)
    {
        foreach (var button in _levelButtons.Values)
        {
            button.GetComponent<Image>().color = new Color(248f/255f, 211f/255f, 123f/255f);
            button.GetComponent<Button>().interactable = true;
        }
        
        SelectedButton = _levelButtons[unitInfo.Level].GetComponent<Button>();
        
        SetCardImage(_unitInfo);
        SetStatus();
        SetSkillPanel(_unitInfo);
        SetMainSkillPanel(_unitInfo);

        if (_statDetailPanel.gameObject.activeSelf)
        {
            SetDetailStat();
        }
    }

    private void SetCardImage(IAsset asset)
    {
        // Set card image
        SetObjectSize(GetImage((int)Images.CardPanel).gameObject, 0.7f, 1.12f);
        
        var parent = GetImage((int)Images.CardPanel).transform;
        var cardPanelSize = parent.GetComponent<RectTransform>().sizeDelta;
        var cardFrame = Util.GetCardResources<UnitId>(asset, parent);
        var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
        
        cardFrame.transform.SetParent(parent);
        cardFrame.GetComponent<RectTransform>().anchorMax = Vector2.one;
        cardFrame.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        cardUnit.GetComponent<RectTransform>().sizeDelta = new Vector2(cardPanelSize.x * 0.9f, cardPanelSize.x * 0.9f);
    }

    private void SetStatus()
    {
        var classPath = $"Sprites/Icons/icon_class_{_unitInfo.Class.ToString()}";
        var regionPath = $"Sprites/Icons/icon_region_{_unitInfo.Region.ToString()}";
        var rolePath = $"Sprites/Icons/icon_role_{_unitInfo.Role.ToString()}";
        var locationPath = $"Sprites/Icons/icon_location_{_unitData.RecommendedLocation}";

        var type = _unitData.Stat.UnitType == 0 ? "ground" : "air";
        var attackType = _unitData.Stat.AttackType switch
        {
            0 => "ground",
            1 => "air",
            2 => "both",
            _ => throw new ArgumentOutOfRangeException()
        };
        
        var typePath = $"Sprites/Icons/icon_type_{type}";
        var attackTypePath = $"Sprites/Icons/icon_attack_{attackType}";
        
        GetImage((int)Images.UnitClassImage).sprite = Resources.Load<Sprite>(classPath);
        GetImage((int)Images.UnitRegionImage).sprite = Resources.Load<Sprite>(regionPath);
        GetImage((int)Images.UnitRoleImage).sprite = Resources.Load<Sprite>(rolePath);
        GetImage((int)Images.UnitLocationImage).sprite = Resources.Load<Sprite>(locationPath);
        GetImage((int)Images.UnitTypeImage).sprite = Resources.Load<Sprite>(typePath);
        GetImage((int)Images.UnitAttackTypeImage).sprite = Resources.Load<Sprite>(attackTypePath);
        
        GetText((int)Texts.UnitNameText).text = _unitData.Name;
        GetText((int)Texts.UnitClassText).text = _unitInfo.Class.ToString();
        GetText((int)Texts.UnitRegionText).text = _unitInfo.Region.ToString();
        GetText((int)Texts.UnitRoleText).text = _unitInfo.Role.ToString();
        GetText((int)Texts.UnitLocationText).text = _unitData.RecommendedLocation;
        GetText((int)Texts.UnitTypeText).text = type;
        GetText((int)Texts.UnitAttackTypeText).text = attackType;
    }
    
    private void SetSkillPanel(IAsset asset)
    {
        if (_currentSkillPanel != null)
        {
            Destroy(_currentSkillPanel);
        }
        
        var parent = GetImage((int)Images.SkillPanel);
        AdjustLayoutElement(parent.gameObject, 0.6f, 0.96f);
        
        var skillPanelPath = $"UI/InGame/SkillPanel/{((UnitId)asset.Id).ToString()}SkillPanel";
        _currentSkillPanel = Managers.Resource.Instantiate(skillPanelPath);
        _currentSkillPanel.transform.SetParent(parent.transform);
        
        var skillPanelRect = _currentSkillPanel.GetComponent<RectTransform>();
        skillPanelRect.anchoredPosition = new Vector2(0, 0);
        skillPanelRect.sizeDelta = new Vector2(0, 0);
        
        var skillButtons = _currentSkillPanel.GetComponentsInChildren<Button>();
        foreach (var skillButton in skillButtons)
        {
            skillButton.gameObject.BindEvent(OnSkillButtonClicked);
            SetObjectSize(skillButton.gameObject, 0.22f);
        }

        _skillDescriptionPanel = GetImage((int)Images.SkillDescriptionPanel).gameObject;
        AdjustLayoutElement(_skillDescriptionPanel, 0.2f, 0.96f);
        _skillDescriptionPanel.gameObject.SetActive(false);
    }

    private void SetMainSkillPanel(IAsset asset)
    {
        if (_mainSkillIcon != null)
        {
            Destroy(_mainSkillIcon);
        }
        
        Managers.Data.MainSkillDict.TryGetValue((UnitId)asset.Id, out var mainSkill);
        if (mainSkill == null) return;

        if (mainSkill.Count == 0)
        {
            _mainSkillTextPanel.SetActive(false);
            _mainSkillPanel.SetActive(false);
        }
        else
        {
            _mainSkillTextPanel.SetActive(true);
            _mainSkillPanel.SetActive(true);

            var skillId = mainSkill[0];
            var mainSkillText = GetText((int)Texts.MainSkillDescriptionText);
            var mainSkillImage = GetImage((int)Images.MainSkillImage);
            var mainSkillIcon = _currentSkillPanel.GetComponentsInChildren<Button>()
                .FirstOrDefault(button => button.name.Contains(skillId.ToString()))?
                .transform.parent.parent.gameObject;
            
            _mainSkillIcon = Instantiate(mainSkillIcon, _mainSkillPanel.transform);
            var copiedSkillIconRect = _mainSkillIcon.GetComponent<RectTransform>();
            var mainSkillImageRect = mainSkillImage.GetComponent<RectTransform>();

            Managers.Data.SkillDict.TryGetValue((int)skillId, out var skillData);
            if (skillData == null || mainSkillIcon == null) return;
            
            mainSkillText.text = skillData.Explanation;
            mainSkillImageRect.sizeDelta = new Vector2(Screen.width * 0.1f, Screen.width * 0.1f);
            
            _mainSkillIcon.transform.SetParent(mainSkillImage.transform);
            copiedSkillIconRect.offsetMin = Vector2.zero;
            copiedSkillIconRect.offsetMax = Vector2.zero;
            copiedSkillIconRect.anchorMax = Vector2.one;
            copiedSkillIconRect.anchorMin = Vector2.zero;
        }
    }

    private void SetDetailStat()
    {
        GetText((int)Texts.UnitHpText).text = _unitData.Stat.Hp.ToString();
        GetText((int)Texts.UnitMpText).text = _unitData.Stat.Mp.ToString();
        GetText((int)Texts.UnitAttackText).text = _unitData.Stat.Attack.ToString();
        GetText((int)Texts.UnitMagicalAttackText).text = _unitData.Stat.Skill.ToString();
        GetText((int)Texts.UnitAttackSpeedText).text = _unitData.Stat.AttackSpeed.ToString();
        GetText((int)Texts.UnitMoveSpeedText).text = _unitData.Stat.MoveSpeed.ToString();
        GetText((int)Texts.UnitDefenceText).text = _unitData.Stat.Defence.ToString();
        // TODO: Add Magical Defence Stat
        GetText((int)Texts.UnitMagicalDefenceText).text = _unitData.Stat.Defence.ToString();
        GetText((int)Texts.UnitFireResistText).text = _unitData.Stat.FireResist + " %";
        GetText((int)Texts.UnitPoisonResistText).text = _unitData.Stat.PoisonResist + " %";
        GetText((int)Texts.UnitAttackRangeText).text = _unitData.Stat.AttackRange.ToString();
        GetText((int)Texts.UnitSkillRangeText).text = _unitData.Stat.SkillRange.ToString();
    }

    private void AdjustLayoutElement(GameObject go, float height, float width)
    {
        var layoutElement = go.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = _skillInfoPanelRect.rect.height * height;
        layoutElement.preferredWidth = _skillInfoPanelRect.rect.width * width;
    }

    private void ToggleStatDetailPanel()
    {
        if (_isAnimating) return;

        var rect = _statDetailPanel.GetComponent<RectTransform>();
        
        if (ShowDetails)
        {
            _statDetailPanel.SetActive(true);
            StartCoroutine(AnimateStatDetailPanel(rect, Vector2.one, 0.2f));
        }
        else
        {
            StartCoroutine(AnimateStatDetailPanel(rect, new Vector2(0.43f, 1), 0.2f, true));
        }
    }

    private IEnumerator AnimateStatDetailPanel(RectTransform rectTransform, Vector2 targetAnchorMax, float duration,
        bool deactivateAfter = false)
    {
        _isAnimating = true;
        float elapsedTime = 0;
        Vector2 initialAnchorMax = rectTransform.anchorMax;

        while (elapsedTime < duration)
        {
            rectTransform.anchorMax = Vector2.Lerp(initialAnchorMax, targetAnchorMax, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        rectTransform.anchorMax = targetAnchorMax;

        if (deactivateAfter)
        {
            rectTransform.gameObject.SetActive(false);
        }

        _isAnimating = false;
    }
    
    // Button events
    private void OnLevelButtonClicked(PointerEventData data)
    {
        var clickedButton = data.pointerPress.GetComponent<Button>();
        var level = _levelButtons.FirstOrDefault(pair => pair.Value == clickedButton).Key;
        var newUnitId = Managers.Data.UnitDict
            .Where(pair => pair.Value.UnitSpecies == (Species)_unitInfo.Species && pair.Value.Stat.Level == level)
            .Select(pair => pair.Key)
            .FirstOrDefault();

        GetUnitInfo((UnitId)newUnitId);
        SetLevelButton(_unitInfo);
    }

    private void OnSkillButtonClicked(PointerEventData data)
    {
        _skillDescriptionPanel.gameObject.SetActive(true);
        
        var skillDescriptionGoldImage = GetImage((int)Images.SkillDescriptionGoldImage);
        var skillDescriptionText = GetText((int)Texts.SkillDescriptionText);
        var skillDescriptionGoldText = GetText((int)Texts.SkillDescriptionGoldText);
        var skillName = data.pointerPress.name.Replace("Button", "");
        var skillNumber = Managers.Data.SkillDict.Values
            .FirstOrDefault(skill => skill.Id == (int)Enum.Parse(typeof(Skill), skillName))?
            .Id;
        
        if (skillNumber == null) return;
        Managers.Data.SkillDict.TryGetValue((int)skillNumber, out var skillData);

        if (skillData == null) return;
        skillDescriptionGoldImage.rectTransform.sizeDelta = new Vector2(Screen.width * 0.045f, Screen.width * 0.045f);
        skillDescriptionText.text = skillData.Explanation;
        skillDescriptionGoldText.text = skillData.Cost.ToString();
    }
    
    private void OnDetailButtonClicked(PointerEventData data)
    {
        ShowDetails = !ShowDetails;
    }
    
    private void CloseAllPopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
