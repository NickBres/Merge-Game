using NUnit.Framework;
using TMPro;
using UnityEngine;

public class FruitManager : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Transform fruitsParent;
    [SerializeField] private Fruit[] fruitPrefabs;
    [SerializeField] private Fruit[] spawnableFruits;
    [SerializeField] private LineRenderer fruitSpawnLine;
    
    
    private Fruit currentFruit;
    private Fruit nextFruit;


    [Header(" Settings ")]
    [SerializeField] private Transform fruitYSpawnPoint;
    [SerializeField] private float spawnDelay = 0.5f;
    private bool canControl = false;
    private bool isControlling = false;

    [Header(" Debug ")]
    [SerializeField] private bool enableGizmos;

    void Awake()
    {
        MergeManager.onMergeFruit += MergeFruits;
    }

    void Start()
    {
        canControl = true;
        HideLine();
        nextFruit = spawnableFruits[Random.Range(0, spawnableFruits.Length)];
    }

    private void Update()
    {
        if (canControl)
            ManagePlayerInput();
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
        currentFruit = SpawnFruit(nextFruit, GetSpawnPosition());
        nextFruit = spawnableFruits[Random.Range(0, spawnableFruits.Length)];
    }

    public Fruit getNextFruit()
    {
        return nextFruit;
    }
    private void MouseDragCallback()
    {
        DisplayLine();
        currentFruit.MoveTo(GetSpawnPosition());
    }
    private void MouseUpCallback()
    {
        HideLine();
        currentFruit.EnablePhysics();

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

    private Fruit SpawnFruit(Fruit fruit, Vector2 position)
    {
        Fruit newFruit = Instantiate(fruit,
            position,
            Quaternion.identity,
            fruitsParent);
        isControlling = true;
        return newFruit;
    }

    private Vector2 GetClickedWorldPosition()
    {
        Vector2 screenPosition = Input.mousePosition;
        return Camera.main.ScreenToWorldPoint(screenPosition);
    }

    private Vector2 GetSpawnPosition()
    {
        Vector2 spawnPosition = GetClickedWorldPosition();
        spawnPosition.y = fruitYSpawnPoint.position.y;
        return spawnPosition;
    }

    private void HideLine()
    {
        fruitSpawnLine.enabled = false;
    }
    private void DisplayLine()
    {
        fruitSpawnLine.enabled = true;
        fruitSpawnLine.SetPosition(0, GetSpawnPosition());
        fruitSpawnLine.SetPosition(1, GetSpawnPosition() + Vector2.down * 15);
    }

    private void MergeFruits(FruitType type, Vector2 spawnPosition)
    {
        Debug.Log("Merging fruits");
        for (int i = 0; i < fruitPrefabs.Length; i++)
        {
            if (fruitPrefabs[i].GetFruitType() == type)
            {
                canControl = false;
                StartControlTimer();
                Fruit newFruit = SpawnFruit(fruitPrefabs[i], spawnPosition);
                newFruit.EnablePhysics();
                break;
            }
        }
        
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enableGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-50, fruitYSpawnPoint.position.y, 0), new Vector3(50, fruitYSpawnPoint.position.y, 0));
    }
#endif
}
