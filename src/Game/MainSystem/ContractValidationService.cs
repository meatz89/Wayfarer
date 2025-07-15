using Wayfarer.Content;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Game.MainSystem;

/// <summary>
/// Service responsible for validating contract requirements against player capabilities
/// using proper repository access patterns
/// </summary>
public class ContractValidationService
{
    private readonly ContractRepository _contractRepository;
    private readonly ItemRepository _itemRepository;

    public ContractValidationService(ContractRepository contractRepository, ItemRepository itemRepository)
    {
        _contractRepository = contractRepository;
        _itemRepository = itemRepository;
    }

    /// <summary>
    /// Validates contract access and completion requirements with proper repository-based checks
    /// </summary>
    public ContractAccessResult ValidateContractAccess(Contract contract, Player player, string currentLocationId)
    {
        ContractAccessResult result = new ContractAccessResult();
        List<string> acceptanceBlockers = new List<string>();
        List<string> completionBlockers = new List<string>();
        List<string> missingRequirements = new List<string>();
        List<string> warnings = new List<string>();

        // === BASIC REQUIREMENTS ===


        // === CATEGORICAL REQUIREMENTS ===

        // Equipment category validation
        foreach (EquipmentCategory equipmentCategory in contract.RequiredEquipmentCategories)
        {
            ValidationResult equipResult = ValidateEquipmentCategory(player, equipmentCategory);
            if (!equipResult.IsValid)
            {
                string categoryName = equipmentCategory.ToString().Replace("_", " ");
                missingRequirements.Add($"Requires {categoryName} equipment");
                completionBlockers.Add(equipResult.ErrorMessage);
            }
        }


        // Physical capability validation
        if (contract.PhysicalRequirement != PhysicalDemand.None)
        {
            if (!player.CanPerformStaminaAction(contract.PhysicalRequirement))
            {
                string physicalLevel = contract.PhysicalRequirement.ToString().Replace("_", " ");
                completionBlockers.Add($"Insufficient stamina for {physicalLevel} physical demands (requires {GetRequiredStamina(contract.PhysicalRequirement)} stamina)");
            }
        }

        // Information requirements validation
        foreach (InformationRequirementData infoReq in contract.RequiredInformation)
        {
            ValidationResult infoResult = ValidateInformationRequirement(player, infoReq);
            if (!infoResult.IsValid)
            {
                completionBlockers.Add(infoResult.ErrorMessage);
                string infoType = infoReq.RequiredType.ToString().Replace("_", " ");
                string qualityLevel = infoReq.MinimumQuality.ToString();
                missingRequirements.Add($"Requires {qualityLevel} quality {infoType} information");
            }
        }

        // Knowledge requirement validation
        if (contract.RequiredKnowledge != KnowledgeRequirement.None)
        {
            ValidationResult knowledgeResult = ValidateKnowledgeRequirement(player, contract.RequiredKnowledge);
            if (!knowledgeResult.IsValid)
            {
                acceptanceBlockers.Add(knowledgeResult.ErrorMessage);
            }
        }

        // === RISK ASSESSMENT WARNINGS ===
        AddRiskWarnings(contract, warnings);

        // Set results
        result.CanAccept = acceptanceBlockers.Count == 0;
        result.CanComplete = result.CanAccept && completionBlockers.Count == 0;
        result.AcceptanceBlockers = acceptanceBlockers;
        result.CompletionBlockers = completionBlockers;
        result.MissingRequirements = missingRequirements;
        result.Warnings = warnings;

        return result;
    }

    /// <summary>
    /// Validates if player has required equipment category using proper Item object access
    /// </summary>
    private ValidationResult ValidateEquipmentCategory(Player player, EquipmentCategory category)
    {
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (string.IsNullOrEmpty(itemId)) continue;

            Item item = _itemRepository.GetItemById(itemId);
            if (item?.HasEquipmentCategory(category) == true)
            {
                return ValidationResult.Valid();
            }
        }

        string categoryName = category.ToString().Replace("_", " ");
        return ValidationResult.Invalid($"Missing required equipment category: {categoryName}");
    }

    /// <summary>
    /// Validates if player has required tool category using proper Item-ToolCategory mapping
    /// </summary>
    private ValidationResult ValidateToolCategory(Player player, ToolCategory category)
    {
        foreach (string itemId in player.Inventory.ItemSlots)
        {
            if (string.IsNullOrEmpty(itemId)) continue;

            Item item = _itemRepository.GetItemById(itemId);
            if (item != null && ItemFulfillsToolCategory(item, category))
            {
                return ValidationResult.Valid();
            }
        }

        string categoryName = category.ToString().Replace("_", " ");
        return ValidationResult.Invalid($"Missing required tool category: {categoryName}");
    }

    private bool ItemFulfillsToolCategory(Item item, ToolCategory category)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Validates information requirements against player's known information
    /// </summary>
    private ValidationResult ValidateInformationRequirement(Player player, InformationRequirementData requirement)
    {
        bool hasMatchingInfo = player.KnownInformation.Any(info =>
            info.Type == requirement.RequiredType &&
            info.Quality >= requirement.MinimumQuality &&
            (string.IsNullOrEmpty(requirement.SpecificInformationId) || info.Id == requirement.SpecificInformationId));

        if (hasMatchingInfo)
        {
            return ValidationResult.Valid();
        }

        string infoType = requirement.RequiredType.ToString().Replace("_", " ");
        string qualityLevel = requirement.MinimumQuality.ToString();
        return ValidationResult.Invalid($"Missing {qualityLevel} quality {infoType} information");
    }

    /// <summary>
    /// Validates knowledge requirements based on archetype and information collection
    /// </summary>
    private ValidationResult ValidateKnowledgeRequirement(Player player, KnowledgeRequirement requirement)
    {
        if (requirement == KnowledgeRequirement.None)
            return ValidationResult.Valid();

        // Get base knowledge level from player archetype
        KnowledgeRequirement archetypeLevel = GetArchetypeKnowledgeLevel(player.Archetype);

        if (archetypeLevel >= requirement)
        {
            return ValidationResult.Valid();
        }

        string requiredLevel = requirement.ToString().Replace("_", " ");
        string currentLevel = archetypeLevel.ToString().Replace("_", " ");
        return ValidationResult.Invalid($"Requires {requiredLevel} knowledge level (current: {currentLevel})");
    }

    private void AddRiskWarnings(Contract contract, List<string> warnings)
    {
        if (contract.RiskLevel >= ContractRisk.High)
        {
            warnings.Add($"High risk contract - failure may result in equipment loss or injury");
        }

        if (contract.Priority >= ContractPriority.Urgent)
        {
            warnings.Add($"Urgent contract - severe reputation consequences for failure");
        }
    }

    private int GetRequiredStamina(PhysicalDemand demand)
    {
        return demand switch
        {
            PhysicalDemand.Light => 2,
            PhysicalDemand.Moderate => 4,
            PhysicalDemand.Heavy => 6,
            PhysicalDemand.Extreme => 8,
            _ => 0
        };
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

    private KnowledgeRequirement GetHighestKnowledgeLevel(params KnowledgeRequirement[] levels)
    {
        return levels.Max();
    }
}

/// <summary>
/// Result of a validation check
/// </summary>
public class ValidationResult
{
    public bool IsValid { get; private set; }
    public string ErrorMessage { get; private set; } = "";

    private ValidationResult(bool isValid, string errorMessage = "")
    {
        IsValid = isValid;
        ErrorMessage = errorMessage;
    }

    public static ValidationResult Valid()
    {
        return new ValidationResult(true);
    }

    public static ValidationResult Invalid(string errorMessage)
    {
        return new ValidationResult(false, errorMessage);
    }
}