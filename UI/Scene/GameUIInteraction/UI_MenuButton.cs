using UnityEngine;

public class UI_MenuButton : UI_ButtonColleague
{
    public override void SetUI(GameObject go)
    {
        if (go == null) return;
        if (gameObject.name != go.name) return;
        UI.SetMenu(go.name == "MenuButton");
    }
}
