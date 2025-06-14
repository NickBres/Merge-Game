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
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer skinRenderer;

    [Header(" Effects ")]
    [SerializeField] private ParticleSystem mergeEffect;

    protected bool hasCollided = false;
    private bool isFrozen = false;
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
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
    public void Unfreeze()
    {
        isFrozen = false;
    }

    public void EnablePhysics()
    {
        rigidBody.bodyType = RigidbodyType2D.Dynamic;
        rigidBody.linearVelocity = storedVelocity;
        rigidBody.gravityScale = 1f;
        //animalCollider.enabled = true;
        rigidBody.freezeRotation = false;
        isFrozen = false;
    }

    public void DisablePhysics(bool disableMovement, bool disableRB = true)
    {
        if(disableRB) rigidBody.bodyType = RigidbodyType2D.Kinematic;
        storedVelocity = rigidBody.linearVelocity;
        rigidBody.linearVelocity = Vector2.zero;
        rigidBody.gravityScale = 0f;
        //animalCollider.enabled = false;
        rigidBody.freezeRotation = true;
        isFrozen = disableMovement;
    }

    public void MoveTo(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall")) return;
        onCollision?.Invoke();
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall")) return;
        hasCollided = true;

        if (!canBeMerged || isFrozen)
            return;

        if (collision.collider.TryGetComponent(out Animal otherFruit))
        {
            if (otherFruit.GetAnimalType() != type || !otherFruit.CanMerge())
                return;

            onCollisionWithAnimal?.Invoke(this, otherFruit);
        }
    }

    public void Disappear()
    {
        if (mergeEffect != null)
        {
            mergeEffect.transform.SetParent(null);
            mergeEffect.Play();
            VibrationManager.instance.Vibrate(VibrationType.Medium);
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
        if(spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        return spriteRenderer.sprite;
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

    public virtual bool CanMerge()
    {
        return canBeMerged;
    }

    public void Push(Vector2 force)
    {
        if (isFrozen)
            return;
        rigidBody.AddForce(force, ForceMode2D.Impulse);
    }

    public void MoveHorizontally(float horizontalInput)
    {
        if (isFrozen)
            return;
        this.transform.position += new Vector3(horizontalInput, 0f, 0f);
    }

    public void MoveVertically(float verticalInput)
    {
        if (isFrozen)
            return;
        this.transform.position += new Vector3(0f, verticalInput, 0f);
    }

    public void SetPosition(Vector3 newPosition)
    {
        if (isFrozen)
            return;
        this.transform.position = newPosition;
    }
    
}
