using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
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
    // private RectTransform _skillInfoPanelRect;
    private GameObject _statDetailPanel;
    private GameObject _skillDescriptionPanel;
    private GameObject _currentSkillPanel;
    private GameObject _mainSkillTextPanel;
    private GameObject _mainSkillPanel;
    private GameObject _mainSkillIcon;
    private readonly Dictionary<string, GameObject> _textDict = new();
    private readonly Dictionary<int, Button> _levelButtons = new();

    public Card SelectedCard { get; set; }
    
    public bool ShowDetails
    {
        get => _showDetails;
        set
        {
            _showDetails = value;
            _ = ToggleStatDetailPanel();
        }
    }

    public Button SelectedButton
    {
        set
        {
            _selectedButton = value;
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
        
        DetailButton
    }

    private enum Texts
    {
        UnitInfoNameText,
        UnitInfoDetailButtonText,
        
        UnitInfoClassTitleText,
        UnitInfoRegionTitleText,
        UnitInfoRoleTitleText,
        UnitInfoLocationTitleText,
        UnitInfoTypeTitleText,
        UnitInfoAttackTypeTitleText,
        
        UnitClassText,
        UnitRegionText,
        UnitRoleText,
        UnitLocationText,
        UnitTypeText,
        UnitAttackTypeText,
        
        UnitInfoSkillTreeText,
        SkillDescriptionText,
        SkillDescriptionGoldText,
        UnitInfoMainSkillText,
        UnitInfoMainSkillDescriptionText,
        
        UnitInfoHpTitleText,
        UnitInfoMpTitleText,
        UnitInfoAttackTitleText,
        UnitInfoMagicalAttackTitleText,
        UnitInfoAttackSpeedTitleText,
        UnitInfoMoveSpeedTitleText,
        UnitInfoDefenceTitleText,
        UnitInfoMagicalDefenceTitleText,
        UnitInfoFireResistTitleText,
        UnitInfoPoisonResistTitleText,
        UnitInfoAttackRangeTitleText,
        UnitInfoSkillRangeTitleText,
        
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
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindObjects();
            InitButtonEvents();
            GetUnitInfo((UnitId)SelectedCard.Id);
            InitUI();
            await SetLevelButton(_unitInfo);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
        
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
        // _skillInfoPanelRect = GetImage((int)Images.SkillInfoPanel).GetComponent<RectTransform>();
        _statDetailPanel = GetImage((int)Images.StatDetailPanel).gameObject;

        var statDetailPanelRect = _statDetailPanel.GetComponent<RectTransform>();
        SetDetailStat();
        statDetailPanelRect.anchorMax = new Vector2(0.43f, 1);
        statDetailPanelRect.anchorMin = new Vector2(0.43f, 0.12f);
        _statDetailPanel.gameObject.SetActive(false);
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

    private void GetAssetInfo(int id)
    {
        Managers.Data.SheepInfoDict.TryGetValue(id, out var sheepInfo);
        if (sheepInfo == null)
        {
            Managers.Data.EnchantInfoDict.TryGetValue(id, out var enchantInfo);
            if (enchantInfo == null)
            {
                Managers.Data.CharacterInfoDict.TryGetValue(id, out var characterInfo);
            }
        }
    }
    
    private async Task SetLevelButton(UnitInfo unitInfo)
    {
        foreach (var button in _levelButtons.Values)
        {
            button.GetComponent<Button>().interactable = true;
        }
        
        SelectedButton = _levelButtons[unitInfo.Level].GetComponent<Button>();

        await Task.WhenAll(SetCardImage(_unitInfo), SetStatus(), SetSkillPanel(_unitInfo), SetMainSkillPanel(_unitInfo));

        if (_statDetailPanel.gameObject.activeSelf)
        {
            SetDetailStat();
        }
    }

    private async Task SetCardImage(IAsset asset)
    {
        // Set card image
        SetObjectSize(GetImage((int)Images.CardPanel).gameObject, 0.8f, 1.28f);
        
        var parent = GetImage((int)Images.CardPanel).transform;
        var cardFrame = await Managers.Resource.GetCardResources<UnitId>(asset, parent);
        var rect = cardFrame.GetComponent<RectTransform>();
        
        Util.FindChild(cardFrame, "Role", true).SetActive(false);
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
    }

    private async Task SetStatus()
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
        var unitNameText = GetText((int)Texts.UnitInfoNameText);
        var key = string.Concat("unit_name_", Managers.Localization.GetConvertedString(_unitData.Name));
        await Managers.Localization.BindLocalizedText(unitNameText, key, FontType.BlackLined);
        
        GetImage((int)Images.UnitClassImage).sprite = await Managers.Resource.LoadAsync<Sprite>(classPath);
        GetImage((int)Images.UnitRegionImage).sprite = await Managers.Resource.LoadAsync<Sprite>(regionPath);
        GetImage((int)Images.UnitRoleImage).sprite = await Managers.Resource.LoadAsync<Sprite>(rolePath);
        GetImage((int)Images.UnitLocationImage).sprite = await Managers.Resource.LoadAsync<Sprite>(locationPath);
        GetImage((int)Images.UnitTypeImage).sprite = await Managers.Resource.LoadAsync<Sprite>(typePath);
        GetImage((int)Images.UnitAttackTypeImage).sprite = await Managers.Resource.LoadAsync<Sprite>(attackTypePath);
        
        GetText((int)Texts.UnitClassText).text = _unitInfo.Class.ToString();
        GetText((int)Texts.UnitRegionText).text = _unitInfo.Region.ToString();
        GetText((int)Texts.UnitRoleText).text = _unitInfo.Role.ToString();
        GetText((int)Texts.UnitLocationText).text = _unitData.RecommendedLocation;
        GetText((int)Texts.UnitTypeText).text = type;
        GetText((int)Texts.UnitAttackTypeText).text = attackType;
    }
    
    private async Task SetSkillPanel(IAsset asset)
    {
        if (_currentSkillPanel != null)
        {
            Destroy(_currentSkillPanel);
        }
        
        var parent = GetImage((int)Images.SkillPanel);
        var skillPanelPath = $"UI/InGame/SkillPanel/{((UnitId)asset.Id).ToString()}SkillPanel";
        _currentSkillPanel = await Managers.Resource.Instantiate(skillPanelPath);
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
        _skillDescriptionPanel.gameObject.SetActive(false);
    }

    private async Task SetMainSkillPanel(IAsset asset)
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
            var mainSkillText = GetText((int)Texts.UnitInfoMainSkillDescriptionText);
            var mainSkillImage = GetImage((int)Images.MainSkillImage);
            var mainSkillIcon = _currentSkillPanel.GetComponentsInChildren<Button>()
                .FirstOrDefault(button => button.name.Contains(skillId.ToString()))?
                .transform.parent.parent.gameObject;
            
            _mainSkillIcon = Instantiate(mainSkillIcon, _mainSkillPanel.transform);
            var copiedSkillIconRect = _mainSkillIcon.GetComponent<RectTransform>();
            var mainSkillImageRect = mainSkillImage.GetComponent<RectTransform>();

            Managers.Data.SkillDict.TryGetValue((int)skillId, out var skillData);
            if (skillData == null || mainSkillIcon == null) return;
            
            await Managers.Localization.BindLocalizedSkillText(mainSkillText, skillData, _unitInfo.Id);
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
        GetText((int)Texts.UnitMagicalDefenceText).text = _unitData.Stat.Defence.ToString();
        GetText((int)Texts.UnitFireResistText).text = _unitData.Stat.FireResist + " %";
        GetText((int)Texts.UnitPoisonResistText).text = _unitData.Stat.PoisonResist + " %";
        GetText((int)Texts.UnitAttackRangeText).text = _unitData.Stat.AttackRange.ToString();
        GetText((int)Texts.UnitSkillRangeText).text = _unitData.Stat.SkillRange.ToString();
    }

    private async Task ToggleStatDetailPanel()
    {
        if (_isAnimating) return;
        
        var detailButtonText = GetText((int)Texts.UnitInfoDetailButtonText).GetComponent<TextMeshProUGUI>();
        var key = ShowDetails ? "unit_info_detail_button_text_summary" : "unit_info_detail_button_text_detail";
        detailButtonText.text = await Managers.Localization.BindLocalizedText(detailButtonText, key);

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
    private async Task OnLevelButtonClicked(PointerEventData data)
    {
        var cardPanel = GetImage((int)Images.CardPanel);
        var clickedButton = data.pointerPress.GetComponent<Button>();
        var level = _levelButtons.FirstOrDefault(pair => pair.Value == clickedButton).Key;
        var newUnitId = Managers.Data.UnitDict
            .Where(pair => pair.Value.UnitSpecies == (Species)_unitInfo.Species && pair.Value.Stat.Level == level)
            .Select(pair => pair.Key)
            .FirstOrDefault();
        
        Util.DestroyAllChildren(cardPanel.transform);
        GetUnitInfo((UnitId)newUnitId);
        await SetLevelButton(_unitInfo);
    }

    private async Task OnSkillButtonClicked(PointerEventData data)
    {
        _skillDescriptionPanel.gameObject.SetActive(true);
        
        var skillDescriptionGoldImage = GetImage((int)Images.SkillDescriptionGoldImage);
        var skillDescriptionText = GetText((int)Texts.SkillDescriptionText);
        var skillDescriptionGoldText = GetText((int)Texts.SkillDescriptionGoldText);
        var skillName = data.pointerPress.name.Replace("Button", "");
        var skillNumber = Managers.Data.SkillDict.Values
            .FirstOrDefault(skill => skill.Id == (int)skillName.ToEnum<Skill>())?.Id;
        
        if (skillNumber == null) return;
        Managers.Data.SkillDict.TryGetValue((int)skillNumber, out var skillData);

        if (skillData == null) return;
        skillDescriptionGoldImage.rectTransform.sizeDelta = new Vector2(Screen.width * 0.045f, Screen.width * 0.045f);
        
        await Managers.Localization.BindLocalizedSkillText(skillDescriptionText, skillData, _unitInfo.Id);
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
