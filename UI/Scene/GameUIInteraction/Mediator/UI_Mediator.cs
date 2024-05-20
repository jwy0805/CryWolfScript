using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Mediator : MonoBehaviour
{
    public Dictionary<string, GameObject> WindowDictionary = new ();
    public readonly List<UI_SlotColleague> SlotList = new();
    
    private readonly List<UI_Colleague> _uiList = new();
    private readonly List<UI_WindowColleague> _windowList = new();
    private readonly List<UI_ButtonColleague> _buttonList = new();
    private readonly List<UI_PortraitColleague> _portraitList = new();
    
    private GameObject _currentSelectedObject;
    private GameObject _currentSelectedUnit;
    private GameObject _currentSelectedButton;
    private GameObject _currentSelectedPortrait;
    private GameObject _currentWindow;
    private GameObject _currentSelectedSkill;
    private GameObject _currentSelectedSlot;

    public GameObject CurrentSelectedObject
    {
        get => _currentSelectedObject;
        set
        {
            if (_currentSelectedObject != null) PreSelectedObject = _currentSelectedObject;
            _currentSelectedObject = value;

            if (PreSelectedObject == _currentSelectedObject)
            {
                if (_currentSelectedObject.layer != LayerMask.NameToLayer("Ground")) PressedTwice = !PressedTwice;
            }
            else
            {
                if (PressedTwice) PressedTwice = false;
            }

            if (DraggingObject != null) PressedTwice = true;
        }
    }

    public GameObject CurrentSelectedUnit
    {
        get => _currentSelectedUnit;
        set
        {
            _currentSelectedUnit = value;
            CurrentSelectedObject = _currentSelectedUnit;
        }
    }
    
    public GameObject CurrentSelectedButton
    {
        get => _currentSelectedButton;
        set
        {
            if (_currentSelectedButton != null) PreSelectedButton = _currentSelectedButton;
            _currentSelectedButton = value;
            CurrentSelectedObject = _currentSelectedButton;
            SetButton(_currentSelectedButton);
        }
    }
    
    public GameObject CurrentWindow 
    { 
        get => _currentWindow;
        set
        {
            if (_currentWindow != null) PreWindow = _currentWindow;
            _currentWindow = value;
            SetWindow(_currentWindow);
        }
    }
    
    public GameObject CurrentSelectedPortrait
    {
        get => _currentSelectedPortrait;
        set
        {
            if (_currentSelectedPortrait != null) PreSelectedPortrait = _currentSelectedPortrait;
            _currentSelectedPortrait = value;
            CurrentSelectedObject = _currentSelectedPortrait;
            SetPortrait(_currentSelectedObject);
        }
    }
    
    public GameObject CurrentSelectedSkill
    {
        get => _currentSelectedSkill;
        set
        {
            if (_currentSelectedSkill == value) return;
            if (_currentSelectedSkill != null) PreSelectedSkill = _currentSelectedSkill;
            _currentSelectedSkill = value;
            CurrentSelectedObject = _currentSelectedSkill;
            SetButton(_currentSelectedSkill);
        }
    }
    
    public GameObject CurrentSelectedSlot
    {
        get => _currentSelectedSlot;
        set
        {
            if (_currentSelectedSlot != null) PreSelectedSlot = _currentSelectedSlot;
            _currentSelectedSlot = value;
            CurrentSelectedObject = _currentSelectedSlot;
            SetSlotUI(_currentSelectedSlot);
        }
    }
    
    public GameObject DraggingObject { get; set; }
    public GameObject PreSelectedObject { get; private set; }
    public GameObject PreSelectedButton { get; private set; }
    public GameObject PreWindow { get; private set; }
    public GameObject PreSelectedPortrait { get; private set; }
    public GameObject PreSelectedSkill { get; private set; }
    public GameObject PreSelectedSlot { get; private set; }
    public bool PressedTwice { get; set; }

    public void InitState(GameObject go = null)
    {
        PressedTwice = false;
        CurrentSelectedObject = go;
        CurrentSelectedPortrait = null;
        CurrentSelectedButton = null;
    }
    
    public void AddToUIList(UI_Colleague ui)
    {
        _uiList.Add(ui);
    }
    
    public void AddToWindowList(UI_WindowColleague window)
    {
        _windowList.Add(window);
        var go = window.gameObject;
        WindowDictionary.Add(go.name, go);
    }
    
    public void AddToButtonList(UI_ButtonColleague button)
    {
        _buttonList.Add(button);
    }
    
    public void AddToSlotList(UI_SlotColleague slot)
    {
        SlotList.Add(slot);
    }
    
    public void AddToPortraitList(UI_PortraitColleague portrait)
    {
        _portraitList.Add(portrait);
    }

    public UI_SlotColleague FindSlotById(int objectId)
    {
        return SlotList.FirstOrDefault(slot => slot.ObjectIdList.Contains(objectId));
    }
    
    private void SetWindow(GameObject go)
    {
        if (!_windowList.Any()) return;
        foreach (var colleague in _windowList)
        {
            colleague.SetWindow(go);
        }
    }

    private void SetButton(GameObject go)
    {
        if (!_buttonList.Any()) return;
        foreach (var colleague in _buttonList)
        {
            colleague.SetButton(go);
        }
    }

    private void SetSlotUI(GameObject go)
    {
        if (!SlotList.Any()) return;
        foreach (var colleague in SlotList)
        {
            colleague.SetSlot(go);
        }
    }
    
    private void SetPortrait(GameObject go)
    {
        if (!_portraitList.Any()) return;
        foreach (var colleague in _portraitList)
        {
            colleague.SetPortrait(go);
        }
    }
}
