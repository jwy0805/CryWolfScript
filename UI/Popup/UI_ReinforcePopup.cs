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
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    private readonly List<RectTransform> _cardRects = new();
    private readonly List<float> _angles = new();
    private RectTransform _cardPanelRect;
    private RectTransform _rect;
    private float _radius;
    private float _elapsedTime;
    private readonly int _standbyTime = 2500;
    private readonly float _emitterThreshold = 120f;
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
    public void Construct(CraftingViewModel craftingVm)
    {
        _craftingVm = craftingVm;
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            _craftingVm.BindReinforceResult -= BindResult;
            _craftingVm.BindReinforceResult += BindResult;
        
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
        for (var i = 0; i < _cardRects.Count; i++)
        {
            float angle = _angles[i] + 10 * Time.deltaTime;
            Vector3 newPos = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * _radius;
            _elapsedTime += Time.deltaTime;
            
            // Divide the elapsed time by 10 to determine a time-based factor,
            // which will be used for modifying the radius.
            float time = _elapsedTime / 3;

            if (_radius < _emitterThreshold)
            {
                if (_showEffect == false)
                {
                    _showEffect = true;
                    await ShowEmitterEffect();
                }
            }
            
            if (_radius - time < 0)
            {
                _radius = 0;
            }
            else
            {
                _radius -= time * 2;
            }
            
            _angles[i] = angle;
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
            var successEffectPath = (int)newUnitInfo.Class >= 5 ? "UIEffects/SuccessHigh" : "UIEffects/SuccessLow";
            var successText = textButton.GetComponentInChildren<TextMeshProUGUI>();
            var successKey = "reinforce_complete_text_success";
            await Managers.Resource.Instantiate(successEffectPath, _cardPanelRect);
            successText.text = await Managers.Localization.BindLocalizedText(successText, successKey);
            cardFrameRect.SetAsLastSibling();
        }
        
        puffEffect.transform.SetAsLastSibling();
    }

    private void BindResult(UnitId newUnitId, bool success)
    {
        _newUnitId = newUnitId;
        _isSuccess = success;
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
    }
    
    private void OnDestroy()
    {
        _craftingVm.BindReinforceResult -= BindResult;
    }
}
