using System.Text;

public class WorldStateInputBuilder
{
    private GameWorld gameWorld;
    public LocationSystem LocationSystem { get; }
    public CharacterSystem CharacterSystem { get; }

    public WorldStateInputBuilder(
        GameWorld gameWorld,
        LocationSystem locationSystem,
        CharacterSystem characterSystem)
    {
        this.gameWorld = gameWorld;
        LocationSystem = locationSystem;
        CharacterSystem = characterSystem;
    }

    public async Task<WorldStateInput> CreateWorldStateInput(string currentLocation)
    {
        WorldState worldState = gameWorld.WorldState;
        Player player = gameWorld.GetPlayer();

        // Create context for location generation
        string locationSpots = LocationSystem.FormatLocationSpots(worldState.CurrentLocation);
        string connectedLocations = LocationSystem.FormatLocations(LocationSystem.GetConnectedLocations(worldState.CurrentLocation.Id));
        string playerInventory = FormatPlayerInventory(player.Inventory);

        int currentStamina = player.Stamina;

        Guid gameInstanceId = gameWorld.GameInstanceId;
        MemoryFileAccess memoryFileAccess = new MemoryFileAccess(gameInstanceId);
        List<string> memoryContent = await memoryFileAccess.GetAllMemories();
        string memory = string.Join("\n", memoryContent.Where(x => !string.IsNullOrWhiteSpace(x)).Take(5));

        string knownCharacters = CharacterSystem.FormatKnownCharacters(worldState.GetCharacters());
        List<NPC> allCharacters = worldState.GetCharacters();

        WorldStateInput context = new WorldStateInput
        {
            PlayerArchetype = player.Archetype.ToString(),

            Health = player.Health,
            MaxHealth = player.MaxHealth,
            Concentration = player.Concentration,
            MaxConcentration = player.MaxConcentration,
            Stamina = currentStamina,
            MaxStamina = player.MaxStamina,
            Coins = player.Coins,

            CurrentLocation = currentLocation,
            LocationSpots = locationSpots,
            CurrentSpot = worldState.CurrentLocationSpot.SpotID,
            LocationDepth = worldState.CurrentLocation.Depth,
            ConnectedLocations = connectedLocations,

            Inventory = playerInventory,

            KnownCharacters = knownCharacters,
            ActiveContracts = string.Empty,

            MemorySummary = memory,

            Characters = allCharacters,
            RelationshipList = player.Relationships
        };

        await memoryFileAccess.WriteToLogFile(context);

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
        Dictionary<Item, int> itemCounts = new Dictionary<Item, int>();
        foreach (Item itemType in Enum.GetValues(typeof(Item)))
        {
            int count = inventory.GetItemCount(itemType.ToString());
            if (count > 0)
            {
                itemCounts[itemType] = count;
            }
        }

        // Format items with counts
        foreach (KeyValuePair<Item, int> item in itemCounts)
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

    private string GetItemName(Item itemType)
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