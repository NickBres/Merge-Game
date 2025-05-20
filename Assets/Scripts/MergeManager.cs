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
        Animal.onCollisionWithAnimal += CollisionBetweenFruitsCallback;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CollisionBetweenFruitsCallback(Animal sender, Animal otherFruit)
    {
        if (lastSender != null)
            return;

        lastSender = sender;
        ProcessMerge(sender, otherFruit);


    }

    private void ProcessMerge(Animal sender, Animal otherAnimal)
    {
        AnimalType mergeAnimalType = (AnimalType)((int)sender.GetAnimalType() + (int)otherAnimal.GetAnimalType());

        Vector2 animalSpawnPosition = (sender.transform.position + otherAnimal.transform.position) / 2;

        Destroy(sender.gameObject);
        Destroy(otherAnimal.gameObject);

        onMergeAnimal?.Invoke(mergeAnimalType, animalSpawnPosition);

        StartCoroutine(ResetLastSender());
    }

    IEnumerator ResetLastSender()
    {
        yield return new WaitForEndOfFrame();
        lastSender = null;
    }

    void OnDestroy()
    {
        Animal.onCollisionWithAnimal -= CollisionBetweenFruitsCallback;
    }
}
