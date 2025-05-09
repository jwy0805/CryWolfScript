using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* Last Modified : 25. 04. 22
 * Version : 1.02
 */
public class Card : MonoBehaviour, IAsset, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect _scrollRect;
    private Vector2 _pointerDownPosition;
    // private CanvasGroup _canvasGroup;
    
    public int Id { get; set; }
    public UnitClass Class { get; set; }
    public Asset AssetType { get; set; }
    public bool IsDragging { get; set; }

    private void Start()
    {
        if (Managers.Scene.CurrentScene.GetType() != typeof(MainLobbyScene)) return;
        _scrollRect = GetComponentInParent<ScrollRect>();
    }

    public void SetLocalScale(float proportion, bool activeText = true)
    {
        Util.FindChild<TextMeshProUGUI>(gameObject, "UnitNameText").gameObject.SetActive(activeText);
        transform.localScale = new Vector3(proportion, proportion, 1);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (_scrollRect == null) return;
        IsDragging = true;
        _scrollRect.OnBeginDrag(eventData);
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (_scrollRect == null) return;
        _scrollRect.OnDrag(eventData);
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_scrollRect == null) return;
        _scrollRect.OnEndDrag(eventData);
        IsDragging = false;
    }
}
