using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;
using UnityEngine.UI;


public class GameplayController : MonoBehaviour
{
    public static GameplayController instance;
    // New fields for touch hold duration and move delay
    private float holdDuration = 0f;
    [SerializeField] private float moveDelay = 0.1f; // seconds before movement allowed
    [Header(" Elements ")]
    [Header(" Boundaries ")]
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform rightWall;
    public float MinX => leftWall.position.x;
    public float MaxX => rightWall.position.x;
    [SerializeField] private Transform animalsParent;
    [SerializeField] private Animal[] animalPrefabsRound;
    [SerializeField] private Animal[] animalPrefabsSquare;
    [SerializeField] private Capybara capybaraPrefab;
    [SerializeField] private Egg eggPrefab;
    [SerializeField] private List<AnimalType> defaultSpawnableAnimals;
    private HashSet<AnimalType> currentSpawnableAnimals;
    [SerializeField] private Canvas uiCanvas;
    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;
    [SerializeField] private AimLine aimLine;


    public Animal currentAnimal;
    private Animal nextAnimal;
    private AnimalType nextAnimalType;
    private bool isFrozen = false;

    private bool isTouchOverUI;



    [Header(" Settings ")]
    [SerializeField] private Transform animalSpawnPoint;
    [SerializeField] private float spawnPositionOffset = 0;
    [SerializeField] private float iceChance = 0.1f;
    [SerializeField] private int eggLimit = 2;
    [SerializeField] private float eggSpawnChance = 0.1f; // Chance to spawn an egg instead of a normal animal
    private bool canControl = true;
    private Vector2 touchStartPos;


    [Header(" Debug ")]
    [SerializeField] private bool enableGizmos;

    [Header(" Actions ")]
    public static Action onNextAnimalSet;

    #region Unity Lifecycle

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

        MergeManager.onMergeAnimal += MergeAnimals;

        InputManager.instance.OnTouchStart += HandleTouchStart;
        InputManager.instance.OnTouchEnd += HandleTouchEnd;
        InputManager.instance.OnTouchHold += HandleTouchHold;

        GameManager.OnGameStateChanged += CheckState;
        raycaster = uiCanvas.GetComponent<GraphicRaycaster>();
        aimLine.DisableLine();
    }

    // Initialize spawnable animals and next animal
    void Start()
    {
        ResetGameplay();
    }

    // Update loop to handle animal movement and spawning
    private void Update()
    {
        if (GameManager.instance.GetGameState() != GameState.Game) return;

        isTouchOverUI = EventSystem.current.IsPointerOverGameObject();
        if (currentAnimal != null)
            aimLine.MoveLine(currentAnimal.transform.position);
    }

    // Unsubscribe from events
    void OnDestroy()
    {
        MergeManager.onMergeAnimal -= MergeAnimals;
        InputManager.instance.OnTouchStart -= HandleTouchStart;
        InputManager.instance.OnTouchEnd -= HandleTouchEnd;
        InputManager.instance.OnTouchHold -= HandleTouchHold;

    }

    public void ResetGameplay()
    {
        foreach (Transform child in animalsParent)
        {
            Destroy(child.gameObject);
        }
        currentAnimal = null;
        isFrozen = false;
        canControl = true;
        currentSpawnableAnimals = new HashSet<AnimalType>(defaultSpawnableAnimals);
        ResetNextAnimal();
        aimLine.DisableLine();
    }

    #endregion

    #region Event Handling

    // Handle game state changes to freeze/unfreeze animals
    private void CheckState(GameState state)
    {
        if (animalsParent == null)
            return;

        if (state == GameState.Game)
        {
            UnfreezeAnimals();
        }
        else
        {
            FreezeAnimals();
        }
    }
    #endregion

    #region Animal Management

    // Select a random next animal from current spawnable animals
    private void ResetNextAnimal()
    {
        if (!TryToSpawnEgg())
        {
            nextAnimalType = currentSpawnableAnimals.ElementAt(Random.Range(0, currentSpawnableAnimals.Count));
            nextAnimal = GetAnimalFromType(nextAnimalType);
        }
        onNextAnimalSet?.Invoke();
    }

    public void SetNextAnimal(AnimalType animalType)
    {
        nextAnimalType = animalType;
        nextAnimal = GetAnimalFromType(animalType);
        onNextAnimalSet?.Invoke();
    }

    public void SetNextCapybara()
    {
        nextAnimal = capybaraPrefab;
        onNextAnimalSet?.Invoke();
    }

    // Get an animal prefab by type, randomly choosing round or square variant
    public Animal GetAnimalFromType(AnimalType animalType)
    {
        bool isRound = Random.value > 0.5f;
        if (isRound)
        {
            for (int i = 0; i < animalPrefabsRound.Length; i++)
            {
                if (animalPrefabsRound[i].GetAnimalType() == animalType)
                {
                    return animalPrefabsRound[i];
                }
            }
        }
        else
        {
            for (int i = 0; i < animalPrefabsSquare.Length; i++)
            {
                if (animalPrefabsSquare[i].GetAnimalType() == animalType)
                {
                    return animalPrefabsSquare[i];
                }
            }
        }
        return null;
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

    #endregion

    #region Input Handling

    // Handle touch start input
    private void HandleTouchStart(Vector3 screenPos)
    {
        if (!GameManager.instance.IsGameState() || isTouchOverUI || IsTouchOverUI(screenPos) || !canControl)
            return;

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(screenPos);
        touchStartPos = new Vector2(screenPoint.x, screenPoint.y);
        RespawnAnimal(screenPos.x);

        CheckExplosives();
    }

    // Handle touch end input, including swipe detection and tap movement
    private void HandleTouchEnd(Vector3 screenPos)
    {
        if (!GameManager.instance.IsGameState() || !canControl)
            return;

        ReleaseAnimal();
    }

    // Handle touch hold input for continuous movement or positioning
    private void HandleTouchHold(Vector3 screenPos)
    {

        if (!GameManager.instance.IsGameState() || !canControl)
            return;

        if (currentAnimal == null) return;

        Vector3 newPos = screenPos;
        float halfWidth = 0.5f;
        var collider = currentAnimal.GetComponent<Collider2D>();
        if (collider != null)
        {
            halfWidth = collider.bounds.extents.x + 0.2f;
        }

        float minX = MinX + halfWidth;
        float maxX = MaxX - halfWidth;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = animalSpawnPoint.position.y;
        newPos.z = currentAnimal.transform.position.z;
        currentAnimal.SetPosition(newPos);
    }

    // Check if a world position corresponds to a UI element
    private bool IsTouchOverUI(Vector3 worldPos)
    {
        pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Camera.main.WorldToScreenPoint(worldPos);

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        foreach (var result in results)
        {
            if (result.gameObject.CompareTag("IgnoreRaycast"))
                continue;

            return true; // Some other UI element was hit
        }
        return false;
    }

    #endregion

    #region Animal Spawning and Releasing

    // Spawn a new animal at a given horizontal position or default position
    private void RespawnAnimal(float x = 0)
    {

        Vector2 spawnPosition = CalculateSpawnPosition(x);
        currentAnimal = SpawnAnimal(nextAnimal, spawnPosition);
        currentAnimal.DisablePhysics(false, false);
        ResetNextAnimal();
        aimLine.EnableLine();

        currentAnimal.onCollision += ResetCurrent;
    }

    // Calculate spawn position with optional horizontal offset
    private Vector2 CalculateSpawnPosition(float x)
    {
        Vector2 spawnPosition = new Vector2(x, animalSpawnPoint.position.y);
        return spawnPosition;
    }

    // Enable physics on current animal and clear reference
    private void ReleaseAnimal()
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
        canControl = true;
    }


    // Instantiate a new animal prefab at a position
    public Animal SpawnAnimal(Animal animal, Vector2 position)
    {

        Animal newAnimal = Instantiate(animal,
            position,
            Quaternion.identity,
            animalsParent);
        ApplySkinToAnimal(newAnimal);

        Vector2 adjustedPos = AdjustSpawnPoint(position, newAnimal);
        newAnimal.transform.position = adjustedPos;
        EnsureSpawnAreaClear(adjustedPos, newAnimal);

        if (newAnimal.GetComponent<Capybara>() != null)
        {
            AudioManager.instance.PlayChoirSound();
        }
        return newAnimal;
    }

    private bool TryToSpawnEgg()
    {
        float chance = Random.value;
        if (chance > eggSpawnChance || CountEggs() >= eggLimit || GameOverManager.instance.closeToDeath) return false;
        nextAnimal = eggPrefab;
        nextAnimalType = AnimalType.Egg;
        return true;
    }

    private int CountEggs()
    {
        int count = 0;
        foreach (Transform child in animalsParent)
        {
            Animal animal = child.GetComponent<Animal>();
            if (animal != null && animal.GetAnimalType() == AnimalType.Egg)
            {
                count++;
            }
        }
        return count;
    }

    #endregion

    #region Merge Logic

    // Handle merging animals by spawning new merged animal and updating score/combo
    private void MergeAnimals(AnimalType type, Vector2 spawnPosition)
    {
        Animal newAnimal = GetAnimalFromType(type);
        newAnimal = SpawnAnimal(newAnimal, spawnPosition);
        newAnimal.EnablePhysics();
        ComboPopUp.instance.ShowCombo(spawnPosition, ScoreManager.instance.GetComboCount());
        ScoreManager.instance.IncrementCombo();
        if (ScoreManager.instance.isEpicCombo())
        {
            newAnimal.MakeExplosive();
        }
        if (Random.value < iceChance && !GameOverManager.instance.closeToDeath)
        {
            newAnimal.ApplyIce();
        }
    }



    #endregion

    #region Gameplay Mechanics

    // Remove all animals smaller than a given type, optionally adding to score
    public void RemoveAnimalsUpTo(AnimalType upTo, bool addToScore)
    {
        StartCoroutine(RemoveAllSmallAnimalsCoroutine(upTo, addToScore));
    }

    // Coroutine to remove small animals sequentially with delay
    private System.Collections.IEnumerator RemoveAllSmallAnimalsCoroutine(AnimalType upTo, bool addToScore)
    {
        FreezeAnimals();

        while (true)
        {
            bool found = false;
            foreach (Transform child in animalsParent)
            {
                Animal animal = child.GetComponent<Animal>();
                if (animal != null && animal != currentAnimal && animal.GetAnimalType() < upTo)
                {
                    if (addToScore)
                        ScoreManager.instance.UpdateScore(animal.GetAnimalType(), Vector2.zero);

                    animal.Disappear();
                    yield return new WaitForSeconds(0.2f);
                    found = true;
                    break;
                }
            }
            if (!found)
                break;
        }

        UnfreezeAnimals();
    }

    // Freeze all animals' physics
    private void FreezeAnimals()
    {
        if (isFrozen)
            return;
        foreach (Transform child in animalsParent)
        {
            Animal animal = child.GetComponent<Animal>();
            animal.DisablePhysics(true);
        }
        isFrozen = true;
        canControl = false;
        GameOverManager.instance.SetCanLoose(false);

    }

    // Unfreeze animals, enabling physics except for current animal which is unfrozen differently
    private void UnfreezeAnimals()
    {
        if (!isFrozen)
            return;
        foreach (Transform child in animalsParent)
        {
            Animal animal = child.GetComponent<Animal>();
            animal.EnablePhysics();
        }
        isFrozen = false;
        canControl = true;
        GameOverManager.instance.SetCanLoose(true);
    }

    #endregion

    #region Skin Application

    // Apply skin sprite to animal if available
    public void ApplySkinToAnimal(Animal animal)
    {
        var skin = SkinManager.instance.GetSkinForAnimal(animal.GetAnimalType());
        if (skin != null && skin.sprite != null)
        {
            var skinRenderer = animal.transform.Find("Skin/Skin Renderer")?.GetComponent<SpriteRenderer>();
            if (skinRenderer != null)
            {
                skinRenderer.sprite = skin.sprite;
                skinRenderer.enabled = true;
            }
        }
    }

    #endregion

    #region Public Accessors

    // Get the next animal prefab
    public Animal GetNextAnimal()
    {
        return nextAnimal;
    }
    public Animal GetCurrentAnimal()
    {
        return currentAnimal;
    }


    #endregion


    #region Helper Methods

    private void CheckExplosives()
    {
        foreach (Transform child in animalsParent)
        {
            Animal animal = child.GetComponent<Animal>();
            if (animal != null && animal.isExplosive)
            {
                return;
            }
        }

        foreach (Transform child in animalsParent)
        {
            Animal animal = child.GetComponent<Animal>();
            if (animal != null)
            {
                animal.MarkForExplosion(false);
            }
        }
        

        
    }

    private void EnsureSpawnAreaClear(Vector2 spawnPoint, Animal prefab)
    {
        int maxAttempts = 20;
        float radius = prefab.GetComponent<Collider2D>().bounds.extents.magnitude;
        LayerMask animalLayer = LayerMask.GetMask("Animal"); // Ensure your animal objects are on the "Animal" layer

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPoint, radius, animalLayer);
            if (hits.Length == 0)
                break;

            foreach (var hit in hits)
            {
                Animal a = hit.GetComponent<Animal>();
                if (a != null)
                {
                    Vector2 pushDir = (a.transform.position - (Vector3)spawnPoint).normalized;
                    if (pushDir == Vector2.zero) pushDir = UnityEngine.Random.insideUnitCircle.normalized;
                    a.Push(pushDir * 0.5f); // Assumes Animal has a Push method
                }
            }

            Physics2D.SyncTransforms(); // Make sure physics state is updated
        }
    }

    private Vector2 AdjustSpawnPoint(Vector2 spawnPoint, Animal prefab)
    {
        Collider2D collider = prefab.GetComponent<Collider2D>();
        float radius = Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.y);
        Collider2D[] hits = Physics2D.OverlapCircleAll(spawnPoint, radius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Wall"))
            {
                Vector2 closestPoint = hit.ClosestPoint(spawnPoint);
                Vector2 direction = spawnPoint - closestPoint;
                float distance = direction.magnitude;
                if (distance < radius)
                {
                    Vector2 correction = direction.normalized * (radius - distance);
                    spawnPoint += correction;
                }
            }
        }

        return spawnPoint;
    }


    #endregion

    #region Gizmos

#if UNITY_EDITOR
    // Draw spawn line gizmo in editor
    private void OnDrawGizmos()
    {
        if (!enableGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-50, animalSpawnPoint.position.y, 0), new Vector3(50, animalSpawnPoint.position.y, 0));
    }
#endif

    #endregion
}
