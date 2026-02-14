using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;
using Zenject;

public class UI_Loading : UI_Scene
{
    private IWebService _webService;
    private TextMeshProUGUI _text;
    
    private readonly List<object> _fastFollowLabels = new()
    {
        "Init",
        "Game",
    };
    
    private enum Texts
    {
        LoadingResourcesText,
        ResourcesCountText
    }

    [Inject]
    public void Construct(IWebService webService)
    {
        _webService = webService;
    }
    
    private void Awake()
    {
        Managers.UI.RegisterLoadingScene(this);
    }
    
    protected override async void Init()
    {
        try
        {
            base.Init();

            BindObjects();

            await Managers.Resource.InitializeAddressablesAsync();
            await Managers.Resource.EnsureTMPSettingsLoadedAsync();
            
            Managers.Localization.InitLanguage(Application.systemLanguage.ToString());
            await InitUIAsync();

            // Version check before loading resources to avoid unnecessary loading if an update is required.
            var appCheck = await Managers.AppVersion.CheckAsync();
            if (appCheck != null)
            {
                if (appCheck.Force)
                {
                    await LoadForceUpdatePopupAsync(appCheck.StoreUrl);
                    return;
                }

                if (appCheck.NeedUpdate)
                {
                    await LoadOptionalUpdatePopupAsync(appCheck.StoreUrl);
                }
            }
            
            var totalCount = await Managers.Resource.GetWarmLoadCountAsync(_fastFollowLabels);
            if (totalCount == 0)
            {
                Debug.LogWarning("[PAD] No Fast-Follow resources found.");
                Managers.Scene.LoadScene(Define.Scene.Login);
                return;
            }

            var ok = await Managers.Resource.WarmLoadLabelsAsync(_fastFollowLabels, (loaded, total) =>
            {
                if (total <= 0) total = totalCount;
                GetText((int)Texts.LoadingResourcesText).text = $"({loaded}/{total})";
            });

            if (ok)
            {
                Debug.Log("[PAD] Fast-Follow assets warm-loaded.");
                Managers.Scene.LoadScene(Define.Scene.Login);
            }
            else
            {
                Debug.LogError("[PAD] Fast-Follow asset warm-load failed.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    protected override void BindObjects()
    {
        Bind<TextMeshProUGUI>(typeof(Texts));
    }

    protected override async Task InitUIAsync()
    {
        var text = GetText((int)Texts.LoadingResourcesText);
        await Managers.Localization.BindLocalizedText(text, "loading_resources_text");
        Debug.Log("[Manager] UI_Loading initialized.");
    }
    
    private async Task LoadForceUpdatePopupAsync(string storeUrl)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifyPopup>();
        await Managers.Localization.UpdateNotifyPopupText(popup, "notify_must_update_message");
        popup.SetYesCallback(() =>
        {
            Application.OpenURL(storeUrl);
            Application.Quit();
        });
    }
    
    private async Task LoadOptionalUpdatePopupAsync(string storeUrl)
    {
        var popup = await Managers.UI.ShowPopupUI<UI_NotifySelectPopup>();
        await Managers.Localization.UpdateNotifySelectPopupText(popup, "notify_need_update_message");
        popup.NoButtonTextKey = "later_text";
        popup.SetYesCallback(() =>
        {
            Application.OpenURL(storeUrl);
        });
        popup.SetNoCallBack(Managers.UI.ClosePopupUI);
    }
}
