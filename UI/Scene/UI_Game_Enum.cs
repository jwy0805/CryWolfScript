using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class UI_Game
{
    #region Enum

    protected enum CommonButtonsD
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
    protected enum CommonImagesD
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
    protected enum CommonTextsD
    {
        CurrentName,
        CurrentPercent,
        
        ResourceText,
        SubResourceText,
        NorthCapacityText,
        SouthCapacityText,
    }

    protected enum UnitButtonsD
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

    protected enum UnitControlButtonsD
    {
        UnitUpgradeButton,
        UnitMoveButton,
        UnitInactivateButton,
        UnitDeleteButton
    }   
    
    protected enum CommonButtonsS
    {
        CapacityButton,
        ResourceButton,
        SubResourceButton,
        MenuButton,
        CameraButton,
        UpgradeButton,
    }
    protected enum CommonImagesS
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
    }
    protected enum CommonTextsS
    {
        CurrentName,
        CurrentPercent,
        
        ResourceText,
        SubResourceText,
        NorthCapacityText,
    }

    protected enum UnitButtonsS
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
    }

    protected enum UnitControlButtonsS
    {
        UnitUpgradeButton,
        UnitDeleteButton
    }
    
    #endregion
}
