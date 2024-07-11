using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Numerics;
using Google.Protobuf.Protocol;
using ServerCore;
using Vector3 = UnityEngine.Vector3;


public class MapManager
{
    public Grid CurrentGrid { get; private set; }
    public int MapId { get; set; }

    public enum UIType
    {
        UIGameSingleWay,
        UIGameDoubleWay
    }
    
    private Dictionary<int, UIType> _uiMapping = new()
    {
        { 1, UIType.UIGameSingleWay },
        { 2, UIType.UIGameDoubleWay }
    };
    
    public void LoadMap(int mapId = 1)
    {
        DestroyMap();
        
        string mapName = "Map_" + mapId.ToString("000");
        GameObject go = Managers.Resource.Instantiate($"Map/{mapName}");
        go.name = "Map";

        CurrentGrid = go.GetComponent<Grid>();
        LoadMapUI(mapId);
    }

    public void DestroyMap()
    {
        GameObject map = GameObject.Find("Map");
        if (map != null)
        {
            Managers.Resource.Destroy(map);
            CurrentGrid = null;
        }
    }

    private void LoadMapUI(int mapId = 1)
    {
        if (_uiMapping.TryGetValue(mapId, out UIType uiType) == false) return;

        switch (uiType)
        {
            case UIType.UIGameSingleWay:
                Managers.UI.ShowSceneUI<UI_GameDoubleWay>();
                break;
            case UIType.UIGameDoubleWay:
                Managers.UI.ShowSceneUI<UI_GameSingleWay>();
                break;
            default:
                break;
        }
    }
}