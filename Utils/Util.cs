using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Cinemachine;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

/* Last Modified : 24. 09. 09
 * Version : 1.011
 */

public class Util
{
    public static Faction Faction = Faction.Sheep;
    public static Deck Deck = new();
    
    public static Color ThemeColor => Faction == Faction.Sheep
        ? new Color(39 / 255f, 107 / 255f, 214 / 255f)
        : new Color(133 / 255f, 29 / 255f, 72 / 255f);
    
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }
    
    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false, bool includeInactive = false)
        where T : Object
    {
        if (go == null)
            return null;
        
        if (recursive == false)
        {
            for (var i = 0; i < go.transform.childCount; i++)
            {
                var transform = go.transform.GetChild(i);
                if (!string.IsNullOrEmpty(name) && transform.name != name) continue;
                var component = transform.GetComponent<T>();
                if (component != null) return component;
            }
        }
        else
        {
            return go.GetComponentsInChildren<T>(includeInactive)
                .FirstOrDefault(component => string.IsNullOrEmpty(name) || component.name == name);
        }

        return null;
    }
    
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false, bool includeInactive = false)
    {
        var transform = FindChild<Transform>(go, name, recursive, includeInactive);
        return transform == null ? null : transform.gameObject;
    }

    public static List<GameObject> FindChildren(GameObject go, string name = null)
    {
        var foundObjects = new List<GameObject>();
        for (var i = 0; i < go.transform.childCount; i++)
        {
            var transform = go.transform.GetChild(i);
            if (!string.IsNullOrEmpty(name) && transform.name != name) continue;
            foundObjects.Add(transform.gameObject);
        }

        return foundObjects;
    }
    
    public static Vector3 NearestCell(Vector3 worldPosition)
    {
        Vector3 cellPos = Managers.Map.CurrentGrid.CellToWorld(Managers.Map.CurrentGrid.WorldToCell(worldPosition));
        return cellPos;
    }

    public static void SetAlpha(Image img, float alpha)
    {
        img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);
    }
    
    public static Sprite SetCardFrame(UnitClass unitClass)
    {
        switch (unitClass)
        {
            case UnitClass.Squire:
                return Managers.Resource.Load<Sprite>("Externals/UI/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Green");
            case UnitClass.Knight:
                return Managers.Resource.Load<Sprite>("Externals/UI/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Blue");
            case UnitClass.NobleKnight:
                return Managers.Resource.Load<Sprite>("Externals/UI/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Purple");
            case UnitClass.Baron:
            case UnitClass.Earl:
                return Managers.Resource.Load<Sprite>("Externals/UI/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Yellow");
            case UnitClass.Duke:
                return Managers.Resource.Load<Sprite>("Externals/UI/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Red");
            case UnitClass.Peasant:
            case UnitClass.None:
            default:
                return Managers.Resource.Load<Sprite>("Externals/UI/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Brown");
        }
    }

    public static GameObject GetCardResources<TEnum>(IAsset asset, Transform parent, Action<PointerEventData> action = null) 
        where TEnum : struct, Enum
    {
        var cardFrame = Managers.Resource.Instantiate("UI/Deck/CardFrame", parent);
        var unitInCard = FindChild(cardFrame, "CardUnit", true);
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
        
        var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), asset.Id);
        var path = $"Sprites/Portrait/{enumValue.ToString()}";
        var background = FindChild(cardFrame, "Bg", true).GetComponent<Image>();
        var gradient = FindChild(cardFrame, "Gradient", true).GetComponent<Image>();
        var glow = FindChild(cardFrame, "Glow", true).GetComponent<Image>();
        var role = FindChild(cardFrame, "Role", true);
        var roleIcon = FindChild(role, "RoleIcon", true).GetComponent<Image>();
        
        unitInCard.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        BindUnitCardColor(asset.Class, background, gradient, glow);
        BindUnitRoleIcon(card.Id, roleIcon);
        
        if (action != null) cardFrame.BindEvent(action);
        if (card.AssetType != Asset.Unit)
        {
            role.SetActive(false);
            return cardFrame;
        }
        if (Managers.Data.UnitInfoDict.TryGetValue(asset.Id, out var unitInfo) == false) return cardFrame;
        var starPanel = cardFrame.transform.Find("StarPanel").gameObject;
        var nameText = FindChild(cardFrame, "UnitNameText", true).GetComponent<TextMeshProUGUI>();
        nameText.text = ((UnitId)unitInfo.Id).ToString();
            
        for (var i = 1; i <= unitInfo.Level; i++)
        {
            var star = FindChild(starPanel, $"GradeStar{i}", true, true);
            star.SetActive(true);
        }
        
        return cardFrame;
    }

    public static GameObject GetMaterialResources(IAsset asset, Transform parent, Action<PointerEventData> action = null)
    {
        var itemFrame = Managers.Resource.Instantiate("UI/Deck/ItemFrame", parent);
        var materialInFrame = FindChild(itemFrame, "ItemImage", true, true);
        var material = itemFrame.GetOrAddComponent<MaterialItem>();
        
        material.Id = asset.Id;
        material.Class = asset.Class;
        
        var enumValue = (MaterialId)Enum.ToObject(typeof(MaterialId), asset.Id);
        var path = $"Sprites/Materials/{enumValue.ToString()}";
        var background = FindChild(itemFrame, "Bg", true).GetComponent<Image>();
        var cornerDeco = FindChild(itemFrame, "CornerDeco", true).GetComponent<Image>();
        var light = FindChild(itemFrame, "Light", true).GetComponent<Image>();
        var glow = FindChild(itemFrame, "Glow", true).GetComponent<Image>();
        
        materialInFrame.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        BindMaterialCardColor(material.Class, background, cornerDeco, light, glow);
        
        if (action != null) itemFrame.BindEvent(action);
        
        return itemFrame;
    }

    private static void BindUnitCardColor(UnitClass unitClass, Image background, Image gradient, Image glow)
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

    private static void BindUnitRoleIcon(int id, Image roleIcon)
    {
        Managers.Data.UnitInfoDict.TryGetValue(id, out var unitInfo);
        if (unitInfo == null) return;
        
        var path = $"Sprites/Icons/icon_role_{unitInfo.Role.ToString().ToLower()}";
        roleIcon.sprite = Managers.Resource.Load<Sprite>(path);
    }

    private static void BindMaterialCardColor(UnitClass materialClass, Image background, Image deco, Image light, Image glow)
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
    
    public static Faction FindFactionByUnitId(int unitId)
    {
        return unitId / 500 == 0 ? Faction.Sheep : Faction.Wolf;
    }
    
    public static Image GetFrameFromCardButton(ISkillButton button)
    {
        if (button is not MonoBehaviour mono) return null;
        var go = mono.gameObject;
        return go.transform.parent.parent.GetChild(1).GetComponent<Image>();
    }

    public static string ConvertStringToIconFormat(string str)
    {
        str = Regex.Replace(str, "(?<!^)([A-Z])", "_$1");
        str = str.ToLower();

        return "icon_" + str;
    }
    
    public static string ConvertStringToNameFormat(string str)
    {
        str = Regex.Replace(str, "(?<!^)([A-Z])", " $1");
        str = Regex.Replace(str, "(\\d+)", " $1");
        return str;
    }
    
    public static void DestroyAllChildren(Transform parent)
    {
        var children = (from Transform child in parent select child.gameObject).ToList();
        foreach (var child in children)
        {
            Managers.Resource.Destroy(child);
        }
    }
}