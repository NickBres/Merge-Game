using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class AnimalSelector : MonoBehaviour
{
    public static AnimalSelector instance;
    [Header(" Elements ")]
    [SerializeField] private Animal currentAnimalObj;
    [SerializeField] private Animal nextAnimalObj;
    [SerializeField] private Animal previousAnimalObj;

    [System.Serializable]
    public class AnimalSpritePair
    {
        public AnimalType animalType;
        public Sprite sprite;
    }

    [SerializeField] private List<AnimalSpritePair> animalSprites;

    private Dictionary<AnimalType, Sprite> animalSpriteMap;
    private AnimalType currentType;
    private AnimalType[] animalTypes;
    public static Action OnAnimalChanged;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SkinCell.OnSkinChanged += UpdateCurrentAnimalSkin; // Subscribe to skin change event
    }

    private void OnDestroy()
    {
        SkinCell.OnSkinChanged -= UpdateCurrentAnimalSkin; // Unsubscribe to avoid memory leaks
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animalSpriteMap = new Dictionary<AnimalType, Sprite>();
        foreach (var pair in animalSprites)
        {
            if (!animalSpriteMap.ContainsKey(pair.animalType))
            {
                animalSpriteMap.Add(pair.animalType, pair.sprite);
            }
        }

        animalTypes = new AnimalType[] {
            AnimalType.Snake,
            AnimalType.Parrot,
            AnimalType.Rabbit,
            AnimalType.Penguin,
            AnimalType.Monkey,
            AnimalType.Pig,
            AnimalType.Panda,
            AnimalType.Giraffe,
            AnimalType.Hippo,
            AnimalType.Elephant
        };

        currentType = AnimalType.Snake;
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void UpdateUI()
    {

        int currentIndex = System.Array.IndexOf(animalTypes, currentType);

        // Update previous image
        if (previousAnimalObj != null)
        {
            SpriteRenderer prevSprite = previousAnimalObj.GetComponentInChildren<SpriteRenderer>();
            if (prevSprite != null)
            {
                if (currentIndex > 0 && animalSpriteMap.TryGetValue(animalTypes[currentIndex - 1], out Sprite sprite))
                {
                    previousAnimalObj.SetAnimalType(animalTypes[currentIndex - 1]);
                    prevSprite.sprite = sprite;
                    prevSprite.enabled = true;
                    GameplayController.instance.ApplySkinToAnimal(previousAnimalObj);
                }
                else
                {
                    prevSprite.enabled = false;
                    Transform skin = previousAnimalObj.transform.Find("Skin");
                    if (skin != null) skin.gameObject.SetActive(false);
                }
            }
        }

        // Update current 
        if (currentAnimalObj != null && animalSpriteMap.TryGetValue(currentType, out Sprite currSprite))
        {
            currentAnimalObj.SetAnimalType(currentType);
            SpriteRenderer currImage = currentAnimalObj.GetComponentInChildren<SpriteRenderer>();
            if (currImage != null)
            {
                currImage.sprite = currSprite;
            }

            GameplayController.instance.ApplySkinToAnimal(currentAnimalObj);
        }

        // Update next image
        if (nextAnimalObj != null)
        {
            SpriteRenderer nextImage = nextAnimalObj.GetComponentInChildren<SpriteRenderer>();
            if (nextImage != null)
            {
                if (currentIndex < animalTypes.Length - 1 && animalSpriteMap.TryGetValue(animalTypes[currentIndex + 1], out Sprite nextSprite))
                {
                    nextAnimalObj.SetAnimalType(animalTypes[currentIndex + 1]);
                    nextImage.sprite = nextSprite;
                    nextImage.enabled = true;
                    GameplayController.instance.ApplySkinToAnimal(nextAnimalObj);
                }
                else
                {
                    nextImage.enabled = false;
                    Transform skin = nextAnimalObj.transform.Find("Skin");
                    if (skin != null) skin.gameObject.SetActive(false);
                }
            }
        }
        OnAnimalChanged?.Invoke();
    }

    public void SelectNextAnimal()
    {
        currentType = GetNextAnimalType(currentType);
        UpdateUI();
    }

    public void SelectPreviousAnimal()
    {
        currentType = GetPreviousAnimalType(currentType);
        UpdateUI();
    }

    private AnimalType GetNextAnimalType(AnimalType type)
    {
        int index = System.Array.IndexOf(animalTypes, type);
        index = (index + 1) % animalTypes.Length;
        return animalTypes[index];
    }

    private AnimalType GetPreviousAnimalType(AnimalType type)
    {
        int index = System.Array.IndexOf(animalTypes, type);
        index = (index - 1 + animalTypes.Length) % animalTypes.Length;
        return animalTypes[index];
    }

    public void OnNextAnimalClicked()
    {
        currentType = GetNextAnimalType(currentType);
        UpdateUI();
        UIManager.ClickAndVibrate();
    }

    public void OnPreviousAnimalClicked()
    {
        currentType = GetPreviousAnimalType(currentType);
        UpdateUI();
        UIManager.ClickAndVibrate();
    }

    public AnimalType GetCurrentAnimalType()
    {
        return currentType;
    }
    public void UpdateCurrentAnimalSkin()
    {
        if (currentAnimalObj != null)
        {
            GameplayController.instance.ApplySkinToAnimal(currentAnimalObj);
        }
    }
}
