using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{   
    [SerializeField] private GameObject background;
    [SerializeField] private List<Sprite> backgounds;
    [SerializeField] private float parallaxStrength = 0.5f;
    private SpriteRenderer currentBackground;
    private Vector3 initialPosition;

    void Awake()
    {
        currentBackground = background.GetComponent<SpriteRenderer>();
        Reset();
    }

    void Start()
    {
        initialPosition = background.transform.position;
    }

    private void Reset()
    {
        currentBackground.sprite = backgounds[Random.Range(0, backgounds.Count)];
    }

    void Update()
    {
        Vector3 tilt = Input.acceleration;
        background.transform.position = initialPosition + new Vector3(tilt.x, tilt.y, 0) * parallaxStrength;
    }
}
