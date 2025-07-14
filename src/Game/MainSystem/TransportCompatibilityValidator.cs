using Wayfarer.Game.MainSystem;

namespace Wayfarer.Game.MainSystem
{
    /// <summary>
    /// Validates transport method compatibility with terrain, equipment, and item constraints.
    /// Implements the logical transport restrictions outlined in UserStories.md.
    /// </summary>
    public class TransportCompatibilityValidator
    {
        private readonly ItemRepository _itemRepository;

        public TransportCompatibilityValidator(ItemRepository itemRepository)
        {
            _itemRepository = itemRepository;
        }

        /// <summary>
        /// Check if a transport method is compatible with route terrain categories
        /// </summary>
        public TransportCompatibilityResult CheckTerrainCompatibility(TravelMethods transport, List<TerrainCategory> terrainCategories)
        {
            foreach (TerrainCategory terrain in terrainCategories)
            {
                // Cart transport restrictions
                if (transport == TravelMethods.Cart)
                {
                    if (terrain == TerrainCategory.Requires_Climbing)
                        return TransportCompatibilityResult.Blocked("Cart cannot navigate mountain terrain requiring climbing");

                    if (terrain == TerrainCategory.Wilderness_Terrain)
                        return TransportCompatibilityResult.Blocked("Cart cannot navigate rough wilderness terrain");
                }

                // Boat transport restrictions
                if (transport == TravelMethods.Boat)
                {
                    if (terrain != TerrainCategory.Requires_Water_Transport)
                        return TransportCompatibilityResult.Blocked("Boat transport only works on water routes");
                }

                // Walking/Horseback require water transport for water routes
                if (terrain == TerrainCategory.Requires_Water_Transport)
                {
                    if (transport != TravelMethods.Boat)
                        return TransportCompatibilityResult.Blocked("Water routes require boat transport");
                }
            }

            return TransportCompatibilityResult.Allowed();
        }

        /// <summary>
        /// Check if player's equipment is compatible with transport method
        /// </summary>
        public TransportCompatibilityResult CheckEquipmentCompatibility(TravelMethods transport, Player player)
        {
            List<EquipmentCategory> playerEquipment = GetPlayerEquipmentCategories(player);
            List<Item> playerItems = GetPlayerItems(player);

            // Check for heavy equipment restrictions
            bool hasHeavyItems = playerItems.Any(item => item.Size == SizeCategory.Large || item.Size == SizeCategory.Massive);

            switch (transport)
            {
                case TravelMethods.Boat:
                    if (hasHeavyItems)
                        return TransportCompatibilityResult.Blocked("Heavy equipment blocks boat transport - too much weight");
                    break;

                case TravelMethods.Carriage:
                    if (playerItems.Any(item => item.Size == SizeCategory.Massive))
                        return TransportCompatibilityResult.Blocked("Massive items cannot fit in carriage");
                    break;

                case TravelMethods.Horseback:
                    if (hasHeavyItems)
                        return TransportCompatibilityResult.Blocked("Heavy equipment incompatible with horseback travel");
                    break;
            }

            return TransportCompatibilityResult.Allowed();
        }

        /// <summary>
        /// Get comprehensive compatibility result for transport method with route and player state
        /// </summary>
        public TransportCompatibilityResult CheckFullCompatibility(TravelMethods transport, RouteOption route, Player player)
        {
            // Check terrain compatibility first
            TransportCompatibilityResult terrainResult = CheckTerrainCompatibility(transport, route.TerrainCategories);
            if (!terrainResult.IsCompatible)
                return terrainResult;

            // Check equipment compatibility
            TransportCompatibilityResult equipmentResult = CheckEquipmentCompatibility(transport, player);
            if (!equipmentResult.IsCompatible)
                return equipmentResult;

            return TransportCompatibilityResult.Allowed();
        }

        /// <summary>
        /// Get all equipment categories from player's inventory
        /// </summary>
        private List<EquipmentCategory> GetPlayerEquipmentCategories(Player player)
        {
            List<EquipmentCategory> categories = new List<EquipmentCategory>();

            foreach (string itemId in player.Inventory.ItemSlots)
            {
                if (itemId != null && itemId != string.Empty)
                {
                    Item item = _itemRepository.GetItemById(itemId);
                    if (item != null)
                    {
                        categories.AddRange(item.Categories);
                    }
                }
            }

            return categories.Distinct().ToList();
        }

        /// <summary>
        /// Get all items from player's inventory
        /// </summary>
        private List<Item> GetPlayerItems(Player player)
        {
            List<Item> items = new List<Item>();

            foreach (string itemId in player.Inventory.ItemSlots)
            {
                if (itemId != null && itemId != string.Empty)
                {
                    Item item = _itemRepository.GetItemById(itemId);
                    if (item != null)
                    {
                        items.Add(item);
                    }
                }
            }

            return items;
        }
    }

    /// <summary>
    /// Result of transport compatibility check
    /// </summary>
    public class TransportCompatibilityResult
    {
        public bool IsCompatible { get; private set; }
        public string BlockingReason { get; private set; }
        public List<string> Warnings { get; private set; }

        private TransportCompatibilityResult(bool isCompatible, string blockingReason = "", List<string> warnings = null)
        {
            IsCompatible = isCompatible;
            BlockingReason = blockingReason;
            Warnings = warnings ?? new List<string>();
        }

        public static TransportCompatibilityResult Allowed(List<string> warnings = null)
        {
            return new TransportCompatibilityResult(true, "", warnings);
        }

        public static TransportCompatibilityResult Blocked(string reason)
        {
            return new TransportCompatibilityResult(false, reason);
        }
    }
}