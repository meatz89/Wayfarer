using System.Text;

public class WorldStateInputBuilder
{
    private GameWorld gameWorld;
    public LocationSystem LocationSystem { get; }
    public CharacterSystem CharacterSystem { get; }
    public OpportunitySystem OpportunitySystem { get; }

    public WorldStateInputBuilder(
        GameWorld gameWorld,
        LocationSystem locationSystem,
        CharacterSystem characterSystem,
        OpportunitySystem OpportunitySystem)
    {
        this.gameWorld = gameWorld;
        LocationSystem = locationSystem;
        CharacterSystem = characterSystem;
        OpportunitySystem = OpportunitySystem;
    }

    public async Task<WorldStateInput> CreateWorldStateInput(string currentLocation)
    {
        WorldState worldState = gameWorld.WorldState;
        Player player = gameWorld.Player;

        // Create context for location generation
        string locationSpots = LocationSystem.FormatLocationSpots(worldState.CurrentLocation);
        string connectedLocations = LocationSystem.FormatLocations(LocationSystem.GetConnectedLocations(worldState.CurrentLocation.Id));
        string playerInventory = FormatPlayerInventory(player.Inventory);

        int currentEnergy = player.CurrentEnergy();
        string memoryContent = await MemoryFileAccess.ReadFromMemoryFile();

        string knownCharacters = CharacterSystem.FormatKnownCharacters(worldState.GetCharacters());
        List<NPC> allCharacters = worldState.GetCharacters();

        WorldStateInput context = new WorldStateInput
        {
            PlayerArchetype = player.Archetype.ToString(),

            Health = player.Health,
            MaxHealth = player.MaxHealth,
            Concentration = player.Concentration,
            MaxConcentration = player.MaxConcentration,
            Energy = currentEnergy,
            MaxEnergy = player.MaxEnergy,
            Coins = player.Money,

            CurrentLocation = currentLocation,
            LocationSpots = locationSpots,
            CurrentSpot = worldState.CurrentLocationSpot.SpotID,
            LocationDepth = worldState.CurrentLocation.Depth,
            ConnectedLocations = connectedLocations,

            Inventory = playerInventory,

            KnownCharacters = knownCharacters,
            ActiveOpportunities = string.Empty,

            MemorySummary = memoryContent,

            Characters = allCharacters,
            RelationshipList = player.Relationships
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