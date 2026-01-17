using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;
using Button = UnityEngine.UI.Button;

public class UI_RewardOpenPopup : UI_Popup
{
    private MainLobbyViewModel _lobbyVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    private TotalProductInfo _randomProductInfo = new();
    private Transform _rewardView;
    private GameObject _rewardObject;
    private GameObject _openEffect;
    private Coroutine _shakeCo;

    public List<TotalProductInfo> RandomProductInfos { get; set; }
    
    private enum Buttons
    {
        OpenOneButton,
        OpenAllButton,
    }
    
    private enum Images
    {
        Dimed,
        RewardView,
    }
    
    private enum Texts
    {
        RewardOpenText,
        OpenOneText,
        OpenAllText,
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
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        
        if (RandomProductInfos == null || RandomProductInfos.Count == 0)
        {
            Debug.LogWarning("RandomProductInfos is empty.");
            ClosePopupUI();
            return;
        }
        _randomProductInfo = RandomProductInfos[0];
        _rewardView = GetImage((int)Images.RewardView).transform;

        await Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.OpenOneButton).gameObject.BindEvent(OnOpenOneClicked);
        GetButton((int)Buttons.OpenAllButton).gameObject.BindEvent(OnOpenAllClicked);
    }
    
    protected override async Task InitUIAsync()
    {
        var path = $"UI/Shop/NormalizedProducts/{(ProductId)_randomProductInfo.ProductInfo.ProductId}";
        var rewardObject = await Managers.Resource.Instantiate(path, _rewardView);
        var countText = Util.FindChild(rewardObject, "TextNum", true, true);

        _rewardObject = rewardObject;
        _rewardObject.BindEvent(OnOpenAllClicked);
        
        countText.GetComponent<TextMeshProUGUI>().text = $"x{_randomProductInfo.Count}";

        var rect = _rewardObject.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero;
        rect.localScale = Vector3.one * 1.5f;

        if (_shakeCo != null) StopCoroutine(_shakeCo);
        _shakeCo = StartCoroutine(ShakeProduct());
    }

    private void OnOpenOneClicked(PointerEventData data) => _ = OpenAsync(data, openAll:false);
    private void OnOpenAllClicked(PointerEventData data) => _ = OpenAsync(data, openAll:true);

    private async Task OpenAsync(PointerEventData data, bool openAll)
    {
        var button = data.pointerPress?.GetComponent<Button>();
        if (button == null || !button.interactable) return;
        button.interactable = false;

        if (_shakeCo != null) { StopCoroutine(_shakeCo); _shakeCo = null; }

        if (_openEffect != null) Destroy(_openEffect);
        _openEffect = await Managers.Resource.Instantiate("UIEffects/OpenBox", _rewardView);

        await Task.Delay(600);
        await _lobbyVm.OpenProduct(_randomProductInfo.ProductInfo, openAll);

        // 여기서 다시 interactable을 true로 돌릴지 말지는 UX 정책(대개는 팝업이 닫히므로 생략)
    }
    
    private void ClosePopupUI()
    {
        Managers.UI.ClosePopupUI();
    }

    private IEnumerator ShakeProduct()
    {
        var rect = _rewardObject.GetComponent<RectTransform>();
        var basePos = rect.anchoredPosition;

        const float amp = 8f;   // 흔들림 폭(px)
        const float freq = 36f;  // 흔들림 속도

        while (true)
        {
            float x = Mathf.Sin(Time.time * freq) * amp;
            rect.anchoredPosition = basePos + new Vector2(x, 0f);
            yield return null;
        }
    }
    
    private void OnDestroy()
    {
        if (_shakeCo != null) StopCoroutine(_shakeCo);
        if (_openEffect != null) Destroy(_openEffect);
    }
}
