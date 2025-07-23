using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wayfarer.GameState.Actions.Requirements;

namespace Wayfarer.GameState.Actions;

/// <summary>
/// Unified executor that validates prerequisites and executes actions
/// </summary>
public class UnifiedActionExecutor : IActionExecutor
{
    private readonly Dictionary<LocationAction, IActionExecutor> _executors;
    private readonly IGameRuleEngine _ruleEngine;
    private readonly MessageSystem _messageSystem;
    private readonly ActionPrerequisiteFactory _prerequisiteFactory;
    
    public UnifiedActionExecutor(
        IGameRuleEngine ruleEngine,
        MessageSystem messageSystem,
        ActionPrerequisiteFactory prerequisiteFactory,
        IEnumerable<IActionExecutor> executors)
    {
        _ruleEngine = ruleEngine;
        _messageSystem = messageSystem;
        _prerequisiteFactory = prerequisiteFactory;
        _executors = executors.ToDictionary(e => GetActionType(e), e => e);
    }
    
    public async Task<ActionExecutionResult> Execute(ActionOption action, Player player, GameWorld world)
    {
        // 1. Build prerequisites for this action
        var prerequisites = _prerequisiteFactory.CreatePrerequisites(action);
        
        // 2. Validate all prerequisites
        var validation = prerequisites.Validate(player, world);
        if (!validation.IsValid)
        {
            // Show all validation failures
            foreach (var failure in validation.Failures)
            {
                _messageSystem.AddSystemMessage(
                    $"❌ {failure.Reason}",
                    failure.CanBeRemedied ? SystemMessageTypes.Warning : SystemMessageTypes.Danger
                );
            }
            
            return ActionExecutionResult.Failed("Prerequisites not met");
        }
        
        // 3. Find appropriate executor
        if (!_executors.TryGetValue(action.Action, out var executor))
        {
            // Fallback to default executor
            executor = _executors.Values.FirstOrDefault(e => e.CanHandle(action.Action));
            if (executor == null)
            {
                return ActionExecutionResult.Failed($"No executor found for action type: {action.Action}");
            }
        }
        
        // 4. Execute the action
        try
        {
            var result = await executor.Execute(action, player, world);
            
            // 5. Apply resource costs if successful
            if (result.Success && !result.RequiresConversation)
            {
                ApplyResourceCosts(action, player, world);
            }
            
            return result;
        }
        catch (Exception ex)
        {
            _messageSystem.AddSystemMessage(
                $"❌ Action failed: {ex.Message}",
                SystemMessageTypes.Danger
            );
            return ActionExecutionResult.Failed(ex.Message);
        }
    }
    
    public bool CanHandle(LocationAction actionType)
    {
        return true; // This is the unified executor, it handles all actions
    }
    
    private void ApplyResourceCosts(ActionOption action, Player player, GameWorld world)
    {
        // Apply time cost
        if (action.HourCost > 0)
        {
            world.TimeManager.SpendHours(action.HourCost);
        }
        
        // Apply stamina cost
        if (action.StaminaCost > 0)
        {
            player.ModifyStamina(-action.StaminaCost);
        }
        
        // Apply coin cost
        if (action.CoinCost > 0)
        {
            player.ModifyCoins(-action.CoinCost);
        }
    }
    
    private LocationAction GetActionType(IActionExecutor executor)
    {
        // Get the action type this executor handles
        // This would be implemented based on executor metadata or interface
        return LocationAction.Converse; // Placeholder
    }
}

/// <summary>
/// Factory for creating action prerequisites
/// </summary>
public class ActionPrerequisiteFactory
{
    private readonly IGameRuleEngine _ruleEngine;
    private readonly NarrativeManager _narrativeManager;
    private readonly NPCRepository _npcRepository;
    private readonly ItemRepository _itemRepository;
    private readonly ConnectionTokenManager _tokenManager;
    private readonly FlagService _flagService;
    
    public ActionPrerequisiteFactory(
        IGameRuleEngine ruleEngine, 
        NarrativeManager narrativeManager,
        NPCRepository npcRepository,
        ItemRepository itemRepository,
        ConnectionTokenManager tokenManager,
        FlagService flagService)
    {
        _ruleEngine = ruleEngine;
        _narrativeManager = narrativeManager;
        _npcRepository = npcRepository;
        _itemRepository = itemRepository;
        _tokenManager = tokenManager;
        _flagService = flagService;
    }
    
    public ActionPrerequisites CreatePrerequisites(ActionOption action)
    {
        var prerequisites = new ActionPrerequisites();
        
        // Add time requirement
        if (action.HourCost > 0)
        {
            prerequisites.Requirements.Add(new TimeRequirement(action.HourCost));
        }
        
        // Add stamina requirement  
        if (action.StaminaCost > 0)
        {
            prerequisites.Requirements.Add(new StaminaRequirement(action.StaminaCost));
        }
        
        // Add coin requirement
        if (action.CoinCost > 0)
        {
            prerequisites.Requirements.Add(new CoinRequirement(action.CoinCost));
        }
        
        // Add action-specific requirements
        switch (action.Action)
        {
            case LocationAction.Socialize:
                // Requires existing relationship
                if (!string.IsNullOrEmpty(action.NPCId))
                {
                    prerequisites.Requirements.Add(new RelationshipRequirement(action.NPCId, 1, null, _npcRepository, _tokenManager));
                }
                break;
                
            case LocationAction.Collect:
                // Requires inventory space for letter
                if (!string.IsNullOrEmpty(action.LetterId))
                {
                    prerequisites.Requirements.Add(new InventorySpaceRequirement(1, "letter", _itemRepository));
                }
                break;
                
            case LocationAction.Deliver:
                // Custom requirement for delivery
                prerequisites.Requirements.Add(new LetterDeliveryRequirement(action.NPCId, _npcRepository));
                break;
        }
        
        // Add narrative requirements if active
        if (_narrativeManager.HasActiveNarrative())
        {
            prerequisites.Requirements.Add(new NarrativeRequirement(
                _narrativeManager,
                _flagService,
                action.Action,
                action.NPCId
            ));
        }
        
        return prerequisites;
    }
}