using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_RewardOpenPopup : UI_Popup
{
    private MainLobbyViewModel _lobbyVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();

    private GameObject _rewardScrollView;
    private GameObject _openedScrollView;
    private readonly Queue<CompositionInfo> _largeProducts = new();
    private readonly Queue<CompositionInfo> _smallProducts = new();
    
    public List<RandomProductInfo> OriginalProductInfos { get; set; } = new();
    public List<CompositionInfo> CompositionInfos { get; set; } = new();
    
    private enum Images
    {
        Dimed,
        RewardScrollView,
        OpenedScrollView,
    }
    
    private enum Texts
    {
        RewardOpenText,
        OpenedText,
        TapToContinueText,
    }
    
    [Inject]
    public void Construct(MainLobbyViewModel lobbyViewModel)
    {
        _lobbyVm = lobbyViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
    
            await BindObjectsAsync();
            InitButtonEvents();
            await InitUIAsync();
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
        
        _rewardScrollView = GetImage((int)Images.RewardScrollView).gameObject;
        _openedScrollView = GetImage((int)Images.OpenedScrollView).gameObject;

        await Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetText((int)Texts.TapToContinueText).gameObject.BindEvent(ClaimOtherProducts);
    }

    protected override async Task InitUIAsync()
    {
        var rewardRect = _rewardScrollView.GetComponent<RectTransform>();
        BindProductsIntoQueue();

        while (_largeProducts.Count > 0)
        {
            await CreateRow(rewardRect, _largeProducts, 3);
        }

        while (_smallProducts.Count > 0)
        {
            await CreateRow(rewardRect, _smallProducts, 4);
        }

        await BindOpenedProducts();
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
                    cardObject = await Managers.Resource.GetCardResources<EnchantId>(enchantInfo, parent);   
                }
                break;
            case ProductType.Sheep:
                if (Managers.Data.SheepInfoDict.TryGetValue(info.CompositionId, out var sheepInfo))
                {
                    cardObject = await Managers.Resource.GetCardResources<SheepId>(sheepInfo, parent);
                }
                break;
            case ProductType.Character:
                if (Managers.Data.CharacterInfoDict.TryGetValue(info.CompositionId, out var characterInfo))
                {
                    cardObject = await Managers.Resource.GetCardResources<CharacterId>(characterInfo, parent);
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
            default: return;
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
                    layoutElement.preferredWidth = 200;
                    layoutElement.preferredHeight = 200;
                    parentLayoutElement.preferredHeight = 200;
                    break;
            }
        }
    }

    private async Task BindOpenedProducts()
    { 
        var content = Util.FindChild(_openedScrollView, "Content", true);

        foreach (var randomProductInfo in OriginalProductInfos)      
        {
            var path = $"UI/Shop/NormalizedProducts/{(ProductId)randomProductInfo.ProductInfo.ProductId}";
            var openedCard = await Managers.Resource.Instantiate(path, content.transform);
            var childRect = openedCard.transform.GetChild(0).GetComponent<RectTransform>();
            var layoutElement = openedCard.GetOrAddComponent<LayoutElement>();
            
            if (Mathf.Approximately(childRect.anchorMin.x, 0.19f))
            {
                layoutElement.preferredWidth = 320;
                layoutElement.preferredHeight = 320;
            }
            else
            {
                layoutElement.preferredWidth = 200;
                layoutElement.preferredHeight = 200;
            }
            
            var countText = Util.FindChild(openedCard, "CountText", true);
            if (countText != null)
            {
                countText.GetComponent<TextMeshProUGUI>().text = randomProductInfo.Count.ToString();
            }
        }
        
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    private async Task ClaimOtherProducts(PointerEventData data)
    {
        try
        {
            GetText((int)Texts.TapToContinueText).GetComponent<CanvasGroup>().blocksRaycasts = false;
            await _lobbyVm.ClaimFixedAndDisplay();
        }
        catch (Exception)
        {
            Managers.UI.ClosePopupUI();
        }
    }
}
