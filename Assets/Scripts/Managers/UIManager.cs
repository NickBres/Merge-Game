using System;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    [Header(" Elements ")]
    [SerializeField] private GameObject gameOverPanel;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private GameObject gamePanel;
    [SerializeField] private GameObject wallsPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject skinsPanel;
    [SerializeField] private GameObject animalSelectorPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private GameObject optionsPanel;


    void Awake()
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
        GameManager.OnGameStateChanged += OnGameStateChangedCallback;
        OnGameStateChangedCallback(GameState.Menu);
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
        skinsPanel.SetActive(false);
        animalSelectorPanel.SetActive(false);
        wallsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        shopPanel.SetActive(false);
        pausePanel.SetActive(false);
    }

    public void SetOptions()
    {
        optionsPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    private void SetShop()
    {
        menuPanel.SetActive(false);
        shopPanel.SetActive(true);
        rewardPanel.SetActive(false);

    }

    public void SetReward()
    {
        rewardPanel.SetActive(true);
        shopPanel.SetActive(false);
    }

    private void SetGame()
    {
        menuPanel.SetActive(false);
        gamePanel.SetActive(true);
        wallsPanel.SetActive(true);
        pausePanel.SetActive(false);
    }

    private void SetGameOver()
    {
        gameOverPanel.SetActive(true);
        gamePanel.SetActive(false);

        wallsPanel.SetActive(false);
    }

    private void SetPause()
    {
        pausePanel.SetActive(true);

    }

    private void SetSkins()
    {
        menuPanel.SetActive(false);
        skinsPanel.SetActive(true);
        animalSelectorPanel.SetActive(true);
    }

    public void PlayRushButtonCallback()
    {
        GameManager.instance.SetGameMode(GameMode.Rush);
        GameManager.instance.SetGameState(GameState.Game);

        SetGame();
        ClickAndVibrate();
    }

    public void PlayZenButtonCallback()
    {
        GameManager.instance.SetGameMode(GameMode.Zen);
        GameManager.instance.SetGameState(GameState.Game);

        SetGame();
        ClickAndVibrate();
    }

    public void MainMenuButtonCallback()
    {
        GameManager.instance.SetGameState(GameState.Menu);
        SetMenu();
        ClickAndVibrate();
    }

    public void MainMenuWithResetCallback()
    {
        GameplayController.instance.ResetGameplay();
        ScoreManager.instance.ResetScore();
        GameManager.instance.SetGameState(GameState.Menu);
        Skills.instance.Reset();
        SetMenu();
        ClickAndVibrate();
    }



    public void PauseButtonCallback()
    {
        SetPause();
        GameManager.instance.SetGameState(GameState.Pause);
        ClickAndVibrate();
    }

    public void ResumeButtonCallback()
    {
        SetGame();
        GameManager.instance.SetGameState(GameState.Game);
        ClickAndVibrate();
    }

    public void SkinsButtonCallback()
    {
        GameManager.instance.SetGameState(GameState.Menu);
        SetSkins();
        ClickAndVibrate();
    }

    public void ShopButtonCallback()
    {
        GameManager.instance.SetGameState(GameState.Menu);
        SetShop();
        ClickAndVibrate();
    }

    public static void ClickAndVibrate()
    {
        AudioManager.instance.PlayClickSound();
        VibrationManager.instance.Vibrate(VibrationType.Light);
    }



    
    void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChangedCallback;
    }
}
