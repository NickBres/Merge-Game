using UnityEngine;
using System;
using System.Security.Cryptography.X509Certificates;

public class Animal : MonoBehaviour
{
    [Header(" Data ")]
    [SerializeField] private AnimalType type;

    [Header(" Actions ")]
    public static Action<Animal, Animal> onCollisionWithAnimal;

    private Rigidbody2D rigidBody;
    private Collider2D animalCollider;
    private SpriteRenderer spriteRenderer;

    private bool hasCollided = false;
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animalCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnablePhysics()
    {
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        animalCollider.enabled = true;
    }

    public void DisablePhysics()
    {
        rigidBody.bodyType = RigidbodyType2D.Kinematic;
        animalCollider.enabled = false;
    }

    public void MoveTo(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        hasCollided = true;

        if (collision.collider.TryGetComponent(out Animal otherFruit))
        {
            if (otherFruit.GetAnimalType() != type)
                return;
            onCollisionWithAnimal?.Invoke(this, otherFruit);
        }
    }

    public AnimalType GetAnimalType()
    {
        return type;
    }

    public Sprite GetSprite()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        return spriteRenderer?.sprite;
    }

    public bool HasCollided()
    {
        return hasCollided;
    }
}
