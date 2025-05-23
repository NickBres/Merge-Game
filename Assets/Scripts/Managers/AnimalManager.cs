using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using TMPro;
using UnityEngine;

using Random = UnityEngine.Random;

public class AnimalManager : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Transform animalsParent;
    [SerializeField] private Animal[] animalPrefabsRound;
    [SerializeField] private Animal[] animalPrefabsSquare;
    private HashSet<AnimalType> spawnableAnimals;
    [SerializeField] private LineRenderer animalSpawnLine;


    private Animal currentAnimal;
    public Animal nextAnimal;
    public AnimalType nextAnimalType;
    private bool isFreeze = false;



    [Header(" Settings ")]
    [SerializeField] private Transform animalYSpawnPoint;
    [SerializeField] private float spawnDelay = 0.5f;
    private bool canControl = false;
    private bool isControlling = false;

    [Header(" Debug ")]
    [SerializeField] private bool enableGizmos;

    [Header(" Actions ")]
    public static Action onNextAnimalSet;

    void Awake()
    {
        MergeManager.onMergeAnimal += MergeAnimals;
        spawnableAnimals = new HashSet<AnimalType>();
        spawnableAnimals.Add(AnimalType.Snake);
        spawnableAnimals.Add(AnimalType.Parrot);
        spawnableAnimals.Add(AnimalType.Rabbit);
    }

    void Start()
    {
        canControl = true;
        HideLine();
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
            

        if (canControl)
            ManagePlayerInput();
    }

    private void ResetNextAnimal()
    {
        nextAnimalType = spawnableAnimals.ElementAt(Random.Range(0, spawnableAnimals.Count));
        nextAnimal = GetAnimalFromType(nextAnimalType);
        onNextAnimalSet?.Invoke();
    }

    private void ManagePlayerInput()
    {

        if (Input.GetMouseButtonDown(0))
        {
            MouseDownCallback();
        }
        else if (Input.GetMouseButton(0))
        {
            if (isControlling)
                MouseDragCallback();
            else
                MouseDownCallback();
        }
        else if (Input.GetMouseButtonUp(0) && isControlling)
        {
            MouseUpCallback();
        }
    }

    private void MouseDownCallback()
    {
        currentAnimal = SpawnAnimal(nextAnimal, GetSpawnPosition());
        ResetNextAnimal();
        isControlling = true;
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
    private void MouseDragCallback()
    {
        if (currentAnimal == null)
            return;


        DisplayLine();
        currentAnimal.MoveTo(GetSpawnPosition());
    }
    private void MouseUpCallback()
    {
        HideLine();
        currentAnimal.EnablePhysics();
        currentAnimal = null;

        canControl = false;
        StartControlTimer();
        isControlling = false;
    }

    private void StartControlTimer()
    {
        Invoke("StopControlTimer", spawnDelay);
    }

    private void StopControlTimer()
    {
        canControl = true;
    }

    private Animal SpawnAnimal(Animal animal, Vector2 position)
    {
        Animal newAnimal = Instantiate(animal,
            position,
            Quaternion.identity,
            animalsParent);
        return newAnimal;
    }

    private Vector2 GetClickedWorldPosition()
    {
        Vector2 screenPosition = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(screenPosition);
    }

    private Vector2 GetSpawnPosition()
    {
        Vector2 spawnPosition = GetClickedWorldPosition();
        spawnPosition.y = animalYSpawnPoint.position.y;
        return spawnPosition;
    }

    private void HideLine()
    {
        animalSpawnLine.enabled = false;
    }
    private void DisplayLine()
    {
        animalSpawnLine.enabled = true;
        animalSpawnLine.SetPosition(0, GetSpawnPosition());
        animalSpawnLine.SetPosition(1, GetSpawnPosition() + Vector2.down * 15);
    }

    private void MergeAnimals(AnimalType type, Vector2 spawnPosition)
    {
        Animal newAnimal = GetAnimalFromType(type);
        canControl = false;
        StartControlTimer();
        newAnimal = SpawnAnimal(newAnimal, spawnPosition);
        newAnimal.EnablePhysics();
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
            animal.EnablePhysics();
        }
        isFreeze = false;
    }

    void OnDestroy()
    {
        MergeManager.onMergeAnimal -= MergeAnimals;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enableGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-50, animalYSpawnPoint.position.y, 0), new Vector3(50, animalYSpawnPoint.position.y, 0));
    }

#endif
}
