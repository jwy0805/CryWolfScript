using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers s_instance;
    public static Managers Instance { get { Init(); return s_instance; } }

    #region Contents
    private readonly GameManager _game = new();
    private readonly MapManager _map = new();
    private readonly ObjectManager _obj = new();
    private readonly NetworkManager _network = new();
    private readonly WebManager _web = new();
    private readonly UserManager _user = new();

    public static GameManager Game => Instance._game;
    public static MapManager Map => Instance._map;
    public static ObjectManager Object => Instance._obj;
    public static NetworkManager Network => Instance._network;
    public static WebManager Web => Instance._web;
    public static UserManager User => Instance._user;

    #endregion
    
    #region Core

    private DataManager _data = new();
    private InputManager _input = new();
    private PoolManager _pool = new();
    private ResourceManager _resource = new();
    private SoundManager _sound = new();
    private SceneManagerEx _scene = new();
    private TokenManager _token = new();
    private UIManager _ui = new();

    public static DataManager Data => Instance._data;
    public static InputManager Input => Instance._input;
    public static PoolManager Pool => Instance._pool;
    public static ResourceManager Resource => Instance._resource;
    public static SoundManager Sound => Instance._sound;
    public static SceneManagerEx Scene => Instance._scene;
    public static TokenManager Token => Instance._token;
    public static UIManager UI => Instance._ui;

    #endregion
    
    void Start()
    {
        Init();
    }

    void Update()
    {
        _input.OnUpdate();
        _sound.OnUpdate();
        _network.Update();
    }

    static void Init()
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
            
            s_instance._data.Init();
            s_instance._pool.Init();
            s_instance._sound.Init();
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
