using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_WarningPopup : UI_Popup
{
    public string Text { get; set; }
    public TMP_FontAsset Font { get; set; }
    public int FontSize { get; set; }
    public int DelayTime { get; set; } = 3;

    protected override void Init()
    {
        base.Init();
        
        var text = Util.FindChild<TextMeshProUGUI>(gameObject, "WarningText", true);
        text.text = Text;
        text.font = Font;
        text.fontSize = FontSize;
        
        Managers.UI.ClosePopupUI(this, DelayTime);
    }
}
