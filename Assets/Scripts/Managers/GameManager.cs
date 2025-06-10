using Unity.VisualScripting;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header(" Settings ")]
    private GameState gameState;
    private GameMode gameMode;

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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetGameState(GameState.Menu);
    }

    // Update is called once per frame
    void Update()
    {

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

    public void SetGameMode(GameMode newGameMode)
    {
        gameMode = newGameMode;
        OnGameModeChanged?.Invoke(gameMode);
    }

    public GameMode GetGameMode()
    {
        return gameMode;
    }
    
    

    
  
}
