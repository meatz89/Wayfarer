using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Game.ActionSystem;


/// <summary>
/// Validates equipment category requirements based on player's equipment
/// </summary>
public class EquipmentCategoryRequirement : IRequirement
{
    private readonly EquipmentCategory _requiredEquipment;
    private readonly ItemRepository _itemRepository;

    public EquipmentCategoryRequirement(EquipmentCategory requiredEquipment, ItemRepository itemRepository)
    {
        _requiredEquipment = requiredEquipment;
        _itemRepository = itemRepository;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        return PlayerHasEquipmentCategory(player, _requiredEquipment);
    }

    public string GetDescription()
    {
        return $"Requires {GetEquipmentCategoryDescription(_requiredEquipment)}";
    }

    private bool PlayerHasEquipmentCategory(Player player, EquipmentCategory category)
    {
        // Check if player has any item with the required equipment category
        foreach (string? itemId in player.Inventory.ItemSlots)
        {
            if (itemId != null)
            {
                Item? item = _itemRepository.GetItemById(itemId);
                if (item != null && item.HasEquipmentCategory(category))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private string GetEquipmentCategoryDescription(EquipmentCategory category)
    {
        return category switch
        {
            EquipmentCategory.Climbing_Equipment => "climbing equipment",
            EquipmentCategory.Water_Transport => "water transport equipment",
            EquipmentCategory.Special_Access => "special access credentials",
            EquipmentCategory.Navigation_Tools => "navigation tools",
            EquipmentCategory.Weather_Protection => "weather protection",
            EquipmentCategory.Load_Distribution => "load distribution equipment",
            EquipmentCategory.Light_Source => "light source",
            _ => category.ToString().Replace("_", " ").ToLower()
        };
    }
}

/// <summary>
/// Validates tool category requirements based on player's equipment
/// </summary>
public class ToolCategoryRequirement : IRequirement
{
    private readonly ToolCategory _requiredTool;
    private readonly ItemRepository _itemRepository;

    public ToolCategoryRequirement(ToolCategory requiredTool, ItemRepository itemRepository)
    {
        _requiredTool = requiredTool;
        _itemRepository = itemRepository;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        return PlayerHasToolCategory(player, _requiredTool);
    }

    public string GetDescription()
    {
        return $"Requires {GetToolCategoryDescription(_requiredTool)}";
    }

    private bool PlayerHasToolCategory(Player player, ToolCategory category)
    {
        return category switch
        {
            ToolCategory.None => true,
            ToolCategory.Basic_Tools => HasBasicTools(player),
            ToolCategory.Specialized_Equipment => HasSpecializedEquipment(player),
            ToolCategory.Trade_Samples => HasTradeSamples(player),
            ToolCategory.Documentation => HasDocumentation(player),
            ToolCategory.Quality_Materials => HasQualityMaterials(player),
            ToolCategory.Writing_Materials => HasWritingMaterials(player),
            ToolCategory.Measurement_Tools => HasMeasurementTools(player),
            ToolCategory.Safety_Equipment => HasSafetyEquipment(player),
            ToolCategory.Social_Attire => HasSocialAttire(player),
            ToolCategory.Crafting_Supplies => HasCraftingSupplies(player),
            _ => false
        };
    }

    private bool HasBasicTools(Player player)
    {
        return player.Inventory.ItemSlots.Any(item =>
            item != null && (
                item.Contains("tools") ||
                item.Contains("hammer") ||
                item.Contains("knife")
            ));
    }

    private bool HasSpecializedEquipment(Player player)
    {
        return player.Inventory.ItemSlots.Any(item =>
            item != null && (
                item.Contains("specialized") ||
                item.Contains("professional") ||
                item.Contains("precision")
            ));
    }


    private bool HasTradeSamples(Player player)
    {
        return player.Inventory.ItemSlots.Any(item =>
            item != null && (
                item.Contains("sample") ||
                item.Contains("quality") ||
                item.Contains("fine")
            ));
    }

    private bool HasDocumentation(Player player)
    {
        return player.Inventory.ItemSlots.Any(item =>
            item != null && (
                item.Contains("document") ||
                item.Contains("contract") ||
                item.Contains("permit") ||
                item.Contains("papers")
            ));
    }

    private bool HasQualityMaterials(Player player)
    {
        return player.Inventory.ItemSlots.Any(item =>
            item != null && (
                item.Contains("quality") ||
                item.Contains("fine") ||
                item.Contains("premium") ||
                item.Contains("silk") ||
                item.Contains("precious")
            ));
    }

    private bool HasWritingMaterials(Player player)
    {
        return player.Inventory.ItemSlots.Any(item =>
            item != null && (
                item.Contains("ink") ||
                item.Contains("paper") ||
                item.Contains("parchment") ||
                item.Contains("quill")
            ));
    }

    private bool HasMeasurementTools(Player player)
    {
        return player.Inventory.ItemSlots.Any(item =>
            item != null && (
                item.Contains("scale") ||
                item.Contains("ruler") ||
                item.Contains("measure")
            ));
    }

    private bool HasSafetyEquipment(Player player)
    {
        return player.Inventory.ItemSlots.Any(item =>
            item != null && (
                item.Contains("protection") ||
                item.Contains("safety") ||
                item.Contains("armor") ||
                item.Contains("shield")
            ));
    }

    private bool HasSocialAttire(Player player)
    {
        return player.Inventory.ItemSlots.Any(item =>
            item != null && (
                item.Contains("formal") ||
                item.Contains("dress") ||
                item.Contains("ceremonial") ||
                item.Contains("attire")
            ));
    }

    private bool HasCraftingSupplies(Player player)
    {
        return player.Inventory.ItemSlots.Any(item =>
            item != null && (
                item.Contains("material") ||
                item.Contains("supply") ||
                item.Contains("component") ||
                item.Contains("ingredient")
            ));
    }

    private string GetToolCategoryDescription(ToolCategory category)
    {
        return category switch
        {
            ToolCategory.Basic_Tools => "basic tools",
            ToolCategory.Specialized_Equipment => "specialized equipment",
            ToolCategory.Trade_Samples => "quality trade samples",
            ToolCategory.Documentation => "proper documentation",
            ToolCategory.Quality_Materials => "quality materials",
            ToolCategory.Writing_Materials => "writing materials",
            ToolCategory.Measurement_Tools => "measurement tools",
            ToolCategory.Safety_Equipment => "safety equipment",
            ToolCategory.Social_Attire => "appropriate social attire",
            ToolCategory.Crafting_Supplies => "crafting supplies",
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
            ActionSystem.KnowledgeRequirement.None => true,
            ActionSystem.KnowledgeRequirement.Local => HasLocalKnowledge(player),
            ActionSystem.KnowledgeRequirement.Cultural => HasCulturalKnowledge(player),
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

/// <summary>
/// Validates information requirements based on player's knowledge
/// </summary>
public class InformationRequirement : IRequirement
{
    private readonly InformationType _requiredType;
    private readonly InformationQuality _minimumQuality;
    private readonly string _specificTopicId; // Optional: specific information piece required

    public InformationRequirement(
        InformationType requiredType,
        InformationQuality minimumQuality = InformationQuality.Reliable,
        string specificTopicId = null)
    {
        _requiredType = requiredType;
        _minimumQuality = minimumQuality;
        _specificTopicId = specificTopicId;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();

        // If specific information required, check for exact match
        if (!string.IsNullOrEmpty(_specificTopicId))
        {
            Information? specificInfo = player.KnownInformation.FirstOrDefault(info => info.Id == _specificTopicId);
            return specificInfo != null &&
                   specificInfo.MeetsRequirements(_requiredType, _minimumQuality);
        }

        // Otherwise check for any information matching categorical requirements
        return player.KnownInformation.Any(info =>
            info.MeetsRequirements(_requiredType, _minimumQuality));
    }

    public string GetDescription()
    {
        string qualityDesc = _minimumQuality != InformationQuality.Reliable
            ? $" ({_minimumQuality}+ quality)"
            : "";

        if (!string.IsNullOrEmpty(_specificTopicId))
        {
            return $"Requires specific information: {_specificTopicId}{qualityDesc}";
        }

        return $"Requires {_requiredType.ToString().Replace('_', ' ').ToLower()} information{qualityDesc}";
    }
}

/// <summary>
/// Effect that provides information to the player's knowledge base
/// </summary>
public class InformationEffect : IMechanicalEffect
{
    private readonly Information _informationToProvide;
    private readonly bool _upgradeExisting; // If true, improves quality/freshness of existing info

    public InformationEffect(Information information, bool upgradeExisting = false)
    {
        _informationToProvide = information;
        _upgradeExisting = upgradeExisting;
    }

    public void Apply(EncounterState encounterState)
    {
        Player player = encounterState.Player;

        // Check if player already has this information
        Information? existingInfo = player.KnownInformation.FirstOrDefault(info => info.Id == _informationToProvide.Id);

        if (existingInfo != null && _upgradeExisting)
        {
            // Upgrade existing information quality/freshness
            if (_informationToProvide.Quality > existingInfo.Quality)
                existingInfo.Quality = _informationToProvide.Quality;

            // Update source if this is a better source
            if (_informationToProvide.Quality >= existingInfo.Quality)
                existingInfo.Source = _informationToProvide.Source;
        }
        else if (existingInfo == null)
        {
            // Add new information to player's knowledge
            Information newInfo = new Information(_informationToProvide.Id, _informationToProvide.Title, _informationToProvide.Type)
            {
                Content = _informationToProvide.Content,
                Source = _informationToProvide.Source,
                Quality = _informationToProvide.Quality,
                LocationId = _informationToProvide.LocationId,
                NPCId = _informationToProvide.NPCId,
                Value = _informationToProvide.Value,
                IsPublic = _informationToProvide.IsPublic
            };

            newInfo.RelatedItemIds.AddRange(_informationToProvide.RelatedItemIds);
            newInfo.RelatedLocationIds.AddRange(_informationToProvide.RelatedLocationIds);

            player.KnownInformation.Add(newInfo);
        }

        // Add memory of information acquisition for player feedback
        string actionDescription = existingInfo != null && _upgradeExisting
            ? $"Updated knowledge about {_informationToProvide.Title}"
            : $"Learned about {_informationToProvide.Title}";

        player.AddMemory($"info_{_informationToProvide.Id}_{DateTime.Now.Ticks}",
            $"{actionDescription} from {_informationToProvide.Source}",
            _informationToProvide.CalculateCurrentValue() / 10, // Importance based on value
            _informationToProvide.DaysToExpire); // Duration based on info lifespan
    }

    public string GetDescriptionForPlayer()
    {
        string qualityDesc = _informationToProvide.Quality != InformationQuality.Reliable
            ? $" ({_informationToProvide.Quality} quality)"
            : "";

        return $"Learn {_informationToProvide.Type.ToString().Replace('_', ' ').ToLower()}: {_informationToProvide.Title}{qualityDesc}";
    }
}
