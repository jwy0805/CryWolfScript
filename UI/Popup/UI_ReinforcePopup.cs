using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

/* Last Modified : 24. 10. 08
 * Version : 1.013
 */

public class UI_ReinforcePopup : UI_Popup
{
    private CraftingViewModel _craftingVm;
    private CollectionViewModel _collectionVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    private readonly List<RectTransform> _cardRects = new();
    private readonly List<float> _angles = new();
    private RectTransform _cardPanelRect;
    private RectTransform _rect;
    private float _radius;
    private float _elapsedTime;
    private readonly int _standbyTime = 2500;
    private readonly float _emitterThreshold = 150f;
    private bool _showEffect;
    private UnitId? _newUnitId;
    private bool _isSuccess;
    
    private enum Images
    {
        CardPanel,
    }

    private enum Buttons
    {
        TextButton,
    }

    private enum Texts
    {
        ReinforceTouchText,
    }
    
    [Inject]
    public void Construct(CraftingViewModel craftingVm, CollectionViewModel collectionVm)
    {
        _craftingVm = craftingVm;
        _collectionVm = collectionVm;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
            
            BindObjects();
            InitButtonEvents();
            InitUI();
        
            GetButton((int)Buttons.TextButton).gameObject.SetActive(false);
        
            await PlaceCardsInCircle(_craftingVm.ReinforceMaterialUnits);
            await ShowResult();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    private async void Update()
    {
        try
        {
            if (_radius > 0)
            {
                await RotateCards();
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }
    
    private async Task PlaceCardsInCircle(List<UnitInfo> materialUnits)
    {
        var unitCounts = materialUnits.Count;
        var parent = GetImage((int)Images.CardPanel).transform;
        
        for (var i = 0; i < unitCounts; i++)
        {
            float angle = i * Mathf.PI * 2 / unitCounts;
            Vector3 pos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * _radius;
            var cardFrame = await Managers.Resource.GetCardResources<UnitId>(materialUnits[i], parent);
            var cardRect = cardFrame.GetComponent<RectTransform>();
            
            _cardRects.Add(cardRect);
            _angles.Add(angle);
            
            cardRect.localPosition = pos;
            cardRect.sizeDelta = new Vector2(300, 480);
        }
    }

    private async Task RotateCards()
    {
        if (_radius <= 0f)
            return;

        // 이펙트 트리거
        if (_radius < _emitterThreshold && _showEffect == false)
        {
            _showEffect = true;
            await ShowEmitterEffect();
        }

        // 반지름 감소
        float shrinkSpeed = 200f; // 원하는 값, 필드로 빼도 됨
        _radius = Mathf.Max(0f, _radius - shrinkSpeed * Time.deltaTime);

        // 회전 속도
        float deltaAngle = 10f * Time.deltaTime;

        for (var i = 0; i < _cardRects.Count; i++)
        {
            _angles[i] += deltaAngle;

            Vector3 newPos = new Vector3(
                Mathf.Cos(_angles[i]),
                Mathf.Sin(_angles[i])
            ) * _radius;

            _cardRects[i].localPosition = newPos;
        }
    }

    private async Task ShowEmitterEffect()
    {
        var effectPath = "UIEffects/UniformEmitter";
        await Managers.Resource.Instantiate(effectPath, _cardPanelRect);
    }

    private async Task ShowResult()
    {
        await Task.Delay(_standbyTime);
        BindResult();
        
        if (_newUnitId == null)
        {
            Debug.LogWarning("New unit ID is null. Cannot show reinforce result.");
            return;
        }
        
        Util.DestroyAllChildren(_cardPanelRect.transform);

        var newUnitId = _newUnitId.Value;
        var effectPath = "UIEffects/Puff";
        var puffEffect = await Managers.Resource.Instantiate(effectPath, _cardPanelRect);
        puffEffect.transform.localScale = new Vector3(3f, 3f, 3f);
        
        Managers.Data.UnitInfoDict.TryGetValue((int)newUnitId, out UnitInfo newUnitInfo);
        if (newUnitInfo == null)
        {
            Debug.LogWarning($"UnitInfo for UnitId {newUnitId} not found.");
            return;
        }
        
        var cardFrame = await Managers.Resource.GetCardResources<UnitId>(newUnitInfo, _cardPanelRect, ClosePopup);
        var cardFrameRect = cardFrame.GetComponent<RectTransform>();
        var textButton = GetButton((int)Buttons.TextButton).gameObject;
        
        cardFrameRect.sizeDelta = new Vector2(350, 560);
        textButton.SetActive(true);

        if (_isSuccess == false)
        {
            var path = $"Sprites/Portrait/{((UnitId)newUnitInfo.Id).ToString()}_gray";
            var cardUnit = Util.FindChild(cardFrame, "CardUnit", true);
            var successText = textButton.GetComponentInChildren<TextMeshProUGUI>();
            var failKey = "reinforce_complete_text_fail";
            cardUnit.GetComponent<Image>().sprite = await Managers.Resource.LoadAsync<Sprite>(path);
            successText.text = await Managers.Localization.BindLocalizedText(successText, failKey);
        }
        else
        {
            var successEffectPath = 
                (int)newUnitInfo.Class >= (int)UnitClass.NobleKnight ? "UIEffects/SuccessHigh" : "UIEffects/SuccessLow";
            var successText = textButton.GetComponentInChildren<TextMeshProUGUI>();
            var successKey = "reinforce_complete_text_success";
            await Managers.Resource.Instantiate(successEffectPath, _cardPanelRect);
            successText.text = await Managers.Localization.BindLocalizedText(successText, successKey);
            cardFrameRect.SetAsLastSibling();
        }
        
        puffEffect.transform.SetAsLastSibling();
    }

    private void BindResult()
    {
        _newUnitId = _craftingVm.IsReinforceSuccess.newUnitId;
        _isSuccess = _craftingVm.IsReinforceSuccess.isSuccess;
    }
    
    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Image>(typeof(Images));
        Bind<Button>(typeof(Buttons));

        _ = Managers.Localization.UpdateTextAndFont(_textDict);
        
        _rect = GetComponent<RectTransform>();
        _radius = _rect.rect.width / 3;
        _cardPanelRect = GetImage((int)Images.CardPanel).GetComponent<RectTransform>();
    }
    
    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.TextButton).gameObject.BindEvent(ClosePopup);
    }
    
    protected override void InitUI()
    {
        var parent = GetImage((int)Images.CardPanel).transform;
    }

    private void ClosePopup(PointerEventData eventData)
    {
        // Reset the crafting UI
        Managers.UI.ClosePopupUI();
        _ = _collectionVm.LoadCollection();
    }
}
