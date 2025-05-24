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

        inputActions.Gameplay.TouchPress.started += ctx => OnTouchStart?.Invoke(GetTouchPosition());
        inputActions.Gameplay.TouchPress.performed += ctx => OnTouchHold?.Invoke(GetTouchPosition());
        inputActions.Gameplay.TouchPress.canceled += ctx => OnTouchEnd?.Invoke(GetTouchPosition());
    }

    private void OnDisable()
    {
        inputActions.Gameplay.TouchPress.started -= ctx => OnTouchStart?.Invoke(GetTouchPosition());
        inputActions.Gameplay.TouchPress.performed -= ctx => OnTouchHold?.Invoke(GetTouchPosition());
        inputActions.Gameplay.TouchPress.canceled -= ctx => OnTouchEnd?.Invoke(GetTouchPosition());

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
}