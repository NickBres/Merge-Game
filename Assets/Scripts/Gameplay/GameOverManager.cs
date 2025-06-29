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

    public bool closeToDeath = false;
    public bool isAboveLine = false;

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
        if (timerActive && canLoose)
        {
            timer += Time.deltaTime;
            if (timer >= durationThreshold)
            {
                GameOver();
            }
            DeadLinePulse();
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
                CheckCloseToDeadLine(animal);
                CheckAbovoeDeadLine(animal);
            }

        }

        ActivateDeadLine(closeToDeath);
        if (isAboveLine)
        {
            StartTimer();
        }
        else
        {
            StopTimer();
            ResetDeadLine();
        }
    }

    private void CheckCloseToDeadLine(Animal animal)
    {
        Collider2D collider = animal.GetComponent<Collider2D>();
        if (collider != null)
        {
            float topY = collider.bounds.max.y;
            closeToDeath = topY + deadLineActivateDistance >= deadLine.transform.position.y;
        }
    }

    private void CheckAbovoeDeadLine(Animal animal)
    {
        Collider2D collider = animal.GetComponent<Collider2D>();
        if (collider != null)
        {
            float topY = collider.bounds.max.y;
            isAboveLine = topY > deadLine.transform.position.y;
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
        closeToDeath = false;
        isAboveLine = false;
        ResetDeadLine();

    }

    private void ResetDeadLine()
    {
        if (deadLine == null) return;

        var lineRenderer = deadLine.GetComponent<LineRenderer>();
        if (lineRenderer != null)
        {
            lineRenderer.startColor = new Color(1f, 0f, 0f, 0.3f);
            lineRenderer.endColor = new Color(1f, 0f, 0f, 0.3f);
        }
    }
    
    private float pulseTimer = 0f;
    private float pulseInterval = 0.5f;
    private bool pulseState = false;
    private void DeadLinePulse()
    {
        if (deadLine == null) return;

        pulseTimer += Time.deltaTime;
        if (pulseTimer >= pulseInterval)
        {
            pulseState = !pulseState;
            var lineRenderer = deadLine.GetComponent<LineRenderer>();
            if (lineRenderer != null)
            {
                Color pulseColor = pulseState ? Color.red : Color.yellow;
                lineRenderer.startColor = pulseColor;
                lineRenderer.endColor = pulseColor;
            }
            pulseTimer = 0f;
        }
    }
}
