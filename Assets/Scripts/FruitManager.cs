using System;
using Unity.Mathematics;
using UnityEditor.Scripting;
using UnityEngine;

public class FruitManager : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private Fruit fruitPrefab;
    [SerializeField] private LineRenderer fruitSpawnLine;
    private Fruit currentFruit;


    [Header(" Settings ")]
    [SerializeField] private Transform fruitYSpawnPoint;
    private bool canControl = false;
    private bool isControlling = false;

    [Header(" Debug ")]
    [SerializeField] private bool enableGizmos;

    void Start()
    {
        canControl = true;
        HideLine();
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
        SpawnFruit(GetSpawnPosition());
        isControlling = true;
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
        Invoke("StopControlTimer", 1);
    }

    private void StopControlTimer()
    {
        canControl = true;
    }

    private void SpawnFruit(Vector2 position)
    {
        currentFruit = Instantiate(fruitPrefab, GetSpawnPosition(), Quaternion.identity);
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

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!enableGizmos) return;
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(-50, fruitYSpawnPoint.position.y, 0), new Vector3(50, fruitYSpawnPoint.position.y, 0));
    }
#endif
}
