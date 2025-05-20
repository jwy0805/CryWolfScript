using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_SinglePlayMapPopup : UI_Popup
{
    private SinglePlayViewModel _singlePlayVm;

    private GameObject _map;
    private Stage _selectedStage;
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public Faction Faction { get; set; }
    
    [Inject]
    public void Construct(SinglePlayViewModel singlePlayVm)
    {
        _singlePlayVm = singlePlayVm;
    }
    
    private enum Buttons
    {
        BackButton,
        CloseInfoButton,
        FightButton,
        ExitButton,
    }

    private enum Images
    {
        StageInfoPanel,
    }

    private enum Texts
    {
        SinglePlayMapTitleText,
        SinglePlayMapStageTitleText,
        SinglePlayMapFightText,
        SinglePlayMapEnemyText,
        SinglePlayMapRewardText,
    }

    protected override void Init()
    {
        base.Init();
        
        BindObjects();
        InitButtonEvents();
        InitUI();
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        
        Managers.Localization.UpdateTextAndFont(_textDict);
        
        var path = $"Map/SingleMap_{Faction}{_singlePlayVm.StageLevel}";
        var parent = Util.FindChild(gameObject, "Content", true);
        _map = Managers.Resource.Instantiate(path, parent.transform);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.BackButton).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
        GetButton((int)Buttons.CloseInfoButton).gameObject.BindEvent(CloseInfoPanel);
        GetButton((int)Buttons.FightButton).gameObject.BindEvent(OnFightClicked);
        
        var stages = _map.GetComponentsInChildren<Stage>();
        foreach (var stage in stages)
        {
            stage.gameObject.BindEvent(OnStageClicked);
        }
    }
    
    protected override void InitUI()
    {
        GetImage((int)Images.StageInfoPanel).gameObject.SetActive(false);
    }

    private void BindStageInfoPanel(Stage stage)
    {
        var stageInfoPanel = GetImage((int)Images.StageInfoPanel).transform;
        var deckPanel = stageInfoPanel.Find("Deck");
        var rewardPanel = stageInfoPanel.Find("Reward");
        var unitInfos = stage.EnemyInfo.OrderByDescending(info => info.Class).ToList();
        var singleRewardInfos = stage.RewardInfo.OrderBy(info => info.Star).ToList();
        var available = stage.UserStageInfo?.IsAvailable ?? false;
        
        stageInfoPanel.gameObject.SetActive(true);
        
        if (available == false)
        {
            GetButton((int)Buttons.FightButton).interactable = false;
        }
        
        Util.DestroyAllChildren(deckPanel);
        Util.DestroyAllChildren(rewardPanel);
        
        foreach (var unitInfo in unitInfos)
        {
            var cardObject = Managers.Resource.GetCardResources<UnitId>(unitInfo, deckPanel);
            cardObject.GetComponent<Card>().SetLocalScale(1, false);
        }

        foreach (var singleRewardInfo in singleRewardInfos)
        {
            var framePath = "UI/Deck/ProductInfo";
            var rewardFrame = Managers.Resource.Instantiate(framePath, rewardPanel);
            var countText = Util.FindChild(rewardFrame, "TextNum", true);
            var count = singleRewardInfo.Count;
            
            countText.GetComponent<TextMeshProUGUI>().text = count.ToString();

            string path;
            GameObject reward;
            switch (singleRewardInfo.ProductType)
            {
                case ProductType.None:
                case ProductType.Material:
                    var rewardName = ((ProductId)singleRewardInfo.ItemId).ToString();
                    path = $"UI/Shop/NormalizedProducts/{rewardName}";
                    reward = Managers.Resource.Instantiate(path, rewardFrame.transform);
                    reward.transform.SetAsFirstSibling();
                    break;
                    
                case ProductType.Spinel:
                    path = singleRewardInfo.Count switch
                    {
                        >= 100 => "UI/Shop/NormalizedProducts/SpinelFistful",
                        _ => "UI/Shop/NormalizedProducts/SpinelPile"
                    };
                    reward = Managers.Resource.Instantiate(path, rewardFrame.transform);
                    reward.transform.SetAsFirstSibling();
                    break;
                
                case ProductType.Gold:
                    path = singleRewardInfo.Count switch
                    {
                        >= 50000 => "UI/Shop/NormalizedProducts/GoldVault",
                        >= 25000 => "UI/Shop/NormalizedProducts/GoldBasket",
                        >= 2500 => "UI/Shop/NormalizedProducts/GoldPouch",
                        _ => "UI/Shop/NormalizedProducts/GoldPile"
                    };
                    reward = Managers.Resource.Instantiate(path, rewardFrame.transform);
                    reward.transform.SetAsFirstSibling();
                    break;
                
                default:
                    reward = null;
                    break;
            }

            if (reward != null)
            {
                var rewardRect = reward.GetComponent<RectTransform>();
                rewardRect.sizeDelta = new Vector2(150, 150);
                rewardRect.anchoredPosition = Vector2.zero;
                rewardRect.anchorMin = new Vector2(0.5f, 0.5f);
                rewardRect.anchorMax = new Vector2(0.5f, 0.5f);

                var starPath = "UI/Deck/Star";
                var star = Managers.Resource.Instantiate(starPath, rewardFrame.transform);
                var starRect = star.GetComponent<RectTransform>();
                starRect.sizeDelta = new Vector2(150, 35);
                starRect.anchoredPosition = Vector2.zero;
                starRect.anchorMin = new Vector2(0.5f, 1f);
                starRect.anchorMax = new Vector2(0.5f, 1f);
                
                for (var i = 0; i < 3; i++)
                {
                    star.transform.GetChild(i).gameObject.SetActive(i < singleRewardInfo.Star);
                }
            }
        }
    }
    
    // Button Events
    private void OnStageClicked(PointerEventData data)
    {
        var stage = data.pointerPress.GetComponent<Stage>();
        _selectedStage = stage;
        _singlePlayVm.SelectedStageId = stage.stageId;
        BindStageInfoPanel(stage);
    }
    
    private async Task OnFightClicked(PointerEventData data)
    {
        if (_selectedStage == null) return;
        if (_selectedStage.UserStageInfo == null || _selectedStage.UserStageInfo.IsAvailable == false) return;
        await _singlePlayVm.ConnectGameSession();
    }
    
    private void CloseInfoPanel(PointerEventData data)
    {
        var stageInfoPanel = GetImage((int)Images.StageInfoPanel).gameObject;
        stageInfoPanel.SetActive(false);
    }
    
    private void ClosePopup(PointerEventData data)
    {
        _singlePlayVm.SelectedStageId = 0;
        Managers.UI.ClosePopupUI();
    }
}
