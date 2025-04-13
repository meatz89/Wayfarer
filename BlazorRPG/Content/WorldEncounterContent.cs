public class WorldEncounterContent
{
    public static List<EncounterTemplate> GetAllTemplates()
    {
        List<EncounterTemplate> encounterTemplates = new() {
            TravelEncounter,
        };

        return encounterTemplates;
    }

    public static EncounterTemplate TravelEncounter => new EncounterTemplate()
    {
        Name = "Travel",
        Duration = 2,
        MaxPressure = 10,
        PartialThreshold = 4,
        StandardThreshold = 8,
        ExceptionalThreshold = 12,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        PressureReducingFocuses = new[] { FocusTags.Environment, FocusTags.Physical }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship }.ToList(),

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.TunnelVision,
            NarrativeTagRepository.Overthinking,
            NarrativeTagRepository.CautiousRestraint
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Dangerous Analysis", DangerousApproach.Analysis),
            new EnvironmentPropertyTag("Changing Light", Illumination.Shadowy),
            new EnvironmentPropertyTag("Open Road", Population.Quiet),
            new EnvironmentPropertyTag("Journey Hazards", Atmosphere.Tense),
            new EnvironmentPropertyTag("Varied Terrain", Physical.Hazardous)
        ]
    };

    // FOREST ENCOUNTERS
    public static EncounterTemplate HermitEncounter => new EncounterTemplate()
    {
        Name = "HermitEncounter",
        Duration = 4,
        MaxPressure = 10,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        PressureReducingFocuses = new[] { FocusTags.Relationship, FocusTags.Information }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Physical, FocusTags.Resource }.ToList(),

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.ColdCalculation,
            NarrativeTagRepository.Overthinking
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Forest Light", Illumination.Bright),
            new EnvironmentPropertyTag("Ancient Knowledge", Population.Isolated),
            new EnvironmentPropertyTag("Sacred Ground", Atmosphere.Formal),
            new EnvironmentPropertyTag("Wilderness Wisdom", Economic.Humble)
        ]
    };

    private static EncounterTemplate BanditEncounter => new EncounterTemplate()
    {
        Name = "Bandit",
        Duration = 4,
        MaxPressure = 13,
        PartialThreshold = 10,
        StandardThreshold = 12,
        ExceptionalThreshold = 14,

        Hostility = EncounterInfo.HostilityLevels.Hostile,

        PressureReducingFocuses = new[] { FocusTags.Physical, FocusTags.Environment }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship }.ToList(),

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.TunnelVision,
            NarrativeTagRepository.HesitantPoliteness,
            NarrativeTagRepository.ParanoidMindset
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Forest Cover", Illumination.Shadowy),
            new EnvironmentPropertyTag("Remote Trail", Population.Isolated),
            new EnvironmentPropertyTag("Imminent Danger", Atmosphere.Tense),
            new EnvironmentPropertyTag("Rough Terrain", Physical.Hazardous)
        ]
    };

    private static EncounterTemplate HuntingEncounter => new EncounterTemplate()
    {
        Name = "Hunting",
        Duration = 5,
        MaxPressure = 12,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        PressureReducingFocuses = new[] { FocusTags.Physical, FocusTags.Environment }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship }.ToList(),

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.HesitantPoliteness,
            NarrativeTagRepository.PublicAwareness
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Forest Light", Illumination.Shadowy),
            new EnvironmentPropertyTag("Hunting Party", Population.Quiet),
            new EnvironmentPropertyTag("Wilderness", Physical.Expansive),
            new EnvironmentPropertyTag("Survival Focus", Economic.Humble)
        ]
    };

    private static EncounterTemplate HerbalismEncounter => new EncounterTemplate()
    {
        Name = "Herbalism",
        Duration = 4,
        MaxPressure = 10,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,

        Hostility = EncounterInfo.HostilityLevels.Friendly,

        PressureReducingFocuses = new[] { FocusTags.Information, FocusTags.Environment }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship }.ToList(),

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.DetailFixation,
            NarrativeTagRepository.Overthinking
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Filtered Sunlight", Illumination.Shadowy),
            new EnvironmentPropertyTag("Sacred Tree", Population.Isolated),
            new EnvironmentPropertyTag("Natural Wonder", Atmosphere.Formal),
            new EnvironmentPropertyTag("Ancient Wisdom", Economic.Humble)
        ]
    };


    // TAVERN ENCOUNTERS
    private static EncounterTemplate TavernGossipEncounter => new EncounterTemplate()
    {
        Name = "TavernGossip",
        Duration = 4,
        MaxPressure = 10,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,

        Hostility = EncounterInfo.HostilityLevels.Friendly,

        PressureReducingFocuses = new[] { FocusTags.Relationship }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Information, FocusTags.Physical }.ToList(),

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.ColdCalculation,
            NarrativeTagRepository.IntimidatingPresence
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Tavern Lighting", Illumination.Shadowy),
            new EnvironmentPropertyTag("Busy Evening", Population.Crowded),
            new EnvironmentPropertyTag("Merry Atmosphere", Atmosphere.Chaotic),
            new EnvironmentPropertyTag("Drinking Hall", Physical.Confined)
        ]
    };

    private static EncounterTemplate InnRoomEncounter => new EncounterTemplate()
    {
        Name = "InnRoom",
        Duration = 3,
        MaxPressure = 8,
        PartialThreshold = 6,
        StandardThreshold = 10,
        ExceptionalThreshold = 14,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        PressureReducingFocuses = new[] { FocusTags.Environment }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship }.ToList(),

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.ParanoidMindset,
            NarrativeTagRepository.DetailFixation
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Night Shadows", Illumination.Dark),
            new EnvironmentPropertyTag("Private Chamber", Population.Isolated),
            new EnvironmentPropertyTag("Rented Space", Economic.Commercial),
            new EnvironmentPropertyTag("Small Room", Physical.Confined)
        ]
    };

    private static EncounterTemplate QuestBoardEncounter => new EncounterTemplate()
    {
        Name = "QuestBoard",
        Duration = 4,
        MaxPressure = 8,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,

        Hostility = EncounterInfo.HostilityLevels.Friendly,

        PressureReducingFocuses = new[] { FocusTags.Information }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Relationship, FocusTags.Physical }.ToList(),

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.Overthinking,
            NarrativeTagRepository.DetailFixation
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Dim Lighting", Illumination.Shadowy),
            new EnvironmentPropertyTag("Public Posting", Population.Crowded),
            new EnvironmentPropertyTag("Opportunity Board", Economic.Commercial),
            new EnvironmentPropertyTag("Tavern Wall", Physical.Confined)
        ]
    };

    private static EncounterTemplate InformantEncounter => new EncounterTemplate()
    {
        Name = "Informant",
        Duration = 5,
        MaxPressure = 12,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        PressureReducingFocuses = new[] { FocusTags.Relationship, FocusTags.Resource }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Environment, FocusTags.Physical }.ToList(),

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.ColdCalculation,
            NarrativeTagRepository.ShadowVeil
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Hidden Room", Illumination.Dark),
            new EnvironmentPropertyTag("Secret Meeting", Population.Isolated),
            new EnvironmentPropertyTag("Tense Exchange", Atmosphere.Tense),
            new EnvironmentPropertyTag("Back Room", Physical.Confined)
        ]
    };

    private static EncounterTemplate MerchantEncounter => new EncounterTemplate()
    {
        Name = "Merchant",
        Duration = 5,
        MaxPressure = 10,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        PressureReducingFocuses = new[] { FocusTags.Relationship, FocusTags.Resource }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Physical, FocusTags.Environment }.ToList(),

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.SuperficialCharm,
            NarrativeTagRepository.ColdCalculation
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Market Daylight", Illumination.Bright),
            new EnvironmentPropertyTag("Bustling Shoppers", Population.Crowded),
            new EnvironmentPropertyTag("Trading Post", Economic.Commercial),
            new EnvironmentPropertyTag("Market Commotion", Atmosphere.Chaotic)
        ]
    };
}