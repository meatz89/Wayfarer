public class EncounterContent
{
    public static EncounterTemplate LibraryEncounter => new EncounterTemplate()
    {
        Duration = 6,
        MaxPressure = 13,
        PartialThreshold = 12,
        StandardThreshold = 16,
        ExceptionalThreshold = 20,

        Hostility = EncounterInfo.HostilityLevels.Friendly,

        MomentumBoostApproaches = new[] { ApproachTags.Analysis, ApproachTags.Rapport }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Dominance, ApproachTags.Evasion }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Environment, FocusTags.Information }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Physical, FocusTags.Resource }.ToList(),

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
                                                                            // Note: Evasion approach has no strategic tag (neutral)
        ]
    };

    public static EncounterTemplate BanditEncounter => new EncounterTemplate()
    {
        Duration = 6,
        MaxPressure = 13,
        PartialThreshold = 12,
        StandardThreshold = 16,
        ExceptionalThreshold = 20,

        Hostility = EncounterInfo.HostilityLevels.Hostile,

        MomentumBoostApproaches = new[] { ApproachTags.Analysis, ApproachTags.Precision }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Dominance, ApproachTags.Evasion }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Physical }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Environment }.ToList(),

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
        MaxPressure = 13,
        PartialThreshold = 12,
        StandardThreshold = 16,
        ExceptionalThreshold = 20,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        MomentumBoostApproaches = new[] { ApproachTags.Rapport, ApproachTags.Analysis }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Evasion, ApproachTags.Precision }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Relationship, FocusTags.Resource }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Physical, FocusTags.Environment }.ToList(),

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
