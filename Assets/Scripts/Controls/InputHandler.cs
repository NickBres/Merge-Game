using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class InputHandler
{
    private static GameplayController gameplay;
    private static GraphicRaycaster raycaster;
    private static PointerEventData pointerEventData;

    private static float offsetY = 4f;

    public static void Initialize(GameplayController controller, Canvas uiCanvas)
    {
        gameplay = controller;
        raycaster = uiCanvas.GetComponent<GraphicRaycaster>();

        InputManager.instance.OnTouchStart += HandleTouchStart;
        InputManager.instance.OnTouchEnd += HandleTouchEnd;
        InputManager.instance.OnTouchHold += HandleTouchHold;
    }

    public static void Cleanup()
    {
        InputManager.instance.OnTouchStart -= HandleTouchStart;
        InputManager.instance.OnTouchEnd -= HandleTouchEnd;
        InputManager.instance.OnTouchHold -= HandleTouchHold;
    }

private static void HandleTouchStart(Vector3 screenPos)
{
    if (!GameManager.instance.IsGameState() || IsTouchOverUI(screenPos) || !gameplay.CanControl())
        return;

    List<Animal> mockups = gameplay.GetMockupAnimals();
    Camera cam = Camera.main;

    for (int i = 0; i < mockups.Count; i++)
    {
        Animal animal = mockups[i];
        if (animal == null) continue;

        Collider2D col = animal.GetComponent<Collider2D>();
        if (col == null) continue;

        if (col.OverlapPoint(screenPos))
            {
                gameplay.HandleMockupTouch(i);
                return;
            }
    }

    Debug.Log($"Touch at {screenPos} - No mockup hit");
}

    private static void HandleTouchEnd(Vector3 screenPos)
    {
        if (!GameManager.instance.IsGameState() || !gameplay.CanControl())
            return;

        gameplay.ReleaseCurrentAnimal();
    }

    private static void HandleTouchHold(Vector3 screenPos)
    {
        if (!GameManager.instance.IsGameState() || !gameplay.CanControl())
            return;

        var currentAnimal = gameplay.GetCurrentAnimal();
        if (currentAnimal == null) return;

        Vector3 newPos = screenPos;

        float halfWidth = 0.5f;
        var collider = currentAnimal.GetComponent<Collider2D>();
        if (collider != null)
        {
            halfWidth = collider.bounds.extents.x + 0.2f;
        }

        float minX = gameplay.MinX + halfWidth;
        float maxX = gameplay.MaxX - halfWidth;
        
        newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
        newPos.y += offsetY;
        newPos.z = currentAnimal.transform.position.z;
        currentAnimal.SetPosition(newPos);
        currentAnimal.SetSortingGroup(100); // Ensure the animal is above other  elements
    }

    private static bool IsTouchOverUI(Vector3 worldPos)
    {
        pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Camera.main.WorldToScreenPoint(worldPos);

        List<RaycastResult> results = new List<RaycastResult>();
        raycaster.Raycast(pointerEventData, results);

        foreach (var result in results)
        {
            if (!result.gameObject.CompareTag("IgnoreRaycast"))
                return true;
        }
        return false;
    }
}