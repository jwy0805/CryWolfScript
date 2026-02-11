using System;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CardFactory : ICardFactory
{
    public async Task<GameObject> GetCardResourcesF<TEnum>(
        IAsset asset, 
        Transform parent, 
        Func<PointerEventData, Task> action = null, 
        bool activateText = false) where TEnum : struct, Enum
    {
        var cardFrame = await Managers.Resource.Instantiate("UI/Lobby/Deck/CardFrame", parent);
        var card = cardFrame.GetOrAddComponent<Card>();
        
        card.Id = asset.Id;
        card.Class = asset.Class;
        card.AssetType = typeof(TEnum).Name switch
        {
            "UnitId" => Asset.Unit,
            "SheepId" => Asset.Sheep,
            "EnchantId" => Asset.Enchant,
            "CharacterId" => Asset.Character,
            _ => Asset.None
        };
        
        if (action != null) cardFrame.BindEvent(action);
        await SetCardContents<TEnum>(cardFrame, asset, card.AssetType);
        
        var nameTextObject = Util.FindChild(cardFrame, "UnitNameText", true, true);
        nameTextObject.SetActive(activateText);
        
        return cardFrame;
    }
    
    public async Task<GameObject> GetCardResources<TEnum>(
        IAsset asset, 
        Transform parent, 
        Action<PointerEventData> action = null, 
        bool activateText = false) where TEnum : struct, Enum
    {
        var cardFrame = await Managers.Resource.Instantiate("UI/Lobby/Deck/CardFrame", parent);
        var card = cardFrame.GetOrAddComponent<Card>();
        
        card.Id = asset.Id;
        card.Class = asset.Class;
        card.AssetType = typeof(TEnum).Name switch
        {
            "UnitId" => Asset.Unit,
            "SheepId" => Asset.Sheep,
            "EnchantId" => Asset.Enchant,
            "CharacterId" => Asset.Character,
            _ => Asset.None
        };
        
        if (action != null) cardFrame.BindEvent(action);
        await SetCardContents<TEnum>(cardFrame, asset, card.AssetType);
        
        var nameTextObject = Util.FindChild(cardFrame, "UnitNameText", true, true);
        nameTextObject.SetActive(activateText);
        
        return cardFrame;
    }
    
    public async Task<GameObject> GetMaterialResources(IAsset asset, Transform parent, Action<PointerEventData> action = null)
    {
        var itemFrame = await Managers.Resource.Instantiate("UI/Lobby/Deck/ItemFrame", parent);
        var materialInFrame = Util.FindChild(itemFrame, "ItemImage", true, true);
        var material = itemFrame.GetOrAddComponent<MaterialItem>();
        
        material.Id = asset.Id;
        material.Class = asset.Class;
        
        var enumValue = (MaterialId)Enum.ToObject(typeof(MaterialId), asset.Id);
        var path = $"Sprites/Materials/{enumValue.ToString()}";
        var background = Util.FindChild(itemFrame, "Bg", true).GetComponent<Image>();
        var cornerDeco = Util.FindChild(itemFrame, "CornerDeco", true).GetComponent<Image>();
        var light = Util.FindChild(itemFrame, "Light", true).GetComponent<Image>();
        var glow = Util.FindChild(itemFrame, "Glow", true).GetComponent<Image>();
        
        materialInFrame.GetComponent<Image>().sprite = await Managers.Resource.LoadAsync<Sprite>(path);
        BindMaterialCardColor(material.Class, background, cornerDeco, light, glow);
        
        if (action != null) itemFrame.BindEvent(action);
        
        return itemFrame;
    }

    public async Task<GameObject> GetItemFrameGold(int count, Transform parent)
    {
        var itemFrame = await Managers.Resource.Instantiate("UI/Lobby/Deck/ItemFrameGold", parent);
        var goldImage = Util.FindChild<Image>(itemFrame, "ItemIcon", true);
        var countText = Util.FindChild<TextMeshProUGUI>(itemFrame, "CountText", true);
        var path = GetGoldSpritePath(count);
        
        goldImage.sprite = await Managers.Resource.LoadAsync<Sprite>(path);
        countText.text = count.ToString();
        
        return itemFrame;
    }
    
    public async Task<GameObject> GetItemFrameSpinel(int count, Transform parent)
    {
        var itemFrame = await Managers.Resource.Instantiate("UI/Lobby/Deck/ItemFrameSpinel", parent);
        var spinelImage = Util.FindChild<Image>(itemFrame, "ItemIcon", true);
        var countText = Util.FindChild<TextMeshProUGUI>(itemFrame, "CountText", true);
        var path = GetSpinelSpritePath(count);
        
        spinelImage.sprite = await Managers.Resource.LoadAsync<Sprite>(path);
        countText.text = count.ToString();
        
        return itemFrame;
    }
    
    private string GetGoldSpritePath(int count) => count switch
    {
        >= 50000 => "Sprites/ShopIcons/icon_gold_vault",
        >= 25000 => "Sprites/ShopIcons/icon_gold_basket",
        >= 2500 => "Sprites/ShopIcons/icon_gold_pouch",
        _ => "Sprites/ShopIcons/icon_gold_pile"
    };
    
    public string GetGoldPrefabPath(int count) => count switch
    {
        >= 50000 => "UI/Shop/NormalizedProducts/GoldVault",
        >= 25000 => "UI/Shop/NormalizedProducts/GoldBasket",
        >= 2500 => "UI/Shop/NormalizedProducts/GoldPouch",
        _ => "UI/Shop/NormalizedProducts/GoldPile"
    };
    
    private string GetSpinelSpritePath(int count) => count switch
    {
        >= 7000 => "Sprites/ShopIcons/icon_spinel_vault",
        >= 5000 => "Sprites/ShopIcons/icon_spinel_chest",
        >= 3000 => "Sprites/ShopIcons/icon_spinel_basket",
        >= 1000 => "Sprites/ShopIcons/icon_spinel_pouch",
        >= 50 => "Sprites/ShopIcons/icon_spinel_fistful",
        _ => "Sprites/ShopIcons/icon_spinel_pile"
    };
    
    public string GetSpinelPrefabPath(int count) => count switch
    {
        >= 7000 => "UI/Shop/NormalizedProducts/SpinelVault",
        >= 5000 => "UI/Shop/NormalizedProducts/SpinelChest",
        >= 3000 => "UI/Shop/NormalizedProducts/SpinelBasket",
        >= 1000 => "UI/Shop/NormalizedProducts/SpinelPouch",
        >= 50 => "UI/Shop/NormalizedProducts/SpinelFistful",
        _ => "UI/Shop/NormalizedProducts/SpinelPile"
    };
    
    private async Task SetCardContents<TEnum>(GameObject cardFrame, IAsset asset, Asset assetType) where TEnum : struct, Enum
    {
        var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), asset.Id);
        var portraitKey = $"Sprites/Portrait/{enumValue.ToString()}";
        var background = Util.FindChild(cardFrame, "Bg", true).GetComponent<Image>();
        var gradient = Util.FindChild(cardFrame, "Gradient", true).GetComponent<Image>();
        var glow = Util.FindChild(cardFrame, "Glow", true).GetComponent<Image>();
        var role = Util.FindChild(cardFrame, "Role", true);
        var roleIcon = Util.FindChild(role, "RoleIcon", true).GetComponent<Image>();
        var unitInCard = Util.FindChild(cardFrame, "CardUnit", true);
        var startPanel = Util.FindChild(cardFrame, "StarPanel", true);
        var nameTextObject = Util.FindChild(cardFrame, "UnitNameText", true, true);
        
        unitInCard.GetComponent<Image>().sprite = await Managers.Resource.LoadAsync<Sprite>(portraitKey);
        BindUnitCardColor(asset.Class, background, gradient, glow);
        await BindUnitRoleIcon(asset.Id, roleIcon);

        var key = string.Empty;
        switch (assetType)
        {
            case Asset.Unit:
                if (Managers.Data.UnitInfoDict.TryGetValue(asset.Id, out var unitInfo))
                {
                    var unitName = ((UnitId)unitInfo.Id).ToString();
                    key = string.Concat("unit_name_", Managers.Localization.GetConvertedString(unitName));
                    for (var i = 0; i < 3; i++)
                    {
                        startPanel.transform.GetChild(i).gameObject.SetActive(i < unitInfo.Level);
                    }
                }
                break;
            
            case Asset.Character:
                role.SetActive(false);
                startPanel.gameObject.SetActive(false);
                if (Managers.Data.CharacterInfoDict.TryGetValue(asset.Id, out var characterInfo))
                {
                    var characterName = ((CharacterId)characterInfo.Id).ToString();
                    key = string.Concat("character_name_", Managers.Localization.GetConvertedString(characterName));
                }
                break;
            
            case Asset.Sheep:
                role.SetActive(false);
                startPanel.gameObject.SetActive(false);
                if (Managers.Data.SheepInfoDict.TryGetValue(asset.Id, out var sheepInfo))
                {
                    var sheepName = ((SheepId)sheepInfo.Id).ToString();
                    key = string.Concat("sheep_name_", Managers.Localization.GetConvertedString(sheepName));
                }
                break;
            
            case Asset.Enchant:
                var enchantRect = unitInCard.GetComponent<RectTransform>();
                enchantRect.anchorMin = Vector2.zero;
                enchantRect.anchorMax = Vector2.one;
                enchantRect.sizeDelta = Vector2.zero;
                role.SetActive(false);
                startPanel.gameObject.SetActive(false);
                if (Managers.Data.EnchantInfoDict.TryGetValue(asset.Id, out var enchantInfo))
                {
                    var enchantName = ((EnchantId)enchantInfo.Id).ToString();
                    key = string.Concat("enchant_name_", Managers.Localization.GetConvertedString(enchantName));
                }
                break;
        }

        if (key != string.Empty)
        {
            var convertedKey = Managers.Localization.GetConvertedString(key);
            await Managers.Localization.UpdateTextAndFont(nameTextObject, convertedKey);
        }
    }
    
    private void BindUnitCardColor(UnitClass unitClass, Image background, Image gradient, Image glow)
    {
        switch (unitClass)
        {
            case UnitClass.Squire:
                background.color = new Color(52 / 255f, 177 / 255f, 83 / 255f);
                gradient.color = new Color(90 / 255f, 216 / 255f, 72 / 255f);
                glow.color = new Color(156 / 255f, 254 / 255f, 79 / 255f);
                break;
            case UnitClass.Knight:
                background.color = new Color(60 / 255f, 136 / 255f, 246 / 255f);
                gradient.color = new Color(6 / 255f, 172 / 255f, 254 / 255f);
                glow.color = new Color(1 / 255f, 222 / 255f, 1);
                break;
            case UnitClass.NobleKnight:
                background.color = new Color(115 / 255f, 77 / 255f, 238 / 255f);
                gradient.color = new Color(149 / 255f, 85 / 255f, 253 / 255f);
                glow.color = new Color(185 / 255f, 150 / 255f, 1);
                break;
            case UnitClass.Baron:
                background.color = new Color(1, 201 / 255f, 0);
                gradient.color = new Color(1, 245 / 255f, 34 / 255f);
                glow.color = new Color(1, 245 / 255f, 200 / 255f);
                break;
            case UnitClass.Peasant:
            case UnitClass.None:
            default:
                background.color = new Color(98 / 255f, 110 / 255f, 139 / 255f);
                gradient.color = new Color(144 / 255f, 163 / 255f, 186 / 255f);
                glow.color = new Color(131 / 255f, 166 / 255f, 180 / 255f);
                break;
        }
    }
    
    private void BindMaterialCardColor(UnitClass materialClass, Image background, Image deco, Image light, Image glow)
    {
        switch (materialClass)
        {
            case UnitClass.Squire:
                background.color = new Color(29 / 255f, 192 / 255f, 86 / 255f);
                deco.color = new Color(52 / 255f, 217 / 255f, 52 / 255f);
                light.color = new Color(178 / 255f, 241 / 255f, 31 / 255f);
                glow.color = new Color(192 / 255f, 255 / 255f, 81 / 255f);
                break;
            case UnitClass.Knight:
                background.color = new Color(0 / 255f, 168 / 255f, 255 / 255f);
                deco.color = new Color(44 / 255f, 190 / 255f, 255 / 255f);
                light.color = new Color(53 / 255f, 251 / 255f, 255 / 255f);
                glow.color = new Color(8 / 255f, 239 / 255f, 255 / 255f);
                break;
            case UnitClass.NobleKnight:
                background.color = new Color(178 / 255f, 96 / 255f, 253 / 255f);
                deco.color = new Color(200 / 255f, 116 / 255f, 253 / 255f);
                light.color = new Color(1f, 138 / 255f, 1f);
                glow.color = new Color(185 / 255f, 138 / 255f, 1f);
                break;
            case UnitClass.Baron:
                background.color = new Color(1f, 201 / 255f, 0);
                deco.color = new Color(1f, 222 / 255f, 0);
                light.color = new Color(254 / 255f, 138 / 255f, 78 / 255f);
                glow.color = new Color(243 / 244f, 1f, 49 / 255f);
                break;
            case UnitClass.Peasant:
            case UnitClass.None:
            default:
                background.color = new Color(97 / 255f, 126 / 255f, 138 / 255f);
                deco.color = new Color(113 / 255f, 142 / 255f, 153 / 255f);
                light.color = new Color(130 / 255f, 160 / 255f, 171 / 255f);
                glow.color = new Color(131 / 255f, 166 / 255f, 180 / 255f);
                break;
        }
    }
        
    private async Task BindUnitRoleIcon(int id, Image roleIcon)
    {
        Managers.Data.UnitInfoDict.TryGetValue(id, out var unitInfo);
        if (unitInfo == null) return;
        
        var path = $"Sprites/Icons/icon_role_{unitInfo.Role.ToString().ToLower()}";
        roleIcon.sprite = await Managers.Resource.LoadAsync<Sprite>(path);
    }
}
