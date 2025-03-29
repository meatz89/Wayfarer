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

    public static List<ChoiceApproaches> PrimaryArchetypeOnlyApproaches = new()
    {
        ChoiceApproaches.Aggressive,
        ChoiceApproaches.Desperate,
    };

    public static List<ChoiceApproaches> GetPriorityOrder(
        ChoiceArchetypes archetype)
    {
        // Priority orders should only include approaches that are valid for the archetype
        List<ChoiceApproaches> baseOrder = archetype switch
        {
            ChoiceArchetypes.Physical => new List<ChoiceApproaches>
            {
                ChoiceApproaches.Forceful,
                ChoiceApproaches.Tactical,
                ChoiceApproaches.Strategic,
                ChoiceApproaches.Careful
            },

            ChoiceArchetypes.Intellectual => new List<ChoiceApproaches>
            {
                ChoiceApproaches.Methodical,
                ChoiceApproaches.Tactical,
                ChoiceApproaches.Strategic,
                ChoiceApproaches.Careful
            },

            ChoiceArchetypes.Social => new List<ChoiceApproaches>
            {
                ChoiceApproaches.Diplomatic,
                ChoiceApproaches.Tactical,
                ChoiceApproaches.Strategic,
                ChoiceApproaches.Careful
            },

            _ => throw new ArgumentException("Invalid archetype")
        };

        return baseOrder;
    }

    public static List<ChoiceApproaches> GetAvailableApproaches(
        ChoiceArchetypes archetype,
        PlayerState playerState)
    {
        // Start with base approaches
        List<ChoiceApproaches> approaches = new List<ChoiceApproaches>
        {
            ChoiceApproaches.Diplomatic,    // Available with requirements
            ChoiceApproaches.Methodical,    // Available with requirements
            ChoiceApproaches.Forceful,    // Available with requirements
            ChoiceApproaches.Careful,    // Always available
            ChoiceApproaches.Strategic,  // Available with requirements
            ChoiceApproaches.Tactical,    // Available with requirements
            ChoiceApproaches.Aggressive,    // Available with requirements
            ChoiceApproaches.Desperate,    // Available with requirements
        };

        // Filter based on value requirements
        return approaches.Where(approach => IsApproachAvailable(archetype, approach, playerState))
                        .ToList();
    }

    private static bool IsApproachAvailable(
        ChoiceArchetypes archetype,
        ChoiceApproaches approach,
        PlayerState playerState)
    {
        return true;
    }

    public static EnergyTypes GetEnergyTypeForAction(BasicActionTypes actionType)
    {
        EnergyTypes energyType = EnergyTypes.Physical;
        energyType = actionType switch
        {
            BasicActionTypes.Travel => EnergyTypes.Physical,
            BasicActionTypes.Rest => EnergyTypes.Physical,

            BasicActionTypes.Labor => EnergyTypes.Physical,
            BasicActionTypes.Gather => EnergyTypes.Physical,
            BasicActionTypes.Fight => EnergyTypes.Physical,

            BasicActionTypes.Study => EnergyTypes.Concentration,
            BasicActionTypes.Investigate => EnergyTypes.Concentration,
            BasicActionTypes.Analyze => EnergyTypes.Concentration,

            BasicActionTypes.Discuss => EnergyTypes.Concentration,
            BasicActionTypes.Persuade => EnergyTypes.Concentration,
            BasicActionTypes.Perform => EnergyTypes.Physical,
        };
        return energyType;
    }

    public static int GetBaseEnergyCost(BasicActionTypes actionType)
    {
        int energycost = 2;
        energycost = actionType switch
        {
            BasicActionTypes.Travel => 1,
            BasicActionTypes.Rest => 0,

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
