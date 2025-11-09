/// <summary>
/// Handles all NPC state operations in an immutable, validated manner.
/// All NPC state changes must go through this class.
/// </summary>
public static class NPCStateOperations
{
    /// <summary>
    /// Updates the player's relationship with an NPC.
    /// </summary>
    public static NPCOperationResult UpdateRelationship(NPCState state, NPCRelationship newRelationship)
    {
        if (state == null)
            return NPCOperationResult.Failure("NPC state cannot be null");

        if (state.PlayerRelationship == newRelationship)
            return NPCOperationResult.Success(state, "Relationship unchanged");

        NPCState newState = state.WithRelationship(newRelationship);
        return NPCOperationResult.Success(newState,
            $"Relationship with {state.Name} changed from {state.PlayerRelationship} to {newRelationship}");
    }

    /// <summary>
    /// Moves an NPC to a new location.
    /// </summary>
    public static NPCOperationResult MoveToLocation(NPCState state, string LocationId)
    {
        if (state == null)
            return NPCOperationResult.Failure("NPC state cannot be null");

        if (string.IsNullOrWhiteSpace(LocationId))
            return NPCOperationResult.Failure("Location ID cannot be empty");

        if (state.LocationId == LocationId)
            return NPCOperationResult.Success(state, "NPC already at this location");

        NPCState newState = state.WithLocation(LocationId);
        return NPCOperationResult.Success(newState,
            $"Moved {state.Name} to location {LocationId}");
    }
    /// <summary>
    /// Validates if an NPC is available at a specific time.
    /// </summary>
    public static bool IsAvailable(NPCState state, TimeBlocks timeBlock)
    {
        // NPCs are always available by design
        return state != null;
    }

    /// <summary>
    /// Validates if player has sufficient relationship with NPC for an action.
    /// </summary>
    public static bool HasSufficientRelationship(NPCState state, NPCRelationship requiredRelationship)
    {
        if (state == null)
            return false;

        // Convert to numeric values for comparison
        int currentLevel = GetRelationshipLevel(state.PlayerRelationship);
        int requiredLevel = GetRelationshipLevel(requiredRelationship);

        return currentLevel >= requiredLevel;
    }

    /// <summary>
    /// Gets numeric level for relationship comparison.
    /// </summary>
    private static int GetRelationshipLevel(NPCRelationship relationship)
    {
        return relationship switch
        {
            NPCRelationship.Betrayed => -3,    // Worst relationship state
            NPCRelationship.Hostile => -2,
            NPCRelationship.Unfriendly => -1,
            NPCRelationship.Wary => -1,
            NPCRelationship.Neutral => 0,
            NPCRelationship.Helpful => 1,
            NPCRelationship.Friendly => 1,
            NPCRelationship.Allied => 2,
            _ => 0
        };
    }
}

/// <summary>
/// Result of an NPC operation.
/// </summary>
public class NPCOperationResult
{
    public bool IsSuccess { get; }
    public string Message { get; }
    public NPCState NewState { get; }

    private NPCOperationResult(bool success, string message, NPCState newState)
    {
        IsSuccess = success;
        Message = message;
        NewState = newState;
    }

    public static NPCOperationResult Failure(string message)
    {
        return new NPCOperationResult(false, message, null);
    }

    public static NPCOperationResult Success(NPCState newState, string message)
    {
        return new NPCOperationResult(true, message, newState);
    }
}