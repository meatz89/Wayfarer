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
            // Environment-based strategic tags with library-specific names
            new StrategicTag("Scholar's Light", Illumination.Bright),
            new StrategicTag("Hushed Atmosphere", Population.Quiet),
            new StrategicTag("Academic Decorum", Atmosphere.Formal),
            new StrategicTag("Narrow Bookshelves", Physical.Confined)
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
            // Environment-based strategic tags with bandit-specific names
            new StrategicTag("Lurking Shadows", Illumination.Shadowy),
            new StrategicTag("Secluded Ambush", Population.Isolated),
            new StrategicTag("Deadly Threat", Atmosphere.Tense),
            new StrategicTag("Treacherous Ground", Physical.Hazardous)
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
            // Environment-based strategic tags with merchant-specific names
            new StrategicTag("Market Daylight", Illumination.Bright),
            new StrategicTag("Bustling Shoppers", Population.Crowded),
            new StrategicTag("Trading Post", Economic.Commercial),
            new StrategicTag("Market Commotion", Atmosphere.Chaotic)
        ]
    };
}