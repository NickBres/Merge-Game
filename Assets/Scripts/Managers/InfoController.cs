using UnityEngine;

public class InfoController : MonoBehaviour
{
    private float lastClickTime = 0f;

    [SerializeField] private GameObject basicsPanel;
    [SerializeField] private GameObject controlsPanel;
    [SerializeField] private GameObject eventsPanel;
    [SerializeField] private GameObject magicSweepPanel;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameObject bombPanel;

    private int basicsClickCount = 0;

    void Awake()
    {
        DisableAll();
        basicsPanel.SetActive(true);
    }

    public void OnClickBasics()
    {
        DisableAll();
        basicsPanel.SetActive(true);
        Secret();
    }

    private void Secret()
    {
        if (Time.time - lastClickTime > 2f)
        {
            basicsClickCount = 0;
        }

        basicsClickCount++;
        lastClickTime = Time.time;

        if (basicsClickCount >= 4)
        {
            basicsClickCount = 0;
            PlayerDataManager.instance.AddCoins(10000);
            AudioManager.instance.PlayChoirSound();
            VibrationManager.instance.Vibrate(VibrationType.Heavy);
        }
    }

    public void OnClickControls()
    {
        DisableAll();
        controlsPanel.SetActive(true);
    }

    public void OnClickEvents()
    {
        DisableAll();
        eventsPanel.SetActive(true);
    }

    public void OnClickMagic()
    {
        DisableAll();
        magicSweepPanel.SetActive(true);
    }
    public void OnClickUpgrade()
    {
        DisableAll();
        upgradePanel.SetActive(true);
    }

    public void OnClickBomb()
    {
        DisableAll();
        bombPanel.SetActive(true);
    }

    private void DisableAll()
    {
        basicsPanel.SetActive(false);
        controlsPanel.SetActive(false);
        eventsPanel.SetActive(false);
        magicSweepPanel.SetActive(false);
        upgradePanel.SetActive(false);
        bombPanel.SetActive(false);

        UIManager.ClickAndVibrate();
    }
}
