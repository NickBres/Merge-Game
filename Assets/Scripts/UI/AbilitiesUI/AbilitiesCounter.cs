using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilitiesCounter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI magicSweepCountText;
    [SerializeField] private TextMeshProUGUI upgradesCountText;
    [SerializeField] private TextMeshProUGUI bombsCountText;

    void Awake()
    {
        if (magicSweepCountText == null || upgradesCountText == null || bombsCountText == null)
        {
            Debug.LogError("AbilitiesCounter: One or more UI Text components are not assigned.");
            return;
        }

        // Subscribe to PlayerDataManager events
        PlayerDataManager.OnAbilitiesChanged += UpdateCounters;

        // Initial update of the counters
        UpdateCounters();
    }
    
    private void UpdateCounters()
    {
        magicSweepCountText.text = PlayerDataManager.instance.GetMagicSweepCount().ToString();
        upgradesCountText.text = PlayerDataManager.instance.GetUpgradesCount().ToString();
        bombsCountText.text = PlayerDataManager.instance.GetBombsCount().ToString();
    }
}
