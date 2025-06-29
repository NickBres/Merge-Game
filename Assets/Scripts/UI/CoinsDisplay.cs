using UnityEngine;
using TMPro;

public class CoinsDisplay : MonoBehaviour
{
    [Header(" Elements ")]
    private TextMeshProUGUI coinAmount;

    void Awake()
    {
        PlayerDataManager.OnCoinsChanged += UpdateCoinDisplay;

        coinAmount = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Start()
    {
        UpdateCoinDisplay();
    }

    private void OnDestroy()
    {
        PlayerDataManager.OnCoinsChanged -= UpdateCoinDisplay;
    }

    private void UpdateCoinDisplay()
    {
        coinAmount.text = PlayerDataManager.instance.GetBalance().ToString();
    }

}
