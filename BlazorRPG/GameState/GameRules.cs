
using System.Reflection.Metadata.Ecma335;

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
    public LocationNames StartingLocation = LocationNames.DeepForest;

    public string Background = string.Empty;
    public string Name = "Wayfarer";

    public static int GetBaseTimeCost(BasicActionTypes actionType)
    {
        return actionType switch
        {
            BasicActionTypes.Rest => 4,      // Resting takes significant time
            BasicActionTypes.Travel => 2,     // Travel takes moderate time
            BasicActionTypes.Consume => 1,    // Eating/drinking is quick
            BasicActionTypes.Explore => 2,    // Exploration takes time
            BasicActionTypes.Observe => 1,    // Observation is relatively quick
            BasicActionTypes.Forage => 2,     // Foraging takes moderate time
            BasicActionTypes.Labor => 3,      // Labor is time-consuming
            BasicActionTypes.Gather => 2,     // Gathering resources takes time
            BasicActionTypes.Study => 3,      // Studying takes significant time
            BasicActionTypes.Investigate => 2, // Investigation takes moderate time
            BasicActionTypes.Analyze => 2,    // Analysis takes moderate time
            BasicActionTypes.Discuss => 1,    // Discussion is relatively quick
            BasicActionTypes.Persuade => 1,   // Persuasion attempts are quick
            BasicActionTypes.Perform => 2,    // Performances take moderate time
            BasicActionTypes.Fight => 1,      // Combat is intense but quick
            BasicActionTypes.Climb => 2,      // Climbing takes moderate time
            _ => 1                           // Default to 1 hour
        };
    }

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
            _ => 2
        };

        return energycost;
    }

}
