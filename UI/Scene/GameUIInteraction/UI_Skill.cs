using System;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public class UI_Skill : UI_ButtonColleague
{
    public override void SetButton(GameObject go)
    {
        if (go == null) return;
        if (gameObject.name != go.name) return;

        if (Mediator.CurrentSelectedSkill == go)
        {
            UI_Popup deletePopup = null;
            foreach (var uiPopup in Managers.UI.PopupList.Where(popup => popup is UI_UpgradePopup))
            {
                deletePopup = uiPopup;
            }
            
            if (deletePopup != null) Managers.UI.ClosePopupUI(deletePopup);

            Image currentFrame = Mediator.CurrentSelectedSkill.transform.parent.parent.GetChild(1).GetComponent<Image>();
            currentFrame.color = Color.cyan;
            Managers.UI.ShowPopupUI<UI_UpgradePopup>();
            
            var skillName = gameObject.name.Replace("Button", "");
            if (Enum.TryParse(skillName, out Skill skill))
            {
                C_SetUpgradePopup popupPacket = new() { SkillId = (int)skill };
                Managers.Network.Send(popupPacket);
            }

            if (Mediator.PreSelectedSkill == null) return;
            Image preFrame = Mediator.PreSelectedSkill.transform.parent.parent.GetChild(1).GetComponent<Image>();
            preFrame.color = Color.green;
        }
    }
}
