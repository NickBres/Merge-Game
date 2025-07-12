using UnityEngine;
public enum GameMode
{
    Rush,
    Zen
}
public enum AnimalType
{
    Egg = 0,
    Snake = 2,
    Parrot = 4,
    Rabbit = 8,
    Penguin = 16,
    Monkey = 32,
    Pig = 64,
    Panda = 128,
    Giraffe = 256,
    Hippo = 512,
    Elephant = 1024,
    Bomb = 2048,
    Capybara = 2049
}

public enum VibrationType
{
    Light,
    Medium,
    Heavy
}

public enum Rarity
{
    None = 0,      // 1 / 0 = 100% (not applicable)
    Common = 2,     // 1 / 2 = 50%
    Rare = 5,       // 1 / 5 = 20%
    Epic = 10,      // 1 / 10 = 10%
    Legendary = 20  // 1 / 20 = 5%
}

public enum GameState
{
    Menu,
    Game,
    GameOver,
    Pause
}

public enum ShapeState { Both, Square, Circle }