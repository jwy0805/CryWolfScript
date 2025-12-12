using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyCraftingWidget
{
    private readonly CraftingViewModel _craftingVm;
    private readonly CollectionViewModel _collectionVm;
    private readonly LobbyUtilWidget _utilWidget;
    private readonly Action _destroySelectedCard;
    private readonly Func<Card> _getSelectedCard;
    private readonly Func<Define.ArrangeMode> _getArrangeMode;
    private readonly Action<Define.SelectMode> _setSelectMode;
    private readonly Action<IEnumerator> _openCraftingPanelRoutine;
    private readonly Action<PointerEventData> _onCardClicked;

    private Dictionary<string, GameObject> _collectionUiDict;
    private Dictionary<string, GameObject> _craftingUiDict;
    private GameObject _deckScrollView;
    private GameObject _collectionScrollView;
    private ScrollRect _craftingScrollRect;
    private RectTransform _craftingPanel;
    private RectTransform _craftingCardPanel;
    private Transform _craftCardPanel;
    private Transform _materialPanel;
    private Transform _reinforceCardPanel;
    private Transform _reinforceResultPanel;
    private Transform _unitHoldingCardPanel;
    private Transform _arrowPanel;
    private Button _reinforceButton;
    private Button _reinforcingButton;
    private Button _craftingButton;
    private Button _recyclingButton;
    private TextMeshProUGUI _craftCountText;
    private TextMeshProUGUI _reinforceCardNumberText;
    private TextMeshProUGUI _successRateText;
    
    private Card _selectedCardForCrafting;
    
    public bool IsCraftingPanelOpen { get; set; }
    
    public LobbyCraftingWidget(
        CraftingViewModel craftingVm,
        CollectionViewModel collectionVm,
        LobbyUtilWidget utilWidget,
        Action destroySelectedCard,
        Func<Card> getSelectedCard,
        Func<Define.ArrangeMode> getArrangeMode,
        Action<Define.SelectMode> setSelectMode,
        Action<IEnumerator> openCraftingPanelRoutine,
        Action<PointerEventData> onCardClicked)
    {
        _craftingVm = craftingVm;
        _collectionVm = collectionVm;
        _utilWidget = utilWidget;
        _destroySelectedCard = destroySelectedCard;
        _getSelectedCard = getSelectedCard;
        _getArrangeMode = getArrangeMode;
        _setSelectMode = setSelectMode;
        _openCraftingPanelRoutine = openCraftingPanelRoutine;
        _onCardClicked = onCardClicked;
        
        BindEvents();
    }
    
    private void BindEvents()
    {
        _craftingVm.SetCardOnCraftingPanel += SetCardOnCraftingPanel;
        _craftingVm.SetMaterialsOnCraftPanel += InitMaterialsOnCraftPanel;
        _craftingVm.InitCraftingPanel += InitCraftingPanel;
    }
    
    public void BindViews(
        Dictionary<string, GameObject> collectionUiDict,
        Dictionary<string, GameObject> craftingUiDict,
        GameObject deckScrollView,
        GameObject collectionScrollView,
        ScrollRect craftingScrollRect,
        RectTransform craftingPanel,
        RectTransform craftingCardPanel,
        Transform craftCardPanel,
        Transform materialPanel,
        Transform reinforceCardPanel,
        Transform reinforceResultPanel,
        Transform unitHoldingCardPanel,
        Transform arrowPanel,
        Button reinforceButton,
        Button reinforcingButton,
        Button craftingButton,
        Button recyclingButton,
        TextMeshProUGUI craftCountText,
        TextMeshProUGUI reinforceCardNumberText,
        TextMeshProUGUI successRateText)
    {
        _collectionUiDict = collectionUiDict;
        _craftingUiDict = craftingUiDict;
        _deckScrollView = deckScrollView;
        _collectionScrollView = collectionScrollView;
        _craftingScrollRect = craftingScrollRect;
        _craftingPanel = craftingPanel;
        _craftingCardPanel = craftingCardPanel;
        _craftCardPanel = craftCardPanel;
        _materialPanel = materialPanel;
        _reinforceCardPanel = reinforceCardPanel;
        _reinforceResultPanel = reinforceResultPanel;
        _unitHoldingCardPanel = unitHoldingCardPanel;
        _arrowPanel = arrowPanel;
        _reinforceButton = reinforceButton;
        _reinforcingButton = reinforcingButton;
        _craftingButton = craftingButton;
        _recyclingButton = recyclingButton; 
        _craftCountText = craftCountText;
        _reinforceCardNumberText = reinforceCardNumberText;
        _successRateText = successRateText;
    }
    
    public void InitCraftingPanel()
    {
        var cardPanel = _craftingCardPanel.transform;
        Util.DestroyAllChildren(cardPanel);
        
        _setSelectMode?.Invoke(Define.SelectMode.CraftingPanel);    
        _craftingPanel.gameObject.SetActive(true);
        _utilWidget.SetActivePanels(_craftingUiDict, new[] { "CraftingSelectPanel" });

        var card = _getSelectedCard?.Invoke();
        if (card == null)
        {
            _reinforcingButton.interactable = false;
            _craftingButton.interactable = false;
            _recyclingButton.interactable = false;
            return;
        }
        
        _selectedCardForCrafting = card;
        var type = card.AssetType;
        
        switch (type)
        {
            case Asset.Unit:
                var unitLevel = _collectionVm.GetLevelFromUiObject((UnitId)card.Id);
                _reinforcingButton.interactable = unitLevel != 3;
                _craftingButton.interactable = unitLevel == 1;
                break;
        }
    }
    
    private void InitCraftPanel()
    {
        var card = _getSelectedCard?.Invoke();
        if (card == null) return;
        
        var unitLevel = _collectionVm.GetLevelFromUiObject((UnitId)card.Id);
        var unitId = unitLevel switch
        {
            1 => (UnitId)_selectedCardForCrafting.Id,
            2 => (UnitId)_selectedCardForCrafting.Id - 1,
            _ => (UnitId)_selectedCardForCrafting.Id - 2
        };

        Util.DestroyAllChildren(_craftCardPanel);
        
        _ = _selectedCardForCrafting.AssetType switch
        {
            Asset.Unit => Managers.Resource.GetCardResources<UnitId>(_selectedCardForCrafting, _craftCardPanel),
            Asset.Sheep => Managers.Resource.GetCardResources<SheepId>(_selectedCardForCrafting, _craftCardPanel),
            Asset.Enchant => Managers.Resource.GetCardResources<EnchantId>(_selectedCardForCrafting, _craftCardPanel),
            _ => null
        };    
        
        _craftingVm.LoadMaterials(unitId);
        _craftingVm.CraftingCount = 1;
        _craftingVm.TotalCraftingMaterials = _craftingVm.CraftingMaterials;
        _craftCountText.text = _craftingVm.CraftingCount.ToString();
        _setSelectMode?.Invoke(Define.SelectMode.Craft);
    }
    
    private async Task InitMaterialsOnCraftPanel(
        List<OwnedMaterialInfo> craftingMaterials, 
        List<OwnedMaterialInfo> ownedMaterials)
    {
        Util.DestroyAllChildren(_materialPanel);

        foreach (var material in craftingMaterials)
        {
            var craftingFrame = 
                await Managers.Resource.Instantiate("UI/Lobby/Deck/CraftingMaterialFrame", _materialPanel);
            var itemPanel = 
                await Managers.Resource.GetMaterialResources(material.MaterialInfo, craftingFrame.transform);
            var itemPanelRect = itemPanel.GetComponent<RectTransform>();
            var countTextObject = Util.FindChild(craftingFrame, "CountText", true);
            var countText = countTextObject.GetComponent<TextMeshProUGUI>();
            var nameTextObject = Util.FindChild(craftingFrame, "MaterialNameText", true);
            var nameText = nameTextObject.GetComponent<TextMeshProUGUI>();
            var key = $"material_id_{material.MaterialInfo.Id}";
            var materialName = await Managers.Localization.BindLocalizedText(nameText, key);
            
            if (materialName.Length > 11)
            {
                materialName = materialName.Substring(0, 9) + "..";
            }
            
            itemPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemPanelRect.sizeDelta = new Vector2(125, 125);
            
            var ownedCount = ownedMaterials.FirstOrDefault(info =>
                info.MaterialInfo.Id == material.MaterialInfo.Id)?.Count ?? 0;
            countText.text = $"{ownedCount} / {material.Count}";
            nameText.text = materialName;

            CheckAndSetColorOnMaterialFrame(material, craftingFrame);
        }
    }
    
    private async Task InitReinforcePanel()
    {
        var cardParent = _reinforceCardPanel.transform;
        var resultParent = _reinforceResultPanel.transform;
        
        Util.DestroyAllChildren(cardParent);
        Util.DestroyAllChildren(resultParent);

        var selectedUnit = Managers.Data.UnitInfoDict[_selectedCardForCrafting.Id];
        var resultUnit = Managers.Data.UnitInfoDict[_selectedCardForCrafting.Id + 1];
        var cardFrame = await Managers.Resource.GetCardResources<UnitId>(selectedUnit, cardParent);
        var resultFrame = await Managers.Resource.GetCardResources<UnitId>(resultUnit, resultParent);
        var cardFrameRect = cardFrame.GetComponent<RectTransform>();
        var resultFrameRect = resultFrame.GetComponent<RectTransform>();
        var cardNumberText = _reinforceCardNumberText;
        var rateText = _successRateText;
        
        cardFrameRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardFrameRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardFrameRect.sizeDelta = new Vector2(250, 400);
        resultFrameRect.anchorMin = new Vector2(0.5f, 0.5f);
        resultFrameRect.anchorMax = new Vector2(0.5f, 0.5f);
        resultFrameRect.sizeDelta = new Vector2(250, 400);
        cardNumberText.text = "0 / 8";
        rateText.text = "-";
        
        _craftingVm.InitReinforceSetting();
        _craftingVm.LoadMaterials((UnitId)selectedUnit.Id + 1);
        // _craftingVm.AddNewUnitMaterial(selectedUnit);
        _craftingVm.SetReinforcePointNeeded(resultUnit);
        _reinforcingButton.interactable = false;
        _setSelectMode(Define.SelectMode.Reinforce);
        
        var rate = _craftingVm.GetSuccessRate();
        
        _arrowPanel.GetComponent<ReinforceArrowController>().SetArrowColors(rate);
    }
    
    public void UpdateReinforcePanel()
    {
        var cardNumberText = _reinforceCardNumberText;
        var rateText = _successRateText;
        var rate = _craftingVm.GetSuccessRate();
    
        _reinforceButton.interactable = true;
        _arrowPanel.GetComponent<ReinforceArrowController>().SetArrowColors(rate);
        cardNumberText.text = _craftingVm.ReinforceMaterialUnits.Count + " / 8";
        rateText.text = MathF.Min((int)(rate * 100), 100) + "%";
    } 
    
    private void UpdateCraftingMaterials(List<OwnedMaterialInfo> ownedMaterials)
    {
        _craftingVm.TotalCraftingMaterials = _craftingVm.CraftingMaterials.Select(info => new OwnedMaterialInfo 
        {
            MaterialInfo = new MaterialInfo
            {
                Id = info.MaterialInfo.Id,
                Class = info.MaterialInfo.Class,
            },
            Count = info.Count * _craftingVm.CraftingCount
        }).ToList();
        _craftCountText.text = _craftingVm.CraftingCount.ToString();
        
        var materialPanel = _materialPanel.transform;
        var childCount = materialPanel.childCount;
        for (var i = 0; i < childCount; i++)
        {
            var materialFrame = materialPanel.GetChild(i);
            var itemFrame = Util.FindChild(materialFrame.gameObject, "ItemFrame", true);
            var countText = Util.FindChild(materialFrame.gameObject, "CountText", true);
            var material = _craftingVm.TotalCraftingMaterials
                .FirstOrDefault(info => info.MaterialInfo.Id == materialFrame.GetComponentInChildren<MaterialItem>().Id);
            var ownedCount = ownedMaterials
                .FirstOrDefault(info => info.MaterialInfo.Id == material?.MaterialInfo.Id)?.Count ?? 0;
            if (material == null) continue;
            
            countText.GetComponent<TextMeshProUGUI>().text = $"{ownedCount} / {material.Count}";
            CheckAndSetColorOnMaterialFrame(material, materialFrame.gameObject);
        }
    }
    
    private void CheckAndSetColorOnMaterialFrame(OwnedMaterialInfo material, GameObject craftingFrame)
    {
        var ownedCount = User.Instance.OwnedMaterialList
            .FirstOrDefault(info => info.MaterialInfo.Id == material.MaterialInfo.Id)?.Count ?? 0;
        var countTextObject = Util.FindChild(craftingFrame, "CountText", true);
        var countText = countTextObject.GetComponent<TextMeshProUGUI>();
        countText.color = ownedCount >= material.Count ? Color.green : Color.red;
    }
    
    public async Task ResetCollectionUIForReinforce()
    {
        List<OwnedUnitInfo> units;
        var arrangeMode = _getArrangeMode?.Invoke() ?? Define.ArrangeMode.All;
        switch (arrangeMode)
        {
            default:
            case Define.ArrangeMode.All:
                units = User.Instance.OwnedUnitList
                    .Select(unit => new OwnedUnitInfo
                    {
                        UnitInfo = unit.UnitInfo,
                        Count = unit.Count - _craftingVm.ReinforceMaterialUnits.Count(info => info.Id == unit.UnitInfo.Id)
                    })
                    .OrderBy(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id).ToList();
                break;
            case Define.ArrangeMode.Summary:
                units = User.Instance.OwnedUnitList
                    .GroupBy(info => info.UnitInfo.Species)
                    .Select(group => group.OrderByDescending(info => info.UnitInfo.Level).First())
                    .Select(unit => new OwnedUnitInfo
                    {
                        UnitInfo = unit.UnitInfo,
                        Count = unit.Count - _craftingVm.ReinforceMaterialUnits.Count(info => info.Id == unit.UnitInfo.Id)
                    })
                    .OrderBy(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id).ToList();
                break;
            case Define.ArrangeMode.Class:
                units = User.Instance.OwnedUnitList
                    .OrderByDescending(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id)
                    .Select(unit => new OwnedUnitInfo
                    {
                        UnitInfo = unit.UnitInfo,
                        Count = unit.Count - _craftingVm.ReinforceMaterialUnits.Count(info => info.Id == unit.UnitInfo.Id)
                    }).ToList();
                break;
            case Define.ArrangeMode.Count:
                units = User.Instance.OwnedUnitList
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id)
                    .Select(unit => new OwnedUnitInfo
                    {
                        UnitInfo = unit.UnitInfo,
                        Count = unit.Count - _craftingVm.ReinforceMaterialUnits.Count(info => info.Id == unit.UnitInfo.Id)
                    }).ToList();
                break;
        }
                
        _utilWidget.SetActivePanels(_collectionUiDict, new[] { "UnitHoldingLabelPanel", "UnitHoldingCardPanel" });
        Util.DestroyAllChildren(_unitHoldingCardPanel);

        foreach (var unit in units)
        {
            var cardFrame = 
                await Managers.Resource.GetCardResources<UnitId>(
                    unit.UnitInfo, _unitHoldingCardPanel, _onCardClicked);
            _utilWidget.GetCountText(cardFrame.transform, unit.Count);
            if (_craftingVm.IsUnitInDecks(unit.UnitInfo)) await GetInDeckText(cardFrame.transform);
        }
    }
    
    private async Task GetInDeckText(Transform parent, float rate = 0.7f)
    {
        var inDeckTextPanel = await Managers.Resource.Instantiate("UI/Lobby/Deck/DeckMarkPanel", parent);
        var gridLayout = parent.transform.parent.GetComponent<GridLayoutGroup>();
        var inDeckTextRect = inDeckTextPanel.GetComponent<RectTransform>();
        inDeckTextRect.sizeDelta = new Vector2(gridLayout.cellSize.x * rate, inDeckTextRect.sizeDelta.y);
        inDeckTextRect.anchorMin = new Vector2(0.5f, 0.9f);
        inDeckTextRect.anchorMax = new Vector2(0.5f, 0.9f);
    }
    
    private void GoToCraftingTab()
    {
        Managers.UI.CloseAllPopupUI();
        _utilWidget.FocusTabButton("CraftingTabButton");
        _deckScrollView.gameObject.SetActive(false);
        _collectionScrollView.gameObject.SetActive(true);
        _craftingUiDict["CraftingSelectPanel"].gameObject.SetActive(true);

        OpenCraftingPanel();
        _craftingScrollRect.verticalNormalizedPosition = 1f;
    }

    private void OpenCraftingPanel()
    {
        if (IsCraftingPanelOpen) return;
        
        InitCraftingPanel();
        _openCraftingPanelRoutine?.Invoke(AdjustCraftingPanel(_craftingVm.CraftingPanelHeight));
    }
    
    public void CloseCraftingPanel()
    {
        _craftingPanel.sizeDelta = new Vector2(_craftingPanel.sizeDelta.x, 0);
        _craftingPanel.gameObject.SetActive(false);
        _setSelectMode?.Invoke(Define.SelectMode.Normal);
        _destroySelectedCard?.Invoke();
        _craftingVm.InitReinforceSetting();
    }

    private IEnumerator AdjustCraftingPanel(float targetHeight)
    {
        _craftingPanel.GetComponent<RectTransform>();
        IsCraftingPanelOpen = true;
        float elapsedTime = 0;
        float startHeight = _craftingPanel.sizeDelta.y;
    
        while (elapsedTime < _craftingVm.CraftingPanelDuration)
        {
            float newHeight = Mathf.Lerp(startHeight, targetHeight, elapsedTime / _craftingVm.CraftingPanelDuration);
            _craftingPanel.sizeDelta = new Vector2(_craftingPanel.sizeDelta.x, newHeight);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        
        _craftingPanel.sizeDelta = new Vector2(_craftingPanel.sizeDelta.x, 1000);
        IsCraftingPanelOpen = false;
    }
    
    private async Task SetCardOnCraftingPanel(Card card)
    {
        GoToCraftingTab();
        await LoadCraftingCard(card);
    }
    
    public async Task LoadCraftingCard(Card card)
    {
        try
        {
            var parent = _craftingCardPanel.transform;
            Util.DestroyAllChildren(parent);
            var cardFrame = card.AssetType switch
            {
                Asset.Unit => await Managers.Resource.GetCardResources<UnitId>(card, parent),
                Asset.Sheep => await Managers.Resource.GetCardResources<SheepId>(card, parent),
                Asset.Enchant => await Managers.Resource.GetCardResources<EnchantId>(card, parent),
                Asset.Character => await Managers.Resource.GetCardResources<CharacterId>(card, parent),
                _ => null
            };
        
            if (cardFrame == null) return;
            var cardFrameRect = cardFrame.GetComponent<RectTransform>();
            cardFrameRect.sizeDelta = new Vector2(250, 400);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    
    public void OnCraftingTabClicked()
    {
        GoToCraftingTab();
    }
    
    public async Task OnCraftingClicked(PointerEventData data)
    {
        if (!data.pointerPress.gameObject.GetComponent<Button>().interactable) return;
        if (_getSelectedCard?.Invoke() == null || _selectedCardForCrafting == null) return;
        var activeUis = new[]
        {
            "CraftingBackButtonFakePanel", "CraftingCraftPanel", "MaterialScrollView"
        };
        
        _utilWidget.SetActivePanels(_craftingUiDict, activeUis);
        
        if (_getSelectedCard?.Invoke() == null || _selectedCardForCrafting == null)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
            await Managers.Localization.UpdateWarningPopupText(popup, "warning_select_card");
            return;
        }
        
        InitCraftPanel();
    }
    
    public async Task OnReinforcingClicked(PointerEventData data)
    {
        if (!data.pointerPress.gameObject.GetComponent<Button>().interactable) return;
        if (_getSelectedCard?.Invoke() == null || _selectedCardForCrafting == null) return;
        
        var activeUis = new[] { "CraftingBackButtonFakePanel", "CraftingReinforcePanel", "MaterialScrollView" };
        _utilWidget.SetActivePanels(_craftingUiDict, activeUis);
        await InitReinforcePanel();
        await ResetCollectionUIForReinforce();
    }

    public void OnCraftingBackClicked()
    {
        _destroySelectedCard?.Invoke();
        InitCraftingPanel();
    }
    
    public async Task OnCraftClicked()
    {
        if (_getSelectedCard?.Invoke() == null || _selectedCardForCrafting == null)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
            await Managers.Localization.UpdateWarningPopupText(popup, "warning_select_card");
            return;
        }
        
        _craftingVm.CardToBeCrafted = _craftCardPanel.GetComponentInChildren<Card>();
        _ = _craftingVm.CraftCard();
    }
    
    public void OnCraftUpperArrowClicked(PointerEventData data)
    {
        if (_craftingVm.CraftingCount >= 100) return;
        _craftingVm.CraftingCount++;
        UpdateCraftingMaterials(User.Instance.OwnedMaterialList);        
    }
    
    public void OnCraftLowerArrowClicked(PointerEventData data)
    {
        if (_craftingVm.CraftingCount <= 1) return;
        _craftingVm.CraftingCount--;
        UpdateCraftingMaterials(User.Instance.OwnedMaterialList);
    }

    public async void OnReinforceMaterialClicked(PointerEventData data)
    {
        try
        {
            var unitInfo = Managers.Data.UnitInfoDict[data.pointerPress.GetComponent<Card>().Id];
            _craftingVm.RemoveNewUnitMaterial(unitInfo);
            await ResetCollectionUIForReinforce();
            UpdateReinforcePanel();
            Managers.Resource.Destroy(data.pointerPress.gameObject);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    public async Task OnReinforceClicked()
    {
        var unitInfo = Managers.Data.UnitInfoDict[_selectedCardForCrafting.Id];
        await Managers.UI.ShowPopupUI<UI_ReinforcePopup>();
        await _craftingVm.GetReinforceResult(unitInfo);
        
        _craftingVm.InitReinforceSetting();
    }   
    
    public async Task OnRecyclingClicked()
    {
        var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
        await Managers.Localization.UpdateWarningPopupText(popup, "warning_coming_soon");
    }

    public void Dispose()
    {
        _craftingVm.SetCardOnCraftingPanel -= SetCardOnCraftingPanel;
        _craftingVm.SetMaterialsOnCraftPanel -= InitMaterialsOnCraftPanel;
        _craftingVm.InitCraftingPanel -= InitCraftingPanel;
    }
}