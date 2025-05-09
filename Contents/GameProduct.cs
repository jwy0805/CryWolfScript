using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* Last Modified : 25. 04. 22
 * Version : 1.02
 */
public class GameProduct : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect _scrollRect;
    
    public ProductInfo ProductInfo { get; set; }
    public bool IsDragging { get; set; }

    public void Init()
    {
        var panel = GameObject.Find("ShopPanel");
        _scrollRect = panel.GetComponentInChildren<ScrollRect>();
        
        if (_scrollRect == null) Debug.Log("ScrollRect is null");
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        IsDragging = true;
        _scrollRect.OnBeginDrag(eventData);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        _scrollRect.OnDrag(eventData);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        _scrollRect.OnEndDrag(eventData);
        IsDragging = false;
    }
}
