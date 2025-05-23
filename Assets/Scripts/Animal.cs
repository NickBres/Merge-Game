using UnityEngine;
using System;
using System.Security.Cryptography.X509Certificates;

public class Animal : MonoBehaviour
{
    [Header(" Data ")]
    [SerializeField] private AnimalType type;
    private bool canBeMerged = false;
    private Vector2 storedVelocity;

    [Header(" Actions ")]
    public static Action<Animal, Animal> onCollisionWithAnimal;

    private Rigidbody2D rigidBody;
    private Collider2D animalCollider;
    private SpriteRenderer spriteRenderer;

    [Header(" Effects ")]
    [SerializeField] private ParticleSystem mergeEffect;

    private bool hasCollided = false;
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animalCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        mergeEffect = GetComponentInChildren<ParticleSystem>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Invoke("AllowMerge", 0.3f);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void AllowMerge()
    {
        canBeMerged = true;
    }

    public void EnablePhysics()
    {
        rigidBody.linearVelocity = storedVelocity;
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        animalCollider.enabled = true;
        rigidBody.freezeRotation = false;
    }

    public void DisablePhysics()
    {
        storedVelocity = rigidBody.linearVelocity;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.bodyType = RigidbodyType2D.Kinematic;
        animalCollider.enabled = false;
        rigidBody.freezeRotation = true;
    }

    public void MoveTo(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        hasCollided = true;
        
        if(!canBeMerged)
            return;

        if (collision.collider.TryGetComponent(out Animal otherFruit))
            {
                if (otherFruit.GetAnimalType() != type || !otherFruit.CanMerge())
                    return;

                onCollisionWithAnimal?.Invoke(this, otherFruit);
            }
    }

    public void Merge()
    {
        if (mergeEffect != null)
        {
            mergeEffect.transform.SetParent(null);
            mergeEffect.Play();
        }

        Destroy(gameObject);
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

    public bool CanMerge()
    {
        return canBeMerged;
    }
}
