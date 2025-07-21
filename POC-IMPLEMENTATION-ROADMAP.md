# Wayfarer POC Implementation Roadmap

## Overview

This roadmap outlines the path to complete POC implementation following our core architectural principles:
- NO CLASS INHERITANCE - Use composition and helper methods
- NO SPECIAL RULES - Create categorical mechanics, not exceptions
- REUSE EXISTING SYSTEMS - Extend functionality within existing classes

## Current Implementation Status

### ✅ Completed Systems
1. **Core Letter Queue** - 8-slot priority queue with proper shifting
2. **Leverage System** - Token debt affects letter entry positions categorically
3. **Token Management** - Per-NPC tracking with debt support
4. **Environmental Actions** - Domain tag-based action generation
5. **Emergency Actions** - Patron requests, borrowing, illegal work

### 🔄 Partially Implemented
1. **Letter States** - Three-state system exists (Offered/Queued/Collected)
2. **ConversationManager** - AI-driven conversations exist
3. **Delivery Actions** - Simple execution without conversation

### ❌ Not Implemented
1. **Deterministic Conversations** - Non-AI conversation mode
2. **Action-Conversation Integration** - All actions trigger conversations
3. **Letter Discovery Conversations** - NPCs offer letters through dialogue
4. **Physical Letter Collection** - Inventory management
5. **Standing Obligations** - Permanent queue modifiers
6. **Travel Encounters** - Route-based conversations

## Implementation Phases

### Phase 1: Thin Narrative Layer (Immediate)
**Goal**: Minimal narrative wrapper for actions - one sentence, one button, done

#### 1.1 Simple Narrative Flow
```csharp
// Action selection → Narrative sentence → "Continue" button → Execute action → Show results
public class DeterministicNarrativeProvider : INarrativeProvider
{
    public Task<string> GenerateIntroduction(ConversationContext context, ConversationState state)
    {
        var actionContext = context as ActionConversationContext;
        return Task.FromResult(GetActionNarrative(actionContext.ActionType));
    }
    
    public Task<List<ConversationChoice>> GenerateChoices(...)
    {
        // Single "Continue" button for thin layer
        return Task.FromResult(new List<ConversationChoice> 
        { 
            new ConversationChoice { Text = "Continue", Value = "continue" }
        });
    }
}
```

#### 1.2 Action Narratives
Simple one-sentence narratives for each action type:
- GatherResources: "You carefully search the area for edible berries."
- Browse: "You examine the market stalls, noting prices and goods."
- Observe: "You blend into the crowd, listening to local gossip."
- Work: "You begin your work for [NPC Name]."
- Socialize: "You engage [NPC Name] in friendly conversation."

#### 1.3 Direct Execution
- No complex choices or branching
- Action executes immediately after "Continue" 
- Results shown via MessageSystem
- Return to location screen

### Phase 2: Action-Conversation Integration (Week 1-2)
**Goal**: Connect LocationActionManager with ConversationManager

#### 2.1 Extend ActionOption
```csharp
public class ActionOption
{
    // Existing properties...
    
    // Conversation properties
    public bool RequiresConversation { get; set; }
    public List<ChoiceTemplate> ConversationTemplates { get; set; }
    public string InitialNarrative { get; set; }
}
```

#### 2.2 Modify LocationActionManager
```csharp
public bool ExecuteAction(ActionOption option)
{
    if (option.RequiresConversation)
    {
        LaunchActionConversation(option);
        return true;
    }
    // Existing direct execution...
}

private void LaunchActionConversation(ActionOption option)
{
    var context = CreateActionContext(option);
    var conversation = _conversationFactory.CreateConversation(context, _player);
    conversation.EnableDeterministicMode(option.ConversationTemplates);
    _gameWorld.SetPendingConversation(conversation, option);
}
```

#### 2.3 Update MainGameplayView
- Poll for pending conversations
- Switch to ConversationScreen
- Handle completion callbacks
- Process action outcomes

### Phase 3: Letter Queue Conversations (Week 2-3)
**Goal**: All letter interactions through conversations

#### 3.1 Queue Skipping System
- Calculate token costs for skipping
- Create skip confirmation conversation
- Show affected NPCs and relationships
- Implement token burning

#### 3.2 Letter Discovery Flow
- Modify Converse action to check for letters
- Use token thresholds for letter categories
- Create offer/decline conversation flow
- Show queue impact before acceptance

#### 3.3 Letter Collection Mechanics
- Check inventory space requirements
- Create collection conversation if full
- Handle drop/reorganize choices
- Update letter state transitions

### Phase 4: Delivery Conversations (Week 3-4)
**Goal**: Rich narrative delivery experiences

#### 4.1 Delivery Context System
```csharp
public class DeliveryContext
{
    public Letter Letter { get; set; }
    public bool IsLate { get; set; }
    public bool IsFragile { get; set; }
    public int RelationshipLevel { get; set; }
    public List<ChoiceTemplate> GenerateTemplates() { }
}
```

#### 4.2 Contextual Delivery Choices
- Token vs coin rewards
- Private vs public delivery
- Accept return letter
- Share sender news

#### 4.3 Post-Delivery Opportunities
- New letter offers
- Route discoveries
- NPC connections
- All through choices

### Phase 5: Standing Obligations (Week 4-5)
**Goal**: Permanent gameplay modifiers (no special rules!)

#### 5.1 Obligation as Categorical System
```csharp
public class StandingObligation
{
    public ObligationEffect Effect { get; set; }
    public int LeverageModifier { get; set; }
    public ConnectionType AffectedType { get; set; }
    
    // Obligations modify existing systems, not create special rules
    public int ModifyLeverageCalculation(int baseLeverage)
    {
        return baseLeverage + LeverageModifier;
    }
}
```

#### 5.2 Patron's Expectation
- Implemented as high starting leverage
- Not a special rule, just debt mechanics
- Can be reduced through gameplay

#### 5.3 Additional Obligations
- Noble's Courtesy: +2 leverage for nobles
- Shadow's Burden: -1 leverage resistance
- All work through leverage system

### Phase 6: Physical Systems & Travel (Week 5-6)
**Goal**: Complete remaining mechanics

#### 6.1 Physical Letter Management
- Inventory slot requirements
- Three-state transitions
- Collection conversations
- Multi-purpose items

#### 6.2 Travel Encounters
- Route-based conversation triggers
- Equipment enables choices
- Time/resource modifications
- Discovery opportunities

#### 6.3 Route Discovery
- NPC knowledge system
- Token threshold unlocks
- Conversation-based reveals
- No special discovery rules

### Phase 7: Integration & Polish (Week 6-7)
**Goal**: Cohesive, polished experience

#### 7.1 System Integration
- Ensure all systems interconnect
- No isolated mechanics
- Emergent complexity from simple rules

#### 7.2 UI Polish
- Clear consequence display
- Token debt visualization
- Queue state clarity
- Action outcome previews

#### 7.3 Balance Testing
- Token economy flow
- Deadline pressure tuning
- Resource scarcity validation
- Choice meaningfulness

## Key Implementation Principles

### 1. Use Existing Classes
- ConversationManager gains deterministic mode
- ActionOption gains conversation properties
- No new manager classes needed

### 2. Categorical Mechanics Only
- Leverage affects ALL letters
- Obligations modify existing systems
- No "if patron then X" special cases

### 3. Composition Pattern
```csharp
// ❌ WRONG
public class DeterministicConversationManager : ConversationManager

// ✅ RIGHT
public class ConversationManager
{
    private IDeterministicGenerator _deterministicGenerator;
}
```

### 4. Clear Player Communication
- All effects visible before choosing
- No hidden mechanics
- Consistent UI patterns

## Success Metrics

1. **Playability** - Game remains functional throughout
2. **Emergence** - Complex strategies from simple rules
3. **Clarity** - Players understand all mechanics
4. **Performance** - Smooth conversation transitions
5. **Cohesion** - All systems feel connected

## Timeline Summary

- **Week 1**: Deterministic conversation support
- **Week 2**: Action-conversation integration
- **Week 3**: Letter queue conversations
- **Week 4**: Delivery conversations
- **Week 5**: Standing obligations
- **Week 6**: Physical systems & travel
- **Week 7**: Polish & testing

This roadmap creates a complete POC while maintaining architectural integrity and avoiding special-case programming.