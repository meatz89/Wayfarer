public static class ContentBootstrapper
{
    public static ContentRegistry InitializeRegistry()
    {
        var contentRegistry = new ContentRegistry();

        // 1) register spots first
        RegisterLocationSpots(contentRegistry);

        // 2) then register only those locations that actually have spots
        RegisterLocations(contentRegistry);

        RegisterActions(contentRegistry);
        return contentRegistry;
    }

    private static void RegisterLocationSpots(ContentRegistry contentRegistry)
    {
        foreach (var locSpot in WorldLocationsContent.AllLocationSpots)
        {
            var spotId = $"{locSpot.LocationName}:{locSpot.Name}";
            contentRegistry.Register<LocationSpot>(spotId, locSpot);
        }
    }

    private static void RegisterLocations(ContentRegistry contentRegistry)
    {
        // Build a set of all location names that have at least one spot
        var locationsWithSpots = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var spot in contentRegistry.GetAllOfType<LocationSpot>())
            locationsWithSpots.Add(spot.LocationName);

        foreach (var loc in WorldLocationsContent.AllLocations)
        {
            if (!locationsWithSpots.Contains(loc.Name))
                throw new InvalidOperationException(
                    $"Cannot register Location '{loc.Name}' – no LocationSpot has been registered for it.");

            contentRegistry.Register<Location>(loc.Name, loc);
        }
    }

    private static void RegisterActions(ContentRegistry contentRegistry)
    {
        foreach (var action in WorldActionContent.GetAllTemplates())
        {
            contentRegistry.Register<ActionDefinition>(action.Name, action);
        }
    }
}
