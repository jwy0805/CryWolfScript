using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetKits.ParticleImage;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_RewardSelectPopup : UI_Popup
{
    private ICardFactory _cardFactory;
    private MainLobbyViewModel _lobbyVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();

    private GameObject _rewardPanel;
    private GameObject _selectEffect;
    private bool _isSelected = false;
    
    public ProductInfo ProductInfo { get; set; }
    public List<CompositionInfo> CompositionInfos { get; set; } = new();

    private enum Images
    {
        RewardPanel,
        OpenedPanel,
    }

    private enum Texts
    {
        RewardSelectText,
        OpenedText,
    }
    
    [Inject]
    public void Construct(ICardFactory cardFactory, MainLobbyViewModel lobbyViewModel)
    {
        _cardFactory = cardFactory;
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

        _rewardPanel = GetImage((int)Images.RewardPanel).gameObject;
        _selectEffect = await Managers.Resource.Instantiate("UIEffects/ButtonSelectEffect", _rewardPanel.transform);
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        
    }
    
    protected override async Task InitUIAsync()
    {
        if (ProductInfo == null || CompositionInfos == null) return;
        
        var openedPanel = GetImage((int)Images.OpenedPanel);
        var rewardPanel = GetImage((int)Images.RewardPanel);
        var path = $"UI/Shop/NormalizedProducts/{(ProductId)ProductInfo.ProductId}";
        var openedCard = await Managers.Resource.Instantiate(path, openedPanel.transform);
        var openedCardRect = openedCard.GetComponent<RectTransform>();
        openedCardRect.sizeDelta = new Vector2(400, 400);
        openedCardRect.anchorMin = new Vector2(0.5f, 0.5f);
        openedCardRect.anchorMax = new Vector2(0.5f, 0.5f);
        _selectEffect.SetActive(false);
        
        foreach (var compositionInfo in CompositionInfos)
        {
            var card = new GameObject();
            switch (compositionInfo.ProductType)
            {
                case ProductType.Unit:
                    if (Managers.Data.UnitInfoDict.TryGetValue(compositionInfo.CompositionId, out var unitInfo))
                    {
                        card = await _cardFactory.GetCardResources<UnitId>(unitInfo, rewardPanel.transform);
                    }
                    break;
                case ProductType.Enchant:
                    if (Managers.Data.EnchantInfoDict.TryGetValue(compositionInfo.CompositionId, out var enchantInfo))
                    {
                        card = await _cardFactory.GetCardResources<EnchantId>(enchantInfo, rewardPanel.transform);   
                    }
                    break;
                case ProductType.Sheep:
                    if (Managers.Data.SheepInfoDict.TryGetValue(compositionInfo.CompositionId, out var sheepInfo))
                    {
                        card = await _cardFactory.GetCardResources<SheepId>(sheepInfo, rewardPanel.transform);
                    }
                    break;
                case ProductType.Character:
                    if (Managers.Data.CharacterInfoDict.TryGetValue(compositionInfo.CompositionId, out var characterInfo))
                    {
                        card = await _cardFactory.GetCardResources<CharacterId>(characterInfo, rewardPanel.transform);
                    }
                    break;
                default: return;
            }

            card.BindEvent(OnCardSelected);
        }
    }

    private async Task OnCardSelected(PointerEventData data)
    {
        if (_isSelected) return;
        _isSelected = true;
        
        var card = data.pointerPress.GetComponentInParent<Card>();
        if (card == null) return;

        var rt = _selectEffect.GetComponent<RectTransform>();

        rt.SetParent(card.transform, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        _selectEffect.SetActive(true);
        _selectEffect.GetComponent<ParticleImage>().Play();
        _rewardPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        
        await Task.Delay(500);
        
        var compositionId = card.Id;
        var compositionInfo = CompositionInfos.FirstOrDefault(ci => ci.CompositionId == compositionId);
        if (compositionInfo != null)
        {
            await _lobbyVm.SelectProduct(compositionInfo);
        }
    }
}
