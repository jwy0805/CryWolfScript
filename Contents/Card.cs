using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/* Last Modified : 24. 10. 09
 * Version : 1.014
 */

public class Card : MonoBehaviour, IAsset, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private ScrollRect _scrollRect;
    private Vector2 _pointerDownPosition;
    private CanvasGroup _canvasGroup;
    
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
        
        _canvasGroup = GetComponent<CanvasGroup>();
        if (_canvasGroup == null)
            _canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void SetLocalScale(float proportion, bool activeText = true)
    {
        Util.FindChild<TextMeshProUGUI>(gameObject, "UnitNameText").gameObject.SetActive(activeText);
        transform.localScale = new Vector3(proportion, proportion, 1);
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        _pointerDownPosition = eventData.position;
        IsDragging = false;
    }

    // 드래그 시작 시 호출 (이동 거리가 임계값보다 크면 드래그로 인식)
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Vector2.Distance(eventData.position, _pointerDownPosition) > EventSystem.current.pixelDragThreshold)
        {
            IsDragging = true;
            // ScrollRect에 드래그 시작 이벤트 전달
            ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.beginDragHandler);
        }
    }

    // 드래그 중에는 ScrollRect에 계속 이벤트 전달
    public void OnDrag(PointerEventData eventData)
    {
        if (IsDragging)
        {
            ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.dragHandler);
        }
    }

    // 드래그 종료 시 ScrollRect에 종료 이벤트 전달
    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsDragging)
        {
            ExecuteEvents.Execute(_scrollRect.gameObject, eventData, ExecuteEvents.endDragHandler);
        }
    }
}
