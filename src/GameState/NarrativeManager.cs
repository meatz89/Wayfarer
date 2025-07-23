using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;

/// <summary>
/// Manages narrative flows including tutorials, quests, and story sequences
/// Uses existing game systems without special mechanics
/// </summary>
public class NarrativeManager
{
    private GameWorld _gameWorld;
    private FlagService _flagService;
    private MessageSystem _messageSystem;
    private TimeManager _timeManager;
    private LocationActionManager _locationActionManager;
    private StandingObligationManager _obligationManager;
    private LetterTemplateRepository _letterTemplateRepository;
    private ConnectionTokenManager _connectionTokenManager;
    private NarrativeJournal _journal;
    private NarrativeEffectRegistry _effectRegistry;
    private NarrativeLoader _narrativeLoader;
    
    // Active narratives (can have multiple quests/stories active)
    private Dictionary<string, NarrativeDefinition> _activeNarratives = new Dictionary<string, NarrativeDefinition>();
    
    // Narrative definitions loaded from JSON
    private Dictionary<string, NarrativeDefinition> _narrativeDefinitions = new Dictionary<string, NarrativeDefinition>();
    
    public NarrativeManager(
        GameWorld gameWorld,
        FlagService flagService,
        MessageSystem messageSystem,
        TimeManager timeManager,
        LocationActionManager locationActionManager,
        StandingObligationManager obligationManager,
        LetterTemplateRepository letterTemplateRepository,
        ConnectionTokenManager connectionTokenManager,
        IServiceProvider serviceProvider)
    {
        _gameWorld = gameWorld;
        _flagService = flagService;
        _messageSystem = messageSystem;
        _timeManager = timeManager;
        _locationActionManager = locationActionManager;
        _obligationManager = obligationManager;
        _letterTemplateRepository = letterTemplateRepository;
        _connectionTokenManager = connectionTokenManager;
        
        // Initialize new components
        _journal = new NarrativeJournal();
        _effectRegistry = new NarrativeEffectRegistry(serviceProvider);
        _narrativeLoader = new NarrativeLoader(Path.Combine(Directory.GetCurrentDirectory(), "Content"));
    }
    
    /// <summary>
    /// Parameterless constructor for simple initialization
    /// Dependencies can be set later via properties
    /// </summary>
    public NarrativeManager()
    {
        _activeNarratives = new Dictionary<string, NarrativeDefinition>();
        _narrativeDefinitions = new Dictionary<string, NarrativeDefinition>();
        _journal = new NarrativeJournal();
    }
    
    /// <summary>
    /// Initialize dependencies after construction
    /// </summary>
    public void Initialize(GameWorld gameWorld, FlagService flagService)
    {
        _gameWorld = gameWorld;
        _flagService = flagService;
        // Other dependencies can be added as needed
    }
    
    public void LoadNarrativeDefinitions(List<NarrativeDefinition> definitions)
    {
        _narrativeDefinitions.Clear();
        foreach (var def in definitions)
        {
            _narrativeDefinitions[def.Id] = def;
        }
    }
    
    /// <summary>
    /// Load narrative definitions from JSON files
    /// </summary>
    public async Task LoadNarrativesFromJsonAsync()
    {
        if (_narrativeLoader == null)
        {
            _narrativeLoader = new NarrativeLoader(Path.Combine(Directory.GetCurrentDirectory(), "Content"));
        }
        
        var narratives = await _narrativeLoader.LoadNarrativesAsync();
        LoadNarrativeDefinitions(narratives);
    }
    
    public bool HasActiveNarrative()
    {
        return _activeNarratives.Any();
    }
    
    public bool IsNarrativeActive(string narrativeId)
    {
        return _activeNarratives.ContainsKey(narrativeId);
    }
    
    /// <summary>
    /// Get list of currently active narrative IDs
    /// </summary>
    public List<string> GetActiveNarrativeIds()
    {
        return _activeNarratives.Keys.ToList();
    }
    
    public void StartNarrative(string narrativeId)
    {
        if (!_narrativeDefinitions.ContainsKey(narrativeId))
        {
            throw new ArgumentException($"Narrative definition not found: {narrativeId}");
        }
        
        var definition = _narrativeDefinitions[narrativeId];
        _activeNarratives[narrativeId] = definition;
        
        // Record in journal
        _journal.RecordNarrativeStarted(narrativeId);
        
        // Apply starting conditions if specified
        if (definition.StartingConditions != null)
        {
            ApplyStartingConditions(definition.StartingConditions);
        }
        
        // Set narrative flags
        if (_flagService != null)
        {
            _flagService.SetFlag($"narrative_{narrativeId}_started", true);
            _flagService.SetFlag($"narrative_{narrativeId}_completed", false);
        }
        
        // Show introduction message if specified
        if (!string.IsNullOrEmpty(definition.IntroductionMessage) && _messageSystem != null)
        {
            _messageSystem.AddSystemMessage(
                definition.IntroductionMessage,
                SystemMessageTypes.Info
            );
        }
    }
    
    public void CompleteNarrative(string narrativeId)
    {
        if (!_activeNarratives.ContainsKey(narrativeId))
            return;
            
        var definition = _activeNarratives[narrativeId];
        
        // Record in journal
        _journal.RecordNarrativeCompleted(narrativeId);
        
        // Apply rewards if specified
        if (definition.Rewards != null)
        {
            ApplyRewards(definition.Rewards);
        }
        
        // Set completion flags
        _flagService.SetFlag($"narrative_{narrativeId}_completed", true);
        
        // Show completion message
        if (!string.IsNullOrEmpty(definition.CompletionMessage) && _messageSystem != null)
        {
            _messageSystem.AddSystemMessage(
                definition.CompletionMessage,
                SystemMessageTypes.Success
            );
        }
        
        // Remove from active narratives
        _activeNarratives.Remove(narrativeId);
    }
    
    public NarrativeStep GetCurrentStep(string narrativeId)
    {
        if (!_activeNarratives.ContainsKey(narrativeId))
            return null;
            
        var definition = _activeNarratives[narrativeId];
        
        // Find first incomplete step
        foreach (var step in definition.Steps)
        {
            if (!string.IsNullOrEmpty(step.CompletionFlag) && 
                _flagService != null &&
                !_flagService.GetFlag(step.CompletionFlag))
            {
                return step;
            }
        }
        
        // All steps complete
        return null;
    }
    
    public List<LocationAction> FilterActions(List<LocationAction> availableActions)
    {
        // If no active narratives, return all actions
        if (!HasActiveNarrative())
            return availableActions;
        
        // Collect allowed actions from all active narratives
        var allowedActions = new HashSet<LocationAction>();
        
        // Always allow rest
        allowedActions.Add(LocationAction.Rest);
        
        foreach (var narrative in _activeNarratives.Values)
        {
            var currentStep = GetCurrentStep(narrative.Id);
            if (currentStep != null)
            {
                // Add required action for current step
                if (currentStep.RequiredAction.HasValue)
                {
                    allowedActions.Add(currentStep.RequiredAction.Value);
                }
                
                // Add any explicitly allowed actions
                if (currentStep.AllowedActions != null)
                {
                    foreach (var action in currentStep.AllowedActions)
                    {
                        allowedActions.Add(action);
                    }
                }
            }
        }
        
        // Filter available actions to only allowed ones
        return availableActions.Where(a => allowedActions.Contains(a)).ToList();
    }
    
    public async Task OnActionCompleted(LocationAction action, string targetId = null)
    {
        // Check all active narratives for step completion
        var narrativesToCheck = _activeNarratives.Keys.ToList();
        
        foreach (var narrativeId in narrativesToCheck)
        {
            var currentStep = GetCurrentStep(narrativeId);
            if (currentStep == null)
            {
                // All steps complete, finish narrative
                CompleteNarrative(narrativeId);
                continue;
            }
            
            // Check if this action completes the current step
            bool stepCompleted = false;
            
            if (currentStep.RequiredAction.HasValue && action == currentStep.RequiredAction.Value)
            {
                // Additional validation based on step requirements
                if (!string.IsNullOrEmpty(currentStep.RequiredLocation))
                {
                    stepCompleted = targetId == currentStep.RequiredLocation;
                }
                else if (!string.IsNullOrEmpty(currentStep.RequiredNPC))
                {
                    stepCompleted = targetId == currentStep.RequiredNPC;
                }
                else
                {
                    stepCompleted = true;
                }
            }
            
            if (stepCompleted)
            {
                // Record in journal
                _journal.RecordStepCompleted(narrativeId, currentStep.Id);
                
                // Mark step as complete
                if (!string.IsNullOrEmpty(currentStep.CompletionFlag))
                {
                    _flagService.SetFlag(currentStep.CompletionFlag, true);
                }
                
                // Create obligation if specified
                if (currentStep.ObligationToCreate != null && _obligationManager != null)
                {
                    var obligationDef = currentStep.ObligationToCreate;
                    var obligation = new StandingObligation
                    {
                        ID = obligationDef.Id,
                        Name = obligationDef.Name,
                        Description = obligationDef.Description,
                        Source = obligationDef.SourceNpcId,
                        RelatedTokenType = obligationDef.RelatedTokenType,
                        BenefitEffects = obligationDef.BenefitEffects,
                        ConstraintEffects = obligationDef.ConstraintEffects
                    };
                    
                    _obligationManager.AddObligation(obligation);
                    
                    // Record relationship in journal
                    _journal.RecordRelationshipFormed(narrativeId, obligationDef.SourceNpcId, "obligation");
                }
                
                // Apply any narrative effects
                if (currentStep.Effects != null && _effectRegistry != null)
                {
                    var results = await _effectRegistry.ApplyEffects(_gameWorld, currentStep.Effects);
                    // Log any failed effects
                    foreach (var result in results.Where(r => !r.Success))
                    {
                        Console.WriteLine($"Failed to apply effect: {result.Message}");
                    }
                }
                
                // Increment step counter
                _flagService.IncrementCounter($"narrative_{narrativeId}_steps_completed");
                
                // Show next step guidance
                var nextStep = GetCurrentStep(narrativeId);
                if (nextStep != null && !string.IsNullOrEmpty(nextStep.GuidanceText) && _messageSystem != null)
                {
                    _messageSystem.AddSystemMessage(nextStep.GuidanceText, SystemMessageTypes.Info);
                }
            }
        }
    }
    
    public string GetLocationGuidance(string locationId)
    {
        foreach (var narrative in _activeNarratives.Values)
        {
            var currentStep = GetCurrentStep(narrative.Id);
            if (currentStep != null && currentStep.RequiredLocation == locationId)
            {
                return $"{narrative.Name}: {currentStep.Description}";
            }
        }
        
        return null;
    }
    
    public bool ShouldShowNPC(string npcId)
    {
        // If no active narratives, show all NPCs
        if (!HasActiveNarrative())
            return true;
        
        // Check if any narrative restricts this NPC
        foreach (var narrative in _activeNarratives.Values)
        {
            if (narrative.RestrictedNPCs != null && narrative.RestrictedNPCs.Contains(npcId))
            {
                // Check if NPC should be shown yet
                var showFlag = $"narrative_{narrative.Id}_show_{npcId}";
                if (!_flagService.GetFlag(showFlag))
                    return false;
            }
        }
        
        return true;
    }
    
    public string GetNarrativeIntroduction(ConversationContext context)
    {
        // Check if this conversation is part of a narrative step
        foreach (var narrative in _activeNarratives.Values)
        {
            var currentStep = GetCurrentStep(narrative.Id);
            if (currentStep != null && 
                currentStep.RequiredAction == LocationAction.Converse &&
                currentStep.RequiredNPC == context.TargetNPC?.ID)
            {
                // Return narrative-specific introduction if specified
                if (!string.IsNullOrEmpty(currentStep.ConversationIntroduction))
                {
                    return currentStep.ConversationIntroduction;
                }
            }
        }
        
        return null;
    }
    
    private void ApplyStartingConditions(NarrativeStartingConditions conditions)
    {
        var player = _gameWorld.GetPlayer();
        
        if (conditions.PlayerCoins.HasValue)
            player.Coins = conditions.PlayerCoins.Value;
            
        if (conditions.PlayerStamina.HasValue)
            player.Stamina = conditions.PlayerStamina.Value;
            
        // Setting location should be handled by GameWorldManager
        // For now, just log that location should be set
        if (!string.IsNullOrEmpty(conditions.StartingLocation))
        {
            // TODO: Implement location change through proper channels
            // GameWorldManager should handle location changes
        }
        
        // Clear specified game state if requested
        if (conditions.ClearInventory)
            player.Inventory.Clear();
            
        if (conditions.ClearLetterQueue && player.LetterQueue != null)
        {
            // Clear all letter queue positions
            for (int i = 0; i < player.LetterQueue.Length; i++)
            {
                player.LetterQueue[i] = null;
            }
        }
            
        if (conditions.ClearObligations)
            player.StandingObligations.Clear();
    }
    
    private void ApplyRewards(NarrativeRewards rewards)
    {
        var player = _gameWorld.GetPlayer();
        
        if (rewards.Coins.HasValue)
            player.Coins += rewards.Coins.Value;
            
        if (rewards.Stamina.HasValue)
            player.Stamina = Math.Min(player.Stamina + rewards.Stamina.Value, 10); // Max stamina is 10
            
        if (rewards.Items != null)
        {
            foreach (var itemId in rewards.Items)
            {
                // Add items through proper system when available
                // For now, just track with flag
                _flagService.SetFlag($"reward_item_{itemId}_received", true);
            }
        }
    }
    
    public NarrativeDefinition GetNarrativeDefinition(string narrativeId)
    {
        if (_narrativeDefinitions.ContainsKey(narrativeId))
        {
            return _narrativeDefinitions[narrativeId];
        }
        return null;
    }
    
    /// <summary>
    /// Get the narrative journal for querying history
    /// </summary>
    public NarrativeJournal GetJournal()
    {
        return _journal;
    }
    
    /// <summary>
    /// Record a player choice in the journal
    /// </summary>
    public void RecordChoice(string narrativeId, string stepId, string choiceId, Dictionary<string, object> context = null)
    {
        _journal.RecordChoice(narrativeId, stepId, choiceId, context);
    }
}

// Data models for narrative configuration
public class NarrativeDefinition
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string IntroductionMessage { get; set; }
    public string CompletionMessage { get; set; }
    public NarrativeStartingConditions StartingConditions { get; set; }
    public List<NarrativeStep> Steps { get; set; } = new List<NarrativeStep>();
    public NarrativeRewards Rewards { get; set; }
    public List<string> RestrictedNPCs { get; set; } // NPCs that don't appear until unlocked
}

public class NarrativeStep
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public LocationAction? RequiredAction { get; set; }
    public string RequiredLocation { get; set; }
    public string RequiredNPC { get; set; }
    public List<LocationAction> AllowedActions { get; set; }
    public string GuidanceText { get; set; }
    public string CompletionFlag { get; set; }
    public string ConversationIntroduction { get; set; } // Override for conversation intro
    public NarrativeStepObligation ObligationToCreate { get; set; } // Creates obligation when step completes
    public List<NarrativeEffectDefinition> Effects { get; set; } // Effects to apply when step completes
}

public class NarrativeStartingConditions
{
    public int? PlayerCoins { get; set; }
    public int? PlayerStamina { get; set; }
    public string StartingLocation { get; set; }
    public string StartingSpot { get; set; }
    public bool ClearInventory { get; set; }
    public bool ClearLetterQueue { get; set; }
    public bool ClearObligations { get; set; }
}

public class NarrativeRewards
{
    public int? Coins { get; set; }
    public int? Stamina { get; set; }
    public List<string> Items { get; set; }
    public string Message { get; set; }
}

public class NarrativeStepObligation
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string SourceNpcId { get; set; }
    public ConnectionType? RelatedTokenType { get; set; }
    public List<ObligationEffect> BenefitEffects { get; set; } = new List<ObligationEffect>();
    public List<ObligationEffect> ConstraintEffects { get; set; } = new List<ObligationEffect>();
}