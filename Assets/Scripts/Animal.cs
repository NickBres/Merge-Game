using UnityEngine;
using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;
using System.Collections.Generic;

public class Animal : MonoBehaviour
{
    private readonly HashSet<Animal> currentCollisions = new HashSet<Animal>();
    public List<Animal> GetCurrentCollisions() => new List<Animal>(currentCollisions);


    [Header(" Data ")]
    [SerializeField] private AnimalType type;
    private bool canBeMerged = false;
    private Vector2 storedVelocity;

    [Header(" Actions ")]
    public static Action<Animal> onCollisionWithAnimal;
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
        ResolveSpawnOverlaps();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void AllowMerge()
    {
        StartCoroutine(WaitForMergeCondition());
    }

    private System.Collections.IEnumerator WaitForMergeCondition()
    {
        float timer = 3f;
        while (timer > 0f)
        {
            if (rigidBody.linearVelocity.magnitude < 0.05f)
                break;

            timer -= Time.deltaTime;
            yield return null;
        }

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
        if (disableRB) rigidBody.bodyType = RigidbodyType2D.Kinematic;
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

        if (collision.collider.TryGetComponent(out Animal other) && other != this)
        {
            currentCollisions.Add(other);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out Animal other) && other != this)
        {
            currentCollisions.Remove(other);
        }
    }

    protected virtual void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall")) return;
        hasCollided = true;


        AllowMerge();

        if (!canBeMerged || isFrozen)
            return;

        if (collision.collider.TryGetComponent(out Animal otherFruit))
        {
            if (otherFruit.GetAnimalType() != type || !otherFruit.CanMerge())
                return;

            onCollisionWithAnimal?.Invoke(this);
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
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
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


    private void ResolveSpawnOverlaps()
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Animal"));
        filter.useTriggers = false;

        Collider2D[] results = new Collider2D[10];
        List<Rigidbody2D> affectedRigidbodies = new List<Rigidbody2D>();

        int count = Physics2D.OverlapCollider(myCollider, filter, results);
        for (int i = 0; i < count; i++)
        {
            if (results[i] != null && results[i].attachedRigidbody != null)
            {
                var rb = results[i].attachedRigidbody;
                rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
                affectedRigidbodies.Add(rb);
            }
        }

        rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        StartCoroutine(WaitAndResetCollisionMode(affectedRigidbodies));
    }

    private System.Collections.IEnumerator WaitAndResetCollisionMode(List<Rigidbody2D> affectedRigidbodies)
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(LayerMask.GetMask("Animal"));
        filter.useTriggers = false;

        Collider2D[] results = new Collider2D[10];

        while (true)
        {
            int count = Physics2D.OverlapCollider(myCollider, filter, results);
            if (count == 0)
                break;
            yield return null;
        }

        rigidBody.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        foreach (var rb in affectedRigidbodies)
        {
            if (rb != null)
                rb.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        }
    }


}
