using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AnimalSlot : MonoBehaviour
{

    [Header(" Elements ")]
    [SerializeField] private Animal animalPrefab;

    private Image animalImage;
    private Image animalSkin;

    void Awake()
    {
        animalImage = transform.Find("Animal Image")?.GetComponent<Image>();
        animalSkin = transform.Find("Animal Image/Animal Skin")?.GetComponent<Image>();
    }

    void Start()
    {
        LoadSlot();
    }

    void LoadSlot()
    {
        if (animalPrefab == null)
        {
            Debug.LogError("Animal prefab is not assigned in the slot.");
            return;
        }

        animalImage.sprite = animalPrefab.GetSprite();
        
    }

}
