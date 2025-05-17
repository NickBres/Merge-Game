using UnityEngine;

public class Fruit : MonoBehaviour
{

    private Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void EnablePhysics()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
    }

    public void MoveTo(Vector3 newPosition)
    {
        transform.position = newPosition;
    }
}
