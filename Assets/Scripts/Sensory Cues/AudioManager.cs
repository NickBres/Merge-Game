using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;
    [Header(" Elements ")]
    [SerializeField] private AudioSource mergeSource;
    [SerializeField] private AudioSource choirSource;
    [SerializeField] private AudioSource icedSource;
    [SerializeField] private AudioSource iceBreakSource;
    [SerializeField] private AudioSource eggCrackSource;
    [SerializeField] private AudioSource explosionSource;
    [SerializeField] private AudioSource clickSource;
    [SerializeField] private AudioSource coinsSource;
    [SerializeField] private AudioSource errorSource;
    [SerializeField] private AudioSource fuseSource;
    [SerializeField] private AudioSource legendarySource;
    [SerializeField] private AudioSource magicSource;
    [SerializeField] private AudioSource upgradeSource;
    [SerializeField] private AudioSource gameOverSource;
    [SerializeField] private AudioSource newHighScoreSource;
    [SerializeField] private AudioSource cawabungaSource;
    [SerializeField] private AudioSource epicSource;
    [SerializeField] private AudioSource wowSource;


    public void PlayIcedSound()
    {
        icedSource.Play();
    }

    public void PlayChoirSound()
    {
        choirSource.Play();
    }
    public void PlayIceBreakSound()
    {
        iceBreakSource.pitch = Random.Range(0.8f, 1.2f);
        iceBreakSource.Play();
    }

    public void PlayEggCrackSound()
    {
        eggCrackSource.pitch = Random.Range(0.8f, 1.2f);
        eggCrackSource.Play();
    }

    public void PlayGameOverSound()
    {
        gameOverSource.Play();
    }

    public void PlayNewHighScoreSound()
    {
        newHighScoreSource.Play();
    }

    public void PlayCawabungaSound()
    {
        cawabungaSource.Play();
    }

    public void PlayEpicSound()
    {
        epicSource.Play();
    }

    public void PlayWowSound()
    {
        wowSource.Play();
    }

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
        icedSource.mute = !isOn;
        iceBreakSource.mute = !isOn;
        explosionSource.mute = !isOn;
        clickSource.mute = !isOn;
        coinsSource.mute = !isOn;
        errorSource.mute = !isOn;
        fuseSource.mute = !isOn;
        legendarySource.mute = !isOn;
        magicSource.mute = !isOn;
        upgradeSource.mute = !isOn;
        gameOverSource.mute = !isOn;
        newHighScoreSource.mute = !isOn;
        cawabungaSource.mute = !isOn;
        epicSource.mute = !isOn;
        wowSource.mute = !isOn;
        choirSource.mute = !isOn;
        eggCrackSource.mute = !isOn;
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

    public void SpeedUpMusic()
    {
        musicSource.pitch = 1.25f;
    }

    public void ResetMusicSpeed()
    {
        musicSource.pitch = 1f;
    }

    void Update()
    {
        if (!musicSource.isPlaying && musicClips.Length > 0)
        {
            PlayRandomMusic();
        }
    }
}
