using UnityEngine;
using System.Collections.Generic;

public class Egg : Animal
{
    [SerializeField] private float scanRadius = 2f;
    [SerializeField] private AnimalType defaultAnimalType = AnimalType.Monkey;
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        // Prevent merging logic
    }

    public override void Disappear()
    {
        if (mergeEffect != null)
        {
            mergeEffect.transform.SetParent(null);
            mergeEffect.Play();
            VibrationManager.instance.Vibrate(VibrationType.Medium);
        }

        Animal animalToSpawn = GetAnimalToSpawn();
        GameplayController.instance.SpawnAnimal(animalToSpawn, transform.position);

        Destroy(gameObject);
    }

    private Animal GetAnimalToSpawn()
    {
        float radius = scanRadius;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, radius);
        List<AnimalType> nearbyTypes = new List<AnimalType>();

        foreach (var col in nearby)
        {
            Animal a = col.GetComponent<Animal>();
            if (a != null && a != this)
            {
                nearbyTypes.Add(a.GetAnimalType());
            }
        }

        // Fallback if empty
        AnimalType chosenType = nearbyTypes.Count > 0 ?
            nearbyTypes[Random.Range(0, nearbyTypes.Count)] : defaultAnimalType;

        Animal newAnimal = GameplayController.instance.GetAnimalFromType(chosenType);

        // Randomly apply modifiers
        float rand = Random.value;
        if (rand < 0.33f)
            newAnimal.ApplyIce();
        else if (rand < 0.66f)
            newAnimal.MakeExplosive();

        return newAnimal;
    }

    public virtual bool CanMerge()
    {
        // Eggs cannot merge with other animals
        return false;
    }
    
    #if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, scanRadius);
    }
#endif
}
