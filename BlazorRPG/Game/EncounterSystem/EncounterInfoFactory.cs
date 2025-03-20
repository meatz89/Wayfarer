public static class EncounterInfoFactory
{
    /// <summary>
    /// Creates the encounter for the given location
    /// </summary>
    public static EncounterInfo CreateEncounter(
        LocationNames locationName,
        string locationSpot,
        EncounterTypes presentationStyle,
        EncounterTemplate template)
    {
        EncounterInfo encounterInfo = new EncounterInfo(
            locationName,
            locationSpot,
            template.MomentumBoostApproaches,
            template.DangerousApproaches,
            template.PressureReducingFocuses,
            template.MomentumReducingFocuses,
            template.Duration,
            template.MaxPressure,
            template.PartialThreshold, template.StandardThreshold, template.ExceptionalThreshold, // Momentum thresholds: 12+ (Partial), 16+ (Standard), 20+ (Exceptional)
            template.Hostility,
            presentationStyle);

        // Difficulty level 2 (adds +2 pressure per turn)
        encounterInfo.SetDifficulty(2);

        foreach(var narrativeTag in template.encounterNarrativeTags)
        {
            encounterInfo.AddTag(narrativeTag);
        }

        foreach (var strategicTag in template.encounterStrategicTags)
        {
            encounterInfo.AddTag(strategicTag);
        }

        return encounterInfo;
    }

}
