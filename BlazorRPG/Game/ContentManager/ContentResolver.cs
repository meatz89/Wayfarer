/// <summary>
/// Resolves and validates cross-references (e.g., action IDs in spots) after registration.
/// </summary>
public static class ContentResolver
{
    public static List<string> ResolveLocationSpotActions(ContentRegistry contentRegistry)
    {
        List<string> errors = new List<string>();
        foreach (LocationSpot spot in contentRegistry.GetAllOfType<LocationSpot>())
        {
            List<string> missing = contentRegistry.ValidateReferences<ActionDefinition>(GetBaseActionIds(spot));
            foreach (string id in missing)
                errors.Add($"Location '{spot.LocationName}', Spot '{spot.Name}': Action '{id}' not found.");
        }
        return errors;
    }

    private static List<string> GetBaseActionIds(LocationSpot spot)
    {
        List<string> list = new List<string>();

        foreach (SpotLevel level in spot.SpotLevels)
        {
            level.AddedActionIds.ForEach(id =>
            {
                list.Add(id);
            });
            list.Add(level.EncounterActionId);
        }

        return list;
    }
}
