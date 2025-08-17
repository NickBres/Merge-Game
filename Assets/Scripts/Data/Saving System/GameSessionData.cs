using System.Collections.Generic;

[System.Serializable]
public class GameSessionData
{
    public List<AnimalData> animals = new List<AnimalData>();
    public ScoreData score;
    public AnimalType nextAnimal;

    public int eggLimit;
    public float eggSpawnChance;
    public float iceChance;

}