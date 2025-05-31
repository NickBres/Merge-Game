using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager instance;

    [Header(" Elements ")]
    [SerializeField] private Toggle soundToggle;

    public static System.Action<bool> onSFXToggle;
    private const string sfxKey = "sfxMuted";

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
        
    }

    private void Start()
    {
        soundToggle.onValueChanged.AddListener(OnSoundToggleChanged);
        LoadData();
    }

    private void OnSoundToggleChanged(bool isOn)
    {
        onSFXToggle?.Invoke(isOn);
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey(sfxKey))
        {
            bool isMuted = PlayerPrefs.GetInt(sfxKey) == 1;
            SetSfxToggle(isMuted);
        }
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(sfxKey, soundToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        soundToggle.onValueChanged.RemoveListener(OnSoundToggleChanged);
        SaveData();
    }

    private void SetSfxToggle(bool isSoundOn)
    {
        soundToggle.onValueChanged.RemoveListener(OnSoundToggleChanged); // Temporarily detach
        soundToggle.isOn = isSoundOn;
        soundToggle.onValueChanged.AddListener(OnSoundToggleChanged); // Reattach
        onSFXToggle?.Invoke(isSoundOn); // Ensure audio state is updated on load
    }

}
