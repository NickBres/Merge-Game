using UnityEngine;

using System.Collections.Generic;
using System.Linq;

public class SkinManager : MonoBehaviour
{
    public static SkinManager instance;
    [SerializeField] private List<SkinDataSO> allSkins;
    private Dictionary<AnimalType, string> animalToSkinMap = new();

    private const string SkinDataKey = "AnimalSkinData";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadSkinData();
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        ChangeSkin(AnimalType.Snake, "Crown");
        ChangeSkin(AnimalType.Parrot, "Glasses");
    }

    public void SaveSkinData()
    {
        string json = JsonUtility.ToJson(new Serialization<AnimalType, string>(animalToSkinMap));
        PlayerPrefs.SetString(SkinDataKey, json);
        PlayerPrefs.Save();
    }

    public void LoadSkinData()
    {
        if (PlayerPrefs.HasKey(SkinDataKey))
        {
            string json = PlayerPrefs.GetString(SkinDataKey);
            animalToSkinMap = new Serialization<AnimalType, string>(json).ToDictionary();
        }
        else
        {
            AssignDefaultSkins();
        }
    }

    private void AssignDefaultSkins()
    {
        foreach (AnimalType type in System.Enum.GetValues(typeof(AnimalType)))
        {
            animalToSkinMap[type] = "None";
        }
        SaveSkinData();
    }

    public SkinDataSO GetSkinForAnimal(AnimalType type)
    {
        if (animalToSkinMap.TryGetValue(type, out string skinID))
        {
            return allSkins.FirstOrDefault(skin => skin.skinID == skinID);
        }
        return null;
    }

    public void ChangeSkin(AnimalType type, string skinID)
    {
        var skin = allSkins.FirstOrDefault(s => s.skinID == skinID);
        if (skin != null)
        {
            animalToSkinMap[type] = skinID;
            SaveSkinData();
        }
    }
}

// Tip: Adding a "None" skin (with an empty sprite) to `allSkins` is a clean way to handle no-skin cases without branching logic.

// Helper class for serializing Dictionary<AnimalType, string>
[System.Serializable]
public class Serialization<TKey, TValue>
{
    [System.Serializable]
    public struct KeyValue
    {
        public TKey key;
        public TValue value;
    }

    public List<KeyValue> items = new();

    public Serialization(Dictionary<TKey, TValue> dict)
    {
        items = dict.Select(kv => new KeyValue { key = kv.Key, value = kv.Value }).ToList();
    }

    public Serialization(string json)
    {
        JsonUtility.FromJsonOverwrite(json, this);
    }

    public Dictionary<TKey, TValue> ToDictionary()
    {
        return items.ToDictionary(i => i.key, i => i.value);
    }
}
