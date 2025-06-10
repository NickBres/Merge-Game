using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;
using UnityEngine.UI;


public class AnimalManager : MonoBehaviour
{
    public static AnimalManager instance;
    [Header(" Elements ")]
    [SerializeField] private Transform animalsParent;
    [SerializeField] private Animal[] animalPrefabsRound;
    [SerializeField] private Animal[] animalPrefabsSquare;
    [SerializeField] private List<AnimalType> defaultSpawnableAnimals;
    private HashSet<AnimalType> currentSpawnableAnimals;
    [SerializeField] private Canvas uiCanvas;
    private GraphicRaycaster raycaster;
    private PointerEventData pointerEventData;


    public Animal currentAnimal;
    private Animal nextAnimal;
    private AnimalType nextAnimalType;
    private bool isFrozen = false;
    private bool isRush;

    private bool isTouchOverUI;



    [Header(" Settings ")]
    [SerializeField] private float fallingSpeed = 5f;
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
    }

    void Start()
    {
        currentSpawnableAnimals = new HashSet<AnimalType>();
        RestoreSpawnableAnimals();
        ResetNextAnimal();
    }

    private void Update()
    {
        isTouchOverUI = EventSystem.current.IsPointerOverGameObject();
        if (isRush)
        {
            if (currentAnimal != null && !isFrozen)
            {

                currentAnimal.MoveVertically(-fallingSpeed * Time.deltaTime);
                ManagePlayerInput();
            }
            else if (canSpawn && !isFrozen)
            {
                RespawnAnimal();
            }
        }
    }


    private void ResetNextAnimal()
    {
        nextAnimalType = currentSpawnableAnimals.ElementAt(Random.Range(0, currentSpawnableAnimals.Count));
        nextAnimal = GetAnimalFromType(nextAnimalType);
        onNextAnimalSet?.Invoke();
    }

    public void SetNextAnimal(AnimalType animalType)
    {
        nextAnimalType = animalType;
        nextAnimal = GetAnimalFromType(animalType);
        onNextAnimalSet?.Invoke();
    }

    private void ManagePlayerInput()
    {
        // Horizontal Movement
        float inputX = InputManager.instance.HorizontalInput;

        if (inputX != 0)
        {
            currentAnimal?.MoveHorizontally(inputX * moveSpeed * Time.deltaTime);
        }
        // drop
        if (InputManager.instance.InputActions.Gameplay.FastDrop.WasPressedThisFrame())
        {
            ReleaseAnimal();
        }

    }

    public void SwapSpawnableAnimals(List<AnimalType> toSwap)
    {
        currentSpawnableAnimals.Clear();
        foreach (var animalType in toSwap)
        {
            currentSpawnableAnimals.Add(animalType);
        }
        ResetNextAnimal();
    }

    public void RestoreSpawnableAnimals()
    {
        currentSpawnableAnimals.Clear();
        foreach (var animalType in defaultSpawnableAnimals)
        {
            currentSpawnableAnimals.Add(animalType);
        }
        ResetNextAnimal();
    }

    private void HandleTouchStart(Vector3 screenPos)
    {
        if (!GameManager.instance.IsGameState() || isTouchOverUI || IsTouchOverUI(screenPos))
            return;

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(screenPos);
        touchStartPos = new Vector2(screenPoint.x, screenPoint.y);

        if (!isRush && currentAnimal == null)
        {
            if (canSpawn)
            {
                RespawnAnimal(screenPos.x);
            }
            return;
        }
    }


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

    private void CheckGameMode(GameMode mode)
    {
        isRush = mode == GameMode.Rush;
    }


    private void HandleTouchEnd(Vector3 screenPos)
    {
        if (!GameManager.instance.IsGameState() || isTouchOverUI || IsTouchOverUI(screenPos)) // Check if the touch is over a UI element
            return;

        if (currentAnimal == null) return;

        Vector3 screenPoint = Camera.main.WorldToScreenPoint(screenPos);
        Vector2 delta = new Vector2(screenPoint.x, screenPoint.y) - touchStartPos;

        if (isRush)
        {

            // Swipe down
            if (delta.y < -swipeThreshold)
            {
                ReleaseAnimal();
                return;
            }

            // Tap left/right
            if (screenPoint.x < Screen.width / 2)
            {
                currentAnimal.MoveHorizontally(-1 * moveSpeed * Time.deltaTime); // left
            }
            else
            {
                currentAnimal.MoveHorizontally(moveSpeed * Time.deltaTime); // right
            }

        }
        else
        {
            ScoreManager.instance.ResetCombo();
            ReleaseAnimal();
        }
    }

    private void HandleTouchHold(Vector3 screenPos)
    {
        if (!GameManager.instance.IsGameState() || isTouchOverUI || IsTouchOverUI(screenPos))
            return;

        if (currentAnimal == null) return;

        if (isRush)
        {
            Vector3 screenPoint = Camera.main.WorldToScreenPoint(screenPos);
            if (screenPoint.x < Screen.width / 2)
            {
                currentAnimal.MoveHorizontally(-1 * moveSpeed * Time.deltaTime);
            }
            else
            {
                currentAnimal.MoveHorizontally(moveSpeed * Time.deltaTime);
            }
        }
        else
        {
            Vector3 clampedPos = screenPos;
            clampedPos.y = animalSpawnPoint.position.y;
            clampedPos.z = currentAnimal.transform.position.z;
            currentAnimal.SetPosition(clampedPos);
        }
    }

    private bool IsTouchOverUI(Vector3 worldPos)
    {
        pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Camera.main.WorldToScreenPoint(worldPos);

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        return results.Count > 0;
    }

    private void RespawnAnimal(float x = 0)
    {

        Vector2 spawnPosition = CalculateSpawnPosition(x);
        currentAnimal = SpawnAnimal(nextAnimal, spawnPosition);
        currentAnimal.DisablePhysics(false);
        ResetNextAnimal();
        canSpawn = false;
        DelaySpawn();
        
        if (isRush)
        {
            ScoreManager.instance.ResetCombo();
            currentAnimal.onCollision += ReleaseAnimal;
        }
    }

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



    private void ReleaseAnimal()
    {
        if (currentAnimal == null)
            return;

        currentAnimal.EnablePhysics();
        currentAnimal.onCollision -= ReleaseAnimal;
        currentAnimal = null;
    }

    public void RemoveAnimalsUpTo(AnimalType upTo, bool addToScore)
    {
        StartCoroutine(RemoveAllSmallAnimalsCoroutine(upTo, addToScore));
    }

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


    private void StopControlTimer()
    {
        canSpawn = true;
    }

    private Animal SpawnAnimal(Animal animal, Vector2 position)
    {
        Animal newAnimal = Instantiate(animal,
            position,
            Quaternion.identity,
            animalsParent);
        ApplySkinToAnimal(newAnimal);
        return newAnimal;
    }

    private void MergeAnimals(AnimalType type, Vector2 spawnPosition)
    {
        Animal newAnimal = GetAnimalFromType(type);
        newAnimal = SpawnAnimal(newAnimal, spawnPosition);
        newAnimal.EnablePhysics();
        IncreaseFallingSpeed((int)type);
        ComboPopUp.instance.ShowCombo(spawnPosition, ScoreManager.instance.GetComboCount());
        ScoreManager.instance.IncrementCombo();
    }

    private void DelaySpawn()
    {
        Invoke("StopControlTimer", spawnDelay);
    }

    private void IncreaseFallingSpeed(int amount)
    {
        // Normalize amount from 2–1024 to a 0–1 range using log scale
        float normalized = Mathf.InverseLerp(2f, 1024f, amount);
        float speedBoost = Mathf.Lerp(0.05f, 0.3f, normalized); // low type = small boost, high type = bigger
        fallingSpeed = Mathf.Min(fallingSpeed + speedBoost, maxFallingSpeed);
    }

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
    }

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
    }

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

    void OnDestroy()
    {
        MergeManager.onMergeAnimal -= MergeAnimals;
        InputManager.instance.OnTouchStart -= HandleTouchStart;
        InputManager.instance.OnTouchEnd -= HandleTouchEnd;
        InputManager.instance.OnTouchHold -= HandleTouchHold;

    }

    public Animal GetNextAnimal()
    {
        return nextAnimal;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enableGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-50, animalSpawnPoint.position.y, 0), new Vector3(50, animalSpawnPoint.position.y, 0));
    }

    public Animal GetCurrentAnimal()
    {
        return currentAnimal;
    }

#endif
}
