using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Factory for creating action definitions with guaranteed valid references.
/// Action definitions reference location spots and can reference locations for movement.
/// </summary>
public class ActionDefinitionFactory
{
    public ActionDefinitionFactory()
    {
        // No dependencies - factory is stateless
    }
    
    /// <summary>
    /// Create an action definition with validated references
    /// </summary>
    public ActionDefinition CreateActionDefinition(
        string id,
        string name,
        string description,
        LocationSpot locationSpot,
        int silverCost = 0,
        SkillCategories refreshCardType = SkillCategories.None,
        int staminaCost = 0,
        int concentrationCost = 0,
        IEnumerable<TimeBlocks> timeBlocks = null,
        IEnumerable<string> contextTags = null,
        IEnumerable<string> domainTags = null,
        Location moveToLocation = null,
        LocationSpot moveToLocationSpot = null,
        PhysicalDemand physicalDemand = PhysicalDemand.None,
        IEnumerable<ItemCategory> itemRequirements = null,
        KnowledgeRequirement knowledgeRequirement = KnowledgeRequirement.None,
        TimeInvestment timeInvestment = TimeInvestment.Standard,
        IEnumerable<EffectCategory> effectCategories = null)
    {
        if (string.IsNullOrEmpty(id))
            throw new ArgumentException("Action ID cannot be empty", nameof(id));
        if (string.IsNullOrEmpty(name))
            throw new ArgumentException("Action name cannot be empty", nameof(name));
        if (locationSpot == null)
            throw new ArgumentNullException(nameof(locationSpot), "Location spot cannot be null");
        
        var action = new ActionDefinition(id, name, locationSpot.SpotID)
        {
            Description = description ?? $"Perform {name}",
            SilverCost = silverCost,
            RefreshCardType = refreshCardType,
            StaminaCost = staminaCost,
            ConcentrationCost = concentrationCost,
            CurrentTimeBlocks = timeBlocks?.ToList() ?? new List<TimeBlocks>(),
            ContextTags = contextTags?.ToList() ?? new List<string>(),
            DomainTags = domainTags?.ToList() ?? new List<string>(),
            MoveToLocation = moveToLocation?.Id,
            MoveToLocationSpot = moveToLocationSpot?.SpotID,
            PhysicalDemand = physicalDemand,
            ItemRequirements = itemRequirements?.ToList() ?? new List<ItemCategory>(),
            KnowledgeRequirement = knowledgeRequirement,
            TimeInvestment = timeInvestment,
            EffectCategories = effectCategories?.ToList() ?? new List<EffectCategory>()
        };
        
        return action;
    }
    
    /// <summary>
    /// Create an action definition from string IDs with validation
    /// </summary>
    public ActionDefinition CreateActionDefinitionFromIds(
        string id,
        string name,
        string description,
        string locationSpotId,
        IEnumerable<LocationSpot> availableSpots,
        IEnumerable<Location> availableLocations,
        ActionDefinitionDTO dto)
    {
        // Validate location spot
        var locationSpot = availableSpots.FirstOrDefault(s => s.SpotID == locationSpotId);
        if (locationSpot == null)
        {
            Console.WriteLine($"WARNING: Action '{id}' references non-existent location spot '{locationSpotId}'");
            // Can't create action without valid location spot
            throw new InvalidOperationException($"Cannot create action '{id}': location spot '{locationSpotId}' not found");
        }
        
        // Parse refresh card type
        SkillCategories refreshCardType = SkillCategories.None;
        if (!string.IsNullOrEmpty(dto.RefreshCardType))
        {
            if (!Enum.TryParse<SkillCategories>(dto.RefreshCardType, out refreshCardType))
            {
                Console.WriteLine($"WARNING: Unknown refresh card type '{dto.RefreshCardType}' for action '{id}'");
            }
        }
        
        // Parse time blocks
        var timeBlocks = new List<TimeBlocks>();
        foreach (var timeStr in dto.CurrentTimeBlocks ?? new List<string>())
        {
            if (Enum.TryParse<TimeBlocks>(timeStr, out var timeBlock))
            {
                timeBlocks.Add(timeBlock);
            }
            else
            {
                Console.WriteLine($"WARNING: Unknown time block '{timeStr}' for action '{id}'");
            }
        }
        
        // Validate movement locations
        Location moveToLocation = null;
        if (!string.IsNullOrEmpty(dto.MoveToLocation))
        {
            moveToLocation = availableLocations.FirstOrDefault(l => l.Id == dto.MoveToLocation);
            if (moveToLocation == null)
            {
                Console.WriteLine($"WARNING: Action '{id}' references non-existent move-to location '{dto.MoveToLocation}'");
            }
        }
        
        LocationSpot moveToSpot = null;
        if (!string.IsNullOrEmpty(dto.MoveToLocationSpot))
        {
            moveToSpot = availableSpots.FirstOrDefault(s => s.SpotID == dto.MoveToLocationSpot);
            if (moveToSpot == null)
            {
                Console.WriteLine($"WARNING: Action '{id}' references non-existent move-to spot '{dto.MoveToLocationSpot}'");
            }
        }
        
        // Parse physical demand
        PhysicalDemand physicalDemand = PhysicalDemand.None;
        if (!string.IsNullOrEmpty(dto.PhysicalDemand))
        {
            if (!Enum.TryParse<PhysicalDemand>(dto.PhysicalDemand, out physicalDemand))
            {
                Console.WriteLine($"WARNING: Unknown physical demand '{dto.PhysicalDemand}' for action '{id}'");
            }
        }
        
        // Parse item requirements
        var itemRequirements = new List<ItemCategory>();
        foreach (var itemStr in dto.ItemRequirements ?? new List<string>())
        {
            if (Enum.TryParse<ItemCategory>(itemStr.Replace(" ", "_"), out var itemCat))
            {
                itemRequirements.Add(itemCat);
            }
            else
            {
                Console.WriteLine($"WARNING: Unknown item requirement '{itemStr}' for action '{id}'");
            }
        }
        
        // Parse knowledge requirement
        KnowledgeRequirement knowledgeRequirement = KnowledgeRequirement.None;
        if (!string.IsNullOrEmpty(dto.KnowledgeRequirement))
        {
            if (!Enum.TryParse<KnowledgeRequirement>(dto.KnowledgeRequirement, out knowledgeRequirement))
            {
                Console.WriteLine($"WARNING: Unknown knowledge requirement '{dto.KnowledgeRequirement}' for action '{id}'");
            }
        }
        
        // Parse time investment
        TimeInvestment timeInvestment = TimeInvestment.Standard;
        if (!string.IsNullOrEmpty(dto.TimeInvestment))
        {
            if (!Enum.TryParse<TimeInvestment>(dto.TimeInvestment, out timeInvestment))
            {
                Console.WriteLine($"WARNING: Unknown time investment '{dto.TimeInvestment}' for action '{id}'");
            }
        }
        
        // Parse effect categories
        var effectCategories = new List<EffectCategory>();
        foreach (var effectStr in dto.EffectCategories ?? new List<string>())
        {
            if (Enum.TryParse<EffectCategory>(effectStr, out var effectCat))
            {
                effectCategories.Add(effectCat);
            }
            else
            {
                Console.WriteLine($"WARNING: Unknown effect category '{effectStr}' for action '{id}'");
            }
        }
        
        return CreateActionDefinition(
            id,
            name,
            description,
            locationSpot,
            dto.SilverCost,
            refreshCardType,
            dto.StaminaCost,
            dto.ConcentrationCost,
            timeBlocks,
            dto.ContextTags,
            dto.DomainTags,
            moveToLocation,
            moveToSpot,
            physicalDemand,
            itemRequirements,
            knowledgeRequirement,
            timeInvestment,
            effectCategories);
    }
}