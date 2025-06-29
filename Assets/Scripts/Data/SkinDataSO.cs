using UnityEngine;

[CreateAssetMenu(fileName = "NewSkin", menuName = "AnimalGame/Skin")]
public class SkinDataSO : ScriptableObject
{
    public string skinID;
    public Sprite sprite;
    public Rarity rarity;
}
