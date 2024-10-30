using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/* Last Modified : 24. 10. 18
 * Version : 1.016
 */

// This class includes the collection logics for the main lobby UI. 
public partial class UI_MainLobby
{
    private async Task InitCollection()
    {
        _unitCollection = GetImage((int)Images.UnitHoldingCardPanel).transform;
        _unitNoCollection = GetImage((int)Images.UnitNotHoldingCardPanel).transform;
        _assetCollection = GetImage((int)Images.AssetHoldingCardPanel).transform;
        _assetNoCollection = GetImage((int)Images.AssetNotHoldingCardPanel).transform;
        _characterCollection = GetImage((int)Images.CharacterHoldingCardPanel).transform;
        _characterNoCollection = GetImage((int)Images.CharacterNotHoldingCardPanel).transform;
        _materialCollection = GetImage((int)Images.MaterialHoldingPanel).transform;
        
        for (var i = 1; i <= _deckButtonDict.Count; i++)
        {
            _deckButtonDict[$"DeckButton{i}"].GetComponent<DeckButtonInfo>().DeckIndex = i;
            _lobbyDeckButtonDict[$"LobbyDeckButton{i}"].GetComponent<DeckButtonInfo>().DeckIndex = i;
        }

        await Task.WhenAll(_deckVm.Initialize(), _collectionVm.Initialize());
        Debug.Log("Set Cards");
        InitCollectionUI();
    }
    
    private void SetDeckUI(Faction faction)
    {
        // Set Deck - As using layout group, no need to set position
        var deck = _deckVm.GetDeck(faction);
        var deckImage = GetImage((int)Images.Deck);
        var lobbyDeckImage = GetImage((int)Images.LobbyDeck);
        var deckParent = Util.FindChild(gameObject, deckImage.name, true, true).transform;
        var lobbyDeckParent = Util.FindChild(gameObject, lobbyDeckImage.name, true, true).transform;
        
        foreach (var unit in deck.UnitsOnDeck)
        {
            Util.GetCardResources<UnitId>(unit, deckParent, OnCardClicked);
            Util.GetCardResources<UnitId>(unit, lobbyDeckParent);
        }

        var assetParent = GetImage((int)Images.BattleSettingPanel).transform;
        Util.DestroyAllChildren(assetParent);
        
        // Set Asset Frame
        IAsset asset = faction == Faction.Sheep ? User.Instance.BattleSetting.SheepInfo : User.Instance.BattleSetting.EnchantInfo;
        var assetFrame = faction == Faction.Sheep ?
            Util.GetCardResources<SheepId>(asset, assetParent, OnCardClicked):
            Util.GetCardResources<EnchantId>(asset, assetParent, OnCardClicked);
        
        // Set Character Frame
        var character = User.Instance.BattleSetting.CharacterInfo;
        Util.GetCardResources<CharacterId>(character, assetParent, OnCardClicked);
    }
    
    private void SetCollectionUI(Faction faction)
    {
        SetCollectionUIDetails(faction);
        SelectMode = SelectModeEnums.Normal;
    }

    private void SetCollectionUIDetails(Faction faction)
    {
        var ownedUnits = OrderOwnedUnits();
        var ownedSheep = OrderOwnedSheep();
        var ownedEnchants = OrderOwnedEnchants();
        var ownedCharacters = OrderOwnedCharacters();
        var ownedMaterials = OrderOwnedMaterials();
        var notOwnedUnits = OrderAssetList(User.Instance.NotOwnedUnitList.Where(info =>
            info.Faction == faction).ToList());
        var notOwnedSheep = OrderAssetList( User.Instance.NotOwnedSheepList);
        var notOwnedEnchants = OrderAssetList( User.Instance.NotOwnedEnchantList);
        var notOwnedCharacters = OrderAssetList( User.Instance.NotOwnedCharacterList);
        
        SetActivePanels(_collectionUiDict, _collectionUiDict.Keys.ToArray());
        Util.DestroyAllChildren(_unitCollection);
        Util.DestroyAllChildren(_unitNoCollection);
        Util.DestroyAllChildren(_assetCollection);
        Util.DestroyAllChildren(_assetNoCollection);
        Util.DestroyAllChildren(_characterCollection);
        Util.DestroyAllChildren(_characterNoCollection);
        Util.DestroyAllChildren(_materialCollection);

        // Units in collection UI
        foreach (var unit in ownedUnits)
        {
            var cardFrame = 
                Util.GetCardResources<UnitId>(unit.UnitInfo, _unitCollection, OnCardClicked);
            GetCountText(cardFrame.transform, unit.Count);
        }

        foreach (var unit in notOwnedUnits)
        {
            var cardFrame = Util.GetCardResources<UnitId>(unit, _unitNoCollection, OnCardClicked);
            var cardUnit = Util.FindChild(cardFrame, "CardUnit", true);
            var path = $"Sprites/Portrait/{((UnitId)unit.Id).ToString()}_gray";
            cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        }

        // Assets in collection UI
        if (faction == Faction.Sheep)
        {
            foreach (var sheep in ownedSheep)
            {
                var cardFrame = 
                    Util.GetCardResources<SheepId>(sheep.SheepInfo, _assetCollection, OnCardClicked);
                GetCountText(cardFrame.transform, sheep.Count);
            }

            foreach (var sheep in notOwnedSheep)
            {
                var cardFrame = 
                    Util.GetCardResources<SheepId>(sheep, _assetNoCollection, OnCardClicked);
                var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
                var path = $"Sprites/Portrait/{((SheepId)sheep.Id).ToString()}_gray";
                cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
            }
        }
        else
        {
            foreach (var enchant in ownedEnchants)
            {
                var cardFrame = 
                    Util.GetCardResources<EnchantId>(enchant.EnchantInfo, _assetCollection, OnCardClicked);
                GetCountText(cardFrame.transform, enchant.Count);
            }

            foreach (var enchant in notOwnedEnchants)     
            {
                var cardFrame = 
                    Util.GetCardResources<EnchantId>(enchant, _assetNoCollection, OnCardClicked);
                var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
                var path = $"Sprites/Portrait/{((EnchantId)enchant.Id).ToString()}_gray";
                cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
            }
        }
        
        // Characters in collection UI
        foreach (var character in ownedCharacters)
        {
            var cardFrame = 
                Util.GetCardResources<CharacterId>(character.CharacterInfo, _characterCollection, OnCardClicked);
            GetCountText(cardFrame.transform, character.Count);
        }
        
        foreach (var character in notOwnedCharacters)
        {
            var cardFrame = 
                Util.GetCardResources<CharacterId>(character, _characterNoCollection, OnCardClicked);
            var cardUnit = cardFrame.transform.Find("CardUnit").gameObject;
            var path = $"Sprites/Portrait/{((CharacterId)character.Id).ToString()}_gray";
            cardUnit.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        }
        
        // Materials in collection UI
        foreach (var material in ownedMaterials)
        {
            var cardFrame = Util.GetMaterialResources(material.MaterialInfo, _materialCollection);
            GetCountText(cardFrame.transform, material.Count);
        }
    }
    
     private void SetDeckButtonUI(Faction faction)
    {
        var deckButtons = _deckButtonDict.Values.ToList();
        var lobbyDeckButtons = _lobbyDeckButtonDict.Values.ToList();
        var deckNumber = faction == Faction.Sheep 
            ? User.Instance.DeckSheep.DeckNumber 
            : User.Instance.DeckWolf.DeckNumber;
        
        for (var i = 0; i < deckButtons.Count; i++)
        {
            deckButtons[i].GetComponent<DeckButtonInfo>().IsSelected = false;
            lobbyDeckButtons[i].GetComponent<DeckButtonInfo>().IsSelected = false;
        }
        
        _deckButtonDict[$"DeckButton{deckNumber}"].GetComponent<DeckButtonInfo>().IsSelected = true;
        _lobbyDeckButtonDict[$"LobbyDeckButton{deckNumber}"].GetComponent<DeckButtonInfo>().IsSelected = true;
    }
    
    private void InitCollectionUI()
    {
        FocusTabButton("DeckTabButton");
        _craftingPanel = GetImage((int)Images.CraftingPanel).GetComponent<RectTransform>();
        _craftingPanel.sizeDelta = new Vector2(_craftingPanel.sizeDelta.x, 0);
        var obj = GetImage((int)Images.CollectionScrollView);
        obj.gameObject.SetActive(false);
        GetButton((int)Buttons.ArrangeAllButton).GetComponentInChildren<Image>().color = Color.green;
    }

    private void SetCardPopupUI(Card card)
    {
        _selectedCard = card;
        CardPopup = Managers.UI.ShowPopupUI<UI_CardClickPopup>();
        CardPopup.SelectedCard = _selectedCard;
        CardPopup.CardPosition = _selectedCard.transform.position - new Vector3(0, 60);
        CardPopup.FromDeck = card.gameObject.transform.parent == GetImage((int)Images.Deck).transform 
                             || card.gameObject.transform.parent == GetImage((int)Images.BattleSettingPanel).transform;
        
        // TODO: Set Size By Various Resolution
    }

    private void FocusTabButton(string tabButtonName)
    {
        foreach (var go in _tabButtonDict.Values)
        {
            var tabFocus = Util.FindChild(go, "TabFocus", true, true);
            var buttonText = go.GetComponent<TextMeshProUGUI>();
            
            if (go.name == tabButtonName)
            {
                tabFocus.SetActive(true);
                buttonText.color = Color.white;
            }
            else
            {
                tabFocus.SetActive(false);
                buttonText.color = new Color(74/255f, 172/255f, 247/255f);
            }
        }
    }
    
    private void ResetDeckUI(Faction faction)
    {
        var deckParent = GetImage((int)Images.Deck).transform;
        var lobbyDeckParent = GetImage((int)Images.LobbyDeck).transform;
        Util.DestroyAllChildren(deckParent);
        Util.DestroyAllChildren(lobbyDeckParent);
        SetDeckUI(faction);
    }
    
    private void SwitchCollection(Faction faction) 
    {
        SetCollectionUI(faction);
    }
    
    private void GetCountText(Transform parent, int count)
    {
        var countText = Util.FindChild(parent.gameObject, "CountText", true);
        countText.GetComponent<TextMeshProUGUI>().text = count.ToString();
    }

    private void GetInDeckText(Transform parent, float rate = 0.7f)
    {
        var inDeckTextPanel = Managers.Resource.Instantiate("UI/Deck/DeckMarkPanel", parent);
        var gridLayout = parent.transform.parent.GetComponent<GridLayoutGroup>();
        var inDeckTextRect = inDeckTextPanel.GetComponent<RectTransform>();
        inDeckTextRect.sizeDelta = new Vector2(gridLayout.cellSize.x * rate, inDeckTextRect.sizeDelta.y);
        inDeckTextRect.anchorMin = new Vector2(0.5f, 0.9f);
        inDeckTextRect.anchorMax = new Vector2(0.5f, 0.9f);
    }

    private void GoToCraftingTab()
    {
        Managers.UI.CloseAllPopupUI();
        FocusTabButton("CraftingTabButton");
        GetImage((int)Images.DeckScrollView).gameObject.SetActive(false);
        GetImage((int)Images.CollectionScrollView).gameObject.SetActive(true);
        GetImage((int)Images.CraftingSelectPanel).gameObject.SetActive(true);

        OpenCraftingPanel();
        _craftingScrollRect.verticalNormalizedPosition = 1f;
    }
    
    private void OpenCraftingPanel()
    {
        if (_isCraftingPanelOpen) return;
        
        InitCraftingPanel();
        StartCoroutine(AdjustCraftingPanel(_craftingVm.CraftingPanelHeight));
    }
    
    private void CloseCraftingPanel()
    {
        _craftingPanel.sizeDelta = new Vector2(_craftingPanel.sizeDelta.x, 0);
        _craftingPanel.gameObject.SetActive(false);
        SelectMode = SelectModeEnums.Normal;
    }
    
    private IEnumerator AdjustCraftingPanel(float targetHeight)
    {
        _isCraftingPanelOpen = true;
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
        _isCraftingPanelOpen = false;
    }
    
    private void SetCardOnCraftingPanel(Card card)
    {
        GoToCraftingTab();
        LoadCraftingCard(card);
    }

    private void LoadCraftingCard(Card card)
    {
        var parent = GetImage((int)Images.CraftingCardPanel).transform;
        Util.DestroyAllChildren(parent);
        var cardFrame = card.AssetType switch
        {
            Asset.Unit => Util.GetCardResources<UnitId>(card, parent),
            Asset.Sheep => Util.GetCardResources<SheepId>(card, parent),
            Asset.Enchant => Util.GetCardResources<EnchantId>(card, parent),
            Asset.Character => Util.GetCardResources<CharacterId>(card, parent),
            _ => null
        };
        
        if (cardFrame == null) return;
        var cardFrameRect = cardFrame.GetComponent<RectTransform>();
        cardFrameRect.sizeDelta = new Vector2(250, 400);
    }
    
    private void InitCraftingPanel()
    {
        var cardPanel = GetImage((int)Images.CraftingCardPanel).transform;
        Util.DestroyAllChildren(cardPanel);
        
        SelectMode = SelectModeEnums.Normal;
        _craftingPanel.gameObject.SetActive(true);
        SetActivePanels(_craftingUiDict, new[] { "CraftingSelectPanel" });

        if (_selectedCard == null) return;
        _selectedCardForCrafting = _selectedCard;
        var unitLevel = _collectionVm.GetLevelFromUiObject((UnitId)_selectedCard.Id);
        GetButton((int)Buttons.ReinforcingButton).interactable = unitLevel != 3;
    }
    
    private void InitCraftPanel()
    {
        var unitLevel = _collectionVm.GetLevelFromUiObject((UnitId)_selectedCard.Id);
        var unitId = unitLevel switch
        {
            1 => (UnitId)_selectedCardForCrafting.Id,
            2 => (UnitId)_selectedCardForCrafting.Id - 1,
            _ => (UnitId)_selectedCardForCrafting.Id - 2
        };

        var parent = GetImage((int)Images.CraftCardPanel);
        Util.DestroyAllChildren(parent.transform);
        
        _ = _selectedCardForCrafting.AssetType switch
        {
            Asset.Unit => Util.GetCardResources<UnitId>(_selectedCardForCrafting, parent.transform),
            Asset.Sheep => Util.GetCardResources<SheepId>(_selectedCardForCrafting, parent.transform),
            Asset.Enchant => Util.GetCardResources<EnchantId>(_selectedCardForCrafting, parent.transform),
            _ => null
        };    
        
        _craftingVm.LoadMaterials(unitId);
        _craftingVm.CraftingCount = 1;
        _craftingVm.TotalCraftingMaterials = _craftingVm.CraftingMaterials;
        GetText((int)Texts.CraftCountText).text = _craftingVm.CraftingCount.ToString();
    }

    private void InitMaterialsOnCraftPanel(List<OwnedMaterialInfo> craftingMaterials, List<OwnedMaterialInfo> ownedMaterials)
    {
        var parent = GetImage((int)Images.MaterialPanel).transform;
        Util.DestroyAllChildren(parent);

        foreach (var material in craftingMaterials)
        {
            var craftingFrame = Managers.Resource.Instantiate("UI/Deck/CraftingMaterialFrame", parent);
            var itemPanel =  Util.GetMaterialResources(material.MaterialInfo, craftingFrame.transform);
            var itemPanelRect = itemPanel.GetComponent<RectTransform>();
            // var itemFrame = Util.FindChild(itemPanel, "Frame", true);
            var needText = Util.FindChild(craftingFrame, "NeedCountText", true);
            var ownedText = Util.FindChild(craftingFrame, "OwnedCountText", true);
            var nameText = Util.FindChild(craftingFrame, "MaterialNameText", true);
            var materialName = ((MaterialId)material.MaterialInfo.Id).ToString();
            if (materialName.Length > 11)
            {
                materialName = materialName.Substring(0, 9) + "..";
            }
            
            itemPanelRect.anchorMin = new Vector2(0.5f, 0.5f);
            itemPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
            itemPanelRect.sizeDelta = new Vector2(125, 125);
            
            var ownedCount = ownedMaterials.FirstOrDefault(info =>
                info.MaterialInfo.Id == material.MaterialInfo.Id)?.Count ?? 0;
            needText.GetComponent<TextMeshProUGUI>().text = material.Count.ToString();
            ownedText.GetComponent<TextMeshProUGUI>().text = ownedCount.ToString();
            nameText.GetComponent<TextMeshProUGUI>().text = materialName;

            CheckAndSetColorOnMaterialFrame(material, craftingFrame);
        }
    }
    
    private void CheckAndSetColorOnMaterialFrame(OwnedMaterialInfo material, GameObject craftingFrame)
    {
        var ownedCount = User.Instance.OwnedMaterialList
            .FirstOrDefault(info => info.MaterialInfo.Id == material.MaterialInfo.Id)?.Count ?? 0;
        var needText = Util.FindChild(craftingFrame, "NeedCountText", true);
        var slash = Util.FindChild(craftingFrame, "Slash", true);
        var ownedText = Util.FindChild(craftingFrame, "OwnedCountText", true);
        needText.GetComponent<TextMeshProUGUI>().color = material.Count <= ownedCount ? Color.white : Color.red;
        slash.GetComponent<TextMeshProUGUI>().color = material.Count <= ownedCount ? Color.white : Color.red;
        ownedText.GetComponent<TextMeshProUGUI>().color = material.Count <= ownedCount ? Color.white : Color.red;
    }
    
    private void UpdateCraftingMaterials()
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
        GetText((int)Texts.CraftCountText).text = _craftingVm.CraftingCount.ToString();
        
        var materialPanel = GetImage((int)Images.MaterialPanel).transform;
        var childCount = materialPanel.childCount;
        for (var i = 0; i < childCount; i++)
        {
            var materialFrame = materialPanel.GetChild(i);
            var itemFrame = Util.FindChild(materialFrame.gameObject, "Frame", true);
            var needText = Util.FindChild(materialFrame.gameObject, "NeedCountText", true);
            var material = _craftingVm.TotalCraftingMaterials
                .FirstOrDefault(info => info.MaterialInfo.Id == materialFrame.GetComponentInChildren<MaterialItem>().Id);
            if (material == null) continue;
            needText.GetComponent<TextMeshProUGUI>().text = material.Count.ToString();
            CheckAndSetColorOnMaterialFrame(material, itemFrame);
        }
    }
    
    private void InitReinforcePanel()
    {
        var cardParent = GetImage((int)Images.ReinforceCardPanel).transform;
        var resultParent = GetImage((int)Images.ReinforceResultPanel).transform;
        
        Util.DestroyAllChildren(cardParent);
        Util.DestroyAllChildren(resultParent);

        var selectedUnit = Managers.Data.UnitInfoDict[_selectedCardForCrafting.Id];
        var resultUnit = Managers.Data.UnitInfoDict[_selectedCardForCrafting.Id + 1];
        var cardFrame = Util.GetCardResources<UnitId>(selectedUnit, cardParent);
        var resultFrame = Util.GetCardResources<UnitId>(resultUnit, resultParent);
        var cardFrameRect = cardFrame.GetComponent<RectTransform>();
        var resultFrameRect = resultFrame.GetComponent<RectTransform>();
        var cardNumberText = GetText((int)Texts.ReinforceCardNumberText);
        var rateText = GetText((int)Texts.SuccessRateText);
        
        cardFrameRect.anchorMin = new Vector2(0.5f, 0.5f);
        cardFrameRect.anchorMax = new Vector2(0.5f, 0.5f);
        cardFrameRect.sizeDelta = new Vector2(250, 400);
        resultFrameRect.anchorMin = new Vector2(0.5f, 0.5f);
        resultFrameRect.anchorMax = new Vector2(0.5f, 0.5f);
        resultFrameRect.sizeDelta = new Vector2(250, 400);
        cardNumberText.text = "0 / 8";
        rateText.text = "-";
        
        _craftingVm.LoadMaterials((UnitId)selectedUnit.Id + 1);
        _craftingVm.AddNewUnitMaterial(selectedUnit);
        _craftingVm.SetReinforcePointNeeded(resultUnit);
        GetButton((int)Buttons.ReinforceButton).interactable = false;
        
        var rate = _craftingVm.GetSuccessRate();
        var arrowPanel = GetImage((int)Images.ArrowPanel);
        
        arrowPanel.GetComponent<ReinforceArrowController>().SetArrowColors(rate);
    }

    private void ResetCollectionUIForReinforce()
    {
        List<OwnedUnitInfo> units;
        switch (ArrangeMode)
        {
            default:
            case ArrangeModeEnums.All:
                units = User.Instance.OwnedUnitList
                    .Select(unit => new OwnedUnitInfo
                    {
                        UnitInfo = unit.UnitInfo,
                        Count = unit.Count - _craftingVm.ReinforceMaterialUnits.Count(info => info.Id == unit.UnitInfo.Id)
                    })
                    .OrderBy(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id).ToList();
                break;
            case ArrangeModeEnums.Summary:
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
            case ArrangeModeEnums.Class:
                units = User.Instance.OwnedUnitList
                    .OrderByDescending(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id)
                    .Select(unit => new OwnedUnitInfo
                    {
                        UnitInfo = unit.UnitInfo,
                        Count = unit.Count - _craftingVm.ReinforceMaterialUnits.Count(info => info.Id == unit.UnitInfo.Id)
                    }).ToList();
                break;
            case ArrangeModeEnums.Count:
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
        
        SetActivePanels(_collectionUiDict, new[] { "UnitHoldingLabelPanel", "UnitHoldingCardPanel" });
        Util.DestroyAllChildren(_unitCollection);

        foreach (var unit in units)
        {
            var cardFrame = 
                Util.GetCardResources<UnitId>(unit.UnitInfo, _unitCollection, OnCardClicked);
            GetCountText(cardFrame.transform, unit.Count);
            if (_craftingVm.IsUnitInDecks(unit.UnitInfo)) GetInDeckText(cardFrame.transform);
        }
    }

    private bool VerifyCard(UnitInfo unitInfo)
    {
        if (_craftingVm.VerityCardByCondition1(unitInfo) == false)
        {
            var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
            popup.SetWarning("이미 덱에 포함된 카드를 재료로 사용할 수 없습니다.");
            return false;
        }
            
        if (_craftingVm.VerifyCardByCondition2(unitInfo) == false)
        {
            var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
            popup.SetWarning("게임 플레이에 필요한 최소한의 카드가 남아있어야 합니다.");
            return false;
        }
            
        if (_craftingVm.VerifyCardByCondition3(unitInfo) == false)
        {
            var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
            popup.SetWarning("카드가 없습니다.");
            return false;
        }

        return true;
    }

    private void UpdateReinforcePanel()
    {
        var cardNumberText = GetText((int)Texts.ReinforceCardNumberText);
        var rateText = GetText((int)Texts.SuccessRateText);
        var arrowPanel = GetImage((int)Images.ArrowPanel).transform;
        var rate = _craftingVm.GetSuccessRate();

        GetButton((int)Buttons.ReinforceButton).interactable = true;
        arrowPanel.GetComponent<ReinforceArrowController>().SetArrowColors(rate);
        cardNumberText.text = (_craftingVm.ReinforceMaterialUnits.Count - 1) + " / 8";
        rateText.text = MathF.Min((int)(rate * 100), 100) + "%";
    } 
    
    private void InitRecyclePanel()
    {
        
    }
}
