/// <summary>
/// Service for managing mastery cubes per challenge deck.
/// DOMAIN COLLECTION PRINCIPLE: ACCEPTABLE - DeckId is content-defined (variable collection), not fixed enum.
/// Different physical challenge decks (Combat, Athletics, Stealth, etc.) have independent mastery tracking.
/// </summary>
public class MasteryCubeService
{
    public int GetMastery(List<MasteryCubeEntry> cubes, string deckId)
    {
        MasteryCubeEntry entry = cubes.FirstOrDefault(t => t.DeckId == deckId);
        return entry?.CubeCount ?? 0;
    }

    public void AddMastery(List<MasteryCubeEntry> cubes, string deckId, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(deckId)) return;
        MasteryCubeEntry entry = cubes.FirstOrDefault(t => t.DeckId == deckId);
        if (entry != null)
            entry.CubeCount = Math.Min(10, entry.CubeCount + count);
        else
            cubes.Add(new MasteryCubeEntry { DeckId = deckId, CubeCount = Math.Min(10, count) });
    }

    public void SetMastery(List<MasteryCubeEntry> cubes, string deckId, int count)
    {
        if (string.IsNullOrEmpty(deckId)) return;
        int clampedCount = Math.Min(10, Math.Max(0, count));
        MasteryCubeEntry entry = cubes.FirstOrDefault(t => t.DeckId == deckId);
        if (entry != null)
            entry.CubeCount = clampedCount;
        else
            cubes.Add(new MasteryCubeEntry { DeckId = deckId, CubeCount = clampedCount });
    }
}
