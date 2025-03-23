public class EncounterContent
{
    // VILLAGE ENCOUNTERS
    public static EncounterTemplate VillageSquareEncounter => new EncounterTemplate()
    {
        Duration = 4,
        MaxPressure = 10,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,

        Hostility = EncounterInfo.HostilityLevels.Friendly,

        MomentumBoostApproaches = new[] { ApproachTags.Rapport, ApproachTags.Analysis }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Dominance }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Relationship, FocusTags.Information }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Resource }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.IntimidatingPresence,
            NarrativeTagRepository.SocialAwkwardness
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Morning Sun", Illumination.Bright),
            new StrategicTag("Village Gathering", Population.Crowded),
            new StrategicTag("Community Space", Atmosphere.Formal),
            new StrategicTag("Open Plaza", Physical.Expansive)
        ]
    };

    public static EncounterTemplate ElderEncounter => new EncounterTemplate()
    {
        Duration = 5,
        MaxPressure = 10,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Friendly,

        MomentumBoostApproaches = new[] { ApproachTags.Analysis }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Dominance }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Information }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Physical }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.DetailFixation,
            NarrativeTagRepository.BattleRage
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Dim Cottage", Illumination.Shadowy),
            new StrategicTag("Private Home", Population.Quiet),
            new StrategicTag("Respected Elder", Atmosphere.Formal),
            new StrategicTag("Modest Dwelling", Economic.Humble)
        ]
    };

    public static EncounterTemplate WellEncounter => new EncounterTemplate()
    {
        Duration = 4,
        MaxPressure = 8,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        MomentumBoostApproaches = new[] { ApproachTags.Rapport }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Dominance }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Relationship }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Resource }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.ShadowVeil,
            NarrativeTagRepository.IntimidatingPresence
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Open Air", Illumination.Bright),
            new StrategicTag("Social Gathering", Population.Crowded),
            new StrategicTag("Communal Space", Atmosphere.Chaotic),
            new StrategicTag("Water Source", Physical.Confined)
        ]
    };

    // FOREST ENCOUNTERS
    public static EncounterTemplate BanditEncounter => new EncounterTemplate()
    {
        Duration = 6,
        MaxPressure = 13,
        PartialThreshold = 12,
        StandardThreshold = 16,
        ExceptionalThreshold = 20,

        Hostility = EncounterInfo.HostilityLevels.Hostile,

        MomentumBoostApproaches = new[] { ApproachTags.Analysis, ApproachTags.Dominance }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Rapport }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Physical, FocusTags.Environment }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.TunnelVision,
            NarrativeTagRepository.HesitantPoliteness,
            NarrativeTagRepository.ParanoidMindset
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Forest Cover", Illumination.Shadowy),
            new StrategicTag("Remote Trail", Population.Isolated),
            new StrategicTag("Imminent Danger", Atmosphere.Tense),
            new StrategicTag("Rough Terrain", Physical.Hazardous)
        ]
    };

    public static EncounterTemplate HuntingEncounter => new EncounterTemplate()
    {
        Duration = 5,
        MaxPressure = 12,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        MomentumBoostApproaches = new[] { ApproachTags.Analysis }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Rapport }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Physical, FocusTags.Environment }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.HesitantPoliteness,
            NarrativeTagRepository.PublicAwareness
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Forest Light", Illumination.Shadowy),
            new StrategicTag("Hunting Party", Population.Quiet),
            new StrategicTag("Wilderness", Physical.Expansive),
            new StrategicTag("Survival Focus", Economic.Humble)
        ]
    };

    public static EncounterTemplate HerbalismEncounter => new EncounterTemplate()
    {
        Duration = 4,
        MaxPressure = 10,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,

        Hostility = EncounterInfo.HostilityLevels.Friendly,

        MomentumBoostApproaches = new[] { ApproachTags.Analysis }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Dominance }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Information, FocusTags.Environment }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.DetailFixation,
            NarrativeTagRepository.Overthinking
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Filtered Sunlight", Illumination.Shadowy),
            new StrategicTag("Sacred Tree", Population.Isolated),
            new StrategicTag("Natural Wonder", Atmosphere.Formal),
            new StrategicTag("Ancient Wisdom", Economic.Humble)
        ]
    };

    public static EncounterTemplate SecretMeetingEncounter => new EncounterTemplate()
    {
        Duration = 5,
        MaxPressure = 12,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        MomentumBoostApproaches = new[] { ApproachTags.Analysis, ApproachTags.Dominance }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Rapport }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Information }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Environment, FocusTags.Physical }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.ShadowVeil,
            NarrativeTagRepository.ParanoidMindset
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Hidden Place", Illumination.Shadowy),
            new StrategicTag("Secret Meeting", Population.Isolated),
            new StrategicTag("Cautious Exchange", Atmosphere.Tense),
            new StrategicTag("Natural Cover", Physical.Confined)
        ]
    };

    // TAVERN ENCOUNTERS
    public static EncounterTemplate TavernGossipEncounter => new EncounterTemplate()
    {
        Duration = 4,
        MaxPressure = 10,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,

        Hostility = EncounterInfo.HostilityLevels.Friendly,

        MomentumBoostApproaches = new[] { ApproachTags.Rapport }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Analysis, ApproachTags.Dominance }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Relationship }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Information, FocusTags.Physical }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.ColdCalculation,
            NarrativeTagRepository.IntimidatingPresence
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Tavern Lighting", Illumination.Shadowy),
            new StrategicTag("Busy Evening", Population.Crowded),
            new StrategicTag("Merry Atmosphere", Atmosphere.Chaotic),
            new StrategicTag("Drinking Hall", Physical.Confined)
        ]
    };

    public static EncounterTemplate ShadyDealEncounter => new EncounterTemplate()
    {
        Duration = 4,
        MaxPressure = 12,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        MomentumBoostApproaches = new[] { ApproachTags.Dominance, ApproachTags.Analysis }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Rapport }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Resource, FocusTags.Environment }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Physical, FocusTags.Relationship }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.SuperficialCharm,
            NarrativeTagRepository.ParanoidMindset
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Dark Corner", Illumination.Dark),
            new StrategicTag("Private Booth", Population.Isolated),
            new StrategicTag("Illicit Trade", Atmosphere.Tense),
            new StrategicTag("Confined Space", Physical.Confined)
        ]
    };

    public static EncounterTemplate InnRoomEncounter => new EncounterTemplate()
    {
        Duration = 3,
        MaxPressure = 8,
        PartialThreshold = 6,
        StandardThreshold = 10,
        ExceptionalThreshold = 14,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        MomentumBoostApproaches = new[] { ApproachTags.Analysis }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Rapport, ApproachTags.Dominance }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Environment }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.ParanoidMindset,
            NarrativeTagRepository.DetailFixation
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Night Shadows", Illumination.Dark),
            new StrategicTag("Private Chamber", Population.Isolated),
            new StrategicTag("Rented Space", Economic.Commercial),
            new StrategicTag("Small Room", Physical.Confined)
        ]
    };

    public static EncounterTemplate QuestBoardEncounter => new EncounterTemplate()
    {
        Duration = 4,
        MaxPressure = 8,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,

        Hostility = EncounterInfo.HostilityLevels.Friendly,

        MomentumBoostApproaches = new[] { ApproachTags.Analysis }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Dominance }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Information }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship, FocusTags.Physical }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.Overthinking,
            NarrativeTagRepository.DetailFixation
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Dim Lighting", Illumination.Shadowy),
            new StrategicTag("Public Posting", Population.Crowded),
            new StrategicTag("Opportunity Board", Economic.Commercial),
            new StrategicTag("Tavern Wall", Physical.Confined)
        ]
    };

    public static EncounterTemplate InformantEncounter => new EncounterTemplate()
    {
        Duration = 5,
        MaxPressure = 12,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        MomentumBoostApproaches = new[] { ApproachTags.Rapport, ApproachTags.Dominance }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Analysis }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Relationship, FocusTags.Resource }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Environment, FocusTags.Physical }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.ColdCalculation,
            NarrativeTagRepository.ShadowVeil
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Hidden Room", Illumination.Dark),
            new StrategicTag("Secret Meeting", Population.Isolated),
            new StrategicTag("Tense Exchange", Atmosphere.Tense),
            new StrategicTag("Back Room", Physical.Confined)
        ]
    };

    // MerchantEncounter is explicitly requested to keep in the code
    public static EncounterTemplate MerchantEncounter => new EncounterTemplate()
    {
        Duration = 5,
        MaxPressure = 10,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        MomentumBoostApproaches = new[] { ApproachTags.Rapport, ApproachTags.Analysis }.ToList(),
        DangerousApproaches = new[] { ApproachTags.Dominance }.ToList(),

        PressureReducingFocuses = new[] { FocusTags.Relationship, FocusTags.Resource }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Physical, FocusTags.Environment }.ToList(),

        encounterNarrativeTags =
        [
            NarrativeTagRepository.SuperficialCharm,
            NarrativeTagRepository.ColdCalculation
        ],

        encounterStrategicTags =
        [
            new StrategicTag("Market Daylight", Illumination.Bright),
            new StrategicTag("Bustling Shoppers", Population.Crowded),
            new StrategicTag("Trading Post", Economic.Commercial),
            new StrategicTag("Market Commotion", Atmosphere.Chaotic)
        ]
    };
}