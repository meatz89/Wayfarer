public class WorldEncounterContent
{
    public static List<EncounterTemplate> GetAllTemplates()
    {
        List<EncounterTemplate> encounterTemplates = new()
        {
            ForageForFoodEncounter,
            SearchSurroundingsEncounter,
            GatherHerbsEncounter,
            FindPathOutEncounter,
            ClimbTreeEncounter,
            MoveStealthilyEncounter,
        };

        return encounterTemplates;
    }

    // Basic resource gathering encounter
    public static EncounterTemplate ForageForFoodEncounter => new EncounterTemplate()
    {
        Name = "ForageForFood",
        Duration = 4,
        MaxPressure = 12,
        PartialThreshold = 8,   // 1 Food
        StandardThreshold = 12, // 3 Food
        ExceptionalThreshold = 16, // 6 Food
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.DistractingCommotion,
            NarrativeTagRepository.UnsteadyConditions
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Forest Light", Illumination.Shadowy),
            new EnvironmentPropertyTag("Wildlife Presence", Population.Quiet),
            new EnvironmentPropertyTag("Uneven Ground", Physical.Hazardous)
        }
    };

    // Exploration encounter
    public static EncounterTemplate SearchSurroundingsEncounter => new EncounterTemplate()
    {
        Name = "SearchSurroundings",
        Duration = 3,
        MaxPressure = 10,
        PartialThreshold = 6,
        StandardThreshold = 10,
        ExceptionalThreshold = 14,
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.DisorderedAmbience
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Filtered Light", Illumination.Shadowy),
            new EnvironmentPropertyTag("Isolated Area", Population.Quiet),
            new EnvironmentPropertyTag("Wilderness", Physical.Expansive)
        }
    };

    // Medicinal herb gathering encounter
    public static EncounterTemplate GatherHerbsEncounter => new EncounterTemplate()
    {
        Name = "GatherHerbs",
        Duration = 4,
        MaxPressure = 12,
        PartialThreshold = 8,   // 1 Herb
        StandardThreshold = 12, // 3 Herbs
        ExceptionalThreshold = 16, // 6 Herbs
        Hostility = EncounterInfo.HostilityLevels.Friendly,

        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.LucidConcentration,
            NarrativeTagRepository.UnsteadyConditions
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Dappled Sunlight", Illumination.Shadowy),
            new EnvironmentPropertyTag("Nature's Quiet", Population.Quiet),
            new EnvironmentPropertyTag("Varied Terrain", Physical.Hazardous)
        }
    };

    // Tree climbing vantage point encounter
    public static EncounterTemplate ClimbTreeEncounter => new EncounterTemplate()
    {
        Name = "ClimbTree",
        Duration = 3,
        MaxPressure = 14,
        PartialThreshold = 10,
        StandardThreshold = 14,
        ExceptionalThreshold = 18,
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.UnsteadyConditions,
            NarrativeTagRepository.HarmoniousOrder
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Canopy Light", Illumination.Shadowy),
            new EnvironmentPropertyTag("Solitary Climb", Population.Quiet),
            new EnvironmentPropertyTag("Vertical Challenge", Physical.Hazardous)
        }
    };

    // Stealth movement encounter
    public static EncounterTemplate MoveStealthilyEncounter => new EncounterTemplate()
    {
        Name = "MoveStealthily",
        Duration = 4,
        MaxPressure = 12,
        PartialThreshold = 8,
        StandardThreshold = 12,
        ExceptionalThreshold = 16,
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.FluidMovement,
            NarrativeTagRepository.DisorderedAmbience
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Deep Shadows", Illumination.Dark),
            new EnvironmentPropertyTag("Hidden Movement", Population.Quiet),
            new EnvironmentPropertyTag("Forest Obstacles", Physical.Confined)
        }
    };

    // Final "boss" encounter
    public static EncounterTemplate FindPathOutEncounter => new EncounterTemplate()
    {
        Name = "FindPathOut",
        Duration = 6,
        MaxPressure = 18,
        PartialThreshold = 12,
        StandardThreshold = 18,
        ExceptionalThreshold = 24,
        Hostility = EncounterInfo.HostilityLevels.Neutral,

        EncounterNarrativeTags = new List<NarrativeTag>
        {
            NarrativeTagRepository.DistractingCommotion,
            NarrativeTagRepository.DisorderedAmbience,
            NarrativeTagRepository.UnsteadyConditions
        },

        encounterStrategicTags = new List<EnvironmentPropertyTag>
        {
            new EnvironmentPropertyTag("Fading Light", Illumination.Shadowy),
            new EnvironmentPropertyTag("Wilderness Edge", Population.Quiet),
            new EnvironmentPropertyTag("Challenging Terrain", Physical.Hazardous),
            new EnvironmentPropertyTag("Mounting Pressure", Atmosphere.Rough)
        }
    };
}