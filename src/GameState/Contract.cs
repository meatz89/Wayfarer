using Wayfarer.Game.ActionSystem;

public class Contract
{
    public string Id { get; set; }
    public string Description { get; set; }
    public int StartDay { get; set; }
    public int DueDay { get; set; }
    public int Payment { get; set; }
    public string FailurePenalty { get; set; }
    public bool IsCompleted { get; set; } = false;
    public bool IsFailed { get; set; } = false;
    public List<string> UnlocksContractIds { get; set; } = new List<string>();
    public List<string> LocksContractIds { get; set; } = new List<string>();

    // Time pressure enhancements
    public List<TimeBlocks> AvailableTimeBlocks { get; set; } = new List<TimeBlocks>();
    public int CompletedDay { get; set; } = -1; // Day when contract was completed

    // === CATEGORICAL REQUIREMENTS ===
    
    /// <summary>
    /// Equipment categories required to complete this contract successfully
    /// </summary>
    public List<EquipmentCategory> RequiredEquipmentCategories { get; set; } = new List<EquipmentCategory>();
    
    /// <summary>
    /// Tool categories needed for optimal contract completion
    /// </summary>
    public List<ToolCategory> RequiredToolCategories { get; set; } = new List<ToolCategory>();
    
    /// <summary>
    /// Social standing required to access or complete this contract
    /// </summary>
    public SocialRequirement RequiredSocialStanding { get; set; } = SocialRequirement.Any;
    
    /// <summary>
    /// Physical demands of completing this contract
    /// </summary>
    public PhysicalDemand PhysicalRequirement { get; set; } = PhysicalDemand.None;
    
    /// <summary>
    /// Information types that must be known to complete this contract
    /// </summary>
    public List<InformationRequirementData> RequiredInformation { get; set; } = new List<InformationRequirementData>();
    
    /// <summary>
    /// Professional knowledge level required for this contract
    /// </summary>
    public KnowledgeRequirement RequiredKnowledge { get; set; } = KnowledgeRequirement.None;
    
    /// <summary>
    /// Category of contract (determines which NPCs provide it and relationships affected)
    /// </summary>
    public ContractCategory Category { get; set; } = ContractCategory.General;
    
    /// <summary>
    /// Priority level affecting payment and reputation impact
    /// </summary>
    public ContractPriority Priority { get; set; } = ContractPriority.Standard;
    
    /// <summary>
    /// Risk level determining potential negative consequences
    /// </summary>
    public ContractRisk RiskLevel { get; set; } = ContractRisk.Low;

    // === BASIC ACTION REQUIREMENTS ===
    
    /// <summary>
    /// Locations that must be visited to complete this contract
    /// </summary>
    public List<string> RequiredDestinations { get; set; } = new List<string>();
    
    /// <summary>
    /// Market transactions (buy/sell) required to complete this contract
    /// </summary>
    public List<ContractTransaction> RequiredTransactions { get; set; } = new List<ContractTransaction>();
    
    /// <summary>
    /// NPCs that must be talked to for this contract
    /// </summary>
    public List<string> RequiredNPCConversations { get; set; } = new List<string>();
    
    /// <summary>
    /// Specific location actions that must be performed
    /// </summary>
    public List<string> RequiredLocationActions { get; set; } = new List<string>();
    
    // === PROGRESS TRACKING ===
    
    /// <summary>
    /// Destinations that have been visited
    /// </summary>
    public HashSet<string> CompletedDestinations { get; set; } = new HashSet<string>();
    
    /// <summary>
    /// Transactions that have been completed
    /// </summary>
    public List<ContractTransaction> CompletedTransactions { get; set; } = new List<ContractTransaction>();
    
    /// <summary>
    /// NPCs that have been talked to
    /// </summary>
    public HashSet<string> CompletedNPCConversations { get; set; } = new HashSet<string>();
    
    /// <summary>
    /// Location actions that have been performed
    /// </summary>
    public HashSet<string> CompletedLocationActions { get; set; } = new HashSet<string>();

    /// <summary>
    /// Calculate completion progress as percentage (0.0 to 1.0)
    /// </summary>
    public float CalculateProgress()
    {
        int totalRequirements = 0;
        int completedRequirements = 0;

        // Count destination requirements
        totalRequirements += RequiredDestinations.Count;
        completedRequirements += CompletedDestinations.Count;

        // Count transaction requirements
        totalRequirements += RequiredTransactions.Count;
        completedRequirements += CompletedTransactions.Count;

        // Count NPC conversation requirements
        totalRequirements += RequiredNPCConversations.Count;
        completedRequirements += CompletedNPCConversations.Count;

        // Count location action requirements
        totalRequirements += RequiredLocationActions.Count;
        completedRequirements += CompletedLocationActions.Count;


        return totalRequirements > 0 ? (float)completedRequirements / totalRequirements : 1.0f;
    }

    /// <summary>
    /// Check if all basic action requirements are completed
    /// </summary>
    public bool AreBasicActionRequirementsCompleted()
    {
        // Check all destinations visited
        if (!RequiredDestinations.All(dest => CompletedDestinations.Contains(dest)))
            return false;

        // Check all transactions completed
        if (!RequiredTransactions.All(req => CompletedTransactions.Any(comp => 
            comp.ItemId == req.ItemId && comp.LocationId == req.LocationId && comp.TransactionType == req.TransactionType)))
            return false;

        // Check all NPCs talked to
        if (!RequiredNPCConversations.All(npc => CompletedNPCConversations.Contains(npc)))
            return false;

        // Check all location actions performed
        if (!RequiredLocationActions.All(action => CompletedLocationActions.Contains(action)))
            return false;

        return true;
    }

    /// <summary>
    /// Check if contract is fully completed (including legacy requirements)
    /// </summary>
    public bool IsFullyCompleted()
    {
        // Check basic action requirements
        if (!AreBasicActionRequirementsCompleted())
            return false;

        // Check legacy requirements (simplified for now)
        // TODO: Integrate legacy validation with ContractValidationService

        return true;
    }

    public bool IsAvailable(int currentDay, TimeBlocks currentTimeBlock)
    {
        bool basicAvailability = !IsCompleted && !IsFailed && currentDay >= StartDay && currentDay <= DueDay;

        // If no specific time blocks are required, available anytime
        if (!AvailableTimeBlocks.Any())
        {
            return basicAvailability;
        }

        // Check if current time block is in the allowed list
        return basicAvailability && AvailableTimeBlocks.Contains(currentTimeBlock);
    }

    public bool CanComplete(Player player, string currentLocationId)
    {
        ContractAccessResult result = GetAccessResult(player, currentLocationId);
        return result.CanComplete;
    }

    /// <summary>
    /// Provides detailed analysis of contract accessibility and completion requirements
    /// </summary>
    public ContractAccessResult GetAccessResult(Player player, string currentLocationId)
    {
        ContractAccessResult result = new ContractAccessResult();
        List<string> acceptanceBlockers = new List<string>();
        List<string> completionBlockers = new List<string>();
        List<string> missingRequirements = new List<string>();

        // === COMPLETION ACTIONS VALIDATION ===
        // NOTE: Completion actions (transactions, destinations, conversations, location actions) 
        // are validated by ContractProgressionService, not here.
        // This method only checks PREREQUISITES (social standing, equipment, etc.)

        // === CATEGORICAL REQUIREMENTS ===

        // Check equipment category requirements
        foreach (EquipmentCategory equipmentCategory in RequiredEquipmentCategories)
        {
            if (!PlayerHasEquipmentCategory(player, equipmentCategory))
            {
                string categoryName = equipmentCategory.ToString().Replace("_", " ");
                missingRequirements.Add($"Requires {categoryName} equipment");
                completionBlockers.Add($"Missing required equipment category: {categoryName}");
            }
        }

        // Check tool category requirements
        foreach (ToolCategory toolCategory in RequiredToolCategories)
        {
            if (!PlayerHasToolCategory(player, toolCategory))
            {
                string categoryName = toolCategory.ToString().Replace("_", " ");
                missingRequirements.Add($"Requires {categoryName} tools");
                completionBlockers.Add($"Missing required tool category: {categoryName}");
            }
        }

        // Check social standing requirement
        if (RequiredSocialStanding != SocialRequirement.Any)
        {
            if (!PlayerMeetsSocialRequirement(player, RequiredSocialStanding))
            {
                string socialLevel = RequiredSocialStanding.ToString().Replace("_", " ");
                acceptanceBlockers.Add($"Requires {socialLevel} social standing");
            }
        }

        // Check physical capability requirement
        if (PhysicalRequirement != PhysicalDemand.None)
        {
            if (!player.CanPerformStaminaAction(PhysicalRequirement))
            {
                string physicalLevel = PhysicalRequirement.ToString().Replace("_", " ");
                completionBlockers.Add($"Insufficient stamina for {physicalLevel} physical demands");
            }
        }

        // Check information requirements
        foreach (InformationRequirementData infoReq in RequiredInformation)
        {
            if (!PlayerHasRequiredInformation(player, infoReq))
            {
                string infoType = infoReq.RequiredType.ToString().Replace("_", " ");
                string qualityLevel = infoReq.MinimumQuality.ToString();
                completionBlockers.Add($"Missing {qualityLevel} quality {infoType} information");
            }
        }

        // Check knowledge requirement
        if (RequiredKnowledge != KnowledgeRequirement.None)
        {
            if (!PlayerMeetsKnowledgeRequirement(player, RequiredKnowledge))
            {
                string knowledgeLevel = RequiredKnowledge.ToString().Replace("_", " ");
                acceptanceBlockers.Add($"Requires {knowledgeLevel} knowledge level");
            }
        }

        // Set results
        result.CanAccept = acceptanceBlockers.Count == 0;
        result.CanComplete = result.CanAccept && completionBlockers.Count == 0;
        result.AcceptanceBlockers = acceptanceBlockers;
        result.CompletionBlockers = completionBlockers;
        result.MissingRequirements = missingRequirements;

        return result;
    }

    private bool PlayerHasEquipmentCategory(Player player, EquipmentCategory category)
    {
        // Check if player has any items with the required equipment category
        // NOTE: This implementation assumes item IDs in inventory can be resolved to Item objects
        // A proper implementation would need ItemRepository access for GetItemById()
        
        // For now, implement basic logic based on common item naming patterns
        // This is a temporary solution until proper repository access is established
        
        string categoryName = category.ToString().ToLower();
        
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (string.IsNullOrEmpty(itemId)) continue;
            
            // Basic pattern matching for common equipment categories
            // This is a simplified approach that should be replaced with proper Item object access
            string itemIdLower = itemId.ToLower();
            
            bool hasCategory = category switch
            {
                EquipmentCategory.Climbing_Equipment => itemIdLower.Contains("rope") || itemIdLower.Contains("climbing") || itemIdLower.Contains("grapple"),
                EquipmentCategory.Navigation_Tools => itemIdLower.Contains("compass") || itemIdLower.Contains("map") || itemIdLower.Contains("navigation"),
                EquipmentCategory.Weather_Protection => itemIdLower.Contains("cloak") || itemIdLower.Contains("coat") || itemIdLower.Contains("weather"),
                EquipmentCategory.Water_Transport => itemIdLower.Contains("boat") || itemIdLower.Contains("raft") || itemIdLower.Contains("ferry"),
                EquipmentCategory.Light_Source => itemIdLower.Contains("torch") || itemIdLower.Contains("lantern") || itemIdLower.Contains("candle"),
                _ => false
            };
            
            if (hasCategory) return true;
        }
        
        return false;
        
        // TODO: Replace with proper implementation:
        // ItemRepository itemRepo = ...; // Need dependency injection
        // foreach (string itemId in player.Inventory.ItemSlots)
        // {
        //     Item item = itemRepo.GetItemById(itemId);
        //     if (item?.HasEquipmentCategory(category) == true) return true;
        // }
        // return false;
    }

    private bool PlayerHasToolCategory(Player player, ToolCategory category)
    {
        // Check if player has any items with the required tool category
        // NOTE: From Item.HasToolCategory(), tools are handled differently from equipment
        // ToolCategory represents general tool needs, not specific item categories
        
        // For now, implement basic logic based on common tool patterns
        // This is a temporary solution until proper tool-item mapping is established
        
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (string.IsNullOrEmpty(itemId)) continue;
            
            string itemIdLower = itemId.ToLower();
            
            bool hasTool = category switch
            {
                ToolCategory.Basic_Tools => itemIdLower.Contains("hammer") || itemIdLower.Contains("knife") || itemIdLower.Contains("tool"),
                ToolCategory.Specialized_Equipment => itemIdLower.Contains("specialized") || itemIdLower.Contains("professional") || itemIdLower.Contains("master"),
                ToolCategory.Quality_Materials => itemIdLower.Contains("silk") || itemIdLower.Contains("quality") || itemIdLower.Contains("fine"),
                ToolCategory.Documentation => itemIdLower.Contains("permit") || itemIdLower.Contains("document") || itemIdLower.Contains("contract"),
                ToolCategory.Trade_Samples => itemIdLower.Contains("sample") || itemIdLower.Contains("specimen") || itemIdLower.Contains("display"),
                ToolCategory.Writing_Materials => itemIdLower.Contains("ink") || itemIdLower.Contains("quill") || itemIdLower.Contains("parchment"),
                ToolCategory.Measurement_Tools => itemIdLower.Contains("scale") || itemIdLower.Contains("ruler") || itemIdLower.Contains("measure"),
                ToolCategory.Safety_Equipment => itemIdLower.Contains("helmet") || itemIdLower.Contains("safety") || itemIdLower.Contains("protective"),
                ToolCategory.Social_Attire => itemIdLower.Contains("formal") || itemIdLower.Contains("noble") || itemIdLower.Contains("dress"),
                ToolCategory.Crafting_Supplies => itemIdLower.Contains("material") || itemIdLower.Contains("supplies") || itemIdLower.Contains("raw"),
                _ => false
            };
            
            if (hasTool) return true;
        }
        
        return false;
        
        // TODO: Replace with proper implementation:
        // 1. Define mapping between Item properties and ToolCategory requirements
        // 2. Either extend Item class with tool category mappings, or
        // 3. Create a ToolMapping service that can determine if an Item fulfills a ToolCategory
        // 4. Access ItemRepository to get actual Item objects for validation
    }

    private bool PlayerMeetsSocialRequirement(Player player, SocialRequirement requirement)
    {
        // Check if player's social standing meets the requirement
        // Based on reputation level, archetype, and equipped items with social signaling
        
        if (requirement == SocialRequirement.Any)
            return true;
        
        // Get base social level from player archetype
        SocialRequirement archetypeLevel = GetArchetypeSocialLevel(player.Archetype);
        
        // Get reputation-based social level
        SocialRequirement reputationLevel = GetReputationSocialLevel(player.GetReputationLevel());
        
        // Get equipment-based social level (would need ItemRepository access for full implementation)
        SocialRequirement equipmentLevel = GetEquipmentSocialLevel(player);
        
        // Take the highest social level from all sources
        SocialRequirement playerSocialLevel = GetHighestSocialLevel(archetypeLevel, reputationLevel, equipmentLevel);
        
        // Check if player meets requirement
        return playerSocialLevel >= requirement;
    }
    
    private SocialRequirement GetArchetypeSocialLevel(Professions archetype)
    {
        return archetype switch
        {
            Professions.Courtier => SocialRequirement.Minor_Noble,
            Professions.Scholar => SocialRequirement.Professional,
            Professions.Merchant => SocialRequirement.Merchant_Class,
            Professions.Warrior => SocialRequirement.Commoner,  // Warriors are not automatically high social standing
            Professions.Ranger => SocialRequirement.Commoner,
            Professions.Thief => SocialRequirement.Any,         // Thieves may have no social standing
            _ => SocialRequirement.Any
        };
    }
    
    private SocialRequirement GetReputationSocialLevel(ReputationLevel reputation)
    {
        return reputation switch
        {
            ReputationLevel.Revered => SocialRequirement.Minor_Noble,
            ReputationLevel.Respected => SocialRequirement.Professional,
            ReputationLevel.Trusted => SocialRequirement.Merchant_Class,
            ReputationLevel.Neutral => SocialRequirement.Commoner,
            _ => SocialRequirement.Any
        };
    }
    
    private SocialRequirement GetEquipmentSocialLevel(Player player)
    {
        // Basic implementation using item name patterns
        // TODO: Replace with proper Item.SocialSignaling property access via ItemRepository
        
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (string.IsNullOrEmpty(itemId)) continue;
            
            string itemIdLower = itemId.ToLower();
            
            if (itemIdLower.Contains("noble") || itemIdLower.Contains("royal") || itemIdLower.Contains("court"))
                return SocialRequirement.Minor_Noble;
            if (itemIdLower.Contains("professional") || itemIdLower.Contains("guild") || itemIdLower.Contains("formal"))
                return SocialRequirement.Professional;
            if (itemIdLower.Contains("merchant") || itemIdLower.Contains("trade") || itemIdLower.Contains("quality"))
                return SocialRequirement.Merchant_Class;
        }
        
        return SocialRequirement.Any;
    }
    
    private SocialRequirement GetHighestSocialLevel(params SocialRequirement[] levels)
    {
        return levels.Max();
    }

    private bool PlayerHasRequiredInformation(Player player, InformationRequirementData requirement)
    {
        // Check if player has information matching the type, quality, and freshness requirements
        return player.KnownInformation.Any(info => 
            info.Type == requirement.RequiredType &&
            info.Quality >= requirement.MinimumQuality &&
            info.Freshness >= requirement.MinimumFreshness &&
            (string.IsNullOrEmpty(requirement.SpecificInformationId) || info.Id == requirement.SpecificInformationId));
    }

    private bool PlayerMeetsKnowledgeRequirement(Player player, KnowledgeRequirement requirement)
    {
        // Check if player's knowledge level meets the requirement
        // Based on archetype, skills, and known information
        
        if (requirement == KnowledgeRequirement.None)
            return true;
        
        // Get base knowledge level from player archetype
        KnowledgeRequirement archetypeLevel = GetArchetypeKnowledgeLevel(player.Archetype);
        
        // Get knowledge level from information collection
        KnowledgeRequirement informationLevel = GetInformationKnowledgeLevel(player);
        
        // Take the highest knowledge level from all sources
        KnowledgeRequirement playerKnowledgeLevel = GetHighestKnowledgeLevel(archetypeLevel, informationLevel);
        
        // Check if player meets requirement
        return playerKnowledgeLevel >= requirement;
    }
    
    private KnowledgeRequirement GetArchetypeKnowledgeLevel(Professions archetype)
    {
        return archetype switch
        {
            Professions.Scholar => KnowledgeRequirement.Expert,
            Professions.Courtier => KnowledgeRequirement.Advanced,
            Professions.Merchant => KnowledgeRequirement.Commercial,
            Professions.Warrior => KnowledgeRequirement.Professional,
            Professions.Ranger => KnowledgeRequirement.Local,
            Professions.Thief => KnowledgeRequirement.Basic,
            _ => KnowledgeRequirement.None
        };
    }
    
    private KnowledgeRequirement GetInformationKnowledgeLevel(Player player)
    {
        // Calculate knowledge level based on information collection quality and quantity
        int expertCount = player.KnownInformation.Count(i => i.Quality >= InformationQuality.Expert);
        int verifiedCount = player.KnownInformation.Count(i => i.Quality >= InformationQuality.Verified);
        int totalCount = player.KnownInformation.Count;
        
        // High-quality information indicates higher knowledge levels
        if (expertCount >= 3) return KnowledgeRequirement.Expert;
        if (expertCount >= 1 || verifiedCount >= 5) return KnowledgeRequirement.Advanced;
        if (verifiedCount >= 2 || totalCount >= 8) return KnowledgeRequirement.Professional;
        if (totalCount >= 4) return KnowledgeRequirement.Basic;
        
        return KnowledgeRequirement.None;
    }
    
    private KnowledgeRequirement GetHighestKnowledgeLevel(params KnowledgeRequirement[] levels)
    {
        return levels.Max();
    }
}

/// <summary>
/// Represents a market transaction requirement for contract completion
/// </summary>
public class ContractTransaction
{
    public string ItemId { get; set; } = "";
    public string LocationId { get; set; } = "";
    public TransactionType TransactionType { get; set; }
    public int Quantity { get; set; } = 1;
    public int? MinPrice { get; set; } // Minimum price for sell transactions
    public int? MaxPrice { get; set; } // Maximum price for buy transactions
    
    public ContractTransaction() { }
    
    public ContractTransaction(string itemId, string locationId, TransactionType transactionType, int quantity = 1)
    {
        ItemId = itemId;
        LocationId = locationId;
        TransactionType = transactionType;
        Quantity = quantity;
    }
    
    /// <summary>
    /// Check if this transaction matches the requirement
    /// </summary>
    public bool Matches(string itemId, string locationId, TransactionType transactionType, int quantity, int price)
    {
        if (ItemId != itemId || LocationId != locationId || TransactionType != transactionType)
            return false;
            
        if (quantity < Quantity)
            return false;
            
        // Check price constraints
        if (TransactionType == TransactionType.Buy && MaxPrice.HasValue && price > MaxPrice.Value)
            return false;
            
        if (TransactionType == TransactionType.Sell && MinPrice.HasValue && price < MinPrice.Value)
            return false;
            
        return true;
    }
}

/// <summary>
/// Types of market transactions
/// </summary>
public enum TransactionType
{
    Buy,
    Sell
}