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
        Explode(effectRadius, explosionRadius);
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
        if (collision.collider.CompareTag("Wall")) return; // prevent early trigger on wall
        hasCollided = true;
    }
}
