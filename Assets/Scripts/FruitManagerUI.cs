using TMPro;
using UnityEngine;

[RequireComponent(typeof(FruitManager))]
public class FruitManagerUI : MonoBehaviour
{
    [Header(" Elements ")]
    private FruitManager fruitManager;
    [SerializeField] private TextMeshProUGUI nextFruitText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        fruitManager = GetComponent<FruitManager>();
    }

    // Update is called once per frame
    void Update()
    {
        nextFruitText.text = fruitManager.getNextFruit().name;
    }
}
