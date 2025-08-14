using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimalsParent : MonoBehaviour
{

    public static AnimalsParent instance;

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
    public List<AnimalData> GetAnimalsData()
    {
        List<AnimalData> animalsData = new List<AnimalData>();
        foreach (Transform child in transform)
        {
            Animal animal = child.GetComponent<Animal>();
            if (animal != null)
            {
                AnimalData data = new AnimalData
                {
                    animalType = animal.GetAnimalType(),
                    position = animal.transform.position,
                    rotationZ = animal.transform.rotation.eulerAngles.z,
                    isRound = animal.IsRound(),
                    isIced = animal.HasIce(),
                    isExplosive = animal.CanExplode()
                };
                animalsData.Add(data);
            }
        }
        return animalsData;
    }

    public void LoadAnimalsData(List<AnimalData> animalsData, AnimalSpawner animalSpawner)
    {
        foreach (AnimalData data in animalsData)
        {
            Animal prefab = animalSpawner.GetAnimalFromType(data.animalType, data.isRound);
            if (prefab != null)
            {
                Animal spawnedAnimal = animalSpawner.SpawnAnimal(prefab, data.position);
                spawnedAnimal.DisablePhysics(true, true);
                spawnedAnimal.transform.position = data.position;
                spawnedAnimal.transform.rotation = Quaternion.Euler(0, 0, data.rotationZ);
                if (data.isIced) spawnedAnimal.ApplyIce();
                if (data.isExplosive) spawnedAnimal.MakeExplosive();
            }
        }
        GameOverManager.instance.SetCanLoose(false);
    }

    public bool HasAnimalsUpTo(AnimalType upTo)
    {
        foreach (Transform child in transform)
        {
            Animal animal = child.GetComponent<Animal>();
            if (animal != null && animal != GameplayController.instance.GetCurrentAnimal() && animal.GetAnimalType() < upTo)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveAnimalsUpTo(AnimalType upTo, bool addToScore)
    {
        StartCoroutine(RemoveAllSmallAnimalsCoroutine(upTo, addToScore));
    }

    private IEnumerator RemoveAllSmallAnimalsCoroutine(AnimalType upTo, bool addToScore)
    {
        FreezeAnimals();

        while (true)
        {
            bool found = false;
            foreach (Transform child in transform)
            {
                Animal animal = child.GetComponent<Animal>();
                if (animal != null && animal != GameplayController.instance.GetCurrentAnimal() && animal.GetAnimalType() < upTo)
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

    public void FreezeAnimals()
    {
        foreach (Transform child in transform)
        {
            Animal animal = child.GetComponent<Animal>();
            animal.DisablePhysics(true);
        }
        GameplayController.instance.SetCanControl(false);
        GameOverManager.instance.SetCanLoose(false);
    }

    public void UnfreezeAnimals()
    {
        foreach (Transform child in transform)
        {
            Animal animal = child.GetComponent<Animal>();
            animal.EnablePhysics();
        }
        GameplayController.instance.SetCanControl(true);
        GameOverManager.instance.SetCanLoose(true);
    }
}
