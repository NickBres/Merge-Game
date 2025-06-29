using UnityEngine;
using UnityEngine.UI;

public class FrenzyBack : MonoBehaviour
{
    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
    }

    private void Update()
    {
        CycleColors();
    }

    private void CycleColors()
    {
        float hue = Mathf.PingPong(Time.time * 0.2f, 1f); // Slow color change
        Color color = Color.HSVToRGB(hue, 1f, 1f);
        image.color = new Color(color.r, color.g, color.b, 0.2f);
    }
}
