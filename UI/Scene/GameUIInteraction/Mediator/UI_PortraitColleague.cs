using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_PortraitColleague : UI_Colleague
{
    protected override void Start()
    {
        base.Start();
        Mediator.AddToPortraitList(this);
    }
    
    public virtual void SetPortrait(GameObject obj)
    {
        
    }
}
