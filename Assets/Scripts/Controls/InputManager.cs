using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    private InputSystem_Actions inputActions;

    public event Action<Vector3> OnTouchStart;
    public event Action<Vector3> OnTouchHold;
    public event Action<Vector3> OnTouchEnd;

    public float HorizontalInput { get; private set; }

    public InputSystem_Actions InputActions => inputActions;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            inputActions = new InputSystem_Actions();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        inputActions.Enable();

        inputActions.Gameplay.TouchPress.started += HandleTouchPressStarted;
        inputActions.Gameplay.TouchPress.canceled += HandleTouchPressCanceled;
    }

    private void OnDisable()
    {
        inputActions.Gameplay.TouchPress.started -= HandleTouchPressStarted;
        inputActions.Gameplay.TouchPress.canceled -= HandleTouchPressCanceled;

        inputActions.Disable();
    }

    private void Update()
    {
        HorizontalInput = inputActions.Gameplay.MoveHorizontal.ReadValue<float>();

        if (inputActions.Gameplay.TouchPress.IsPressed())
        {
            OnTouchHold?.Invoke(GetWorldTouchPosition());
        }
    }

    private Vector2 GetTouchPosition()
    {
        return inputActions.Gameplay.TouchPosition.ReadValue<Vector2>();
    }

    private Vector3 GetWorldTouchPosition()
    {
        Vector3 screenPos = inputActions.Gameplay.TouchPosition.ReadValue<Vector2>();
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
        worldPos.z = 0;
        return worldPos;
    }

    private void HandleTouchPressStarted(InputAction.CallbackContext ctx)
    {
        OnTouchStart?.Invoke(GetWorldTouchPosition());
    }

    private void HandleTouchPressCanceled(InputAction.CallbackContext ctx)
    {
        OnTouchEnd?.Invoke(GetWorldTouchPosition());
    }
}