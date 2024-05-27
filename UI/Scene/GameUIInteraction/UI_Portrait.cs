using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;
using Enum = System.Enum;

public class UI_Portrait : UI_PortraitColleague
{
    public UnitId UnitId { get; set; }
    private bool _isActive;
    
    public bool IsActive
    {
        get => _isActive;
        set
        {
            _isActive = value;
            ActivePortrait(_isActive);
        }
    }

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
    
    private void ActivePortrait(bool active)
    {
        gameObject.TryGetComponent(out UI_Portrait uiPortrait);
        var level = (int)uiPortrait.UnitId % 3;
        if (level == 0) level = 3;
        gameObject.SetActive(false);

        switch (level.ToString())
        {
            case "1":
                gameObject.SetActive(true);
                var img = gameObject.GetComponent<Image>();
                img.color = active == false 
                    ? new Color(img.color.r, img.color.g, img.color.b, 0.6f) 
                    : new Color(img.color.r, img.color.g, img.color.b, 1.0f);
                break;
            
            default:
                if (active) gameObject.SetActive(true);
                break;
        }
    }
}
