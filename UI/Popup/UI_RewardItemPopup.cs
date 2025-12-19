using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_RewardItemPopup : UI_Popup
{
    private MainLobbyViewModel _lobbyVm;
    private CollectionViewModel _collectionVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private GameObject _itemScrollView;
    private readonly Queue<CompositionInfo> _largeProducts = new();
    private readonly Queue<CompositionInfo> _smallProducts = new();
    
    public List<CompositionInfo> CompositionInfos { get; set; } = new();
    
    private enum Images
    {
        Dimed,
        ItemScrollView
    }

    private enum Texts
    {
        NewItemText,
        TapToContinueText,
    }

    [Inject]
    public void Construct(MainLobbyViewModel lobbyVm, CollectionViewModel collectionVm)
    {
        _lobbyVm = lobbyVm;
        _collectionVm = collectionVm;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
            
            await BindObjectsAsync();
            InitButtonEvents();
            await InitUIAsync();
            await ResetCollectionAndUserInfo();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    protected override async Task BindObjectsAsync()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Image>(typeof(Images));
        
        _itemScrollView = GetImage((int)Images.ItemScrollView).gameObject;
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetText((int)Texts.TapToContinueText).gameObject.BindEvent(ClosePopup);
    }

    protected override async Task InitUIAsync()
    {
        var itemRect = _itemScrollView.GetComponent<RectTransform>();
        BindProductsIntoQueue();
        
        while (_largeProducts.Count > 0)
        {
            await CreateRow(itemRect, _largeProducts, 3);
        }

        while (_smallProducts.Count > 0)
        {
            await CreateRow(itemRect, _smallProducts, 4);
        }
    }

    private void BindProductsIntoQueue()
    {
        foreach (var info in CompositionInfos)
        {
            switch (info.ProductType)
            {
                case ProductType.Unit:
                case ProductType.Enchant:
                case ProductType.Sheep:
                case ProductType.Character:
                    _largeProducts.Enqueue(info);
                    break;
                case ProductType.Material:
                case ProductType.Gold:
                case ProductType.Spinel:
                case ProductType.Container:
                    _smallProducts.Enqueue(info);
                    break;
            }
        }
    }
    
    private async Task CreateRow(RectTransform parent, Queue<CompositionInfo> src, int maxPerRow)
    {
        var rowObject = new GameObject("Row", 
            typeof(RectTransform), 
            typeof(HorizontalLayoutGroup), 
            typeof(ContentSizeFitter),
            typeof(LayoutElement));
        var rowRect = rowObject.GetComponent<RectTransform>();
        var content = Util.FindChild(parent.gameObject, "Content", true);
        rowRect.SetParent(content.transform, false);
        
        var hLayout = rowObject.GetComponent<HorizontalLayoutGroup>();
        hLayout.childAlignment = TextAnchor.MiddleCenter;
        hLayout.childForceExpandWidth = false;
        hLayout.childForceExpandHeight = false;
        hLayout.spacing = 10f;
        
        var fitter = rowObject.GetComponent<ContentSizeFitter>();
        fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        for (var i = 0; i < maxPerRow && src.Count > 0; i++)
        {
            if (src.TryDequeue(out var info) == false) continue;
            await BindItemUI(rowRect.transform, info);            
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }
    
    private async Task BindItemUI(Transform parent, CompositionInfo info)
    {
        GameObject cardObject = null;
        switch (info.ProductType)
        {
            case ProductType.Unit:
                if (Managers.Data.UnitInfoDict.TryGetValue(info.CompositionId, out var unitInfo))
                {
                    cardObject = await Managers.Resource.GetCardResources<UnitId>(unitInfo, parent);
                }
                break;
            case ProductType.Enchant:
                if (Managers.Data.EnchantInfoDict.TryGetValue(info.CompositionId, out var enchantInfo))
                {
                    cardObject =await Managers.Resource.GetCardResources<EnchantId>(enchantInfo, parent);   
                }
                break;
            case ProductType.Sheep:
                if (Managers.Data.SheepInfoDict.TryGetValue(info.CompositionId, out var sheepInfo))
                {
                    cardObject =await Managers.Resource.GetCardResources<SheepId>(sheepInfo, parent);
                }
                break;
            case ProductType.Character:
                if (Managers.Data.CharacterInfoDict.TryGetValue(info.CompositionId, out var characterInfo))
                {
                    cardObject =await Managers.Resource.GetCardResources<CharacterId>(characterInfo, parent);
                }
                break;
            case ProductType.Material:
                if (Managers.Data.MaterialInfoDict.TryGetValue(info.CompositionId, out var materialInfo))
                {
                    cardObject = await Managers.Resource.GetMaterialResources(materialInfo, parent);
                    var countText = Util.FindChild(cardObject, "CountText", true);
                    countText.GetComponent<TextMeshProUGUI>().text = info.Count.ToString();
                }
                break;
            case ProductType.Gold:
                cardObject = await Managers.Resource.GetItemFrameGold(info.Count, parent);
                break;
            case ProductType.Spinel:
                cardObject = await Managers.Resource.GetItemFrameSpinel(info.Count, parent);
                break;
            case ProductType.Container:
            default:
                var path = $"UI/Shop/NormalizedProducts/{(ProductId)info.CompositionId}";
                cardObject = await Managers.Resource.Instantiate(path, parent);
                var go = Util.FindChild(cardObject, "TextNum", true, true);
                if (go != null)
                {
                    var countText = go.GetComponent<TextMeshProUGUI>();
                    countText.text = info.Count.ToString();
                }
                break;
        }

        var parentLayoutElement = parent.GetComponent<LayoutElement>();
        parentLayoutElement.preferredWidth = 1000;
        
        if (cardObject != null)
        {
            var layoutElement = cardObject.GetOrAddComponent<LayoutElement>();
            
            switch (info.ProductType)
            {
                case ProductType.Unit:
                case ProductType.Enchant:
                case ProductType.Sheep:
                case ProductType.Character:
                    layoutElement.preferredWidth = 250;
                    layoutElement.preferredHeight = 400;
                    parentLayoutElement.preferredHeight = 400;
                    break;
                case ProductType.Material:
                case ProductType.Gold:
                case ProductType.Spinel:
                    layoutElement.preferredWidth = 200;
                    layoutElement.preferredHeight = 200;
                    parentLayoutElement.preferredHeight = 200;
                    break;
                case ProductType.Container:
                    layoutElement.preferredWidth = 200;
                    layoutElement.preferredHeight = 200;
                    parentLayoutElement.preferredHeight = 200;
                    cardObject.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 200);
                    break;
            }
        }
    }

    private async Task ResetCollectionAndUserInfo()
    {
        await _collectionVm.LoadCollection();
        await _lobbyVm.InitUserInfo();
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.ClosePopupUI();
    }
}
