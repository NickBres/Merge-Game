using Unity.VisualScripting;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header(" Settings ")]
    private GameState gameState;

    [Header(" Actions ")]
    public static Action<GameState> OnGameStateChanged;
    public static Action<GameMode> OnGameModeChanged;

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
    }
} 
  
}
