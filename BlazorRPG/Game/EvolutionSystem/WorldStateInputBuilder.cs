using System.Text;

public class WorldStateInputBuilder
{
    private readonly GameState gameState;
    public LocationSystem LocationSystem { get; }
    public CharacterSystem CharacterSystem { get; }
    public OpportunitySystem OpportunitySystem { get; }

    public WorldStateInputBuilder(
        GameState gameState,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem opportunitySystem)
    {
        this.gameState = gameState;
        LocationSystem = locationSystem;
        CharacterSystem = characterSystem;
        OpportunitySystem = opportunitySystem;
    }

    public async Task<WorldStateInput> CreateWorldStateInput(string currentLocation)
    {
        WorldState worldState = gameState.WorldState;
        PlayerState playerState = gameState.PlayerState;

        // Create context for location generation
        WorldStateInput context = new WorldStateInput
        {
            CharacterArchetype = playerState.Archetype.ToString(),

            Health = playerState.Health,
            MaxHealth = playerState.MaxHealth,
            Concentration = playerState.Concentration,
            MaxConcentration = playerState.MaxConcentration,
            Energy = playerState.CurrentEnergy(),
            MaxEnergy = playerState.MaxEnergyPoints,
            Coins = playerState.Coins,

            CurrentLocation = currentLocation,
            LocationSpots = LocationSystem.FormatLocationSpots(worldState.CurrentLocation),
            CurrentSpot = worldState.CurrentLocationSpot.Id,
            LocationDepth = worldState.CurrentLocation.Depth,
            ConnectedLocations = LocationSystem.FormatLocations(LocationSystem.GetConnectedLocations(worldState.CurrentLocation.Id)),

            Inventory = FormatPlayerInventory(playerState.Inventory),
            Relationships = playerState.Relationships.ToString(),

            KnownCharacters = CharacterSystem.FormatKnownCharacters(worldState.GetCharacters()),
            ActiveOpportunities = OpportunitySystem.FormatActiveOpportunities(worldState.GetOpportunities()),

            MemorySummary = await MemoryFileAccess.ReadFromMemoryFile(),
        };

        await MemoryFileAccess.WriteToLogFile(context);

        return context;
    }

    private string FormatPlayerInventory(Inventory inventory)
    {
        if (inventory == null || inventory.UsedCapacity == 0)
        {
            return "No significant items";
        }

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("Carrying:");

        // Group items by type and count them
        Dictionary<ItemTypes, int> itemCounts = new Dictionary<ItemTypes, int>();
        foreach (ItemTypes itemType in Enum.GetValues(typeof(ItemTypes)))
        {
            if (itemType != ItemTypes.None)
            {
                int count = inventory.GetItemCount(itemType.ToString());
                if (count > 0)
                {
                    itemCounts[itemType] = count;
                }
            }
        }

        // Format items with counts
        foreach (KeyValuePair<ItemTypes, int> item in itemCounts)
        {
            string itemName = GetItemName(item.Key);

            if (item.Value > 1)
            {
                sb.AppendLine($"- {itemName} ({item.Value})");
            }
            else
            {
                sb.AppendLine($"- {itemName}");
            }
        }

        return sb.ToString();
    }

    private string GetItemName(ItemTypes itemType)
    {
        // Convert enum to display name
        return SplitCamelCase(itemType.ToString());
    }

    public static string SplitCamelCase(string str)
    {
        return System.Text.RegularExpressions.Regex.Replace(
            str,
            "([A-Z])",
            " $1",
            System.Text.RegularExpressions.RegexOptions.Compiled).Trim();
    }

}