using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers s_instance;
    private static bool _isSigningIn;
    public static Managers Instance { get { Init(); return s_instance; } }

    #region Contents
    private readonly GameManager _game = new();
    private readonly MapManager _map = new();
    private readonly ObjectManager _obj = new();
    private readonly NetworkManager _network = new();
    private readonly EventManager _event = new();

    public static GameManager Game => Instance._game;
    public static MapManager Map => Instance._map;
    public static ObjectManager Object => Instance._obj;
    public static NetworkManager Network => Instance._network;
    public static EventManager Event => Instance._event;

    #endregion
    
    #region Core

    private readonly DataManager _data = new();
    private readonly InputManager _input = new();
    private readonly PoolManager _pool = new();
    private readonly ResourceManager _resource = new();
    private readonly SoundManager _sound = new();
    private readonly SceneManagerEx _scene = new();
    private readonly UIManager _ui = new();

    public static DataManager Data => Instance._data;
    public static InputManager Input => Instance._input;
    public static PoolManager Pool => Instance._pool;
    public static ResourceManager Resource => Instance._resource;
    public static SoundManager Sound => Instance._sound;
    public static SceneManagerEx Scene => Instance._scene;
    public static UIManager UI => Instance._ui;

    #endregion
    
    private void Start()
    {
        Init();
    }

    private void Update()
    {
        _input.OnUpdate();
        _sound.OnUpdate();
        _network.Update();
    }

    private static async void Init()
    {
        if (s_instance == null)
        {
            GameObject go = GameObject.Find("@Managers");
            if (go == null)
            {
                go = new GameObject { name = "@Managers" };
                go.AddComponent<Managers>();
            }

            DontDestroyOnLoad(go);
            s_instance = go.GetComponent<Managers>();
            
            s_instance._pool.Init();
            s_instance._sound.Init();
            await s_instance._data.InitAsync();
        }
    }
    
    public static void Clear()
    {
        Input.Clear();
        Sound.Clear();
        Scene.Clear();
        Pool.Clear();
        UI.Clear();
    }
}
