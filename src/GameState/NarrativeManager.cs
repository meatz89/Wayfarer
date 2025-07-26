using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Manages narrative flows including tutorials, quests, and story sequences.
/// Works by filtering available actions based on narrative state, NOT by adding special mechanics.
/// </summary>
public class NarrativeManager
{
    private readonly FlagService _flagService;
    private readonly NPCRepository _npcRepository;
    private readonly LocationRepository _locationRepository;
    private readonly LocationSystem _locationSystem;
    private readonly ITimeManager _timeManager;
    private readonly Dictionary<string, NarrativeDefinition> _narrativeDefinitions = new Dictionary<string, NarrativeDefinition>();
    private readonly Dictionary<string, NarrativeState> _activeNarratives = new Dictionary<string, NarrativeState>();
    
    public NarrativeManager(
        FlagService flagService, 
        NPCRepository npcRepository,
        LocationRepository locationRepository,
        LocationSystem locationSystem,
        ITimeManager timeManager)
    {
        _flagService = flagService;
        _npcRepository = npcRepository;
        _locationRepository = locationRepository;
        _locationSystem = locationSystem;
        _timeManager = timeManager;
    }
    
    /// <summary>
    /// Load narrative definitions (called during initialization)
    /// </summary>
    public void LoadNarrativeDefinitions(List<NarrativeDefinition> definitions)
    {
        _narrativeDefinitions.Clear();
        foreach (var definition in definitions)
        {
            _narrativeDefinitions[definition.Id] = definition;
        }
    }
    
    /// <summary>
    /// Start a narrative (e.g., tutorial)
    /// </summary>
    public void StartNarrative(string narrativeId)
    {
        if (!_narrativeDefinitions.ContainsKey(narrativeId))
        {
            throw new InvalidOperationException($"Narrative '{narrativeId}' not found");
        }
        
        var definition = _narrativeDefinitions[narrativeId];
        
        // Check starting conditions
        if (!CheckStartingConditions(definition))
        {
            return;
        }
        
        // Create active narrative state
        var state = new NarrativeState
        {
            NarrativeId = narrativeId,
            CurrentStepIndex = 0,
            StartedAt = DateTime.Now,
            IsActive = true
        };
        
        _activeNarratives[narrativeId] = state;
        
        // Set narrative started flag
        _flagService.SetFlag($"narrative_{narrativeId}_started", true);
        
        // Apply starting effects
        ApplyStartingEffects(definition);
        
        // Apply forced state changes for the first step
        if (definition.Steps.Count > 0)
        {
            ApplyStepStartEffects(narrativeId, definition.Steps[0]);
        }
    }
    
    /// <summary>
    /// Get all active narrative IDs
    /// </summary>
    public List<string> GetActiveNarratives()
    {
        return _activeNarratives.Where(kvp => kvp.Value.IsActive)
            .Select(kvp => kvp.Key)
            .ToList();
    }
    
    /// <summary>
    /// Check if a narrative is currently active
    /// </summary>
    public bool IsNarrativeActive(string narrativeId)
    {
        return _activeNarratives.TryGetValue(narrativeId, out var state) && state.IsActive;
    }
    
    /// <summary>
    /// Check if any narrative is active
    /// </summary>
    public bool HasActiveNarrative()
    {
        return _activeNarratives.Any(kvp => kvp.Value.IsActive);
    }
    
    /// <summary>
    /// Get the current step of a narrative
    /// </summary>
    public NarrativeStep GetCurrentStep(string narrativeId)
    {
        if (!_activeNarratives.TryGetValue(narrativeId, out var state) || !state.IsActive)
        {
            return null;
        }
        
        var definition = _narrativeDefinitions[narrativeId];
        if (state.CurrentStepIndex >= definition.Steps.Count)
        {
            return null;
        }
        
        return definition.Steps[state.CurrentStepIndex];
    }
    
    /// <summary>
    /// Get the current step index (for progress tracking)
    /// </summary>
    public int GetCurrentStepIndex(string narrativeId)
    {
        if (!_activeNarratives.TryGetValue(narrativeId, out var state))
        {
            return -1;
        }
        return state.CurrentStepIndex;
    }
    
    /// <summary>
    /// Get total steps in a narrative
    /// </summary>
    public int GetTotalSteps(string narrativeId)
    {
        if (!_narrativeDefinitions.TryGetValue(narrativeId, out var definition))
        {
            return 0;
        }
        return definition.Steps.Count;
    }
    
    /// <summary>
    /// Called when a command is completed to check for narrative progression
    /// </summary>
    public void OnCommandCompleted(IGameCommand command, CommandResult result)
    {
        if (!result.IsSuccess) return;
        
        // Drop appropriate flags based on command type
        DropCommandFlags(command);
        
        // Drop narrative-specific flags based on current narrative context
        DropNarrativeSpecificFlags(command);
        
        // Check each active narrative for progression
        foreach (var kvp in _activeNarratives.ToList())
        {
            if (!kvp.Value.IsActive) continue;
            
            CheckNarrativeProgression(kvp.Key);
        }
    }
    
    /// <summary>
    /// Filter available commands based on active narratives
    /// </summary>
    public List<DiscoveredCommand> FilterCommands(List<DiscoveredCommand> commands)
    {
        // If no active narratives, return all commands
        if (!HasActiveNarrative())
        {
            return commands;
        }
        
        // Get allowed command types from all active narratives
        var allowedCommandTypes = new HashSet<string>();
        var allowedCommands = new List<DiscoveredCommand>();
        
        foreach (var narrativeId in GetActiveNarratives())
        {
            var currentStep = GetCurrentStep(narrativeId);
            if (currentStep != null && currentStep.AllowedActions.Any())
            {
                // This step has specific allowed actions
                foreach (var action in currentStep.AllowedActions)
                {
                    allowedCommandTypes.Add(action);
                }
            }
        }
        
        // If no specific restrictions, allow all commands
        if (!allowedCommandTypes.Any())
        {
            return commands;
        }
        
        // Filter commands based on allowed types
        foreach (var command in commands)
        {
            var commandType = GetCommandType(command.Command);
            if (allowedCommandTypes.Contains(commandType))
            {
                allowedCommands.Add(command);
            }
        }
        
        return allowedCommands;
    }
    
    /// <summary>
    /// Check if an NPC should be visible based on narrative state
    /// </summary>
    public bool IsNPCVisible(string npcId)
    {
        // If no active narratives, all NPCs are visible
        if (!HasActiveNarrative())
        {
            return true;
        }
        
        // Check each active narrative for NPC visibility rules
        foreach (var narrativeId in GetActiveNarratives())
        {
            var currentStep = GetCurrentStep(narrativeId);
            if (currentStep != null)
            {
                // If this step has visibility rules and NPC is not in the list, hide them
                if (currentStep.VisibleNPCs.Any() && !currentStep.VisibleNPCs.Contains(npcId))
                {
                    return false;
                }
            }
        }
        
        return true;
    }
    
    /// <summary>
    /// Get narrative-specific dialogue for an NPC
    /// </summary>
    public string GetNarrativeDialogue(string npcId)
    {
        // Check each active narrative for dialogue overrides
        foreach (var narrativeId in GetActiveNarratives())
        {
            var currentStep = GetCurrentStep(narrativeId);
            if (currentStep != null && currentStep.DialogueOverrides.ContainsKey(npcId))
            {
                return currentStep.DialogueOverrides[npcId];
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Complete a narrative
    /// </summary>
    public void CompleteNarrative(string narrativeId)
    {
        if (!_activeNarratives.TryGetValue(narrativeId, out var state))
        {
            return;
        }
        
        state.IsActive = false;
        state.CompletedAt = DateTime.Now;
        
        // Set completion flag
        _flagService.SetFlag($"narrative_{narrativeId}_completed", true);
        
        // Apply completion rewards
        if (_narrativeDefinitions.TryGetValue(narrativeId, out var definition))
        {
            ApplyCompletionRewards(definition);
        }
    }
    
    /// <summary>
    /// Get a narrative definition by ID
    /// </summary>
    public NarrativeDefinition GetNarrativeDefinition(string narrativeId)
    {
        return _narrativeDefinitions.TryGetValue(narrativeId, out var definition) ? definition : null;
    }
    
    /// <summary>
    /// Get state for serialization
    /// </summary>
    public Dictionary<string, NarrativeState> GetActiveNarrativeStates()
    {
        return new Dictionary<string, NarrativeState>(_activeNarratives);
    }
    
    /// <summary>
    /// Restore state from serialization
    /// </summary>
    public void RestoreNarrativeStates(Dictionary<string, NarrativeState> states)
    {
        _activeNarratives.Clear();
        if (states != null)
        {
            foreach (var kvp in states)
            {
                _activeNarratives[kvp.Key] = kvp.Value;
            }
        }
    }
    
    // Private helper methods
    
    private bool CheckStartingConditions(NarrativeDefinition definition)
    {
        foreach (var condition in definition.StartingConditions)
        {
            if (!CheckCondition(condition))
            {
                return false;
            }
        }
        return true;
    }
    
    private bool CheckCondition(NarrativeCondition condition)
    {
        switch (condition.Type)
        {
            case ConditionType.FlagSet:
                return _flagService.HasFlag(condition.Value);
            case ConditionType.FlagNotSet:
                return !_flagService.HasFlag(condition.Value);
            case ConditionType.CounterGreaterThan:
                return _flagService.GetCounter(condition.Key) > int.Parse(condition.Value);
            case ConditionType.CounterLessThan:
                return _flagService.GetCounter(condition.Key) < int.Parse(condition.Value);
            case ConditionType.CounterEquals:
                return _flagService.GetCounter(condition.Key) == int.Parse(condition.Value);
            default:
                return true;
        }
    }
    
    private void ApplyStartingEffects(NarrativeDefinition definition)
    {
        // Apply any flags or counters needed at start
        foreach (var effect in definition.StartingEffects)
        {
            ApplyEffect(effect);
        }
    }
    
    private void ApplyCompletionRewards(NarrativeDefinition definition)
    {
        // Apply any completion rewards
        foreach (var reward in definition.CompletionRewards)
        {
            ApplyEffect(reward);
        }
    }
    
    private void ApplyEffect(NarrativeEffect effect)
    {
        switch (effect.Type)
        {
            case EffectType.SetFlag:
                _flagService.SetFlag(effect.Value, true);
                break;
            case EffectType.ClearFlag:
                _flagService.SetFlag(effect.Value, false);
                break;
            case EffectType.SetCounter:
                _flagService.SetCounter(effect.Key, int.Parse(effect.Value));
                break;
            case EffectType.IncrementCounter:
                _flagService.IncrementCounter(effect.Key, int.Parse(effect.Value));
                break;
        }
    }
    
    private void DropCommandFlags(IGameCommand command)
    {
        // Drop generic flags based on command type - no narrative-specific logic here
        // The narrative system should observe these generic flags and interpret them
        switch (command)
        {
            case TravelCommand:
                _flagService.SetFlag("player_moved", true);
                _flagService.IncrementCounter("movements_made");
                break;
            case ConverseCommand:
                _flagService.SetFlag("npc_conversed", true);
                _flagService.IncrementCounter("npcs_talked_to");
                break;
            case WorkCommand:
                _flagService.SetFlag("work_performed", true);
                _flagService.IncrementCounter("work_actions_taken");
                break;
            case CollectLetterCommand:
                _flagService.SetFlag("letter_collected", true);
                _flagService.IncrementCounter("letters_collected_count");
                break;
            case DeliverLetterCommand:
                _flagService.SetFlag("letter_delivered", true);
                _flagService.IncrementCounter("letters_delivered_count");
                break;
            case LetterQueueActionCommand:
                _flagService.SetFlag("queue_action_taken", true);
                break;
            case BrowseCommand:
                _flagService.SetFlag("market_browsed", true);
                break;
            case BorrowMoneyCommand:
                _flagService.SetFlag("money_borrowed", true);
                _flagService.IncrementCounter("total_debt");
                break;
            case AdvanceTimeCommand:
            case RestCommand:
                _flagService.IncrementCounter("time_blocks_passed");
                break;
        }
    }
    
    private void CheckNarrativeProgression(string narrativeId)
    {
        var state = _activeNarratives[narrativeId];
        var definition = _narrativeDefinitions[narrativeId];
        var currentStep = definition.Steps[state.CurrentStepIndex];
        
        // Check if current step requirements are met
        bool stepComplete = true;
        foreach (var requirement in currentStep.CompletionRequirements)
        {
            if (!CheckCondition(requirement))
            {
                stepComplete = false;
                break;
            }
        }
        
        if (stepComplete)
        {
            // Apply step completion effects
            foreach (var effect in currentStep.CompletionEffects)
            {
                ApplyEffect(effect);
            }
            
            // Move to next step
            state.CurrentStepIndex++;
            
            // Check if narrative is complete
            if (state.CurrentStepIndex >= definition.Steps.Count)
            {
                CompleteNarrative(narrativeId);
            }
            else
            {
                // Apply forced state changes for the new step
                ApplyStepStartEffects(narrativeId, definition.Steps[state.CurrentStepIndex]);
            }
        }
    }
    
    private string GetCommandType(IGameCommand command)
    {
        // Map command types to action names used in narrative definitions
        return command switch
        {
            TravelCommand => "Travel",
            ConverseCommand => "Converse",
            WorkCommand => "Work",
            RestCommand => "Rest",
            CollectLetterCommand => "CollectLetter",
            DeliverLetterCommand => "DeliverLetter",
            LetterQueueActionCommand => "QueueAction",
            SocializeCommand => "Socialize",
            BorrowMoneyCommand => "BorrowMoney",
            GatherResourcesCommand => "Gather",
            BrowseCommand => "Browse",
            ObserveCommand => "Observe",
            PatronFundsCommand => "PatronFunds",
            _ => command.GetType().Name.Replace("Command", "")
        };
    }
    
    /// <summary>
    /// Drop narrative-specific flags based on current narrative context
    /// </summary>
    private void DropNarrativeSpecificFlags(IGameCommand command)
    {
        // Check if tutorial is active
        if (_activeNarratives.TryGetValue("wayfarer_tutorial", out var tutorialState) && tutorialState.IsActive)
        {
            var currentStep = GetCurrentStep("wayfarer_tutorial");
            if (currentStep == null) return;
            
            // Map generic events to tutorial-specific flags based on current step
            switch (currentStep.Id)
            {
                case "day1_wake":
                    if (command is RestCommand)
                    {
                        _flagService.SetFlag(FlagService.TUTORIAL_FIRST_REST, true);
                        _flagService.SetFlag(FlagService.TUTORIAL_FIRST_MOVEMENT, true); // For backward compatibility
                    }
                    break;
                    
                case "day1_square":
                    if (command is ConverseCommand)
                        _flagService.SetFlag(FlagService.TUTORIAL_FIRST_NPC_TALK, true);
                    break;
                    
                case "day1_first_work":
                    if (command is WorkCommand)
                    {
                        _flagService.SetFlag(FlagService.TUTORIAL_FIRST_WORK, true);
                        _flagService.SetFlag(FlagService.TUTORIAL_FIRST_TOKEN_EARNED, true);
                    }
                    break;
                    
                case "day1_buy_food":
                    if (command is BrowseCommand)
                        _flagService.SetFlag(FlagService.TUTORIAL_FOOD_PURCHASED, true);
                    break;
                    
                // Add more step-specific flag mappings as needed
            }
        }
    }
    
    /// <summary>
    /// Apply forced state changes when a narrative step starts
    /// </summary>
    private void ApplyStepStartEffects(string narrativeId, NarrativeStep step)
    {
        // Apply forced location change
        if (!string.IsNullOrEmpty(step.ForcedLocation) && !string.IsNullOrEmpty(step.ForcedSpot))
        {
            var location = _locationSystem.GetLocation(step.ForcedLocation);
            var spot = _locationSystem.GetLocationSpot(step.ForcedLocation, step.ForcedSpot);
            
            if (location != null && spot != null)
            {
                _locationRepository.SetCurrentLocation(location, spot);
            }
        }
        
        // Apply forced time change
        // TODO: Add SetTimeOfDay method to ITimeManager interface when needed
        // if (step.ForcedHour.HasValue)
        // {
        //     _timeManager.SetTimeOfDay(step.ForcedHour.Value);
        // }
        
        // Give item to player if specified
        if (!string.IsNullOrEmpty(step.ItemToGiveOnStart))
        {
            // Note: This requires GameWorld access, which NarrativeManager doesn't have directly
            // The item giving should be handled through the command system or a dedicated service
            _flagService.SetFlag($"narrative_give_item_{step.ItemToGiveOnStart}", true);
        }
    }
}

/// <summary>
/// Represents a single step in a narrative
/// </summary>
public class NarrativeStep
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string GuidanceText { get; set; }
    
    // Which actions are allowed during this step (empty = all allowed)
    public List<string> AllowedActions { get; set; } = new List<string>();
    
    // Which NPCs should be visible during this step (empty = all visible)
    public List<string> VisibleNPCs { get; set; } = new List<string>();
    
    // Which locations should be visible during this step (empty = all visible)
    public List<string> VisibleLocations { get; set; } = new List<string>();
    
    // Dialogue overrides for NPCs during this step
    public Dictionary<string, string> DialogueOverrides { get; set; } = new Dictionary<string, string>();
    
    // Requirements to complete this step
    public List<NarrativeCondition> CompletionRequirements { get; set; } = new List<NarrativeCondition>();
    
    // Effects when step is completed
    public List<NarrativeEffect> CompletionEffects { get; set; } = new List<NarrativeEffect>();
    
    // Forced state changes when step starts
    public string ForcedLocation { get; set; }
    public string ForcedSpot { get; set; }
    public int? ForcedHour { get; set; }
    
    // Item to give the player when step starts
    public string ItemToGiveOnStart { get; set; }
}

/// <summary>
/// Represents a narrative definition
/// </summary>
public class NarrativeDefinition
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<NarrativeStep> Steps { get; set; } = new List<NarrativeStep>();
    public List<NarrativeCondition> StartingConditions { get; set; } = new List<NarrativeCondition>();
    public List<NarrativeEffect> StartingEffects { get; set; } = new List<NarrativeEffect>();
    public List<NarrativeEffect> CompletionRewards { get; set; } = new List<NarrativeEffect>();
}

/// <summary>
/// Represents the current state of an active narrative
/// </summary>
public class NarrativeState
{
    public string NarrativeId { get; set; }
    public int CurrentStepIndex { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>
/// Represents a condition that must be met
/// </summary>
public class NarrativeCondition
{
    public ConditionType Type { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}

/// <summary>
/// Types of conditions
/// </summary>
public enum ConditionType
{
    FlagSet,
    FlagNotSet,
    CounterGreaterThan,
    CounterLessThan,
    CounterEquals
}

/// <summary>
/// Represents an effect to apply
/// </summary>
public class NarrativeEffect
{
    public EffectType Type { get; set; }
    public string Key { get; set; }
    public string Value { get; set; }
}

/// <summary>
/// Types of effects
/// </summary>
public enum EffectType
{
    SetFlag,
    ClearFlag,
    SetCounter,
    IncrementCounter
}

/// <summary>
/// Collection of narrative definitions
/// </summary>
public static class NarrativeDefinitions
{
    private static List<NarrativeDefinition> _definitions = new List<NarrativeDefinition>();
    
    public static List<NarrativeDefinition> All => _definitions;
    
    public static void Add(NarrativeDefinition definition)
    {
        _definitions.Add(definition);
    }
    
    public static void Clear()
    {
        _definitions.Clear();
    }
}