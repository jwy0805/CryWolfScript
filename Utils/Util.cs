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
using Zenject;
using Object = UnityEngine.Object;

/* Last Modified : 24. 09. 09
 * Version : 1.011
 */

public class Util
{
    public static Faction Faction = Faction.Sheep;
    
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

    public static void Inject(object obj)
    {
        var sceneContainer = Object.FindAnyObjectByType<SceneContext>().Container;
        sceneContainer.Inject(obj);
    }

    public static void InjectGameObject(GameObject go)
    {
        var sceneContainer = Object.FindAnyObjectByType<SceneContext>().Container;
        sceneContainer.InjectGameObject(go);
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

    public static Faction FindFactionByUnitId(int unitId)
    {
        return unitId / 500 == 0 ? Faction.Sheep : Faction.Wolf;
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

    public static string ExtractName(string path)
    {
        int idx = path.LastIndexOf('/');
        return idx >= 0 ? path[(idx + 1)..] : path;
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