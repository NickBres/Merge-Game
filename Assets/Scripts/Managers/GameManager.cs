using Unity.VisualScripting;
using UnityEngine;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    [Header(" Settings ")]
    private GameState gameState;

    [Header(" Actions ")]
    public static Action<GameState> onGameStateChanged;

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
        onGameStateChanged?.Invoke(gameState);
    }

    public GameState GetGameState()
    {
        return gameState;
    }

    public bool IsGameState()
    {
        return gameState == GameState.Game;
    }
    
    

    
  
}
