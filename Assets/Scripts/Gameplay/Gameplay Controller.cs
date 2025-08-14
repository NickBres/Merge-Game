using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Collections.Generic;
using System.Linq;

public class GameplayController : MonoBehaviour
{
    public static GameplayController instance;
    [Header(" Elements ")]
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform rightWall;
    public float MinX => leftWall.position.x;
    public float MaxX => rightWall.position.x;
    [SerializeField] private List<AnimalType> defaultSpawnableAnimals;
    private HashSet<AnimalType> currentSpawnableAnimals;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private AimLine aimLine;

    private AnimalsParent animalsParentManager;

    private Animal currentAnimal;
    private Animal nextAnimal;
    private AnimalType nextAnimalType;



    [Header(" Settings ")]
    [SerializeField] private float iceChance = 0.1f;
    [SerializeField] private int eggLimit = 2;
    [SerializeField] private float eggSpawnChance = 0.1f; // Chance to spawn an egg instead of a normal animal
    private bool canControl = true;
    private ShapeState animalsShape;
    private AnimalSpawner animalSpawner;


    [Header(" Actions ")]
    public static Action onNextAnimalSet;

    #region Initialization

    // Initialize singleton, subscribe to events
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

        InputHandler.Initialize(this, uiCanvas);

        GameManager.OnGameStateChanged += CheckState;
        aimLine.DisableLine();
        animalSpawner = AnimalSpawner.instance;

        animalsParentManager = AnimalsParent.instance;
    }

    // Initialize spawnable animals and next animal
    void Start()
    {
        ResetGameplay();
    }

    // Unsubscribe from events
    void OnDestroy()
    {
        InputHandler.Cleanup();
    }

    #endregion


    #region Animal Flow

    // Update loop to handle animal movement and spawning
    private void Update()
    {
        if (GameManager.instance.GetGameState() != GameState.Game) return;
        if (currentAnimal != null)
            aimLine.MoveLine(currentAnimal.transform.position);
        else
            canControl = true;
    }

    public void ResetGameplay()
    {
        animalSpawner.ResetAnimalsParent();
        currentAnimal = null;
        canControl = true;
        currentSpawnableAnimals = new HashSet<AnimalType>(defaultSpawnableAnimals);
        ResetNextAnimal();
        aimLine.DisableLine();
        animalsShape = GameManager.instance.GetAnimalShape();
        animalSpawner.SetShapeState(animalsShape);
    }

    // Select a random next animal from current spawnable animals
    private void ResetNextAnimal()
    {
        if (!animalSpawner.TryToSpawnEgg(eggSpawnChance, eggLimit))
        {
            nextAnimalType = currentSpawnableAnimals.ElementAt(Random.Range(0, currentSpawnableAnimals.Count));
            nextAnimal = animalSpawner.GetAnimalFromType(nextAnimalType);
        }
        else
        {
            nextAnimal = animalSpawner.GetEggPrefab();
            nextAnimalType = AnimalType.Egg;
        }
        onNextAnimalSet?.Invoke();
    }

    public void SetNextAnimal(AnimalType animalType)
    {
        nextAnimalType = animalType;
        nextAnimal = animalSpawner.GetAnimalFromType(animalType);
        onNextAnimalSet?.Invoke();
    }

    public void SetNextCapybara()
    {
        nextAnimalType = AnimalType.Capybara;
        nextAnimal = animalSpawner.GetCapybaraPrefab();
        onNextAnimalSet?.Invoke();
    }

    // Replace current spawnable animals with a new list
    public void SwapSpawnableAnimals(List<AnimalType> toSwap)
    {
        currentSpawnableAnimals.Clear();
        foreach (var animalType in toSwap)
        {
            currentSpawnableAnimals.Add(animalType);
        }
        ResetNextAnimal();
    }

    // Restore spawnable animals to default list
    public void RestoreSpawnableAnimals()
    {
        currentSpawnableAnimals.Clear();
        foreach (var animalType in defaultSpawnableAnimals)
        {
            currentSpawnableAnimals.Add(animalType);
        }
        ResetNextAnimal();
    }

    // Spawn a new animal at a given horizontal position or default position
    public void RespawnAnimal(float x = 0)
    {
        Vector2 spawnPosition = animalSpawner.CalculateSpawnPosition(x);
        currentAnimal = animalSpawner.SpawnAnimal(nextAnimal, spawnPosition);
        currentAnimal.DisablePhysics(false, false);
        ResetNextAnimal();
        aimLine.EnableLine();
        currentAnimal.onCollision += ResetCurrent;
        ExplosionHighlighter.instance.CheckExplosives();
    }

    // Enable physics on current animal and clear reference
    public void ReleaseCurrentAnimal()
    {
        if (currentAnimal == null)
            return;

        currentAnimal.EnablePhysics();
        canControl = false;

        ScoreManager.instance.ResetCombo();
        aimLine.DisableLine();
    }


    public void ResetCurrent()
    {
        currentAnimal.onCollision -= ResetCurrent;
        currentAnimal = null;
    }

    #endregion


    #region Game State

    // Handle game state changes to freeze/unfreeze animals
    private void CheckState(GameState state)
    {
        if (animalsParentManager == null)
            return;

        if (state == GameState.Game)
        {
            animalsParentManager.UnfreezeAnimals();
        }
        else
        {
            ReleaseCurrentAnimal();
            animalsParentManager.FreezeAnimals();
        }
    }

    #endregion


    #region Save/Load

    private List<AnimalData> GetAnimalsData()
    {
        return animalsParentManager.GetAnimalsData();
    }

    public GameSessionData GetSessionData()
    {
        GameSessionData sessionData = new GameSessionData
        {
            animals = GetAnimalsData(),
            score = ScoreManager.instance.GetScoreData(),
            nextAnimal = nextAnimalType
        };
        return sessionData;
    }

    public void LoadSessionData(GameSessionData sessionData)
    {
        ResetGameplay();
        nextAnimalType = sessionData.nextAnimal;
        nextAnimal = animalSpawner.GetAnimalFromType(nextAnimalType);
        onNextAnimalSet?.Invoke();
        animalsParentManager.LoadAnimalsData(sessionData.animals, animalSpawner);
        animalsParentManager.UnfreezeAnimals();
    }

    #endregion


    #region Accessors

    public bool CanControl() => canControl;

    public Animal GetAnimalFromType(AnimalType type) => animalSpawner.GetAnimalFromType(type);
    public Animal SpawnAnimal(Animal animal, Vector2 position) => animalSpawner.SpawnAnimal(animal, position);
    
    public float GetIceChance() => iceChance;

    public void SetCanControl(bool control)
    {
        canControl = control;
    }

    public Animal GetNextAnimal() => nextAnimal;
    public Animal GetCurrentAnimal() => currentAnimal;

    #endregion
}
