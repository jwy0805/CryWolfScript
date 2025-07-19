using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

public class MailInfoProduct : UI_Base
{
    private ShopViewModel _shopVm;
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    public MailInfo MailInfo { get; set; }
    
    private enum Texts
    {
        InfoText,
        ExpiresText,
        ClaimText,
    }

    private enum Buttons
    {
        ClaimButton,
    }
    
    [Inject]
    public void Construct(ShopViewModel shopViewModel)
    {
        _shopVm = shopViewModel;
    }
    
    protected override async void Init()
    {
        try
        {
            BindObjects();
            InitButtonEvents();
            await InitUIAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override void BindObjects()
    {
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));

        var claimText = GetText((int)Texts.ClaimText);
        _ = Managers.Localization.BindLocalizedText(claimText, "claim_text");
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ClaimButton).onClick.AddListener( () => _ =  OnClaimClicked());
    }

    protected override async Task InitUIAsync()
    {
        GetButton((int)Buttons.ClaimButton).interactable = !MailInfo.Claimed;
        
        var infoText = GetText((int)Texts.InfoText);
        await Managers.Localization.UpdateFont(infoText);
        var productKey = ((ProductId)MailInfo.ProductId).ToString();
        await Managers.Localization.BindLocalizedText(infoText, productKey);
        
        var expiresText = GetText((int)Texts.ExpiresText);
        var key = "mail_expires_text";
        var placeholderKeys = new List<string> { "value" };
        var expiresAt = (MailInfo.ExpiresAt.Date - DateTime.Today).Days;
        var replacers = new List<string> { Mathf.Max(expiresAt, 0).ToString() };
        await Managers.Localization.FormatLocalizedText(expiresText, key, placeholderKeys, replacers);
    }

    private async Task OnClaimClicked()
    {
        GetButton((int)Buttons.ClaimButton).interactable = false;
        await _shopVm.ClaimProductFromMailbox(false, MailInfo);
        GetButton((int)Buttons.ClaimButton).interactable = true;
    }

    private void OnDestroy()
    {
        GetButton((int)Buttons.ClaimButton).onClick.RemoveAllListeners();
    }
}
