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
    [SerializeField] private GameObject gameBackPanel;
    [SerializeField] private GameObject wallsPanel;
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private GameObject skinsPanel;
    [SerializeField] private GameObject animalSelectorPanel;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject rewardPanel;
    [SerializeField] private GameObject optionsPanel;
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private GameObject saveAlertPanel;
    [SerializeField] private GameObject skinAlertPanel;


    void Awake()
    {
#if !UNITY_WEBGL
        Application.targetFrameRate = 120;
        QualitySettings.vSyncCount = 0;
#endif

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
        gameBackPanel.SetActive(false);
        skinsPanel.SetActive(false);
        animalSelectorPanel.SetActive(false);
        wallsPanel.SetActive(false);
        optionsPanel.SetActive(false);
        shopPanel.SetActive(false);
        pausePanel.SetActive(false);
        infoPanel.SetActive(false);
    }

    private void SetOptions()
    {
        optionsPanel.SetActive(true);
        menuPanel.SetActive(false);
    }

    private void SetAlert()
    {
        saveAlertPanel.SetActive(true);
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
        gameBackPanel.SetActive(true);
        wallsPanel.SetActive(true);
        pausePanel.SetActive(false);
        saveAlertPanel.SetActive(false);
    }

    private void SetGameOver()
    {
        gameOverPanel.SetActive(true);
        gamePanel.SetActive(false);
        gameBackPanel.SetActive(false);

        wallsPanel.SetActive(false);
    }

    private void SetPause()
    {
        pausePanel.SetActive(true);
        gamePanel.SetActive(false);
        gameBackPanel.SetActive(false);
    }

    private void SetSkins()
    {
        menuPanel.SetActive(false);
        skinsPanel.SetActive(true);
        animalSelectorPanel.SetActive(true);
    }

    private void SetInfo()
    {
        menuPanel.SetActive(false);
        infoPanel.SetActive(true);
    }

    public void ExitButtonCallback()
    {
        Application.Quit();
    }

    public void InfoButtonCallback()
    {
        GameManager.instance.SetGameState(GameState.Menu);

        SetInfo();
        ClickAndVibrate();
    }


    public void PlayButtonCallback()
    {
        bool isSave = GameStateSaveController.instance.SaveExists();
        if (isSave)
        {
            SetAlert();
            return;
        }

        GameManager.instance.SetGameState(GameState.Game);
        GameplayController.instance.ResetGameplay();
        SetGame();
        ClickAndVibrate();
    }

    public void ContinueButtonCallback()
    {
        GameManager.instance.SetGameState(GameState.Game);
        GameplayController.instance.ResetGameplay();
        GameManager.instance.LoadGame();
        SetGame();
        ClickAndVibrate();
    }

    public void ResetGameButtonCallback()
    {
        GameStateSaveController.instance.DeleteSave();
        GameManager.instance.SetGameState(GameState.Game);
        GameplayController.instance.ResetGameplay();
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
        SetMenu();
        GameplayController.instance.ResetGameplay();
        GameOverManager.instance.Reset();
        Abilities.instance.Reset();
        ScoreManager.instance.Reset();
        BackgroundManager.instance.Reset();
        GameManager.instance.SetGameState(GameState.Menu);
        ComboText.instance.Reset();
        ClickAndVibrate();
    }



    public void PauseButtonCallback()
    {
        SetPause();
        GameManager.instance.SetGameState(GameState.Pause);
        GameManager.instance.SaveGame();
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
        ClickAndVibrate();
        GameManager.instance.SetGameState(GameState.Menu);
        if (PlayerDataManager.instance.IsWhaleUnlocked())
        {
            SetSkins();
        }
        else
        {
            ShowSkinAlert();
        }
        
    }

    private void ShowSkinAlert()
    {
        skinAlertPanel.SetActive(true);
    }

    public void OnSkinAlertClilck()
    {
        skinAlertPanel.SetActive(false);
        ClickAndVibrate();
    }

    public void ShopButtonCallback()
    {
        GameManager.instance.SetGameState(GameState.Menu);
        SetShop();
        ClickAndVibrate();
    }

    public void OptionsButtonCallback()
    {
        GameManager.instance.SetGameState(GameState.Menu);
        SetOptions();
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

    public void QuitButtonCallback()
    {
        Application.Quit();
    }
}
