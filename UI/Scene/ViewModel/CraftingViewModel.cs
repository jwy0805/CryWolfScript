using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using UnityEngine;
using Zenject;

public class CraftingViewModel
{
    private readonly IUserService _userService;
    private readonly IWebService _webService;
    private readonly ITokenService _tokenService;

    public List<OwnedMaterialInfo> CraftingMaterials { get; set; } = new();
    public List<OwnedMaterialInfo> TotalCraftingMaterials { get; set; } = new();
    public Card CardToBeCrafted { get; set; }
    public int Count { get; set; }
    
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
    public event Action<Card> SetCardOnCraftingPanel;
    public event Action<List<OwnedMaterialInfo>, List<OwnedMaterialInfo>> SetMaterialsOnCraftPanel;
    public event Action InitCraftingPanel;
    public event Action<Faction> SetCollectionUI;

    public void InitSetting()
    {
        CraftingMaterials.Clear();
        TotalCraftingMaterials.Clear();
        CardToBeCrafted = null;
        Count = 0;
    }
    
    public void SetCard(Card card)
    {
        SetCardOnCraftingPanel?.Invoke(card);
    }
    
    public async Task LoadMaterials(UnitId unitId)
    {
        // Load materials from the server
        var materialPacket = new LoadMaterialsPacketRequired
        {
            AccessToken = _tokenService.GetAccessToken(),
            UnitId = unitId
        };
        
        var materialTask = _webService.SendWebRequestAsync<LoadMaterialsPacketResponse>(
            "Crafting/LoadMaterials", "POST", materialPacket);
        var res = await materialTask;
        
        if (res.LoadMaterialsOk == false) return;

        CraftingMaterials = res.CraftingMaterialList.ToList();
        SetMaterialsOnCraftPanel?.Invoke(CraftingMaterials, User.Instance.OwnedMaterialList);
    }

    public async Task CraftCard()
    {
         var craftPacket = new CraftCardPacketRequired
         {
             AccessToken = _tokenService.GetAccessToken(),
             Materials = TotalCraftingMaterials,
             UnitId = (UnitId)CardToBeCrafted.Id,
             Count = Count
         };
         
         var craftTask = _webService.SendWebRequestAsync<CraftCardPacketResponse>(
                "Crafting/CraftCard", "PUT", craftPacket);
         var res = await craftTask;
         
         if (res.CraftCardOk == false)
         {
             var popup = Managers.UI.ShowPopupUI<UI_WarningPopup>();
             popup.SetWarning("Server trouble has occurred. Please try again later.");
         }
         
         Managers.UI.ShowPopupUI<UI_CraftSuccessPopup>();
         
         // Update the user's owned  list
         var ownedMaterialList = User.Instance.OwnedMaterialList;
         foreach (var ownedMaterial in ownedMaterialList)
         {
             var materialInfo = ownedMaterial.MaterialInfo;
             var matchingMaterial = TotalCraftingMaterials
                 .FirstOrDefault(info => info.MaterialInfo.Id == materialInfo.Id);
             
             if (matchingMaterial != null)
             {
                 ownedMaterial.Count -= matchingMaterial.Count;
             }
         }
         
         AddAssets();
         SetCollectionUI?.Invoke(Util.Faction);
         
         // Initialize the crafting panel ui and collection ui
         InitCraftingPanel?.Invoke();
    }

    private void AddAssets()
    {
        var card = CardToBeCrafted;
        switch (card.AssetType)
        {
            case Asset.Unit:
                User.Instance.OwnedUnitList.Add(new OwnedUnitInfo
                {
                    UnitInfo = Managers.Data.UnitInfoDict[card.Id],
                    Count = Count
                });
                break;
            case Asset.Enchant:
                User.Instance.OwnedEnchantList.Add(new OwnedEnchantInfo
                {
                    EnchantInfo = Managers.Data.EnchantInfoDict[card.Id],
                    Count = Count
                });
                break;
            case Asset.Sheep:
                User.Instance.OwnedSheepList.Add(new OwnedSheepInfo
                {
                    SheepInfo = Managers.Data.SheepInfoDict[card.Id],
                    Count = Count
                });
                break;
            case Asset.Character:
                User.Instance.OwnedCharacterList.Add(new OwnedCharacterInfo
                {
                    CharacterInfo = Managers.Data.CharacterInfoDict[card.Id],
                    Count = Count
                });
                break;
        }
    }
}