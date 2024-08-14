using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

public class UIManager
{
    public readonly List<UI_Popup> PopupList = new();
    
    private int _order = 10;
    // private UI_Scene _sceneUI = null;
    
    public GameObject Root
    {
        get
        {
            GameObject root = GameObject.Find("@UI_Root");
            if (root == null)
                root = new GameObject { name = "@UI_Root" };
            return root;
        }
    }

    public void SetCanvas(GameObject gameObject, bool sort = true)
    {
        Canvas canvas = Util.GetOrAddComponent<Canvas>(gameObject);
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.overrideSorting = true;
        
        if (sort)
        {
            canvas.sortingOrder = _order;
            _order++;
        }
        else
        {
            canvas.sortingOrder = 0;
        }
    }

    public T MakeWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        GameObject gameObject = Managers.Resource.Instantiate($"UI/WorldSpace/{name}");
        if (parent != null)
        {
            gameObject.transform.SetParent(parent);
        }

        Canvas canvas = gameObject.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        return Util.GetOrAddComponent<T>(gameObject);
    }

    public T MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        GameObject gameObject = Managers.Resource.Instantiate($"UI/SubItem/{name}");
        if (parent != null)
        {
            gameObject.transform.SetParent(parent);
        }

        return Util.GetOrAddComponent<T>(gameObject);
    }

    public void ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }
        
        Managers.Resource.InstantiateFromContainer($"UI/Scene/{name}", Root.transform);
    }
    
    private Type GetType<T>()
    {
        var name = typeof(T).Name;

        switch (name)
        {
            case "UI_Login":
                return typeof(LoginViewModel);
        }

        return null;
    }
    
    public T ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        GameObject gameObject = Managers.Resource.Instantiate($"UI/Popup/{name}");
        T popup = Util.GetOrAddComponent<T>(gameObject);
        PopupList.Add(popup);
        
        gameObject.transform.SetParent(Root.transform);

        var sceneContext = Object.FindObjectOfType<SceneContext>().Container;
        sceneContext.Inject(popup);

        return popup;
    }

    public void CloseUpgradePopup()
    {
        if (PopupList.Count == 0) return;

        var deleteList = PopupList.Where(pop => pop is UI_UpgradePopup).ToList();
        foreach (var popup in deleteList)
        {
            PopupList.Remove(popup);
            Managers.Resource.Destroy(popup.gameObject);
            _order--;
        }
    }

    public void ClosePopupUI(UI_Popup popup)
    {
        if (PopupList.Count == 0) return;

        if (!PopupList.Contains(popup)) return;
        PopupList.Remove(popup);
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
        _order--;
    }
    
    public void ClosePopupUI(UI_Popup popup, int delay)
    {
        if (PopupList.Count == 0) return;

        if (!PopupList.Contains(popup)) return;
        PopupList.Remove(popup);
        Managers.Resource.Destroy(popup.gameObject, delay);
        popup = null;
        _order--;
    }
    
    public void ClosePopupUI()
    {
        if (PopupList.Count == 0) return;

        var popup = PopupList[^1];
        PopupList.RemoveAt(PopupList.Count - 1);
        Managers.Resource.Destroy(popup.gameObject);
        popup = null;
        _order--;
    }

    public void CloseAllPopupUI()
    {
        while (PopupList.Count > 0)
        {
            ClosePopupUI();
        }
    }

    public void Clear()
    {
        CloseAllPopupUI();
        // _sceneUI = null;
    }
}
