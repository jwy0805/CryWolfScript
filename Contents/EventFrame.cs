using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

public class EventFrame : UI_Base
{
    private const int PreviewLength = 18;

    public EventInfo EventInfo { get; set; }
    
    protected override async void Init()
    {
        try
        {
            InitButtonEvents();
            await InitUIAsync();
        }
        catch (Exception e)
        {
            Debug.LogWarning(e);
        }
    }

    protected override async Task InitUIAsync()
    {
        var contentText = Util.FindChild(gameObject, "EventContentText").GetComponent<TextMeshProUGUI>();
        var titleText = Util.FindChild(gameObject, "EventTitleText").GetComponent<TextMeshProUGUI>();
        var contentTask = Managers.Localization.UpdateFont(contentText, FontType.Bold);
        var titleTask = Managers.Localization.UpdateFont(titleText, FontType.Bold);
            
        contentText.text = MakePreview(EventInfo.Content, PreviewLength); 

        await Task.WhenAll(contentTask, titleTask);
    }

    protected override void InitButtonEvents()
    {
        gameObject.BindEvent(ShowDetailPopup);
    }
    
    private string MakePreview(string text, int maxLength)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            return text;

        return text[..maxLength] + "...";
    }

    private async Task ShowDetailPopup(PointerEventData data)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_EventPopup>();
        popup.EventInfo = EventInfo;
    }
}
