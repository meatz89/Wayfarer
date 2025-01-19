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

    public static List<ChoiceApproaches> GetPriorityOrder(
        ChoiceArchetypes archetype,
        EncounterStateValues values)
    {
        if (values.Pressure >= 7)
        {
            // High pressure priority order is the same for all archetypes
            return new List<ChoiceApproaches>
        {
            ChoiceApproaches.Careful,
            ChoiceApproaches.Desperate
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
            ChoiceApproaches.Desperate
        },

            ChoiceArchetypes.Focus => new List<ChoiceApproaches>
        {
            ChoiceApproaches.Aggressive,
            ChoiceApproaches.Strategic,
            ChoiceApproaches.Careful,
            ChoiceApproaches.Desperate
        },

            ChoiceArchetypes.Social => new List<ChoiceApproaches>
        {
            ChoiceApproaches.Careful,
            ChoiceApproaches.Aggressive,
            ChoiceApproaches.Strategic,
            ChoiceApproaches.Desperate
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
}
