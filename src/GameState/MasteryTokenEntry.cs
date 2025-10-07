using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Physical challenge mastery tracking - list-based, not dictionary.
/// Parallel to NPCTokenEntry for Social system and FamiliarityEntry for Mental system.
/// Each entry tracks mastery tokens earned for a specific physical challenge type.
/// </summary>
public class MasteryTokenEntry
{
    /// <summary>
    /// References PhysicalChallengeType.Id (string, not enum).
    /// Examples: "combat", "athletics", "finesse", "endurance", "strength"
    /// </summary>
    public string ChallengeTypeId { get; set; }

    /// <summary>
    /// Number of mastery tokens earned for this challenge type.
    /// Earned through repeated successful completions.
    /// Provides specialist bonuses (reduces Danger baseline).
    /// </summary>
    public int TokenCount { get; set; }
}

/// <summary>
/// Extension methods for List<MasteryTokenEntry> to make lookups easy.
/// Follows the exact pattern from NPCTokenEntry and FamiliarityEntry.
/// </summary>
public static class MasteryTokenExtensions
{
    /// <summary>
    /// Get mastery token count for a specific challenge type.
    /// Returns 0 if no entry exists.
    /// </summary>
    public static int GetMastery(this List<MasteryTokenEntry> tokens, string challengeTypeId)
    {
        return tokens.FirstOrDefault(t => t.ChallengeTypeId == challengeTypeId)?.TokenCount ?? 0;
    }

    /// <summary>
    /// Add mastery tokens for a specific challenge type.
    /// Creates new entry if it doesn't exist.
    /// </summary>
    public static void AddMastery(this List<MasteryTokenEntry> tokens, string challengeTypeId, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(challengeTypeId)) return;

        MasteryTokenEntry entry = tokens.FirstOrDefault(t => t.ChallengeTypeId == challengeTypeId);
        if (entry != null)
        {
            entry.TokenCount += count;
        }
        else
        {
            tokens.Add(new MasteryTokenEntry
            {
                ChallengeTypeId = challengeTypeId,
                TokenCount = count
            });
        }
    }

    /// <summary>
    /// Set mastery token count for a specific challenge type (overwrite).
    /// Creates new entry if it doesn't exist.
    /// </summary>
    public static void SetMastery(this List<MasteryTokenEntry> tokens, string challengeTypeId, int count)
    {
        if (string.IsNullOrEmpty(challengeTypeId)) return;

        MasteryTokenEntry entry = tokens.FirstOrDefault(t => t.ChallengeTypeId == challengeTypeId);
        if (entry != null)
        {
            entry.TokenCount = count;
        }
        else
        {
            tokens.Add(new MasteryTokenEntry
            {
                ChallengeTypeId = challengeTypeId,
                TokenCount = count
            });
        }
    }
}
