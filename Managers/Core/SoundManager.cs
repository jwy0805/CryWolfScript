using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class SoundManager 
{
	private AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
    private Dictionary<string, AudioClip> _audioClips = new();
    private Define.Scene _sceneType;
    private float _musicVolume;
    private float _sfxVolume;
    
    
    // SFX field
    private const int SfxPoolSize = 16;
    private Transform _root;
    private Transform _sfxPoolRoot;
    private readonly Queue<AudioSource> _sfxPool = new();
    private readonly HashSet<AudioSource> _activeSfx = new();
    private readonly Dictionary<int, AudioSource> _loopingSfx = new();
    private int _nextHandleId = 1;
    private readonly Dictionary<string, LinkedList<AudioSource>> _voicesPerClip = new();
    private List<AudioSource> _tmpList;
    
    // MP3 Player   -> AudioSource
    // MP3 음원     -> AudioClip
    // 관객(귀)     -> AudioListener

    public void Init()
    {
	    GameObject root = GameObject.Find("@Sound");
        if (root == null)
        {
            root = new GameObject { name = "@Sound" };
            Object.DontDestroyOnLoad(root);
        }

        _root = root.transform;
        
        string[] soundNames = System.Enum.GetNames(typeof(Define.Sound));
        for (int i = 0; i < soundNames.Length - 1; i++)
        {
	        GameObject go = new GameObject { name = soundNames[i] };
	        _audioSources[i] = go.AddComponent<AudioSource>();
	        go.transform.parent = root.transform;
        }

        _audioSources[(int)Define.Sound.Bgm].loop = true;

        if (_sfxPoolRoot == null)
        {
	        var poolObj = new GameObject("SFX_Pool");
	        poolObj.transform.SetParent(_root, false);
	        _sfxPoolRoot = poolObj.transform;

	        for (int i = 0; i < SfxPoolSize; i++)
	        {
		        _sfxPool.Enqueue(CreatePooledSource(_sfxPoolRoot));
	        }
        }
        
        _sceneType = BaseScene.SceneType;
        PlayBgm(_sceneType);
    }

    public void OnUpdate()
    {
	    Define.Scene currentSceneType = BaseScene.SceneType;
	    if (_sceneType != currentSceneType)
	    {
		    _sceneType = currentSceneType;
		    PlayBgm(_sceneType);
	    }

	    if (_activeSfx.Count > 0)
	    {
		    _tmpList ??= new List<AudioSource>(32);
		    _tmpList.Clear();
		    foreach (var source in _activeSfx)
		    {
			    if (!source.isPlaying && !source.loop) 
			    {
				    _tmpList.Add(source);
			    }
		    }

		    foreach (var source in _tmpList)
		    {
				ReleaseSource(source);
		    }
	    }
    }

    public void Clear()
    {
        foreach (AudioSource audioSource in _audioSources)
        {
            audioSource.clip = null;
            audioSource.Stop();
        }
        _audioClips.Clear();
    }

    public async Task Play(string path, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
    {
        AudioClip audioClip = await GetOrAddAudioClip(path, type);
        Play(audioClip, type, pitch);
    }

	public void Play(AudioClip audioClip, Define.Sound type = Define.Sound.Effect, float pitch = 1.0f)
	{
        if (audioClip == null)
            return;

        var source = _audioSources[(int)type];
        source.volume = _musicVolume;
        
		if (type == Define.Sound.Bgm)
		{
			AudioSource audioSource = _audioSources[(int)Define.Sound.Bgm];
			if (audioSource.isPlaying) audioSource.Stop();

			audioSource.pitch = pitch;
			audioSource.clip = audioClip;
			audioSource.Play();
		}
		else
		{
			AudioSource audioSource = _audioSources[(int)Define.Sound.Effect];
			audioSource.pitch = pitch;
			audioSource.PlayOneShot(audioClip);
		}
	}

	private async Task<AudioClip> GetOrAddAudioClip(string path, Define.Sound type = Define.Sound.Effect)
    {
		if (path.Contains("Sounds/") == false)
			path = $"Sounds/{path}";
		
		if (type == Define.Sound.Bgm)
			return await Managers.Resource.LoadAsync<AudioClip>(path, "mp3");

		if (_audioClips.TryGetValue(path, out var cached))
			return cached;

		var clip = await Managers.Resource.LoadAsync<AudioClip>(path, "wav");
		if (clip != null) _audioClips[path] = clip;
		else Debug.Log($"AudioClip Missing ! {path}");
		return clip;
    }
	
	private void PlayBgm(Define.Scene scene)
	{
		_musicVolume = PlayerPrefs.GetFloat("MusicVolume");
		
		switch(scene)
		{
			case Define.Scene.Game:
				string nameBgm = "muffin_man";
				_ = Play(nameBgm, Define.Sound.Bgm);
				break;
		}
	}

	public async Task<int> PlaySfx(
		string path,
		float volume = 1f,
		float pitch = 1f,
		float pitchJitter = 0f, // 예: 0.03f → ±3%
		bool loop = false,
		Vector3? worldPos = null, // null이면 2D
		Transform follow = null, // 3D 추적대상
		float spatialBlend = 0f, // 0=2D, 1=3D
		int maxVoicesPerClip = 6 // 같은 클립 동시 최대 재생 수
	)
	{
		var clip = await GetOrAddAudioClip(path);
		if (clip == null) return 0;

		var key = path;
		if (!_voicesPerClip.TryGetValue(key, out var list))
		{
			list = new LinkedList<AudioSource>();
			_voicesPerClip[key] = list;
		}
		
		if (maxVoicesPerClip > 0 && list.Count >= maxVoicesPerClip)
		{
			// 최대 동시 재생 수 초과 시 가장 오래된 소스 제거
			var oldestSource = list.First;
			if (oldestSource != null)
			{
				list.RemoveFirst();
				ForceStopAndRelease(oldestSource.Value);
			}
		}
		
		var source = GetSourceFromPool();
		ConfigureSource(source, clip, volume * _sfxVolume, pitch, pitchJitter, spatialBlend, worldPos, follow, loop);
		list.AddLast(source);

		if (loop)
		{
			var id = _nextHandleId++;
			_loopingSfx[id] = source;
			source.Play();
			return id;
		}
		else
		{
			_activeSfx.Add(source);
			source.Play();
			return 0; // 0은 일회성 SFX 재생을 의미
		}
	}	
	
	// Stop looping SFX by handle ID
	public void StopSfx(int handle, float fadeOutSeconds = 0.0f)
	{
		if (!_loopingSfx.TryGetValue(handle, out var source) || source == null) return;
		
		_loopingSfx.Remove(handle);

		if (fadeOutSeconds > 0f)
		{
			_ = FadeOutAndRelease(source, fadeOutSeconds);
		}
		else
		{
			ForceStopAndRelease(source);
		}
	}
	
	private AudioSource CreatePooledSource(Transform parent)
	{
		var go = new GameObject("SFX_AudioSource");
		go.transform.SetParent(parent, false);
		var source = go.GetOrAddComponent<AudioSource>();
		source.playOnAwake = false;
		source.loop = false;
		source.spatialBlend = 0f;
		source.dopplerLevel = 0f;
		source.rolloffMode = AudioRolloffMode.Linear;
		source.minDistance = 1f;
		source.maxDistance = 50f;

		return source;
	}

	private AudioSource GetSourceFromPool()
	{
		return _sfxPool.Count > 0 ? _sfxPool.Dequeue() : CreatePooledSource(_sfxPoolRoot);
	}

	private void ConfigureSource(AudioSource source, AudioClip clip, float volume, float pitch, float pitchJitter,
		float spatialBlend, Vector3? worldPos, Transform follow, bool loop)
	{
		var p = pitchJitter > 0f ? pitch + Random.Range(-pitchJitter, pitchJitter) : pitch;
		source.clip = clip;
		source.volume = Mathf.Clamp01(PlayerPrefs.GetFloat("SfxVolume", 1f));
		source.pitch = Mathf.Clamp(p, -3f, 3f);
		source.loop = loop;
		source.spatialBlend = Mathf.Clamp01(spatialBlend);

		if (follow != null)
		{
			source.transform.SetParent(follow, false);
			source.transform.localScale = Vector3.zero;
		}
		else
		{
			source.transform.SetParent(_sfxPoolRoot, false);
			if (worldPos.HasValue)
			{
				source.transform.position = worldPos.Value;
			}
		}
	}

	private void ReleaseSource(AudioSource source)
	{
		if (source == null) return;

		foreach (var pair in _voicesPerClip)
		{
			if (pair.Value.Contains(source))
			{
				pair.Value.Remove(source);
				break;
			}
		}
		
		source.Stop();
		source.clip = null;
		source.loop = false;
		source.transform.SetParent(_sfxPoolRoot, false);
		_activeSfx.Remove(source);
		_sfxPool.Enqueue(source);
	}

	private void ForceStopAndRelease(AudioSource source)
	{
		if (source == null) return;
		source.Stop();
		ReleaseSource(source);
	}

	private async Task FadeOutAndRelease(AudioSource source, float duration)
	{
		if (source == null) return;
		float t = 0f;
		float start = source.volume;
		
		while (t < duration && source != null)
		{
			t += Time.unscaledDeltaTime;
			if (source != null)
			{
				source.volume = Mathf.Lerp(start, 0f, t / duration);
			}
			
			await Task.Yield();
		}

		if (source != null) source.volume = 0;
		ForceStopAndRelease(source);
	}

	// short 2D sfx
	public Task<int> PlaySfx2D(string path, float volume = 1f, float pitchJitter = 0.02f, int maxVoicesPerClip = 6)
		=> PlaySfx(path, volume, 1f, pitchJitter, false, null, null, 0f, maxVoicesPerClip);
	
	// short 3D sfx
	public Task<int> PlaySfx3D(string path, Vector3 pos, float volume = 1f, float pitchJitter = 0.2f, int maxVoicesPerClip = 6)
		=> PlaySfx(path, volume, 1f, pitchJitter, false, pos, null, 1f, maxVoicesPerClip);
	
	// short 3D sfx that follows a Transform
	public Task<int> StartLoopSfxFollow(string path, Transform follow, float volume = 1f, float pitch = 1f)
		=> PlaySfx(path, volume, pitch, 0f, true, null, follow, 1f, 2);
}
