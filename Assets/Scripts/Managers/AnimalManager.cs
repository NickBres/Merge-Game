

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
    private HashSet<AnimalType> spawnableAnimals;


    private Animal currentAnimal;
    public Animal nextAnimal;
    public AnimalType nextAnimalType;
    private bool isFreeze = false;



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
        spawnableAnimals = new HashSet<AnimalType>();
        spawnableAnimals.Add(AnimalType.Snake);
        spawnableAnimals.Add(AnimalType.Parrot);
        spawnableAnimals.Add(AnimalType.Rabbit);

        InputManager.instance.OnTouchStart += HandleTouchStart;
        InputManager.instance.OnTouchEnd += HandleTouchEnd;
        InputManager.instance.OnTouchHold += HandleTouchHold;
    }

    void Start()
    {
        ResetNextAnimal();
    }

    private void Update()
    {
        if (!GameManager.instance.IsGameState())
        {
            FreezeAnimals();
            return;
        }
        else
        {
            UnfreezeAnimals();
        }

        if (currentAnimal != null)
        {
            currentAnimal.MoveVertically(-fallingSpeed * Time.deltaTime);
            ManagePlayerInput();
        }
        else if (canSpawn)
        {
            RespawnAnimal();
        }
    }

    private void ResetNextAnimal()
    {
        nextAnimalType = spawnableAnimals.ElementAt(Random.Range(0, spawnableAnimals.Count));
        nextAnimal = GetAnimalFromType(nextAnimalType);
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

    private void HandleTouchStart(Vector2 screenPos)
    {
        if (!GameManager.instance.IsGameState() || EventSystem.current.IsPointerOverGameObject()) // Check if the touch is over a UI element
            return;

        touchStartPos = screenPos;
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
        currentAnimal.DisablePhysics();
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
        if (isFreeze)
            return;
        foreach (Transform child in animalsParent)
        {
            Animal animal = child.GetComponent<Animal>();
            animal.DisablePhysics();
        }
        isFreeze = true;
    }

    private void UnfreezeAnimals()
    {
        if (!isFreeze)
            return;
        foreach (Transform child in animalsParent)
        {
            Animal animal = child.GetComponent<Animal>();
            if (animal == currentAnimal)
                continue;
            animal.EnablePhysics();
        }
        isFreeze = false;
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
