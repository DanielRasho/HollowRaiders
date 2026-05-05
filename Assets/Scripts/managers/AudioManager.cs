using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource fxSource;

    [Header("Default Volumes")]
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float fxVolume    = 1f;

    [Header("Music Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    private AudioSource activeSource;
    private AudioSource inactiveSource;

    private Coroutine fadeRoutine;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Debug.Log($"Duplicate AudioManager destroyed in scene: {gameObject.scene.name}");
            Destroy(gameObject);
            return;
        }

        Init();
    }

    private void Init()
    {
        // Setup music sources
        activeSource = musicSourceA;
        inactiveSource = musicSourceB;

        musicSourceA.volume = musicVolume;
        musicSourceB.volume = 0f;

        musicSourceA.loop = true;
        musicSourceB.loop = true;

        // Setup FX
        fxSource.volume = fxVolume;
    }

    #region Music

    public void PlayMusic(AudioClip clip, bool restart = false)
    {
        if (clip == null) return;

        if (!restart && activeSource.clip == clip && activeSource.isPlaying)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(Crossfade(clip));
    }

    private IEnumerator Crossfade(AudioClip newClip)
    {
        inactiveSource.clip = newClip;
        inactiveSource.volume = 0f;
        inactiveSource.Play();

        float time = 0f;
        float startVolume = activeSource.volume;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeDuration;

            activeSource.volume = Mathf.Lerp(startVolume, 0f, t);
            inactiveSource.volume = Mathf.Lerp(0f, musicVolume, t);

            yield return null;
        }

        activeSource.Stop();
        activeSource.volume = musicVolume;

        // Swap sources
        var temp = activeSource;
        activeSource = inactiveSource;
        inactiveSource = temp;
    }

    public void StopMusic()
    {
        activeSource.Stop();
        inactiveSource.Stop();
    }

    public void PauseMusic()
    {
        activeSource.Pause();
        inactiveSource.Pause();
    }

    public void ResumeMusic()
    {
        activeSource.UnPause();
        inactiveSource.UnPause();
    }

    #endregion

    #region FX

    /// <summary>Fire and forget — plays the clip once at current FX volume.</summary>
    public void PlayFX(AudioClip clip)
    {
        if (clip == null) return;
        fxSource.PlayOneShot(clip, fxVolume);
    }

    /// <summary>Play FX at a world position (uses a temporary AudioSource).</summary>
    public void PlayFXAt(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        AudioSource.PlayClipAtPoint(clip, position, fxVolume);
    }

    #endregion

    // ---- Volume ----

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        activeSource.volume = musicVolume;
    }

    public void SetFXVolume(float volume)
    {
        fxVolume = Mathf.Clamp01(volume);
        fxSource.volume = fxVolume;
    }

    public float MusicVolume => musicVolume;
    public float FXVolume    => fxVolume;
}