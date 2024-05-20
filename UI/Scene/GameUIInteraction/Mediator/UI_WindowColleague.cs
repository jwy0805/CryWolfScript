using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_WindowColleague : UI_Colleague
{
    private RectTransform _rect;
    protected override void Start()
    {
        base.Start();
        Mediator.AddToWindowList(this);
        _rect = Util.FindChild(UI.gameObject, "CameraButton", true).GetComponent<RectTransform>();
    }

    public virtual void SetWindow(GameObject go)
    {
        if (go == null) /* CurrentWindow == null */
        {
            if (Mediator.PreWindow == null) return;
            Mediator.PreWindow.SetActive(false);
            LowerCameraButton();
            return;
        }

        UpperCameraButton();
        gameObject.SetActive(go.name == gameObject.name);
    }

    private void LowerCameraButton()
    {
        _rect.anchorMin = new Vector2(0.86f, 0.115f);
        _rect.anchorMax = new Vector2(0.97f, 0.156f);
    }

    private void UpperCameraButton()
    {
        _rect.anchorMin = new Vector2(0.86f, 0.315f);
        _rect.anchorMax = new Vector2(0.97f, 0.356f);  
    }
}
