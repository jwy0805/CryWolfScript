using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class UI_MailBoxPopup : UI_Popup
{
    private IUserService _userService;
    private IWebService _webService;
    private ITokenService _tokenService;
    private MainLobbyViewModel _lobbyVm;
    private ShopViewModel _shopVm;
    
    private readonly Dictionary<string, GameObject> _textDict = new();
    
    private enum Images
    {
        NoMailBackground,
    }

    private enum Buttons
    {   
        DeleteReadButton,
        ClaimAllButton,
        ExitButton,
    }

    private enum Texts
    {
        MailBoxTitleText,
        MailBoxNoMailText,
        MailBoxDeleteReadText,
        MailBoxClaimAllText,
    }

    [Inject]
    public void Construct(
        IUserService userService,
        IWebService webService, 
        ITokenService tokenService, 
        MainLobbyViewModel lobbyViewModel, 
        ShopViewModel shopViewModel)
    {
        _userService = userService;
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
        BindData<TextMeshProUGUI>(typeof(Texts), _textDict);
        Bind<Button>(typeof(Buttons));
        Bind<Image>(typeof(Images));
        
        _ = Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
    }

    protected override async void InitUI()
    {
        try
        {
            GetImage((int)Images.NoMailBackground).gameObject.SetActive(false);

            var mailTask = await _lobbyVm.GetMailList();
            var mailList = mailTask.OrderBy(mail => mail.Type).ToList();
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
                    await SetMailFrameUI(mail, parent);
                }
            }
        }
        catch (Exception e)
        {
            await Managers.UI.ShowErrorPopup(e.ToString(), Managers.UI.CloseAllPopupUI);
        }
    }

    private async Task SetMailFrameUI(MailInfo mail, Transform frameParent = null)
    {
        var mailInfo = new MailInfo
        {
            MailId = mail.MailId,
            Type = mail.Type,
            SentAt = mail.SentAt,
            ExpiresAt = mail.ExpiresAt,
            Claimed = mail.Claimed,
            Message = mail.Message,
            ProductId = mail.ProductId,
            ProductCategory = mail.ProductCategory,
            Sender = mail.Sender
        };

        GameObject mailFrame;
        switch (mailInfo.Type)
        {
            case MailType.Notice:
                mailFrame = await Managers.Resource.Instantiate("UI/Deck/MailInfoNotification", frameParent);
                var mailInfoNotification = mailFrame.GetOrAddComponent<MailInfoNotification>();
                mailInfoNotification.MailInfo = mailInfo;
                break;
            case MailType.Invite:
                mailFrame = await Managers.Resource.Instantiate("UI/Deck/MailInfoInvitation", frameParent);
                var mailInfoInvitation = mailFrame.GetOrAddComponent<MailInfoInvitation>();
                mailInfoInvitation.MailInfo = mailInfo;
                break;
            case MailType.Product:
                mailFrame = await Managers.Resource.GetProductMailFrame(mailInfo, frameParent);
                var mailInfoProduct = mailFrame.GetOrAddComponent<MailInfoProduct>();
                mailInfoProduct.MailInfo = mailInfo;
                await SetMailIcon(mailFrame, mailInfo);
                break;
            default: return;
        }
        
        Util.InjectGameObject(mailFrame);
    }

    private async Task SetMailIcon(GameObject mailFrame, MailInfo mailInfo)
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
                icon = await Managers.Resource.Instantiate(iconPath, iconParent);
                iconRect = icon.GetComponent<RectTransform>();
                break;
            
            case ProductCategory.ReservedSale:
                iconPath = $"UI/Shop/NormalizedProducts/RainbowEgg";
                icon = await Managers.Resource.Instantiate(iconPath, iconParent);
                iconRect = icon.GetComponent<RectTransform>();
                break;
            
            case ProductCategory.None:
            case ProductCategory.GoldStore:
            case ProductCategory.SpinelStore:
            case ProductCategory.GoldPackage:
            case ProductCategory.SpinelPackage:
            case ProductCategory.DailyDeal:
            case ProductCategory.Other:
            default:
                iconPath = $"UI/Shop/NormalizedProducts/{(ProductId)mailInfo.ProductId}";
                icon = await Managers.Resource.Instantiate(iconPath, iconParent);
                iconRect = icon.GetComponent<RectTransform>();
                break;
        }
        
        iconRect.anchorMin = Vector2.zero;
        iconRect.anchorMax = Vector2.one;
        
        var parentRect = iconParent.GetComponent<RectTransform>();
        var parentWidth = parentRect.rect.width;
        var scaleParam = parentWidth / iconRect.rect.width;
        iconRect.localScale = new Vector3(scaleParam, scaleParam, 1);
    }
    
    private async void OnProductClaim(PointerEventData data)
    {
        try
        {
            await ClaimMail(data.pointerPress);
            // Showing Item Popup
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }

    private async Task ClaimMail(GameObject mailObject)
    {
        mailObject.TryGetComponent(out Button claimButton);
        if (claimButton.interactable == false) return;
        claimButton.interactable = false;
        
        var packet = new ClaimMailPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            // MailId = mailObject.GetComponent<Mail>().MailId,
        };
        
        await _webService.SendWebRequestAsync<ClaimMailPacketResponse>("Mail/ClaimMail", "PUT", packet);
        _ =  _lobbyVm.InitMailAlert();
    }
}
