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
    private readonly ConnectionTokenManager _tokenManager;
    private readonly NPCRepository _npcRepository;
    private readonly MessageSystem _messageSystem;
    
    public AccessRequirementChecker(
        GameWorld gameWorld,
        ItemRepository itemRepository,
        ConnectionTokenManager tokenManager,
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
        var player = _gameWorld.GetPlayer();
        var missingRequirements = new List<string>();
        
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
            
        var playerEquipment = GetPlayerEquipmentCategories(player);
        
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
            var missingEquipment = requirement.RequiredEquipment.Where(req => !playerEquipment.Contains(req)).ToList();
            if (missingEquipment.Any())
            {
                foreach (var eq in missingEquipment)
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
            
        var playerItems = player.Inventory.ItemSlots.Where(i => !string.IsNullOrEmpty(i)).ToList();
        
        if (requirement.Logic == RequirementLogic.Or)
        {
            // Need ANY of the required items
            bool hasAny = requirement.RequiredItemIds.Any(req => playerItems.Contains(req));
            if (!hasAny)
            {
                var itemNames = requirement.RequiredItemIds.Select(id => GetItemName(id));
                missing.Add($"Need one of: {string.Join(", ", itemNames)}");
            }
            return hasAny;
        }
        else
        {
            // Need ALL of the required items
            var missingItems = requirement.RequiredItemIds.Where(req => !playerItems.Contains(req)).ToList();
            if (missingItems.Any())
            {
                foreach (var itemId in missingItems)
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
            foreach (var (npcId, requiredCount) in requirement.RequiredTokensPerNPC)
            {
                var npcTokens = _tokenManager.GetTokensWithNPC(npcId);
                var totalTokens = npcTokens.Values.Sum();
                if (totalTokens >= requiredCount)
                {
                    hasAny = true;
                    break;
                }
            }
            
            if (!hasAny)
            {
                var options = requirement.RequiredTokensPerNPC.Select(kvp => 
                    $"{kvp.Value} tokens with {GetNPCName(kvp.Key)}"
                );
                missing.Add($"Need one of: {string.Join(", ", options)}");
            }
            return hasAny;
        }
        else
        {
            // Need tokens with ALL specified NPCs
            foreach (var (npcId, requiredCount) in requirement.RequiredTokensPerNPC)
            {
                var npcTokens = _tokenManager.GetTokensWithNPC(npcId);
                var totalTokens = npcTokens.Values.Sum();
                if (totalTokens < requiredCount)
                {
                    missing.Add($"Need {requiredCount} tokens with {GetNPCName(npcId)} (have {totalTokens})");
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
            foreach (var (tokenType, requiredCount) in requirement.RequiredTokensPerType)
            {
                var totalOfType = _tokenManager.GetTotalTokensOfType(tokenType);
                if (totalOfType >= requiredCount)
                {
                    hasAny = true;
                    break;
                }
            }
            
            if (!hasAny)
            {
                var options = requirement.RequiredTokensPerType.Select(kvp => 
                    $"{kvp.Value} {kvp.Key} tokens"
                );
                missing.Add($"Need one of: {string.Join(", ", options)}");
            }
            return hasAny;
        }
        else
        {
            // Need tokens of ALL specified types
            foreach (var (tokenType, requiredCount) in requirement.RequiredTokensPerType)
            {
                var totalOfType = _tokenManager.GetTotalTokensOfType(tokenType);
                if (totalOfType < requiredCount)
                {
                    missing.Add($"Need {requiredCount} {tokenType} tokens (have {totalOfType})");
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
        foreach (var (tokenType, requiredCount) in requirement.RequiredTokensPerType)
        {
            var spent = _tokenManager.SpendTokensOfType(tokenType, requiredCount);
            if (!spent)
            {
                _messageSystem.AddSystemMessage(
                    $"Insufficient {tokenType} tokens for access.",
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
        var categories = new List<ItemCategory>();
        
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (!string.IsNullOrEmpty(itemId))
            {
                var item = _itemRepository.GetItemById(itemId);
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
        var item = _itemRepository.GetItemById(itemId);
        return item?.Name ?? itemId;
    }
    
    private string GetNPCName(string npcId)
    {
        var npc = _npcRepository.GetNPCById(npcId);
        return npc?.Name ?? npcId;
    }
}