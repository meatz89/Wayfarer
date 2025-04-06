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
        Duration = 1,
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
            new StrategicTag("Changing Light", Illumination.Shadowy),
            new StrategicTag("Open Road", Population.Quiet),
            new StrategicTag("Journey Hazards", Atmosphere.Tense),
            new StrategicTag("Varied Terrain", Physical.Hazardous)
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
            new StrategicTag("Forest Light", Illumination.Bright),
            new StrategicTag("Ancient Knowledge", Population.Isolated),
            new StrategicTag("Sacred Ground", Atmosphere.Formal),
            new StrategicTag("Wilderness Wisdom", Economic.Humble)
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
            new StrategicTag("Forest Cover", Illumination.Shadowy),
            new StrategicTag("Remote Trail", Population.Isolated),
            new StrategicTag("Imminent Danger", Atmosphere.Tense),
            new StrategicTag("Rough Terrain", Physical.Hazardous)
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
            new StrategicTag("Forest Light", Illumination.Shadowy),
            new StrategicTag("Hunting Party", Population.Quiet),
            new StrategicTag("Wilderness", Physical.Expansive),
            new StrategicTag("Survival Focus", Economic.Humble)
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
            new StrategicTag("Filtered Sunlight", Illumination.Shadowy),
            new StrategicTag("Sacred Tree", Population.Isolated),
            new StrategicTag("Natural Wonder", Atmosphere.Formal),
            new StrategicTag("Ancient Wisdom", Economic.Humble)
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
            new StrategicTag("Tavern Lighting", Illumination.Shadowy),
            new StrategicTag("Busy Evening", Population.Crowded),
            new StrategicTag("Merry Atmosphere", Atmosphere.Chaotic),
            new StrategicTag("Drinking Hall", Physical.Confined)
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
            new StrategicTag("Night Shadows", Illumination.Dark),
            new StrategicTag("Private Chamber", Population.Isolated),
            new StrategicTag("Rented Space", Economic.Commercial),
            new StrategicTag("Small Room", Physical.Confined)
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
            new StrategicTag("Dim Lighting", Illumination.Shadowy),
            new StrategicTag("Public Posting", Population.Crowded),
            new StrategicTag("Opportunity Board", Economic.Commercial),
            new StrategicTag("Tavern Wall", Physical.Confined)
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
            new StrategicTag("Hidden Room", Illumination.Dark),
            new StrategicTag("Secret Meeting", Population.Isolated),
            new StrategicTag("Tense Exchange", Atmosphere.Tense),
            new StrategicTag("Back Room", Physical.Confined)
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
            new StrategicTag("Market Daylight", Illumination.Bright),
            new StrategicTag("Bustling Shoppers", Population.Crowded),
            new StrategicTag("Trading Post", Economic.Commercial),
            new StrategicTag("Market Commotion", Atmosphere.Chaotic)
        ]
    };
}