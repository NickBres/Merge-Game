using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AnimalManager))]
public class AnimalManagerUI : MonoBehaviour
{
    [Header(" Elements ")]
    private AnimalManager animalManager;
    [SerializeField] private Image nextAnimalImage;
    void Awake()
    {
        animalManager = GetComponent<AnimalManager>();
        AnimalManager.onNextAnimalSet += UpdateNextImage;
    }

    private void UpdateNextImage()
    {
        if (animalManager == null)
            return;


        nextAnimalImage.sprite = animalManager.nextAnimal.GetSprite();
        nextAnimalImage.SetNativeSize();
    }
    
    void OnDestroy()
    {
        AnimalManager.onNextAnimalSet -= UpdateNextImage;
    }
}
