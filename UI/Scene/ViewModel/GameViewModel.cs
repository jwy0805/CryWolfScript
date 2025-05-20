using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameViewModel : IDisposable
{
    /*
     * The principles of using events and interfaces in the ViewModel are as follows:
     * 1. Events
     *  - Events have to be used for reacting to UI events such as clicking, dragging, etc.
     * 2. Interfaces
     *  - Interfaces have to be used for data binding, initializing, and updating UI elements.
     * These principles must be observed especially managing popups. 
     */

    // Events using in UI_GameSingleWay
    public event Func<UnitId, IPortrait> SetPortraitFromFieldUnitEvent;  
    public event Action<IPortrait, bool> OnPortraitClickedEvent;   // Show Portrait Select Effect
    public event Action<int> TurnOnSelectRingCoroutineEvent;
    public event Action<int> TurnOffOneSelectRingEvent;
    public event Action TurnOffSelectRingEvent;

    // Events using in CapacityWindow
    public event Action SetDeleteImageOnWindowEvent;
    public event Action HighlightDeleteImageOnWindowEvent;
    public event Action RestoreDeleteImageOnWindowEvent;
    public event Action DisappearDeleteImageOnWindowEvent;
    
    public bool OnPortraitDrag { get; set; }
    public bool OnSlotDrag { get; set; }
    public ObservableCollection<int> SelectedObjectIds { get; set; } = new();
    public List<Skill> SkillsUpgraded { get; set; } = new();
    public GameObject DeleteImage => CapacityWindow.GetDeleteImage();
    public ISkillWindow SkillWindow { get; set; }
    public IUnitControlWindow UnitControlWindow { get; set; }
    public ICapacityWindow CapacityWindow { get; set; }
    public IBaseSkillWindow BaseSkillWindow { get; set; }
    public IPortrait CurrentSelectedPortrait { get; set; }
    public ISkillButton CurrentSelectedSkillButton { get; set; }

    public GameViewModel()
    {
        Managers.Event.StopListening("ShowRings", OnReceiveRanges);
        Managers.Event.StartListening("ShowRings", OnReceiveRanges);
        Managers.Event.StopListening("ShowBounds", OnReceiveBounds);
        Managers.Event.StartListening("ShowBounds", OnReceiveBounds);
        Managers.Event.StopListening("CanSpawn", OnCanSpawn);
        Managers.Event.StartListening("CanSpawn", OnCanSpawn);
        Managers.Event.StopListening("UpdateUpgradeCost", UpdateUnitUpgradeCostResponse);
        Managers.Event.StartListening("UpdateUnitUpgradeCost", UpdateUnitUpgradeCostResponse);
        Managers.Event.StopListening("UpdateUnitDeleteCost", UpdateUnitDeleteCostResponse);
        Managers.Event.StartListening("UpdateUnitDeleteCost", UpdateUnitDeleteCostResponse);
        Managers.Event.StopListening("UpdateUnitRepairCost", UpdateUnitRepairCostResponse);
        Managers.Event.StartListening("UpdateUnitRepairCost", UpdateUnitRepairCostResponse);
        Managers.Event.StopListening("SetBaseSkillCost", SetBaseSkillCostResponse);
        Managers.Event.StartListening("SetBaseSkillCost", SetBaseSkillCostResponse);
    }
    
    public int GetLevelFromUiObject(UnitId unitId)
    {
        var level = (int)unitId % 100 % 3;
        if (level == 0) { level = 3; }

        return level;
    }

    public void ShowUpgradePopup(string name = null)
    {
        Managers.UI.ShowPopupUiInGame<UI_UpgradePopup>();
        var skillName = name ?? CurrentSelectedSkillButton.Name.Replace("Button", "");
        
        if (Enum.TryParse(skillName, out Skill skill))
        {
            Managers.Network.Send(new C_SetUpgradePopup { SkillId = (int)skill });
        }
    }
    
    public void ShowUpgradePopupNoCost(string name = null)
    {
        Managers.UI.ShowPopupUiInGame<UI_UpgradePopupNoCost>();
        var skillName = name ?? CurrentSelectedSkillButton.Name.Replace("Button", "");
        
        if (Enum.TryParse(skillName, out Skill skill))
        {
            Managers.Network.Send(new C_SetUpgradePopup { SkillId = (int)skill });
        }
    }
    
    public void UpgradeSkill()
    {
        var skillName = CurrentSelectedSkillButton.Name.Replace("Button", "");
        if (Enum.TryParse(skillName, out Skill skill) == false) return;
        
        Managers.Network.Send(new C_SkillUpgrade{ Skill = skill });
        Managers.UI.ClosePopupUI<UI_UpgradePopup>();
        Managers.UI.ClosePopupUI<UI_UpgradePopupNoCost>();
    }

    public void UpgradeBaseSkill()
    {
        var skillName = CurrentSelectedSkillButton.Name.Replace("Button", "") switch
        {
            "BaseUpgrade" => Util.Faction == Faction.Sheep ? "BaseUpgradeSheep" : "BaseUpgradeWolf",
            "Repair" => Util.Faction == Faction.Sheep ? "RepairSheep" : "RepairWolf",
            "Resource" => Util.Faction == Faction.Sheep ? "ResourceSheep" : "ResourceWolf",
            "Asset" => Util.Faction == Faction.Sheep ? "AssetSheep" : "AssetWolf",
            _ => null
        };
        
        if (Enum.TryParse(skillName, out Skill skill) == false) return;
        
        Managers.Network.Send(new C_BaseSkillRun{ Skill = skill });
        Managers.UI.ClosePopupUI<UI_UpgradePopup>();
        Managers.UI.ClosePopupUI<UI_UpgradePopupNoCost>();
    }

    public void UpdateSkillPanel(IPortrait portrait)
    {
        portrait.UnitId += 1;
        SkillWindow?.InitUpgradeButton();
        SkillWindow?.InitUI(portrait.UnitId);
    }

    public void OnSkillUpgraded(Skill skill)
    {
        SkillsUpgraded.Add(skill);
        SkillWindow?.UpdateSkillButton();
    }

    public void UpdateUpgradeCostRequired()
    {
        Managers.Network.Send(new C_SetUpgradeButtonCost { UnitId = (int)CurrentSelectedPortrait.UnitId });
    }

    public void UpdateUpgradeCostResponse(int cost)
    {
        SkillWindow?.UpdateUpgradeCost(cost);
    }

    public void UpdateUnitUpgradeCostRequired(int[] objectIds)
    {
        var packet = new C_SetUnitUpgradeCost();
        packet.ObjectIds.AddRange(objectIds);
        Managers.Network.Send(packet);
    }
    
    private void UpdateUnitUpgradeCostResponse(object eventData)
    {
        var packet = (S_SetUnitUpgradeCost)eventData;
        CapacityWindow?.UpdateUpgradeCostText(packet.Cost);
        UnitControlWindow?.UpdateUpgradeCostText(packet.Cost);
    }
    
    public void UpdateUnitDeleteCostRequired(int[] objectIds)
    {
        var packet = new C_SetUnitDeleteCost();
        packet.ObjectIds.AddRange(objectIds);
        Managers.Network.Send(packet);
    }
    
    private void UpdateUnitDeleteCostResponse(object eventData)
    {
        var packet = (S_SetUnitDeleteCost)eventData;
        CapacityWindow?.UpdateDeleteCostText(packet.Cost);
        UnitControlWindow?.UpdateDeleteCostText(packet.Cost);
    }
    
    public void UpdateUnitRepairCostRequired(int[] objectIds)
    {
        var packet = new C_SetUnitRepairCost();
        packet.Faction = Util.Faction;
        packet.ObjectIds.AddRange(objectIds);
        Managers.Network.Send(packet);
    }
    
    private void UpdateUnitRepairCostResponse(object eventData)
    {
        var packet = (S_SetUnitRepairCost)eventData;
        CapacityWindow?.UpdateRepairCostText(packet.Cost);
        UnitControlWindow?.UpdateRepairCostText(packet.Cost);
        UnitControlWindow?.UpdateRepairAllCostText(packet.CostAll);
    }
    
    public void SetBaseSkillCostRequired()
    {
        var packet = new C_SetBaseSkillCost { Faction = Util.Faction };
        Managers.Network.Send(packet);
    }
    
    private void SetBaseSkillCostResponse(object eventData)
    {
        var packet = (S_SetBaseSkillCost)eventData;
        BaseSkillWindow?.UpdateBaseSkillCost(packet);
    }
    
    private void OnReceiveRanges(object eventData)
    {
        var packet = (S_GetRanges)eventData;
        CurrentSelectedPortrait?.ShowRing(packet.AttackRange, packet.SkillRange);
    }

    private void OnReceiveBounds(object eventData)
    {
        var packet = (S_GetSpawnableBounds)eventData;
        CurrentSelectedPortrait?.ShowSpawnableBounds(packet.MinZ, packet.MaxZ);
    }
    
    private void OnCanSpawn(object eventData)
    {
        var packet = (S_UnitSpawnPos)eventData;
        if (CurrentSelectedPortrait == null) return;
        CurrentSelectedPortrait.CanSpawn = packet.CanSpawn;
    }
    
    public void OnPortraitClicked(IPortrait portrait)
    {
        if (OnPortraitDrag) return;
        
        CancelClickedEffect();
        Managers.UI.CloseAllPopupUI();
        
        CurrentSelectedPortrait = portrait;
        Managers.UI.ShowPopupUiInGame<SkillWindow>();
        OnPortraitClickedEvent?.Invoke(portrait, true);
    }

    public void SetPortraitFromFieldUnit(UnitId unitId)
    {
        CurrentSelectedPortrait = SetPortraitFromFieldUnitEvent?.Invoke(unitId);
    }
    
    public void CancelClickedEffect()
    {
        if (CurrentSelectedPortrait != null)
        {
            OnPortraitClickedEvent?.Invoke(CurrentSelectedPortrait, false);   
        }
    }
    
    public void OnSlotClicked(int index)
    {
        if (OnSlotDrag) return;
        Managers.UI.CloseAllPopupUI();
        var window = Managers.UI.ShowPopupUiInGame<UnitControlWindow>();
        var go = Managers.Object.FindById(SelectedObjectIds[index]);
        if (go == null) return;
        var id = go.GetComponent<CreatureController>().Id;
        window.SelectedUnit = go;
        TurnOffSelectRing();
        TurnOnSelectRingCoroutineEvent?.Invoke(id);
    }
    
    public void TurnOnSelectRing(int id)
    {
        TurnOnSelectRingCoroutineEvent?.Invoke(id);
    }

    public void TurnOffOneSelectRing(int index)
    {
        var id = SelectedObjectIds[index];
        TurnOffOneSelectRingEvent?.Invoke(id);
        SelectedObjectIds.Remove(id);
    }

    public void TurnOffSelectRing()
    {
        TurnOffSelectRingEvent?.Invoke();
        SelectedObjectIds.Clear();
    }

    public void SetDeleteImageOnWindow()
    {
        SetDeleteImageOnWindowEvent?.Invoke();
    }
    
    public void HighlightDeleteImageOnWindow()
    {
        HighlightDeleteImageOnWindowEvent?.Invoke();
    }
    
    public void RestoreDeleteImageOnWindow()
    {
        RestoreDeleteImageOnWindowEvent?.Invoke();
    }
    
    public void DisappearDeleteImageOnWindow()
    {
        DisappearDeleteImageOnWindowEvent?.Invoke();
    }
    
    public int? GetButtonIndex(GameObject button)
    {
        return CapacityWindow.GetButtonIndex(button);
    }
    
    // Upgrade button on skill panel clicked
    public void OnUpgradeButtonClicked()
    {
        var level = GetLevelFromUiObject(CurrentSelectedPortrait.UnitId);
        if (level >= 3) return;
        Managers.Network.Send(new C_PortraitUpgrade { UnitId = CurrentSelectedPortrait.UnitId });
    }
    
    // Upgrade button on unit control panel
    public void OnUnitUpgradeClicked(IEnumerable<int> ids)
    {
        var packet = new C_UnitUpgrade();
        packet.ObjectId.AddRange(ids);
        Managers.Network.Send(packet);
        ClearSelectedObjects();
    }

    public void OnUnitDeleteClicked(IEnumerable<int> ids)
    {
        var packet = new C_UnitDelete();
        packet.ObjectIds.AddRange(ids);
        Managers.Network.Send(packet);
        ClearSelectedObjects();
    }
    
    public void OnUnitRepairClicked(IEnumerable<int> ids)
    {
        var packet = new C_UnitRepair();
        packet.ObjectId.AddRange(ids);
        packet.RepairAll = false;
        Managers.Network.Send(packet);
        ClearSelectedObjects();
    }

    public void OnUnitRepairAllClicked()
    {
        Managers.Network.Send(new C_UnitRepair { RepairAll = true });
        ClearSelectedObjects();
    }
    
    private void ClearSelectedObjects()
    {
        Managers.UI.CloseAllPopupUI();
        TurnOffSelectRing();
        SelectedObjectIds.Clear();
    }

    public void OnUnitSkillClicked()
    {
        Managers.UI.CloseAllPopupUI();
        Managers.UI.ShowPopupUiInGame<SkillWindow>();
    }

    public void WarningHandler(string warningMessageKey)
    {
        if (UnitControlWindow != null && warningMessageKey == "warning_in_game_needs_to_evolve")
        {
            OnUnitSkillClicked();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
    
    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        SelectedObjectIds = null;
        SkillsUpgraded = null;
        SkillWindow = null;
        UnitControlWindow = null;
        CapacityWindow = null;
        BaseSkillWindow = null;
        CurrentSelectedPortrait = null;
        CurrentSelectedSkillButton = null;    
    }
}
