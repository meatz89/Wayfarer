using System;
using System.Collections.Generic;

/// <summary>
/// Maps authentic personality descriptions from JSON to categorical types
/// for personality-specific conversation cards.
/// 
/// Preserves human truth by maintaining descriptive personalities in content
/// while enabling mechanical differentiation for gameplay systems.
/// </summary>
public class PersonalityMappingService
{
    /// <summary>
    /// Maps descriptive personality strings to categorical types.
    /// Uses case-insensitive substring matching to handle variations.
    /// </summary>
    private static readonly Dictionary<string, PersonalityType> PersonalityMappings = new()
    {
        // DEVOTED: Family-oriented, faith-driven, emotionally invested
        ["intelligent and desperate"] = PersonalityType.DEVOTED,
        ["caring and worried"] = PersonalityType.DEVOTED,
        ["concerned and faithful"] = PersonalityType.DEVOTED,
        ["devoted"] = PersonalityType.DEVOTED,
        ["desperate"] = PersonalityType.DEVOTED,
        ["worried"] = PersonalityType.DEVOTED,
        ["faithful"] = PersonalityType.DEVOTED,

        // MERCANTILE: Business-focused, trade-oriented, practical
        ["businesslike and hurried"] = PersonalityType.MERCANTILE,
        ["practical and efficient"] = PersonalityType.MERCANTILE,
        ["merchant"] = PersonalityType.MERCANTILE,
        ["trader"] = PersonalityType.MERCANTILE,
        ["businesslike"] = PersonalityType.MERCANTILE,
        ["hurried"] = PersonalityType.MERCANTILE,
        ["practical"] = PersonalityType.MERCANTILE,

        // PROUD: Status-conscious, hierarchy-focused, formal
        ["formal and insistent"] = PersonalityType.PROUD,
        ["impatient and powerful"] = PersonalityType.PROUD,
        ["noble and demanding"] = PersonalityType.PROUD,
        ["formal"] = PersonalityType.PROUD,
        ["insistent"] = PersonalityType.PROUD,
        ["powerful"] = PersonalityType.PROUD,
        ["noble"] = PersonalityType.PROUD,
        ["demanding"] = PersonalityType.PROUD,

        // CUNNING: Information-focused, secretive, observant
        ["mysterious and observant"] = PersonalityType.CUNNING,
        ["calculating and discrete"] = PersonalityType.CUNNING,
        ["mysterious"] = PersonalityType.CUNNING,
        ["observant"] = PersonalityType.CUNNING,
        ["calculating"] = PersonalityType.CUNNING,
        ["discrete"] = PersonalityType.CUNNING,
        ["secretive"] = PersonalityType.CUNNING,

        // STEADFAST: Duty-bound, reliable, honor-focused
        ["stern and dutiful"] = PersonalityType.STEADFAST,
        ["friendly and observant"] = PersonalityType.STEADFAST,
        ["reliable and honest"] = PersonalityType.STEADFAST,
        ["stern"] = PersonalityType.STEADFAST,
        ["dutiful"] = PersonalityType.STEADFAST,
        ["reliable"] = PersonalityType.STEADFAST,
        ["honest"] = PersonalityType.STEADFAST,
        ["duty"] = PersonalityType.STEADFAST
    };

    /// <summary>
    /// Convert a descriptive personality string to a categorical type.
    /// Preserves the authentic description while enabling mechanical categorization.
    /// </summary>
    /// <param name="personalityDescription">Authentic personality description from JSON</param>
    /// <returns>Categorical personality type or default if no match found</returns>
    public static PersonalityType GetPersonalityType(string personalityDescription)
    {
        if (string.IsNullOrWhiteSpace(personalityDescription))
        {
            return PersonalityType.STEADFAST; // Default fallback
        }

        // Normalize input for comparison
        string normalized = personalityDescription.ToLowerInvariant().Trim();

        // Check for exact matches first
        if (PersonalityMappings.TryGetValue(normalized, out PersonalityType exactMatch))
        {
            return exactMatch;
        }

        // Check for substring matches (for complex descriptions)
        foreach (KeyValuePair<string, PersonalityType> mapping in PersonalityMappings)
        {
            if (normalized.Contains(mapping.Key))
            {
                return mapping.Value;
            }
        }

        // Log unmapped personalities for content team review
        Console.WriteLine($"[PersonalityMapping] Unknown personality: '{personalityDescription}' - defaulting to STEADFAST");
        return PersonalityType.STEADFAST;
    }

    /// <summary>
    /// Validate that a personality description maps to a known type.
    /// Used for content validation during development.
    /// </summary>
    /// <param name="personalityDescription">Personality to validate</param>
    /// <returns>True if personality maps to a known type</returns>
    public static bool IsValidPersonality(string personalityDescription)
    {
        if (string.IsNullOrWhiteSpace(personalityDescription))
        {
            return false;
        }

        string normalized = personalityDescription.ToLowerInvariant().Trim();

        // Check exact matches
        if (PersonalityMappings.ContainsKey(normalized))
        {
            return true;
        }

        // Check substring matches
        foreach (string key in PersonalityMappings.Keys)
        {
            if (normalized.Contains(key))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Get all supported personality descriptions for validation.
    /// Used by content validation systems.
    /// </summary>
    /// <returns>All mapped personality descriptions</returns>
    public static IEnumerable<string> GetSupportedPersonalities()
    {
        return PersonalityMappings.Keys;
    }
}