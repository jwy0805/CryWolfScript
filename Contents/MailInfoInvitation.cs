using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MailInfoInvitation : UI_Base
{
    private MainLobbyViewModel _lobbyVm;
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public MailInfo MailInfo { get; set; }
    
    private enum Texts
    {
        MailInvitationText,
        ExpiresText,
    }

    private enum Buttons
    {
        AcceptButton,
        DenyButton
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
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.AcceptButton).onClick.AddListener(() => _ =  OnAcceptClicked());
        GetButton((int)Buttons.DenyButton).onClick.AddListener(() => _ =  OnDenyClicked());
    }

    protected override void InitUI()
    {
         InitInvitationText();
         InitExpiresText();
    }

    private void InitInvitationText()
    {
        var text = GetText((int)Texts.MailInvitationText);
        var key = "mail_invitation_text";
        var placeholderKeys = new List<string> { "value" };
        var replacers = new List<string> { MailInfo.Sender };
        Managers.Localization.UpdateFont(text);
        Managers.Localization.FormatLocalizedText(text, key, placeholderKeys, replacers);
    }

    private void InitExpiresText()
    {
        var text = GetText((int)Texts.ExpiresText);
        var key = "mail_expires_text";
        var placeholderKeys = new List<string> { "value" };
        var expiresAt = (MailInfo.ExpiresAt.Date - DateTime.Today).Minutes;
        var replacers = new List<string> { Mathf.Max(expiresAt, 0).ToString() };
        Managers.Localization.FormatLocalizedText(text, key, placeholderKeys, replacers);
    }

    private async Task OnAcceptClicked()
    {
        GetButton((int)Buttons.AcceptButton).interactable = false;
        GetButton((int)Buttons.DenyButton).interactable = false;
        await _lobbyVm.AcceptInvitation(MailInfo, true);
    }

    private async Task OnDenyClicked()
    {
        GetButton((int)Buttons.AcceptButton).interactable = false;
        GetButton((int)Buttons.DenyButton).interactable = false;
        await _lobbyVm.AcceptInvitation(MailInfo, false);
    }
    
    private void OnDestroy()
    {
        GetButton((int)Buttons.AcceptButton).onClick.RemoveAllListeners();
        GetButton((int)Buttons.DenyButton).onClick.RemoveAllListeners();
    }
}
