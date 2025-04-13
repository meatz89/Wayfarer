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

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.TunnelVision,
            NarrativeTagRepository.Overthinking,
            NarrativeTagRepository.CautiousRestraint
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Changing Light", Illumination.Bright),
            new EnvironmentPropertyTag("Open Road", Population.Quiet),
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

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.ColdCalculation,
            NarrativeTagRepository.Overthinking
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Forest Light", Illumination.Bright),
            new EnvironmentPropertyTag("Ancient Knowledge", Population.Scholarly),
            new EnvironmentPropertyTag("Sacred Ground", Atmosphere.Formal),
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

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.TunnelVision,
            NarrativeTagRepository.HesitantPoliteness,
            NarrativeTagRepository.ParanoidMindset
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Forest Cover", Illumination.Shadowy),
            new EnvironmentPropertyTag("Remote Trail", Population.Scholarly),
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

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.DetailFixation,
            NarrativeTagRepository.Overthinking
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Filtered Sunlight", Illumination.Shadowy),
            new EnvironmentPropertyTag("Sacred Tree", Population.Scholarly),
            new EnvironmentPropertyTag("Natural Wonder", Atmosphere.Formal),
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

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.ParanoidMindset,
            NarrativeTagRepository.DetailFixation
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Night Shadows", Illumination.Dark),
            new EnvironmentPropertyTag("Private Chamber", Population.Scholarly),
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

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.Overthinking,
            NarrativeTagRepository.DetailFixation
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Dim Lighting", Illumination.Shadowy),
            new EnvironmentPropertyTag("Public Posting", Population.Crowded),
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

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.ColdCalculation,
            NarrativeTagRepository.ShadowVeil
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Hidden Room", Illumination.Dark),
            new EnvironmentPropertyTag("Secret Meeting", Population.Scholarly),
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

        EncounterNarrativeTags =
        [
            NarrativeTagRepository.SuperficialCharm,
            NarrativeTagRepository.ColdCalculation
        ],

        encounterStrategicTags =
        [
            new EnvironmentPropertyTag("Market Daylight", Illumination.Bright),
            new EnvironmentPropertyTag("Bustling Shoppers", Population.Crowded),
            new EnvironmentPropertyTag("Market Commotion", Atmosphere.Chaotic)
        ]
    };
}