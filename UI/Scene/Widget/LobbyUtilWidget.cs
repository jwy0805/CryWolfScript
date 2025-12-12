using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUtilWidget
{
    private Dictionary<string, GameObject> _tabButtonDict;
    private Dictionary<string, GameObject> _arrangeButtonDict;
    private Dictionary<string, GameObject> _bottomButtonDict;
    private Dictionary<string, GameObject> _bottomButtonFocusDict;
    
    private readonly float _modeChangeTime = 0.25f;

    public void BindViews(
        Dictionary<string, GameObject> tabButtonDict,
        Dictionary<string, GameObject> arrangeButtonDict,
        Dictionary<string, GameObject> bottomButtonDict,
        Dictionary<string, GameObject> bottomButtonFocusDict)
    {
        _tabButtonDict = tabButtonDict;
        _arrangeButtonDict = arrangeButtonDict;
        _bottomButtonDict = bottomButtonDict;
        _bottomButtonFocusDict = bottomButtonFocusDict;
    }
    
    // Animations and Other Methods, 공통 메서드들
    public void GetCountText(Transform parent, int count)
    {
        var countText = Util.FindChild(parent.gameObject, "CountText", true);
        countText.GetComponent<TextMeshProUGUI>().text = count.ToString();
    }
    
    public void SetActivePanels(Dictionary<string, GameObject> dictionary, string[] uiNames)
    {
        foreach (var pair in dictionary)
        {
            pair.Value.SetActive(uiNames.Contains(pair.Key));
        }
    }
    
    public void SetArrangeButtonColor(string buttonName)
    {
        foreach (var go in _arrangeButtonDict.Values)
        {
            var buttonImage = go.GetComponent<Image>();
            buttonImage.color = go.name == buttonName ? Color.cyan : Color.white;
        }
    }

    public void SetBottomButton(string buttonName)
    {
        foreach (var pair in _bottomButtonDict)
        {
            pair.Value.SetActive(pair.Key != buttonName);
        }

        foreach (var pair in _bottomButtonFocusDict)
        {
            pair.Value.SetActive(pair.Key == $"{buttonName}Focus");
        }
    }
    
    public void FocusTabButton(string tabButtonName)
    {
        foreach (var go in _tabButtonDict.Values)
        {
            var tabFocus = Util.FindChild(go, "TabFocus", true, true);
            var buttonText = go.GetComponent<TextMeshProUGUI>();
            
            if (go.name == tabButtonName)
            {
                tabFocus.SetActive(true);
                buttonText.color = Color.white;
            }
            else
            {
                tabFocus.SetActive(false);
                buttonText.color = new Color(74/255f, 172/255f, 247/255f);
            }
        }
    }

    #region Animations

    public IEnumerator ShakeModeSelectButtons(RectTransform left, RectTransform right)
    {
        if (left == null || right == null) yield break;
        
        const float amplitude = 20f; // 바깥으로 벌어지는 거리
        const float speed = 3.0f;

        var leftOrigin = left.anchoredPosition;
        var rightOrigin = right.anchoredPosition;
        
        float t = 0;

        while (true)
        {
            t += Time.deltaTime * speed;
            
            // 0 -> 1 -> 0 으로 변하는 값 (ease-in-out)
            float normalized = (Mathf.Sin(t) + 1f) * 0.5f;
            float outward = normalized * amplitude;

            left.anchoredPosition = leftOrigin + Vector2.left * outward;
            right.anchoredPosition = rightOrigin + Vector2.right * outward;
            
            yield return null;
        }
    }
    
    public IEnumerator MoveModeIcons(List<GameObject> modes, int modeIndex)
    {
        var startAnchors = new Vector2[modes.Count];
        var targetAnchors = new Vector2[modes.Count];
        var anchorPositions = new Vector2[] { new(0.17f, 0.17f), new(0.5f, 0.5f), new(0.83f, 0.83f) };

        for (var i = 0; i < modes.Count; i++)
        {
            var targetIndex = (modeIndex + i) % modes.Count;
            var rect = modes[i].GetComponent<RectTransform>();
            startAnchors[i] = new Vector2(rect.anchorMin.x, rect.anchorMin.y);
            targetAnchors[i] = anchorPositions[targetIndex];
        }

        float elapsedTime = 0;
        
        while (elapsedTime < _modeChangeTime)
        {
            elapsedTime += Time.deltaTime;
            var t = Mathf.Clamp01(elapsedTime / _modeChangeTime);

            for (var i = 0; i < modes.Count; i++)
            {
                var newX = Mathf.Lerp(startAnchors[i].x, targetAnchors[i].x, t);
                var rect = modes[i].GetComponent<RectTransform>();
                rect.anchorMin = new Vector2(newX, rect.anchorMin.y);
                rect.anchorMax = new Vector2(newX, rect.anchorMax.y);
            }

            yield return null;
        }

        for (var i = 0; i < modes.Count; i++)
        {
            var rect = modes[i].GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(targetAnchors[i].x, rect.anchorMin.y);
            rect.anchorMax = new Vector2(targetAnchors[i].y, rect.anchorMax.y);
        }

        foreach (var mode in modes)
        {
            var iconRect = mode.transform.GetChild(0).GetComponent<RectTransform>();
            
            if (Mathf.Approximately(mode.GetComponent<RectTransform>().anchorMin.x, 0.5f))
            {
                Util.FindChild(mode, $"{mode.name}Text", false, true).SetActive(true);
                iconRect.anchorMin = new Vector2(iconRect.anchorMin.x, 0.66f);
                iconRect.anchorMax = new Vector2(iconRect.anchorMax.x, 0.66f);
            }
            else
            {
                iconRect.anchorMin = new Vector2(iconRect.anchorMin.x, 0.5f);
                iconRect.anchorMax = new Vector2(iconRect.anchorMax.x, 0.5f);
                var go = Util.FindChild(mode, $"{mode.name}Text", false, true);
                if (go != null) go.SetActive(false);
            }
        }
    }

    #endregion
    
    #region ordering

    public List<OwnedUnitInfo> OrderOwnedUnits(List<OwnedUnitInfo> units, Define.ArrangeMode mode)
    {
        switch (mode)
        {
            default:
            case Define.ArrangeMode.All:
                return units
                    .Where(info => info.UnitInfo.Faction == Util.Faction)
                    .OrderBy(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id).ToList();
            case Define.ArrangeMode.Summary:
                return units
                    .Where(info => info.UnitInfo.Faction == Util.Faction)
                    .GroupBy(info => info.UnitInfo.Species)
                    .Select(group => group.OrderByDescending(info => info.UnitInfo.Level).First())
                    .ToList();
            case Define.ArrangeMode.Class:
                return units
                    .Where(info => info.UnitInfo.Faction == Util.Faction)
                    .OrderByDescending(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id).ToList();
            case Define.ArrangeMode.Count:
                return units
                    .Where(info => info.UnitInfo.Faction == Util.Faction)
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.UnitInfo.Class)
                    .ThenBy(info => info.UnitInfo.Id).ToList();
        }
    }

    public List<OwnedSheepInfo> OrderOwnedSheep(List<OwnedSheepInfo> sheep, Define.ArrangeMode mode)
    {
        switch (mode)
        {
            default:
            case Define.ArrangeMode.All:
            case Define.ArrangeMode.Summary:
                return sheep
                    .OrderBy(info => info.SheepInfo.Class)
                    .ThenBy(info => info.SheepInfo.Id).ToList();
            case Define.ArrangeMode.Class:
                return sheep
                    .OrderByDescending(info => info.SheepInfo.Class)
                    .ThenBy(info => info.SheepInfo.Id).ToList();
            case Define.ArrangeMode.Count:
                return sheep
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.SheepInfo.Class)
                    .ThenBy(info => info.SheepInfo.Id).ToList();
        }
    }

    public List<OwnedEnchantInfo> OrderOwnedEnchants(List<OwnedEnchantInfo> enchants, Define.ArrangeMode mode)
    {
        switch (mode)
        {
            default:
            case Define.ArrangeMode.All:
            case Define.ArrangeMode.Summary:
                return enchants
                    .OrderBy(info => info.EnchantInfo.Class)
                    .ThenBy(info => info.EnchantInfo.Id).ToList();
            case Define.ArrangeMode.Class:
                return enchants
                    .OrderByDescending(info => info.EnchantInfo.Class)
                    .ThenBy(info => info.EnchantInfo.Id).ToList();
            case Define.ArrangeMode.Count:
                return enchants
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.EnchantInfo.Class)
                    .ThenBy(info => info.EnchantInfo.Id).ToList();
        }
    }

    public List<OwnedCharacterInfo> OrderOwnedCharacters(List<OwnedCharacterInfo> characters, Define.ArrangeMode mode)
    {
        switch (mode)
        {
            default:
            case Define.ArrangeMode.All:
            case Define.ArrangeMode.Summary:
                return characters
                    .OrderBy(info => info.CharacterInfo.Class)
                    .ThenBy(info => info.CharacterInfo.Id).ToList();
            case Define.ArrangeMode.Class:
                return characters
                    .OrderByDescending(info => info.CharacterInfo.Class)
                    .ThenBy(info => info.CharacterInfo.Id).ToList();
            case Define.ArrangeMode.Count:
                return characters
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.CharacterInfo.Class)
                    .ThenBy(info => info.CharacterInfo.Id).ToList();
        }
    }
    
    public List<OwnedMaterialInfo> OrderOwnedMaterials(List<OwnedMaterialInfo> materials, Define.ArrangeMode mode)
    {
        switch (mode)
        {
            default:
            case Define.ArrangeMode.All:
            case Define.ArrangeMode.Summary:
                return materials
                    .OrderBy(info => info.MaterialInfo.Class)
                    .ThenBy(info => info.MaterialInfo.Id).ToList();
            case Define.ArrangeMode.Class:
                return materials
                    .OrderByDescending(info => info.MaterialInfo.Class)
                    .ThenBy(info => info.MaterialInfo.Id).ToList();
            case Define.ArrangeMode.Count:
                return materials
                    .OrderByDescending(info => info.Count)
                    .ThenBy(info => info.MaterialInfo.Class)
                    .ThenBy(info => info.MaterialInfo.Id).ToList();
        }
    }
    
    public List<T> OrderAssetList<T>(List<T> assetList, Define.ArrangeMode mode) where T : IAsset
    {
        switch (mode)
        {
            default:
            case Define.ArrangeMode.All:
            case Define.ArrangeMode.Summary:
            case Define.ArrangeMode.Count:
                return assetList.OrderBy(info => info.Class).ThenBy(info => info.Id).ToList();
            case Define.ArrangeMode.Class:
                return assetList.OrderByDescending(info => info.Class).ThenBy(info => info.Id).ToList();
        }
    }

    #endregion
    
    public void Dispose()
    {
        
    }
}