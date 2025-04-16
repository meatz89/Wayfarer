public class WorldEncounterContent
{
    public static EncounterTemplate GetDefaultTemplate()
    {
        return  new EncounterTemplate()
            {
                Name = EncounterNames.ForageForFood.ToString(),
                Duration = 4,
                MaxPressure = 12,
                PartialThreshold = 8,  
                StandardThreshold = 12, 
                ExceptionalThreshold = 16, 
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
}


    public static List<EncounterTemplate> GetAllTemplates()
    {
        List<EncounterTemplate> encounterTemplates = new()
        {
            ForageForFoodEncounter,
            SearchSurroundingsEncounter,
            GatherHerbsEncounter,
            FindPathOutEncounter
        };

        return encounterTemplates;
    }

    // Basic resource gathering encounter
    public static EncounterTemplate HuntGameEncounter => new EncounterTemplate()
    {
        Name = EncounterNames.HuntGame.ToString(),
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

    public static EncounterTemplate NightWatchEncounter => new EncounterTemplate()
    {
        Name = EncounterNames.NightWatch.ToString(),
        Duration = 4,
        MaxPressure = 12,
        PartialThreshold = 8,  
        StandardThreshold = 12, 
        ExceptionalThreshold = 16, 
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

    public static EncounterTemplate ForageForFoodEncounter => new EncounterTemplate()
    {
        Name = EncounterNames.ForageForFood.ToString(),
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
        Name = EncounterNames.SearchSurroundings.ToString(),
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
        Name = EncounterNames.GatherHerbs.ToString(),
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

    // Final "boss" encounter
    public static EncounterTemplate FindPathOutEncounter => new EncounterTemplate()
    {
        Name = EncounterNames.FindPathOut.ToString(),
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