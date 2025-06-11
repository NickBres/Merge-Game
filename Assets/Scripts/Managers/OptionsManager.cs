using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager instance;

    [Header(" Elements ")]
    [SerializeField] private Toggle sfxOnToggle;
    [SerializeField] private Toggle musicOffToggle;

    public static System.Action<bool> onSFXToggle;
    public static System.Action<bool> onMusicToggle;
    private const string sfxKey = "sfxOn";
    private const string musicKey = "musicMuted";

    private void Awake()
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

        LoadData();
    }

    private void Start()
    {
        sfxOnToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        musicOffToggle.onValueChanged.AddListener(OnMusicToggleChanged);
    }

    private void OnSoundToggleChanged(bool isOn)
    {
        onSFXToggle?.Invoke(isOn);
        SaveData();
    }

    private void OnMusicToggleChanged(bool isOn)
    {
        onMusicToggle?.Invoke(!isOn); // Toggle ON = muted → pass false to indicate music off
        SaveData();
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey(sfxKey))
        {
            bool isOn = PlayerPrefs.GetInt(sfxKey) == 1;
            SetSfxToggle(isOn);
        }
        if (PlayerPrefs.HasKey(musicKey))
        {
            bool isMusicMuted = PlayerPrefs.GetInt(musicKey) == 1;
            SetMusicToggle(isMusicMuted); // Pass true if music is muted
        }
        else
        {
            SetSfxToggle(true);
            SetMusicToggle(false);
            SaveData(); // Save defaults if no data exists
        }

        Debug.Log($"OptionsManager loaded: SFX On = {sfxOnToggle.isOn}, Music Muted = {musicOffToggle.isOn}");
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(sfxKey, sfxOnToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt(musicKey, musicOffToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();

        Debug.Log($"OptionsManager saved: SFX On = {sfxOnToggle.isOn}, Music Muted = {musicOffToggle.isOn}");
    }

    void OnDestroy()
    {
        sfxOnToggle.onValueChanged.RemoveListener(OnSoundToggleChanged);
        musicOffToggle.onValueChanged.RemoveListener(OnMusicToggleChanged);
    }

    private void SetSfxToggle(bool isSoundOn)
    {
        sfxOnToggle.onValueChanged.RemoveListener(OnSoundToggleChanged); // Temporarily detach
        sfxOnToggle.isOn = isSoundOn;
        sfxOnToggle.onValueChanged.AddListener(OnSoundToggleChanged); // Reattach
        onSFXToggle?.Invoke(isSoundOn); // Ensure audio state is updated on load
    }

    private void SetMusicToggle(bool isMusicMuted)
    {
        musicOffToggle.onValueChanged.RemoveListener(OnMusicToggleChanged); // Temporarily detach
        musicOffToggle.isOn = isMusicMuted; // Toggle visually shows "muted" when ON
        musicOffToggle.onValueChanged.AddListener(OnMusicToggleChanged); // Reattach
        onMusicToggle?.Invoke(!isMusicMuted); // Ensure audio state is updated on load
    }

}
