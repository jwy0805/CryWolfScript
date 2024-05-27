using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_Result : UI_Scene
{
    private TextMeshProUGUI _text;
    
    protected override void Init()
    {
        base.Init();

        _text = Util.FindChild<TextMeshProUGUI>(gameObject, "ResultText", true, true);
        _text.text = Managers.Game.GameResult ? "You Win!" : "You Lose!";
    }
}
