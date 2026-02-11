using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using Docker.DotNet.Models;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;
using Random = System.Random;

public class UI_RewardPopup : UI_Popup
{
    private IUserService _userService;
    private ICardFactory _cardFactory;
    
    private Transform _levelPanel;
    private RectTransform _rewardTitle;
    private Slider _expSlider;
    private TextMeshProUGUI _expPlusText;
    private TextMeshProUGUI _expText;
    private TextMeshProUGUI _levelText;
    private RectTransform _arrowRect;

    private int _currentLevel;
    private int _currentExp;
    private int _currentMaxExp;
    private int _gainedExp;
    private bool _willLevelUp;
    private Sequence _sequence;

    private GameObject _rewardScrollView;
    private readonly Random _random = new();
    private readonly Dictionary<string, GameObject> _textDict = new();

    public List<Reward> Rewards { get; set; } = new();
    public bool FromRank { get; set; }
    public bool FromTutorial { get; set; }
    
    #region Enums

    private enum Images
    {
        LevelPanel,
        ExpPanel,
        UpImage,
        LightImage,
        SquareImage,
        StarImage1,
        StarImage2,
        LeftTitleLine,
        RightTitleLine,
        RewardScrollView,
    }
    
    private enum Buttons
    {
        PanelButton,
    }

    private enum Texts
    {
        ExpText,
        ExpPlusText,
        LevelText,
        RewardText,
        TapToContinueText,
    }

    #endregion
    
    [Inject]
    public void Construct(ICardFactory cardFactory, IUserService userService)
    {
        _cardFactory = cardFactory;
        _userService = userService;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindObjects();
            InitButtonEvents();
            InitUI();

            if (_gainedExp > 0)
            {
                DOVirtual.DelayedCall(0.5f, PlayGainExp);
            }
            else
            {
                await AfterExpGained();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));
        
        var expPanel = GetImage((int)Images.ExpPanel);
        
        _levelPanel = GetImage((int)Images.LevelPanel).transform;
        _rewardTitle = Util.FindChild<RectTransform>(gameObject, "RewardTitle", true);
        _expSlider = Util.FindChild<Slider>(expPanel.gameObject, "ExpSlider", true, true);
        _expPlusText = GetText((int)Texts.ExpPlusText);
        _expText = GetText((int)Texts.ExpText);
        _levelText = GetText((int)Texts.LevelText);
        _arrowRect = GetImage((int)Images.UpImage).GetComponent<RectTransform>();
        _gainedExp = Rewards
            .FirstOrDefault(reward => reward.ProductType == Google.Protobuf.Protocol.ProductType.Exp)?.Count ?? 0;

        _currentLevel = _userService.User.UserInfo.Level;
        _currentExp = _userService.User.UserInfo.Exp;
        _currentMaxExp = _userService.User.ExpTable[_currentLevel];
        
        _rewardScrollView = GetImage((int)Images.RewardScrollView).gameObject;
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.PanelButton).gameObject.BindEvent(OnPanelClicked);
        GetText((int)Texts.TapToContinueText).gameObject.BindEvent(OnPanelClicked);
    }

    protected override void InitUI()
    {
        _levelPanel.gameObject.SetActive(false);
        _rewardTitle.gameObject.SetActive(false);
        _rewardScrollView.gameObject.SetActive(false);
    }

    #region Exp Anim

    private void PlayGainExp()
    {
        _sequence = DOTween.Sequence();
        _sequence.SetUpdate(true);          // 타임스케일 영향 X

        _willLevelUp = _currentExp + _gainedExp >= _currentMaxExp;
        
        // Xp slider & Text
        int startExp = _currentExp;
        int targetExp = Mathf.Min(_currentExp + _gainedExp, _currentMaxExp);
        float tick = (targetExp - startExp) / 25f; // Xp 25 당 1초
        
        // Slider
        _sequence.Append(DOTween.To(
                () => _expSlider.value,
                v => _expSlider.value = v, 
                (float)targetExp / _currentMaxExp,
                tick))
            .SetEase(Ease.InOutSine);
        
        // Fill slider
        _sequence.Join(DOTween.To(
            () => startExp, val => { _expText.text = $"{val}/{_currentMaxExp}"; }, targetExp, tick)
            .SetEase(Ease.Linear));
        
        _sequence.Join(DOTween.To(
                () => _gainedExp, val => _expPlusText.text = $"+{val}", 0, tick)
            .SetEase(Ease.InOutSine));
        
        // Level up
        if (_willLevelUp)
        {
            var newMaxExp = _userService.User.ExpTable[_currentLevel];

            _sequence.AppendCallback(() =>
            {
                _currentLevel++;
                _userService.User.UserInfo.Level = _currentLevel;
                _levelText.text = _currentLevel.ToString();
                _currentExp = startExp + _gainedExp - _currentMaxExp; // 넘어온 exp
                _expPlusText.text = $"+{_currentExp}";
                _expSlider.value = 0;
                _expText.text = $"{_currentExp}/{newMaxExp}";
                
                float tick2 = _currentExp / 25f;

                _sequence.Append(DOTween.To(
                        () => _expSlider.value,
                        v => _expSlider.value = v,
                        (float)_currentExp / newMaxExp,
                        tick2).SetEase(Ease.InOutSine)
                );
            
                _sequence.Join(DOTween.To(
                        () => 0, val => _expPlusText.text = $"+{_currentExp - val}", _currentExp, tick2))
                    .SetEase(Ease.Linear);

                _sequence.Join(DOTween.To(
                        () => 0, val => _expText.text = $"{val}/{newMaxExp}", _currentExp, tick2))
                    .SetEase(Ease.Linear);
            });
        }
        else
        {
            _currentExp += _gainedExp;
        }

        if (_willLevelUp)
        {
            Vector2 size1 = new(108, 108);
            Vector2 size2 = new(108, 126);
            Vector2 size3 = new(126, 108);
            Vector2 pos1  = new(_arrowRect.anchoredPosition.x, 17);
            Vector2 pos2  = new(_arrowRect.anchoredPosition.x, 50);

            _sequence.Join(_arrowRect.DOSizeDelta(size2, 0.4f).From(size1).SetEase(Ease.OutBack))
                .AppendInterval(0f)
                .Append(_arrowRect.DOSizeDelta(size3, 0.3f).SetDelay(0.4f).SetEase(Ease.OutBack))
                .Join(_arrowRect.DOAnchorPosY(pos2.y, 0.3f).From(pos1).SetDelay(0.4f).SetEase(Ease.OutQuad));
        }
        
        _sequence.OnComplete(() =>
        {
            _ = AfterExpGained();
        });
    }

    private async Task AfterExpGained()
    {
        _expPlusText.text = "+0";
        _arrowRect.gameObject.SetActive(false);
            
        await BindRewards();

        if (_willLevelUp)
        {
            GetImage((int)Images.ExpPanel).gameObject.SetActive(false);
            _levelPanel.gameObject.SetActive(true);
            PlayLevelPanelAnim();
        }
        
        _rewardScrollView.gameObject.SetActive(true);
        _rewardTitle.gameObject.SetActive(true);
        PlayRewardPanelAnim();
    }

    private void PlayLevelPanelAnim()
    {
        _levelPanel.localScale = Vector3.zero;
        _levelPanel.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine);
        
        var lightImage = GetImage((int)Images.LightImage).GetComponent<RectTransform>();
        var squareImage = GetImage((int)Images.SquareImage).GetComponent<RectTransform>();
        var starImage1 = GetImage((int)Images.StarImage1).GetComponent<RectTransform>();
        var starImage2 = GetImage((int)Images.StarImage2).GetComponent<RectTransform>();
        
        lightImage.DORotate(new Vector3(0, 0, -360), 4f, RotateMode.FastBeyond360).SetEase(Ease.Linear).SetLoops(-1);
        lightImage.DOScale(0.9f, 1.5f).SetEase(Ease.InOutSine).SetLoops(-1);

        squareImage.DOScale(0.2f, _random.Next(5, 10) * 0.2f)
            .SetEase(Ease.Flash, _random.Next(3, 6), 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRelative();
        
        starImage1.DOScale(0.2f, _random.Next(5, 10) * 0.2f)
            .SetEase(Ease.Flash, _random.Next(3, 6), 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRelative();
        
        starImage2.DOScale(0.2f, _random.Next(5, 10) * 0.2f)
            .SetEase(Ease.Flash, _random.Next(3, 6), 0.6f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetRelative();
    }
    
    private void PlayRewardPanelAnim()
    {
        var leftTitleLine = GetImage((int)Images.LeftTitleLine).GetComponent<RectTransform>();
        var rightTitleLine = GetImage((int)Images.RightTitleLine).GetComponent<RectTransform>();
        
        leftTitleLine.localScale = Vector3.zero;
        leftTitleLine.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine);

        rightTitleLine.localScale = Vector3.zero;
        rightTitleLine.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine);
        
        _rewardScrollView.transform.localScale = new Vector3(0, 1, 1);
        _rewardScrollView.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.InOutSine);
    }
    
    private void SkipAnimation()
    {
        if (_sequence == null || _sequence.IsActive() == false) return;
        _sequence.Complete(true);
        _sequence = null;
    }
    
    #endregion
    
    private async Task BindRewards()
    {
        _rewardTitle.gameObject.SetActive(true);
        _rewardScrollView.gameObject.SetActive(true);
        
        var content = Util.FindChild(_rewardScrollView, "Content", true);

        foreach (var reward in Rewards)
        {
            Debug.Log($"ItemId: {reward.ItemId}, Type: {reward.ProductType}");
            await BindRewardUI(content.transform, reward);
        }
    }
    
    private async Task BindRewardUI(Transform parent, Reward reward)
    {
        GameObject card = null;
        switch (reward.ProductType)
        {
            case Google.Protobuf.Protocol.ProductType.Unit:
                if (Managers.Data.UnitInfoDict.TryGetValue(reward.ItemId, out var unitInfo))
                {
                    card = await _cardFactory.GetCardResources<UnitId>(unitInfo, parent);
                }
                break;
            case Google.Protobuf.Protocol.ProductType.Enchant:
                if (Managers.Data.EnchantInfoDict.TryGetValue(reward.ItemId, out var enchantInfo))
                {
                    card = await _cardFactory.GetCardResources<EnchantId>(enchantInfo, parent);
                }
                break;
            case Google.Protobuf.Protocol.ProductType.Sheep:
                if (Managers.Data.SheepInfoDict.TryGetValue(reward.ItemId, out var sheepInfo))
                {
                    card = await _cardFactory.GetCardResources<SheepId>(sheepInfo, parent);
                }
                break;
            case Google.Protobuf.Protocol.ProductType.Character:
                if (Managers.Data.CharacterInfoDict.TryGetValue(reward.ItemId, out var characterInfo))
                {
                    card = await _cardFactory.GetCardResources<CharacterId>(characterInfo, parent);
                }
                break;
            case Google.Protobuf.Protocol.ProductType.Material:
                if (Managers.Data.MaterialInfoDict.TryGetValue(reward.ItemId, out var materialInfo))
                {
                    card = await _cardFactory.GetMaterialResources(materialInfo, parent);
                }
                break;
            case Google.Protobuf.Protocol.ProductType.Gold:
                card = await _cardFactory.GetItemFrameGold(reward.Count, parent);
                break;
            case Google.Protobuf.Protocol.ProductType.Spinel:
                card = await _cardFactory.GetItemFrameSpinel(reward.Count, parent);
                break;
            case Google.Protobuf.Protocol.ProductType.Container:
                var rewardName = ((ProductId)reward.ItemId).ToString();
                var path = $"UI/Shop/NormalizedProducts/{rewardName}";
                card = await Managers.Resource.Instantiate(path, parent);
                break;
            default:
                break;
        }

        var layoutElement = card.GetOrAddComponent<LayoutElement>();
        
        switch (reward.ProductType)
        {
            case Google.Protobuf.Protocol.ProductType.Unit:
            case Google.Protobuf.Protocol.ProductType.Enchant:
            case Google.Protobuf.Protocol.ProductType.Sheep:
            case Google.Protobuf.Protocol.ProductType.Character:
                layoutElement.preferredWidth = 200;
                layoutElement.preferredHeight = 320;
                break;
            case Google.Protobuf.Protocol.ProductType.Material:
                layoutElement.preferredWidth = 200;
                layoutElement.preferredHeight = 200;
                break;
            case Google.Protobuf.Protocol.ProductType.Gold:
            case Google.Protobuf.Protocol.ProductType.Spinel:
            case Google.Protobuf.Protocol.ProductType.Container:
                if (card != null)
                {
                    if (card.transform.GetChild(0).TryGetComponent(out RectTransform childRect))
                    {
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
                    }
                }
                break;
        }        
        
        var countText = Util.FindChild(card, "TextNum", true);
        if (countText != null)
        {
            countText.GetComponent<TextMeshProUGUI>().text = reward.Count.ToString();
        }
    }

    private void CheckRewards()
    {
        var scene = FromRank || FromTutorial ? Define.Scene.MainLobby : Define.Scene.SinglePlay;
        Managers.Scene.LoadScene(scene);
        Managers.UI.ClosePopupUI();
    }

    private void OnPanelClicked(PointerEventData data)
    {
        if (_sequence != null && _sequence.IsActive())
        {
            SkipAnimation();
        }
        else
        {
            CheckRewards();
        }
    }

    private void OnDestroy()
    {
        _sequence?.Kill();
    }
}
