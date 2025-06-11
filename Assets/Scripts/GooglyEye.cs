using UnityEngine;

public class GooglyEye : MonoBehaviour
{
    [Header(" Sprites ")]
    [SerializeField] private Sprite middle;
    [SerializeField] private Sprite leftTop;
    [SerializeField] private Sprite leftBottom;
    [SerializeField] private Sprite rightTop;
    [SerializeField] private Sprite rightBottom;

    private SpriteRenderer spriteRenderer;

    private Transform target => GameplayController.instance.currentAnimal?.transform;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (target == null)
        {
            spriteRenderer.sprite = middle;
            return;
        }


        Vector2 direction = (target.position - transform.position).normalized;

        if (direction.y > 0.5f)
            spriteRenderer.sprite = direction.x < 0 ? leftTop : rightTop;
        else if (direction.y < -0.5f)
            spriteRenderer.sprite = direction.x < 0 ? leftBottom : rightBottom;
        else
            spriteRenderer.sprite = middle;
    }
}
