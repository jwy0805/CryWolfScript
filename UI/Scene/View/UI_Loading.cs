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

public class UI_Loading : UI_Scene
{
    private TextMeshProUGUI _text;
    
    private readonly List<object> _fastFollowLabels = new()
    {
        "LoadPoolLobby"
    };
    
    private enum Texts
    {
        LoadingResourcesText,
        ResourcesCountText
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

            Managers.Localization.InitLanguage(Application.systemLanguage.ToString());
            await Addressables.InitializeAsync().Task;
            await Addressables.LoadAssetAsync<TMP_Settings>("Externals/TextMesh Pro/Resources/TMP Settings.asset").Task;
            Managers.Resource.InitAddressables = true;
            
            BindObjects();
            await InitUIAsync();

            Managers.Resource.InitAddressables = true;
            Debug.Log("[Manager] Addressables initialized.");
            var found = Addressables.ResourceLocators.Any(l =>
                l.Locate("Prefabs/UI/Scene/UI_Loading.prefab", typeof(GameObject), out _));
            Debug.Log($"UI_Loading present in catalog? {found}");

            var locHandle = Addressables.LoadResourceLocationsAsync(_fastFollowLabels, Addressables.MergeMode.Union);
            await locHandle.Task;
            var locations = locHandle.Result;
            var totalCount = locations.Count;
            if (totalCount == 0)
            {
                Debug.LogWarning("[PAD] No Fast-Follow resources found.");
                Managers.Scene.LoadScene(Define.Scene.Login);
                return;
            }

            var loadedCount = 0;
            var loadHandle = Addressables.LoadAssetsAsync<object>(
                _fastFollowLabels, asset =>
                {
                    loadedCount++;
                    GetText((int)Texts.ResourcesCountText).text = $"({loadedCount}/{totalCount})";
                }, 
                Addressables.MergeMode.Union);
            
            await loadHandle.Task;
            
            if (loadHandle.Status is AsyncOperationStatus.Succeeded)
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
}
