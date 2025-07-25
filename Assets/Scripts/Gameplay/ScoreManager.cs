using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    private float frenzyTimer = 0f;
    private bool isFrenzyActive = false;

    public static ScoreManager instance;
    [Header(" Elements ")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private GameObject frenzyBack;

    [Header(" Settings ")]
    [SerializeField] private int frenzyTime = 20;
    private int score = 0;
    private int bestScore = 0;
    private int comboCount = 1;
    private int multiplier = 1;

    [Header(" Data ")]
    private const string bestScoreKey = "BestScore";
    private bool isScoreChanged = false;

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
        score += (int)((int)animalType * comboCount * multiplier);
        UpdateTextScore();
        isScoreChanged = true;
    }

    private void UpdateTextScore()
    {
        scoreText.text = (multiplier > 1 ? "x" + multiplier + " " : "") + score.ToString();
    }

    private void UpdateTextBestScore()
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
            UpdateTextBestScore();
        }
    }

    public int CalculateCoinsFromScore()
    {
        return (int)score / 20;
    }

    private void SetBestScore()
    {
        LoadData();
        if (score > bestScore)
        {
            bestScore = score;
            SaveData();
            AudioManager.instance.PlayNewHighScoreSound();
        }
        else
        {
            AudioManager.instance.PlayGameOverSound();
        }
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
        if (!isScoreChanged)
            comboCount = 1;
        isScoreChanged = false;
    }


    public void IncrementCombo()
    {
        comboCount++;
        ComboEffect();
    }

    private void ComboEffect()
    {
        if (isCawabungaCombo())
        {
            comboCount = 1;
            ComboText.instance.ShowCawabunga();
            EnableFrenzy();
        }
        else if (isEpicCombo())
        {

            ComboText.instance.ShowEpic();
        }
        else if (isWOWCombo())
        {
            ComboText.instance.ShowWOW();
            GameplayController.instance.SetNextCapybara();
        }
    }



    public bool isEpicCombo()
    {
        return comboCount == 8;

    }

    public bool isCawabungaCombo()
    {
        return comboCount == 10;

    }

    public bool isWOWCombo()
    {
        return comboCount == 6;

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
        DisableFrenzy();
    }

    public void EnableFrenzy()
    {
        frenzyTimer = frenzyTime;
        multiplier *= 2;
        multiplier = Mathf.Min(multiplier, 8);
        UpdateTextScore();

        if (!isFrenzyActive)
        {
            StartCoroutine(FrenzyRoutine());
        }
    }

    private System.Collections.IEnumerator FrenzyRoutine()
    {
        isFrenzyActive = true;
        frenzyBack.SetActive(true);
        AudioManager.instance.SpeedUpMusic();

        while (frenzyTimer > 0 && GameManager.instance.GetGameState() == GameState.Game)
        {
            frenzyTimer -= Time.deltaTime;
            yield return null;
        }

        DisableFrenzy();
    }

    private void DisableFrenzy()
    {
        multiplier = 1;
        frenzyBack.SetActive(false);
        AudioManager.instance.ResetMusicSpeed();
        frenzyTimer = 0f;
        isFrenzyActive = false;
        UpdateTextScore();
    }

    public ScoreData GetScoreData()
    {
        ScoreData scoreData = new ScoreData
        {
            score = score,
            comboCount = comboCount
        };
        return scoreData;
    }

    public void LoadScoreData(ScoreData scoreData)
    {
        score = scoreData.score;
        comboCount = scoreData.comboCount;
        
        UpdateTextScore();
    }
}
