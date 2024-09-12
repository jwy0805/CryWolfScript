using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

/* Last Modified : 24. 09. 12
 * Version : 0.02
 */

public class UI_UnitInfoPopup : UI_Popup
{
    private bool _showDetails;
    private Button _selectedButton;
    private UnitInfo _unitInfo;
    private RectTransform _skillInfoPanelRect;
    private GameObject _skillDescriptionPanel;
    private GameObject _currentSkillPanel;
    private GameObject _mainSkillTextPanel;
    private GameObject _mainSkillPanel;
    private GameObject _mainSkillIcon;
    private readonly Dictionary<int, Button> _levelButtons = new();

    public Card SelectedCard { get; set; }

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
        
        UnitClassText,
        UnitRegionText,
        UnitRoleText,
        UnitLocationText,
        UnitTypeText,
        UnitAttackTypeText,
        
        SkillDescriptionText,
        SkillDescriptionGoldText,
        MainSkillDescriptionText
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
        
        GetUnitInfo((UnitId)SelectedCard.Id);
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
    }

    protected override void InitUI()
    { 
        var skillTreeTextPanel = GetImage((int)Images.SkillTreeTextPanel);
        _mainSkillTextPanel = GetImage((int)Images.MainSkillTextPanel).gameObject;
        _mainSkillPanel = GetImage((int)Images.MainSkillPanel).gameObject;
        _skillInfoPanelRect = GetImage((int)Images.SkillInfoPanel).GetComponent<RectTransform>();

        AdjustLayoutElement(_mainSkillTextPanel, 0.12f, 0.5f);
        AdjustLayoutElement(_mainSkillPanel, 0.12f, 0.96f);
        AdjustLayoutElement(skillTreeTextPanel.gameObject, 0.12f, 0.5f);
    }
    
    private void GetUnitInfo(UnitId unitId)
    {
        Managers.Data.UnitDict.TryGetValue((int)unitId, out var unitData);
        if (unitData == null) return;
        
        _unitInfo = new UnitInfo
        {
            Id = unitData.Id,
            Level = unitData.Stat.Level,
            Camp = unitData.Camp,
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
        var locationPath = $"Sprites/Icons/icon_location_{Managers.Data.UnitDict[_unitInfo.Id].RecommendedLocation}";

        var type = Managers.Data.UnitDict[_unitInfo.Id].Stat.UnitType == 0 ? "ground" : "air";
        var attackType = Managers.Data.UnitDict[_unitInfo.Id].Stat.AttackType switch
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
        
        GetText((int)Texts.UnitNameText).text = Managers.Data.UnitDict[_unitInfo.Id].Name;
        GetText((int)Texts.UnitClassText).text = _unitInfo.Class.ToString();
        GetText((int)Texts.UnitRegionText).text = _unitInfo.Region.ToString();
        GetText((int)Texts.UnitRoleText).text = _unitInfo.Role.ToString();
        GetText((int)Texts.UnitLocationText).text = Managers.Data.UnitDict[_unitInfo.Id].RecommendedLocation;
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

    private void AdjustLayoutElement(GameObject go, float height, float width)
    {
        var layoutElement = go.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = _skillInfoPanelRect.rect.height * height;
        layoutElement.preferredWidth = _skillInfoPanelRect.rect.width * width;
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
        var skillNumber = Managers.Data.SkillDict.Values
            .FirstOrDefault(skill => data.pointerPress.name.Contains(((Skill)skill.Id).ToString()))?
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
        _showDetails = !_showDetails;
    }
    
    private void CloseAllPopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
