using UnityEngine;

public class ExplosionHighlighter : MonoBehaviour
{
    public static ExplosionHighlighter instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void CheckExplosives()
    {
        AnimalsParent animalsParent = AnimalsParent.instance;
        foreach (Transform child in animalsParent.transform)
        {
            Animal animal = child.GetComponent<Animal>();
            if (animal != null && animal.CanExplode())
            {
                return;
            }
        }

        foreach (Transform child in animalsParent.transform)
        {
            Animal animal = child.GetComponent<Animal>();
            if (animal != null)
            {
                animal.MarkForExplosion(false);
            }
        }
    }
}
