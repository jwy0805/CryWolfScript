using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class UI_Loading : UI_Scene
{
    Slider _slider;
    
    protected override async void Init()
    {
        try
        {
            base.Init();
            BindObjects();
            InitUI();
        
            Debug.Log($"[PAD] Downloading {Managers.Resource.ToDownloadSize / (1024 * 1024)} MB of resources.");
            
            var handle = Addressables.DownloadDependenciesAsync("Fast Follow Resources", true);
            while (!handle.IsDone)
            {
                _slider.value = handle.PercentComplete;
                await Task.Delay(100);   
            }

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                Debug.Log("[PAD] Resources downloaded successfully.");
                Managers.Scene.LoadScene(Define.Scene.Login);
            }
            else
            {
                // 다운로드 실패 경고
                Debug.Log("[PAD] Failed to download resources.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    protected override void BindObjects()
    {
        _slider = Util.FindChild(gameObject, "LoadingSlider").GetComponent<Slider>();
    }

    protected override void InitUI()
    {
        
    }
}
