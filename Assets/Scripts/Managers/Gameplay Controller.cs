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
    [Header(" Elements ")]
    [Header(" Boundaries ")]
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform rightWall;
    public float MinX => leftWall.position.x;
    public float MaxX => rightWall.position.x;
    [SerializeField] private Transform animalsParent;
    [SerializeField] private Animal[] animalPrefabsRound;
    [SerializeField] private Animal[] animalPrefabsSquare;
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
    private bool isRush;
    [SerializeField] private float rushAccelerationTime = 1f; // Time in seconds to reach max speed
    private float rushHoldTime = 0f;

    private bool isTouchOverUI;



    [Header(" Settings ")]
    [SerializeField] private float fallingSpeed = 5f;
    private float currentFallingSpeed;
    [SerializeField] private float maxFallingSpeed = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform animalSpawnPoint;
    [SerializeField] private float spawnDelay = 0.5f;
    [SerializeField] private float spawnPositionOffset = 0;
    private bool canSpawn = true;
    private Vector2 touchStartPos;
    [SerializeField] private float swipeThreshold = 50f; // in pixels


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
        GameManager.OnGameModeChanged += CheckGameMode;
        isRush = GameManager.instance.GetGameMode() == GameMode.Rush;
        raycaster = uiCanvas.GetComponent<GraphicRaycaster>();
        currentFallingSpeed = fallingSpeed;
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
        if (isRush)
        {
            if (currentAnimal != null && !isFrozen)
            {

                currentAnimal.MoveVertically(-currentFallingSpeed * Time.deltaTime);
                ManagePlayerInput();
            }
            else if (canSpawn && !isFrozen)
            {
                RespawnAnimal();
            }
        }
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
        currentFallingSpeed = fallingSpeed;
        isFrozen = false;
        canSpawn = true;
        currentSpawnableAnimals = new HashSet<AnimalType>(defaultSpawnableAnimals);
        ResetNextAnimal();
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

    // Update rush mode flag on game mode change
    private void CheckGameMode(GameMode mode)
    {
        isRush = mode == GameMode.Rush;
    }

    #endregion

    #region Animal Management

    // Select a random next animal from current spawnable animals
    private void ResetNextAnimal()
    {
        nextAnimalType = currentSpawnableAnimals.ElementAt(Random.Range(0, currentSpawnableAnimals.Count));
        nextAnimal = GetAnimalFromType(nextAnimalType);
        onNextAnimalSet?.Invoke();
    }

    // Set next animal explicitly
    public void SetNextAnimal(AnimalType animalType)
    {
        nextAnimalType = animalType;
        nextAnimal = GetAnimalFromType(animalType);
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
        if (!GameManager.instance.IsGameState() || isTouchOverUI || IsTouchOverUI(screenPos))
            return;

        if (isRush) rushHoldTime = 0f;

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(screenPos);
        touchStartPos = new Vector2(screenPoint.x, screenPoint.y);



        if (!isRush)
        {
            if (canSpawn && currentAnimal == null)
            {
                aimLine.EnableLine();
                aimLine.MoveLine(screenPos.x);
                RespawnAnimal(screenPos.x);
            }
        }
    }

    // Handle touch end input, including swipe detection and tap movement
    private void HandleTouchEnd(Vector3 screenPos)
    {
        if (!GameManager.instance.IsGameState() || isTouchOverUI || IsTouchOverUI(screenPos)) // Check if the touch is over a UI element
            return;

        if (currentAnimal == null) return;

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(screenPos);
        Vector2 delta = new Vector2(screenPoint.x, screenPoint.y) - touchStartPos;

        if (isRush)
        {
            rushHoldTime = 0f;
            // Swipe down
            if (delta.y < -swipeThreshold)
            {
                ReleaseAnimal();
                return;
            }

            MoveAnimal(moveSpeed * Time.deltaTime * (screenPoint.x < Screen.width / 2 ? -1 : 1));

        }
        else
        {
            aimLine.DisableLine();
            ScoreManager.instance.ResetCombo();
            ReleaseAnimal();
        }
    }

    // Handle touch hold input for continuous movement or positioning
    private void HandleTouchHold(Vector3 screenPos)
    {
        if (!GameManager.instance.IsGameState() || isTouchOverUI || IsTouchOverUI(screenPos))
            return;

        if (currentAnimal == null) return;

        if (isRush)
        {
            rushHoldTime += Time.deltaTime;
            float speedMultiplier = Mathf.Clamp01(rushHoldTime / rushAccelerationTime);
            float adjustedSpeed = moveSpeed * speedMultiplier;

            Vector3 screenPoint = Camera.main.WorldToScreenPoint(screenPos);
            MoveAnimal(adjustedSpeed * Time.deltaTime * (screenPoint.x < Screen.width / 2 ? -1 : 1));
        }
        else
        {
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
            aimLine.MoveLine(newPos.x);
        }
    }

    // Manage horizontal movement and fast drop input
    private void ManagePlayerInput()
    {
        // Horizontal Movement
        float inputX = InputManager.instance.HorizontalInput;

        if (inputX != 0)
        {
            float moveDist = inputX * moveSpeed * Time.deltaTime;
            MoveAnimal(moveDist);
        }
        // drop
        if (InputManager.instance.InputActions.Gameplay.FastDrop.WasPressedThisFrame())
        {
            ReleaseAnimal();
        }
    }

    private void MoveAnimal(float moveDist)
    {
        if (currentAnimal == null) return;
        Vector3 newPos = currentAnimal.transform.position + new Vector3(moveDist, 0, 0);
        float halfWidth = 0.5f;
        var collider = currentAnimal.GetComponent<Collider2D>();
        if (collider != null)
        {
            halfWidth = collider.bounds.extents.x + 0.2f;
        }

        float minX = MinX + halfWidth;
        float maxX = MaxX - halfWidth;
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y = currentAnimal.transform.position.y;
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
        canSpawn = false;
        DelaySpawn();

        if (isRush)
        {
            ScoreManager.instance.ResetCombo();
            currentAnimal.onCollision += ReleaseAnimal;
        }
    }

    // Calculate spawn position with optional horizontal offset
    private Vector2 CalculateSpawnPosition(float x)
    {
        Vector2 spawnPosition = new Vector2(x, animalSpawnPoint.position.y);
        if (isRush)
        {
            spawnPosition.x = animalSpawnPoint.position.x;
            spawnPosition.y += spawnPositionOffset;
        }
        return spawnPosition;
    }

    // Enable physics on current animal and clear reference
    private void ReleaseAnimal()
    {
        if (currentAnimal == null)
            return;

        currentAnimal.EnablePhysics();
        currentAnimal.onCollision -= ReleaseAnimal;
        currentAnimal = null;
    }

    // Delay allowing next spawn
    private void DelaySpawn()
    {
        Invoke("StopControlTimer", spawnDelay);
    }

    // Allow spawning again
    private void StopControlTimer()
    {
        canSpawn = true;
    }

    // Instantiate a new animal prefab at a position
    private Animal SpawnAnimal(Animal animal, Vector2 position)
    {
        Animal newAnimal = Instantiate(animal,
            position,
            Quaternion.identity,
            animalsParent);
        ApplySkinToAnimal(newAnimal);
        return newAnimal;
    }

    #endregion

    #region Merge Logic

    // Handle merging animals by spawning new merged animal and updating score/combo
    private void MergeAnimals(AnimalType type, Vector2 spawnPosition)
    {
        Animal newAnimal = GetAnimalFromType(type);
        newAnimal = SpawnAnimal(newAnimal, spawnPosition);
        newAnimal.EnablePhysics();
        IncreaseFallingSpeed((int)type);
        ComboPopUp.instance.ShowCombo(spawnPosition, ScoreManager.instance.GetComboCount());
        ScoreManager.instance.IncrementCombo();
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

    // Increase falling speed based on animal type using normalized log scale
    private void IncreaseFallingSpeed(int amount)
    {
        // Normalize amount from 2–1024 to a 0–1 range using log scale
        float normalized = Mathf.InverseLerp(2f, 1024f, amount);
        float speedBoost = Mathf.Lerp(0.05f, 0.3f, normalized); // low type = small boost, high type = bigger
        currentFallingSpeed = Mathf.Min(currentFallingSpeed + speedBoost, maxFallingSpeed);
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
        canSpawn = false;
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
            if (animal == currentAnimal)
                animal.Unfreeze();
            else
                animal.EnablePhysics();
        }
        isFrozen = false;
        canSpawn = true;
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
