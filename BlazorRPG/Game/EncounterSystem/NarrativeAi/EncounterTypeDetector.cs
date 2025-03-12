
// Handles encounter type detection
using BlazorRPG.Game.EncounterManager;

public class EncounterTypeDetector
{
    public EncounterTypes DetermineEncounterType(string location, EncounterStatus state)
    {
        // Determine encounter type based on location and dominant approach tags
        if (location.Contains("Tavern") || location.Contains("Market") || location.Contains("Inn") ||
            location.Contains("Village") || location.Contains("Court"))
        {
            // Consider more social locations or high rapport/dominance as social
            if (state.ApproachTags.TryGetValue(ApproachTags.Rapport, out int rapport) &&
                rapport > state.ApproachTags.Values.Where(v => v != rapport).DefaultIfEmpty(0).Max())
            {
                return EncounterTypes.Social;
            }
        }

        // Consider high analysis as intellectual
        if (state.ApproachTags.TryGetValue(ApproachTags.Analysis, out int analysis) &&
            analysis > state.ApproachTags.Values.Where(v => v != analysis).DefaultIfEmpty(0).Max())
        {
            return EncounterTypes.Intellectual;
        }

        // Consider high dominance, precision, or concealment as physical
        if (state.ApproachTags.TryGetValue(ApproachTags.Dominance, out int dominance) &&
            dominance > state.ApproachTags.Values.Where(v => v != dominance).DefaultIfEmpty(0).Max())
        {
            return EncounterTypes.Physical;
        }

        if (state.ApproachTags.TryGetValue(ApproachTags.Precision, out int precision) &&
            precision > state.ApproachTags.Values.Where(v => v != precision).DefaultIfEmpty(0).Max())
        {
            return EncounterTypes.Physical;
        }

        if (state.ApproachTags.TryGetValue(ApproachTags.Concealment, out int concealment) &&
            concealment > state.ApproachTags.Values.Where(v => v != concealment).DefaultIfEmpty(0).Max())
        {
            return EncounterTypes.Physical;
        }

        // Default to social if no clear pattern
        return EncounterTypes.Social;
    }
}
