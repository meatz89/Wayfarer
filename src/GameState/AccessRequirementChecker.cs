using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Service for checking if a player meets access requirements for locations, spots, and routes.
/// </summary>
public class AccessRequirementChecker
{
    private readonly GameWorld _gameWorld;
    private readonly ItemRepository _itemRepository;
    private readonly TokenMechanicsManager _tokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;

    public AccessRequirementChecker(
        GameWorld gameWorld,
        ItemRepository itemRepository,
        TokenMechanicsManager tokenManager,
        NPCRepository npcRepository,
        MessageSystem messageSystem)
    {
        _gameWorld = gameWorld;
        _itemRepository = itemRepository;
        _tokenManager = tokenManager;
        _npcRepository = npcRepository;
        _messageSystem = messageSystem;
    }

    /// <summary>
    /// Check if player meets access requirements for a location.
    /// </summary>
    public AccessCheckResult CheckLocationAccess(Location location)
    {
        if (location.AccessRequirement == null)
            return AccessCheckResult.Allowed();

        return CheckRequirements(location.AccessRequirement);
    }

    /// <summary>
    /// Check if player meets access requirements for a location spot.
    /// </summary>
    public AccessCheckResult CheckLocationSpotAccess(LocationSpot spot)
    {
        if (spot.AccessRequirement == null)
            return AccessCheckResult.Allowed();

        return CheckRequirements(spot.AccessRequirement);
    }

    /// <summary>
    /// Alias for CheckLocationSpotAccess for cleaner API.
    /// </summary>
    public AccessCheckResult CheckSpotAccess(LocationSpot spot)
    {
        return CheckLocationSpotAccess(spot);
    }

    /// <summary>
    /// Check if player meets access requirements for a route (in addition to terrain checks).
    /// </summary>
    public AccessCheckResult CheckRouteAccess(RouteOption route)
    {
        if (route.AccessRequirement == null)
            return AccessCheckResult.Allowed();

        return CheckRequirements(route.AccessRequirement);
    }

    /// <summary>
    /// Core requirement checking logic.
    /// </summary>
    private AccessCheckResult CheckRequirements(AccessRequirement requirement)
    {
        Player player = _gameWorld.GetPlayer();
        List<string> missingRequirements = new List<string>();

        // First check the triple-gate system
        bool tierMet = CheckTierRequirement(requirement, player, missingRequirements);

        // If information gate or tier gate fails, block access immediately
        if (!tierMet)
        {
            return AccessCheckResult.Blocked(
                requirement.BlockedMessage,
                requirement.HintMessage,
                missingRequirements
            );
        }

        // Then check permission and capability gates
        bool equipmentMet = CheckEquipmentRequirements(requirement, player, missingRequirements);
        bool itemsMet = CheckItemRequirements(requirement, player, missingRequirements);
        bool npcTokensMet = CheckNPCTokenRequirements(requirement, missingRequirements);
        bool typeTokensMet = CheckTypeTokenRequirements(requirement, missingRequirements);
        bool requirementsMet = requirement.Logic == RequirementLogic.And
            ? equipmentMet && itemsMet && npcTokensMet && typeTokensMet
            : equipmentMet || itemsMet || npcTokensMet || typeTokensMet;

        if (requirementsMet)
        {
            return AccessCheckResult.Allowed();
        }

        return AccessCheckResult.Blocked(
            requirement.BlockedMessage,
            requirement.HintMessage,
            missingRequirements
        );
    }

    /// <summary>
    /// Check if player has required equipment categories.
    /// </summary>
    private bool CheckEquipmentRequirements(AccessRequirement requirement, Player player, List<string> missing)
    {
        if (!requirement.RequiredEquipment.Any())
            return true;

        List<ItemCategory> playerEquipment = GetPlayerEquipmentCategories(player);

        if (requirement.Logic == RequirementLogic.Or)
        {
            // Need ANY of the required equipment
            bool hasAny = requirement.RequiredEquipment.Any(req => playerEquipment.Contains(req));
            if (!hasAny)
            {
                missing.Add($"Need one of: {string.Join(", ", requirement.RequiredEquipment.Select(e => GetEquipmentName(e)))}");
            }
            return hasAny;
        }
        else
        {
            // Need ALL of the required equipment
            List<ItemCategory> missingEquipment = requirement.RequiredEquipment.Where(req => !playerEquipment.Contains(req)).ToList();
            if (missingEquipment.Any())
            {
                foreach (ItemCategory eq in missingEquipment)
                {
                    missing.Add($"Missing equipment: {GetEquipmentName(eq)}");
                }
            }
            return !missingEquipment.Any();
        }
    }

    /// <summary>
    /// Check if player has required specific items.
    /// </summary>
    private bool CheckItemRequirements(AccessRequirement requirement, Player player, List<string> missing)
    {
        if (!requirement.RequiredItemIds.Any())
            return true;

        List<string> playerItems = player.Inventory.GetAllItems().Where(i => !string.IsNullOrEmpty(i)).ToList();

        if (requirement.Logic == RequirementLogic.Or)
        {
            // Need ANY of the required items
            bool hasAny = requirement.RequiredItemIds.Any(req => playerItems.Contains(req));
            if (!hasAny)
            {
                IEnumerable<string> itemNames = requirement.RequiredItemIds.Select(id => GetItemName(id));
                missing.Add($"Need one of: {string.Join(", ", itemNames)}");
            }
            return hasAny;
        }
        else
        {
            // Need ALL of the required items
            List<string> missingItems = requirement.RequiredItemIds.Where(req => !playerItems.Contains(req)).ToList();
            if (missingItems.Any())
            {
                foreach (string? itemId in missingItems)
                {
                    missing.Add($"Missing item: {GetItemName(itemId)}");
                }
            }
            return !missingItems.Any();
        }
    }

    /// <summary>
    /// Check if player has required tokens with specific NPCs.
    /// </summary>
    private bool CheckNPCTokenRequirements(AccessRequirement requirement, List<string> missing)
    {
        if (!requirement.RequiredTokensPerNPC.Any())
            return true;

        if (requirement.Logic == RequirementLogic.Or)
        {
            // Need tokens with ANY of the specified NPCs
            bool hasAny = false;
            foreach (TokenRequirement tokenReq in requirement.RequiredTokensPerNPC)
            {
                Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(tokenReq.NPCId);
                int totalTokens = npcTokens.Values.Sum();
                if (totalTokens >= tokenReq.MinimumCount)
                {
                    hasAny = true;
                    break;
                }
            }

            if (!hasAny)
            {
                IEnumerable<string> options = requirement.RequiredTokensPerNPC.Select(req =>
                    $"{req.MinimumCount} tokens with {GetNPCName(req.NPCId)}"
                );
                missing.Add($"Need one of: {string.Join(", ", options)}");
            }
            return hasAny;
        }
        else
        {
            // Need tokens with ALL specified NPCs
            foreach (TokenRequirement tokenReq in requirement.RequiredTokensPerNPC)
            {
                Dictionary<ConnectionType, int> npcTokens = _tokenManager.GetTokensWithNPC(tokenReq.NPCId);
                int totalTokens = npcTokens.Values.Sum();
                if (totalTokens < tokenReq.MinimumCount)
                {
                    missing.Add($"Need {tokenReq.MinimumCount} tokens with {GetNPCName(tokenReq.NPCId)} (have {totalTokens})");
                }
            }
            return !missing.Any();
        }
    }

    /// <summary>
    /// Check if player has required tokens of specific types.
    /// </summary>
    private bool CheckTypeTokenRequirements(AccessRequirement requirement, List<string> missing)
    {
        if (!requirement.RequiredTokensPerType.Any())
            return true;

        if (requirement.Logic == RequirementLogic.Or)
        {
            // Need tokens of ANY specified type
            bool hasAny = false;
            foreach (TokenTypeRequirement typeReq in requirement.RequiredTokensPerType)
            {
                int totalOfType = _tokenManager.GetTotalTokensOfType(typeReq.TokenType);
                if (totalOfType >= typeReq.MinimumCount)
                {
                    hasAny = true;
                    break;
                }
            }

            if (!hasAny)
            {
                IEnumerable<string> options = requirement.RequiredTokensPerType.Select(req =>
                    $"{req.MinimumCount} {req.TokenType} tokens"
                );
                missing.Add($"Need one of: {string.Join(", ", options)}");
            }
            return hasAny;
        }
        else
        {
            // Need tokens of ALL specified types
            foreach (TokenTypeRequirement typeReq in requirement.RequiredTokensPerType)
            {
                int totalOfType = _tokenManager.GetTotalTokensOfType(typeReq.TokenType);
                if (totalOfType < typeReq.MinimumCount)
                {
                    missing.Add($"Need {typeReq.MinimumCount} {typeReq.TokenType} tokens (have {totalOfType})");
                }
            }
            return !missing.Any();
        }
    }


    /// <summary>
    /// Spend tokens to gain access (for token-gated areas).
    /// </summary>
    public bool SpendTokensForAccess(AccessRequirement requirement, string contextDescription)
    {
        if (requirement.RequiredTokensPerNPC.Any())
        {
            // For NPC-specific requirements, we need to know which NPC to spend from
            // This would typically be passed in as context
            _messageSystem.AddSystemMessage(
                "Token spending for access requires specifying which NPC's favor to use.",
                SystemMessageTypes.Warning
            );
            return false;
        }

        // For type-based requirements, spend from any NPCs with those token types
        foreach (TokenTypeRequirement typeReq in requirement.RequiredTokensPerType)
        {
            bool spent = _tokenManager.SpendTokensOfType(typeReq.TokenType, typeReq.MinimumCount);
            if (!spent)
            {
                _messageSystem.AddSystemMessage(
                    $"Insufficient {typeReq.TokenType} tokens for access.",
                    SystemMessageTypes.Danger
                );
                return false;
            }
        }

        _messageSystem.AddSystemMessage(
            $"You spend tokens to {contextDescription}.",
            SystemMessageTypes.Success
        );
        return true;
    }

    /// <summary>
    /// Get all equipment categories from player's inventory.
    /// </summary>
    private List<ItemCategory> GetPlayerEquipmentCategories(Player player)
    {
        List<ItemCategory> categories = new List<ItemCategory>();

        foreach (string itemId in player.Inventory.GetAllItems())
        {
            if (!string.IsNullOrEmpty(itemId))
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

    private string GetEquipmentName(ItemCategory category)
    {
        return category.ToString().Replace('_', ' ');
    }

    private string GetItemName(string itemId)
    {
        Item item = _itemRepository.GetItemById(itemId);
        return item?.Name ?? itemId;
    }

    private string GetNPCName(string npcId)
    {
        NPC npc = _npcRepository.GetById(npcId);
        return npc?.Name ?? npcId;
    }

    /// <summary>
    /// Check if player meets tier requirement for access.
    /// </summary>
    private bool CheckTierRequirement(AccessRequirement requirement, Player player, List<string> missing)
    {
        if (requirement.MinimumTier <= 1)
            return true; // Tier 1 is always accessible

        // For now, use player level as a proxy for tier access
        // In a full implementation, this could check:
        // - Total tokens earned
        // - Seals acquired
        // - Specific achievements
        int playerTier = CalculatePlayerTier(player);

        if (playerTier < requirement.MinimumTier)
        {
            missing.Add($"Requires Tier {requirement.MinimumTier} access (you are Tier {playerTier})");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Calculate the player's effective tier based on progression.
    /// </summary>
    private int CalculatePlayerTier(Player player)
    {
        // Base tier from player level
        int tierFromLevel = Math.Min(5, 1 + (player.Level - 1) / 2);

        // Bonus tier from total tokens (every 10 tokens = potential tier increase)
        int totalTokens = _tokenManager.GetTokenCount(ConnectionType.Trust) +
                         _tokenManager.GetTokenCount(ConnectionType.Commerce) +
                         _tokenManager.GetTokenCount(ConnectionType.Status) +
                         _tokenManager.GetTokenCount(ConnectionType.Shadow);
        int tierFromTokens = Math.Min(5, 1 + totalTokens / 10);

        // Take the highest tier achieved (seals removed from game)
        return Math.Max(tierFromLevel, tierFromTokens);
    }
}