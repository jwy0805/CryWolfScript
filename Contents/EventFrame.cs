using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class EventFrame : MonoBehaviour
{
    private const int PreviewLength = 18;

    public EventInfo EventInfo { get; set; }
    
    private async void Start()
    {
        try
        {
            var contentText = Util.FindChild(gameObject, "EventContentText").GetComponent<TextMeshProUGUI>();
            var titleText = Util.FindChild(gameObject, "EventTitleText").GetComponent<TextMeshProUGUI>();
            var contentTask = Managers.Localization.UpdateFont(contentText, FontType.Bold);
            var titleTask = Managers.Localization.UpdateFont(titleText, FontType.Bold);
            
            contentText.text = MakePreview(EventInfo.NoticeInfo.Content, PreviewLength); 

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
        var popup = await Managers.UI.ShowPopupUI<UI_EventPopup>();
        popup.TitleText   = EventInfo.NoticeInfo.Title;
        popup.ContentText = EventInfo.NoticeInfo.Content;
    }
}
