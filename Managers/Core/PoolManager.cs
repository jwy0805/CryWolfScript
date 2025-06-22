using System.Collections;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using UnityEngine;

public class PoolManager
{
    #region Pool
    private class Pool
    {
        private readonly Stack<Poolable> _poolStack = new();

        public GameObject Original { get; private set; }
        public Transform Root { get; set; }
        
        public void Init(GameObject original, int count = 7)
        {
            _poolStack.Clear();
            Original = original;
            Root = new GameObject().transform;
            Root.name = $"{original.name}_Root";

            for (int i = 0; i < count; i++)
                Push(Create());
        }

        Poolable Create()
        {
            var go = Object.Instantiate(Original);
            go.name = Original.name;
            return go.GetOrAddComponent<Poolable>();
        }

        public void Push(Poolable poolable)
        {
            if (poolable == null)
                return;

            poolable.transform.parent = Root;
            poolable.gameObject.SetActive(false);
            poolable.IsUsing = false;
            
            _poolStack.Push(poolable);
        }

        public Poolable Pop(Transform parent)
        {
            var poolable = _poolStack.Count > 0 ? _poolStack.Pop() : Create();
            poolable.gameObject.SetActive(true);
            
            // Initializing
            if (poolable.gameObject.TryGetComponent(out CreatureController cc)) cc.Stat.Targetable = true;
            if (poolable.gameObject.TryGetComponent(out BaseController bc)) bc.State = State.Idle;
            
            // DontDestroyOnLoad 해제 용도
            // if (parent == null)
            //     poolable.transform.parent = Object.FindObjectOfType<Managers>().transform;

            poolable.transform.parent = parent == null ? null : parent;
            poolable.IsUsing = true;

            return poolable;
        }
    }
    #endregion

    private readonly Dictionary<string, Pool> _pool = new();
    private Transform _root;
    
    public void Init()
    {
        if (_root == null)
        {
            _root = new GameObject { name = "@Pool_Root" }.transform;
        }
    }

    private void CreatePool(GameObject original, int count = 7)
    {
        Pool pool = new Pool();
        pool.Init(original, count);
        pool.Root.parent = _root;
        
        _pool.Add(original.name, pool);
    }

    public void Push(Poolable poolable)
    {
        string name = poolable.gameObject.name;
        if (_pool.TryGetValue(name, out var pool) == false)
        {
            Object.Destroy(poolable.gameObject);
            return;
        }
        
        pool.Push(poolable);
    }

    public Poolable Pop(GameObject original, Transform parent = null)
    {
        if (_pool.ContainsKey(original.name) == false)
            CreatePool(original);

        return _pool[original.name].Pop(parent);
    }

    public GameObject GetOriginal(string name)
    {
        return _pool.TryGetValue(name, out var value) == false ? null : value.Original;
    }

    public void Clear()
    {
        _pool.Clear();
        if (_root == null) return;
        
        foreach (Transform child in _root)
        {
            Object.Destroy(child.gameObject);
        }
    }
}
