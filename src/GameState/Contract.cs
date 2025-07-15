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

    // === UNIFIED CONTRACT STEP SYSTEM ===

    /// <summary>
    /// All steps that must be completed to fulfill this contract.
    /// Each contract must have at least one completion step.
    /// </summary>
    public List<ContractStep> CompletionSteps { get; set; } = new List<ContractStep>();


    /// <summary>
    /// Check if the player's action completes any contract steps
    /// </summary>
    public bool CheckStepCompletion(Player player, string currentLocationId, object actionContext = null, ItemRepository itemRepository = null)
    {
        bool anyCompleted = false;

        foreach (ContractStep step in CompletionSteps.Where(s => !s.IsCompleted))
        {
            if (step.CheckCompletion(player, currentLocationId, actionContext, itemRepository))
            {
                anyCompleted = true;
            }
        }

        return anyCompleted;
    }

    /// <summary>
    /// Get all completion requirements for this contract
    /// </summary>
    public List<ContractStepRequirement> GetCompletionRequirements()
    {
        return CompletionSteps.Select(step => step.GetRequirement()).ToList();
    }

    /// <summary>
    /// Get remaining (incomplete) requirements for this contract
    /// </summary>
    public List<ContractStepRequirement> GetRemainingRequirements()
    {
        return CompletionSteps
            .Where(step => step.IsRequired && !step.IsCompleted)
            .Select(step => step.GetRequirement())
            .ToList();
    }

    /// <summary>
    /// Reset all contract steps to incomplete (for contract resets)
    /// </summary>
    public void ResetProgress()
    {
        foreach (ContractStep step in CompletionSteps)
        {
            step.Reset();
        }

        IsCompleted = false;
        IsFailed = false;
        CompletedDay = -1;
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

    public bool CanComplete(Player player, string currentLocationId, ItemRepository itemRepository)
    {
        ContractAccessResult result = GetAccessResult(player, currentLocationId, itemRepository);
        return result.CanComplete;
    }

    /// <summary>
    /// Provides detailed analysis of contract accessibility and completion requirements
    /// </summary>
    public ContractAccessResult GetAccessResult(Player player, string currentLocationId, ItemRepository itemRepository)
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
            if (!PlayerHasEquipmentCategory(player, equipmentCategory, itemRepository))
            {
                string categoryName = equipmentCategory.ToString().Replace("_", " ");
                missingRequirements.Add($"Requires {categoryName} equipment");
                completionBlockers.Add($"Missing required equipment category: {categoryName}");
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

    private bool PlayerHasEquipmentCategory(Player player, EquipmentCategory category, ItemRepository itemRepository)
    {
        // Check if player has any items with the required equipment category
        // Uses proper repository access to get actual Item objects and check their categories
        
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (string.IsNullOrEmpty(itemId)) continue;
            
            Item item = itemRepository.GetItemById(itemId);
            if (item?.HasEquipmentCategory(category) == true)
            {
                return true;
            }
        }
        
        return false;
    }


    private bool PlayerHasRequiredInformation(Player player, InformationRequirementData requirement)
    {
        // Check if player has information matching the type, quality, and freshness requirements
        return player.KnownInformation.Any(info =>
            info.Type == requirement.RequiredType &&
            info.Quality >= requirement.MinimumQuality &&
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
            Professions.Merchant => KnowledgeRequirement.Commercial,
            Professions.Ranger => KnowledgeRequirement.Local,
            _ => KnowledgeRequirement.None
        };
    }

    private KnowledgeRequirement GetInformationKnowledgeLevel(Player player)
    {
        // Calculate knowledge level based on information collection quality and quantity
        int expertCount = player.KnownInformation.Count(i => i.Quality >= InformationQuality.Expert);
        int verifiedCount = player.KnownInformation.Count(i => i.Quality >= InformationQuality.Verified);
        int totalCount = player.KnownInformation.Count;

        return KnowledgeRequirement.None;
    }

    private KnowledgeRequirement GetHighestKnowledgeLevel(params KnowledgeRequirement[] levels)
    {
        return levels.Max();
    }

    internal bool IsFullyCompleted()
    {
        // Contract is fully completed when all required steps are completed
        // Optional steps (IsRequired = false) don't prevent completion
        return CompletionSteps.Where(step => step.IsRequired).All(step => step.IsCompleted);
    }
}
