# Narrative System Implementation Plan

## Overview

This document outlines the implementation plan for a reusable narrative system that can handle tutorials, quests, and story sequences without special mechanics. The system uses existing game mechanics with a narrative overlay that guides players through structured experiences.

## Core Design Principles

### 1. No Special Mechanics
- All narrative sequences use existing game systems
- No tutorial-specific logic in core mechanics
- Narratives guide and restrict, but don't alter rules

### 2. Categorical Design
- NarrativeRequirement system filters available actions
- FlagService tracks progress without coupling
- Clean separation between narrative flow and game mechanics

### 3. Reusability
- Same system handles tutorials, quests, and story moments
- Generic naming (NarrativeManager, not TutorialManager)
- Flexible configuration through data files

## Builder Pattern Architecture

### Overview
The narrative system uses a builder pattern to define complex narrative sequences programmatically. This provides a fluent API that makes narrative definitions readable and maintainable while keeping the configuration flexible.

### Key Benefits
1. **Readable Code**: Fluent API makes narrative flow clear
2. **Type Safety**: Compile-time checking of narrative structure
3. **Reusability**: Common patterns can be extracted into helper methods
4. **Testability**: Narratives can be unit tested
5. **Separation**: Logic separated from data

### Builder Components

#### NarrativeBuilder
The main builder that constructs a complete narrative:
```csharp
var tutorial = new NarrativeBuilder("wayfarer_tutorial", "From Destitute to Patronage")
    .WithDescription("Learn the ways of a Wayfarer")
    .WithIntroduction("You wake on cold stone...")
    .WithStartingConditions(conditions => conditions
        .WithCoins(3)
        .WithStamina(5)
        .AtLocation("millbrook", "lower_ward_warehouse"))
    .AddStep("first_movement", "Leave the Warehouse", step => step
        .RequiresAction(LocationAction.Move)
        .AtLocation("lower_ward_square")
        .WithGuidance("Click on locations to move."))
    .Build();
```

#### StepBuilder
Defines individual steps within the narrative:
```csharp
.AddStep("meet_tam", "Meet Tam", step => step
    .WithDescription("Talk to Tam the Beggar")
    .RequiresAction(LocationAction.Converse)
    .WithNPC("tam_beggar")
    .AllowActions(LocationAction.Move, LocationAction.Rest)
    .WithGuidance("NPCs give information.")
    .WithConversationIntro("A ragged figure notices you...")
    .CompletesWhenFlagSet("tutorial_first_conversation"))
```

#### Usage Flow
```csharp
// 1. Build the narrative
var tutorialNarrative = NarrativeDefinitions.CreateTutorial();

// 2. Load into NarrativeManager
narrativeManager.LoadNarrativeDefinitions(new List<NarrativeDefinition> { tutorialNarrative });

// 3. Start the narrative
narrativeManager.StartNarrative("wayfarer_tutorial");

// 4. System automatically manages flow
// - Filters available actions
// - Shows guidance
// - Tracks completion
// - Applies rewards
```

## Architecture Components

### 1. FlagService (Event Tracking)
**Purpose**: Track game events and narrative progress without coupling systems

**Key Features**:
- Boolean flags for state tracking
- Counters for progression metrics
- Timestamps for event timing
- No direct system dependencies

**Implementation**:
```csharp
public class FlagService
{
    // Generic event tracking
    SetFlag(string key, bool value)
    GetFlag(string key)
    IncrementCounter(string key)
    
    // Narrative-specific helpers
    IsNarrativeActive(string narrativeId)
    GetNarrativeStep(string narrativeId)
}
```

### 2. NarrativeManager (Flow Orchestration)
**Purpose**: Orchestrate narrative sequences using existing game systems

**Key Features**:
- Manages active narrative flows
- Filters available actions based on current step
- Provides contextual guidance
- Tracks completion and progression

**Implementation**:
```csharp
public class NarrativeManager
{
    // Core narrative flow
    StartNarrative(string narrativeId)
    GetCurrentStep(string narrativeId)
    GetAllowedActions(List<LocationAction> available)
    OnActionCompleted(LocationAction action, string targetId)
    
    // Guidance and hints
    GetLocationGuidance(string locationId)
    GetActionTooltip(LocationAction action)
    ShouldShowNPC(string npcId)
}
```

### 3. NarrativeRequirement (Action Filtering)
**Purpose**: Define requirements for actions during narrative sequences

**Key Features**:
- Implements IRequirement interface
- Checks if actions are allowed in current narrative step
- Provides reason text for blocked actions
- Integrates with existing requirement system

**Implementation**:
```csharp
public class NarrativeRequirement : IRequirement
{
    public string NarrativeId { get; set; }
    public string RequiredStep { get; set; }
    public List<LocationAction> AllowedActions { get; set; }
    
    public bool IsSatisfiedBy(Player player) 
    {
        // Check if narrative allows this action
    }
}
```

### 4. NarrativeStep (Configuration)
**Purpose**: Define individual steps within a narrative sequence

**Data Structure**:
```csharp
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
}
```

### 5. NarrativeDefinition (Data Model)
**Purpose**: Define complete narrative sequences

**Data Structure**:
```json
{
  "id": "wayfarer_tutorial",
  "name": "From Destitute to Patronage",
  "startingConditions": {
    "playerCoins": 3,
    "playerStamina": 5,
    "startingLocation": "millbrook",
    "startingSpot": "lower_ward_warehouse"
  },
  "steps": [
    {
      "id": "first_movement",
      "name": "Leave the Warehouse",
      "requiredAction": "Move",
      "requiredLocation": "lower_ward_square",
      "guidanceText": "Click on locations to move. Each movement takes time.",
      "completionFlag": "tutorial_first_movement"
    }
  ],
  "rewards": {
    "coins": 10,
    "message": "Tutorial Complete!"
  }
}
```

## Implementation Strategy

### Phase 1: Core Infrastructure (Immediate)
1. **FlagService** ✅ - Already created
2. **NarrativeManager** - Core orchestration logic
3. **NarrativeRequirement** - IRequirement implementation
4. **NarrativeStep** - Configuration classes

### Phase 2: Integration Points
1. **LocationActionManager**
   - Add narrative requirement checking
   - Filter actions based on active narratives
   - Report action completion to NarrativeManager

2. **GameWorld**
   - Add FlagService instance
   - Add NarrativeManager instance
   - Include in serialization

3. **ConversationManager**
   - Check for narrative-specific dialogue branches
   - Report conversation outcomes to FlagService

### Phase 3: UI Components
1. **NarrativeOverlay**
   - Shows current objective
   - Displays guidance text
   - Highlights relevant UI elements

2. **MainGameplayView Integration**
   - Check for active narratives
   - Show/hide narrative overlay
   - Block certain navigation during critical steps

### Phase 4: Content Configuration
1. **narrative_definitions.json**
   - Tutorial sequence (10 days)
   - Quest templates
   - Story moments

2. **narrative_dialogues.json**
   - NPC dialogue variations for narrative steps
   - Contextual responses based on progress

## Tutorial Narrative Breakdown

### Day 1: Movement and Survival
**Steps**:
1. `wake_warehouse` - Starting narrative
2. `first_movement` - Learn movement mechanics
3. `meet_tam` - First NPC interaction
4. `discover_work` - Learn about schedules
5. `survival_choice` - Eat or save money

**Controlled Elements**:
- Only Tam visible initially
- Limited movement options
- Forced rest at night

### Day 2: Work and Relationships
**Steps**:
1. `morning_opportunity` - Work becomes available
2. `first_work` - Complete work action
3. `earn_token` - First relationship token
4. `meet_elena` - Second NPC appears
5. `sharing_choice` - Build relationships

**Controlled Elements**:
- Martha appears in morning only
- Elena appears after work
- Letter offers not yet available

### Day 3: Letter Discovery
**Steps**:
1. `letter_opportunity` - Token enables letter offer
2. `accept_letter` - First queue entry
3. `collect_package` - Physical collection
4. `first_delivery` - Complete delivery
5. `network_growth` - NPCs notice success

**Controlled Elements**:
- Letter offers from NPCs with tokens
- Simple delivery (no conflicts)
- Growing NPC availability

### Days 4-10: Advanced Mechanics
- Queue conflicts and token burning
- Standing obligations
- Patron introduction
- Complex choices

## Flag Naming Convention

### Event Flags
- `narrative_[id]_started`
- `narrative_[id]_completed`
- `narrative_[id]_step_[stepId]`

### Counter Keys
- `narrative_[id]_steps_completed`
- `narrative_[id]_choices_made`
- `narrative_[id]_days_elapsed`

### Generic Game Events
- `first_token_earned`
- `first_letter_accepted`
- `first_delivery_completed`
- `first_obligation_accepted`

## Integration with Existing Systems

### LocationActionManager
```csharp
public List<ActionOption> GetAvailableActions(LocationSpot spot)
{
    var actions = GenerateBaseActions(spot);
    
    // Let narrative system filter if active
    if (_narrativeManager.HasActiveNarrative())
    {
        actions = _narrativeManager.FilterActions(actions);
    }
    
    return actions;
}
```

### ConversationManager
```csharp
public async Task<string> GenerateIntroduction(ConversationContext context)
{
    // Check for narrative-specific introduction
    var narrativeIntro = _narrativeManager.GetNarrativeIntroduction(context);
    if (narrativeIntro != null)
        return narrativeIntro;
        
    // Normal introduction flow
    return await _narrativeProvider.GenerateIntroduction(context);
}
```

### NPCRepository
```csharp
public List<NPC> GetNPCsAtSpot(string spotId, TimeBlocks time)
{
    var npcs = base.GetNPCsAtSpot(spotId, time);
    
    // Let narrative system filter NPCs
    if (_narrativeManager.HasActiveNarrative())
    {
        npcs = npcs.Where(npc => _narrativeManager.ShouldShowNPC(npc.ID)).ToList();
    }
    
    return npcs;
}
```

## Benefits of This Approach

1. **No Special Rules**: Uses existing mechanics with filtered availability
2. **Reusable**: Same system for tutorials, quests, and story
3. **Maintainable**: Clear separation of concerns
4. **Testable**: Each component can be unit tested
5. **Data-Driven**: Narratives defined in JSON, not code
6. **Emergent**: Players can still discover unintended solutions

## Testing Strategy

### Unit Tests
- FlagService state management
- NarrativeRequirement satisfaction logic
- NarrativeManager step progression
- Action filtering logic

### Integration Tests
- Complete tutorial flow
- Save/load with active narratives
- Narrative interruption and resumption
- Multiple concurrent narratives

### Content Validation
- All narrative steps reachable
- No soft-locks possible
- Guidance text accuracy
- Reward delivery

## Future Extensions

### Quest System
- Same NarrativeManager handles quests
- Quest chains through narrative steps
- Branching paths based on choices
- Persistent quest log UI

### Story Moments
- Triggered narratives for key events
- Patron reveals
- Major discoveries
- Character development

### Achievement System
- FlagService tracks all events
- Achievement definitions check flags
- Natural integration with narratives

## Implementation Order

1. **Core Classes** (4 hours)
   - NarrativeManager
   - NarrativeRequirement
   - NarrativeStep/Definition models

2. **System Integration** (4 hours)
   - LocationActionManager modifications
   - GameWorld integration
   - Serialization support

3. **UI Components** (4 hours)
   - NarrativeOverlay
   - MainGameplayView integration
   - Guidance tooltips

4. **Content Creation** (6 hours)
   - Tutorial narrative JSON
   - NPC dialogue variations
   - Testing and refinement

5. **Polish** (2 hours)
   - Animation transitions
   - Sound cues
   - Edge case handling

Total Estimate: ~20 hours for complete implementation

## Success Criteria

1. Player can complete tutorial without getting stuck
2. No special mechanics - only filtered choices
3. System reusable for future narratives
4. Clean code following architecture principles
5. Comprehensive test coverage
6. Tutorial teaches all core mechanics effectively