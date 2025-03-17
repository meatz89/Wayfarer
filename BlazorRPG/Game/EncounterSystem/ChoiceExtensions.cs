public static class ChoiceExtensions
{
    public static EncounterStateTags GetPrimaryApproach(this IChoice choice)
    {
        // Find the approach tag with the largest modification
        var approachMods = choice.TagModifications
            .Where(m => m.Type == TagModification.TagTypes.EncounterState)
            .Where(m => IsApproachTag((EncounterStateTags)m.Tag))
            .OrderByDescending(m => m.Delta)
            .ToList();

        if (approachMods.Any())
        {
            return (EncounterStateTags)approachMods.First().Tag;
        }

        // Default to Analysis if no approach is found (fallback)
        return EncounterStateTags.Analysis;
    }

    private static bool IsApproachTag(EncounterStateTags tag)
    {
        return tag == EncounterStateTags.Dominance ||
               tag == EncounterStateTags.Rapport ||
               tag == EncounterStateTags.Analysis ||
               tag == EncounterStateTags.Precision ||
               tag == EncounterStateTags.Concealment;
    }
}