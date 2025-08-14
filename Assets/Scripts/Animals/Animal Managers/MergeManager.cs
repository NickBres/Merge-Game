using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class MergeManager : MonoBehaviour
{

    Animal lastSender;

    [Header(" Actions ")]
    public static Action<AnimalType, Vector2> onMergeAnimal;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Animal.onCollisionWithAnimal += CollisionBetweenAnimalsCallback;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CollisionBetweenAnimalsCallback(Animal sender)
    {
        if (lastSender != null)
            return;

        lastSender = sender;

        // Start the multi-animal merge process
        TryMultiMerge(sender);
    }

    private void TryMultiMerge(Animal startAnimal)
    {
        var mergeType = startAnimal.GetAnimalType();
        var toMerge = new System.Collections.Generic.List<Animal>();
        var toCheck = new System.Collections.Generic.Queue<Animal>();

        toMerge.Add(startAnimal);
        toCheck.Enqueue(startAnimal);

        while (toCheck.Count > 0)
        {
            var current = toCheck.Dequeue();
            foreach (var other in current.GetCurrentCollisions())
            {
                if (other == null || other == current || other.HasIce())
                    continue;

                if (other.GetAnimalType() == mergeType && !toMerge.Contains(other))
                {
                    toMerge.Add(other);
                    toCheck.Enqueue(other);
                }
            }
        }

        if (toMerge.Count >= 2)
        {
            Vector2 spawnPos = Vector2.zero;
            int count = 0;
            foreach (var animal in toMerge)
            {
                FindIced(animal);
                spawnPos += (Vector2)animal.transform.position;
                animal.Disappear();
                if (count > 1)
                    ScoreManager.instance.IncrementCombo();
                count++;
            }

            spawnPos /= toMerge.Count;
            int rawValue = (int)mergeType * toMerge.Count;
            int closestPowerOfTwo = Mathf.NextPowerOfTwo(rawValue);
            if (closestPowerOfTwo > rawValue) closestPowerOfTwo /= 2;
            closestPowerOfTwo = Mathf.Min(closestPowerOfTwo, 2048);
            AnimalType newType = (AnimalType)closestPowerOfTwo;
            SpawnMergedAnimal(newType, spawnPos);
            onMergeAnimal?.Invoke(newType, spawnPos);
        }

        StartCoroutine(ResetLastSender());
    }

    private void FindIced(Animal animal)
    {
        foreach (var other in animal.GetCurrentCollisions())
        {
            if (other != null && other != animal && other.isActiveAndEnabled)
            {
                if (other.HasIce())
                {
                    other.RemoveIce();
                }
            }
        }
    }

    IEnumerator ResetLastSender()
    {
        yield return new WaitForEndOfFrame();
        lastSender = null;
    }

    private void SpawnMergedAnimal(AnimalType type, Vector2 spawnPosition)
    {
        Animal newAnimal = GameplayController.instance.GetAnimalFromType(type);
        newAnimal = GameplayController.instance.SpawnAnimal(newAnimal, spawnPosition);
        newAnimal.EnablePhysics();
        ComboPopUp.instance.ShowCombo(spawnPosition, ScoreManager.instance.GetComboCount());
        ScoreManager.instance.IncrementCombo();

        if (ScoreManager.instance.isEpicCombo())
        {
            newAnimal.MakeExplosive();
        }

        if (UnityEngine.Random.value < GameplayController.instance.GetIceChance() && !GameOverManager.instance.closeToDeath)
        {
            newAnimal.ApplyIce();
        }
    }

    void OnDestroy()
    {
        Animal.onCollisionWithAnimal -= CollisionBetweenAnimalsCallback;
    }
}
