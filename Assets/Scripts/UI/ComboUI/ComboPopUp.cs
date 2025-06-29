using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboPopUp : MonoBehaviour
{
    public static ComboPopUp instance;
    [SerializeField] private GameObject comboPopupPrefab;

    [Header(" Settings ")]
    [SerializeField] private float time = 2f;

    void Awake()
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
    }

    public void ShowCombo(Vector3 worldPosition, int comboValue)
    {
        if (comboValue < 2) return;
        GameObject popup = Instantiate(comboPopupPrefab, worldPosition, Quaternion.identity);

        TextMeshPro comboText = popup.GetComponent<TextMeshPro>();
        comboText.text = comboValue.ToString() + "x";

        DesignText(comboText, comboValue);
        Destroy(popup, time); // clean up after animation
    }

    private void DesignText(TextMeshPro text, int comboValue)
    {
        float sizeMultiplier = Mathf.Clamp(comboValue / 5f, 1f, 2f);
        text.fontSize *= sizeMultiplier;

        if (comboValue >= 8)
            text.color = Color.red;
        else if (comboValue >= 4)
            text.color = Color.orange;
        else
            text.color = Color.yellow;
    }
}
