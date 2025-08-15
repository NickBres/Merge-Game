

using System.Collections.Generic;
using UnityEngine;

public class AnimalSpawner : MonoBehaviour
{
    public static AnimalSpawner instance;
    [SerializeField] private Animal[] animalPrefabsRound;
    [SerializeField] private Animal[] animalPrefabsSquare;
    [SerializeField] private Capybara capybaraPrefab;
    [SerializeField] private Egg eggPrefab;
    [SerializeField] private Transform animalsParent;
    private ShapeState shapeState;


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
    }


    public void SetShapeState(ShapeState state)
    {
        shapeState = state;
    }

    public Animal GetAnimalFromType(AnimalType animalType, bool setShape = false, bool toRound = false)
    {
        if (animalType == AnimalType.Egg) return eggPrefab;
        if (animalType == AnimalType.Capybara) return capybaraPrefab;
        bool isRound = false;

        switch (shapeState)
        {
            case ShapeState.Circle:
                isRound = true;
                break;
            case ShapeState.Square:
                isRound = false;
                break;
            case ShapeState.Both:
                isRound = Random.value < 0.5f;
                break;
        }

        if(setShape)
        {
            isRound = toRound;
        }

        Animal[] sourceArray = isRound ? animalPrefabsRound : animalPrefabsSquare;
        foreach (var animal in sourceArray)
        {
            if (animal.GetAnimalType() == animalType)
            {
                animal.SetRound(isRound);
                return animal;
            }
        }
        return null;
    }

    public Animal SpawnAnimal(Animal prefab, Vector2 position)
    {
        Animal newAnimal = Instantiate(prefab, position, Quaternion.identity, animalsParent);
        Vector2 adjustedPos = AdjustSpawnPoint(position, newAnimal);
        newAnimal.transform.position = adjustedPos;
        newAnimal.SetRound(prefab.IsRound());

        if (newAnimal.GetComponent<Capybara>() != null)
        {
            AudioManager.instance.PlayChoirSound();
        }
        return newAnimal;
    }

    public bool TryToSpawnEgg(float eggSpawnChance, int eggLimit)
    {
        float chance = Random.value;
        if (chance > eggSpawnChance || CountEggs() >= eggLimit || GameOverManager.instance.closeToDeath) return false;
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

    public void ResetAnimalsParent()
    {
        foreach (Transform child in animalsParent)
        {
            Destroy(child.gameObject);
        }
    }

    public Egg GetEggPrefab() => eggPrefab;
    public Capybara GetCapybaraPrefab() => capybaraPrefab;
    
    
}