using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UI_Colleague : MonoBehaviour
{
    protected UI_Game UI;
    protected UI_Mediator Mediator;
    
    protected virtual void Start()
    {
        GameObject ui = GameObject.FindWithTag("UI");
        UI = ui.GetComponent<UI_GameSingleWay>();
        Mediator = ui.GetComponent<UI_Mediator>();
        Mediator.AddToUIList(this);
    }

    public virtual void SetUI(GameObject go)
    {
        
    }
}
