using UnityEngine;
using UnityEngine.UI;

public class ShapeToggle : MonoBehaviour
{

    [SerializeField] private Sprite squareIcon;
    [SerializeField] private Sprite circleIcon;
    [SerializeField] private Sprite squareAndCircleIcon;
 
    private ShapeState currentState = ShapeState.Square;

    private Image imageRenderer;

    public static System.Action<int> OnShapeChanged;
    private const string SHAPE_KEY = "shape_index";

    void Awake()
    {
        imageRenderer = transform.Find("Background/Icon").GetComponent<Image>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        currentState = (ShapeState)PlayerPrefs.GetInt(SHAPE_KEY, 0);
        UpdateVisual();
        OnShapeChanged?.Invoke((int)currentState);
    }

    public void OnToggleClick()
    {
        currentState = (ShapeState)(((int)currentState + 1) % 3);
        PlayerPrefs.SetInt(SHAPE_KEY, (int)currentState);
        PlayerPrefs.Save();
        UpdateVisual();

        AudioManager.instance.PlayClickSound();
        VibrationManager.instance.Vibrate(VibrationType.Light);

        OnShapeChanged?.Invoke((int)currentState);
    }

    private void UpdateVisual()
    {
        switch (currentState)
        {
            case ShapeState.Square:
                imageRenderer.sprite = squareIcon;
                break;
            case ShapeState.Circle:
                imageRenderer.sprite = circleIcon;
                break;
            case ShapeState.Both:
                imageRenderer.sprite = squareAndCircleIcon;
                break;
        }
    }
}
