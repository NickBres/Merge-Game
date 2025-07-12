using System.Collections.Generic;

[System.Serializable]
public class GameSessionData
{
    public List<AnimalData> animals = new List<AnimalData>();
    public ScoreData score;
    public AnimalType nextAnimal;

}