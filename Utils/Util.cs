using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
                return Managers.Resource.Load<Sprite>("Externals/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Green");
            case UnitClass.Knight:
                return Managers.Resource.Load<Sprite>("Externals/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Blue");
            case UnitClass.NobleKnight:
                return Managers.Resource.Load<Sprite>("Externals/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Purple");
            case UnitClass.Baron:
            case UnitClass.Earl:
                return Managers.Resource.Load<Sprite>("Externals/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Yellow");
            case UnitClass.Duke:
                return Managers.Resource.Load<Sprite>("Externals/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Red");
            case UnitClass.Peasant:
            case UnitClass.None:
            default:
                return Managers.Resource.Load<Sprite>("Externals/UICasual/ResourcesData/Sprite/Component/Frame/Frame_ItemFrame01_Color_Brown");
        }
    }

    public static GameObject GetCardResources<TEnum>(IAsset asset, 
        Transform parent, float cardSize = 0, Action<PointerEventData> action = null) where TEnum : struct, Enum
    {
        var cardFrame = Managers.Resource.Instantiate("UI/Deck/CardFrame", parent);
        var unitInCard = cardFrame.transform.Find("CardUnit").gameObject;
        
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
        cardFrame.GetComponent<Image>().sprite = SetCardFrame(asset.Class);
        unitInCard.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);

        if (cardSize != 0)
        {
            var unitRect = unitInCard.GetComponent<RectTransform>();
            unitRect.anchorMin = new Vector2((1 - cardSize) * 0.5f, (1 - cardSize) * 0.5f);
            unitRect.anchorMax = new Vector2((1 + cardSize) * 0.5f, (1 + cardSize) * 0.5f);
            unitRect.offsetMin = Vector2.zero;
            unitRect.offsetMax = Vector2.zero;
        }
        
        if (action != null) cardFrame.BindEvent(action);

        return cardFrame;
    }

    public static GameObject GetMaterialResources(
        IAsset asset, Transform parent, float cardSize = 0, Action<PointerEventData> action = null)
    {
        var panel = Managers.Resource.Instantiate("UI/Deck/ItemPanel", parent);
        var materialInFrame = FindChild(panel, "ItemButton", true, true);
        
        var material = panel.GetOrAddComponent<MaterialItem>();
        material.Id = asset.Id;
        material.Class = asset.Class;
        
        var enumValue = (MaterialId)Enum.ToObject(typeof(MaterialId), asset.Id);
        var path = $"Sprites/Materials/{enumValue.ToString()}";
        var background = FindChild(panel, "ItemClassBackground", true, true);
        materialInFrame.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>(path);
        background.GetComponent<Image>().color = asset.Class switch
        {
            UnitClass.Peasant => new Color(.48f, .48f, .48f),
            UnitClass.Squire => new Color(.2f, .8f, .16f),
            UnitClass.Knight => new Color(.16f, .65f, .8f),
            UnitClass.NobleKnight => new Color(.8f, .16f, .8f),
            UnitClass.Baron => new Color(1, .86f, 0),
            UnitClass.Earl => new Color(1, 0.7f, 0),
            UnitClass.Duke => new Color(1, .26f, 0),
            UnitClass.None => new Color(.48f, .48f, .48f),
            _ => new Color(.48f, .48f, .48f),
        };
        
        if (action != null) panel.BindEvent(action);
        
        return panel;
    }
    
    public static Faction FindFactionByUnitId(int unitId)
    {
        return unitId / 500 == 0 ? Faction.Sheep : Faction.Wolf;
    }
    
    public static void SetCardSize(RectTransform rect, float x, float y, Vector2 anchoredVector = new())
    {
        rect.anchoredPosition = anchoredVector;
        rect.sizeDelta = new Vector2(x, y);
        rect.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(x, x);
    }
    
    public static Image GetFrameFromCardButton(ISkillButton button)
    {
        if (button is not MonoBehaviour mono) return null;
        var go = mono.gameObject;
        return go.transform.parent.parent.GetChild(1).GetComponent<Image>();
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