using TMPro;
using UnityEngine;

public class GameOverManager : MonoBehaviour
{
    public static GameOverManager instance;
    [Header(" Elements ")]
    [SerializeField] private GameObject deadLine;
    [SerializeField] private Transform animalsParent;
    [SerializeField] private int deadLineActivateDistance = 5;
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header(" Timer ")]
    [SerializeField] private float durationThreshold = 3;
    private float timer;
    private bool timerActive = false;
    private bool isGameOver = false;
    private bool canLoose = true;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        GameManager.OnGameStateChanged += CheckState;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (isGameOver)
            return;
        ManageGameOver();
    }

    private void ManageGameOver()
    {
        if (timerActive)
        {
            timer += Time.deltaTime;
            if (timer >= durationThreshold)
            {
                GameOver();
            }
        }
        CheckAnimals();
    }

    private void CheckState(GameState state)
    {
        canLoose = state == GameState.Game;
    }

    private void GameOver()
    {
        GameManager.instance.SetGameState(GameState.GameOver);
        isGameOver = true;
        ResetTexts();
    }

    private void CheckAnimals()
    {
        if (animalsParent.childCount == 0 || !canLoose)
            return;

        foreach (Transform child in animalsParent)
        {
            Animal animal = child.GetComponent<Animal>();
            if (animal.HasCollided() && animal != GameplayController.instance.currentAnimal)
            {
                CheckCloseToDeadLine(child);
                if (child.position.y > deadLine.transform.position.y)
                {
                    StartTimer();
                    return;
                }
            }

        }

        StopTimer();
    }

    private void CheckCloseToDeadLine(Transform animal)
    {
        if (animal.position.y + deadLineActivateDistance >= deadLine.transform.position.y)
        {
            ActivateDeadLine(true);
        }
        else
        {
            ActivateDeadLine(false);
        }
    }

    private void StartTimer()
    {
        timerActive = true;
    }
    private void StopTimer()
    {
        timerActive = false;
        timer = 0;
    }

    public void ResetTexts()
    {
        int score = ScoreManager.instance.GetScore();
        int coins = ScoreManager.instance.CalculateCoinsFromScore();

        scoreText.text = score.ToString();
        coinsText.text = "+" + coins.ToString();
    }

    public void SetCanLoose(bool canLoose)
    {
        this.canLoose = canLoose;
    }

    private void ActivateDeadLine(bool toActivate)
    {
        if (deadLine == null) return;

        deadLine.SetActive(toActivate);
    }


    public void Reset()
    {
        ActivateDeadLine(false);
        StopTimer();
        isGameOver = false;
    }
}
