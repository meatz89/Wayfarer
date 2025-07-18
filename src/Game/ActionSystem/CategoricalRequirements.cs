

/// <summary>
/// Validates item category requirements based on player's inventory
/// </summary>
public class ItemCategoryRequirement : IRequirement
{
    private readonly ItemCategory _requiredCategory;
    private readonly ItemRepository _itemRepository;

    public ItemCategoryRequirement(ItemCategory requiredCategory, ItemRepository itemRepository)
    {
        _requiredCategory = requiredCategory;
        _itemRepository = itemRepository;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        return PlayerHasItemCategory(player, _requiredCategory);
    }

    public string GetDescription()
    {
        return $"Requires {GetItemCategoryDescription(_requiredCategory)}";
    }

    private bool PlayerHasItemCategory(Player player, ItemCategory category)
    {
        // Check if player has any item with the required equipment category
        foreach (string? itemId in player.Inventory.ItemSlots)
        {
            if (itemId != null)
            {
                Item? item = _itemRepository.GetItemById(itemId);
                if (item != null && item.HasCategory(category))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private string GetItemCategoryDescription(ItemCategory category)
    {
        return category switch
        {
            ItemCategory.Climbing_Equipment => "climbing equipment",
            ItemCategory.Water_Transport => "water transport equipment",
            ItemCategory.Special_Access => "special access credentials",
            ItemCategory.Navigation_Tools => "navigation tools",
            ItemCategory.Weather_Protection => "weather protection",
            ItemCategory.Load_Distribution => "load distribution equipment",
            ItemCategory.Light_Source => "light source",
            ItemCategory.Food => "food",
            ItemCategory.Documents => "documents",
            ItemCategory.Tools => "tools",
            ItemCategory.Weapons => "weapons",
            ItemCategory.Clothing => "clothing",
            ItemCategory.Medicine => "medicine",
            ItemCategory.Valuables => "valuables",
            ItemCategory.Materials => "materials",
            _ => category.ToString().Replace("_", " ").ToLower()
        };
    }
}



/// <summary>
/// Validates knowledge and skill requirements
/// </summary>
public class KnowledgeLevelRequirement : IRequirement
{
    private readonly KnowledgeRequirement _requiredLevel;

    public KnowledgeLevelRequirement(KnowledgeRequirement requiredLevel)
    {
        _requiredLevel = requiredLevel;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        return HasRequiredKnowledge(player, _requiredLevel);
    }

    public string GetDescription()
    {
        return $"Requires {_requiredLevel.ToString().Replace('_', ' ').ToLower()} knowledge";
    }

    private bool HasRequiredKnowledge(Player player, KnowledgeRequirement requirement)
    {
        // For now, use player level as a proxy for knowledge
        // In full categorical system, would check specific knowledge categories
        return requirement switch
        {
            KnowledgeRequirement.None => true,
            KnowledgeRequirement.Local => HasLocalKnowledge(player),
            KnowledgeRequirement.Cultural => HasCulturalKnowledge(player),
            _ => false
        };
    }

    private bool HasLocalKnowledge(Player player)
    {
        // Check if player has spent time in current area
        return player.KnownLocations.Count >= 3;
    }

    private bool HasCulturalKnowledge(Player player)
    {
        // Check if player has cultural understanding
        return player.Relationships.GetAllRelationships().Count >= 2;
    }
}

/// <summary>
/// Physical recovery effect that restores player stamina based on recovery category
/// </summary>
public class PhysicalRecoveryEffect : IMechanicalEffect
{
    private readonly int _recoveryAmount;
    private readonly string _description;

    public PhysicalRecoveryEffect(int recoveryAmount, string description = "physical recovery")
    {
        _recoveryAmount = recoveryAmount;
        _description = description;
    }

    public void Apply(EncounterState encounterState)
    {
        Player player = encounterState.Player;
        int oldStamina = player.Stamina;
        player.Stamina = Math.Min(player.MaxStamina, player.Stamina + _recoveryAmount);

        // Log the stamina recovery for feedback
        int actualRecovery = player.Stamina - oldStamina;
        if (actualRecovery > 0)
        {
            // Add memory of recovery for consistency with existing pattern
            player.AddMemory($"recovery_{DateTime.Now.Ticks}", $"Recovered {actualRecovery} stamina from {_description}", 1, 1);
        }
    }

    public string GetDescriptionForPlayer()
    {
        return $"Restores {_recoveryAmount} stamina ({_description})";
    }
}

/// <summary>
/// Validates stamina requirements based on physical demand category
/// </summary>
public class StaminaCategoricalRequirement : IRequirement
{
    private readonly PhysicalDemand _requiredPhysicalCapability;

    public StaminaCategoricalRequirement(PhysicalDemand requiredPhysicalCapability)
    {
        _requiredPhysicalCapability = requiredPhysicalCapability;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        return player.CanPerformStaminaAction(_requiredPhysicalCapability);
    }

    public string GetDescription()
    {
        return _requiredPhysicalCapability switch
        {
            PhysicalDemand.None => "No physical requirements",
            PhysicalDemand.Light => "Requires light physical capability (2+ stamina)",
            PhysicalDemand.Moderate => "Requires moderate physical capability (4+ stamina)",
            PhysicalDemand.Heavy => "Requires heavy physical capability (6+ stamina)",
            PhysicalDemand.Extreme => "Requires extreme physical capability (8+ stamina)",
            _ => "Unknown physical requirement"
        };
    }
}


