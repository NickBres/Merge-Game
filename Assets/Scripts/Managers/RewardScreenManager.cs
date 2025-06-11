using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RewardScreenManager : MonoBehaviour
{
    public static RewardScreenManager instance;

    [SerializeField] private Animator portalAnimator;
    [SerializeField] private Image skinIcon;
    [SerializeField] private TextMeshProUGUI rewardText;
    [SerializeField] private Image rewardTextBox;
    [SerializeField] private Sprite coinsImg;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Ensure the portal animator is assigned
        if (portalAnimator == null)
        {
            Debug.LogError("Portal Animator is not assigned in RewardScreenManager.");
        }
    }

    private void SetupPortal(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: portalAnimator.SetTrigger("PlayGreen"); break;
            case Rarity.Rare: portalAnimator.SetTrigger("PlayBlue"); break;
            case Rarity.Epic: portalAnimator.SetTrigger("PlayPurple"); break;
            case Rarity.Legendary: portalAnimator.SetTrigger("PlayGold"); break;
        }
    }

    public void ShowReward(SkinDataSO skinData, bool isDuplicate, int coinAmount = 0)
    {
        SetupPortal(skinData.rarity);

        // Show skin image
        skinIcon.sprite = isDuplicate ? coinsImg : skinData.sprite;

        // Update reward description
        rewardText.text = isDuplicate ? $"+{coinAmount}" : skinData.name;

        // Set background color based on rarity
        rewardTextBox.color = SkinCell.GetColorByRarity(skinData.rarity);

        if(skinData.rarity == Rarity.Legendary)
        {
            AudioManager.instance.PlayLegendarySound();
        }
    }
}
