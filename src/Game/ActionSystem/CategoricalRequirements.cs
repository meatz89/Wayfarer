using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;

namespace Wayfarer.Game.ActionSystem;

/// <summary>
/// Validates social access requirements based on player's social signaling through equipment
/// </summary>
public class SocialAccessRequirement : IRequirement
{
    private readonly SocialRequirement _requiredLevel;
    private readonly ItemRepository _itemRepository;

    public SocialAccessRequirement(SocialRequirement requiredLevel, ItemRepository itemRepository)
    {
        _requiredLevel = requiredLevel;
        _itemRepository = itemRepository;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        return HasAppropriateClassSignaling(player, _requiredLevel);
    }

    public string GetDescription()
    {
        return $"Requires {_requiredLevel.ToString().Replace('_', ' ').ToLower()} social standing";
    }

    private bool HasAppropriateClassSignaling(Player player, SocialRequirement requirement)
    {
        // Check equipment categories for social signaling items
        return requirement switch
        {
            SocialRequirement.Any => true,
            SocialRequirement.Commoner => true, // Anyone can meet commoner requirements
            SocialRequirement.Merchant_Class => HasMerchantCredentials(player),
            SocialRequirement.Artisan_Class => HasArtisanCredentials(player),
            SocialRequirement.Minor_Noble => HasNobleAttire(player),
            SocialRequirement.Major_Noble => HasMajorNobleAttire(player),
            SocialRequirement.Guild_Member => HasGuildMembership(player),
            SocialRequirement.Professional => HasProfessionalCredentials(player),
            _ => false
        };
    }

    private bool HasMerchantCredentials(Player player)
    {
        // Check for merchant attire or trade credentials in inventory
        return player.Inventory.ItemSlots.Any(item => 
            item != null && (
                item.Contains("merchant") || 
                item.Contains("trade") || 
                item.Contains("fine_cloak") ||
                item.Contains("quality_attire")
            ));
    }

    private bool HasArtisanCredentials(Player player)
    {
        // Check for artisan tools or guild tokens
        return player.Inventory.ItemSlots.Any(item => 
            item != null && (
                item.Contains("tools") || 
                item.Contains("guild") ||
                item.Contains("crafting") ||
                item.Contains("artisan")
            ));
    }

    private bool HasNobleAttire(Player player)
    {
        // Check for noble attire or formal wear
        return player.Inventory.ItemSlots.Any(item => 
            item != null && (
                item.Contains("noble") || 
                item.Contains("formal") ||
                item.Contains("silk") ||
                item.Contains("fine_garment")
            ));
    }

    private bool HasMajorNobleAttire(Player player)
    {
        // Requires exceptional formal attire
        return player.Inventory.ItemSlots.Any(item => 
            item != null && (
                item.Contains("royal") || 
                item.Contains("ceremonial") ||
                item.Contains("court_dress")
            ));
    }

    private bool HasGuildMembership(Player player)
    {
        // Check for guild tokens or official membership items
        return player.Inventory.ItemSlots.Any(item => 
            item != null && (
                item.Contains("guild_token") || 
                item.Contains("membership") ||
                item.Contains("seal")
            ));
    }

    private bool HasProfessionalCredentials(Player player)
    {
        // Check for professional tools or documentation
        return player.Inventory.ItemSlots.Any(item => 
            item != null && (
                item.Contains("professional") || 
                item.Contains("credentials") ||
                item.Contains("certification") ||
                item.Contains("license")
            ));
    }
}

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
/// Validates environmental requirements based on current location
/// </summary>
public class EnvironmentRequirement : IRequirement
{
    private readonly EnvironmentCategory _requiredEnvironment;

    public EnvironmentRequirement(EnvironmentCategory requiredEnvironment)
    {
        _requiredEnvironment = requiredEnvironment;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        Location location = gameWorld.CurrentLocation;
        LocationSpot spot = gameWorld.WorldState.CurrentLocationSpot;
        return LocationProvidesEnvironment(location, spot, _requiredEnvironment, gameWorld);
    }

    public string GetDescription()
    {
        return $"Requires {_requiredEnvironment.ToString().Replace('_', ' ').ToLower()} environment";
    }

    private bool LocationProvidesEnvironment(Location location, LocationSpot spot, EnvironmentCategory requirement, GameWorld gameWorld)
    {
        return requirement switch
        {
            EnvironmentCategory.Any => true,
            EnvironmentCategory.Indoor => IsIndoorSpot(spot),
            EnvironmentCategory.Outdoor => !IsIndoorSpot(spot),
            EnvironmentCategory.Workshop => IsWorkshop(location, spot),
            EnvironmentCategory.Commercial_Setting => IsCommercialSetting(location),
            EnvironmentCategory.Private_Space => IsPrivateSpace(location, spot),
            EnvironmentCategory.Good_Light => HasGoodLight(gameWorld),
            EnvironmentCategory.Quiet => IsQuietEnvironment(location),
            EnvironmentCategory.Weather_public => IsWeatherpublic(spot),
            EnvironmentCategory.Specific_Location => true, // Handled by location-specific logic
            EnvironmentCategory.Hearth => HasHearth(spot),
            EnvironmentCategory.Library => IsLibrary(spot),
            EnvironmentCategory.Market_Square => IsMarketSquare(location),
            EnvironmentCategory.Noble_Court => IsNobleCourt(location),
            EnvironmentCategory.Sacred_Space => IsSacredSpace(location, spot),
            _ => false
        };
    }

    private bool IsIndoorSpot(LocationSpot spot)
    {
        return spot.SpotID.Contains("hearth") || 
               spot.SpotID.Contains("library") || 
               spot.SpotID.Contains("shop") ||
               spot.SpotID.Contains("interior") ||
               spot.SpotID.Contains("room");
    }

    private bool IsWorkshop(Location location, LocationSpot spot)
    {
        return spot.SpotID.Contains("workshop") || 
               spot.SpotID.Contains("forge") || 
               location.Id.Contains("smithy") ||
               spot.SpotID.Contains("crafting");
    }

    private bool IsCommercialSetting(Location location)
    {
        return location.Id.Contains("market") || 
               location.Id.Contains("shop") || 
               location.Id.Contains("trade") ||
               location.Id.Contains("merchant");
    }

    private bool IsPrivateSpace(Location location, LocationSpot spot)
    {
        return location.AccessLevel == Access_Level.Private ||
               spot.SpotID.Contains("private") ||
               spot.SpotID.Contains("chamber");
    }

    private bool HasGoodLight(GameWorld gameWorld)
    {
        // Check time of day for natural light
        var currentTime = gameWorld.TimeManager.CurrentTimeBlock;
        return currentTime == TimeBlocks.Morning || currentTime == TimeBlocks.Afternoon;
    }

    private bool IsQuietEnvironment(Location location)
    {
        // Markets and busy areas are noisy
        return !location.Id.Contains("market") && 
               !location.Id.Contains("square") &&
               !location.Id.Contains("tavern");
    }

    private bool IsWeatherpublic(LocationSpot spot)
    {
        return IsIndoorSpot(spot) || 
               spot.SpotID.Contains("covered") ||
               spot.SpotID.Contains("shelter");
    }

    private bool HasHearth(LocationSpot spot)
    {
        return spot.SpotID.Contains("hearth") || 
               spot.SpotID.Contains("fireplace") ||
               spot.SpotID.Contains("fire");
    }

    private bool IsLibrary(LocationSpot spot)
    {
        return spot.SpotID.Contains("library") || 
               spot.SpotID.Contains("books") ||
               spot.SpotID.Contains("study");
    }

    private bool IsMarketSquare(Location location)
    {
        return location.Id.Contains("market") || 
               location.Id.Contains("square");
    }

    private bool IsNobleCourt(Location location)
    {
        return location.Id.Contains("court") || 
               location.Id.Contains("manor") ||
               location.Id.Contains("palace");
    }

    private bool IsSacredSpace(Location location, LocationSpot spot)
    {
        return location.Id.Contains("temple") || 
               location.Id.Contains("shrine") ||
               spot.SpotID.Contains("altar");
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
            ActionSystem.KnowledgeRequirement.Basic => true, // All players have basic knowledge
            ActionSystem.KnowledgeRequirement.Professional => player.Level >= 3,
            ActionSystem.KnowledgeRequirement.Advanced => player.Level >= 5,
            ActionSystem.KnowledgeRequirement.Expert => player.Level >= 7,
            ActionSystem.KnowledgeRequirement.Master => player.Level >= 10,
            ActionSystem.KnowledgeRequirement.Local => HasLocalKnowledge(player),
            ActionSystem.KnowledgeRequirement.Commercial => HasCommercialKnowledge(player),
            ActionSystem.KnowledgeRequirement.Academic => HasAcademicKnowledge(player),
            ActionSystem.KnowledgeRequirement.Technical => HasTechnicalKnowledge(player),
            ActionSystem.KnowledgeRequirement.Cultural => HasCulturalKnowledge(player),
            ActionSystem.KnowledgeRequirement.Legal => HasLegalKnowledge(player),
            _ => false
        };
    }

    private bool HasLocalKnowledge(Player player)
    {
        // Check if player has spent time in current area
        return player.KnownLocations.Count >= 3;
    }

    private bool HasCommercialKnowledge(Player player)
    {
        // Check if player has trading experience
        return player.Inventory.ItemSlots.Any(item => 
            item != null && item.Contains("trade"));
    }

    private bool HasAcademicKnowledge(Player player)
    {
        // Check if player has academic materials
        return player.Inventory.ItemSlots.Any(item => 
            item != null && (item.Contains("book") || item.Contains("scroll")));
    }

    private bool HasTechnicalKnowledge(Player player)
    {
        // Check if player has technical tools
        return player.Inventory.ItemSlots.Any(item => 
            item != null && item.Contains("tools"));
    }

    private bool HasCulturalKnowledge(Player player)
    {
        // Check if player has cultural understanding
        return player.Relationships.GetAllRelationships().Count >= 2;
    }

    private bool HasLegalKnowledge(Player player)
    {
        // Check if player has legal documentation
        return player.Inventory.ItemSlots.Any(item => 
            item != null && (item.Contains("contract") || item.Contains("legal")));
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
    private readonly InformationFreshness _minimumFreshness;
    private readonly string _specificTopicId; // Optional: specific information piece required

    public InformationRequirement(
        InformationType requiredType, 
        InformationQuality minimumQuality = InformationQuality.Reliable,
        InformationFreshness minimumFreshness = InformationFreshness.Recent,
        string specificTopicId = null)
    {
        _requiredType = requiredType;
        _minimumQuality = minimumQuality;
        _minimumFreshness = minimumFreshness;
        _specificTopicId = specificTopicId;
    }

    public bool IsMet(GameWorld gameWorld)
    {
        Player player = gameWorld.GetPlayer();
        
        // If specific information required, check for exact match
        if (!string.IsNullOrEmpty(_specificTopicId))
        {
            var specificInfo = player.KnownInformation.FirstOrDefault(info => info.Id == _specificTopicId);
            return specificInfo != null && 
                   specificInfo.MeetsRequirements(_requiredType, _minimumQuality, _minimumFreshness);
        }
        
        // Otherwise check for any information matching categorical requirements
        return player.KnownInformation.Any(info => 
            info.MeetsRequirements(_requiredType, _minimumQuality, _minimumFreshness));
    }

    public string GetDescription()
    {
        string qualityDesc = _minimumQuality != InformationQuality.Reliable 
            ? $" ({_minimumQuality}+ quality)" 
            : "";
            
        string freshnessDesc = _minimumFreshness != InformationFreshness.Recent 
            ? $" ({_minimumFreshness}+ freshness)" 
            : "";

        if (!string.IsNullOrEmpty(_specificTopicId))
        {
            return $"Requires specific information: {_specificTopicId}{qualityDesc}{freshnessDesc}";
        }

        return $"Requires {_requiredType.ToString().Replace('_', ' ').ToLower()} information{qualityDesc}{freshnessDesc}";
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
        var existingInfo = player.KnownInformation.FirstOrDefault(info => info.Id == _informationToProvide.Id);
        
        if (existingInfo != null && _upgradeExisting)
        {
            // Upgrade existing information quality/freshness
            if (_informationToProvide.Quality > existingInfo.Quality)
                existingInfo.Quality = _informationToProvide.Quality;
                
            if (_informationToProvide.Freshness > existingInfo.Freshness)
                existingInfo.Freshness = _informationToProvide.Freshness;
                
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
                Freshness = _informationToProvide.Freshness,
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

/// <summary>
/// Social standing effect that improves player reputation and relationships
/// </summary>
public class SocialStandingEffect : IMechanicalEffect
{
    private readonly int _reputationChange;
    private readonly SocialRequirement _socialContext;
    private readonly string _description;

    public SocialStandingEffect(int reputationChange, SocialRequirement socialContext, string description = "social interaction")
    {
        _reputationChange = reputationChange;
        _socialContext = socialContext;
        _description = description;
    }

    public void Apply(EncounterState encounterState)
    {
        Player player = encounterState.Player;
        
        // Apply reputation change
        int oldReputation = player.Reputation;
        player.Reputation += _reputationChange;
        
        // Log the reputation change via memory system (consistent with existing pattern)
        string changeDescription = _reputationChange > 0 ? "improved" : "diminished";
        player.AddMemory($"social_standing_{DateTime.Now.Ticks}",
            $"Your {GetSocialContextDescription(_socialContext)} standing {changeDescription} from {_description}.",
            Math.Abs(_reputationChange), 7); // Higher importance, lasts a week
    }

    public string GetDescriptionForPlayer()
    {
        string change = _reputationChange > 0 ? "improves" : "reduces";
        return $"{change} {GetSocialContextDescription(_socialContext)} reputation ({_description})";
    }


    private string GetSocialContextDescription(SocialRequirement context)
    {
        return context switch
        {
            SocialRequirement.Merchant_Class => "merchant",
            SocialRequirement.Artisan_Class => "artisan",
            SocialRequirement.Minor_Noble => "noble",
            SocialRequirement.Major_Noble => "high noble",
            SocialRequirement.Guild_Member => "guild",
            SocialRequirement.Professional => "professional",
            _ => "social"
        };
    }
}