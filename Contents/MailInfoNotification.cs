using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MailInfoNotification : UI_Base
{
    private MainLobbyViewModel _lobbyVm;
    private readonly Dictionary<string, GameObject> _textDict = new();

    public MailInfo MailInfo { get; set; }

    private enum Texts
    {
        InfoText,
        ExpiresText,
        CheckText,
    }

    private enum Buttons
    {
        CheckButton,
    }

    [Inject]
    public void Construct(MainLobbyViewModel lobbyVm)
    {
        _lobbyVm = lobbyVm;
    }
    
    protected override void Init()
    {
        BindObjects();
        InitButtonEvents();
        InitUI();
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));

        var checkText = GetText((int)Texts.CheckText);
        _ = Managers.Localization.BindLocalizedText(checkText, "check_text");
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.CheckButton).onClick.AddListener( () => _ =  OnCheckClicked());
    }

    protected override async Task InitUIAsync()
    {
        var infoText = GetText((int)Texts.InfoText);
        await Managers.Localization.UpdateFont(infoText);
        infoText.text = MailInfo.Message;
        
        var expiresText = GetText((int)Texts.ExpiresText);
        var key = "mail_expires_text";
        var placeholderKeys = new List<string> { "value" };
        var expiresAt = (MailInfo.ExpiresAt.Date - DateTime.Today).Days;
        var replacers = new List<string> { Mathf.Max(expiresAt, 0).ToString() };
        await Managers.Localization.FormatLocalizedText(expiresText, key, placeholderKeys, replacers);

        GetButton((int)Buttons.CheckButton).interactable = !MailInfo.Claimed;
    }

    private async Task OnCheckClicked()
    {
        GetButton((int)Buttons.CheckButton).interactable = false;
        await _lobbyVm.ClaimMail(MailInfo);
    }

    private void OnDestroy()
    {
        GetButton((int)Buttons.CheckButton).onClick.RemoveAllListeners();
    }
}
