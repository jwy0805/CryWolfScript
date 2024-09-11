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

/* Last Modified : 24. 09. 10
 * Version : 1.012
 */

public class UI_UnitInfoPopup : UI_Popup
{
    private Button _selectedButton;
    private UnitInfo _unitInfo;
    private readonly Dictionary<int, Button> _levelButtons = new();
    
    public Card SelectedCard { get; set; }

    public Button SelectedButton
    {
        get => _selectedButton;
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
        CardPanel,
        
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
        GetButton((int)Buttons.LevelButton1).gameObject.BindEvent(LevelButtonClicked);
        GetButton((int)Buttons.LevelButton2).gameObject.BindEvent(LevelButtonClicked);
        GetButton((int)Buttons.LevelButton3).gameObject.BindEvent(LevelButtonClicked);
    }

    protected override void InitUI()
    {
        
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

    private void SetInitialCardImage(IAsset asset)
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
    
    private void SetLevelButton(UnitInfo unitInfo)
    {
        foreach (var button in _levelButtons.Values)
        {
            button.GetComponent<Image>().color = new Color(248f/255f, 211f/255f, 123f/255f);
            button.GetComponent<Button>().interactable = true;
        }
        
        SelectedButton = _levelButtons[unitInfo.Level].GetComponent<Button>();
        
        SetInitialCardImage(_unitInfo);
        SetInitialStatus();
    }

    private void SetInitialStatus()
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
    
    // Button events
    private void LevelButtonClicked(PointerEventData data)
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
    
    private void CloseAllPopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
