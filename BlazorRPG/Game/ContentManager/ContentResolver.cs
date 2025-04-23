/// <summary>
/// Resolves and validates cross-references (e.g., action IDs in spots) after registration.
/// </summary>
public static class ContentResolver
{
    public static List<string> ResolveLocationSpotActions(ContentRegistry contentRegistry)
    {
        List<string> errors = new List<string>();
        foreach (Location loc in contentRegistry.GetAllOfType<Location>())
        {
            foreach (LocationSpot spot in loc.LocationSpots)
            {
                List<string> missing = contentRegistry.ValidateReferences<ActionDefinition>(spot.BaseActionIds);
                foreach (string id in missing)
                    errors.Add($"Location '{loc.Name}', Spot '{spot.Name}': Action '{id}' not found.");
            }
        }
        return errors;
    }
}
