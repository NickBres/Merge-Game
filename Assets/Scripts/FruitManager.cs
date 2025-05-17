using System;
using Unity.Mathematics;
using UnityEditor.Scripting;
using UnityEngine;

public class FruitManager : MonoBehaviour
{
    [Header(" Elements ")]
    [SerializeField] private GameObject fruitPrefab;
    [SerializeField] private LineRenderer fruitSpawnLine;


    [Header(" Settings ")]
    [SerializeField] private Transform fruitYSpawnPoint;

    [Header(" Debug ")]
    [SerializeField] private bool enableGizmos;

    void Start()
    {
        HideLine();
    }

    private void Update()
    {
        ManagePlayerInput();
    }

    private void ManagePlayerInput()
    {

        if (Input.GetMouseButtonDown(0))
        {
            MouseDawnCallback();
        }
        else if (Input.GetMouseButton(0))
        {
            MouseDragCallback();
        }
        else if (Input.GetMouseButtonUp(0))
        {
            MouseUpCallback();
        }

        
    }

    private void MouseDawnCallback()
    {
        
    }
    private void MouseDragCallback()
    {
        DisplayLine();
    }
    private void MouseUpCallback()
    {
        HideLine();
        SpawnFruit(GetClickedWorldPosition());
    }

    private void SpawnFruit(Vector2 position)
    {
        Instantiate(fruitPrefab, GetSpawnPosition(), Quaternion.identity);
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
