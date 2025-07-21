# Action-Conversation Architecture

## Overview

This document defines how LocationActionManager and ConversationManager work together to create a narrative-rich action system where every player action triggers contextual storytelling through the conversation system.

## Core Architecture Principles

### 1. Actions Generate Conversations
- **LocationActionManager** identifies available actions based on context (NPCs present, location type, time, equipment)
- **ConversationManager** provides narrative wrapper and choices for each action
- Every action, no matter how simple, gets at least one line of narrative context

### 2. Action Sources
Actions emerge from two primary sources:
- **NPC Actions**: Work, socialize, trade, deliver letters - all require specific NPCs present
- **Environmental Actions**: Gather resources, read notices, clear obstacles - based on location properties

### 3. Narrative Complexity Levels

#### Simple Actions (1 beat, no choices)
- Gather berries: "You carefully pick ripe berries, filling your pouch. (+2 food)"
- Read notice: "The weathered notice mentions bandit activity on the eastern road."
- Basic rest: "You find a quiet spot and rest for an hour. (+3 stamina)"

#### Medium Actions (2-3 beats, 1-2 choices)
- NPC Work:
  - Beat 1: "Marcus needs help in his shop. What would you prefer?"
  - Choice A: "Heavy lifting (2 stamina, 4 coins)"
  - Choice B: "Inventory work (1 stamina, 3 coins)"
  - Beat 2: Outcome based on choice
  
#### Complex Actions (3+ beats, branching paths)
- Travel Encounters:
  - Beat 1: "A merchant caravan blocks the narrow mountain pass."
  - Choice A: "Wait for them to pass (+1 hour)"
  - Choice B: "Offer to help clear rocks (1 stamina, possible reward)"
  - Choice C: "Take dangerous cliff path (requires climbing gear)"
  - Multiple follow-up beats based on choices

## Action Categories

### 1. NPC Professional Actions
Actions that emerge from an NPC's profession:
```
Merchant → "Help with inventory", "Load wagons", "Trade goods"
Scribe → "Copy documents", "Read letters for illiterate"
TavernKeeper → "Serve drinks", "Clean tables", "Buy drinks while resting"
Noble → "Deliver private message", "Attend as courier"
```

### 2. NPC Social Actions
Relationship-building with any NPC:
```
No relationship (0 tokens) → "Introduce yourself"
Has relationship (1+ tokens) → "Spend time together", "Share a meal"
Strong relationship (5+ tokens) → "Ask for favor", "Learn secrets"
```

### 3. Environmental Actions
Context-specific to location properties:
```
Forest spots → "Gather berries", "Collect herbs"
Crossroads → "Check notice board", "Leave message for other carriers"
Market → "Browse stalls" (if no merchant NPCs present)
Sacred sites → "Rest at shrine", "Leave offering"
```

### 4. Travel Actions
Encounters during route traversal:
```
Meeting other travelers → Trade, share news, get warnings
Natural obstacles → Fallen trees, flooded paths, rockslides
Bandits → Pay toll, fight, negotiate, take detour
Weather events → Seek shelter, push through, wait it out
```

### 5. Emergency Actions
Desperation options with lasting consequences:
```
Patron requests → Write for funds/equipment (damages relationship)
Borrow money → From merchants (creates debt)
Illegal work → From shadow NPCs (they gain leverage)
```

## Integration Flow

### 1. Action Generation
```csharp
LocationActionManager.GetAvailableActions()
├── Check NPCs present
│   ├── Generate profession-specific actions
│   ├── Generate social actions based on relationship
│   └── Generate letter-related actions
├── Check location properties
│   ├── Generate environmental actions
│   └── Generate context-specific options
└── Check player state
    └── Generate emergency actions if desperate
```

### 2. Action Selection to Resolution
```
1. Player sees available actions in LocationActions UI
2. Player selects action
3. LocationActionManager validates resources
4. ConversationFactory creates context with:
   - Action type and parameters
   - NPC (if applicable)
   - Environmental context
5. ConversationManager launches with action context
6. Narrative plays out with appropriate beats/choices
7. Mechanical effects apply after conversation
8. Return to location view
```

### 3. Conversation Context for Actions
```csharp
public class ActionConversationContext : ConversationContext
{
    public ActionOption SourceAction { get; set; }
    public ActionComplexity Complexity { get; set; } // Simple, Medium, Complex
    public List<ActionOutcome> PossibleOutcomes { get; set; }
    public EnvironmentalFactors Environment { get; set; }
}
```

## Design Principles

### 1. Every Action Has Weight
Even gathering berries gets narrative context. This makes the world feel alive and gives weight to time spent.

### 2. NPCs Drive Most Actions
The majority of actions come from NPCs present at the location. This reinforces that relationships and people matter.

### 3. Environmental Actions Are Intentional
Not random events but deliberate choices: "I choose to spend an hour gathering berries" not "You stumble upon berries."

### 4. Travel Is Active, Not Passive
Travel between locations includes encounters and choices, making it engaging gameplay rather than mere transition.

### 5. Consequences Are Visible
Players see the full cost/benefit before choosing. No hidden mechanics or surprise outcomes.

### 6. Equipment Enables Options
Having the right gear opens new action possibilities (climbing gear for shortcuts, waterproof satchel for river crossings).

## Current Implementation Details

### LocationActionManager Architecture

#### Dependencies
- **GameWorld** - Core game state
- **MessageSystem** - For immediate feedback messages
- **ConnectionTokenManager** - Token tracking and management
- **LetterQueueManager** - Letter delivery mechanics
- **NPCRepository** - NPC queries
- **ItemRepository** - Item lookups

#### Action Flow
1. **GetAvailableActions()** - Called by LocationActions UI component
   - Gets current player location and spot
   - Queries NPCs at current spot and time
   - Filters by NPC availability
   - Generates actions per NPC (basic, profession, letter, emergency)
   - Adds patron actions if at desk/room/study

2. **ExecuteAction()** - Validates resources, spends them, executes specific action
   - Resource validation (hours, stamina, coins)
   - Resource spending (deducted before execution)
   - Action-specific execution methods
   - System messages for feedback

#### Current Action Types (LocationAction enum)
- **Basic Actions**: Converse, Work, Socialize, Rest, Collect, Deliver, Trade
- **Debt Actions**: RequestPatronFunds, RequestPatronEquipment, BorrowMoney, PleedForAccess, AcceptIllegalWork

#### NPC Structure
- **Identity**: ID, Name, Role, Description, Location, SpotId
- **Categories**: Profession, AvailabilitySchedule, ProvidedServices
- **Letter System**: LetterTokenTypes (which tokens they give)
- **Availability**: Schedule-based with IsAvailable(TimeBlocks) method

### ConversationManager Architecture

#### Core Components
1. **ConversationManager** - Orchestrates AI-driven conversations
   - Manages conversation flow and state
   - Integrates with AIGameMaster for narrative generation
   - Tracks choices and player selections
   - Controls conversation beats and duration

2. **ConversationContext** - Conversation setup data
   - GameWorld reference
   - Target NPC and player
   - Location context (name, spot, properties)
   - Relationship tracking (tokens with NPC)
   - Conversation topic and history

3. **ConversationState** - Runtime conversation tracking
   - Current NPC and player references
   - Duration counter (beats) and max duration
   - Focus points for complex conversations
   - Current and last narrative text
   - Conversation outcome tracking

4. **ConversationFactory** - Creates conversation instances
   - Injects dependencies (AIGameMaster, WorldStateInputBuilder, ConnectionTokenManager)
   - Queries current relationship tokens with NPC
   - Creates context and state objects
   - Returns configured ConversationManager

#### Dependencies
- **AIGameMaster** - Generates narrative content
- **WorldStateInputBuilder** - Creates world state for AI context
- **ConnectionTokenManager** - Queries relationship tokens
- **GameWorld** - Core game state reference

#### Conversation Flow
1. **InitializeConversation()** - Sets up initial narrative
   - Resets duration counter
   - Generates AI introduction narrative
   - Sets initial conversation text

2. **ProcessNextBeat()** - Generates player choices
   - Checks AI availability
   - Requests choices from AIGameMaster
   - Populates Choices list for UI

3. **ProcessPlayerChoice()** - Handles player selection
   - Generates AI reaction narrative
   - Updates conversation state
   - Advances duration counter
   - Checks for completion conditions
   - Returns ConversationBeatOutcome

#### ConversationChoice Structure
- **ChoiceID** - Unique identifier
- **NarrativeText** - Display text for player
- **FocusCost** - Focus points required
- **IsAffordable** - Can player select this
- **SkillOption** - Associated skill check
- **Success/FailureNarrative** - Outcome narratives

#### UI Integration
- **ConversationView.razor** - Displays conversation UI
- **ConversationViewBase** - Component logic
- Currently has TODO comments for integration
- Receives ConversationManager as parameter
- Handles choice selection and tooltip display

## Integration Strategy

### Current Conversation System Architecture

#### Key Components
1. **MainGameplayView** - Controls which screen is displayed via CurrentViews enum
2. **ConversationManager** - Stored as property in MainGameplayView
3. **ConversationFactory** - Creates conversation instances with dependencies
4. **ConversationView** - UI component that displays conversations

#### View Switching Pattern
```csharp
// In MainGameplayView.razor.cs
public ConversationManager ConversationManager = null;

// In MainGameplayView.razor
case CurrentViews.ConversationScreen:
    <ConversationView ConversationManager="ConversationManager"
                      OnConversationCompleted="OnConversationCompleted"
                      @key="StateVersion" />
    break;
```

### Connecting LocationActionManager with ConversationManager

#### 1. Deterministic Conversation System (Phase 1)
For initial implementation, create a simplified deterministic conversation flow:

```csharp
// Extend ActionOption to include conversation data
public class ActionOption
{
    // Existing properties...
    
    // New conversation properties
    public bool RequiresConversation { get; set; }
    public List<ActionChoice> PossibleChoices { get; set; }
    public string InitialNarrative { get; set; }
}

public class ActionChoice
{
    public string ChoiceText { get; set; }
    public string Outcome { get; set; }
    public int HourModifier { get; set; }  // Extra hours cost/saved
    public int StaminaModifier { get; set; }
    public int CoinModifier { get; set; }
    public Dictionary<string, int> ItemsGained { get; set; }
}
```

#### 2. Action Execution Flow Integration
```csharp
// In LocationActionManager.ExecuteAction()
if (option.RequiresConversation)
{
    // Store the action for post-conversation processing
    _gameWorld.PendingAction = option;
    
    // Create deterministic conversation
    var context = new ConversationContext
    {
        GameWorld = _gameWorld,
        Player = _gameWorld.GetPlayer(),
        LocationName = _gameWorld.GetPlayer().CurrentLocation.Name,
        LocationSpotName = _gameWorld.GetPlayer().CurrentLocationSpot?.Name,
        TargetNPC = option.NPCId != null ? _npcRepository.GetNPCById(option.NPCId) : null,
        ConversationTopic = $"Action_{option.Action}"
    };
    
    // Create simplified conversation state
    var state = new ConversationState(
        _gameWorld.GetPlayer(),
        context.TargetNPC,
        0,  // No focus cost for simple actions
        1); // Single beat conversation
    
    // For Phase 1: Create a simplified ConversationManager without AI
    var conversationManager = new DeterministicConversationManager(context, state, option);
    
    // Signal MainGameplayView to switch to conversation
    _gameWorld.LaunchConversation(conversationManager);
    return true;
}

// Otherwise execute directly as before
```

#### 3. DeterministicConversationManager Implementation
```csharp
public class DeterministicConversationManager : ConversationManager
{
    private ActionOption _actionOption;
    
    public DeterministicConversationManager(
        ConversationContext context, 
        ConversationState state,
        ActionOption actionOption) 
        : base(context, state, null, null, context.GameWorld)
    {
        _actionOption = actionOption;
        _isAvailable = true;
    }
    
    public override async Task InitializeConversation()
    {
        _state.CurrentNarrative = _actionOption.InitialNarrative;
        _state.IsConversationComplete = false;
    }
    
    public override async Task<bool> ProcessNextBeat()
    {
        // Generate deterministic choices
        Choices.Clear();
        
        for (int i = 0; i < _actionOption.PossibleChoices.Count; i++)
        {
            var actionChoice = _actionOption.PossibleChoices[i];
            Choices.Add(new ConversationChoice
            {
                ChoiceID = (i + 1).ToString(),
                NarrativeText = actionChoice.ChoiceText,
                FocusCost = 0,
                IsAffordable = true
            });
        }
        
        return true;
    }
    
    public override async Task<ConversationBeatOutcome> ProcessPlayerChoice(ConversationChoice selectedChoice)
    {
        var choiceIndex = int.Parse(selectedChoice.ChoiceID) - 1;
        var actionChoice = _actionOption.PossibleChoices[choiceIndex];
        
        // Apply choice modifiers to the original action
        _actionOption.HourCost += actionChoice.HourModifier;
        _actionOption.StaminaCost += actionChoice.StaminaModifier;
        _actionOption.CoinCost -= actionChoice.CoinModifier; // Negative because cost reduction
        
        var outcome = new ConversationBeatOutcome
        {
            NarrativeDescription = actionChoice.Outcome,
            IsConversationComplete = true
        };
        
        _state.IsConversationComplete = true;
        
        return outcome;
    }
}
```

#### 4. MainGameplayView Integration
```csharp
// In GameWorld
public void LaunchConversation(ConversationManager manager)
{
    ConversationPending = true;
    PendingConversationManager = manager;
}

// In MainGameplayView.PollGameState()
if (GameWorld.ConversationPending)
{
    ConversationManager = GameWorld.PendingConversationManager;
    CurrentScreen = CurrentViews.ConversationScreen;
    GameWorld.ConversationPending = false;
}

// Handle conversation completion
private void OnConversationCompleted(ConversationResult result)
{
    if (GameWorld.PendingAction != null)
    {
        // Execute the action with modified parameters
        LocationActionManager.CompleteActionAfterConversation(GameWorld.PendingAction);
        GameWorld.PendingAction = null;
    }
    
    // Return to location view
    CurrentScreen = CurrentViews.LocationScreen;
}
```

### Environmental Action Implementation

#### Domain Tag System
Location spots use domain tags to indicate what types of activities are available:
```json
// From location_spots.json
"domainTags": [ "COMMERCE", "TRADE", "SOCIAL", "REST", "RESOURCES", "CRAFTING", "LABOR" ]
```

#### Environmental Action Generation Based on Domain Tags
```csharp
private void AddEnvironmentalActions(List<ActionOption> actions)
{
    var spot = _gameWorld.CurrentLocationSpot;
    if (spot == null) return;
    
    // Forest/Nature locations (RESOURCES tag)
    if (spot.DomainTags.Contains("RESOURCES"))
    {
        actions.Add(new ActionOption
        {
            ActionType = LocationAction.GatherResources,
            Name = "Gather wild berries",
            Description = "Search the area for edible berries",
            HourCost = 1,
            Effect = "+2 food items",
            RequiresConversation = true,
            ConversationComplexity = ActionComplexity.Simple
        });
        
        actions.Add(new ActionOption
        {
            ActionType = LocationAction.GatherResources,
            Name = "Collect herbs",
            Description = "Look for medicinal herbs in the undergrowth",
            HourCost = 1,
            StaminaCost = 1,
            Effect = "+1-3 herbs (based on location)",
            RequiresConversation = true,
            ConversationComplexity = ActionComplexity.Simple
        });
    }
    
    // Market/Commerce locations (when no merchant NPCs present)
    if (spot.DomainTags.Contains("COMMERCE") && !HasMerchantNPCs())
    {
        actions.Add(new ActionOption
        {
            ActionType = LocationAction.Browse,
            Name = "Browse market stalls",
            Description = "Look through unattended stalls for opportunities",
            HourCost = 1,
            Effect = "Discover prices and goods",
            RequiresConversation = true,
            ConversationComplexity = ActionComplexity.Simple
        });
    }
    
    // Social locations (SOCIAL tag)
    if (spot.DomainTags.Contains("SOCIAL"))
    {
        actions.Add(new ActionOption
        {
            ActionType = LocationAction.Observe,
            Name = "Listen to local gossip",
            Description = "Sit quietly and overhear conversations",
            HourCost = 1,
            Effect = "Learn about recent events",
            RequiresConversation = true,
            ConversationComplexity = ActionComplexity.Simple
        });
    }
    
    // Rest locations (REST tag)
    if (spot.DomainTags.Contains("REST"))
    {
        actions.Add(new ActionOption
        {
            ActionType = LocationAction.Rest,
            Name = "Take a proper rest",
            Description = "Find a comfortable spot to recover",
            HourCost = 1,
            Effect = "+3 stamina",
            RequiresConversation = false // Direct execution
        });
    }
}
```

#### Spot Type Considerations
```csharp
// Location spot types from JSON
public enum LocationSpotType
{
    FEATURE,    // Market, Workshop, Tavern, etc.
    DISTRICT,   // Noble Quarter, Merchant District
    LANDMARK,   // Significant locations
    CROSSING    // Travel points
}

// Different spot types enable different environmental actions
private bool ShouldAddEnvironmentalActions(LocationSpot spot)
{
    // Features always have environmental actions
    if (spot.Type == LocationSpotType.FEATURE) return true;
    
    // Districts only when appropriate
    if (spot.Type == LocationSpotType.DISTRICT && 
        spot.DomainTags.Any(tag => tag == "COMMERCE" || tag == "SOCIAL"))
        return true;
        
    return false;
}
```

## Implementation Notes

### Phase 1: Narrative Wrapper
- Keep current direct execution
- Add single-beat narrative messages via MessageSystem
- Test the flow and UI integration
- Focus on simple environmental actions first

### Phase 2: Simple Conversations
- Convert simple actions to use ConversationManager
- Single beat, no choices
- Ensure mechanical effects still apply
- Start with Gather, Read, Rest actions

### Phase 3: Complex Conversations
- Add choice-based conversations for NPC actions
- Implement branching narratives
- Different outcomes based on choices
- Work actions with choice of heavy/light labor

### Phase 4: Travel System
- Implement travel as series of encounters
- Add equipment-based options
- Create risk/reward decisions
- Route-specific encounters in ConversationManager

## Example Action Flows

### Simple Environmental Action
```
Location: Forest Clearing
Available Action: "Gather wild berries"
Player selects action
Conversation:
  "You spend an hour carefully picking ripe berries from the bushes. 
   The sweet juice stains your fingers purple as you work."
  [+2 food items, -1 hour]
Return to location view
```

### Medium NPC Work Action
```
Location: Market (Marcus the Merchant present)
Available Action: "Help Marcus with inventory"
Player selects action
Conversation Beat 1:
  "Marcus looks up from his ledger. 'Perfect timing! I need help with...'"
  Choice A: "The morning deliveries" (Heavy work: 2 stamina, 4 coins)
  Choice B: "Organizing the stockroom" (Light work: 1 stamina, 3 coins)
Player selects A
Conversation Beat 2:
  "You spend the hour hauling heavy crates from the delivery wagon.
   Marcus nods approvingly and counts out your wages."
  [+4 coins, -2 stamina, -1 hour, +1 Trade token with Marcus]
Return to location view
```

### Complex Travel Encounter
```
Travel Route: Mountain Pass (with climbing gear)
Random Encounter Triggers
Conversation Beat 1:
  "A recent rockslide has blocked the main path. You could clear it,
   but you spot a narrow ledge that might offer a quicker route."
  Choice A: "Clear the rocks" (1 hour, 1 stamina)
  Choice B: "Try the ledge" (Requires climbing gear, risky)
  Choice C: "Turn back and find another route" (+2 hours)
Player selects B
Conversation Beat 2:
  "You secure your climbing gear and edge along the narrow ledge.
   Halfway across, you notice an old cache hidden in the rocks."
  Choice A: "Investigate the cache" (Risk falling)
  Choice B: "Keep moving" (Safe but no reward)
Player selects A
Conversation Beat 3:
  "Your fingers find purchase on the weathered leather pouch.
   Inside: 15 coins and a cryptic note about 'the shadow route.'"
  [+15 coins, new route information]
Continue travel to destination
```

## Travel Encounter System

### Travel as Conversation Sequences
Travel between locations becomes a series of ConversationManager encounters rather than simple time passage:

#### Travel Context Creation
```csharp
public class TravelConversationContext : ConversationContext
{
    public Route CurrentRoute { get; set; }
    public Equipment PlayerEquipment { get; set; }
    public Weather CurrentWeather { get; set; }
    public int EncounterRoll { get; set; }
}
```

#### Route-Specific Encounters
```csharp
// Each route defines possible encounters
public class RouteEncounters
{
    public string RouteId { get; set; }
    public List<EncounterTemplate> PossibleEncounters { get; set; }
    
    // Encounter examples based on route type
    // Mountain Pass: Rockslides, narrow ledges, altitude sickness
    // Forest Path: Berry bushes, fallen trees, lost travelers
    // River Road: Flooding, bridge tolls, fishing opportunities
    // Trade Route: Merchant caravans, bandits, patrol checkpoints
}
```

#### Equipment-Based Options
```csharp
private List<ConversationChoice> GenerateTravelChoices(TravelConversationContext context)
{
    var choices = new List<ConversationChoice>();
    
    // Base choice - always available
    choices.Add(new ConversationChoice
    {
        NarrativeText = "Continue on the main path",
        FocusCost = 0
    });
    
    // Equipment-specific choices
    if (context.PlayerEquipment.Has("ClimbingGear") && context.CurrentRoute.HasCliffs)
    {
        choices.Add(new ConversationChoice
        {
            NarrativeText = "Use climbing gear to take the cliff shortcut",
            FocusCost = 1,
            RequiresSkillCheck = true,
            SkillOption = new SkillOption { Skill = Skills.Agility }
        });
    }
    
    if (context.PlayerEquipment.Has("WaterproofSatchel") && context.CurrentRoute.HasRiverCrossing)
    {
        choices.Add(new ConversationChoice
        {
            NarrativeText = "Ford the river directly (letters stay dry)",
            FocusCost = 0
        });
    }
    
    return choices;
}
```

#### Travel Encounter Flow
1. Player initiates travel from TravelManager
2. TravelManager creates TravelConversationContext
3. ConversationFactory creates travel conversation
4. Series of encounters based on route distance:
   - Short routes (1-2 hours): 0-1 encounters
   - Medium routes (3-4 hours): 1-2 encounters  
   - Long routes (5+ hours): 2-3 encounters
5. Each encounter can modify:
   - Travel time
   - Player resources (stamina, coins, items)
   - Letter conditions
   - Discovery of new routes/information
6. Travel completes at destination

## Future Considerations

### Dynamic Narrative Generation
The AI GameMaster could generate unique narratives based on:
- Current relationships
- Recent events
- Player reputation
- Environmental factors

### Narrative Memory
Track important narrative moments:
- First meeting with each NPC
- Significant choices made
- Recurring themes in player behavior

### Emergent Storytelling
Let narrative choices affect future options:
- Help a merchant → they offer better work later
- Discover bandit activity → new travel options appear
- Build reputation → NPCs seek you out

## Key Architecture Insights

### Separation of Concerns
- **LocationActionManager**: Generates available actions, validates resources, applies mechanical effects
- **ConversationManager**: Provides narrative wrapper, handles player choices, creates story beats
- **Domain Tags**: Drive environmental action availability based on location properties
- **NPCs**: Primary source of actions at location spots
- **Travel**: Separate conversation sequences during route traversal

### Integration Points
1. **Action Selection**: LocationActions UI → LocationActionManager → ConversationFactory (if needed)
2. **Conversation Flow**: ConversationManager → AI narrative → Player choices → Mechanical outcomes
3. **Travel Flow**: TravelManager → ConversationFactory → Travel encounters → Arrival

### Design Principles Reinforced
- Every action has narrative context (even simple ones)
- NPCs drive most location actions
- Environmental actions are intentional player choices
- Travel is active gameplay, not passive transition
- Equipment enables new options without forcing them
- All consequences visible before choosing

## Conclusion

This architecture creates a living world where every action has narrative weight while maintaining clear mechanical outcomes. Players engage with a story, not just a spreadsheet of numbers, while still making strategic resource decisions.