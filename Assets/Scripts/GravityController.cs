using UnityEngine;

public class GravityController : MonoBehaviour
{
    public static GravityController instance;

    [Header("Tilt Gravity Settings")]
    public bool enableTiltGravity = true;
    [Range(0f, 10f)] public float sensitivity = 5f;
    public float verticalGravity = -9.8f;
    public float maxTilt = 1.0f; // to clamp the max horizontal influence

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        OptionsManager.OnTiltChanged += ToggleTilt;
    }

    private void Update()
    {
        if (!enableTiltGravity) return;

        Vector3 tilt = Input.acceleration;

        float clampedX = Mathf.Clamp(tilt.x, -maxTilt, maxTilt);
        float gravityX = clampedX * sensitivity;
        Physics2D.gravity = new Vector2(gravityX, verticalGravity);
    }

    private void ToggleTilt(bool isEnabled)
    {
        enableTiltGravity = isEnabled;
        Physics2D.gravity = new Vector2(0f, verticalGravity);
    }

    private void OnDestroy()
    {
        OptionsManager.OnMusicChanged -= ToggleTilt;
    }
}
