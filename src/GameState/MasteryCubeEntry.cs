/// <summary>
/// Physical challenge mastery tracking - list-based, not dictionary.
/// Parallel to StoryCubes on NPC entity and InvestigationCubes on Location entity.
/// Each entry tracks mastery cubes earned for a specific physical challenge deck.
/// </summary>
public class MasteryCubeEntry
{
/// <summary>
/// References physical challenge deck ID (string).
/// Examples: "combat", "athletics", "finesse", "endurance", "strength"
/// </summary>
public string DeckId { get; set; }

/// <summary>
/// Number of mastery cubes earned for this challenge type (0-10 scale).
/// Earned through repeated successful completions.
/// Reduces Physical Danger threshold for this specific deck type.
/// </summary>
public int CubeCount { get; set; }
}

/// <summary>
/// Extension methods for List<MasteryCubeEntry> to make lookups easy.
/// Parallel pattern to StoryCubes and InvestigationCubes.
/// </summary>
public static class MasteryCubeExtensions
{
/// <summary>
/// Get mastery cube count for a specific challenge deck.
/// Returns 0 if no entry exists.
/// </summary>
public static int GetMastery(this List<MasteryCubeEntry> cubes, string deckId)
{
    MasteryCubeEntry entry = cubes.FirstOrDefault(t => t.DeckId == deckId);
    if (entry == null)
        return 0;
    return entry.CubeCount;
}

/// <summary>
/// Add mastery cubes for a specific challenge deck (max 10).
/// Creates new entry if it doesn't exist.
/// </summary>
public static void AddMastery(this List<MasteryCubeEntry> cubes, string deckId, int count)
{
    if (count <= 0 || string.IsNullOrEmpty(deckId)) return;

    MasteryCubeEntry entry = cubes.FirstOrDefault(t => t.DeckId == deckId);
    if (entry != null)
    {
        entry.CubeCount = System.Math.Min(10, entry.CubeCount + count);
    }
    else
    {
        cubes.Add(new MasteryCubeEntry
        {
            DeckId = deckId,
            CubeCount = System.Math.Min(10, count)
        });
    }
}

/// <summary>
/// Set mastery cube count for a specific challenge deck (overwrite, max 10).
/// Creates new entry if it doesn't exist.
/// </summary>
public static void SetMastery(this List<MasteryCubeEntry> cubes, string deckId, int count)
{
    if (string.IsNullOrEmpty(deckId)) return;

    int clampedCount = System.Math.Min(10, System.Math.Max(0, count));

    MasteryCubeEntry entry = cubes.FirstOrDefault(t => t.DeckId == deckId);
    if (entry != null)
    {
        entry.CubeCount = clampedCount;
    }
    else
    {
        cubes.Add(new MasteryCubeEntry
        {
            DeckId = deckId,
            CubeCount = clampedCount
        });
    }
}
}
