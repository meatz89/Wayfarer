using System;
using System.Collections.Generic;
using System.Linq;
using Wayfarer.Game.ActionSystem;

namespace Wayfarer.Game.MainSystem;

/// <summary>
/// Generates renewable contracts from templates based on NPC categories and game state
/// </summary>
public class ContractGenerator
{
    private readonly List<Contract> _contractTemplates;
    private readonly ContractRepository _contractRepository;
    private readonly Random _random;

    public ContractGenerator(List<Contract> contractTemplates, ContractRepository contractRepository)
    {
        _contractTemplates = contractTemplates ?? new List<Contract>();
        _contractRepository = contractRepository;
        _random = new Random();
    }

    /// <summary>
    /// Generate renewable contracts for a specific NPC based on their contract categories
    /// </summary>
    public List<Contract> GenerateRenewableContracts(NPC npc, int currentDay, int maxContracts = 2)
    {
        if (npc.ContractCategories == null || !npc.ContractCategories.Any())
        {
            return new List<Contract>();
        }

        List<Contract> generatedContracts = new List<Contract>();

        // Generate 1-2 contracts per NPC per day based on their categories
        int contractsToGenerate = Math.Min(_random.Next(1, maxContracts + 1), npc.ContractCategories.Count);

        for (int i = 0; i < contractsToGenerate; i++)
        {
            // Pick a random category from NPC's available categories
            string categoryName = npc.ContractCategories[_random.Next(npc.ContractCategories.Count)];
            
            // Find templates matching this category
            List<Contract> matchingTemplates = _contractTemplates
                .Where(template => template.Id.Contains(categoryName.ToLower()) || 
                                 template.Description.Contains(categoryName, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (matchingTemplates.Any())
            {
                Contract template = matchingTemplates[_random.Next(matchingTemplates.Count)];
                Contract generatedContract = CreateContractFromTemplate(template, npc, currentDay);
                
                if (generatedContract != null)
                {
                    generatedContracts.Add(generatedContract);
                }
            }
        }

        return generatedContracts;
    }

    /// <summary>
    /// Create a unique contract instance from a template with NPC-specific modifications
    /// </summary>
    public Contract CreateContractFromTemplate(Contract template, NPC npc, int currentDay)
    {
        if (template == null || npc == null)
        {
            return null;
        }

        // Create unique contract ID
        string uniqueId = $"{npc.ID}_{template.Id}_{currentDay}_{_random.Next(1000, 9999)}";

        // Create new contract instance
        Contract newContract = new Contract
        {
            Id = uniqueId,
            Description = GenerateContractDescription(template, npc),
            StartDay = currentDay,
            DueDay = currentDay + GetTemplateDuration(template),
            Payment = CalculateContractPayment(template, npc, currentDay),
            FailurePenalty = template.FailurePenalty,
            IsCompleted = false,
            IsFailed = false,

            // Copy categorical requirements from template
            RequiredEquipmentCategories = new List<EquipmentCategory>(template.RequiredEquipmentCategories),
            RequiredToolCategories = new List<ToolCategory>(template.RequiredToolCategories),
            PhysicalRequirement = template.PhysicalRequirement,
            RequiredInformation = new List<InformationRequirementData>(template.RequiredInformation),
            RequiredKnowledge = template.RequiredKnowledge,
            Category = template.Category,
            Priority = template.Priority,
            RiskLevel = template.RiskLevel,

            // Copy completion steps from template
            CompletionSteps = template.CompletionSteps.Select(step => CloneContractStep(step)).ToList()
        };

        return newContract;
    }

    /// <summary>
    /// Generate NPC-specific contract description
    /// </summary>
    private string GenerateContractDescription(Contract template, NPC npc)
    {
        string baseDescription = template.Description;
        
        // Add NPC-specific flavor to the description
        string npcFlavor = npc.Name switch
        {
            "Workshop Master" => "commissioned by the Workshop Master",
            "Market Trader" => "arranged by the Market Trader",
            "Tavern Keeper" => "requested by the Tavern Keeper",
            "Logger" => "posted by the Logger",
            "Herb Gatherer" => "offered by the Herb Gatherer",
            "Camp Boss" => "assigned by the Camp Boss",
            "Dock Master" => "arranged by the Dock Master",
            "Trade Captain" => "contracted by the Trade Captain",
            "River Worker" => "requested by the River Worker",
            _ => $"offered by {npc.Name}"
        };

        return $"{baseDescription} - {npcFlavor}";
    }

    /// <summary>
    /// Calculate contract payment based on template, NPC reputation, and game progression
    /// </summary>
    private int CalculateContractPayment(Contract template, NPC npc, int currentDay)
    {
        int basePayment = GetTemplatePayment(template);
        
        // Game progression scaling - slightly higher payments as game progresses
        float progressionMultiplier = 1.0f + (currentDay / 30.0f * 0.2f); // 20% increase over 30 days
        
        // NPC relationship modifier
        float relationshipMultiplier = npc.PlayerRelationship switch
        {
            NPCRelationship.Helpful => 1.1f,
            NPCRelationship.Neutral => 1.0f,
            NPCRelationship.Wary => 0.9f,
            NPCRelationship.Hostile => 0.8f,
            _ => 1.0f
        };

        // Small random variation (±10%)
        float randomVariation = 0.9f + (float)_random.NextDouble() * 0.2f;

        int finalPayment = (int)(basePayment * progressionMultiplier * relationshipMultiplier * randomVariation);
        
        return Math.Max(1, finalPayment); // Ensure minimum payment of 1 coin
    }

    /// <summary>
    /// Clone a contract step for the new contract instance
    /// </summary>
    private ContractStep CloneContractStep(ContractStep originalStep)
    {
        // Create a basic clone - in a full implementation, this would need proper deep cloning
        // For now, we'll assume ContractStep has a copy constructor or implement basic cloning
        
        // This is a simplified approach - the actual implementation would depend on
        // the specific structure of ContractStep and its requirements
        return originalStep; // Placeholder - actual cloning would need to be implemented
    }

    /// <summary>
    /// Get all available contract templates for debugging/testing
    /// </summary>
    public List<Contract> GetContractTemplates()
    {
        return new List<Contract>(_contractTemplates);
    }

    /// <summary>
    /// Check if an NPC can offer contracts of a specific category
    /// </summary>
    public bool CanNPCOfferCategory(NPC npc, string categoryName)
    {
        return npc.ContractCategories?.Contains(categoryName, StringComparer.OrdinalIgnoreCase) == true;
    }

    /// <summary>
    /// Extract duration from contract template based on contract type
    /// </summary>
    private int GetTemplateDuration(Contract template)
    {
        // Extract duration from template ID or use default values
        if (template.Id.Contains("rush"))
            return 1;
        else if (template.Id.Contains("standard"))
            return 3;
        else if (template.Id.Contains("craft"))
            return 2;
        else if (template.Id.Contains("exploration"))
            return 5;
        else
            return 3; // Default duration
    }

    /// <summary>
    /// Extract payment from contract template based on contract type
    /// </summary>
    private int GetTemplatePayment(Contract template)
    {
        // Extract payment from template ID or use default values
        if (template.Id.Contains("rush"))
            return 15;
        else if (template.Id.Contains("standard"))
            return 8;
        else if (template.Id.Contains("craft"))
            return 12;
        else if (template.Id.Contains("exploration"))
            return 6;
        else
            return 8; // Default payment
    }
}