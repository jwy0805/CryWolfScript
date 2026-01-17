using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using Zenject;
using Object = UnityEngine.Object;

public class UIManager
{
    public readonly List<UI_Popup> PopupList = new();
    
    private int _order = 10;
    private UI_Loading _loadingUI = null;
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

    public async Task<T> MakeWorldSpaceUI<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        GameObject gameObject = await Managers.Resource.Instantiate($"UI/WorldSpace/{name}");
        if (parent != null)
        {
            gameObject.transform.SetParent(parent);
        }

        Canvas canvas = gameObject.GetOrAddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;

        return Util.GetOrAddComponent<T>(gameObject);
    }

    public async Task<T> MakeSubItem<T>(Transform parent = null, string name = null) where T : UI_Base
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        GameObject gameObject = await Managers.Resource.Instantiate($"UI/InGame/SubItem/{name}");
        if (parent != null)
        {
            gameObject.transform.SetParent(parent);
        }

        return Util.GetOrAddComponent<T>(gameObject);
    }

    public async Task ShowSceneUI<T>(string name = null) where T : UI_Scene
    {
        try
        {
            if (string.IsNullOrEmpty(name))
            {
                name = typeof(T).Name;
            }

            if (name == "UI_Loading")
            {
                await Managers.Data.InitAsync();
            }
            else
            {
                if (Managers.Resource.InitAddressables == false)
                {
                    Managers.Localization.InitLanguage(Application.systemLanguage.ToString());
                    await Addressables.InitializeAsync().Task;
                    await Addressables.LoadAssetAsync<TMP_Settings>("Externals/TextMesh Pro/Resources/TMP Settings.asset").Task;
                    Managers.Resource.InitAddressables = true;
                }
                
                var key = $"UI/Scene/{name}";
                var sceneUI = await Managers.Resource.InstantiateAsyncFromContainer(key, Root.transform);
                sceneUI.GetComponent<UI_Scene>().Clear();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Admin Log: Managers.Data.InitAsync failed with" + e);
        }
    }

    public void RegisterLoadingScene(UI_Loading loading)
    {
        _loadingUI = loading;
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
    
    public async Task<T> ShowPopupUI<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        var gameObject = await Managers.Resource.Instantiate($"UI/Popup/{name}");
        var popup = Util.GetOrAddComponent<T>(gameObject);
        PopupList.Add(popup);
        
        gameObject.transform.SetParent(Root.transform);
        Util.Inject(popup);

        return popup;
    }
    
    public async Task<T> ShowPopupUiInGame<T>(string name = null) where T : UI_Popup
    {
        if (string.IsNullOrEmpty(name))
        {
            name = typeof(T).Name;
        }

        var gameObject = await Managers.Resource.Instantiate($"UI/InGame/Popup/{name}");
        var popup = Util.GetOrAddComponent<T>(gameObject);
        PopupList.Add(popup);
        
        gameObject.transform.SetParent(Root.transform);
        Util.Inject(popup);

        return popup;
    }

    public async Task ShowErrorPopup(string errorMessage, Action callback = null)
    {
        var popup = await ShowPopupUI<UI_NotifyPopup>();
        popup.MessageText = errorMessage;
        if (callback != null)
        {
            popup.SetYesCallback(callback);
        }
    }
    
    public async Task ShowNotifyPopup(string titleKey, string messageKey, Action callback = null)
    {
        var popup = await ShowPopupUI<UI_NotifyPopup>();
        await Managers.Localization.UpdateNotifyPopupText(popup, messageKey, titleKey);
        if (callback != null)
        {
            popup.SetYesCallback(callback);
        }
    }
    
    public async Task ShowNotifySelectPopup(string titleKey, string messageKey, Action yesCallback, Action noCallback)
    {
        var popup = await ShowPopupUI<UI_NotifySelectPopup>();
        await Managers.Localization.UpdateNotifySelectPopupText(popup, messageKey, titleKey);
        popup.SetYesCallback(yesCallback);
        popup.SetNoCallBack(noCallback);
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

    public void ClosePopupUI<T>()
    {
        if (PopupList.Count == 0) return;
        
        var popup = PopupList.FirstOrDefault(pop => pop is T);
        if (popup == null) return;
        PopupList.Remove(popup);
        Managers.Resource.Destroy(popup.gameObject);
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
