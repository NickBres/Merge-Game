using UnityEngine;
using UnityEngine.UI;

public class UpgradeProgressUI : MonoBehaviour
{
    [SerializeField] private Image fillImage;

    public void SetProgress(float value)
    {
        fillImage.fillAmount = value;
    }

    public void Show(bool show)
    {
        gameObject.SetActive(show);
    }

    
}