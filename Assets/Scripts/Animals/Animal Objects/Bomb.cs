using System.Collections;
using UnityEngine;

public class Bomb : Animal
{
    [SerializeField] private float explosionDelay = 2f;
    [SerializeField] private float killRadius = 3f;

    private bool coroutineStarted = false;

    private void Update()
    {
        if(exploded) return;
        PreviewExplosionRadius(killRadius);
        if (!coroutineStarted && hasCollided)
        {
            isExplosive = true;
            coroutineStarted = true;
            StartCoroutine(ExplodeAfterDelay());
        }
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        Explode(killRadius, killRadius * 2.5f, explosionForce);
    }




    // Override to disable default Animal merging behavior when colliding.
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall")) return; // prevent early trigger on wall
        hasCollided = true;
    }
}
