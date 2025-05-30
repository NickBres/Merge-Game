using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{

    [Header(" Elements ")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject skinsPanel;
    [SerializeField] private GameObject animalSelectorPanel;


    void Awake()
    {
        GameManager.onGameStateChanged += OnGameStateChangedCallback;
    }

    private void OnGameStateChangedCallback(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.Menu:
                SetMenu();
                break;
            case GameState.Game:
                SetGame();
                break;
            case GameState.GameOver:
                SetGameOver();
                break;
            case GameState.Pause:
                SetPause();
                break;
        }
    }

    private void SetMenu()
    {
        gameOverPanel.SetActive(false);
        menuPanel.SetActive(true);
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        skinsPanel.SetActive(false);
        animalSelectorPanel.SetActive(false);
    }

    private void SetGame()
    {
        gameOverPanel.SetActive(false);
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        pausePanel.SetActive(false);
        skinsPanel.SetActive(false);
    }

    private void SetGameOver()
    {
        gameOverPanel.SetActive(true);
        menuPanel.SetActive(false);
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        skinsPanel.SetActive(false);
    }

    private void SetPause()
    {
        gameOverPanel.SetActive(false);
        menuPanel.SetActive(false);
        gamePanel.SetActive(false);
        pausePanel.SetActive(true);
        skinsPanel.SetActive(false);
    }

    private void SetSkins()
    {
        gameOverPanel.SetActive(false);
        menuPanel.SetActive(false);
        gamePanel.SetActive(false);
        pausePanel.SetActive(false);
        skinsPanel.SetActive(true);
        animalSelectorPanel.SetActive(true);
    }

    public void PlayButtonCallback()
    {
        SetGame();
        GameManager.instance.SetGameState(GameState.Game);
    }

    public void RestartButtonCallback()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void PauseButtonCallback()
    {
        SetPause();
        GameManager.instance.SetGameState(GameState.Pause);
    }

    public void ResumeButtonCallback()
    {
        SetGame();
        GameManager.instance.SetGameState(GameState.Game);
    }

    public void SkinsButtonCallback()
    {
        SetSkins();
    }

    
    void OnDestroy()
    {
        GameManager.onGameStateChanged -= OnGameStateChangedCallback;
    }
}
