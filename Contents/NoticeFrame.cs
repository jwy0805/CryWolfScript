using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class NoticeFrame : MonoBehaviour
{
    private const int PreviewLength = 18;

    public NoticeInfo NoticeInfo { get; set; }

    private async void Start()
    {
        try
        {
            var contentText = Util.FindChild(gameObject, "NoticeContentText").GetComponent<TextMeshProUGUI>();
            var titleText = Util.FindChild(gameObject, "NoticeTitleText").GetComponent<TextMeshProUGUI>();
            var contentTask = Managers.Localization.UpdateFont(contentText, FontType.Bold);
            var titleTask = Managers.Localization.UpdateFont(titleText, FontType.Bold);
            
            contentText.text = MakePreview(NoticeInfo.Content, PreviewLength); 

            await Task.WhenAll(contentTask, titleTask);

            gameObject.BindEvent(ShowDetailPopup);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private string MakePreview(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "...";
    }

    private async Task ShowDetailPopup(PointerEventData data)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NoticePopup>();
        popup.TitleText   = NoticeInfo.Title;
        popup.ContentText = NoticeInfo.Content;
    }
}
