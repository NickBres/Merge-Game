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

    public static Action OnCoinsChanged;

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
       AddCoins(10000); // For testing, give initial coins
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
            ownedSkins = ownedSkins.ToList()
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
        }
    }

    [System.Serializable]
    private class PlayerData
    {
        public int coins;
        public List<string> ownedSkins;
    }
}
