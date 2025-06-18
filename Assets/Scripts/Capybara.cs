using UnityEngine;

public class Capybara : Animal
{
    protected override void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Wall")) return;
        hasCollided = true;
        AllowMerge();

        if (!canBeMerged || isFrozen)
            return;

        if (collision.collider.TryGetComponent(out Animal otherAnimal))
        {
            if (otherAnimal == this) return;
            if (otherAnimal.CanMerge())
            {
                this.SetAnimalType(otherAnimal.GetAnimalType()); // mimic the type of the other animal
                onCollisionWithAnimal?.Invoke(this); // merge
            }
        }
    }
}
