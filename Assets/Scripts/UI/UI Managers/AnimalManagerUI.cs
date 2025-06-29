using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GameplayController))]
public class AnimalManagerUI : MonoBehaviour
{
    [Header(" Elements ")]
    private GameplayController animalManager;
    [SerializeField] private Image nextAnimalImage;
    void Awake()
    {
        animalManager = GetComponent<GameplayController>();
        GameplayController.onNextAnimalSet += UpdateNextImage;
    }

    private void UpdateNextImage()
    {
        if (animalManager == null)
            return;


        nextAnimalImage.sprite = animalManager.GetNextAnimal().GetSprite();
    }
    
    void OnDestroy()
    {
        GameplayController.onNextAnimalSet -= UpdateNextImage;
    }
}
