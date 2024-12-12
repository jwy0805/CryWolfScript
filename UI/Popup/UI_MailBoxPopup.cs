using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_MailBoxPopup : UI_Popup
{
    private IWebService _webService;
    private ITokenService _tokenService;
    private MainLobbyViewModel _lobbyVm;
    private ShopViewModel _shopVm;
    
    private enum Images
    {
        NoMailBackground,
    }

    private enum Buttons
    {   
        ExitButton,
    }

    private enum Texts
    {
        
    }

    [Inject]
    public void Construct(
        IWebService webService, 
        ITokenService tokenService, 
        MainLobbyViewModel lobbyViewModel, 
        ShopViewModel shopViewModel)
    {
        _webService = webService;
        _tokenService = tokenService;
        _lobbyVm = lobbyViewModel;
        _shopVm = shopViewModel;
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
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
    }

    protected override async void InitUI()
    {
        GetImage((int)Images.NoMailBackground).gameObject.SetActive(false);
        GetButton((int)Buttons.ExitButton).interactable = false;

        var mailTask = _lobbyVm.GetMailList();

        await mailTask;

        var mailList = mailTask.Result;
        var parent = Util.FindChild(gameObject, "Content", true).transform;
        Util.DestroyAllChildren(parent);

        if (mailList.Count == 0)
        {
            GetImage((int)Images.NoMailBackground).gameObject.SetActive(true);
        }
        else
        {
            foreach (var mail in mailList)
            {
                SetMailFrameUI(mail, parent);
            }
        }
    }

    private void SetMailFrameUI(MailInfo mail, Transform frameParent = null)
    {
        var mailInfo = new MailInfo
        {
            MailId = mail.MailId,
            SentAt = mail.SentAt,
            ExpiresAt = mail.ExpiresAt,
            Claimed = mail.Claimed,
            Message = mail.Message,
            ProductId = mail.ProductId,
            ProductCategory = mail.ProductCategory
        };

        var mailFrame = Managers.Resource.GetMailFrame(mailInfo, frameParent);
        var countText = Util.FindChild(mailFrame, "CountText", true).GetComponent<TextMeshProUGUI>();
        var infoText = Util.FindChild(mailFrame, "InfoText", true).GetComponent<TextMeshProUGUI>();
        var expiresText = Util.FindChild(mailFrame, "ExpiresText", true).GetComponent<TextMeshProUGUI>();
        countText.gameObject.SetActive(false);
        mailFrame.GetComponent<Mail>().MailId = mail.MailId;
        SetMailIcon(mailFrame, mailInfo);
        BindClaimButton(mailFrame, mailInfo);
    }

    private void SetMailIcon(GameObject mailFrame, MailInfo mailInfo)
    {
        var iconParent = Util.FindChild(mailFrame, "IconFrame", true).transform;
        string iconPath;
        GameObject icon;
        RectTransform iconRect;
        switch (mailInfo.ProductCategory)
        {
            case ProductCategory.SpecialPackage:
            case ProductCategory.BeginnerPackage:
                iconPath = $"UI/Shop/{(ProductId)mailInfo.ProductId}";
                icon = Managers.Resource.Instantiate(iconPath, iconParent);
                iconRect = icon.GetComponent<RectTransform>();
                var parentRect = iconParent.GetComponent<RectTransform>();
                var parentWidth = parentRect.rect.width;
                var scaleParam = parentWidth / iconRect.rect.width;
                iconRect.localScale = new Vector3(scaleParam, scaleParam, 1);
                break;
            
            case ProductCategory.ReservedSale:
                iconPath = $"UI/Shop/NormalizedProducts/RainbowEgg";
                icon = Managers.Resource.Instantiate(iconPath, iconParent);
                iconRect = icon.GetComponent<RectTransform>();
                break;
            
            case ProductCategory.None:
            case ProductCategory.GoldPackage:
            case ProductCategory.SpinelPackage:
            case ProductCategory.GoldItem:
            case ProductCategory.SpinelItem:
            case ProductCategory.DailyDeal:
            case ProductCategory.Other:
            default:
                iconPath = $"UI/Shop/NormalizedProducts/{(ProductId)mailInfo.ProductId}";
                icon = Managers.Resource.Instantiate(iconPath, iconParent);
                iconRect = icon.GetComponent<RectTransform>();
                break;
        }
        
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
    }

    private void BindClaimButton(GameObject mailFrame, MailInfo mailInfo)
    {
        var claimButton = Util.FindChild(mailFrame, "ClaimButton", true);
        claimButton.BindEvent(OnClaimClicked);
    }
    
    private async void OnClaimClicked(PointerEventData data)
    {
        data.pointerPress.gameObject.TryGetComponent(out Button claimButton);
        claimButton.interactable = false;
        
        var packet = new ClaimMailPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            MailId = data.pointerPress.GetComponent<Mail>().MailId
        };
        
        await _webService.SendWebRequestAsync<ClaimMailPacketResponse>("Mail/ClaimMail", "PUT", packet);
        _ =  _lobbyVm.InitMailAlert();
        // Showing Item Popup
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
