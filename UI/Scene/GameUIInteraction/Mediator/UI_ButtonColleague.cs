using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_ButtonColleague : UI_Colleague
{
    protected override void Start()
    {
        base.Start();
        Mediator.AddToButtonList(this);
    }
    
    public virtual void SetButton(GameObject obj)
    {
        
    }
}
