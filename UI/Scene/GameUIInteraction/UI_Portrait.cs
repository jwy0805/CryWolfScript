using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;
using Enum = System.Enum;

public class UI_Portrait : UI_PortraitColleague
{
    public UnitId UnitId { get; set; }

    public override void SetPortrait(GameObject go)
    {
        if (go == null)
        {
            if (!gameObject.activeSelf) return;
            InitSkillPanel(gameObject);
            Mediator.CurrentWindow = null;
            return;
        }

        if (go.TryGetComponent(out UI_Portrait uiPortrait) == false) return;
        if (UnitId != uiPortrait.UnitId) return;

        if (Mediator.PreSelectedPortrait != null)
        {   /* 이미 선택된 portrait가 있으면 SkillPanel초기화 */
            InitSkillPanel(Mediator.PreSelectedPortrait);
        }
        
        if (Mediator.PressedTwice == false)
        {
            InitSkillPanel(go, true);
            Mediator.CurrentWindow = Mediator.WindowDictionary["SkillWindow"];
            UI.SetUpgradeButton(go);
        }
        else
        {
            InitSkillPanel(gameObject);
            Mediator.CurrentWindow = null;
        }
    }

    public void InitSkillPanel(GameObject go, bool active = false)
    {
        var glowObject = go.transform.parent.transform.GetChild(1).gameObject;
        glowObject.TryGetComponent(out GlowCycle glow);
        if (go.TryGetComponent(out ButtonBounce bounce) == false) return;
        glow.Selected = active;
        bounce.Selected = active;
        UI.SetSkillPanel(go, active);
    }
}
