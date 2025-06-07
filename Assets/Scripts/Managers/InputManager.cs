using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class InputManager : MonoBehaviour
{
    public static InputManager instance;

    private InputSystem_Actions inputActions;

    public event Action<Vector2> OnTouchStart;
    public event Action<Vector2> OnTouchHold;
    public event Action<Vector2> OnTouchEnd;

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
        inputActions.Gameplay.TouchPress.performed += HandleTouchPressPerformed;
        inputActions.Gameplay.TouchPress.canceled += HandleTouchPressCanceled;
    }

    private void OnDisable()
    {
        inputActions.Gameplay.TouchPress.started -= HandleTouchPressStarted;
        inputActions.Gameplay.TouchPress.performed -= HandleTouchPressPerformed;
        inputActions.Gameplay.TouchPress.canceled -= HandleTouchPressCanceled;

        inputActions.Disable();
    }

    private void Update()
    {
        HorizontalInput = inputActions.Gameplay.MoveHorizontal.ReadValue<float>();
    }

    private Vector2 GetTouchPosition()
    {
        return inputActions.Gameplay.TouchPosition.ReadValue<Vector2>();
    }

    private void HandleTouchPressStarted(InputAction.CallbackContext ctx)
    {
        OnTouchStart?.Invoke(GetTouchPosition());
    }

    private void HandleTouchPressPerformed(InputAction.CallbackContext ctx)
    {
        OnTouchHold?.Invoke(GetTouchPosition());
    }

    private void HandleTouchPressCanceled(InputAction.CallbackContext ctx)
    {
        OnTouchEnd?.Invoke(GetTouchPosition());
    }
}