using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager instance;

    [SerializeField] private Toggle sfxToggle;
    [SerializeField] private Toggle musicToggle;
    [SerializeField] private Toggle vibrationToggle;
    [SerializeField] private Toggle tiltToggle;

    public static System.Action<bool> OnSFXChanged;
    public static System.Action<bool> OnMusicChanged;
    public static System.Action<bool> OnVibrationChanged;
    public static System.Action<bool> OnTiltChanged;

    private const string SFX_KEY = "sfx_enabled";
    private const string MUSIC_KEY = "music_enabled";
    private const string VIBRATION_KEY = "vibration_enabled";
    private const string TILT_KEY = "tilt_enabled";

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
        vibrationToggle.onValueChanged.AddListener(HandleVibrationToggle);
        tiltToggle.onValueChanged.AddListener(HandleTiltToggle);
    }

    private void OnDestroy()
    {
        sfxToggle.onValueChanged.RemoveListener(HandleSFXToggle);
        musicToggle.onValueChanged.RemoveListener(HandleMusicToggle);
        vibrationToggle.onValueChanged.RemoveListener(HandleVibrationToggle);
        tiltToggle.onValueChanged.RemoveListener(HandleTiltToggle);
    }

    private void HandleSFXToggle(bool isOn)
    {
        UIManager.ClickAndVibrate();
        PlayerPrefs.SetInt(SFX_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        OnSFXChanged?.Invoke(isOn);
    }

    private void HandleMusicToggle(bool isOn)
    {
        UIManager.ClickAndVibrate();
        PlayerPrefs.SetInt(MUSIC_KEY, isOn ? 0 : 1);
        PlayerPrefs.Save();
        OnMusicChanged?.Invoke(!isOn);
    }

    private void HandleVibrationToggle(bool isOn)
    {
        UIManager.ClickAndVibrate();
        PlayerPrefs.SetInt(VIBRATION_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        OnVibrationChanged?.Invoke(isOn);    
    }
    
    private void HandleTiltToggle(bool isOn)
    {
        UIManager.ClickAndVibrate();
        PlayerPrefs.SetInt(TILT_KEY, isOn ? 1 : 0);
        PlayerPrefs.Save();
        OnTiltChanged?.Invoke(isOn);
    }

    public void LoadSettings()
    {
        bool sfxOn = PlayerPrefs.GetInt(SFX_KEY, 1) == 1;
        bool musicOn = PlayerPrefs.GetInt(MUSIC_KEY, 1) == 1;
        bool vibrationOn = PlayerPrefs.GetInt(VIBRATION_KEY, 1) == 1;
        bool tiltOn = PlayerPrefs.GetInt(TILT_KEY, 1) == 1;

        sfxToggle.isOn = sfxOn;
        musicToggle.isOn = !musicOn;
        vibrationToggle.isOn = vibrationOn;
        tiltToggle.isOn = tiltOn;

        OnSFXChanged?.Invoke(sfxOn);
        OnMusicChanged?.Invoke(musicOn);
        OnVibrationChanged?.Invoke(vibrationOn);
        OnTiltChanged?.Invoke(tiltOn);
    }
}
