using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UI_Game
{
    #region Enum

    private enum CommonButtons
    {
        CapacityButton,
        ResourceButton,
        SubResourceButton,
        MenuButton,
        MenuEmotionButton,
        MenuChatButton,
        MenuOptionButton,
        MenuExitButton,
        CameraButton,
        UpgradeButton,
    }
    private enum CommonImages
    {
        UnitPanel0,
        UnitPanel1,
        UnitPanel2,
        UnitPanel3,
        UnitPanel4,
        UnitPanel5,
        
        SkillPanel,
        SkillWindow,
        CapacityWindow,
        SubResourceWindow,
        UnitControlWindow,
        
        MenuItemPanel,
    }
    private enum CommonTexts
    {
        CurrentName,
        CurrentPercent,
        
        ResourceText,
        SubResourceText,
        NorthCapacityText,
        SouthCapacityText,
    }

    private enum UnitButtons
    {
        NorthUnitButton0,
        NorthUnitButton1,
        NorthUnitButton2,
        NorthUnitButton3,
        NorthUnitButton4,
        NorthUnitButton5,
        NorthUnitButton6,
        NorthUnitButton7,
        NorthUnitButton8,
        NorthUnitButton9,
        NorthUnitButton10,
        NorthUnitButton11,
        SouthUnitButton0,
        SouthUnitButton1,
        SouthUnitButton2,
        SouthUnitButton3,
        SouthUnitButton4,
        SouthUnitButton5,
        SouthUnitButton6,
        SouthUnitButton7,
        SouthUnitButton8,
        SouthUnitButton9,
        SouthUnitButton10,
        SouthUnitButton11,
    }

    private enum UnitControlButtons
    {
        UnitUpgradeButton,
        UnitMoveButton,
        UnitInactivateButton,
        UnitDeleteButton
    }
    
    #endregion
}
