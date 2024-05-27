using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_Popup : UI_Base
{
    protected override void Init()
    {
        bool sort = true;
        Managers.UI.SetCanvas(gameObject, sort);
    }
}
