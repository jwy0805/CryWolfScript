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
        var scrollObject = GameObject.Find("CollectionScrollView");
        
        if (scrollObject != null)
        {
            _scrollRect = scrollObject.GetComponent<ScrollRect>();
        }
        else
        {
            Debug.LogError("ScrollRect object not found!");
        }    
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
