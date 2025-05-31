using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SkinCell : MonoBehaviour
{
    [SerializeField] private SkinDataSO skinData;
    [SerializeField] private Image skinImage;
    [SerializeField] private Image background;
    [SerializeField] private Sprite selectedBackground;
    [SerializeField] private Sprite defaultBackground;
    private Color cellColor;
    private bool isSelected = false;

    public static Action OnSkinChanged;

    public void Initialize(SkinDataSO data, bool selected)
    {
        skinData = data;
        isSelected = selected;
        SetCell();

        Button button = GetComponent<Button>();
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClick);
        }
    }

    private void OnClick()
    {
        if (!PlayerDataManager.instance.HasSkin(skinData.name)) return;

        AnimalType currentAnimal = AnimalSelector.instance.GetCurrentAnimalType();
        PlayerDataManager.instance.AssignSkinToAnimal(currentAnimal, skinData.name);
        OnSkinChanged?.Invoke();
    }

    public void selectCell()
    {
        isSelected = true;
        SetCell();
    }

    public void deselectCell()
    {
        isSelected = false;
        SetCell();
    }

    public void SetCell()
    {
        if (skinData == null || PlayerDataManager.instance == null) return;
        bool isUnlocked = PlayerDataManager.instance.HasSkin(skinData.name);
        skinImage.sprite = skinData.sprite;
        skinImage.color = isUnlocked ? Color.white : GetLockedTint();
        cellColor = GetColorByRarity(isUnlocked ? skinData.rarity : Rarity.None);
        background.color = cellColor;
        background.sprite = isSelected ? selectedBackground : defaultBackground;
    }
    
    public static Color GetColorByRarity(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.None => Color.white, // No skin (used to skip rarity coloring)
            Rarity.Common => new Color(0.2558098f, 0.6415094f, 0.1664293f, 1),
            Rarity.Rare => new Color(0.1647059f, 0.5456201f, 0.6431373f, 1),
            Rarity.Epic => new Color(0.4269549f, 0.1647059f, 0.6431373f),
            Rarity.Legendary => new Color(0.8584906f, 0.6995611f, 0.07694017f, 1),
            _ => Color.white
        };
    }

    private Color GetLockedTint()
    {
        return new Color(0f, 0f, 0f, 0.7f); // Black with 70% alpha
    }
}