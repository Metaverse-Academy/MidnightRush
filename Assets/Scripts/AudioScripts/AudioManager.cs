using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip BackgroundMusicClip;
    [SerializeField] private bool loopBackgroundMusic = true;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;
    
    private AudioSource musicSource;
    private AudioSource sfxSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
            PlayBackgroundMusic();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeAudioSources()
    {
        // Create two audio sources: one for music, one for sound effects
        musicSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        
        // Configure music source
        musicSource.loop = loopBackgroundMusic;
        musicSource.volume = musicVolume;
        
        // Configure SFX source
        sfxSource.loop = false;
    }

    private void PlayBackgroundMusic()
    {
        if (BackgroundMusicClip != null && musicSource != null)
        {
            musicSource.clip = BackgroundMusicClip;
            musicSource.Play();
        }
        else
        {
            Debug.LogWarning("Background music clip or music source is null");
        }
    }

    public void PlaySound(AudioClip clip, float volume = 1.0f)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip, volume);
        }
    }

    public void StopBackgroundMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    public void SetMusicVolume(float volume)
    {
        if (musicSource != null)
        {
            musicSource.volume = Mathf.Clamp01(volume);
        }
    }
}