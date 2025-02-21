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
    private ISignalRClient _signalRClient;
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
        ISignalRClient signalRClient,
        MainLobbyViewModel lobbyViewModel, 
        ShopViewModel shopViewModel)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
        _signalRClient = signalRClient;
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
        
        Managers.Localization.UpdateTextAndFont(_textDict);
    }

    protected override void InitButtonEvents()
    {
        GetButton((int)Buttons.ExitButton).gameObject.BindEvent(ClosePopup);
    }

    protected override async void InitUI()
    {
        GetImage((int)Images.NoMailBackground).gameObject.SetActive(false);

        var mailTask = _lobbyVm.GetMailList();

        await mailTask;

        var mailList = mailTask.Result.OrderBy(mail => mail.Type).ToList();
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
                mailFrame = Managers.Resource.GetNotifyMailFrame(mailInfo, frameParent, OnNotifyClaim);
                break;
            case MailType.Invite:
                mailFrame = Managers.Resource.GetInviteMailFrame(mailInfo, frameParent);
                var acceptButton = Util.FindChild(mailFrame, "AcceptButton", true);
                var rejectButton = Util.FindChild(mailFrame, "DenyButton", true);
                acceptButton.BindEvent(OnInvitationAccept);
                rejectButton.BindEvent(OnInvitationReject);
                break;
            case MailType.Product:
                mailFrame = Managers.Resource.GetProductMailFrame(mailInfo, frameParent, OnProductClaim);
                SetMailIcon(mailFrame, mailInfo);
                break;
            default: return;
        }
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
            case ProductCategory.GoldStore:
            case ProductCategory.SpinelStore:
            case ProductCategory.GoldPackage:
            case ProductCategory.SpinelPackage:
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

    private async void OnNotifyClaim(PointerEventData data)
    {
        await ClaimMail(data.pointerPress);
    }

    private async void OnInvitationAccept(PointerEventData data)
    {
        var packet = new AcceptInvitationPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Accept = true,
            InviteeName = _userService.UserInfo.UserName,
        };
        await Task.WhenAll(ClaimMail(data.pointerPress), _signalRClient.SendAcceptInvitation(packet));
    }
    
    private async void OnInvitationReject(PointerEventData data)
    {
        var packet = new AcceptInvitationPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            Accept = false,
            InviteeName = _userService.UserInfo.UserName,
        };
        await Task.WhenAll(ClaimMail(data.pointerPress), _signalRClient.SendAcceptInvitation(packet));
    }
    
    private async void OnProductClaim(PointerEventData data)
    {
        await ClaimMail(data.pointerPress);
        // Showing Item Popup
    }
    
    private void ClosePopup(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }

    private async Task ClaimMail(GameObject mailObject)
    {
        mailObject.TryGetComponent(out Button claimButton);
        claimButton.interactable = false;
        
        var packet = new ClaimMailPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            MailId = mailObject.GetComponent<Mail>().MailId
        };
        
        await _webService.SendWebRequestAsync<ClaimMailPacketResponse>("Mail/ClaimMail", "PUT", packet);
        _ =  _lobbyVm.InitMailAlert();
    }
}
