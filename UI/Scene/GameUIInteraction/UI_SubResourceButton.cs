using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_SubResourceButton : UI_ButtonColleague
{
    public override void SetButton(GameObject go)
    {
        if (go == null) return;
        if (gameObject.name != go.name) return;

        Mediator.CurrentWindow = Mediator.PressedTwice == false ? Mediator.WindowDictionary["SubResourceWindow"] : null;
    }
}
