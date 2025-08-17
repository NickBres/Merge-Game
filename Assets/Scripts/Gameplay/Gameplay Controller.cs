using UnityEngine;
using Random = UnityEngine.Random;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class GameplayController : MonoBehaviour
{
    public static GameplayController instance;
    [Header(" Elements ")]
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform rightWall;
    [SerializeField] private Transform ground;
    public float MinX => leftWall.position.x;
    public float MaxX => rightWall.position.x;
    public float MinY => ground.position.y;
    [SerializeField] private List<AnimalType> defaultSpawnableAnimals;
    private HashSet<AnimalType> currentSpawnableAnimals;
    [SerializeField] private Canvas uiCanvas;
    [SerializeField] private AimLine aimLine;
    [SerializeField] private Transform[] spawnPoints;

    private AnimalsParent animalsParentManager;

    private Animal currentAnimal;
    private Animal nextAnimal;
    private AnimalType nextAnimalType;
    private AnimalType[] animalsToChooseFrom = new AnimalType[3];
    private List<Animal> mockups = new List<Animal>();
    private int lastIndex = 0;



    [Header(" Settings ")]
    [SerializeField] private float iceChance = 0.1f;
    [SerializeField] private int eggLimit = 2;
    [SerializeField] private float eggSpawnChance = 0.1f; // Chance to spawn an egg instead of a normal animal
    private bool canControl = true;
    private ShapeState animalsShape;
    private AnimalSpawner animalSpawner;

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

        CheckMockupsEmptied();
    }

    public void ResetGameplay()
    {
        if (animalSpawner == null)
            animalSpawner = AnimalSpawner.instance;
        animalSpawner.ResetAnimalsParent();
        currentAnimal = null;
        canControl = true;
        currentSpawnableAnimals = new HashSet<AnimalType>(defaultSpawnableAnimals);
        aimLine.DisableLine();
        animalsShape = GameManager.instance.GetAnimalShape();
        animalSpawner.SetShapeState(animalsShape);
        ClearSpawnAnimals();
    }

    public void SetNextAnimal(AnimalType animalType, bool setShape = false, bool toRound = false)
    {
        nextAnimalType = animalType;
        nextAnimal = animalSpawner.GetAnimalFromType(animalType, setShape, toRound);
    }

    public void SetNextCapybara()
    {
        ClearSpawnAnimals();
        Animal animal = animalSpawner.GetCapybaraPrefab();
        animal = animalSpawner.SpawnAnimal(animal, spawnPoints[1].position);
        animal.MakeMockup();
        mockups.Add(animal);
    }

    public void SetNextBomb()
    {
        ClearSpawnAnimals();
        Animal animal = animalSpawner.GetAnimalFromType(AnimalType.Bomb);
        animal = animalSpawner.SpawnAnimal(animal, spawnPoints[1].position);
        animal.MakeMockup();
        mockups.Add(animal);
    }

    // Replace current spawnable animals with a new list
    public void SwapSpawnableAnimals(List<AnimalType> toSwap)
    {
        currentSpawnableAnimals.Clear();
        foreach (var animalType in toSwap)
        {
            currentSpawnableAnimals.Add(animalType);
        }
        SpawnAnimalsToChooseFrom();
    }

    // Restore spawnable animals to default list
    public void RestoreSpawnableAnimals()
    {
        currentSpawnableAnimals.Clear();
        foreach (var animalType in defaultSpawnableAnimals)
        {
            currentSpawnableAnimals.Add(animalType);
        }
        SpawnAnimalsToChooseFrom();
    }

    // Spawn a new animal at a given horizontal position or default position
    public void RespawnAnimal(float x = 0)
    {
        currentAnimal = animalSpawner.SpawnAnimal(nextAnimal, Vector2.zero);
        currentAnimal.DisablePhysics(false, true);
        aimLine.EnableLine();
        currentAnimal.onCollision += ResetCurrent;
        ExplosionHighlighter.instance.CheckExplosives();
    }

    // Enable physics on current animal and clear reference
    public void ReleaseCurrentAnimal()
    {
        if (CanBeReleased())
        {
            currentAnimal.EnablePhysics();
            canControl = false;
            RemoveMockup(lastIndex);
        }
        else
        {
            ReturnToLastIndex();
        }
            
        ScoreManager.instance.ResetCombo();
        aimLine.DisableLine();
    }

    private void ReturnToLastIndex()
    {
        ShowMockup(lastIndex);
        if (currentAnimal == null) return;
        currentAnimal.onCollision -= ResetCurrent;
        Destroy(currentAnimal.gameObject);
        currentAnimal = null;
        AudioManager.instance.PlayClickSound();
    }

    private bool CanBeReleased()
    {
        if (currentAnimal == null)
            return false;

        Vector2 position = currentAnimal.transform.position;

        // 1. Check if inside board horizontally
        if (position.x < MinX || position.x > MaxX || position.y < MinY)
            return false;

        float currAnimalRadius = currentAnimal.GetComponent<Collider2D>()?.bounds.extents.x ?? 0.5f;

        // 2. Check if colliding with wall or other animal
        Collider2D[] hits = Physics2D.OverlapCircleAll(position,currAnimalRadius); // radius depends on animal size
        foreach (var hit in hits)
        {
            if (hit.gameObject == currentAnimal.gameObject)
                continue;

            if (hit.CompareTag("Wall") || hit.GetComponent<Animal>() != null)
                return false;
        }

        return true;
    }


    public void ResetCurrent()
    {
        if (currentAnimal == null)
            return;
        currentAnimal.onCollision -= ResetCurrent;
        currentAnimal = null;
    }

    private void RefillAnimalsToChooseFrom()
    {
        animalsToChooseFrom[0] = currentSpawnableAnimals.ElementAt(Random.Range(0, currentSpawnableAnimals.Count));
        animalsToChooseFrom[1] = currentSpawnableAnimals.ElementAt(Random.Range(0, currentSpawnableAnimals.Count));
        animalsToChooseFrom[2] = currentSpawnableAnimals.ElementAt(Random.Range(0, currentSpawnableAnimals.Count));
    }

    private void SpawnAnimalsToChooseFrom()
    {
        ClearSpawnAnimals();
        RefillAnimalsToChooseFrom();
        for (int i = 0; i < animalsToChooseFrom.Length; i++)
        {
            Animal animal = animalSpawner.GetAnimalFromType(animalsToChooseFrom[i]);
            if (animalSpawner.TryToSpawnEgg(eggSpawnChance, eggLimit)) animal = animalSpawner.GetEggPrefab();
            animal = animalSpawner.SpawnAnimal(animal, spawnPoints[i].position);
            animal.MakeMockup();
            mockups.Add(animal);
        }
    }

    private void ClearSpawnAnimals()
    {
        foreach (var mockup in mockups)
        {
            if (mockup != null)
            {
                Destroy(mockup.gameObject);
            }
        }
        mockups.Clear();
    }

    private void HideMockup(int index)
    {
        if (index < 0 || index >= animalsToChooseFrom.Length)
            return;

        Animal mockup = mockups[index];
        if (mockup != null)
        {
            mockup.gameObject.SetActive(false);
        }

    }

    private void ShowMockup(int index)
    {
        if (index < 0 || index >= mockups.Count)
            return;

        Animal mockup = mockups[index];
        if (mockup != null)
        {
            mockup.gameObject.SetActive(true);
        }
    }
    private void RemoveMockup(int index)
    {
        if (index < 0 || index >= animalsToChooseFrom.Length)
            return;

        Animal mockup = mockups[index];
        if (mockup != null)
        {
            mockups.Remove(mockup);
            Destroy(mockup.gameObject);
        }
    }

    private void CheckMockupsEmptied()
    {
        if (mockups.Count == 0)
        {
            SpawnAnimalsToChooseFrom();
        }
    }

    public List<Animal> GetMockupAnimals() => mockups;


    public Animal GetAnimalFromMockup(int index)
    {
        if (index < 0 || index >= animalsToChooseFrom.Length)
            return null;

        return mockups[index] == null ? null : mockups[index];
    }

    public void HandleMockupTouch(int index)
    {
        if (index < 0 || index >= animalsToChooseFrom.Length)
            return;

        Animal mockup = GetAnimalFromMockup(index);
        if (mockup == null)
            return;

        HideMockup(index);
        SetNextAnimal(mockup.GetAnimalType(), true, mockup.IsRound());
        lastIndex = index;
        RespawnAnimal();
        AudioManager.instance.PlayWhooshSound();
    }

    public bool IsMockup(Animal animal)
    {
        return mockups.Contains(animal);
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
        animalsParentManager.LoadAnimalsData(sessionData.animals, animalSpawner);

        List<Animal> animals = animalsParentManager.GetAnimals();

        foreach (var animal in animals)
        {
            if (animal.IsMockup())
            {
                mockups.Add(animal);
            }
        }
        
        animalsParentManager.UnfreezeAnimals();
    }

    public void IncreaseDifficulty()
    {
        eggLimit++;
        if (eggLimit > 4) eggLimit = 4;
        iceChance += 0.05f;
        if (iceChance > 0.5f) iceChance = 0.5f;
    }

    #endregion


    #region Accessors

    public bool CanControl() => canControl;

    public Animal GetAnimalFromType(AnimalType type) => animalSpawner.GetAnimalFromType(type);
    public Animal SpawnAnimal(Animal animal, Vector2 position, bool physics = false) => animalSpawner.SpawnAnimal(animal, position, physics);
    
    public float GetIceChance() => iceChance;

    public void SetCanControl(bool control)
    {
        canControl = control;
    }

    public Animal GetNextAnimal() => nextAnimal;
    public Animal GetCurrentAnimal() => currentAnimal;

    #endregion
}
