

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SocialPlatforms.Impl;
using Random = UnityEngine.Random;

public class AnimalManager : MonoBehaviour
{
    public static AnimalManager instance;
    [Header(" Elements ")]
    [SerializeField] private Transform animalsParent;
    [SerializeField] private Animal[] animalPrefabsRound;
    [SerializeField] private Animal[] animalPrefabsSquare;
    [SerializeField] private List<AnimalType> defaultSpawnableAnimals;
    private HashSet<AnimalType> currentSpawnableAnimals;


    public Animal currentAnimal;
    private Animal nextAnimal;
    private AnimalType nextAnimalType;
    private bool isFrozen = false;



    [Header(" Settings ")]
    [SerializeField] private float fallingSpeed = 5f;
    [SerializeField] private float maxFallingSpeed = 10f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Transform animalSpawnPoint;
    [SerializeField] private float spawnDelay = 0.5f;
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
    }

    void Start()
    {
        currentSpawnableAnimals = new HashSet<AnimalType>();
        RestoreSpawnableAnimals();
        ResetNextAnimal();
    }

    private void Update()
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

    private void HandleTouchStart(Vector2 screenPos)
    {
        if (!GameManager.instance.IsGameState() || EventSystem.current.IsPointerOverGameObject()) // Check if the touch is over a UI element
            return;

        touchStartPos = screenPos;
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
    private void HandleTouchEnd(Vector2 screenPos)
    {
        if (!GameManager.instance.IsGameState() || EventSystem.current.IsPointerOverGameObject()) // Check if the touch is over a UI element
            return;

        if (currentAnimal == null) return;

        Vector2 delta = screenPos - touchStartPos;

        // Swipe down
        if (delta.y < -swipeThreshold)
        {
            ReleaseAnimal();
            return;
        }

        // Tap left/right
        if (screenPos.x < Screen.width / 2)
        {
            currentAnimal.MoveHorizontally(-1 * moveSpeed * Time.deltaTime); // left
        }
        else
        {
            currentAnimal.MoveHorizontally(moveSpeed * Time.deltaTime); // right
        }
    }

    private void HandleTouchHold(Vector2 screenPos)
    {
        if (!GameManager.instance.IsGameState() || EventSystem.current.IsPointerOverGameObject())
            return;

        if (currentAnimal == null) return;

        // Move left or right based on touch position
        if (screenPos.x < Screen.width / 2)
        {
            currentAnimal.MoveHorizontally(-1 * moveSpeed * Time.deltaTime); // left
        }
        else
        {
            currentAnimal.MoveHorizontally(moveSpeed * Time.deltaTime); // right
        }
    }

    private void RespawnAnimal()
    {
        ScoreManager.instance.ResetCombo();
        currentAnimal = SpawnAnimal(nextAnimal, animalSpawnPoint.position);
        currentAnimal.DisablePhysics(false);
        currentAnimal.onCollision += ReleaseAnimal;
        ResetNextAnimal();
        canSpawn = false;
        DelaySpawn();
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
