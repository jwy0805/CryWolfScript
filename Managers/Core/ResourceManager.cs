using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Docker.DotNet.Models;
using JetBrains.Annotations;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using Zenject;
using Object = UnityEngine.Object;

public class ResourceManager
{
    private const string MovedRoot = "Assets/Resources_moved/";
    
    private bool _addressablesInitialized = false;
    private bool _tmpSettingsLoaded = false;

    private readonly Dictionary<string, AsyncOperationHandle> _warmLoadHandles = new();
    
    /// <summary>
    /// Asynchronously Loading according to Addressables -> Pool -> Resources.
    /// If Fast-Follow / ODR bundles are needed, pending here.
    /// </summary>
    public async Task<T> LoadAsync<T>(string key, string extension = "png") where T : Object
    {
        // Check pool first
        if (typeof(T) == typeof(GameObject))
        {
            string name = Util.ExtractName(key);
            GameObject pooledObject = Managers.Pool.GetOriginal(name);
            if (pooledObject != null) 
                return pooledObject as T;
        }

        if (Managers.Network.UseAddressables)
        {
            // Addressables
            AsyncOperationHandle<T> handle = Addressables.LoadAssetAsync<T>($"{key}.{extension}");
            await handle.Task;
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                return handle.Result;
            }

            Debug.LogError($"Failed to load asset from Addressables: {key}.{extension}");
            Addressables.Release(handle); 
        }
        else
        {
#if UNITY_EDITOR
            // Editor -> Resources_Moved
            string editorPath = FindInMovedFolder<T>(key);
            if (editorPath != null)
                return AssetDatabase.LoadAssetAtPath<T>(editorPath);
#endif
        }

        Debug.Log("Not exists in Addressables : " + key);
        return null;
    }
    
#if UNITY_EDITOR
    // Editor 전용: .prefab / .png / .asset 등 확장자 자동 추적
    private static string FindInMovedFolder<T>(string key) where T : Object
    {
        // "Prefabs/UI/Menu/StartButton" → dir="Prefabs/UI/Menu", name="StartButton"
        string cleaned = key.TrimStart('/');
        string wantedDir = System.IO.Path.GetDirectoryName(cleaned)?.Replace('\\', '/');      
        string wantedName = System.IO.Path.GetFileNameWithoutExtension(cleaned);            
        
        // Searching directory
        string absoluteDir = string.IsNullOrEmpty(wantedDir) ? MovedRoot.TrimEnd('/') : $"{MovedRoot}{wantedDir}";
        if (Directory.Exists(absoluteDir))
        {
            string match = Directory
                .GetFiles(absoluteDir, $"{wantedName}.*", SearchOption.TopDirectoryOnly)
                .FirstOrDefault();
            if (string.IsNullOrEmpty(match) == false) return match.Replace('\\', '/');
        }

        // Searching all guids
        string[] guids = AssetDatabase.FindAssets(wantedName, new[] { MovedRoot });
        foreach (var guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            string relativeDir = System.IO.Path.GetDirectoryName(assetPath)?
                .Replace(MovedRoot, string.Empty)
                .Replace('\\', '/');
            
            if (System.IO.Path.GetFileNameWithoutExtension(assetPath).Equals(wantedName, StringComparison.OrdinalIgnoreCase) &&
                (relativeDir ?? string.Empty).Equals(wantedDir ?? string.Empty, StringComparison.OrdinalIgnoreCase))
            {
                return assetPath;
            }
        }

        return null;
    }
#endif
    
    public async Task<GameObject> Instantiate(string key, Transform parent = null)
    {
        GameObject original = await LoadAsync<GameObject>($"Prefabs/{key}", "prefab");
        
        if (original == null)
        {
            Debug.Log($"Failed to load Prefab : {key}");
            return null;
        }

        if (original.TryGetComponent(out Poolable _))
        {
            return Managers.Pool.Pop(original, parent).gameObject;
        }

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
    }   
    
    public async Task<GameObject> Instantiate(string key, Vector3 position)
    {
        GameObject original = await LoadAsync<GameObject>($"Prefabs/{key}", "prefab");
        
        if (original == null)
        {
            Debug.Log($"Failed to load Prefab : {key}");
            return null;
        }

        if (original.TryGetComponent(out Poolable _))
        {
            return Managers.Pool.Pop(original).gameObject;
        }

        GameObject go = Object.Instantiate(original, position, Quaternion.identity);
        go.name = original.name;
        return go;
    }

    public async Task<GameObject> InstantiateZenjectAsync(string key, Transform parent = null)
    {
        var prefab = await LoadAsync<GameObject>($"Prefabs/{key}", "prefab");
        if (prefab == null)
        {
            Debug.LogError($"Failed to load Prefab : {key}");
            return null;
        }

        var sceneContext = Object.FindAnyObjectByType<SceneContext>();
        if (sceneContext == null)
        {
            Debug.LogError($"SceneContext not found. '{key}' requires Scene bindings.");
            return null;
        }

        return sceneContext.Container.InstantiatePrefab(prefab, parent);
    }
    
    public void Destroy(GameObject go, float time)
    {
        if (go == null)
        {
            return;
        }

        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        }
        
        Object.Destroy(go, time);
    }
    
    public void Destroy(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
            return;
        }
        
        Object.Destroy(go);
    }
    
    public IEnumerator Despawn(GameObject go, float time)
    {
        yield return new WaitForSeconds(time * Time.deltaTime);
        Destroy(go);
    }

    IEnumerator DestroyAndPush(GameObject go, float time)
    {
        yield return new WaitForSeconds(time * Time.deltaTime);
        
        Poolable poolable = go.GetComponent<Poolable>();
        if (poolable != null)
        {
            Managers.Pool.Push(poolable);
        }
        
        Object.Destroy(go, time * Time.deltaTime);
    }
    
    private bool ExistsInAddressables(string key)
    {
        return Addressables.ResourceLocators.Any(loc => loc.Keys.Contains(key));
    }

    public async Task InitializeAddressablesAsync()
    {
        if (_addressablesInitialized) return;

        var initHandle = Addressables.InitializeAsync();
        await initHandle.Task;
        if (initHandle.Status != AsyncOperationStatus.Succeeded) throw new Exception($"Addressables initialization failed: {initHandle.Status}");
        
        Addressables.Release(initHandle);
        _addressablesInitialized = true;
    }

    public async Task EnsureTMPSettingsLoadedAsync()
    {
        if (!_tmpSettingsLoaded) return;

        await InitializeAddressablesAsync();
        
        var handle = Addressables.LoadAssetAsync<TMP_Settings>("Externals/TextMesh Pro/Resources/TMP Settings.asset");
        await handle.Task;
        
        if (handle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load TMP Settings: {handle.Status}");
            Addressables.Release(handle); 
        }

        _tmpSettingsLoaded = true;
    }

    public async Task<int> GetWarmLoadCountAsync(IReadOnlyList<object> labels)
    {
        await InitializeAddressablesAsync();

        var locHandle = Addressables.LoadResourceLocationsAsync(labels, Addressables.MergeMode.Union);
        await locHandle.Task;
        
        if (locHandle.Status != AsyncOperationStatus.Succeeded)
        {
            Debug.LogError($"Failed to load resource locations for labels: {string.Join(", ", labels)}");
            Addressables.Release(locHandle);
            return 0;
        }
        
        int count = locHandle.Result.Count;
        Addressables.Release(locHandle);
        return count;
    }

    public async Task<bool> WarmLoadLabelsAsync(
        IReadOnlyList<object> labels,
        Action<int, int> onProgress = null,
        bool keepAlive = true)
    {
        await InitializeAddressablesAsync();

        string key = string.Join(",", labels.Select(obj => obj.ToString()));
        // 이미 로드된 경우
        if (keepAlive && _warmLoadHandles.TryGetValue(key, out var cached) &&
            cached.IsValid() &&
            cached.IsDone &&
            cached.Status == AsyncOperationStatus.Succeeded)
        {
            onProgress?.Invoke(1, 1);
            return true;
        }

        var locHandle = Addressables.LoadResourceLocationsAsync(labels, Addressables.MergeMode.Union);
        await locHandle.Task;

        if (locHandle.Status != AsyncOperationStatus.Succeeded || locHandle.Result == null)
        {
            Addressables.Release(locHandle);
            return false;
        }

        int totalCount = locHandle.Result.Count;
        Addressables.Release(locHandle);

        if (totalCount <= 0)
        {
            onProgress?.Invoke(0, 0);
            return true;
        }
        
        // Warm Load
        int loadedCount = 0;
        onProgress?.Invoke(loadedCount, totalCount);
        var loadHandle = Addressables.LoadAssetsAsync<Object>(labels, _ =>
        {
            loadedCount++;
            onProgress?.Invoke(loadedCount, totalCount);
        }, Addressables.MergeMode.Union, true);

        if (keepAlive)
        {
            _warmLoadHandles[key] = loadHandle;
        }

        await loadHandle.Task;

        bool ok = loadHandle.Status == AsyncOperationStatus.Succeeded;
        if (!keepAlive)
        {
            Addressables.Release(loadHandle);
        }

        return ok;
    }

    public void ReleaseWarmLoad(string labelsKeys)
    {
        if (_warmLoadHandles.TryGetValue(labelsKeys, out var handle))
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
            
            _warmLoadHandles.Remove(labelsKeys);
        } 
    }
    
    public void ReleaseAllWarmLoads()
    {
        foreach (var handle in _warmLoadHandles.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }
        
        _warmLoadHandles.Clear();
    }
}