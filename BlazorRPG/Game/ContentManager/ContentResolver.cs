/// <summary>
/// Resolves and validates cross-references (e.g., action IDs in spots) after registration.
/// </summary>
public static class ContentResolver
{
    /// <summary>
    /// Validate that all action IDs referenced by location spots exist.
    /// </summary>
    /// <returns>List of error messages for missing references.</returns>
    public static List<string> ResolveLocationSpotActions(GameContentRegistry registry)
    {
        List<string> errors = new List<string>();

        foreach (LocationSpot spot in registry.GetAllLocationSpots())
        {
            List<string> actionIds = GetBaseActionIds(spot);
            List<string> missing = new List<string>();

            // Validate each action ID
            foreach (string actionId in actionIds)
            {
                if (!string.IsNullOrEmpty(actionId) && !registry.TryGetAction(actionId, out _))
                {
                    missing.Add(actionId);
                }
            }

            // Add error messages for missing actions
            foreach (string id in missing)
            {
                errors.Add($"Location '{spot.LocationName}', Spot '{spot.Name}': Action '{id}' not found.");
            }
        }

        return errors;
    }

    /// <summary>
    /// Get all action IDs referenced by a location spot.
    /// </summary>
    private static List<string> GetBaseActionIds(LocationSpot spot)
    {
        List<string> list = new List<string>();

        foreach (SpotLevel level in spot.LevelData)
        {
            // Add regular actions
            foreach (string actionId in level.AddedActionIds)
            {
                if (!string.IsNullOrEmpty(actionId))
                {
                    list.Add(actionId);
                }
            }

            // Add encounter action if present
            if (!string.IsNullOrEmpty(level.EncounterActionId))
            {
                list.Add(level.EncounterActionId);
            }
        }

        return list;
    }

    /// <summary>
    /// Validate that all locations referenced by other locations exist.
    /// </summary>
    /// <returns>List of error messages for missing references.</returns>
    public static List<string> ResolveLocationConnections(GameContentRegistry registry)
    {
        List<string> errors = new List<string>();

        foreach (Location location in registry.GetAllLocations())
        {
            foreach (string connectedLocationId in location.ConnectedTo)
            {
                if (!string.IsNullOrEmpty(connectedLocationId) &&
                    !registry.TryGetLocation(connectedLocationId, out _))
                {
                    errors.Add($"Location '{location.Name}' references non-existent location '{connectedLocationId}'.");
                }
            }
        }

        return errors;
    }
}