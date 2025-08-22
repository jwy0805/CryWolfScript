using UnityEngine;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AssetKits.ParticleImage;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;
using Zenject;

public class UI_RewardSelectPopup : UI_Popup
{
    private MainLobbyViewModel _lobbyVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();

    private GameObject _rewardPanel;
    private GameObject _selectEffect;
    
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
                        card = await Managers.Resource.GetCardResources<UnitId>(unitInfo, rewardPanel.transform);
                    }
                    break;
                case ProductType.Enchant:
                    if (Managers.Data.EnchantInfoDict.TryGetValue(compositionInfo.CompositionId, out var enchantInfo))
                    {
                        card = await Managers.Resource.GetCardResources<EnchantId>(enchantInfo, rewardPanel.transform);   
                    }
                    break;
                case ProductType.Sheep:
                    if (Managers.Data.SheepInfoDict.TryGetValue(compositionInfo.CompositionId, out var sheepInfo))
                    {
                        card = await Managers.Resource.GetCardResources<SheepId>(sheepInfo, rewardPanel.transform);
                    }
                    break;
                case ProductType.Character:
                    if (Managers.Data.CharacterInfoDict.TryGetValue(compositionInfo.CompositionId, out var characterInfo))
                    {
                        card = await Managers.Resource.GetCardResources<CharacterId>(characterInfo, rewardPanel.transform);
                    }
                    break;
                default: return;
            }

            card.BindEvent(OnCardSelected);
        }
    }

    private async Task OnCardSelected(PointerEventData data)
    {
        var go = data.pointerPress;
        _selectEffect.transform.SetParent(data.pointerPress.transform, false);
        _selectEffect.GetComponent<RectTransform>().anchorMin = new Vector2(0.5f, 0.5f);
        _selectEffect.GetComponent<RectTransform>().anchorMax = new Vector2(0.5f, 0.5f);
        _selectEffect.SetActive(true);
        _selectEffect.GetComponent<ParticleImage>().Play();
        _rewardPanel.GetComponent<CanvasGroup>().blocksRaycasts = false;
        
        await Task.Delay(500);
        
        var compositionId = go.GetComponent<Card>().Id;
        var compositionInfo = CompositionInfos.FirstOrDefault(ci => ci.CompositionId == compositionId);
        if (compositionInfo != null)
        {
            await _lobbyVm.CardSelected(compositionInfo);
        }
    }
}
