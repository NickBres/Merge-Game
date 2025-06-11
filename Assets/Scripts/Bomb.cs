using System.Collections;
using UnityEngine;

public class Bomb : Animal
{
    [SerializeField] private float explosionRadius = 2f;
    [SerializeField] private float effectRadius = 4f;
    [SerializeField] private float explosionDelay = 2f;

    private bool exploded = false;
    private bool coroutineStarted = false;

    private void Update()
    {
        if (!exploded && !coroutineStarted && hasCollided)
        {
            coroutineStarted = true;
            StartCoroutine(ExplodeAfterDelay());
        }
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode();
    }

    private void Explode()
    {
        if (exploded) return;
        exploded = true;


        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, effectRadius);
        foreach (var hit in hitColliders)
        {
            if (hit.TryGetComponent(out Animal animal))
            {
                float distance = Vector2.Distance(transform.position, animal.transform.position);
                if (distance <= explosionRadius)
                {
                    ScoreManager.instance.UpdateScore(animal.GetAnimalType(), Vector2.zero);
                    animal.Disappear(); // Delete animal
                }
                else
                {
                    Vector2 forceDir = (animal.transform.position - transform.position).normalized;
                    float force = 5f * (1f - distance / effectRadius);
                    animal.Push(forceDir * force);
                }
            }
        }
        AudioManager.instance.PlayExplosionSound(transform.position);
        Disappear();
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
#endif

    // Override to disable default Animal merging behavior when colliding.
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        hasCollided = true;
    }
}
