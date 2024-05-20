using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_Dim : UI_Popup
{
    enum Images
    {
        Dim,
    }

    protected override void Init()
    {
        base.Init();

        Bind<Image>(typeof(Images));
        
        GetImage((int)Images.Dim).gameObject.BindEvent(OnDimClicked);
    }
    
    private void OnDimClicked(PointerEventData data)
    {
        Managers.UI.CloseAllPopupUI();
    }
}
