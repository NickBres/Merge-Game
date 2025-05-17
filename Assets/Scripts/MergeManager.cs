using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class MergeManager : MonoBehaviour
{

    Fruit lastSender;

    [Header(" Actions ")]
    public static Action<FruitType, Vector2> onMergeFruit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Fruit.onCollisionWithFruit += CollisionBetweenFruitsCallback;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void CollisionBetweenFruitsCallback(Fruit sender, Fruit otherFruit)
    {
        if (lastSender != null)
            return;

        lastSender = sender;
        ProcessMerge(sender, otherFruit);


    }

    private void ProcessMerge(Fruit sender, Fruit otherFruit)
    {
        FruitType mergeFruitType = (FruitType)((int)sender.GetFruitType() + (int)otherFruit.GetFruitType());

        Vector2 fruiSpawnPosition = (sender.transform.position + otherFruit.transform.position) / 2;

        Destroy(sender.gameObject);
        Destroy(otherFruit.gameObject);

        onMergeFruit?.Invoke(mergeFruitType, fruiSpawnPosition);

        StartCoroutine(ResetLastSender());
    }

    IEnumerator ResetLastSender()
    {
        yield return new WaitForEndOfFrame();
        lastSender = null;
    }
}
