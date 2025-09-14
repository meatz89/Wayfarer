using System;
using System.Linq;

/// <summary>
/// Handles all location state operations in an immutable, validated manner.
/// All location state changes must go through this class.
/// </summary>
public static class LocationStateOperations
{
    /// <summary>
    /// Marks a location as visited.
    /// </summary>
    public static LocationOperationResult VisitLocation(LocationState state)
    {
        if (state == null)
            return LocationOperationResult.Failure("Location state cannot be null");

        LocationState newState = state.WithVisit();
        return LocationOperationResult.Success(newState, $"Visited {state.Name} (visit #{newState.VisitCount})");
    }

    /// <summary>
    /// Updates player knowledge of a location.
    /// </summary>
    public static LocationOperationResult UpdatePlayerKnowledge(LocationState state, bool hasKnowledge)
    {
        if (state == null)
            return LocationOperationResult.Failure("Location state cannot be null");

        if (state.PlayerKnowledge == hasKnowledge)
            return LocationOperationResult.Success(state, "Player knowledge unchanged");

        LocationState newState = state.WithPlayerKnowledge(hasKnowledge);
        return LocationOperationResult.Success(newState,
            hasKnowledge ? $"Player learned about {state.Name}" : $"Player forgot about {state.Name}");
    }

    /// <summary>
    /// Adds a new connection to the location.
    /// </summary>
    public static LocationOperationResult AddConnection(LocationState state, LocationConnection connection)
    {
        if (state == null)
            return LocationOperationResult.Failure("Location state cannot be null");

        if (connection == null)
            return LocationOperationResult.Failure("Connection cannot be null");

        // Check if connection already exists
        if (state.Connections.Any(c => c.DestinationLocationId == connection.DestinationLocationId))
            return LocationOperationResult.Success(state, "Connection already exists");

        LocationState newState = state.WithAddedConnection(connection);
        return LocationOperationResult.Success(newState, $"Added connection from {state.Name} to {connection.DestinationLocationId}");
    }

    /// <summary>
    /// Validates if a location is accessible based on requirements.
    /// </summary>
    public static bool IsAccessible(LocationState state, Player playerState, int currentDay)
    {
        if (state?.AccessRequirement == null)
            return true;

        return false;
    }

    /// <summary>
    /// Gets available services for a specific time block.
    /// </summary>
    public static ServiceTypes[] GetAvailableServices(LocationState state, TimeBlocks timeBlock)
    {
        if (state == null)
            return Array.Empty<ServiceTypes>();

        // Services are available at all times unless restricted by other mechanics
        return state.AvailableServices.ToArray();
    }

    /// <summary>
    /// Gets environmental properties for a specific time block.
    /// </summary>
    public static string[] GetTimeProperties(LocationState state, TimeBlocks timeBlock)
    {
        if (state == null)
            return Array.Empty<string>();

        return timeBlock switch
        {
            TimeBlocks.Morning => state.MorningProperties.ToArray(),
            TimeBlocks.Midday => state.AfternoonProperties.ToArray(),
            TimeBlocks.Afternoon => state.EveningProperties.ToArray(),
            TimeBlocks.Evening => state.NightProperties.ToArray(),
            _ => Array.Empty<string>()
        };
    }
}

/// <summary>
/// Result of a location operation.
/// </summary>
public class LocationOperationResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public LocationState NewState { get; }

    private LocationOperationResult(bool success, string message, LocationState newState)
    {
        IsSuccess = success;
        Message = message;
        NewState = newState;
    }

    public static LocationOperationResult Failure(string message)
    {
        return new LocationOperationResult(false, message, null);
    }

    public static LocationOperationResult Success(LocationState newState, string message)
    {
        return new LocationOperationResult(true, message, newState);
    }
}