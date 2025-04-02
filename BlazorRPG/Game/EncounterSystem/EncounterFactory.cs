public static class EncounterFactory
{
    /// <summary>
    /// Creates the encounter for the given location
    /// </summary>
    public static EncounterInfo CreateEncounterFromTemplate(
        EncounterTemplate template,
        Location location,
        string locationSpot,
        EncounterTypes EncounterType)
    {
        EncounterInfo encounterInfo = new EncounterInfo(
            location.Name,
            locationSpot,
            template.PressureReducingFocuses,
            template.MomentumReducingFocuses,
            template.Duration,
            template.MaxPressure,
            template.PartialThreshold, template.StandardThreshold, template.ExceptionalThreshold, // Momentum thresholds: 12+ (Partial), 16+ (Standard), 20+ (Exceptional)
            template.Hostility,
            EncounterType);

        encounterInfo.SetDifficulty(location.Difficulty);

        foreach (NarrativeTag narrativeTag in template.EncounterNarrativeTags)
        {
            encounterInfo.AddTag(narrativeTag);
        }

        foreach (StrategicTag strategicTag in template.encounterStrategicTags)
        {
            encounterInfo.AddTag(strategicTag);
        }

        return encounterInfo;
    }
}
