using UnityEngine;

public class AimLine : MonoBehaviour
{
    [SerializeField] private Transform originPoint;
    private LineRenderer lineRenderer; // Use LineRenderer instead of SpriteRenderer

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }


    public void EnableLine()
    {
        Animal currentAnimal = GameplayController.instance.GetCurrentAnimal();
        if (currentAnimal != null && lineRenderer != null)
        {
            float width = currentAnimal.transform.localScale.x;
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }
        gameObject.SetActive(true);

    }

    public void DisableLine()
    {
        gameObject.SetActive(false);
    }

    public void MoveLine(Vector3 targetPoint)
    {
        transform.position = targetPoint;
    }
}
