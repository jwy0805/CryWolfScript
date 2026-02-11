using System;
using System.Globalization;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UI;

public class UIFactory : IUIFactory
{
    public Image GetFrameFromCardButton(ISkillButton button)
    {
        if (button is not MonoBehaviour mono) return null;
        var go = mono.gameObject;
        return go.transform.parent.parent.GetChild(1).GetComponent<Image>();
    }

    public async Task<GameObject> GetFriendFrame(FriendUserInfo friendInfo, Transform parent, Action<PointerEventData> action = null)
    {
        var frame = await Managers.Resource.Instantiate("UI/Lobby/Friend/FriendInfo", parent);
        var nameText = Util.FindChild(frame, "NameText", true).GetComponent<TextMeshProUGUI>();
        var rankText = Util.FindChild(frame, "RankText", true).GetComponent<TextMeshProUGUI>();
        var actText = Util.FindChild(frame, "UserActText", true).GetComponent<TextMeshProUGUI>();
        
        nameText.text = $"{friendInfo.UserName} #{friendInfo.UserTag}";
        rankText.text = friendInfo.RankPoint.ToString();
        actText.text = friendInfo.Act.ToString();
        frame.GetOrAddComponent<Friend>().FriendTag = friendInfo.UserTag;
        frame.BindEvent(action);
        
        return frame;
    }

    public async Task<GameObject> GetFriendInviteFrame(
        FriendUserInfo friendInfo,
        Transform parent,
        Action<PointerEventData> action = null)
    {
        var frame = await Managers.Resource.Instantiate("UI/Lobby/Friend/FriendInviteFrame", parent);
        var nameText = Util.FindChild(frame, "NameText", true).GetComponent<TextMeshProUGUI>();
        var rankText = Util.FindChild(frame, "RankText", true).GetComponent<TextMeshProUGUI>();
        var actText = Util.FindChild(frame, "UserActText", true).GetComponent<TextMeshProUGUI>();
        
        nameText.text = $"{friendInfo.UserName} #{friendInfo.UserTag}";
        rankText.text = friendInfo.RankPoint.ToString();
        actText.text = friendInfo.Act.ToString();
        frame.GetOrAddComponent<Friend>().FriendTag = friendInfo.UserTag;
        frame.BindEvent(action);
        
        return frame;
    }
    
    public async Task<GameObject> GetFriendRequestFrame(
        FriendUserInfo friendInfo, 
        Transform parent, 
        Action<PointerEventData> action = null)
    {
        var frame = await Managers.Resource.Instantiate("UI/Lobby/Friend/FriendRequestInfo", parent);
        var nameText = Util.FindChild(frame, "NameText", true).GetComponent<TextMeshProUGUI>();
        var rankText = Util.FindChild(frame, "RankText", true).GetComponent<TextMeshProUGUI>();
        
        nameText.text = $"{friendInfo.UserName} #{friendInfo.UserTag}";
        rankText.text = friendInfo.RankPoint.ToString();
        frame.GetOrAddComponent<Friend>().FriendTag = friendInfo.UserTag;
        frame.BindEvent(action);
        
        return frame;
    }

    public async Task<GameObject> GetProductMailFrame(MailInfo mailInfo, Transform parent)
    {
        var frame = await Managers.Resource.Instantiate("UI/Lobby/Mail/MailInfoProduct", parent);
        var claimButton = Util.FindChild(frame, "ClaimButton", true);
        var countText = Util.FindChild(frame, "CountText", true).GetComponent<TextMeshProUGUI>();
        var infoText = Util.FindChild(frame, "InfoText", true).GetComponent<TextMeshProUGUI>();
        var expiresText = Util.FindChild(frame, "ExpiresText", true).GetComponent<TextMeshProUGUI>();  
        
        frame.GetOrAddComponent<MailInfoProduct>().MailInfo = mailInfo;
        countText.gameObject.SetActive(false);
        infoText.text = mailInfo.Message;
        expiresText.text = mailInfo.ExpiresAt.ToString(CultureInfo.CurrentCulture);

        return frame;
    }

    public async Task<GameObject> GetNoticeFrame(NoticeInfo noticeInfo, Transform parent)
    {
        var frame = await Managers.Resource.Instantiate("UI/Lobby/Notice/NoticeFrame", parent);
        var titleText = Util.FindChild(frame, "NoticeTitleText", true).GetComponent<TextMeshProUGUI>();
        var infoText = Util.FindChild(frame, "NoticeContentText", true).GetComponent<TextMeshProUGUI>();

        frame.GetOrAddComponent<NoticeFrame>().NoticeInfo = noticeInfo;
        titleText.text = noticeInfo.Title;
        infoText.text = noticeInfo.Content;

        return frame;
    }
    
    public RenderTexture CreateRenderTexture(string textureName)
    {  
        var rt = new RenderTexture(256, 256, 0)
        {
            useMipMap = false,
            autoGenerateMips = false,
            antiAliasing = 1,
            name = textureName,
            graphicsFormat = GraphicsFormat.R8G8B8A8_SRGB,
            depthStencilFormat = GraphicsFormat.D16_UNorm,
        };
        
        rt.Create();
        
        return rt;
    }
}
