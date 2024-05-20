using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_ResultPopup : UI_Popup
{  
    public void SetPopup()
    {
        var text = Util.FindChild<TextMeshProUGUI>(gameObject, "ResultText", true);
        text.text = Managers.Game.GameResult ? "You Win !" : "You Lose !";
    }
}
