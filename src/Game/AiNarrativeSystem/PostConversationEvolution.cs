// STUB: Post-conversation evolution types for compilation
public class PostConversationEvolutionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}

public class PostConversationEvolutionInput
{
    public string ConversationId { get; set; }
    public string NpcId { get; set; }
    public string CharacterBackground { get; set; }
    public string CurrentLocation { get; set; }
    public string ConversationOutcome { get; set; }
    public int Health { get; set; }
    public int MaxHealth { get; set; }
    public int Stamina { get; set; }
    public int MaxStamina { get; set; }
    public List<string> KnownLocations { get; set; } = new List<string>();
    public List<string> ConnectedLocations { get; set; } = new List<string>();
    public List<string> CurrentLocationSpots { get; set; } = new List<string>();
    public List<string> KnownCharacters { get; set; } = new List<string>();
    public List<string> ActiveContracts { get; set; } = new List<string>();
}

public class ProposedChange
{
    public string ChangeType { get; set; }
    public string Description { get; set; }
}

public class LocationCreationInput
{
    public string LocationId { get; set; }
    public string Name { get; set; }
    public string CharacterArchetype { get; set; }
    public string TravelDestination { get; set; }
    public List<string> KnownLocations { get; set; } = new List<string>();
    public string TravelOrigin { get; set; }
    public List<string> KnownCharacters { get; set; } = new List<string>();
    public List<string> ActiveContracts { get; set; } = new List<string>();
}

public class MemoryConsolidationInput
{
    public string PlayerId { get; set; }
    public string OldMemory { get; set; }
}

public class WorldStateInput
{
    public string Description { get; set; }
    public string PlayerArchetype { get; set; }
    public int Stamina { get; set; }
    public int MaxStamina { get; set; }
    public int Coins { get; set; }
    public string CurrentLocation { get; set; }
    public int LocationDepth { get; set; }
    public string CurrentSpot { get; set; }
    public List<string> ConnectedLocations { get; set; } = new List<string>();
    public List<string> LocationSpots { get; set; } = new List<string>();
    public List<string> Inventory { get; set; } = new List<string>();
    public List<string> KnownCharacters { get; set; } = new List<string>();
    public List<string> ActiveContracts { get; set; } = new List<string>();
    public string MemorySummary { get; set; }
}

public class LocationDetails
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public PlayerLocationUpdate LocationUpdate { get; set; }
    public string History { get; set; }
    public List<string> PointsOfInterest { get; set; } = new List<string>();
    public int TravelTimeMinutes { get; set; }
    public string TravelDescription { get; set; }
    public List<string> ConnectedLocationIds { get; set; } = new List<string>();
    public List<SpotDetails> NewLocationSpots { get; set; } = new List<SpotDetails>();
    public List<string> StrategicTags { get; set; } = new List<string>();
    public List<string> NarrativeTags { get; set; } = new List<string>();
}

public class PlayerLocationUpdate
{
    public string LocationId { get; set; }
    public string Description { get; set; }
    public string NewLocationName { get; set; }
    public bool LocationChanged { get; set; }
}

public class SpotDetails
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string InteractionDescription { get; set; }
    public List<string> EnvironmentalProperties { get; set; } = new List<string>();
}

public class WorldStateInputBuilder
{
    public WorldStateInput Build()
    {
        return new WorldStateInput();
    }
    
    public WorldStateInput CreateWorldStateInput(object gameWorld, object player)
    {
        return new WorldStateInput();
    }
}

public class LocationCreationSystem
{
    public static async Task CreateLocation(string locationId)
    {
        // STUB: Location creation
        await Task.CompletedTask;
    }
    
    public static async Task CreateLocationSpot(string spotId)
    {
        // STUB: Location spot creation
        await Task.CompletedTask;
    }
}