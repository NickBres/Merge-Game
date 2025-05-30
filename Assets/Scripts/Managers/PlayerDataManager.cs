using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class PlayerDataManager : MonoBehaviour
{
    public static PlayerDataManager instance;

    private int coins = 0;
    private HashSet<string> ownedSkins = new();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        LoadData();
    }

    private void Start()
    {
        OwnSkin("None"); // Ensure default skin is owned
        OwnSkin("Glasses"); // Example of owning a default skin
        OwnSkin("Crown"); // Example of owning another default skin
    }

    // Money Management
    public void AddCoins(int amount)
    {
        coins += amount;
        SaveData();
    }

    public bool SpendCoins(int amount)
    {
        if (coins >= amount)
        {
            coins -= amount;
            SaveData();
            return true;
        }
        return false;
    }

    public int GetBalance()
    {
        return coins;
    }

    // Skin Ownership
    public void OwnSkin(string skinID)
    {
        ownedSkins.Add(skinID);
        SaveData();
    }

    public bool HasSkin(string skinID)
    {
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
    public void SaveData()
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

    public void LoadData()
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
