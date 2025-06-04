using UnityEngine;

public class WallManager : MonoBehaviour
{
    [Header("Wall Transforms")]
    [SerializeField] private Transform leftWall;
    [SerializeField] private Transform rightWall;
    [SerializeField] private Transform floor;
    [SerializeField] private Transform ceiling;

    [Header("Wall Thickness")]
    [SerializeField] private float wallThickness = 1f;

    [Header("Visual Offsets")]
    [SerializeField] private float wallVisibleOffset = -0.5f;
    [SerializeField] private float floorRaiseOffset = 1.5f;

    void Start()
    {
        Camera cam = Camera.main;
        float z = 0f;

        Vector3 bottomLeft = cam.ViewportToWorldPoint(new Vector3(0, 0, cam.nearClipPlane));
        Vector3 topRight = cam.ViewportToWorldPoint(new Vector3(1, 1, cam.nearClipPlane));

        float screenWidth = topRight.x - bottomLeft.x;
        float screenHeight = topRight.y - bottomLeft.y;

        if (leftWall != null)
            leftWall.position = new Vector3(bottomLeft.x - wallThickness * wallVisibleOffset, 0, z);

        if (rightWall != null)
            rightWall.position = new Vector3(topRight.x + wallThickness * wallVisibleOffset, 0, z);

        if (floor != null)
            floor.position = new Vector3(0, bottomLeft.y + wallThickness * floorRaiseOffset, z);

        if (ceiling != null)
            ceiling.position = new Vector3(0, topRight.y + wallThickness / 2f, z);

        if (leftWall != null)
            leftWall.localScale = new Vector3(wallThickness, screenHeight * 2f, 1);
        if (rightWall != null)
            rightWall.localScale = new Vector3(wallThickness, screenHeight * 2f, 1);
        if (floor != null)
            floor.localScale = new Vector3(screenWidth * 2f, wallThickness, 1);
        if (ceiling != null)
            ceiling.localScale = new Vector3(screenWidth * 2f, wallThickness, 1);
    }
}
