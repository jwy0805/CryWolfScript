using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using ServerCore;
using Vector3 = UnityEngine.Vector3;


public class MapManager
{
    public Grid CurrentGrid { get; private set; }
    public int MapId { get; set; } = 1;

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
    
    public async Task LoadMap(int mapId = 1)
    {
        DestroyMap();
        
        string mapName = "Map_" + mapId.ToString("000");
        GameObject go = await Managers.Resource.Instantiate($"Map/{mapName}");
        go.name = "Map";
        CurrentGrid = go.GetComponent<Grid>();
        
        await LoadMapUI(mapId);
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

    private async Task LoadMapUI(int mapId = 1)
    {
        if (_uiMapping.TryGetValue(mapId, out UIType uiType) == false) return;

        switch (uiType)
        {
            case UIType.UIGameSingleWay:
                await Managers.UI.ShowSceneUI<UI_GameSingleWay>();
                break;
            case UIType.UIGameDoubleWay:
                await Managers.UI.ShowSceneUI<UI_GameDoubleWay>();
                break;
            default:
                break;
        }
    }
}