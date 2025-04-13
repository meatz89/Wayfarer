public class WorldEncounterContent
{
    public static List<EncounterTemplate> GetAllTemplates()
    {
        List<EncounterTemplate> encounterTemplates = new()
        {
            TravelEncounter,
        };

        return encounterTemplates;
    }

    public static EncounterTemplate TravelEncounter => new EncounterTemplate()
    {
        Name = "Travel",
        Duration = 12,
        MaxPressure = 40,
        PartialThreshold = 44,
        StandardThreshold = 48,
        ExceptionalThreshold = 52,
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        // For a traveler, shifting, sometimes uneasy conditions prevail.
        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.DistractingCommotion,  // Disrupts mental clarity (+1 Information)
            NarrativeTagRepository.UnsteadyConditions,      // Rough physical conditions (+1 Physical)
            NarrativeTagRepository.StrainedInteraction      // Social exchanges become less fluid (+1 Relationship)
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Changing Light", Illumination.Bright),
            new EnvironmentPropertyTag("Open Road", Population.Quiet),
            new EnvironmentPropertyTag("Varied Terrain", Physical.Hazardous)
        }
    };

    public static EncounterTemplate HermitEncounter => new EncounterTemplate()
    {
        Name = "Hermit Encounter",
        Duration = 4,
        MaxPressure = 10,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        // A solitary hermit's mindset brings clarity and calm order.
        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.LucidConcentration,  // Clear, focused mind (-2 Information)
            NarrativeTagRepository.HarmoniousOrder       // Order and quiet ease environmental demands (-1 Environment)
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Gentle Glow", Illumination.Bright),
            new EnvironmentPropertyTag("Quiet Study", Population.Scholarly),
            new EnvironmentPropertyTag("Calm Expanse", Atmosphere.Formal)
        }
    };

    public static EncounterTemplate BanditEncounter => new EncounterTemplate()
    {
        Name = "Bandit Ambush",
        Duration = 4,
        MaxPressure = 13,
        PartialThreshold = 10,
        StandardThreshold = 12,
        ExceptionalThreshold = 14,
        Hostility = EncounterInfo.HostilityLevels.Hostile,

        // Bandit encounters are marked by hostile and chaotic conditions.
        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.StrainedInteraction, // Social tension complicates communication (+1 Relationship)
            NarrativeTagRepository.UnsteadyConditions,     // Rough, unpredictable physical conditions (+1 Physical)
            NarrativeTagRepository.DisorderedAmbience      // Chaotic environment increases perceptive demands (+2 Environment)
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Dappled Shadows", Illumination.Shadowy),
            new EnvironmentPropertyTag("Hidden Paths", Population.Scholarly),
            new EnvironmentPropertyTag("Rocky Ground", Physical.Hazardous)
        }
    };

    public static EncounterTemplate HuntingEncounter => new EncounterTemplate()
    {
        Name = "Hunting",
        Duration = 5,
        MaxPressure = 12,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        // In the hunt, calm focus and smooth physical coordination are essential.
        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.LucidConcentration,  // Heightened mental clarity (-2 Information)
            NarrativeTagRepository.FluidMovement          // Ease in physical motion (-1 Physical)
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Subdued Twilight", Illumination.Shadowy),
            new EnvironmentPropertyTag("Stealthy Assembly", Population.Quiet),
            new EnvironmentPropertyTag("Expansive Wilds", Physical.Expansive)
        }
    };

    public static EncounterTemplate HerbalismEncounter => new EncounterTemplate()
    {
        Name = "Herbalism",
        Duration = 4,
        MaxPressure = 10,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,
        Hostility = EncounterInfo.HostilityLevels.Friendly,

        // The practice of herbalism benefits from mental clarity and an ease in resource handling.
        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.LucidConcentration,  // Focus on subtle details (-2 Information)
            NarrativeTagRepository.PlentifulProvisions      // An impression of abundance eases resource tasks (-1 Resource)
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Filtered Sunlight", Illumination.Shadowy),
            new EnvironmentPropertyTag("Ancient Lore", Population.Scholarly),
            new EnvironmentPropertyTag("Nature's Order", Atmosphere.Formal)
        }
    };

    public static EncounterTemplate TavernGossipEncounter => new EncounterTemplate()
    {
        Name = "Tavern Gossip",
        Duration = 4,
        MaxPressure = 10,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,
        Hostility = EncounterInfo.HostilityLevels.Friendly,

        // In a lively tavern, friendly banter mixes with a disruptive ambience.
        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.AffableManner,       // Warm and easy social atmosphere (-1 Relationship)
            NarrativeTagRepository.DisorderedAmbience     // Chaotic surroundings increase perceptive demands (+2 Environment)
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Mellow Glow", Illumination.Shadowy),
            new EnvironmentPropertyTag("Bustling Crowd", Population.Crowded),
            new EnvironmentPropertyTag("Rowdy Air", Atmosphere.Chaotic)
        }
    };

    public static EncounterTemplate InnRoomEncounter => new EncounterTemplate()
    {
        Name = "Inn Room",
        Duration = 3,
        MaxPressure = 8,
        PartialThreshold = 6,
        StandardThreshold = 10,
        ExceptionalThreshold = 14,
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        // A private inn room offers a mix of quiet focus and underlying tension.
        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.LucidConcentration,  // Enhances detailed focus (-2 Information)
            NarrativeTagRepository.UnsteadyConditions       // Underlying anxiety increases physical demands (+1 Physical)
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Deep Shadows", Illumination.Dark),
            new EnvironmentPropertyTag("Quiet Nook", Population.Scholarly),
            new EnvironmentPropertyTag("Cozy Quarters", Physical.Confined)
        }
    };

    public static EncounterTemplate QuestBoardEncounter => new EncounterTemplate()
    {
        Name = "Quest Board",
        Duration = 4,
        MaxPressure = 8,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,
        Hostility = EncounterInfo.HostilityLevels.Friendly,

        // The quest board is a hub of competing information and social demands.
        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.DistractingCommotion,  // Competing chatter disrupts focus (+1 Information)
            NarrativeTagRepository.StrainedInteraction      // The social scene complicates connection (+1 Relationship)
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Faint Glow", Illumination.Shadowy),
            new EnvironmentPropertyTag("Busy Posting", Population.Crowded),
            new EnvironmentPropertyTag("Narrow Wall", Physical.Confined)
        }
    };

    public static EncounterTemplate InformantEncounter => new EncounterTemplate()
    {
        Name = "Informant",
        Duration = 5,
        MaxPressure = 12,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        // Interactions with informants require clear perception and wary social exchanges.
        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.LucidConcentration,   // Clear, discerning mind (-2 Information)
            NarrativeTagRepository.StrainedInteraction     // A guarded social dynamic complicates trust (+1 Relationship)
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Muted Shadows", Illumination.Dark),
            new EnvironmentPropertyTag("Quiet Confab", Population.Scholarly),
            new EnvironmentPropertyTag("Small Backroom", Physical.Confined)
        }
    };

    public static EncounterTemplate MerchantEncounter => new EncounterTemplate()
    {
        Name = "Merchant",
        Duration = 5,
        MaxPressure = 10,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        // Merchant interactions benefit from charm and a sense of abundance.
        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.AffableManner,         // Easy, congenial social atmosphere (-1 Relationship)
            NarrativeTagRepository.PlentifulProvisions      // An impression of abundance eases trade (-1 Resource)
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Vivid Light", Illumination.Bright),
            new EnvironmentPropertyTag("Lively Crowd", Population.Crowded),
            new EnvironmentPropertyTag("Dynamic Exchange", Atmosphere.Chaotic)
        }
    };
}
