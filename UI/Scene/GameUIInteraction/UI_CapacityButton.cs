using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

public interface IRequiresGameViewModel
{
    void SetGameViewModel(GameViewModel gameViewModel);
}

public class UI_CapacityButton : 
    MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IRequiresGameViewModel  
{
    private GameViewModel _gameVm;

    private bool _isHovering;
    private int _index;
    private Vector3 _originalPanelPos;
    private Image _targetImage;
    private GraphicRaycaster _raycaster;
    private PointerEventData _pointerEventData;
    private EventSystem _eventSystem;
    
    public void SetGameViewModel(GameViewModel gameViewModel)
    {
        _gameVm = gameViewModel;
    }

    private void Start()
    {
        _raycaster = FindObjectOfType<GraphicRaycaster>();
        _eventSystem = FindObjectOfType<EventSystem>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _targetImage = _gameVm.DeleteImage.GetComponent<Image>();
        _gameVm.OnSlotDrag = true;
        _gameVm.SetDeleteImageOnWindow();
        _isHovering = false;
        _originalPanelPos = transform.parent.parent.position;
        
        var index = _gameVm.GetButtonIndex(gameObject);
        if (index != null) _index = index.Value;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.parent.parent.position = Input.mousePosition;
        _pointerEventData = new PointerEventData(_eventSystem) { position = Input.mousePosition };
        var results = new List<RaycastResult>();
        _raycaster.Raycast(_pointerEventData, results);
        var hoveringNow = results.Any(result => result.gameObject == _targetImage.gameObject);
        
        if (hoveringNow && _isHovering == false)
        {
            _isHovering = true;
            _gameVm.HighlightDeleteImageOnWindow();
        }
        else if (hoveringNow == false && _isHovering)
        {
            _isHovering = false;
            _gameVm.RestoreDeleteImageOnWindow();
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.parent.parent.position = _originalPanelPos;
        
        _gameVm.RestoreDeleteImageOnWindow();
        _gameVm.DisappearDeleteImageOnWindow();
        
        if (_isHovering)
        {
            _gameVm.TurnOffOneSelectRing(_index);
        }
        
        _gameVm.OnSlotDrag = false;
    }
}
