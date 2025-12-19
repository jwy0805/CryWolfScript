using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf.Protocol;
using Moq;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class SoundManager 
{
	private AudioSource[] _audioSources = new AudioSource[(int)Define.Sound.MaxCount];
    private Dictionary<string, AudioClip> _audioClips = new();
    private Define.Scene _sceneType = Define.Scene.Unknown;
    private Faction _currentFaction = Faction.None;
    private string _currentBgmName = string.Empty;
    private float _musicVolume = 1f;
    private float _sfxVolume = 1f;
    private CancellationTokenSource _bgmFadeCts;
    private float _bgmFade = 1f; // 0~1 페이드 계수
    private const float BgmFadeDuration = 1.0f;
    
    
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

        for (int i = 0; i < (int)Define.Sound.MaxCount; i++)
        {
	        var go = new GameObject(((Define.Sound)i).ToString());
	        go.transform.SetParent(_root, false);
	        _audioSources[i] = go.AddComponent<AudioSource>();
        }
        
        var bgmSource = _audioSources[(int)Define.Sound.Bgm];
        if (bgmSource != null) bgmSource.loop = true;
        
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
        
        _musicVolume = PlayerPrefs.GetFloat("MusicVolume", 1f);
        _sfxVolume = PlayerPrefs.GetFloat("SfxVolume", 1f);

        _bgmFade = 1f;
        SetMusicVolume(_musicVolume, false);
        SetSfxVolume(_sfxVolume, false);
        
        _sceneType = BaseScene.SceneType;
        _currentFaction = Util.Faction;
        
        PlayBgm(_sceneType, _currentFaction);
    }

    public void OnUpdate()
    {
	    Define.Scene currentSceneType = BaseScene.SceneType;
	    Faction currentFaction = Util.Faction;
	    
	    bool sceneChanged = _sceneType != currentSceneType;
	    bool factionChanged = _currentFaction != currentFaction;

	    if (sceneChanged || NeedRefreshBgm(currentSceneType, factionChanged))
	    {
		    _sceneType = currentSceneType;
		    _currentFaction = currentFaction;
		    PlayBgm(_sceneType, _currentFaction);
	    }

	    // 재생 끝난 sfx 회수
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

    public void SetMusicVolume(float volume, bool save = true)
    {
	    volume = Mathf.Clamp01(volume);
	    _musicVolume = volume;
		
	    var bgmSource = _audioSources[(int)Define.Sound.Bgm];
	    if (bgmSource != null)
	    {
		    bgmSource.volume = _musicVolume * _bgmFade;
	    }
		
	    if (save)
	    {
		    PlayerPrefs.SetFloat("MusicVolume", volume);
	    }
    }
    
    public void SetSfxVolume(float volume, bool save = true)
	{
	    volume = Mathf.Clamp01(volume);
	    _sfxVolume = volume;
		
	    var sfxSource = _audioSources[(int)Define.Sound.Effect];
	    if (sfxSource != null)
	    {
		    sfxSource.volume = _sfxVolume;
	    }
	    
	    if (save)
	    {
		    PlayerPrefs.SetFloat("SfxVolume", volume);
	    }
	}
    
    private bool NeedRefreshBgm(Define.Scene scene, bool factionChanged)
    {
	    if (!factionChanged) return false;
	    return scene is Define.Scene.MainLobby or Define.Scene.FriendlyMatch;
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
	
	private void PlayBgm(Define.Scene scene, Faction faction)
	{
		var bgmName = GetBgmName(scene, faction); // <-- faction 사용
		if (string.IsNullOrEmpty(bgmName)) return;

		var bgmSource = _audioSources[(int)Define.Sound.Bgm];
		if (bgmSource == null) return;

		// 같은 트랙이면 재시작x, 볼륨만 조정
		if (_currentBgmName == bgmName && bgmSource.clip != null && bgmSource.isPlaying)
		{
			bgmSource.volume = _musicVolume;
			return;
		}

		// 이전 페이드가 있다면 확실히 정리
		_bgmFadeCts?.Cancel();
		_bgmFadeCts?.Dispose();

		_currentBgmName = bgmName;

		_bgmFadeCts = new CancellationTokenSource();
		var token = _bgmFadeCts.Token;

		_ = FadeToNewBgmAsync(bgmSource, bgmName, BgmFadeDuration, token);
	}

	private string GetBgmName(Define.Scene scene, Faction faction)
	{
		return scene switch
		{
			Define.Scene.Game => "muffin_man",
			Define.Scene.MainLobby => faction == Faction.Sheep ? "lobby_sheep" : "lobby_wolf",
			Define.Scene.FriendlyMatch => faction == Faction.Sheep ? "lobby_sheep" : "lobby_wolf",
			Define.Scene.SinglePlay => faction == Faction.Sheep ? "lobby_sheep" : "lobby_wolf",
			Define.Scene.MatchMaking => "match_making",
			_ => string.Empty
		};
	}

	public async Task FadeToNewBgmAsync(AudioSource source, string bgmName, float duration, CancellationToken token)
	{
		try
		{
			float t = 0;
			float startFade = _bgmFade;

			while (t < duration)
			{
				token.ThrowIfCancellationRequested();
				t += Time.unscaledDeltaTime;
				float lerp = Mathf.Clamp01(t / duration);

				_bgmFade = Mathf.Lerp(startFade, 0f, lerp);
				source.volume = _musicVolume * _bgmFade;
				
				await Task.Yield();
			}

			_bgmFade = 0;
			source.volume = 0;
			source.Stop();
			source.clip = null;

			// 새 클립 로드
			AudioClip newClip = await GetOrAddAudioClip(bgmName, Define.Sound.Bgm);
			token.ThrowIfCancellationRequested();
			if (newClip == null) return;

			source.clip = newClip;
			source.loop = true;
			source.Play();

			t = 0f;
			while (t < duration)
			{
				token.ThrowIfCancellationRequested();
				t += Time.unscaledDeltaTime;
				float lerp = Mathf.Clamp01(t / duration);
				
				_bgmFade = Mathf.Lerp(0f, 1f, lerp);
				source.volume = _musicVolume * _bgmFade;

				await Task.Yield();
			}

			_bgmFade = 1;
			source.volume = _musicVolume;
		}
		catch (OperationCanceledException) { }
		catch (Exception e)
		{
			Debug.LogError($"[Sound Manager]: {e}");
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
		ConfigureSource(source, clip, volume, pitch, pitchJitter, spatialBlend, worldPos, follow, loop);
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

	private void ConfigureSource(AudioSource source, AudioClip clip, float baseVolume, float pitch, float pitchJitter,
		float spatialBlend, Vector3? worldPos, Transform follow, bool loop)
	{
		var p = pitchJitter > 0f ? pitch + Random.Range(-pitchJitter, pitchJitter) : pitch;
		baseVolume = Mathf.Clamp01(baseVolume);
		
		source.clip = clip;
		source.volume = baseVolume * _sfxVolume;
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
	
	public void Clear()
	{
		// 1) 루프 SFX 전부 정지 + 풀로 반환
		if (_loopingSfx.Count > 0)
		{
			foreach (var kv in _loopingSfx)
			{
				var src = kv.Value;
				if (src != null)
					ForceStopAndRelease(src);
			}
			_loopingSfx.Clear();
		}

		// 2) 재생 중인(일회성) SFX 전부 정지 + 풀로 반환
		if (_activeSfx.Count > 0)
		{
			_tmpList ??= new List<AudioSource>(_activeSfx.Count);
			_tmpList.Clear();
			foreach (var src in _activeSfx)
				_tmpList.Add(src);

			foreach (var src in _tmpList)
				if (src != null)
					ForceStopAndRelease(src);

			_activeSfx.Clear();
		}

		// 3) per-clip voice tracking 정리
		_voicesPerClip.Clear();

		// 4) 원샷용 Effect AudioSource 정리 (BGM은 건드리지 않음)
		var effectSource = _audioSources[(int)Define.Sound.Effect];
		if (effectSource != null)
		{
			effectSource.Stop();
			effectSource.clip = null;
		}

		// 5) SFX 클립 캐시만 비움 (BGM mp3는 여기 저장하지 않음)
		_audioClips.Clear();

		// 6) 풀에 남아있는 소스들도 안전하게 기본 상태로 리셋 (선택)
		foreach (var src in _sfxPool)
		{
			if (src == null) continue;
			src.Stop();
			src.clip = null;
			src.loop = false;
			src.transform.SetParent(_sfxPoolRoot, false);
		}
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
