
using Unity.VisualScripting;
using UnityEngine;
using System;



public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header(" Settings ")]
    private GameState gameState;
    private ShapeState animalsShape;

    [Header(" Actions ")]
    public static Action<GameState> OnGameStateChanged;
    public static Action<GameMode> OnGameModeChanged;

    private const string SHAPE_KEY = "shape_index";

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SetGameState(GameState.Menu);
    }



    public void SetGameState(GameState newGameState)
    {
        gameState = newGameState;
        OnGameStateChanged?.Invoke(gameState);
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public bool IsGameState()
    {
        return gameState == GameState.Game;
    }

    void OnApplicationPause(bool pause)
    {
        if (pause && gameState == GameState.Game)
        {
            // App is in background
            SetGameState(GameState.Pause);
            SaveGame();
        }
    }

    public void SaveGame()
    {
        GameSessionData gameSessionData = GameplayController.instance.GetSessionData();
        GameStateSaveController.instance.SaveGame(gameSessionData);
    }

    public void LoadGame()
    {
        GameSessionData loadedData = GameStateSaveController.instance.LoadGame();
        if (loadedData == null) return;
        GameplayController.instance.LoadSessionData(loadedData);
        ScoreManager.instance.LoadScoreData(loadedData.score);

        GameStateSaveController.instance.DeleteSave();
    }
    

    public ShapeState GetAnimalShape()
    {
        animalsShape = (ShapeState)PlayerPrefs.GetInt(SHAPE_KEY, 0);
        return animalsShape;
    }

}
