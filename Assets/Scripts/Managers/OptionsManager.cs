using UnityEngine;
using UnityEngine.UI;

public class OptionsManager : MonoBehaviour
{
    public static OptionsManager instance;

    [Header(" Elements ")]
    [SerializeField] private Toggle muteToggle;

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
        muteToggle.onValueChanged.AddListener(OnMuteToggleChanged);
        LoadData();
    }

    private void OnMuteToggleChanged(bool isOn)
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
        PlayerPrefs.SetInt(sfxKey, muteToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    void OnDestroy()
    {
        muteToggle.onValueChanged.RemoveListener(OnMuteToggleChanged);
        SaveData();
    }

    private void SetSfxToggle(bool isMuted)
    {
        muteToggle.onValueChanged.RemoveListener(OnMuteToggleChanged); // Temporarily detach
        muteToggle.isOn = isMuted;
        muteToggle.onValueChanged.AddListener(OnMuteToggleChanged); // Reattach
        onSFXToggle?.Invoke(isMuted); // Ensure audio state is updated on load
    }

}
