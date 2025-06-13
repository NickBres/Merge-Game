using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    [Header(" Elements ")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreTextZen;
    [SerializeField] private TextMeshProUGUI bestScoreTextRush;

    [Header(" Settings ")]
    private int score = 0;
    private int bestScoreZen = 0;
    private int bestScoreRush = 0;
    private int comboCount = 1;

    [Header(" Data ")]
    private const string bestScoreZenKey = "BestScoreZen";
    private const string bestScoreRushKey = "BestScoreRush";

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
        MergeManager.onMergeAnimal += UpdateScore;
        GameManager.OnGameStateChanged += GameStateChanged;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        LoadData();
        UpdateTextScore();
        UpdateTextBestScore();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void OnDestroy()
    {
        MergeManager.onMergeAnimal -= UpdateScore;
        GameManager.OnGameStateChanged -= GameStateChanged;
    }

    public void UpdateScore(AnimalType animalType, Vector2 unused)
    {
        score += (int)((int)animalType * comboCount);
        UpdateTextScore();
    }

    private void UpdateTextScore()
    {
        scoreText.text = score.ToString();
    }

    private void UpdateTextBestScore()
    {
        bestScoreTextZen.text = bestScoreZen.ToString();
        bestScoreTextRush.text = bestScoreRush.ToString();
    }

    private void GameStateChanged(GameState gameState)
    {
        if (gameState == GameState.GameOver)
        {
            SetBestScore();
            PlayerDataManager.instance.AddCoins(CalculateCoinsFromScore());
        }
        else if (gameState == GameState.Menu)
        {
            LoadData();
            UpdateTextBestScore();
        }
    }

    public int CalculateCoinsFromScore()
    {
        return (int)score / 10;
    }

    private void SetBestScore()
    {
        LoadData();
        if (GameManager.instance.GetGameMode() == GameMode.Zen && score > bestScoreZen)
        {
            bestScoreZen = score;
            SaveData();
        }
        if (GameManager.instance.GetGameMode() == GameMode.Rush && score > bestScoreRush)
        {
            bestScoreRush = score;
            SaveData();
        }
    }

    public int GetScore()
    {
        return score;
    }

    private void LoadData()
    {
        bestScoreZen = PlayerPrefs.GetInt(bestScoreZenKey, 0);
        bestScoreRush = PlayerPrefs.GetInt(bestScoreRushKey, 0);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(bestScoreZenKey, bestScoreZen);
        PlayerPrefs.SetInt(bestScoreRushKey, bestScoreRush);
        PlayerPrefs.Save();
    }

    public void ResetCombo()
    {
        comboCount = 1;
    }

    public void IncrementCombo()
    {
        if (GameManager.instance.GetGameMode() == GameMode.Zen)
        {
            comboCount += 1;
        }
        else if (GameManager.instance.GetGameMode() == GameMode.Rush)
        {
            comboCount *= 2;
        }

        comboCount = math.min(comboCount, 64);

    }

    public int GetComboCount()
    {
        return comboCount;
    }

    public void Reset()
    {
        LoadData();
        score = 0;
        UpdateTextScore();
        UpdateTextBestScore();
    }
}
