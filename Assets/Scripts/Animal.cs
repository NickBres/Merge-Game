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
    protected bool canBeMerged = false;
    private Vector2 storedVelocity;

    [Header(" Bomb ")]
    private SpriteRenderer crackRenderer;
    protected bool isExplosive = false;

    [Header(" Effects ")]
    [SerializeField] private ParticleSystem mergeEffect;
    private GameObject iceCube;

    [Header(" Actions ")]
    public static Action<Animal> onCollisionWithAnimal;
    public event Action onCollision;

    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer skinRenderer;

    protected bool hasCollided = false;
    protected bool isIced = false;
    protected bool isFrozen = false;
    private void Awake()
    {
        rigidBody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mergeEffect = GetComponentInChildren<ParticleSystem>();
        skinRenderer = transform.Find("Skin/Skin Renderer")?.GetComponent<SpriteRenderer>();
        crackRenderer = transform.Find("Crack")?.GetComponent<SpriteRenderer>();
        iceCube = transform.Find("IceCube")?.gameObject;
        if (crackRenderer != null)
            crackRenderer.enabled = false;
        // iceCube assignment retained, do not modify here
        if (iceCube != null)
            iceCube.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        FlashCrack();
    }

    protected void AllowMerge()
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
    public void ApplyIce()
    {
        isIced = true;
        if (iceCube != null)
        {
            iceCube.SetActive(true);
            AudioManager.instance.PlayIcedSound();
        }
    }

    public void RemoveIce()
    {
        isIced = false;
        if (iceCube != null)
        {
            iceCube.SetActive(false);
            AudioManager.instance.PlayIceBreakSound();
        }

    }

    public bool HasIce()
    {
        return isIced;
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

        Animal other = collision.collider.GetComponent<Animal>() ?? collision.collider.GetComponentInParent<Animal>();
        if (other != null && other != this)
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
        if (collision.collider.CompareTag("Wall") || HasIce()) return;
        hasCollided = true;


        AllowMerge();

        if (!canBeMerged || isFrozen)
            return;

        if (collision.collider.TryGetComponent(out Animal otherAnimal))
        {
            if (otherAnimal.GetAnimalType() != type || !otherAnimal.CanMerge())
                return;

            onCollisionWithAnimal?.Invoke(this);
        }


        //ResolveSpawnOverlaps();
    }

    public void Disappear()
    {
        if (isExplosive)
        {
            Explode(1.5f, 3f, 5f);
            return;
        }
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
    private void FlashCrack()
    {
        if (!isExplosive || crackRenderer == null)
            return;

        float t = Mathf.PingPong(Time.time, 1f);
        Color c = Color.Lerp(Color.yellow, new Color(1f, 0.5f, 0f), t); // yellow ↔ orange
        crackRenderer.color = c;
    }

    public void MakeExplosive()
    {
        isExplosive = true;
        if (crackRenderer != null)
            crackRenderer.enabled = true;
    }

    protected void Explode(float killRadius, float pushRadius, float force = 5f)
    {
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, pushRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Animal other) && other != this)
            {
                float distance = Vector2.Distance(transform.position, other.transform.position);

                // Destroy animals with lower type in kill radius
                if (distance <= killRadius)
                {
                    if (other.HasIce())
                    {
                        other.RemoveIce();
                    }
                    else if (other.GetAnimalType() < this.GetAnimalType())
                    {
                        other.Disappear();
                        ScoreManager.instance.UpdateScore(other.GetAnimalType(), transform.position);
                    }
                    
                }
                else
                {
                    // Push animals within push radius
                    Vector2 pushDirection = (other.transform.position - transform.position).normalized;
                    float strength = Mathf.Lerp(force, 0f, distance / pushRadius);
                    other.Push(pushDirection * strength);
                }
            }
        }

        // Optional: play effect and destroy self
        if (mergeEffect != null)
        {
            mergeEffect.transform.SetParent(null);
            mergeEffect.Play();
        }

        VibrationManager.instance.Vibrate(VibrationType.Heavy);
        AudioManager.instance.PlayExplosionSound(transform.position);
        Destroy(gameObject);
    }



}
