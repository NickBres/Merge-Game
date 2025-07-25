using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{
    public static BackgroundManager instance;
    [SerializeField] private GameObject background;
    [SerializeField] private List<Sprite> backgounds;
    [SerializeField] private float parallaxStrength = 1f;
    private SpriteRenderer currentBackground;
    private Vector3 initialPosition;
    private Vector3 currentOffset;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        currentBackground = background.GetComponent<SpriteRenderer>();
        Reset();
    }

    void Start()
    {
        initialPosition = background.transform.position;
    }

    public void Reset()
    {
        currentBackground.sprite = backgounds[Random.Range(0, backgounds.Count)];
    }

    void Update()
    {
        Vector3 tilt = Input.acceleration;
        Vector3 targetOffset = new Vector3(tilt.x, tilt.y, 0) * parallaxStrength;
        currentOffset = Vector3.Lerp(currentOffset, targetOffset, Time.deltaTime * 5f); // smoothing factor
        background.transform.position = initialPosition + currentOffset;
    }
}
