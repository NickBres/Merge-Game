using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShowAbilities : MonoBehaviour
{
    public GameObject abilitiesPanel;
    public Image toggleIcon;
    public Sprite arrowDownIcon;
    public Sprite placeholderIcon; // Replace this with your final "closed" icon

    private bool isOpen = false;

    public void ToggleAbilities()
    {
        isOpen = !isOpen;
        abilitiesPanel.SetActive(isOpen);
        toggleIcon.sprite = isOpen ? arrowDownIcon : placeholderIcon;
        AudioManager.instance.PlayClickSound();
    }
}
