using Wayfarer.Game.ActionSystem;
using Wayfarer.Game.MainSystem;

public class ActionFactory
{
    private WorldState worldState;
    private ItemRepository itemRepository;
    private ContractRepository contractRepository;
    private ContractValidationService contractValidationService;

    public ActionFactory(
        ActionRepository actionRepository,
        GameWorld gameWorld,
        ItemRepository itemRepository,
        ContractRepository contractRepository,
        ContractValidationService contractValidationService)
    {
        this.worldState = gameWorld.WorldState;
        this.itemRepository = itemRepository;
        this.contractRepository = contractRepository;
        this.contractValidationService = contractValidationService;
    }

    public LocationAction CreateActionFromTemplate(
        ActionDefinition template,
        string location,
        string locationSpot,
        ActionExecutionTypes actionType)
    {
        LocationAction locationAction = new LocationAction();
        locationAction.ActionId = template.Id;
        locationAction.Name = template.Name;
        locationAction.ObjectiveDescription = template.Description;
        locationAction.LocationId = location;
        locationAction.LocationSpotId = locationSpot;

        if (!string.IsNullOrEmpty(template.MoveToLocation))
        {
            locationAction.DestinationLocation = template.MoveToLocation;
        }
        if (!string.IsNullOrEmpty(template.MoveToLocationSpot))
        {
            locationAction.DestinationLocationSpot = template.MoveToLocationSpot;
        }

        locationAction.RequiredCardType = SkillCategories.Physical;
        locationAction.ActionExecutionType = actionType;

        locationAction.Requirements = CreateRequirements(template);
        locationAction.Effects = CreateEffects(template);
        locationAction.ActionPointCost = template.ActionPointCost;
        locationAction.SilverCost = template.SilverCost;
        locationAction.RefreshCardType = template.RefreshCardType;
        locationAction.StaminaCost = template.StaminaCost;
        locationAction.ConcentrationCost = template.ConcentrationCost;
        locationAction.PhysicalDemand = template.PhysicalDemand;

        locationAction.Requirements.Add(new ActionPointRequirement(template.ActionPointCost));
        return locationAction;
    }

    private List<IRequirement> CreateRequirements(ActionDefinition template)
    {
        List<IRequirement> requirements = new();
        
        // === CATEGORICAL REQUIREMENTS ===
        // Social access requirements
        if (template.SocialRequirement != SocialRequirement.Any)
        {
            requirements.Add(new SocialAccessRequirement(template.SocialRequirement, itemRepository));
        }
        
        // Tool category requirements  
        foreach (var toolCategory in template.ToolRequirements)
        {
            requirements.Add(new ToolCategoryRequirement(toolCategory, itemRepository));
        }

        // Equipment category requirements
        foreach (var equipmentCategory in template.EquipmentRequirements)
        {
            requirements.Add(new EquipmentCategoryRequirement(equipmentCategory, itemRepository));
        }
        
        // Environment requirements
        foreach (var envCategory in template.EnvironmentRequirements)
        {
            requirements.Add(new EnvironmentRequirement(envCategory));
        }
        
        // Knowledge requirements
        if (template.KnowledgeRequirement != KnowledgeRequirement.None)
        {
            requirements.Add(new KnowledgeLevelRequirement(template.KnowledgeRequirement));
        }

        // Stamina categorical requirements
        if (template.PhysicalDemand != PhysicalDemand.None)
        {
            requirements.Add(new StaminaCategoricalRequirement(template.PhysicalDemand));
        }

        // Information requirements
        foreach (var infoReq in template.InformationRequirements)
        {
            requirements.Add(new InformationRequirement(
                infoReq.RequiredType, 
                infoReq.MinimumQuality, 
                infoReq.MinimumFreshness,
                string.IsNullOrEmpty(infoReq.SpecificInformationId) ? null : infoReq.SpecificInformationId));
        }
        
        // === EXISTING NUMERICAL REQUIREMENTS ===
        // Time window requirement
        if (template.CurrentTimeBlocks != null && template.CurrentTimeBlocks.Count > 0)
        {
            requirements.Add(new CurrentTimeBlockRequirement(template.CurrentTimeBlocks));
        }
        
        // Resource requirements (keeping during transition)
        if (template.StaminaCost > 0)
        {
            requirements.Add(new StaminaRequirement(template.StaminaCost));
        }
        if (template.SilverCost > 0)
        {
            requirements.Add(new CoinRequirement(template.SilverCost));
        }
        
        return requirements;
    }

    private List<IMechanicalEffect> CreateEffects(ActionDefinition template)
    {
        List<IMechanicalEffect> effects = new();
        
        // === CATEGORICAL EFFECTS ===
        // Physical recovery effects based on effect categories
        foreach (var effectCategory in template.EffectCategories)
        {
            switch (effectCategory)
            {
                case EffectCategory.Physical_Recovery:
                    // Create stamina recovery effect based on physical demand level
                    int recoveryAmount = GetPhysicalRecoveryAmount(template.PhysicalDemand);
                    if (recoveryAmount > 0)
                    {
                        effects.Add(new PhysicalRecoveryEffect(recoveryAmount, "rest and recuperation"));
                    }
                    break;
                    
                case EffectCategory.Social_Standing:
                    // Create social standing effect based on social context
                    if (template.SocialRequirement != SocialRequirement.Any)
                    {
                        int reputationGain = GetSocialStandingGain(template.SocialRequirement);
                        effects.Add(new SocialStandingEffect(reputationGain, template.SocialRequirement, "social interaction"));
                    }
                    break;
                    
                case EffectCategory.Knowledge_Gain:
                    // Future: Implement knowledge gain effects for learning actions
                    break;
                    
                case EffectCategory.Relationship_Building:
                    // Future: Implement relationship effects for social actions
                    break;
                    
                case EffectCategory.Contract_Discovery:
                    // Contract discovery effects are handled separately below
                    // (Added here for completeness, actual processing after information effects)
                    break;
            }
        }

        // Information effects
        foreach (var infoEffect in template.InformationEffects)
        {
            // Create Information object from effect data
            Information information = new Information(infoEffect.InformationId, infoEffect.Title, infoEffect.Type)
            {
                Content = infoEffect.Content,
                Quality = infoEffect.Quality,
                Freshness = infoEffect.Freshness,
                Source = infoEffect.Source,
                Value = infoEffect.Value,
                LocationId = infoEffect.LocationId,
                NPCId = infoEffect.NPCId
            };
            
            information.RelatedItemIds.AddRange(infoEffect.RelatedItemIds);
            information.RelatedLocationIds.AddRange(infoEffect.RelatedLocationIds);
            
            effects.Add(new InformationEffect(information, infoEffect.UpgradeExisting));
        }
        
        // Contract discovery effects
        foreach (var contractDiscovery in template.ContractDiscoveryEffects)
        {
            effects.Add(new ContractDiscoveryEffect(
                contractDiscovery.NPCId,
                contractDiscovery.ContractCategory,
                contractDiscovery.MaxContractsRevealed,
                contractRepository,
                contractValidationService));
        }
        
        return effects;
    }

    private int GetPhysicalRecoveryAmount(PhysicalDemand demand)
    {
        // Rest actions provide more recovery for less demanding activities
        return demand switch
        {
            PhysicalDemand.None => 2,           // Non-physical activities allow recovery
            PhysicalDemand.Light => 1,          // Light activity with some recovery
            PhysicalDemand.Moderate => 0,       // Moderate activity with no recovery
            PhysicalDemand.Heavy => 0,          // Heavy activity with no recovery
            PhysicalDemand.Extreme => 0,        // Extreme activity with no recovery
            _ => 0                              // Default no recovery
        };
    }

    private int GetSocialStandingGain(SocialRequirement requirement)
    {
        // Higher social levels provide more reputation gain
        return requirement switch
        {
            SocialRequirement.Major_Noble => 3,
            SocialRequirement.Minor_Noble => 2,
            SocialRequirement.Professional => 2,
            SocialRequirement.Guild_Member => 2,
            SocialRequirement.Merchant_Class => 1,
            SocialRequirement.Artisan_Class => 1,
            _ => 1
        };
    }

}