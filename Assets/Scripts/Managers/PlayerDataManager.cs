using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using NUnit.Framework;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;

    private int coins = 0;
    private HashSet<string> ownedSkins = new();
    private int magicSweepCount = 0;
    private int upgrdesCount = 0;
    private int bombsCount = 0;

    public static Action OnCoinsChanged;
    public static Action OnAbilitiesChanged;



    private void Awake()
    {
        //PlayerPrefs.DeleteAll(); // For testing, clear PlayerPrefs
        OwnSkin("None"); // Ensure default skin is owned
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        LoadData();
    }

    private void Start()
    {
        
    }

    // Money Management
    public void AddCoins(int amount)
    {
        LoadData();
        coins += amount;
        SaveData();
        OnCoinsChanged?.Invoke();
    }

    public bool BuyMagicSweep(int price)
    {
        LoadData();
        if (!SpendCoins(price))
            return false;
        magicSweepCount++;
        SaveData();
        OnAbilitiesChanged?.Invoke();
        return true;
    }

    public bool UseMagicSweep()
    {
        LoadData();
        if (magicSweepCount > 0)
        {
            magicSweepCount--;
            SaveData();
            OnAbilitiesChanged?.Invoke();
            return true;
        }
        return false;
    }

    public int GetMagicSweepCount()
    {
        LoadData();
        return magicSweepCount;
    }

    public bool BuyUpgrade(int price)
    {
        LoadData();
        if (!SpendCoins(price))
            return false;
        upgrdesCount++;
        SaveData();
        OnAbilitiesChanged?.Invoke();
        return true;
    }

    public bool UseUpgrade()
    {
        LoadData();
        if (upgrdesCount > 0)
        {
            upgrdesCount--;
            SaveData();
            OnAbilitiesChanged?.Invoke();
            return true;
        }
        return false;
    }

    public int GetUpgradesCount()
    {
        LoadData();
        return upgrdesCount;
    }
    public bool BuyBomb(int price)
    {
        LoadData();
        if (!SpendCoins(price))
            return false;
        bombsCount++;
        SaveData();
        OnAbilitiesChanged?.Invoke();
        return true;
    }

    public bool UseBomb()
    {
        LoadData();
        if (bombsCount > 0)
        {
            bombsCount--;
            SaveData();
            OnAbilitiesChanged?.Invoke();
            return true;
        }
        return false;
    }

    public int GetBombsCount()
    {
        LoadData();
        return bombsCount;
    }

    public bool SpendCoins(int amount)
    {
        LoadData();
        if (coins >= amount)
        {
            coins -= amount;
            SaveData();
            OnCoinsChanged?.Invoke();
            return true;
        }
        return false;
    }

    public int GetBalance()
    {
        LoadData();
        return coins;
    }

    // Skin Ownership
    public void OwnSkin(string skinID)
    {
        if(HasSkin(skinID))
            return;
        
        LoadData();
        ownedSkins.Add(skinID);
        SaveData();
    }

    public bool HasSkin(string skinID)
    {
        LoadData();
        return ownedSkins.Contains(skinID);
    }

    public void AssignSkinToAnimal(AnimalType type, string skinID)
    {
        if (HasSkin(skinID))
        {
            SkinManager.instance.ChangeSkin(type, skinID);
        }
    }

    // Save and Load
    private void SaveData()
    {
        var data = new PlayerData
        {
            coins = coins,
            ownedSkins = ownedSkins.ToList(),
            magicSweepCount = magicSweepCount,
            upgrdesCount = upgrdesCount,
            bombsCount = bombsCount
        };
        string json = JsonUtility.ToJson(data);
        PlayerPrefs.SetString("PlayerData", json);
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey("PlayerData"))
        {
            string json = PlayerPrefs.GetString("PlayerData");
            var data = JsonUtility.FromJson<PlayerData>(json);
            coins = data.coins;
            ownedSkins = new HashSet<string>(data.ownedSkins);
            magicSweepCount = data.magicSweepCount;
            upgrdesCount = data.upgrdesCount;
            bombsCount = data.bombsCount;
        }
    }

    [System.Serializable]
    private class PlayerData
    {
        public int coins;
        public List<string> ownedSkins;
        public int magicSweepCount;
        public int upgrdesCount;
        public int bombsCount;
    }
}
