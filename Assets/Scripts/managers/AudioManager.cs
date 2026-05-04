using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource ambientSource;
    [SerializeField] private AudioSource fxSource;

    [Header("Default Volumes")]
    [Range(0f, 1f)] [SerializeField] private float musicVolume = 0.5f;
    [Range(0f, 1f)] [SerializeField] private float fxVolume    = 1f;

    void Awake()
    {
        // Singleton — survive scene loads
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ambientSource.volume = musicVolume;
        ambientSource.loop   = true;
        fxSource.volume    = fxVolume;
    }

    // ---- Music ----

    public void PlayMusic(AudioClip clip, bool restart = false)
    {
        if (clip == null) return;
        if (!restart && ambientSource.clip == clip && ambientSource.isPlaying) return;

        ambientSource.clip = clip;
        ambientSource.Play();
    }

    public void StopMusic() => ambientSource.Stop();

    public void PauseMusic() => ambientSource.Pause();

    public void ResumeMusic() => ambientSource.UnPause();

    // ---- FX ----

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

    // ---- Volume ----

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        ambientSource.volume = musicVolume;
    }

    public void SetFXVolume(float volume)
    {
        fxVolume = Mathf.Clamp01(volume);
        fxSource.volume = fxVolume;
    }

    public float MusicVolume => musicVolume;
    public float FXVolume    => fxVolume;
}
