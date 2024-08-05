using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public partial class UI_MainLobby : IPointerClickHandler, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("click");
        if (CardPopup != null) Managers.UI.ClosePopupUI(CardPopup);
    }

    public void OnDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        throw new System.NotImplementedException();
    }

    public void OnEndDrag(PointerEventData eventData)
    { 
        Debug.Log("end drag");
    }
}
