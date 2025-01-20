public class GameRules
{
    public static GameRules StandardRuleset = new GameRules
    {
        StartingHealth = 10,
        StartingCoins = 10,
        StartingInventorySize = 10,

        StartingPhysicalEnergy = 2,
        StartingFocusEnergy = 3,
        StartingSocialEnergy = 7,

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

    public static int StrategicMomentumRequirement = 5;
    public static int StrategicInsightRequirement = 6;
    public static int StrategicResonanceRequirement = 7;

    public static List<ChoiceApproaches> GetPriorityOrder(
        ChoiceArchetypes archetype,
        EncounterValues values)
    {
        if (values.Pressure >= 7)
        {
            // High pressure priority order is the same for all archetypes
            return new List<ChoiceApproaches>
        {
            ChoiceApproaches.Careful,
        };
        }

        // Normal pressure priority orders vary by archetype
        return archetype switch
        {
            ChoiceArchetypes.Physical => new List<ChoiceApproaches>
        {
            ChoiceApproaches.Strategic,
            ChoiceApproaches.Careful,
            ChoiceApproaches.Aggressive,
        },

            ChoiceArchetypes.Focus => new List<ChoiceApproaches>
        {
            ChoiceApproaches.Strategic,
            ChoiceApproaches.Careful,
            ChoiceApproaches.Aggressive,
        },

            ChoiceArchetypes.Social => new List<ChoiceApproaches>
        {
            ChoiceApproaches.Strategic,
            ChoiceApproaches.Careful,
            ChoiceApproaches.Aggressive,
        },

            _ => throw new ArgumentException("Invalid archetype")
        };
    }

    // Conversion methods (copied from EncounterChoice for consistency)
    public static ChangeTypes ConvertValueTypeToChangeType(ValueTypes valueType)
    {
        return valueType switch
        {
            ValueTypes.Outcome => ChangeTypes.Outcome,
            ValueTypes.Momentum => ChangeTypes.Momentum,
            ValueTypes.Insight => ChangeTypes.Insight,
            ValueTypes.Resonance => ChangeTypes.Resonance,
            ValueTypes.Pressure => ChangeTypes.Pressure,
            _ => throw new ArgumentException("Invalid ValueType")
        };
    }

    public static ChangeTypes ConvertEnergyTypeToChangeType(EnergyTypes energyType)
    {
        return energyType switch
        {
            EnergyTypes.Physical => ChangeTypes.PhysicalEnergy,
            EnergyTypes.Focus => ChangeTypes.FocusEnergy,
            EnergyTypes.Social => ChangeTypes.SocialEnergy,
            _ => throw new ArgumentException("Invalid EnergyType")
        };
    }

    public static int GetBaseEnergyCost(ChoiceArchetypes archetype, ChoiceApproaches approach)
    {
        // Each approach has a base energy cost that reflects its intensity
        return approach switch
        {
            ChoiceApproaches.Aggressive => 3, // High energy cost for aggressive actions
            ChoiceApproaches.Careful => 2,    // Moderate cost for careful actions
            ChoiceApproaches.Strategic => 2,   // Moderate cost for strategic actions
            ChoiceApproaches.Desperate => 1,   // Low cost as a fallback option
            _ => throw new ArgumentException("Invalid approach")
        };
    }

}
