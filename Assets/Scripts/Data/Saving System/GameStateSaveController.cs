

using UnityEngine;
using System.IO;

public class GameStateSaveController : MonoBehaviour
{

    public static GameStateSaveController instance;
    private string savePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        savePath = Path.Combine(Application.persistentDataPath, "save.json");
    }

    public void SaveGame(GameSessionData data)
    {
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(savePath, json);
        Debug.Log("Game saved to: " + savePath);
    }

    public GameSessionData LoadGame()
    {
        if (!File.Exists(savePath))
        {
            Debug.LogWarning("No save file found at: " + savePath);
            return null;
        }

        string json = File.ReadAllText(savePath);
        GameSessionData data = JsonUtility.FromJson<GameSessionData>(json);
        Debug.Log("Game loaded from: " + savePath);
        return data;
    }

    public void DeleteSave()
    {
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("Save file deleted from: " + savePath);
        }
    }

    public bool SaveExists()
    {
        return File.Exists(savePath);
    }
}