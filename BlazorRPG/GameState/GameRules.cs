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

    public ArchetypeTypes PlayerArchetype = ArchetypeTypes.Ranger;

    public static int GetBaseEnergyCost(BasicActionTypes actionType)
    {
        int energycost = 2;
        energycost = actionType switch
        {
            BasicActionTypes.Travel => 2,
            BasicActionTypes.Rest => -3,

            BasicActionTypes.Labor => 3,
            BasicActionTypes.Fight => 3,
            BasicActionTypes.Gather => 2,

            BasicActionTypes.Study => 3,
            BasicActionTypes.Investigate => 2,
            BasicActionTypes.Analyze => 2,

            BasicActionTypes.Discuss => 1,
            BasicActionTypes.Persuade => 3,
            BasicActionTypes.Perform => 3,
        };

        return energycost;
    }

}
