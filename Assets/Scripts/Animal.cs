using UnityEngine;
using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;

public class Animal : MonoBehaviour
{


    [Header(" Data ")]
    [SerializeField] private AnimalType type;
    private bool canBeMerged = false;
    private Vector2 storedVelocity;

    [Header(" Actions ")]
    public static Action<Animal, Animal> onCollisionWithAnimal;
    public event Action onCollision;

    private Rigidbody2D rigidBody;
    private Collider2D animalCollider;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer skinRenderer;

    [Header(" Effects ")]
    [SerializeField] private ParticleSystem mergeEffect;

    private bool hasCollided = false;
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        animalCollider = GetComponent<Collider2D>();
        spriteRenderer = transform.Find("Animal Renderer")?.GetComponent<SpriteRenderer>();
        mergeEffect = GetComponentInChildren<ParticleSystem>();
        skinRenderer = transform.Find("Skin/Skin Renderer")?.GetComponent<SpriteRenderer>();
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
        rigidBody.gravityScale = 1f;
        //animalCollider.enabled = true;
        rigidBody.freezeRotation = false;
    }

    public void DisablePhysics()
    {
        storedVelocity = rigidBody.linearVelocity;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.gravityScale = 0f;
        //animalCollider.enabled = false;
        rigidBody.freezeRotation = true;
    }

    public void MoveTo(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        onCollision?.Invoke();
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        hasCollided = true;

        if (!canBeMerged)
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
    
    public void SetAnimalType(AnimalType newType)
    {
        type = newType;
    }

    public Sprite GetSprite()
    {
        if (spriteRenderer == null)
        {
            var spriteTransform = transform.Find("Animal Renderer");
            spriteRenderer = spriteTransform?.GetComponent<SpriteRenderer>();
        }

        return spriteRenderer?.sprite;
    }

    public Sprite GetSkinSprite()
    {
        if (skinRenderer == null)
        {
            var skinTransform = transform.Find("Skin/Skin Renderer");
            skinRenderer = skinTransform?.GetComponent<SpriteRenderer>();
        }

        return skinRenderer?.sprite;
    }

    public bool HasCollided()
    {
        return hasCollided;
    }

    public bool CanMerge()
    {
        return canBeMerged;
    }

    public void Push(Vector2 force)
    {
        rigidBody.AddForce(force, ForceMode2D.Impulse);
    }

    public void MoveHorizontally(float horizontalInput)
    {
        this.transform.position += new Vector3(horizontalInput, 0f, 0f);
    }

    public void MoveVertically(float verticalInput)
    {
        this.transform.position += new Vector3(0f, verticalInput, 0f);
    }
}
