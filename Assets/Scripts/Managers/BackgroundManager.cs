using System.Collections.Generic;
using UnityEngine;

public class BackgroundManager : MonoBehaviour
{   
    [SerializeField] private GameObject background;
    [SerializeField] private List<Sprite> backgounds;
    private SpriteRenderer currentBackground;

    void Awake()
    {
        currentBackground = background.GetComponent<SpriteRenderer>();
        Reset();
    }

    private void Reset()
    {
        currentBackground.sprite = backgounds[Random.Range(0, backgounds.Count)];
    } 
}
