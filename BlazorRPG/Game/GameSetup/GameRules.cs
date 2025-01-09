public class GameRules
{
    public static GameRules StandardRuleset = new GameRules
    {
        StartingHealth = 40,
        StartingCoins = 40,
        StartingInventorySize = 20,

        StartingPhysicalEnergy = 40,
        StartingFocusEnergy = 40,
        StartingSocialEnergy = 40,

        MinimumHealth = 0,
        DailyFoodRequirement = 2,
        NoFoodEffectOnHealth = -2,
        NoShelterEffectOnHealth = -2,
    };

    public int StartingHealth;
    public int StartingCoins;
    public int StartingInventorySize;

    public int StartingPhysicalEnergy;
    public int StartingFocusEnergy;
    public int StartingSocialEnergy;

    public int MinimumHealth;
    public int DailyFoodRequirement;
    public int NoFoodEffectOnHealth;
    public int NoShelterEffectOnHealth;

}
