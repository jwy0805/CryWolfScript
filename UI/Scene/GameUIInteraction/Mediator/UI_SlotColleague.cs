using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class UI_SlotColleague : UI_Colleague
{
    public readonly List<int> ObjectIdList = new();

    private int _objectId;
    
    public bool Selected { get; set; }
    public GameObject Go { get; private set; } /* Camera Switching에 사용됨, UI 상호작용에만 사용 */
    
    public int ObjectId
    {
        get => _objectId;
        set
        {
            _objectId = value;
            ObjectIdList.Add(_objectId);
        }
    }
    
    protected override void Start()
    {
        base.Start();
        Mediator.AddToSlotList(this);
    }
    
    public void SetSlot(GameObject go)
    {
        if (go == null) return;
        if (name != go.name) return;
        if (ObjectIdList.Count == 0) return;

        Selected = !Selected;
        int index = Random.Range(0, ObjectIdList.Count);
        Go = Managers.Object.FindById(ObjectIdList[index]);
        var cc = Go.GetComponent<CreatureController>();
        var ui = GameObject.FindWithTag("UI").GetComponent<UI_Game>();
        ui.Player.SelectedUnitId = cc.Id;

        Mediator.CurrentSelectedUnit = Go;
        Mediator.CurrentWindow = Mediator.WindowDictionary["UnitControlWindow"];
    }
}
