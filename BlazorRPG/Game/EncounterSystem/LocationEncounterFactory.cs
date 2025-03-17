/// <summary>
/// Factory for creating common location types
/// </summary>
public static class LocationEncounterFactory
{
    // Other location methods remain unchanged

    /// <summary>
    /// Creates the Ancient Library location from the Wayfarer example
    /// </summary>
    public static LocationEncounterInfo CreateAncientLibraryEncounter(LocationNames locationName)
    {
        // Core properties for the Ancient Library
        LocationEncounterInfo location = new LocationEncounterInfo(
            locationName,
            new[] { ApproachTags.Analysis, ApproachTags.Precision }.ToList(),
            new[] { ApproachTags.Dominance }.ToList(),
            6, // Duration (6 turns)
            12, 16, 20, // Momentum thresholds: 12+ (Partial), 16+ (Standard), 20+ (Exceptional)
            LocationEncounterInfo.HostilityLevels.Neutral,
            PresentationStyles.Intellectual);

        // Difficulty level 2 (adds +2 pressure per turn)
        location.SetDifficulty(2);

        // Add narrative tags
        location.AddTag(NarrativeTagRepository.DetailFixation);
        location.AddTag(NarrativeTagRepository.TheoreticalMindset);
        location.AddTag(NarrativeTagRepository.ParanoidMindset);

        // Add strategic tags
        location.AddTag(StrategicTagRepository.InsightfulApproach);    // Analysis → Increases Momentum
        location.AddTag(StrategicTagRepository.CarefulPositioning);    // Precision → Decreases Pressure
        location.AddTag(StrategicTagRepository.EscalatingTension);     // Dominance → Increases Pressure
        location.AddTag(StrategicTagRepository.SocialDistraction);     // Rapport → Decreases Momentum
                                                                       // Note: Concealment approach has no strategic tag (neutral)

        return location;
    }

}