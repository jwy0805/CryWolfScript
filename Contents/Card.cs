using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* Last Modified : 24. 10. 09
 * Version : 1.014
 */

public class Card : MonoBehaviour, IAsset, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect _scrollRect;
    
    public int Id { get; set; }
    public UnitClass Class { get; set; }
    public Asset AssetType { get; set; }
    public bool IsDragging { get; set; }

    private void Start()
    {
        if (Managers.Scene.CurrentScene.GetType() != typeof(MainLobbyScene)) return;
        
        var panel = GameObject.Find("DeckCollectionPanel");
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
