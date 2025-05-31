using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class ShopManager : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Button buyChestButton;

    [Header(" Positions ")]
    [SerializeField] private int chestPrice = 1000;
    [SerializeField] private List<SkinDataSO> skinsInChest;


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
    }

    public void BuyChest()
    {
        if (PlayerDataManager.instance.GetBalance() >= chestPrice)
        {
            PlayerDataManager.instance.SpendCoins(chestPrice);
            OpenChest();
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
                            Rarity.Common => (int)(chestPrice / 2),
                            Rarity.Rare => chestPrice,
                            Rarity.Epic => (int)(chestPrice * 1.5f),
                            Rarity.Legendary => chestPrice * 5,
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
