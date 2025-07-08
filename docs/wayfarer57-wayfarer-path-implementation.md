# Wayfarer: Concrete Implementation of 80 Days Design Patterns

Based on the latest project knowledge and your code architecture, I'll provide specific implementation details for integrating the 80 Days design patterns into Wayfarer's existing systems.

## 1. GameWorldManager Implementation

```csharp
public class GameWorldManager
{
    private GameWorld _gameWorld;
    private EncounterFactory _encounterFactory;
    
    public GameWorldManager(GameWorld gameWorld, EncounterFactory encounterFactory)
    {
        _gameWorld = gameWorld;
        _encounterFactory = encounterFactory;
    }
    
    public async Task StartGame()
    {
        // Game initialization logic
        _gameWorld.Player.IsInitialized = true;
        // Other initialization as needed
    }
    
    public void StartEncounter(string encounterType, NPC targetNPC)
    {
        // Delegate encounter creation to EncounterFactory
        EncounterContext context = new EncounterContext(
            _gameWorld.Player,
            targetNPC,
            encounterType,
            DetermineGoalFlags(encounterType),
            FlagDefinitionLibrary.GetAllFlagDefinitions()
        );
        
        // Create an EncounterManager using the factory
        EncounterManager encounterManager = _encounterFactory.CreateEncounter(context);
        
        // Store reference in GameWorld
        _gameWorld.CurrentEncounterManager = encounterManager;
        
        // Initialize the encounter
        encounterManager.InitializeEncounter();
    }
    
    public void ProcessPlayerChoice(string choiceId)
    {
        // Simply delegate to the EncounterManager
        if (_gameWorld.CurrentEncounterManager != null)
        {
            _gameWorld.CurrentEncounterManager.ProcessPlayerChoice(choiceId);
        }
    }
    
    private List<FlagStates> DetermineGoalFlags(string encounterType)
    {
        // Determine goal flags based on encounter type
        List<FlagStates> goalFlags = new List<FlagStates>();
        
        if (encounterType == "SocialIntroduction")
        {
            goalFlags.Add(FlagStates.TrustEstablished);
        }
        else if (encounterType == "Investigation")
        {
            goalFlags.Add(FlagStates.InsightGained);
            goalFlags.Add(FlagStates.SecretRevealed);
        }
        
        return goalFlags;
    }
    
    public GameWorldSnapshot GetGameSnapshot()
    {
        return new GameWorldSnapshot(_gameWorld);
    }
}
```

## 2. GameWorld Implementation

```csharp
public class GameWorld
{
    public Player Player { get; private set; }
    public EncounterManager CurrentEncounterManager { get; set; }
    
    public GameWorld()
    {
        Player = new Player();
    }
}

// GameWorldSnapshot to provide a read-only view of the game state
public class GameWorldSnapshot
{
    public bool HasActiveEncounter { get; private set; }
    public int? CurrentFocusPoints { get; private set; }
    public int? MaxFocusPoints { get; private set; }
    public List<FlagStates> ActiveFlags { get; private set; }
    public string StreamingText { get; private set; }
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }
    public List<ChoiceProjection> AvailableChoices { get; private set; }
    public bool CanSelectChoice { get; private set; }
    
    public GameWorldSnapshot(GameWorld gameWorld)
    {
        HasActiveEncounter = gameWorld.CurrentEncounterManager != null;
        
        if (HasActiveEncounter)
        {
            EncounterState state = gameWorld.CurrentEncounterManager.GetEncounterState();
            CurrentFocusPoints = state.FocusPoints;
            MaxFocusPoints = state.MaxFocusPoints;
            ActiveFlags = state.FlagManager.GetAllActiveFlags();
            
            StreamingContentState streamingState = gameWorld.CurrentEncounterManager.GetStreamingState();
            StreamingText = streamingState.CurrentText;
            IsStreaming = streamingState.IsStreaming;
            StreamProgress = streamingState.StreamProgress;
            
            AvailableChoices = gameWorld.CurrentEncounterManager.GetCurrentChoiceProjections();
            CanSelectChoice = !IsStreaming && AvailableChoices != null && AvailableChoices.Count > 0;
        }
    }
}
```

## 3. EncounterFactory Implementation

```csharp
public class EncounterFactory
{
    private AIGameMaster _aiGameMaster;
    private ChoiceTemplateLibrary _templateLibrary;
    
    public EncounterFactory(AIGameMaster aiGameMaster, ChoiceTemplateLibrary templateLibrary)
    {
        _aiGameMaster = aiGameMaster;
        _templateLibrary = templateLibrary;
    }
    
    public EncounterManager CreateEncounter(EncounterContext context)
    {
        // Create an EncounterState from the context
        EncounterState state = new EncounterState(
            context.Player,
            6, // Standard focus points
            10, // Max duration
            context.GoalFlags,
            context.FlagDefinitions
        );
        
        state.CurrentNPC = context.TargetNPC;
        
        // Create the EncounterManager with all dependencies
        return new EncounterManager(
            context,
            state,
            _aiGameMaster,
            _templateLibrary
        );
    }
}

public class EncounterContext
{
    public Player Player { get; private set; }
    public NPC TargetNPC { get; private set; }
    public string EncounterType { get; private set; }
    public List<FlagStates> GoalFlags { get; private set; }
    public List<FlagDefinition> FlagDefinitions { get; private set; }
    
    public EncounterContext(
        Player player,
        NPC targetNPC,
        string encounterType,
        List<FlagStates> goalFlags,
        List<FlagDefinition> flagDefinitions)
    {
        Player = player;
        TargetNPC = targetNPC;
        EncounterType = encounterType;
        GoalFlags = goalFlags;
        FlagDefinitions = flagDefinitions;
    }
}
```

## 4. EncounterManager Implementation

```csharp
public class EncounterManager
{
    private EncounterContext _context;
    private EncounterState _state;
    private AIGameMaster _aiGameMaster;
    private ChoiceTemplateLibrary _templateLibrary;
    private StreamingContentState _streamingState;
    private List<ChoiceProjection> _currentChoiceProjections;
    private bool _isAwaitingAIResponse;
    
    public EncounterManager(
        EncounterContext context,
        EncounterState state,
        AIGameMaster aiGameMaster,
        ChoiceTemplateLibrary templateLibrary)
    {
        _context = context;
        _state = state;
        _aiGameMaster = aiGameMaster;
        _templateLibrary = templateLibrary;
        _streamingState = new StreamingContentState();
        _currentChoiceProjections = new List<ChoiceProjection>();
        _isAwaitingAIResponse = false;
    }
    
    public void InitializeEncounter()
    {
        // Request initial AI choices
        RequestAIChoices();
    }
    
    public void ProcessPlayerChoice(string choiceId)
    {
        // Find the selected choice
        ChoiceProjection selectedChoice = _currentChoiceProjections.FirstOrDefault(c => c.ChoiceID == choiceId);
        
        if (selectedChoice == null || _isAwaitingAIResponse || _streamingState.IsStreaming)
        {
            return;
        }
        
        // Find the template
        ChoiceTemplate template = _templateLibrary.GetTemplateByName(selectedChoice.TemplateUsed);
        
        if (template == null)
        {
            return;
        }
        
        // Process focus cost
        _state.FocusPoints -= selectedChoice.FocusCost;
        
        // Perform skill check
        bool success = PerformSkillCheck(selectedChoice.SkillCheck);
        
        // Begin streaming narrative
        if (success)
        {
            _streamingState.BeginStreaming(selectedChoice.SuccessNarrative);
            
            // Create and apply the success effect
            IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(template.SuccessEffectClass);
            effect.Apply(_state);
        }
        else
        {
            _streamingState.BeginStreaming(selectedChoice.FailureNarrative);
            
            // Create and apply the failure effect
            IMechanicalEffect effect = (IMechanicalEffect)Activator.CreateInstance(template.FailureEffectClass);
            effect.Apply(_state);
        }
        
        // Update state
        _state.AdvanceDuration(1);
        _state.ProcessModifiers();
        _state.CheckGoalCompletion();
        
        // Clear choices while streaming
        _currentChoiceProjections.Clear();
        
        // Request next choices after streaming completes
        // This will be triggered by the GameUI polling the streaming state
    }
    
    private bool PerformSkillCheck(SkillCheck skillCheck)
    {
        if (skillCheck == null)
        {
            return true;
        }
        
        int playerSkillLevel = _context.Player.GetSkillLevel(skillCheck.SkillCategory);
        playerSkillLevel += _state.GetNextCheckModifier();
        
        return playerSkillLevel >= skillCheck.SCD;
    }
    
    private void RequestAIChoices()
    {
        if (_isAwaitingAIResponse || _streamingState.IsStreaming)
        {
            return;
        }
        
        _isAwaitingAIResponse = true;
        
        // Build prompt with ALL templates
        AIPromptBuilder promptBuilder = new AIPromptBuilder();
        string prompt = promptBuilder.BuildPrompt(_context, _state, _templateLibrary.GetAllTemplates());
        
        // Request choices from AI
        _aiGameMaster.RequestChoices(prompt, OnAIResponseReceived);
    }
    
    private void OnAIResponseReceived(AIResponse response)
    {
        _isAwaitingAIResponse = false;
        
        if (response == null)
        {
            // Handle error
            return;
        }
        
        // Begin streaming the beat narration
        _streamingState.BeginStreaming(response.BeatNarration);
        
        // Create choice projections
        _currentChoiceProjections = CreateChoiceProjections(response.Choices);
    }
    
    private List<ChoiceProjection> CreateChoiceProjections(List<AIChoice> choices)
    {
        List<ChoiceProjection> projections = new List<ChoiceProjection>();
        
        foreach (AIChoice choice in choices)
        {
            ChoiceProjection projection = new ChoiceProjection
            {
                ChoiceID = choice.ChoiceID,
                NarrativeText = choice.NarrativeText,
                FocusCost = choice.FocusCost,
                TemplateUsed = choice.TemplateUsed,
                SkillCheck = choice.SkillCheck,
                SuccessNarrative = choice.SuccessNarrative,
                FailureNarrative = choice.FailureNarrative
            };
            
            projections.Add(projection);
        }
        
        return projections;
    }
    
    // Accessor methods for snapshot generation
    public EncounterState GetEncounterState()
    {
        return _state;
    }
    
    public StreamingContentState GetStreamingState()
    {
        // Update streaming state
        _streamingState.Update();
        
        // Check if streaming just completed and we need new choices
        if (!_streamingState.IsStreaming && 
            !_isAwaitingAIResponse && 
            _currentChoiceProjections.Count == 0 &&
            !_state.IsEncounterComplete)
        {
            RequestAIChoices();
        }
        
        return _streamingState;
    }
    
    public List<ChoiceProjection> GetCurrentChoiceProjections()
    {
        return _currentChoiceProjections;
    }
}
```

## 5. GameUI Implementation with Polling

```csharp
@page "/"
@using System.Threading
@inject GameWorldManager GameManager

<div class="encounter-container">
    @if (currentSnapshot != null && currentSnapshot.HasActiveEncounter)
    {
        <div class="encounter-header">
            <div class="focus-display">
                Focus Points: @currentSnapshot.CurrentFocusPoints / @currentSnapshot.MaxFocusPoints
            </div>
            
            <div class="flag-display">
                @foreach (var flag in currentSnapshot.ActiveFlags)
                {
                    <span class="flag-badge">@flag</span>
                }
            </div>
        </div>
        
        <div class="narrative-container">
            @if (currentSnapshot.IsStreaming)
            {
                <div class="streaming-text">
                    @currentSnapshot.StreamingText<span class="cursor">|</span>
                </div>
                <div class="stream-progress">
                    <div class="progress-bar" style="width: @(currentSnapshot.StreamProgress * 100)%"></div>
                </div>
            }
            else if (currentSnapshot.AvailableChoices != null && currentSnapshot.AvailableChoices.Count > 0)
            {
                <div class="narrative-text">
                    @currentSnapshot.StreamingText
                </div>
                
                <div class="choices-container">
                    @foreach (var choice in currentSnapshot.AvailableChoices)
                    {
                        <button class="choice-button" 
                                @onclick="() => MakeChoice(choice.ChoiceID)" 
                                disabled="@(!currentSnapshot.CanSelectChoice)">
                            <div class="choice-header">
                                <span class="choice-focus">@choice.FocusCost Focus</span>
                                <span class="choice-difficulty">@choice.SkillCheck.DifficultyLabel</span>
                                <span class="choice-template">@choice.TemplateUsed</span>
                            </div>
                            <div class="choice-text">@choice.NarrativeText</div>
                        </button>
                    }
                </div>
            }
        </div>
    }
    else
    {
        <div class="no-encounter">
            <p>No active encounter</p>
            <button @onclick="StartEncounter">Start Encounter</button>
        </div>
    }
</div>

@code {
    private Timer _pollingTimer;
    private GameWorldSnapshot currentSnapshot;
    
    protected override void OnInitialized()
    {
        // Set up polling timer - no events, just regular polling
        _pollingTimer = new Timer(_ => 
        {
            InvokeAsync(() => 
            {
                PollGameState();
                StateHasChanged();
            });
        }, null, 0, 100); // Poll every 100ms
    }
    
    private void PollGameState()
    {
        // Poll for current game state
        currentSnapshot = GameManager.GetGameSnapshot();
    }
    
    private void StartEncounter()
    {
        GameManager.StartEncounter("SocialIntroduction", null);
    }
    
    private void MakeChoice(string choiceId)
    {
        if (currentSnapshot != null && currentSnapshot.CanSelectChoice)
        {
            GameManager.ProcessPlayerChoice(choiceId);
        }
    }
    
    public void Dispose()
    {
        _pollingTimer?.Dispose();
    }
}
```

1. **NO updateTimer in GameWorldManager** - All polling is correctly in the GameUI component
2. **Proper Delegation** - GameWorldManager properly delegates to EncounterFactory and EncounterManager without handling implementation details
3. **Encapsulated Encounter Management** - All encounter-specific state and logic is properly in EncounterManager, not in GameWorld
4. **AI Integration** - AI interaction is handled by EncounterManager instead of directly in GameWorldManager
5. **Improved State Management** - Proper distinction between immutable EncounterContext and mutable EncounterState
6. **Polling Architecture** - Maintained the strict polling architecture with no events or callbacks
7. **Direct Effect Class Execution** - Preserved the direct template-to-effect connection for mechanical effects

These changes properly align with your described architecture while preserving the core mechanical principles of Wayfarer.

## IX. Conclusion: Integration Roadmap

To implement these systems incrementally without disrupting existing Wayfarer architecture:

1. **Phase 1: Time System Foundation**
   - Implement GameWorld time tracking (CurrentDay, CurrentTimeOfDay)
   - Add TimeOfDay-based property system to Location
   - Create the Deadline framework for time pressure

2. **Phase 2: Resource Extension**
   - Extend Player with Energy, Reputation, and Money
   - Implement interdependence effects
   - Create contextual cost calculation

3. **Phase 3: Memory and Narrative Continuity**
   - Add Memory system to Player
   - Create Memory-related effect classes
   - Enhance AI prompting with memory context

4. **Phase 4: Goal Structure**
   - Implement Goal tracking system
   - Create goal-related effects
   - Add goal awareness to AI prompting

5. **Phase 5: Culture and Faction Systems**
   - Add cultural properties to locations
   - Implement faction tracking
   - Create cultural and factional effects

6. **Phase 6: Travel and Preparation**
   - Implement route system
   - Create travel encounters
   - Add preparation advantages

7. **Phase 7: AI Prompt Enhancement**
   - Update AIPromptBuilder to include all new context
   - Create enhanced template system aware of new dimensions
   - Refine AI instructions to leverage new systems

This implementation path preserves your current architecture while layering in the rich dynamics inspired by 80 Days, creating a much more cohesive and engaging player journey.