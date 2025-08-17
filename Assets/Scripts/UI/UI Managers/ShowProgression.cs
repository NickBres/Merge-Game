using UnityEngine;
using UnityEngine.UI;

public class ShowProgression : MonoBehaviour
{
    [SerializeField] private Image progressionImage;
    [SerializeField] private Sprite secretWhale;
    [SerializeField] private Sprite openedWhale;

    private void Start()
    {
        if (PlayerDataManager.instance.IsWhaleUnlocked())
        {
            progressionImage.sprite = openedWhale;
        }
        else
        {
            progressionImage.sprite = secretWhale;
        }
    }
}
