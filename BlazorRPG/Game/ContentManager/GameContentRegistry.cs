/// <summary>
/// Registry for game objects that handles specific types directly without generics.
/// </summary>
public class GameContentRegistry
{
    // Type-specific storage with case-insensitive string comparison
    private readonly Dictionary<string, Location> _locations = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, LocationSpot> _locationSpots = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, ActionDefinition> _actions = new(StringComparer.OrdinalIgnoreCase);

    #region Location Registration and Retrieval

    /// <summary>
    /// Register a location with the specified ID.
    /// </summary>
    /// <returns>True if registration was successful, false if ID already exists.</returns>
    public bool RegisterLocation(string id, Location location)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));

        if (_locations.ContainsKey(id))
            return false;

        _locations[id] = location;
        return true;
    }

    /// <summary>
    /// Try to get a location by ID.
    /// </summary>
    /// <returns>True if location was found, false otherwise.</returns>
    public bool TryGetLocation(string id, out Location location)
    {
        if (!string.IsNullOrWhiteSpace(id) && _locations.TryGetValue(id, out location))
            return true;

        location = null;
        return false;
    }

    /// <summary>
    /// Get a location by ID or throw an exception if not found.
    /// </summary>
    public Location GetLocation(string id)
    {
        if (TryGetLocation(id, out Location location))
            return location;

        throw new KeyNotFoundException($"Location with ID '{id}' not found.");
    }

    /// <summary>
    /// Get all registered locations.
    /// </summary>
    public List<Location> GetAllLocations()
    {
        return _locations.Values.ToList();
    }

    #endregion

    #region LocationSpot Registration and Retrieval

    /// <summary>
    /// Register a location spot with the specified ID.
    /// </summary>
    /// <returns>True if registration was successful, false if ID already exists.</returns>
    public bool RegisterLocationSpot(string id, LocationSpot locationSpot)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));

        if (_locationSpots.ContainsKey(id))
            return false;

        _locationSpots[id] = locationSpot;
        return true;
    }

    /// <summary>
    /// Try to get a location spot by ID.
    /// </summary>
    /// <returns>True if location spot was found, false otherwise.</returns>
    public bool TryGetLocationSpot(string id, out LocationSpot locationSpot)
    {
        if (!string.IsNullOrWhiteSpace(id) && _locationSpots.TryGetValue(id, out locationSpot))
            return true;

        locationSpot = null;
        return false;
    }

    /// <summary>
    /// Get a location spot by ID or throw an exception if not found.
    /// </summary>
    public LocationSpot GetLocationSpot(string id)
    {
        if (TryGetLocationSpot(id, out LocationSpot locationSpot))
            return locationSpot;

        throw new KeyNotFoundException($"LocationSpot with ID '{id}' not found.");
    }

    /// <summary>
    /// Get all registered location spots.
    /// </summary>
    public List<LocationSpot> GetAllLocationSpots()
    {
        return _locationSpots.Values.ToList();
    }

    /// <summary>
    /// Get all location spots for a specific location.
    /// </summary>
    public List<LocationSpot> GetLocationSpotsForLocation(string locationId)
    {
        return _locationSpots.Values
            .Where(spot =>
            {
                return spot.LocationName.Equals(locationId, StringComparison.OrdinalIgnoreCase);
            })
            .ToList();
    }

    #endregion

    #region Action Registration and Retrieval

    /// <summary>
    /// Register an action with the specified ID.
    /// </summary>
    /// <returns>True if registration was successful, false if ID already exists.</returns>
    public bool RegisterAction(string id, ActionDefinition action)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("ID cannot be null or whitespace.", nameof(id));

        if (_actions.ContainsKey(id))
            return false;

        _actions[id] = action;
        return true;
    }

    /// <summary>
    /// Try to get an action by ID.
    /// </summary>
    /// <returns>True if action was found, false otherwise.</returns>
    public bool TryGetAction(string id, out ActionDefinition action)
    {
        if (!string.IsNullOrWhiteSpace(id) && _actions.TryGetValue(id, out action))
            return true;

        action = null;
        return false;
    }

    /// <summary>
    /// Get an action by ID or throw an exception if not found.
    /// </summary>
    public ActionDefinition GetAction(string id)
    {
        if (TryGetAction(id, out ActionDefinition action))
            return action;

        throw new KeyNotFoundException($"Action with ID '{id}' not found.");
    }

    /// <summary>
    /// Get all registered actions.
    /// </summary>
    public List<ActionDefinition> GetAllActions()
    {
        return _actions.Values.ToList();
    }

    #endregion

    #region Validation Methods

    /// <summary>
    /// Validate that all location IDs in the list exist.
    /// </summary>
    /// <returns>List of missing location IDs.</returns>
    public List<string> ValidateLocationReferences(List<string> locationIds)
    {
        List<string> missing = new List<string>();
        foreach (string id in locationIds)
            if (!TryGetLocation(id, out _))
                missing.Add(id);
        return missing;
    }

    /// <summary>
    /// Validate that all location spot IDs in the list exist.
    /// </summary>
    /// <returns>List of missing location spot IDs.</returns>
    public List<string> ValidateLocationSpotReferences(List<string> spotIds)
    {
        List<string> missing = new List<string>();
        foreach (string id in spotIds)
            if (!TryGetLocationSpot(id, out _))
                missing.Add(id);
        return missing;
    }

    /// <summary>
    /// Validate that all action IDs in the list exist.
    /// </summary>
    /// <returns>List of missing action IDs.</returns>
    public List<string> ValidateActionReferences(List<string> actionIds)
    {
        List<string> missing = new List<string>();
        foreach (string id in actionIds)
            if (!TryGetAction(id, out _))
                missing.Add(id);
        return missing;
    }

    #endregion
}