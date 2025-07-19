using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_RewardChoicePopup : UI_Popup
{
    private readonly ITokenService _tokenService;
    private readonly IWebService _webService;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private Transform _rewardPanel;
    
    public ProductInfo ProductInfo { get; set; }
    public List<CompositionInfo> CompositionInfos { get; set; } = new();
    
    // [Inject]
    // public UI_RewardChoicePopup(ITokenService tokenService, IWebService webService)
    // {
    //     _tokenService = tokenService;
    //     _webService = webService;
    // }
    
    private enum Images
    {
        Dimed,
        RibbonImage,
        RewardPanel,
        OpenedPanel,
    }

    private enum Texts
    {
        RewardSelectText,
        OpenedText,
        TapToContinueText,
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
        
        _rewardPanel = GetImage((int)Images.RewardPanel).transform;
        
        await Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetText((int)Texts.TapToContinueText).gameObject.BindEvent(OnTapToContinueClicked);
    }

    protected override async Task InitUIAsync()
    {
        
    }
    
    private void OnTapToContinueClicked(PointerEventData data)
    {
        
    }
}
