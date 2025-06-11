using UnityEngine;

public class MergePushEffect : MonoBehaviour
{
    [Header(" Settings ")]
    [SerializeField] private float pushMagnitude = 5f;
    [SerializeField] private float pushRadius = 0.5f;
    private Vector2 pushPosition;

    [Header(" Debug ")]
    [SerializeField] private bool enableGizmos;
    void Awake()
    {
        MergeManager.onMergeAnimal += PushEffect;
    }


    void OnDestroy()
    {
        MergeManager.onMergeAnimal -= PushEffect;
    }

    private void PushEffect(AnimalType animalType, Vector2 position)
    {
        pushPosition = position;

        Collider2D[] colliders = Physics2D.OverlapCircleAll(pushPosition, pushRadius);

        foreach (Collider2D collider in colliders)
        {
            if (collider.TryGetComponent<Animal>(out Animal animal))
            {
                if (animal == GameplayController.instance.GetCurrentAnimal())
                    continue;

                Vector2 force = ((Vector2)collider.transform.position - pushPosition).normalized;
                force *= pushMagnitude;
                animal.Push(force);
            }
        }

    }
#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!enableGizmos)
            return;
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(pushPosition, pushRadius);
    }
#endif
}
