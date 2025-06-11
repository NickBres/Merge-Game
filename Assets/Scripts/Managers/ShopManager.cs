using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
        // sort skins by rarity, from highest to lowest
        skinsInChest.Sort((a, b) => ((int)b.rarity).CompareTo((int)a.rarity));
        var sortedSkins = skinsInChest;

        bool gotSomething = false;

        while (!gotSomething)
        {
            foreach (SkinDataSO skinData in sortedSkins)
            {
                float chance = 1f / (int)skinData.rarity;
                if (Random.Range(0f, 1f) < chance) // got the skin
                {
                    gotSomething = true;
                    bool isDuplicate = PlayerDataManager.instance.HasSkin(skinData.name);
                    if (isDuplicate) // player already has this skin, give coins instead                    
                    {
                        int coinsToAdd = skinData.rarity switch
                        {
                            Rarity.Common => (int)(chestPrice / 3),
                            Rarity.Rare => (int)(chestPrice / 2),
                            Rarity.Epic => (int)(chestPrice),
                            Rarity.Legendary => chestPrice * 2,
                            _ => 0
                        };
                        UIManager.instance.SetReward(); 
                        PlayerDataManager.instance.AddCoins(coinsToAdd);
                        RewardScreenManager.instance.ShowReward(skinData, isDuplicate, coinsToAdd);
                    }
                    else
                    {
                        UIManager.instance.SetReward();
                        PlayerDataManager.instance.OwnSkin(skinData.name);
                        RewardScreenManager.instance.ShowReward(skinData, isDuplicate, 0);
                    }
                    
                    
                    break;
                }
            }
        }
    }

}
