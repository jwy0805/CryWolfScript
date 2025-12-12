using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using Zenject;

/* Last Modified : 24. 10. 09
 * Version : 1.014
 */

public class CraftingViewModel
{
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;
    
    private UnitInfo _newReinforceMaterialUnit;

    public List<OwnedMaterialInfo> CraftingMaterials { get; private set; } = new();
    // Only used for the crafting panel because the user can adjust 'the number of cards' to be crafted.
    public List<OwnedMaterialInfo> TotalCraftingMaterials { get; set; } = new();
    public List<UnitInfo> ReinforceMaterialUnits { get; set; } = new();
    public int ReinforcePointNeeded { get; set; }
    public Card CardToBeCrafted { get; set; }
    public int CraftingCount { get; set; }

    [Inject]
    public CraftingViewModel(IUserService userService, IWebService webService, ITokenService tokenService)
    {
        _userService = userService;
        _webService = webService;
        _tokenService = tokenService;
    }
    
    public float CraftingPanelDuration => 0.5f;
    public float CraftingPanelHeight => 1000f;

    // Crafting Panel <- crafting panel itself in the scene
    // Craft Panel <- craft panel when a crafting button is clicked
    public event Func<Card, Task> SetCardOnCraftingPanel;
    public event Func<List<OwnedMaterialInfo>, List<OwnedMaterialInfo>, Task> SetMaterialsOnCraftPanel;
    public event Action InitCraftingPanel;
    public event Func<Faction, Task> SetCollectionUI;
    public event Action<UnitId, bool> BindReinforceResult;

    public void InitReinforceSetting()
    {
        CraftingMaterials.Clear();
        TotalCraftingMaterials.Clear();
        ReinforceMaterialUnits.Clear();
        ReinforcePointNeeded = 0;
        CardToBeCrafted = null;
        CraftingCount = 0;
    }
    
    public void SetCard(Card card)
    {
        SetCardOnCraftingPanel?.Invoke(card);
    }
    
    public void LoadMaterials(UnitId unitId)
    {
        CraftingMaterials = Managers.Data.CraftingMaterialDict[(int)unitId].Materials.ToList();
        SetMaterialsOnCraftPanel?.Invoke(CraftingMaterials, User.Instance.OwnedMaterialList);
    }

    public async Task CraftCard()
    {
         var craftPacket = new CraftCardPacketRequired
         {
             AccessToken = _tokenService.GetAccessToken(),
             Materials = TotalCraftingMaterials,
             UnitId = (UnitId)CardToBeCrafted.Id,
             Count = CraftingCount
         };
         
         var craftTask = _webService.SendWebRequestAsync<CraftCardPacketResponse>(
                "Crafting/CraftCard", "PUT", craftPacket);
         var res = await craftTask;
         
         if (res.CraftCardOk == false)
         {
             var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
             await Managers.Localization.UpdateWarningPopupText(popup, "warning_server_error");
         }
         
         await Managers.UI.ShowPopupUI<UI_CraftSuccessPopup>();
         
         // Update the user's owned list on the client side
         UpdateOwnedMaterials(TotalCraftingMaterials);
         AddAssetsByCrafting(CardToBeCrafted);
        
         // Initialize the crafting panel ui and collection ui
         SetCollectionUI?.Invoke(Util.Faction);
         InitCraftingPanel?.Invoke();
    }

    public async Task GetReinforceResult(UnitInfo unitInfo)
    {
        var reinforcePacket = new ReinforceResultPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            UnitInfo = unitInfo,
            UnitList = ReinforceMaterialUnits
        };
        
        var res = await _webService.SendWebRequestAsync<ReinforceResultPacketResponse>(
            "Crafting/ReinforceCard", "PUT", reinforcePacket);

        if (res.ReinforceResultOk == false)
        {
            var popup = await Managers.UI.ShowPopupUI<UI_WarningPopup>();
            await Managers.Localization.UpdateWarningPopupText(popup, "warning_server_error");
        }
        
        BindReinforceResult?.Invoke((UnitId)unitInfo.Id + 1, res.IsSuccess);
        
        // Update the user's owned list on the client side
        UpdateOwnedMaterials(CraftingMaterials);
        UpdateOwnedUnits(ReinforceMaterialUnits);
        if (res.IsSuccess) AddAssetsByReinforcing(unitInfo);
        
        // Initialize the crafting panel ui and collection ui
        SetCollectionUI?.Invoke(Util.Faction);
        InitCraftingPanel?.Invoke();
    }
    
    private void UpdateOwnedMaterials(List<OwnedMaterialInfo> materials)
    {
        var ownedMaterialList = User.Instance.OwnedMaterialList;

        foreach (var material in materials)
        {
            var matchingOwnedMaterial = ownedMaterialList
                .FirstOrDefault(owned => owned.MaterialInfo.Id == material.MaterialInfo.Id);

            if (matchingOwnedMaterial != null)
            {
                matchingOwnedMaterial.Count -= material.Count;
                
                if (matchingOwnedMaterial.Count <= 0)
                {
                    ownedMaterialList.Remove(matchingOwnedMaterial);
                }
            }
        }
    }

    private void UpdateOwnedUnits(List<UnitInfo> unitInfos)
    {
        var ownedUnitList = User.Instance.OwnedUnitList;
        
        foreach (var unitInfo in unitInfos)    
        {
            var matchingOwnedUnit = ownedUnitList
                .FirstOrDefault(owned => owned.UnitInfo.Id == unitInfo.Id);

            if (matchingOwnedUnit != null)
            {
                matchingOwnedUnit.Count -= 1;

                if (matchingOwnedUnit.Count <= 0)
                {
                    ownedUnitList.Remove(matchingOwnedUnit);
                    User.Instance.NotOwnedUnitList.Add(matchingOwnedUnit.UnitInfo);
                }
            }
        }
    }
    
    private void AddAssetsByCrafting(Card card)
    {
        switch (card.AssetType)
        {
            case Asset.Unit:
                var unit = User.Instance.OwnedUnitList.FirstOrDefault(info => info.UnitInfo.Id == card.Id);
                
                if (unit != null)
                {
                    unit.Count += CraftingCount;
                }
                else
                {
                    User.Instance.OwnedUnitList.Add(new OwnedUnitInfo
                    {
                        UnitInfo = Managers.Data.UnitInfoDict[card.Id],
                        Count = CraftingCount
                    });
                }
                break;
            
            case Asset.Enchant:
                var enchant = User.Instance.OwnedEnchantList.FirstOrDefault(info => info.EnchantInfo.Id == card.Id);
                
                if (enchant != null)
                {
                    enchant.Count += CraftingCount;
                }
                else
                {
                    User.Instance.OwnedEnchantList.Add(new OwnedEnchantInfo
                    {
                        EnchantInfo = Managers.Data.EnchantInfoDict[card.Id],
                        Count = CraftingCount
                    });
                }
                break;
                
            case Asset.Sheep:
                var sheep = User.Instance.OwnedSheepList.FirstOrDefault(info => info.SheepInfo.Id == card.Id);
                
                if (sheep != null)
                {
                    sheep.Count += CraftingCount;
                }
                else
                {
                    User.Instance.OwnedSheepList.Add(new OwnedSheepInfo
                    {
                        SheepInfo = Managers.Data.SheepInfoDict[card.Id],
                        Count = CraftingCount
                    });
                }
                break;
            
            case Asset.Character:
                var character = User.Instance.OwnedCharacterList.FirstOrDefault(info => info.CharacterInfo.Id == card.Id);
                
                if (character != null)
                {
                    character.Count += CraftingCount;
                }
                else
                {
                    User.Instance.OwnedCharacterList.Add(new OwnedCharacterInfo
                    {
                        CharacterInfo = Managers.Data.CharacterInfoDict[card.Id],
                        Count = CraftingCount
                    });
                }
                break;
        }
    }

    private void AddAssetsByReinforcing(UnitInfo unitInfo)
    {
        var newUnitId = unitInfo.Id + 1;
        var newUnit = User.Instance.OwnedUnitList.FirstOrDefault(info => info.UnitInfo.Id == newUnitId);
        
        if (newUnit != null)
        {
            newUnit.Count += 1;
        }
        else
        {
            User.Instance.OwnedUnitList.Add(new OwnedUnitInfo
            {
                UnitInfo = Managers.Data.UnitInfoDict[newUnitId],
                Count = 1
            });
        }
    }

    public void SetReinforcePointNeeded(UnitInfo unitInfo)
    {
        ReinforcePointNeeded = Managers.Data.ReinforcePointDict[Tuple.Create(unitInfo.Class, unitInfo.Level)].Point;
    }

    public float GetSuccessRate()
    {
        var dictionary = Managers.Data.ReinforcePointDict;
        var numerator = ReinforceMaterialUnits
            .Sum(info => dictionary[Tuple.Create(info.Class, info.Level)].Point);
        
        return numerator / (float)ReinforcePointNeeded;
    }

    public bool IsUnitInDecks(UnitInfo unitInfo)
    {
        var instance = User.Instance;
        var units = instance.AllDeckSheep.Concat(instance.AllDeckWolf)
            .SelectMany(deck => deck.UnitsOnDeck)
            .GroupBy(unit => unit.Id)
            .Select(group => new OwnedUnitInfo
            {
                UnitInfo = group.First(),
                Count = group.Count()
            }).ToList();
        
        return units.Any(info => info.UnitInfo.Id == unitInfo.Id);
    }
    
    // Check if the user has enough cards to do games.
    // 1. Cannot use the only card if the card is already in the deck.
    public bool VerityCardByCondition1(UnitInfo unitInfo)
    {
        var instance = User.Instance;
        var units = instance.AllDeckSheep.Concat(instance.AllDeckWolf)
            .SelectMany(deck => deck.UnitsOnDeck)
            .GroupBy(unit => unit.Id)
            .Select(group => new OwnedUnitInfo
            {
                UnitInfo = group.First(),
                Count = group.Count()
            }).ToList();
        
        var isUnitInDecks = units.Any(info => info.UnitInfo.Id == unitInfo.Id);

        if (isUnitInDecks)
        {
            return instance.OwnedUnitList.First(info => info.UnitInfo.Id == unitInfo.Id).Count > 1;
        }

        return true;
    }
    
    // 2. There must be at least 6 cards of each different species and at least 4 cards are level 3.
    public bool VerifyCardByCondition2(UnitInfo unitInfo)
    {
        var instance = User.Instance;
        var sheepUnits = instance.OwnedUnitList
            .Where(info => info.UnitInfo.Faction == Faction.Sheep).ToList();
        var wolfUnits = instance.OwnedUnitList
            .Where(info => info.UnitInfo.Faction == Faction.Wolf).ToList();
        
        return VerifyFactionCondition(new List<OwnedUnitInfo>(sheepUnits), unitInfo)
               && VerifyFactionCondition(new List<OwnedUnitInfo>(wolfUnits), unitInfo);
    }
    
    // 3. The user must have more than 0 card.
    public bool VerifyCardByCondition3(UnitInfo unitInfo)
    {
        var realCount = User.Instance.OwnedUnitList.First(info => info.UnitInfo.Id == unitInfo.Id).Count;
        var virtualCount = ReinforceMaterialUnits.Count(info => info.Id == unitInfo.Id);
        
        return realCount - virtualCount > 0;
    }
    
    private bool VerifyFactionCondition(List<OwnedUnitInfo> units, UnitInfo unitInfo)
    {
        // Are there at least 6 different species
        var speciesCount = units
            .Select(unit => new OwnedUnitInfo
            {
                UnitInfo = unit.UnitInfo,
                Count = unit.Count - (unit.UnitInfo.Id == unitInfo.Id ? 1 : 0)
            })
            .Where(unit => unit.Count > 0)
            .GroupBy(unit => unit.UnitInfo.Species).ToList().Count >= 6;
        
        // Are there at least 4 cards of level 3
        var levelCount = units.Count(unit => unit.UnitInfo.Level >= 3) >= 4;
        
        return speciesCount && levelCount;
    }
    
    public void AddNewUnitMaterial(UnitInfo unitInfo)
    {
        if (ReinforceMaterialUnits.Count < 8)
        {
            ReinforceMaterialUnits.Add(unitInfo);
        }
    }
    
    public void RemoveNewUnitMaterial(UnitInfo unitInfo)
    {
        ReinforceMaterialUnits.Remove(unitInfo);
    }
}