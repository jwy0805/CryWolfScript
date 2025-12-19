using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LobbyCollectionWidget
{
    private readonly CollectionViewModel _collectionVm;
    private readonly LobbyUtilWidget _utilWidget;
    private readonly Action<PointerEventData> _onCardClicked;
    private readonly Action<Define.SelectMode> _setSelectMode;
    
    private Transform _unitCollection;
    private Transform _unitNoCollection;
    private Transform _assetCollection;
    private Transform _assetNoCollection;
    private Transform _characterCollection;
    private Transform _characterNoCollection;
    private Transform _materialCollection;
    private RectTransform _verticalContent;
    private RectTransform _craftingPanel;
    private GameObject _collectionScrollView;
    private GameObject _arrangeAllButton;
    private Dictionary<string, GameObject> _collectionUiDict;
    
    public Define.ArrangeMode ArrangeMode { private get; set; }
    
    public LobbyCollectionWidget(
        CollectionViewModel collectionVm,
        LobbyUtilWidget utilWidget,
        Action<PointerEventData> onCardClicked,
        Action<Define.SelectMode> setSelectMode)
    {
        _collectionVm = collectionVm;
        _utilWidget = utilWidget;
        _onCardClicked = onCardClicked;
        _setSelectMode = setSelectMode;
        
        BindEvents();
    }

    public void BindViews(
        Transform unitCollection,
        Transform unitNoCollection,
        Transform assetCollection,
        Transform assetNoCollection,
        Transform characterCollection,
        Transform characterNoCollection,
        Transform materialCollection,
        RectTransform craftingPanel,
        GameObject collectionScrollView,
        GameObject arrangeAllButton,
        Dictionary<string, GameObject> collectionUiDict)
    {
        _unitCollection = unitCollection;
        _unitNoCollection = unitNoCollection;
        _assetCollection = assetCollection;
        _assetNoCollection = assetNoCollection;
        _characterCollection = characterCollection;
        _characterNoCollection = characterNoCollection;
        _materialCollection = materialCollection;
        _verticalContent = _unitCollection.GetComponent<RectTransform>();
        _craftingPanel = craftingPanel;
        _collectionScrollView = collectionScrollView;
        _arrangeAllButton = arrangeAllButton;
        _collectionUiDict = collectionUiDict;
        
        ReleaseEvents();
        BindEvents();
    }

    private void BindEvents()
    {
        _collectionVm.OnCardInitialized += SetCollectionUI;
        _collectionVm.OnCardSwitched += SetCollectionUI;
    }

    private void ReleaseEvents()
    {
        _collectionVm.OnCardInitialized -= SetCollectionUI;
        _collectionVm.OnCardSwitched -= SetCollectionUI;
    }

    public async Task InitCollection()
    {
        await _collectionVm.Initialize();
        
        InitCollectionTabUI();
    }

    private void InitCollectionTabUI()
    {
        _utilWidget.FocusTabButton("DeckTabButton");
        _craftingPanel.sizeDelta = new Vector2(_craftingPanel.sizeDelta.x, 0);
        _collectionScrollView.SetActive(false);
        _arrangeAllButton.GetComponentInChildren<Image>().color = Color.cyan;
    }
    
    public async Task SetCollectionUI(Faction faction)
    {
        try
        {
            await SetCollectionUIDetails(faction);
            _setSelectMode?.Invoke(Define.SelectMode.Normal);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    public async Task SetCollectionUIDetails(Faction faction)
    {
        var user = User.Instance;
        var ownedUnits = _utilWidget.OrderOwnedUnits(user.OwnedUnitList, ArrangeMode);
        var ownedSheep = _utilWidget.OrderOwnedSheep(user.OwnedSheepList, ArrangeMode);
        var ownedEnchants = _utilWidget.OrderOwnedEnchants(user.OwnedEnchantList, ArrangeMode);
        var ownedCharacters = _utilWidget.OrderOwnedCharacters(user.OwnedCharacterList, ArrangeMode);
        var ownedMaterials = _utilWidget.OrderOwnedMaterials(user.OwnedMaterialList, ArrangeMode);
        var notOwnedUnits = _utilWidget.OrderAssetList(user.NotOwnedUnitList
            .FindAll(info => info.Faction == faction), ArrangeMode);
        var notOwnedSheep = _utilWidget.OrderAssetList(user.NotOwnedSheepList, ArrangeMode);
        var notOwnedEnchants = _utilWidget.OrderAssetList(user.NotOwnedEnchantList, ArrangeMode);
        var notOwnedCharacters = _utilWidget.OrderAssetList(user.NotOwnedCharacterList, ArrangeMode);
        
        _utilWidget.SetActivePanels(_collectionUiDict, _collectionUiDict.Keys.ToArray());
        Util.DestroyAllChildren(_unitCollection);
        Util.DestroyAllChildren(_unitNoCollection);
        Util.DestroyAllChildren(_assetCollection);
        Util.DestroyAllChildren(_assetNoCollection);
        Util.DestroyAllChildren(_characterCollection);
        Util.DestroyAllChildren(_characterNoCollection);
        Util.DestroyAllChildren(_materialCollection);
        
        // Units
        Debug.Log($"{ownedUnits.Count} owned units, {notOwnedUnits.Count} not owned units");
        foreach (var unit in ownedUnits)
        {
            var cardFrame = await Managers.Resource.GetCardResources<UnitId>(
                unit.UnitInfo, _unitCollection, _onCardClicked);
            _utilWidget.GetCountText(cardFrame.transform, unit.Count);
        }

        foreach (var unit in notOwnedUnits)
        {
            var cardFrame = await Managers.Resource.GetCardResources<UnitId>(
                unit, _unitNoCollection, _onCardClicked);
            var cardUnit = Util.FindChild(cardFrame, "CardUnit", true);
            var path = $"Sprites/Portrait/{((UnitId)unit.Id).ToString()}_gray";
            cardUnit.GetComponent<Image>().sprite =
                await Managers.Resource.LoadAsync<Sprite>(path);
        }

        // Sheep / Enchant
        if (faction == Faction.Sheep)
        {
            foreach (var sheep in ownedSheep)
            {
                var cardFrame = await Managers.Resource.GetCardResources<SheepId>(
                    sheep.SheepInfo, _assetCollection, _onCardClicked);
                _utilWidget.GetCountText(cardFrame.transform, sheep.Count);
            }

            foreach (var sheep in notOwnedSheep)
            {
                var cardFrame = await Managers.Resource.GetCardResources<SheepId>(
                    sheep, _assetNoCollection, _onCardClicked);
                var cardUnit = Util.FindChild(cardFrame, "CardUnit", true);
                var path = $"Sprites/Portrait/{((SheepId)sheep.Id).ToString()}_gray";
                cardUnit.GetComponent<Image>().sprite =
                    await Managers.Resource.LoadAsync<Sprite>(path);
            }
        }
        else
        {
            foreach (var enchant in ownedEnchants)
            {
                var cardFrame = await Managers.Resource.GetCardResources<EnchantId>(
                    enchant.EnchantInfo, _assetCollection, _onCardClicked);
                _utilWidget.GetCountText(cardFrame.transform, enchant.Count);
            }

            foreach (var enchant in notOwnedEnchants)
            {
                var cardFrame = await Managers.Resource.GetCardResources<EnchantId>(
                    enchant, _assetNoCollection, _onCardClicked);
                var cardUnit = Util.FindChild(cardFrame, "CardUnit", true);
                var path = $"Sprites/Portrait/{((EnchantId)enchant.Id).ToString()}_gray";
                cardUnit.GetComponent<Image>().sprite =
                    await Managers.Resource.LoadAsync<Sprite>(path);
            }
        }

        // Characters
        foreach (var character in ownedCharacters)
        {
            var cardFrame = await Managers.Resource.GetCardResources<CharacterId>(
                character.CharacterInfo, _characterCollection, _onCardClicked);
            _utilWidget.GetCountText(cardFrame.transform, character.Count);
        }

        foreach (var character in notOwnedCharacters)
        {
            var cardFrame = await Managers.Resource.GetCardResources<CharacterId>(
                character, _characterNoCollection, _onCardClicked);
            var cardUnit = Util.FindChild(cardFrame, "CardUnit", true);
            var path = $"Sprites/Portrait/{((CharacterId)character.Id).ToString()}_gray";
            cardUnit.GetComponent<Image>().sprite =
                await Managers.Resource.LoadAsync<Sprite>(path);
        }

        // Materials
        foreach (var material in ownedMaterials)
        {
            var cardFrame =
                await Managers.Resource.GetMaterialResources(material.MaterialInfo, _materialCollection);
            _utilWidget.GetCountText(cardFrame.transform, material.Count);
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(_verticalContent);
    }
    
    public void Dispose()
    {
        ReleaseEvents();
    }
}
