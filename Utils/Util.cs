using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using Google.Protobuf.Protocol;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;

public class Util
{
    public static Camp Camp;
    public static UserManager.Deck Deck = new();
    
    public static T GetOrAddComponent<T>(GameObject go) where T : Component
    {
        var component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }
    
    public static T FindChild<T>(GameObject gameObject, string name = null, bool recursive = false, bool includeInactive = false)
        where T : Object
    {
        if (gameObject == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < gameObject.transform.childCount; i++)
            {
                var transform = gameObject.transform.GetChild(i);
                if (!string.IsNullOrEmpty(name) && transform.name != name) continue;
                var component = transform.GetComponent<T>();
                if (component != null) return component;
            }
        }
        else
        {
            return gameObject.GetComponentsInChildren<T>(includeInactive)
                .FirstOrDefault(component => string.IsNullOrEmpty(name) || component.name == name);
        }

        return null;
    }
    
    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false, bool includeInactive = false)
    {
        var transform = FindChild<Transform>(go, name, recursive, includeInactive);
        return transform == null ? null : transform.gameObject;
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
    
    public static GameObject GetCardResources(UnitInfo unitInfo, Transform parent, float cardSize = 0, Action<PointerEventData> action = null)
    {
        var cardFrame = Managers.Resource.Instantiate("UI/Deck/CardFrame", parent);
        var unitInCard = cardFrame.transform.Find("CardUnit").gameObject;
        
        if (cardFrame.TryGetComponent(out Card card) == false) return null;
        card.UnitInfo = unitInfo;
        
        cardFrame.GetComponent<Image>().sprite = SetCardFrame(unitInfo.Class);
        unitInCard.GetComponent<Image>().sprite = Managers.Resource.Load<Sprite>($"Sprites/Portrait/{unitInfo.Id.ToString()}");

        if (cardSize != 0)
        {
            var unitRect = unitInCard.GetComponent<RectTransform>();
            unitRect.sizeDelta = new Vector2(cardSize, cardSize);
        }
        
        if (action != null) cardFrame.BindEvent(action);

        return cardFrame;
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