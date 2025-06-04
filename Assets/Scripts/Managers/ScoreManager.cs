using TMPro;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager instance;
    [Header(" Elements ")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    [Header(" Settings ")]
    private int score = 0;
    private int bestScore = 0;
    private int comboCount = 1;

    [Header(" Data ")]
    private const string bestScoreKey = "BestScore";

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
        UpdateScore();
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
        UpdateScore();
    }

    private void UpdateScore()
    {
        scoreText.text = score.ToString();
    }

    private void UpdateBestScore()
    {
        bestScoreText.text = bestScore.ToString();
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
            UpdateBestScore();
        }
    }

    public int CalculateCoinsFromScore()
    {
        return (int)score / 10;
    }

    private void SetBestScore()
    {
        if (score > bestScore)
        {
            bestScore = score;
            SaveData();
        }
        //score = 0;
    }

    public int GetScore()
    {
        return score;
    }

    private void LoadData()
    {
        bestScore = PlayerPrefs.GetInt(bestScoreKey, 0);
    }

    private void SaveData()
    {
        PlayerPrefs.SetInt(bestScoreKey, bestScore);
        PlayerPrefs.Save();
    }

    public void ResetCombo()
    {
        comboCount = 1;
    }

    public void IncrementCombo()
    {
        comboCount *= 2;
    }

    public int GetComboCount()
    {
        return comboCount;
    }
}
