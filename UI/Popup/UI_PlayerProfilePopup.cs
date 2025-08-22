using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Networking;
using UnityEngine.UI;
using Zenject;

public class UI_PlayerProfilePopup : UI_Popup
{
    private IWebService _webService;
    private ITokenService _tokenService;
    private MainLobbyViewModel _lobbyVm;
    
    private Slider _expSlider;
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public UserInfo PlayerUserInfo { get; set; }
    
    private enum Images
    {
        SliderBackground,
        Flag,
    }

    private enum Buttons
    {
        ExitButton,
        PencilButton,
    }

    private enum Texts
    {
        PlayerProfileTitleText,
        PlayerProfileRankingTitleText,
        PlayerProfileVictoriesTitleText,
        PlayerProfileWinRateTitleText,
        
        UsernameText,
        RankPointText,
        LevelText,
        ExpText,
        
        RankingText,
        VictoriesText,
        WinRateText,
    }

    [Inject]
    public void Construct(IWebService webService, ITokenService tokenService, MainLobbyViewModel lobbyVm)
    {
        _webService = webService;
        _tokenService = tokenService;
        _lobbyVm = lobbyVm;
    }

    
    protected override async void Init()
    {
        try
        {
            base.Init();
        
            BindObjects();
            InitButtonEvents();
            InitEvents(); 
            await InitUIAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override async void BindObjects()
    {
        try
        {
            BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
            Bind<Image>(typeof(Images));
            Bind<Button>(typeof(Buttons));

            await Managers.Localization.UpdateTextAndFont(_textDict);
            await Managers.Localization.UpdateFont(GetText((int)Texts.UsernameText));
        
            var exp = PlayerUserInfo.Exp;
            var expMax = PlayerUserInfo.ExpToLevelUp;
            GetText((int)Texts.ExpText).text = $"{exp.ToString()} / {expMax.ToString()}";
        
            GetText((int)Texts.UsernameText).text = PlayerUserInfo.UserName;
            GetText((int)Texts.LevelText).text = PlayerUserInfo.Level.ToString();
            GetText((int)Texts.RankPointText).text = PlayerUserInfo.RankPoint.ToString();

            GetText((int)Texts.RankingText).text = "100";
            GetText((int)Texts.VictoriesText).text = PlayerUserInfo.Victories.ToString();
            GetText((int)Texts.WinRateText).text = PlayerUserInfo.WinRate.ToString();
            
            var pencilButton = GetButton((int)Buttons.PencilButton).gameObject;
            pencilButton.SetActive(User.Instance.IsGuest == false);
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
    }

    private void InitEvents()
    {
        _lobbyVm.OnUpdateUsername += UpdateUsername;
    }
    
    protected override async Task InitUIAsync()
    {
        var background = GetImage((int)Images.SliderBackground);
        _expSlider = background.transform.parent.GetComponent<Slider>();
        
        var exp = PlayerUserInfo.Exp;
        var expMax = PlayerUserInfo.ExpToLevelUp;
        _expSlider.value = exp / (float)expMax;
        
        var flag = GetImage((int)Images.Flag);
        var flagPath = $"Sprites/Icons/IconFlag/Small/icon_flag_{Managers.Localization.Language2Letter}";
        flag.sprite = await Managers.Resource.LoadAsync<Sprite>(flagPath);
        
        var pencilButton = GetButton((int)Buttons.PencilButton).gameObject;
        if (User.Instance.UserInfo.NameInitialized)
        {
            pencilButton.SetActive(false);
        }
        else
        {
            pencilButton.BindEvent(OnPencilClicked);
        }
    }

    private void UpdateUsername()
    {
        GetText((int)Texts.UsernameText).text = User.Instance.UserInfo.UserName;
    }
    
    private async Task OnPencilClicked(PointerEventData data)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_InputPopup>();
        const string titleKey = "input_update_username_title";
        const string ruleKey = "input_update_username_rule_text";

        popup.TitleKey = titleKey;
        popup.RuleKey = ruleKey;
        popup.SetInputAsyncCallback(async text =>
        {
            var warningObject = Util.FindChild(popup.gameObject, "WarningText", true, true);
            var warningText = warningObject.GetComponent<TextMeshProUGUI>();
            
            if (string.IsNullOrEmpty(text))
            {
                warningText.gameObject.SetActive(true);
                await Managers.Localization.BindLocalizedText(warningText, "warning_empty_username");
            }
            else
            {
                var packet = new UpdateNamePacketRequired
                {
                    AccessToken = _tokenService.GetAccessToken(),
                    NewName = text
                };
                var task = _webService.SendWebRequestAsync<UpdateNamePacketResponse>(
                    "UserAccount/UpdateUsername", UnityWebRequest.kHttpVerbPUT, packet);
                var response = await task;

                if (response.ChangeNameOk)
                {
                    _lobbyVm.UpdateUsername(text);
                }
                else
                {
                    warningText.gameObject.SetActive(true);
                    string warningTextKey;
                    switch (response.ErrorCode)
                    {
                        case 1:
                            warningTextKey = "warning_username_already_exists";
                            await Managers.Localization.BindLocalizedText(warningText, warningTextKey);
                            break;
                        case 2:
                            warningTextKey = "warning_username_not_exists";
                            await Managers.Localization.BindLocalizedText(warningText, warningTextKey);
                            break;
                    }
                }
            }
        });
    }

    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }

    private void OnDestroy()
    {
        _lobbyVm.OnUpdateUsername -= UpdateUsername;
    }
}
