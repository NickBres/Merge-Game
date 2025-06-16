using UnityEngine;

public class AimLine : MonoBehaviour
{
    [SerializeField] private Transform originPoint;


    public void EnableLine()
    {
        gameObject.SetActive(true);
    }

    public void DisableLine()
    {
        gameObject.SetActive(false);
    }

    public void MoveLine(float x)
    {
        Vector3 pos = originPoint.position;
        pos.x = x;
        transform.position = pos;
    }
}
