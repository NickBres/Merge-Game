using UnityEngine;

[CreateAssetMenu(fileName = "AnimalData", menuName = "Animals/Animal Data", order = 1)]
public class AnimalDataSO : ScriptableObject
{
    public AnimalType animalType;
    public Sprite animalSpriteRound;
    public Sprite animalSpriteSquare;
    public Sprite animalSkin;
    public Color particleColor;
    public Vector3 scale;


    public Sprite GetSprite()
    {
        return animalSpriteRound;
    }

}
