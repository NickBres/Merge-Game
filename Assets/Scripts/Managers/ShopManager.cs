using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System;

using Random = UnityEngine.Random;

public class ShopManager : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Button buyChestButton;
    [SerializeField] private Button buyMagicSweepButton;
    [SerializeField] private Button buyUpgradeAnimalsButton;
    [SerializeField] private Button buyBombButton;

    [Header(" Positions ")]
    [SerializeField] private int chestPrice = 1000;
    [SerializeField] private List<SkinDataSO> skinsInChest;
    [SerializeField] private int magicSweepPrice;
    [SerializeField] private int upgradeAnimalsPrice;
    [SerializeField] private int bombPrice;

    public static Action OnGotSkin;


    void Awake()
    {
        SetPriceOnButtons();
    }

    private void SetPriceOnButtons()
    {
        if (buyChestButton != null)
        {
            buyChestButton.GetComponentInChildren<TextMeshProUGUI>().text = chestPrice.ToString();
        }
        if (buyMagicSweepButton != null)
        {
            buyMagicSweepButton.GetComponentInChildren<TextMeshProUGUI>().text = magicSweepPrice.ToString();
        }
        if (buyUpgradeAnimalsButton != null)
        {
            buyUpgradeAnimalsButton.GetComponentInChildren<TextMeshProUGUI>().text = upgradeAnimalsPrice.ToString();
        }
        if (buyBombButton != null)
        {
            buyBombButton.GetComponentInChildren<TextMeshProUGUI>().text = bombPrice.ToString();
        }
    }

    public void BuyMagicSweep()
    {
        if (!PlayerDataManager.instance.BuyMagicSweep(magicSweepPrice))
        {
            AudioManager.instance.PlayErrorSound();
            VibrationManager.instance.Vibrate(VibrationType.Light);
        }
    }

    public void BuyUpgradeAnimals()
    {

        if (!PlayerDataManager.instance.BuyUpgrade(upgradeAnimalsPrice))
        {
            AudioManager.instance.PlayErrorSound();
            VibrationManager.instance.Vibrate(VibrationType.Light);
        }
    }

    public void BuyBomb()
    {
        if (!PlayerDataManager.instance.BuyBomb(bombPrice))
        {
            AudioManager.instance.PlayErrorSound();
            VibrationManager.instance.Vibrate(VibrationType.Light);
        }
    }

    public void BuyChest()
    {
        if (PlayerDataManager.instance.GetBalance() >= chestPrice)
        {
            PlayerDataManager.instance.SpendCoins(chestPrice);
            VibrationManager.instance.Vibrate(VibrationType.Light);
            OpenChest();
        }
        else
        {
            AudioManager.instance.PlayErrorSound();
            VibrationManager.instance.Vibrate(VibrationType.Heavy);
        }

    }

    private void OpenChest()
    {
        // Filter out owned skins
        List<SkinDataSO> availableSkins = skinsInChest.FindAll(skin => !PlayerDataManager.instance.HasSkin(skin.name));

        // Determine rarity by roll
        float roll = Random.Range(0f, 1f);
        Rarity selectedRarity;
        if (roll < 0.55f) selectedRarity = Rarity.Common;
        else if (roll < 0.80f) selectedRarity = Rarity.Rare;
        else if (roll < 0.95f) selectedRarity = Rarity.Epic;
        else selectedRarity = Rarity.Legendary;

        List<SkinDataSO> raritySkins = availableSkins.FindAll(skin => skin.rarity == selectedRarity);

        if (raritySkins.Count > 0)
        {
            // Choose one randomly
            SkinDataSO selectedSkin = raritySkins[Random.Range(0, raritySkins.Count)];
            PlayerDataManager.instance.OwnSkin(selectedSkin.name);
            UIManager.instance.SetReward();
            RewardScreenManager.instance.ShowReward(selectedSkin, false, 0, selectedSkin.rarity);
            OnGotSkin?.Invoke();
        }
        else
        {
            // No skins of chosen rarity, compensate
            int coinsToAdd = selectedRarity switch
            {
                Rarity.Common => (int)(chestPrice / 3),
                Rarity.Rare => (int)(chestPrice / 2),
                Rarity.Epic => (int)(chestPrice),
                Rarity.Legendary => chestPrice * 2,
                _ => 0
            };
            PlayerDataManager.instance.AddCoins(coinsToAdd);
            UIManager.instance.SetReward();
            RewardScreenManager.instance.ShowReward(null, true, coinsToAdd, selectedRarity);
        }
    }

}
