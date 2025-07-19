using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public class LockParentScrollRect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private MainLobbyViewModel _lobbyVm;
    
    private ScrollRect _parentScroll;

    [Inject]
    public void Construct(MainLobbyViewModel lobbyViewModel)
    {
        _lobbyVm = lobbyViewModel;
    }
    
    private void Start()
    {
        _parentScroll = GameObject.Find("ScrollView").GetComponent<ScrollRect>();
        
        if (_parentScroll == null)
        {
            Debug.LogError("ScrollRect component not found in the scene.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _lobbyVm.ChildScrolling = true;
        _parentScroll.enabled = false;      
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _lobbyVm.ChildScrolling = false;
        _parentScroll.enabled = true;
    }
}
