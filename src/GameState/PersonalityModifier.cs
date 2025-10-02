using System.Collections.Generic;

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
    /// </summary>
    public Dictionary<string, int> Parameters { get; set; } = new Dictionary<string, int>();

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
                modifier.Type = PersonalityModifierType.MomentumLossDoubled;
                modifier.Parameters["multiplier"] = 2; // Double momentum losses
                break;

            case PersonalityType.MERCANTILE:
                modifier.Type = PersonalityModifierType.HighestFocusBonus;
                modifier.Parameters["momentumBonus"] = 3; // +3 Momentum bonus
                break;

            case PersonalityType.CUNNING:
                modifier.Type = PersonalityModifierType.RepeatFocusPenalty;
                modifier.Parameters["penalty"] = -2; // -2 rapport for repeat focus
                break;

            case PersonalityType.STEADFAST:
                modifier.Type = PersonalityModifierType.RapportChangeCap;
                modifier.Parameters["cap"] = 2; // Cap rapport changes at ±2
                break;

            default:
                modifier.Type = PersonalityModifierType.None;
                break;
        }

        return modifier;
    }
}