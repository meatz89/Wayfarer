using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Physical challenge mastery tracking - list-based, not dictionary.
/// Parallel to NPCTokenEntry for Social system and FamiliarityEntry for Mental system.
/// Each entry tracks mastery tokens earned for a specific physical challenge deck.
/// </summary>
public class MasteryTokenEntry
{
    /// <summary>
    /// References physical challenge deck ID (string).
    /// Examples: "combat", "athletics", "finesse", "endurance", "strength"
    /// </summary>
    public string DeckId { get; set; }

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
    /// Get mastery token count for a specific challenge deck.
    /// Returns 0 if no entry exists.
    /// </summary>
    public static int GetMastery(this List<MasteryTokenEntry> tokens, string deckId)
    {
        MasteryTokenEntry entry = tokens.FirstOrDefault(t => t.DeckId == deckId);
        if (entry == null)
            return 0;
        return entry.TokenCount;
    }

    /// <summary>
    /// Add mastery tokens for a specific challenge deck.
    /// Creates new entry if it doesn't exist.
    /// </summary>
    public static void AddMastery(this List<MasteryTokenEntry> tokens, string deckId, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(deckId)) return;

        MasteryTokenEntry entry = tokens.FirstOrDefault(t => t.DeckId == deckId);
        if (entry != null)
        {
            entry.TokenCount += count;
        }
        else
        {
            tokens.Add(new MasteryTokenEntry
            {
                DeckId = deckId,
                TokenCount = count
            });
        }
    }

    /// <summary>
    /// Set mastery token count for a specific challenge deck (overwrite).
    /// Creates new entry if it doesn't exist.
    /// </summary>
    public static void SetMastery(this List<MasteryTokenEntry> tokens, string deckId, int count)
    {
        if (string.IsNullOrEmpty(deckId)) return;

        MasteryTokenEntry entry = tokens.FirstOrDefault(t => t.DeckId == deckId);
        if (entry != null)
        {
            entry.TokenCount = count;
        }
        else
        {
            tokens.Add(new MasteryTokenEntry
            {
                DeckId = deckId,
                TokenCount = count
            });
        }
    }
}
