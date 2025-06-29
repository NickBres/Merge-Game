using UnityEngine;
using System;
using CandyCoded.HapticFeedback;

public class VibrationManager : MonoBehaviour
{
    public static VibrationManager instance;

    private bool vibrationEnabled = true;

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

        OptionsManager.OnVibrationChanged += ToggleVibration;
    }

    private void Start()
    {
        OptionsManager.instance.LoadSettings();
    }

    private void OnDestroy()
    {
        OptionsManager.OnVibrationChanged -= ToggleVibration;
    }

    private void ToggleVibration(bool isOn)
    {
        vibrationEnabled = isOn;
    }

    public void Vibrate(VibrationType type)
    {
        if (!vibrationEnabled) return;

#if (UNITY_IOS || UNITY_ANDROID) && !UNITY_EDITOR
        switch (type)
        {
            case VibrationType.Light:
                HapticFeedback.LightFeedback();
                break;
            case VibrationType.Medium:
                HapticFeedback.MediumFeedback();
                break;
            case VibrationType.Heavy:
                HapticFeedback.HeavyFeedback();
                break;
        }
#else
        Debug.Log($"Vibration triggered: {type} (editor or unsupported platform)");
#endif
    }
}