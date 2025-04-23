/// <summary>
/// Loads and registers all content into the contentRegistry.
/// </summary>
public static class ContentBootstrapper
{
    public static ContentRegistry InitializeRegistry()
    {
        ContentRegistry contentRegistry = new ContentRegistry();
        RegisterLocations(contentRegistry);
        RegisterActions(contentRegistry);

        return contentRegistry;
    }

    private static void RegisterLocations(ContentRegistry contentRegistry)
    {
        // Register Locations and their Spots
        foreach (Location loc in WorldLocationsContent.AllLocations)
        {
            contentRegistry.Register<Location>(loc.Name, loc);
            foreach (LocationSpot spot in loc.LocationSpots)
            {
                string spotId = $"{loc.Name}:{spot.Name}";
                contentRegistry.Register<LocationSpot>(spotId, spot);
            }
        }
    }

    private static void RegisterActions(ContentRegistry contentRegistry)
    {
        // Register Actions
        foreach (ActionDefinition action in WorldActionContent.GetAllTemplates())
        {
            contentRegistry.Register<ActionDefinition>(action.Id, action);
        }
    }
}