using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceManager
{
    public T Load<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject))
        {
            string name = path;
            int index = name.LastIndexOf('/');
            if (index >= 0)
                name = name.Substring(index + 1);

            GameObject go = Managers.Pool.GetOriginal(name);
            if (go != null)
                return go as T;
        }
        return Resources.Load<T>(path);
    }

    public GameObject Instantiate(string path, Transform parent = null)
    {
        GameObject original = Load<GameObject>($"Prefabs/{path}");
        
        if (original == null)
        {
            Debug.Log($"Failed to load Prefab : {path}");
            return null;
        }

        if (original.GetComponent<Poolable>() != null)
            return Managers.Pool.Pop(original, parent).gameObject;

        GameObject go = Object.Instantiate(original, parent);
        go.name = original.name;
        return go;
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
        Managers.Resource.Destroy(go);
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