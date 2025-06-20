using UnityEngine;

public class SkinsUIManager : MonoBehaviour
{
    [SerializeField] private GameObject animalSelectorPanel;
    [SerializeField] private Transform gridParent;
    [SerializeField] private SkinCell skinCellPrefab;

    void Awake()
    {
        AnimalSelector.OnAnimalChanged += ResetGrid; // Subscribe to the event
        SkinCell.OnSkinChanged += ResetGrid; // Subscribe to skin change event
        ShopManager.OnGotSkin += ResetGrid;
    }

    void OnDestroy()
    {
        AnimalSelector.OnAnimalChanged -= ResetGrid; // Unsubscribe to avoid memory leaks
        SkinCell.OnSkinChanged -= ResetGrid; // Unsubscribe to skin change event
        ShopManager.OnGotSkin -= ResetGrid;
    }

    private void Start()
    {

        ResetGrid();

    }

    private void ResetGrid()
    {
        var allSkins = SkinManager.instance.GetAllSkins();
        var currentAnimal = GetCurrentAnimal();
        SkinDataSO currAnimalSkin = SkinManager.instance.GetSkinForAnimal(currentAnimal);
        CleanGrid();

        foreach (var skin in allSkins)
        {
            SkinCell cell = Instantiate(skinCellPrefab, gridParent);
            bool isSelected = currAnimalSkin != null && skin.skinID == currAnimalSkin.skinID;
            cell.Initialize(skin, isSelected);
        }


    }

    private void CleanGrid()
    {
        foreach (Transform child in gridParent)
        {
            Destroy(child.gameObject); // Clean up any existing children
        }
    }

    private AnimalType GetCurrentAnimal()
    {
        Transform currentAnimalTransform = animalSelectorPanel.transform.Find("Current");
        if (currentAnimalTransform == null)
        {
            Debug.LogError("Current animal object not found!");
            return default;
        }

        Animal animalComponent = currentAnimalTransform.GetComponentInChildren<Animal>();
        if (animalComponent == null)
        {
            Debug.LogError("Animal component not found in Current object!");
            return default;
        }

        return animalComponent.GetAnimalType();
    }
}

    
