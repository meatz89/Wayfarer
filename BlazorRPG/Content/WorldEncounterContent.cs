public class WorldEncounterContent
{
    public static List<EncounterTemplate> GetAllTemplates()
    {
        List<EncounterTemplate> encounterTemplates = new() {
            VillageSquareEncounter,
            WellEncounter,
            BanditEncounter,
            HuntingEncounter,
            HerbalismEncounter,
            MerchantEncounter,
            InformantEncounter,
            QuestBoardEncounter,
            InnRoomEncounter,
            ShadyDealEncounter,
            TavernGossipEncounter
            };
        return encounterTemplates;
    }

    // VILLAGE ENCOUNTERS
    private static EncounterTemplate VillageSquareEncounter => new EncounterTemplate()
    {
        Name = "VillageSquare",
        Duration = 4,
        MaxPressure = 10,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,

        Hostility = EncounterInfo.HostilityLevels.Friendly,

        PressureReducingFocuses = new[] { FocusTags.Relationship, FocusTags.Information }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Resource }.ToList(),

        EncounterNarrativeTags =
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

    private static EncounterTemplate WellEncounter => new EncounterTemplate()
    {
        Name = "Well",
        Duration = 4,
        MaxPressure = 8,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        PressureReducingFocuses = new[] { FocusTags.Relationship }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Resource }.ToList(),

        EncounterNarrativeTags =
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
    private static EncounterTemplate BanditEncounter => new EncounterTemplate()
    {
        Name = "Bandit",
        Duration = 6,
        MaxPressure = 13,
        PartialThreshold = 12,
        StandardThreshold = 16,
        ExceptionalThreshold = 20,

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

    private static EncounterTemplate ShadyDealEncounter => new EncounterTemplate()
    {
        Name = "ShadyDeal",
        Duration = 4,
        MaxPressure = 12,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,

        Hostility = EncounterInfo.HostilityLevels.Neutral,

        PressureReducingFocuses = new[] { FocusTags.Resource, FocusTags.Environment }.ToList(),
        MomentumReducingFocuses = new[] { FocusTags.Physical, FocusTags.Relationship }.ToList(),

        EncounterNarrativeTags =
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

    // MerchantEncounter is explicitly requested to keep in the code
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