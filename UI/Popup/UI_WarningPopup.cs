using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_WarningPopup : UI_Popup
{
    public void SetWarning(string warningText)
    {
        TextMeshProUGUI text = Util.FindChild<TextMeshProUGUI>(gameObject, "WarningText", true);
        text.text = warningText;
        Managers.UI.ClosePopupUI(this, 2);
    }
}
