public class WorldEncounterContent
{
    public static EncounterTemplate GetDefaultTemplate()
    {
        return new EncounterTemplate()
        {
            Name = "ForageForFood",
            Duration = 4,
            MaxPressure = 12,
            PartialThreshold = 8,
            StandardThreshold = 12,
            ExceptionalThreshold = 16,
            Hostility = Encounter.HostilityLevels.Neutral,

            EncounterNarrativeTags = new List<NarrativeTag>
            {
                NarrativeTagRepository.DistractingCommotion,
                NarrativeTagRepository.UnsteadyConditions
            },

        };
    }

}