using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
    
    public bool InitAddressables { get; set; }

    private bool ExistsInAddressables(string key)
    {
        return Addressables.ResourceLocators.Any(loc => loc.Keys.Contains(key));
    }

    
    
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
}