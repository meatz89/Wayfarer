public static class GameContentBootstrapper
{
    public static GameContentRegistry InitializeRegistry(string contentDirectory)
    {
        GameContentRegistry registry = new GameContentRegistry();
        ContentLoader contentLoader = new ContentLoader(contentDirectory);

        // 1) Load and register location spots first
        foreach (LocationSpot spot in contentLoader.LoadLocationSpots())
        {
            string spotId = $"{spot.LocationName}:{spot.Name}";
            registry.RegisterLocationSpot(spotId, spot);
        }

        // 2) Load and register locations (only those with spots)
        List<LocationSpot> allSpots = registry.GetAllLocationSpots();
        HashSet<string> locationsWithSpots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (LocationSpot spot in allSpots)
            locationsWithSpots.Add(spot.LocationName);

        foreach (Location location in contentLoader.LoadLocations())
        {
            if (!locationsWithSpots.Contains(location.Name))
            {
                Console.WriteLine($"Warning: Location '{location.Name}' has no registered spots. Skipping registration.");
                continue;
            }

            registry.RegisterLocation(location.Name, location);
        }

        // 3) Load and register actions
        foreach (ActionDefinition action in contentLoader.LoadActions())
        {
            registry.RegisterAction(action.Id, action);
        }

        return registry;
    }

    public static EncounterTemplate GetDefaultEncounterTemplate()
    {
        return new EncounterTemplate()
        {
            Name = "Default Template",
            Duration = 4,
            MaxPressure = 12,
            PartialThreshold = 8,
            StandardThreshold = 12,
            ExceptionalThreshold = 16,
            Hostility = Encounter.HostilityLevels.Neutral,

            EncounterNarrativeTags = new List<NarrativeTag>
            {
                NarrativeTagRepository.DistractingCommotion,
                NarrativeTagRepository.UnsteadyConditions
            },
        };
    }
}