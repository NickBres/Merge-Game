using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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
        Transform crack = transform.Find("Egg Crack");
        if (crack != null)
        {
            crack.gameObject.SetActive(true);
            AudioManager.instance.PlayEggCrackSound();
        }
        Animal animalToSpawn = GetAnimalToSpawn();
        StartCoroutine(DelayedDisappear(animalToSpawn));
    }

    private IEnumerator DelayedDisappear(Animal animalToSpawn)
    {
        yield return new WaitForSeconds(Random.Range(1f, 1.5f));

        if (mergeEffect != null)
        {
            mergeEffect.transform.SetParent(null);
            mergeEffect.Play();
            VibrationManager.instance.Vibrate(VibrationType.Medium);
        }

       
        GameplayController.instance.SpawnAnimal(animalToSpawn, transform.position);

        Destroy(gameObject);
        AudioManager.instance.PlayEggCrackSound();

    }

    private Animal GetAnimalToSpawn()
    {
        float radius = scanRadius;
        Collider2D[] nearby = Physics2D.OverlapCircleAll(transform.position, radius);
        List<AnimalType> nearbyTypes = new List<AnimalType>();

        foreach (var col in nearby)
        {
            Animal a = col.GetComponent<Animal>();
            if (a != null && a != this && a.GetAnimalType() != AnimalType.Egg && a.GetAnimalType() < AnimalType.Bomb)

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

    public override bool CanMerge()
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
