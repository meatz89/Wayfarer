public class GameRules
{
    public static GameRules StandardRuleset = new GameRules
    {
        StartingPhysicalEnergy = 20,

        StartingCoins = 5,
        StartingInventorySize = 10,

        StartingHealth = 20,
        StartingConcentration = 20,
        StartingConfidence = 20,

        MinimumHealth = 0,
    };

    public int StartingHealth;
    public int StartingCoins;
    public int StartingInventorySize;

    public int StartingPhysicalEnergy;
    public int StartingConcentration;
    public int StartingConfidence;

    public int MinimumHealth;
    public int DailyFoodRequirement;
    public int NoFoodEffectOnHealth;
    public int NoShelterEffectOnHealth;

    public static int StrategicMomentumRequirement = 4;
    public static int StrategicInsightRequirement = 4;
    public static int StrategicResonanceRequirement = 4;

    public ArchetypeTypes PlayerArchetype = ArchetypeTypes.Artisan;

    public string Background = string.Empty;
    public string Name = "Wayfarer";

}
