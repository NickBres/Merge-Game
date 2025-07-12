using UnityEngine;
using System;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.UI;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.Video;

public class Animal : MonoBehaviour
{
    protected readonly List<Animal> animalsMarkedForExplosion = new List<Animal>();
    private readonly HashSet<Animal> currentCollisions = new HashSet<Animal>();
    public List<Animal> GetCurrentCollisions() => new List<Animal>(currentCollisions);

    protected bool exploded = false;

    [Header(" Data ")]
    [SerializeField] private AnimalType type;
    [SerializeField] protected float explosionForce = 15f;
    protected bool canBeMerged = false;
    private Vector2 storedVelocity;

    [Header(" Bomb ")]
    private SpriteRenderer crackRenderer;
    protected bool isExplosive = false;

    [Header(" Effects ")]
    protected ParticleSystem mergeEffect;
    private GameObject iceCube;

    [Header(" Explosion Preview ")]
    private bool markedForExplosion = false;

    [Header(" Actions ")]
    public static Action<Animal> onCollisionWithAnimal;
    public event Action onCollision;

    private Rigidbody2D rigidBody;
    private SpriteRenderer spriteRenderer;
    private SpriteRenderer skinRenderer;

    protected bool hasCollided = false;
    protected bool isIced = false;
    protected bool isFrozen = false;
    protected bool isRound;
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
        if(!exploded)
            PreviewExplosionRadius(CalculateKillRadius());
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

        StartCoroutine(DelayedRemoveIce());
    }

    public void RemoveIceImmediate()
    {
        isIced = false;
        if (iceCube != null)
        {
            iceCube.SetActive(false);
            AudioManager.instance.PlayIceBreakSound();
        }
    }

    private System.Collections.IEnumerator DelayedRemoveIce()
    {
        if (iceCube != null)
        {
            Transform crack = iceCube.transform.Find("Ice Crack");
            if (crack != null)
            {
                var sr = crack.GetComponent<SpriteRenderer>();
                if (sr != null) sr.enabled = true;
                AudioManager.instance.PlayIceBreakSound();
            }
        }

        yield return new WaitForSeconds(Random.Range(1f, 1.5f));

        RemoveIceImmediate();
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

        Animal other = collision.collider.GetComponent<Animal>() ?? collision.collider.GetComponentInParent<Animal>();
        if (other != null && other != this && !currentCollisions.Contains(other))
        {
            currentCollisions.Add(other);
        }

        if (!canBeMerged || isFrozen)
            return;

        if (other != null)
        {
            if (other.GetAnimalType() != type || !other.CanMerge())
                return;

            onCollisionWithAnimal?.Invoke(this);
        }
    }

    public virtual void Disappear()
    {

        if (isExplosive)
        {
            float killRadius = CalculateKillRadius();
            Explode(killRadius, killRadius * 2.5f, explosionForce);
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

    protected virtual float CalculateKillRadius()
    {
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            return collider.bounds.extents.magnitude * 1.3f;
        }
        return 1f;
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

    public bool CanExplode()
    {
        return isExplosive && !exploded;
    }

    public bool IsRound()
    {
        return isRound;
    }

    public void SetRound(bool value)
    {
        isRound = value;
    }

    protected void Explode(float killRadius, float pushRadius, float force = 5f)
    {
        exploded = true;

        // Step 1: Mark animals
        PreviewExplosionRadius(killRadius);

        // Step 2: Handle eggs first
        for (int i = animalsMarkedForExplosion.Count - 1; i >= 0; i--)
        {
            Animal a = animalsMarkedForExplosion[i];
            if (a == null)
            {
                animalsMarkedForExplosion.RemoveAt(i);
                continue;
            }

            if (a.GetAnimalType() == AnimalType.Egg)
            {
                a.Disappear();
                animalsMarkedForExplosion.RemoveAt(i);
            }
        }

        // Step 3: Destroy others (check for ice and type)
        for (int i = animalsMarkedForExplosion.Count - 1; i >= 0; i--)
        {
            Animal a = animalsMarkedForExplosion[i];
            if (a == null)
            {
                animalsMarkedForExplosion.RemoveAt(i);
                continue;
            }

            if (a.HasIce())
            {
                a.RemoveIce();
            }
            else
            {
                a.Disappear();
                ScoreManager.instance.UpdateScore(a.GetAnimalType(), transform.position);
            }

            animalsMarkedForExplosion.RemoveAt(i);
        }

        // Step 4: Push other animals in push radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, pushRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Animal other) && other != this)
            {
                float distance = Vector2.Distance(transform.position, other.transform.position);
                if (distance > killRadius && distance <= pushRadius)
                {
                    Vector2 pushDirection = (other.transform.position - transform.position).normalized;
                    float strength = Mathf.Lerp(force, 0f, distance / pushRadius);
                    other.Push(pushDirection * strength);
                }
            }
        }

        // Step 5: Play effect
        if (mergeEffect != null)
        {
            mergeEffect.transform.SetParent(null);
            mergeEffect.Play();
        }

        // Step 6: Cleanup
        VibrationManager.instance.Vibrate(VibrationType.Heavy);
        AudioManager.instance.PlayExplosionSound(transform.position);
        Destroy(gameObject);
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        float killRadius = CalculateKillRadius();
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, killRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, killRadius * 2.5f);
    }
#endif



    public void ShowExplosionPreviewEffect()
    {
        Transform outline = transform.Find("Outline");
        if (outline != null)
            outline.gameObject.SetActive(true);
    }

    public void HideExplosionPreviewEffect()
    {
        Transform outline = transform.Find("Outline");
        if (outline != null)
            outline.gameObject.SetActive(false);
    }

    public void MarkForExplosion(bool enable)
    {
        markedForExplosion = enable;
        if (enable)
            ShowExplosionPreviewEffect();
        else
            HideExplosionPreviewEffect();
    }


    public void PreviewExplosionRadius(float killRadius)
    {
        if (!isExplosive) return;

        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, killRadius);

        HashSet<Animal> currentlyInRadius = new HashSet<Animal>();

        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Animal other) && other != this)
            {
                if ((other.GetAnimalType() < this.GetAnimalType() || other.HasIce()) && !animalsMarkedForExplosion.Contains(other))
                {
                    other.MarkForExplosion(true);
                    animalsMarkedForExplosion.Add(other);
                }

                currentlyInRadius.Add(other);
            }
        }

        // Check previously marked animals and unmark those no longer in radius
        for (int i = animalsMarkedForExplosion.Count - 1; i >= 0; i--)
        {
            Animal a = animalsMarkedForExplosion[i];
            if (!currentlyInRadius.Contains(a))
            {
                if(a != null)
                    a.MarkForExplosion(false);
                animalsMarkedForExplosion.RemoveAt(i);
            }
        }
    }

}
