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
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject skinsPanel;
    [SerializeField] private GameObject animalSelectorPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject rewardPanel;


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

    private void SetShop()
    {
        menuPanel.SetActive(false);
        shopPanel.SetActive(true);
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
        pausePanel.SetActive(false);
    }

    private void SetGameOver()
    {
        gameOverPanel.SetActive(true);
        gamePanel.SetActive(false);
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

    public void ShopButtonCallback()
    {
        SetShop();
    }

    
    void OnDestroy()
    {
        GameManager.OnGameStateChanged -= OnGameStateChangedCallback;
    }
}
