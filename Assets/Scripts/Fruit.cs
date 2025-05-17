using UnityEngine;
using System;
using System.Security.Cryptography.X509Certificates;

public class Fruit : MonoBehaviour
{
    [Header(" Data ")]
    [SerializeField] private FruitType type;

    [Header(" Actions ")]
    public static Action<Fruit,Fruit> onCollisionWithFruit;

    private Rigidbody2D rb;
    private Collider2D collider;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
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
        collider.enabled = true;
    }

    public void MoveTo(Vector3 newPosition)
    {
        transform.position = newPosition;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out Fruit otherFruit))
        {
            if (otherFruit.GetFruitType() != type)
                return;  
            onCollisionWithFruit?.Invoke(this, otherFruit);
        }
    }
    
    public FruitType GetFruitType()
    {
        return type;
    }
}
