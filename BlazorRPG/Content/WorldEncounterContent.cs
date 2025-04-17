public class WorldEncounterContent
{
    public static EncounterTemplate GetDefaultTemplate()
    {
        return new EncounterTemplate()
        {
            Name = EncounterNames.ForageForFood.ToString(),
            Duration = 4,
            MaxPressure = 12,
            PartialThreshold = 8,
            StandardThreshold = 12,
            ExceptionalThreshold = 16,
            Hostility = EncounterInfo.HostilityLevels.Neutral,

            EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.DistractingCommotion,
            NarrativeTagRepository.UnsteadyConditions
        },

            encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Forest Light", Illumination.Shadowy),
            new EnvironmentPropertyTag("Wildlife Presence", Population.Quiet),
            new EnvironmentPropertyTag("Uneven Ground", Physical.Hazardous)
        }
        };
    }

}