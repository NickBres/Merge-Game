using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager instance;

    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle musicToggle;

    public static System.Action<bool> OnSFXChanged;
    public static System.Action<bool> OnMusicChanged;

    private const string SFX_KEY = "sfx_enabled";
    private const string MUSIC_KEY = "music_enabled";

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadSettings();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        sfxToggle.onValueChanged.AddListener(HandleSFXToggle);
        musicToggle.onValueChanged.AddListener(HandleMusicToggle);
    }

    private void OnDestroy()
    {
        sfxToggle.onValueChanged.RemoveListener(HandleSFXToggle);
        musicToggle.onValueChanged.RemoveListener(HandleMusicToggle);
    }

    private void HandleSFXToggle(bool isOn)
    {
        PlayerPrefs.SetInt(SFX_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        OnSFXChanged?.Invoke(isOn);
    }

    private void HandleMusicToggle(bool isOn)
    {
        PlayerPrefs.SetInt(MUSIC_KEY, isOn ? 0 : 1);
        PlayerPrefs.Save();
        OnMusicChanged?.Invoke(!isOn);
    }

    public void LoadSettings()
    {
        bool sfxOn = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;
        bool musicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;

        sfxToggle.isOn = sfxOn;
        musicToggle.isOn = !musicOn;

        OnSFXChanged?.Invoke(sfxOn);
        OnMusicChanged?.Invoke(musicOn);
    }
}
