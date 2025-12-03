/// <summary>
/// Entry for modifier parameter values.
/// INLINED from CollectionEntries.cs per HIGHLANDER principle (keep class with its primary consumer)
/// </summary>
public class ModifierParameterEntry
{
    public string ParameterName { get; set; }
    public int Value { get; set; }
}

/// <summary>
/// Represents a personality-specific conversation modifier
/// Maps a personality type to its mechanical effect and parameters
/// </summary>
public class PersonalityModifier
{
    /// <summary>
    /// The type of personality modifier
    /// </summary>
    public PersonalityModifierType Type { get; set; }

    /// <summary>
    /// Optional parameters for the modifier (e.g., multipliers, caps)
    /// DOMAIN COLLECTION PRINCIPLE: List<T> instead of Dictionary
    /// </summary>
    public List<ModifierParameterEntry> Parameters { get; set; } = new List<ModifierParameterEntry>();

    /// <summary>
    /// Get parameter value by name, returns 0 if not found
    /// </summary>
    public int GetParameter(string name)
    {
        ModifierParameterEntry entry = Parameters.FirstOrDefault(p => p.ParameterName == name);
        return entry?.Value ?? 0;
    }

    /// <summary>
    /// Set parameter value by name
    /// </summary>
    public void SetParameter(string name, int value)
    {
        ModifierParameterEntry entry = Parameters.FirstOrDefault(p => p.ParameterName == name);
        if (entry != null)
        {
            entry.Value = value;
        }
        else
        {
            Parameters.Add(new ModifierParameterEntry { ParameterName = name, Value = value });
        }
    }

    /// <summary>
    /// Create a personality modifier based on personality type
    /// </summary>
    public static PersonalityModifier CreateFromPersonalityType(PersonalityType personalityType)
    {
        PersonalityModifier modifier = new PersonalityModifier();

        switch (personalityType)
        {
            case PersonalityType.PROUD:
                modifier.Type = PersonalityModifierType.AscendingFocusRequired;
                break;

            case PersonalityType.DEVOTED:
                modifier.Type = PersonalityModifierType.MomentumLossIncreased;
                modifier.SetParameter("additionalLoss", 2); // +2 to any momentum loss
                break;

            case PersonalityType.MERCANTILE:
                modifier.Type = PersonalityModifierType.HighestFocusBonus;
                modifier.SetParameter("momentumBonus", 3); // +3 Momentum bonus
                break;

            case PersonalityType.CUNNING:
                modifier.Type = PersonalityModifierType.RepeatFocusPenalty;
                modifier.SetParameter("penalty", -2); // -2 rapport for repeat focus
                break;

            case PersonalityType.STEADFAST:
                modifier.Type = PersonalityModifierType.RapportChangeCap;
                modifier.SetParameter("cap", 2); // Cap rapport changes at ±2
                break;

            default:
                modifier.Type = PersonalityModifierType.None;
                break;
        }

        return modifier;
    }
}