using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class EventCondition
{
    [JsonProperty("type")]
    [JsonConverter(typeof(StringEnumConverter))]
    public EventConditionType Type { get; set; }

    [JsonProperty("counterKey")]
    [JsonConverter(typeof(StringEnumConverter))]
    public EventCounterKey CounterKey { get; set; }

    [JsonProperty("value")]
    public int Value { get; set; }
}

public class UI_EventPopup : UI_Popup
{
    private ICardFactory _cardFactory;
    private IUIFactory _uiFactory;
    private MainLobbyViewModel _lobbyVm;
    private CollectionViewModel _collectionVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    private Dictionary<int, GameObject> _rewardPanelDict = new();
    private Dictionary<int, Button> _rewardButtonDict = new();
    private Dictionary<int, List<RewardInfo>> _rewardInfoDict = new();

    private Slider _progressFill;
    private GetEventProgressResponse _response;
    
    public EventInfo EventInfo { get; set; }

    private enum Buttons
    {
        EnterButton,
        RewardTier1ClaimButton,
        RewardTier2ClaimButton,
        RewardTier3ClaimButton,
    }

    private enum Texts
    {
        EventPopupTitleText,
        EventPopupContentText,
        EnterText,
        
        RewardTier1ClaimText,    
        RewardTier2ClaimText,
        RewardTier3ClaimText,
    }

    private enum Images
    {
        Middle,
        RewardTier1Panel,
        RewardTier1DisplayPanel,
        RewardTier2Panel,
        RewardTier2DisplayPanel,
        RewardTier3Panel,
        RewardTier3DisplayPanel,
    }
    
    [Inject]
    public void Construct(ICardFactory cardFactory,
        IUIFactory uiFactory,
        MainLobbyViewModel lobbyViewModel,
        CollectionViewModel collectionViewModel)
    {
        _cardFactory = cardFactory;
        _uiFactory = uiFactory;
        _lobbyVm = lobbyViewModel;
        _collectionVm = collectionViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            _response = await _lobbyVm.GetEventProgress(EventInfo.EventId);
             
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
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        
        var middle = GetImage((int)Images.Middle).gameObject;
        _progressFill = Util.FindChild(middle, "ProgressFill", true).GetComponent<Slider>();
        
        _rewardPanelDict = new Dictionary<int, GameObject>
        {
            { 1, GetImage((int)Images.RewardTier1Panel).gameObject },
            { 2, GetImage((int)Images.RewardTier2Panel).gameObject },
            { 3, GetImage((int)Images.RewardTier3Panel).gameObject },
        };
        
        _rewardButtonDict = new Dictionary<int, Button>
        {
            { 1, GetButton((int)Buttons.RewardTier1ClaimButton) },
            { 2, GetButton((int)Buttons.RewardTier2ClaimButton) },
            { 3, GetButton((int)Buttons.RewardTier3ClaimButton) },
        };
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.EnterButton).onClick.AddListener(ClosePopupUI);
        GetButton((int)Buttons.RewardTier1ClaimButton).onClick.AddListener(() => _ = ClaimReward(1));
        GetButton((int)Buttons.RewardTier2ClaimButton).onClick.AddListener(() => _ = ClaimReward(2));
        GetButton((int)Buttons.RewardTier3ClaimButton).onClick.AddListener(() => _ = ClaimReward(3));
    }

    protected override async Task InitUIAsync()
    {
        var title = _textDict["EventPopupTitleText"].GetComponent<TextMeshProUGUI>();
        var contentText = _textDict["EventPopupContentText"].GetComponent<TextMeshProUGUI>();
        var enter = _textDict["EnterText"].GetComponent<TextMeshProUGUI>();
        var updateTitleTask = Managers.Localization.UpdateFont(title, FontType.BlackLined);
        var updateContentTask = Managers.Localization.UpdateFont(contentText);
        var updateEnterTask = Managers.Localization.UpdateFont(enter, FontType.BlackLined);
        var initProgressTask = InitProgressBar();
        var initRewardTask = InitRewardPanels();
        
        title.text = _response.Title;
        contentText.text = _response.Content;
        
        await Task.WhenAll(updateTitleTask, updateContentTask, updateEnterTask, initProgressTask, initRewardTask);
        
        contentText.ForceMeshUpdate();
            
        var contentRect = contentText.rectTransform.parent as RectTransform;
        if (contentRect != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRect);
        }
    }

    private async Task InitProgressBar()
    {
        var conditions = _response.TierInfos
            .Select(ti => JsonConvert.DeserializeObject<EventCondition>(ti.ConditionJson))
            .Where(c => c != null)
            .ToList();

        if (conditions.Count == 0)
        {
            _progressFill.maxValue = 1;
            _progressFill.value = 0;
            return;
        }

        var values = conditions.Select(c => c.Value).ToList();
        var maxValue = values.Max();
        if (maxValue <= 0)
        {
            _progressFill.maxValue = 1;
            _progressFill.value = 0;
        }
        
        _progressFill.minValue = 0;
        _progressFill.maxValue = maxValue;
        _progressFill.value = Mathf.Clamp(_response.ProgressValue, 0, maxValue);

        var progressGroup = Util.FindChild(_progressFill.gameObject, "ProgressGroup", true);
        var progressHorizontalLayout = progressGroup.GetComponent<HorizontalLayoutGroup>();
        progressHorizontalLayout.spacing = 700f / maxValue - 54f;

        for (var i = 0; i <= maxValue; i++)
        {
            var iconPath = i switch
            {
                0 => "Sprites/UIIcons/progress_blue",
                _ when i < maxValue && !values.Contains(i) => "Sprites/UIIcons/progress_blue",
                _ => "Sprites/UIIcons/progress_yellow"
            };

            var sprite = await Managers.Resource.LoadAsync<Sprite>(iconPath);
            var iconObject = new GameObject($"Icon{i}", typeof(RectTransform), typeof(Image));
            var iconImage = iconObject.GetComponent<Image>();
            iconImage.sprite = sprite;
            iconImage.SetNativeSize();
            iconObject.transform.SetParent(progressGroup.transform, false);
        }
    }
    
    private async Task InitRewardPanels()
    {
        var tierInfos = _response.TierInfos;

        for (var i = 1; i <= 3; i++)
        {
            var hasTier = tierInfos.Any(ti => ti.Tier == i);
            _rewardPanelDict[i].SetActive(hasTier);
        }
        
        foreach (var tierInfo in tierInfos)
        {
            var tier = tierInfo.Tier;
            var parent = tier switch
            {
                1 => GetImage((int)Images.RewardTier1DisplayPanel).transform,
                2 => GetImage((int)Images.RewardTier2DisplayPanel).transform,
                3 => GetImage((int)Images.RewardTier3DisplayPanel).transform,
                _ => null
            };
            if (parent == null) continue;
            
            _rewardPanelDict[tier].SetActive(true);

            var claimText = _textDict[$"RewardTier{tier}ClaimText"].GetComponent<TextMeshProUGUI>();

            if (tierInfo.IsClaimed)
            {
                _rewardButtonDict[tier].interactable = false;
                await Managers.Localization.UpdateTextAndFont(claimText.gameObject, "reward_claimed_text");
            }
            else
            {
                _rewardButtonDict[tier].interactable = tierInfo.IsClaimable;
                await Managers.Localization.UpdateTextAndFont(claimText.gameObject, "claim_text");
            }
            
            try
            {
                var rewards = JsonConvert.DeserializeObject<List<RewardInfo>>(tierInfo.RewardJson);
                if (rewards == null || rewards.Count == 0) continue;

                _rewardInfoDict.TryAdd(tier, rewards);
                
                foreach (var reward in rewards)
                {
                    await BindItemUI(parent, reward);
                }
            }
            catch
            {
                continue;
            }
        }
    }
    
    private async Task BindItemUI(Transform parent, RewardInfo info)
    {
        GameObject cardObject = null;
        switch (info.ProductType)
        {
            case ProductType.Unit:
                if (Managers.Data.UnitInfoDict.TryGetValue(info.ItemId, out var unitInfo))
                {
                    cardObject = await _cardFactory.GetCardResources<UnitId>(unitInfo, parent);
                }
                break;
            case ProductType.Enchant:
                if (Managers.Data.EnchantInfoDict.TryGetValue(info.ItemId, out var enchantInfo))
                {
                    cardObject =await _cardFactory.GetCardResources<EnchantId>(enchantInfo, parent);   
                }
                break;
            case ProductType.Sheep:
                if (Managers.Data.SheepInfoDict.TryGetValue(info.ItemId, out var sheepInfo))
                {
                    cardObject =await _cardFactory.GetCardResources<SheepId>(sheepInfo, parent);
                }
                break;
            case ProductType.Character:
                if (Managers.Data.CharacterInfoDict.TryGetValue(info.ItemId, out var characterInfo))
                {
                    cardObject =await _cardFactory.GetCardResources<CharacterId>(characterInfo, parent);
                }
                break;
            case ProductType.Material:
                if (Managers.Data.MaterialInfoDict.TryGetValue(info.ItemId, out var materialInfo))
                {
                    cardObject = await _cardFactory.GetMaterialResources(materialInfo, parent);
                    var countText = Util.FindChild(cardObject, "CountText", true);
                    countText.GetComponent<TextMeshProUGUI>().text = info.Count.ToString();
                }
                break;
            case ProductType.Gold:
                cardObject = await _cardFactory.GetItemFrameGold(info.Count, parent);
                break;
            case ProductType.Spinel:
                cardObject = await _cardFactory.GetItemFrameSpinel(info.Count, parent);
                break;
            case ProductType.Container:
                var path = $"UI/Shop/NormalizedProducts/{(ProductId)info.ItemId}";
                cardObject = await Managers.Resource.Instantiate(path, parent);
                var go = Util.FindChild(cardObject, "TextNum", true, true);
                if (go != null)
                {
                    var countText = go.GetComponent<TextMeshProUGUI>();
                    countText.text = info.Count.ToString();
                }
                break;
            default: return;
        }
        
        var parentLayoutElement = parent.GetComponent<LayoutElement>();
        parentLayoutElement.preferredWidth = 600;
        
        if (cardObject != null)
        {
            var layoutElement = cardObject.GetOrAddComponent<LayoutElement>();
            
            switch (info.ProductType)
            {
                case ProductType.Unit:
                case ProductType.Enchant:
                case ProductType.Sheep:
                case ProductType.Character:
                    layoutElement.preferredWidth = 150;
                    layoutElement.preferredHeight = 240;
                    parentLayoutElement.preferredHeight = 240;
                    break;
                case ProductType.Material:
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
    
    private async Task ClaimReward(int tier)
    {
        var response = await _lobbyVm.ClaimEventReward(EventInfo.EventId, tier);
        
        if (response.ClaimOk)
        {
            await Managers.UI.ShowNotifyPopup("empty_text", "notify_claim_event_reward_success",
                () => _rewardButtonDict[tier].interactable = false);
            
            var rewardTypes = _rewardInfoDict[tier]
                .Select(ri => ri.ProductType)
                .ToHashSet();

            if (rewardTypes.Contains(ProductType.Gold) 
                || rewardTypes.Contains(ProductType.Spinel) 
                || rewardTypes.Contains(ProductType.Exp))
            {
                await _lobbyVm.InitUserInfo();
            }
            
            if (rewardTypes.Contains(ProductType.Unit)
                || rewardTypes.Contains(ProductType.Character)
                || rewardTypes.Contains(ProductType.Sheep)
                || rewardTypes.Contains(ProductType.Enchant)
                || rewardTypes.Contains(ProductType.Material))
            {
                await _collectionVm.LoadCollection();
            }
        }
        else
        {
            await Managers.UI.ShowNotifyPopup("empty_text", "notify_claim_event_reward_failed");
        }
    }
    
    private void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI();
    }
}
