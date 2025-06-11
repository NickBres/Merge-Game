using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [Header(" Elements ")]
    [SerializeField] private AudioSource mergeSource;
    [SerializeField] private AudioSource explosionSource;
    [SerializeField] private AudioSource clickSource;
    [SerializeField] private AudioSource coinsSource;
    [SerializeField] private AudioSource errorSource;
    [SerializeField] private AudioSource fuseSource;
    [SerializeField] private AudioSource legendarySource;
    [SerializeField] private AudioSource magicSource;
    [SerializeField] private AudioSource upgradeSource;

    [Header(" Music ")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioClip[] musicClips;



    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        MergeManager.onMergeAnimal += PlayMergeSound;
        OptionsManager.OnSFXChanged += ToggleSFX;
        OptionsManager.OnMusicChanged += ToggleMusic;

        
    }

    void Start()
    {
        OptionsManager.instance.LoadSettings();
        PlayRandomMusic();
    }



    void OnDestroy()
    {
        MergeManager.onMergeAnimal -= PlayMergeSound;
        OptionsManager.OnSFXChanged -= ToggleSFX;
        OptionsManager.OnMusicChanged -= ToggleMusic;
    }

    public void PlayMergeSound(AnimalType animalType, Vector2 position)
    {
        mergeSource.pitch = Random.Range(0.8f, 1.2f);
        mergeSource.Play();
    }

    public void PlayExplosionSound(Vector2 position)
    {
        StopFuseSound();
        explosionSource.Play();
    }

    public void PlayClickSound()
    {
        clickSource.pitch = Random.Range(0.8f, 1.2f);
        clickSource.Play();
    }

    public void PlayCoinsSound()
    {
        coinsSource.Play();
    }

    public void PlayErrorSound()
    {
        errorSource.Play();
    }

    public void PlayFuseSound()
    {
        fuseSource.Play();
        fuseSource.loop = true;
    }

    public void PlayLegendarySound()
    {
        legendarySource.Play();
    }

    public void PlayMagicSound()
    {
        magicSource.Play();
    }

    public void PlayUpgradeSound()
    {
        upgradeSource.Play();
    }

    public void StopFuseSound()
    {
        fuseSource.Stop();
    }



    private void ToggleSFX(bool isOn)
    {
        mergeSource.mute = !isOn;
        explosionSource.mute = !isOn;
        clickSource.mute = !isOn;
        coinsSource.mute = !isOn;
        errorSource.mute = !isOn;
        fuseSource.mute = !isOn;
        legendarySource.mute = !isOn;
        magicSource.mute = !isOn;
        upgradeSource.mute = !isOn;

    }

    private void ToggleMusic(bool isOn)
    {
        musicSource.mute = !isOn;
        if (isOn && !musicSource.isPlaying)
        {
            PlayRandomMusic();
        }
    }

    private void PlayRandomMusic()
    {
        if (musicClips.Length == 0) return;
        AudioClip randomClip = musicClips[Random.Range(0, musicClips.Length)];
        musicSource.clip = randomClip;
        // musicSource.loop = true;
        musicSource.Play();
    }

    void Update()
    {
        if (!musicSource.isPlaying && musicClips.Length > 0)
        {
            PlayRandomMusic();
        }
    }
}
