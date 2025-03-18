public class EncounterContent
{
    public static EncounterTemplate LibraryEncounter => new EncounterTemplate()
    {
        Duration = 6,
        PartialThreshold = 12,
        StandardThreshold = 16,
        ExceptionalThreshold = 20,

        hostility = EncounterInfo.HostilityLevels.Friendly,

        favoredApproaches = new[] { ApproachTags.Analysis, ApproachTags.Precision }.ToList(),
        disfavoredApproaches = new[] { ApproachTags.Dominance }.ToList(),
        
        favoredFocuses = new[] { FocusTags.Relationship, FocusTags.Resource }.ToList(),
        disfavoredFocuses = new[] { FocusTags.Physical }.ToList(),

        encounterNarrativeTags =
        [
            // Add narrative tags
            NarrativeTagRepository.DetailFixation,
            NarrativeTagRepository.TheoreticalMindset,
            NarrativeTagRepository.ParanoidMindset,
        ],

        encounterStrategicTags =
        [
            // Add strategic tags
            StrategicTagRepository.InsightfulApproach,    // Analysis → Increases Momentum
            StrategicTagRepository.CarefulPositioning,    // Precision → Decreases Pressure
            StrategicTagRepository.EscalatingTension,     // Dominance → Increases Pressure
            StrategicTagRepository.SocialDistraction,     // Rapport → Decreases Momentum
                                                                            // Note: Concealment approach has no strategic tag (neutral)
        ]
    };

    public static EncounterTemplate BanditEncounter => new EncounterTemplate()
    {
        Duration = 6,
        PartialThreshold = 12,
        StandardThreshold = 16,
        ExceptionalThreshold = 20,

        hostility = EncounterInfo.HostilityLevels.Hostile,

        favoredApproaches = new[] { ApproachTags.Concealment, ApproachTags.Precision }.ToList(),
        disfavoredApproaches = new[] { ApproachTags.Dominance }.ToList(),

        // New properties - Physical actions risk injury in a fight
        favoredFocuses = new[] { FocusTags.Environment, FocusTags.Information }.ToList(),
        disfavoredFocuses = new[] { FocusTags.Physical }.ToList(),

        encounterNarrativeTags =
        [
            // Add narrative tags
            NarrativeTagRepository.TunnelVision,
            NarrativeTagRepository.PerfectionistParalysis,
            NarrativeTagRepository.SocialAwkwardness,
        ],

        encounterStrategicTags =
        [
            // Add strategic tags
            StrategicTagRepository.TacticalAdvantage,
            StrategicTagRepository.CarefulPositioning,
            StrategicTagRepository.EscalatingTension,
            StrategicTagRepository.SocialDistraction,
        ]
    };

    public static EncounterTemplate MerchantEncounter => new EncounterTemplate()
    {
        Duration = 6,
        PartialThreshold = 12,
        StandardThreshold = 16,
        ExceptionalThreshold = 20,

        hostility = EncounterInfo.HostilityLevels.Neutral,

        favoredApproaches = new[] { ApproachTags.Rapport, ApproachTags.Analysis }.ToList(),
        disfavoredApproaches = new[] { ApproachTags.Concealment }.ToList(),

        // New properties - Physical actions might be seen as threatening
        favoredFocuses = new[] { FocusTags.Relationship, FocusTags.Resource }.ToList(),
        disfavoredFocuses = new[] { FocusTags.Physical }.ToList(),

        encounterNarrativeTags =
        [
            // Add narrative tags
            NarrativeTagRepository.SuperficialCharm,
            NarrativeTagRepository.ColdCalculation,
            NarrativeTagRepository.ShadowVeil,
        ],

        encounterStrategicTags =
        [
            // Add strategic tags
            StrategicTagRepository.SocialCurrency,
            StrategicTagRepository.CalculatedResponse,
            StrategicTagRepository.RigidMethodology,
            StrategicTagRepository.SuspiciousBehavior,
        ]
    };
}
