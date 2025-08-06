# Conversation-as-Mechanics Integration Design
## The Complete Framework for Wayfarer's Core Interaction System

## Executive Summary

All game mechanics in Wayfarer execute through conversation. Players manipulate the letter queue, exchange connection tokens, unlock routes/NPCs, and create obligations entirely through dialogue choices. This document defines the complete integration framework.

## Core Principle: Conversations ARE the Game

There are no separate mechanical interfaces. NPCs are the UI for:
- Letter queue management
- Token exchanges
- Route/location discovery
- Obligation creation/resolution
- Information trading

## The Attention Economy

### Binary System
- **0 Attention**: Surface mechanics available
  - Basic token trades (+1/-1)
  - Simple queue operations
  - Standard information exchanges
  - Observable NPC states

- **1 Attention**: Deep mechanics unlocked
  - Complex obligations
  - Multi-system trades
  - Hidden discoveries
  - Leverage plays

### Why Binary Works
- Clear cognitive model (engaged vs observing)
- Prevents decision paralysis
- Natural conversation flow
- Matches real social dynamics

## Mechanical Integration Framework

### Every Choice Touches 2-3 Systems

```csharp
public class ConversationChoice {
    // Narrative presentation (what player sees)
    public string Text { get; set; }
    
    // Mechanical effects (2-3 typically active)
    public QueueEffect? Queue { get; set; }
    public TokenChange? Tokens { get; set; }
    public Obligation? Creates { get; set; }
    public Unlock? Reveals { get; set; }
    public Information? Shares { get; set; }
    
    // Cost
    public int AttentionRequired { get; set; }
    
    // Atomic execution
    public GameState Execute(GameState state) {
        // All changes succeed or all rollback
    }
}
```

### Surface Mechanics (0 Attention)

#### Token Building
```
"I appreciate your patience" 
→ Spend 20 minutes
→ Gain +1 Trust token
→ NPC remembers this kindness
```

#### Queue Negotiation
```
"I'll carry your letter"
→ Add to queue at position 6
→ Gain +1 appropriate token
→ Create soft expectation
```

#### Information Trading
```
"I heard about the morning carriage"
→ Share known information
→ Receive different information
→ Both parties benefit
```

### Deep Mechanics (1 Attention)

#### Obligation Creation
```
"I'll always prioritize your letters"
→ Create standing rule
→ Their letters enter at position 2
→ Gain +5 Trust tokens
→ Permanent relationship change
```

#### Complex Discovery
```
"Is there another way through?"
→ Reveal hidden route exists
→ Learn access requirements
→ Opens new strategic options
→ Changes travel gameplay
```

#### Leverage Plays
```
"Remember what I know about the debts?"
→ Use information as pressure
→ Force favorable trade
→ Damage relationship
→ Create future consequences
```

## Specific Integration Examples

### Complete Conversation: Desperate Merchant

```
SCENE: Market Square, Morning
CONTEXT: Merchant has urgent letter, you're behind schedule

PERIPHERAL (No focus):
"Marcus paces near his stall, glancing at you repeatedly"

SURFACE OPTIONS (0 Attention):
1. "What's troubling you?"
   → Learn about urgent letter
   → No mechanical change
   → Opens negotiation

2. "I'll take your letter"
   → Add to queue position 7
   → Gain +1 Commerce token
   → 2-day deadline added

3. "Not today, Marcus"
   → No mechanical change
   → Lose -1 Commerce token
   → Conversation ends

DEEP OPTIONS (1 Attention):
1. "Let's solve this permanently"
   → Create "Marcus Priority" obligation
   → His letters always position 3 or better
   → Gain +3 Commerce tokens
   → Unlock merchant guild connections

2. "I know about the stolen goods"
   → Use leverage for queue priority
   → Letter jumps to position 1
   → Lose -3 Commerce tokens permanently
   → Marcus becomes hostile

3. "My patron could help your business"
   → Unlock new trade route for Marcus
   → Receive insider trading information
   → Create mutual obligation
   → Opens smuggling questline
```

### Location Investigation

```
COPPER KETTLE TAVERN - Evening

OBSERVATION (0 Attention):
- See: Guards drinking heavily
- See: Elena in corner booth
- See: Crowded, noisy atmosphere

INVESTIGATION (1 Attention):
Discover: Hidden notice board (Shadow letters)
Discover: Guard captain takes bribes
Discover: Elena meeting someone secretly
Discover: Back room available for meetings
```

## UI/UX Implementation

### Critical: Maximum 5 Choices

```csharp
public List<Choice> FilterChoices(List<Choice> all) {
    var filtered = new List<Choice>();
    
    // Always show critical/urgent
    filtered.AddRange(all.Where(c => c.IsCritical).Take(2));
    
    // Add contextually relevant
    filtered.AddRange(all.Where(c => c.MatchesContext).Take(2));
    
    // Include one standard option
    filtered.Add(all.First(c => c.IsStandard));
    
    return filtered.Take(5);
}
```

### Visual Hierarchy

```html
<div class="conversation-interface">
    <!-- Primary: Current speaker state -->
    <div class="npc-state">
        <h3>Elena - Anxious</h3>
        <p class="body-language">Fingers worrying her shawl</p>
    </div>
    
    <!-- Secondary: Available choices -->
    <div class="choices">
        <!-- Surface choice (0 attention) -->
        <div class="choice surface">
            <em>"I'll handle your letter today"</em>
            <span class="consequences">
                ↕️ Affects queue order
            </span>
        </div>
        
        <!-- Deep choice (1 attention) -->
        <div class="choice deep">
            <em>"Let's make a permanent arrangement"</em>
            <span class="attention-cost">● Requires focus</span>
            <span class="consequences">
                ⛓ Creates lasting obligation
            </span>
        </div>
    </div>
    
    <!-- Peripheral: Environmental awareness -->
    <div class="peripheral-info">
        <span class="deadline-pressure">💀 Lord B's letter: 2 hours</span>
    </div>
</div>
```

### Emotional Framing Over Mechanical Display

❌ **Wrong**: "Spend 2 Trust tokens for information"
✅ **Right**: "Call in the favor from last winter"

❌ **Wrong**: "Reorder queue: Position 6 → Position 1"  
✅ **Right**: "This means delaying everyone else"

❌ **Wrong**: "Create StandingObligation object"
✅ **Right**: "This promise will bind you"

## State Management

### Transaction Atomicity

```csharp
public class ConversationTransaction {
    private List<IGameChange> changes = new();
    private GameState checkpoint;
    
    public void Begin(GameState current) {
        checkpoint = current.DeepClone();
    }
    
    public void Add(IGameChange change) {
        changes.Add(change);
    }
    
    public GameState Commit() {
        try {
            var state = checkpoint;
            foreach (var change in changes) {
                state = change.Apply(state);
            }
            return state;
        }
        catch {
            return checkpoint; // Rollback on any failure
        }
    }
}
```

### Edge Case Handling

1. **Queue Overflow During Conversation**
   - Solution: Deferred addition with overflow warning
   
2. **Circular Obligation Dependencies**
   - Solution: DAG validation before creation
   
3. **Token Underflow**
   - Solution: Reserve tokens during choice preview
   
4. **Information Paradoxes**
   - Solution: Two-phase reveal (existence, then details)

## Narrative Preservation

### Making Mechanics Feel Human

Every mechanical action needs emotional weight:

```
Accepting letter → "The weight of their trust"
Burning tokens → "Calling in carefully built favors"  
Creating obligation → "Words that will haunt you"
Sharing information → "Secrets that change everything"
```

### NPC Humanity

NPCs must feel like people, not vending machines:
- Varied responses to same mechanical state
- Emotional reactions to player choices
- Memory of past interactions
- Conflicting desires and needs

## Implementation Priorities

### Phase 1: Core Framework (Week 1)
- [ ] ConversationChoice class structure
- [ ] Transaction system for atomic changes
- [ ] Choice generation from game state
- [ ] Basic UI components

### Phase 2: Mechanical Integration (Week 2)
- [ ] Queue manipulation through dialogue
- [ ] Token exchange systems
- [ ] Obligation creation/tracking
- [ ] Information discovery mechanics

### Phase 3: UI Polish (Week 3)
- [ ] 5-choice filtering algorithm
- [ ] Visual hierarchy implementation
- [ ] Emotional framing for all mechanics
- [ ] Peripheral awareness system

### Phase 4: Edge Cases (Week 4)
- [ ] Overflow handling
- [ ] Circular dependency prevention
- [ ] State validation
- [ ] Rollback mechanisms

## Success Metrics

Players should:
1. Never see more than 5 choices
2. Understand consequences before choosing
3. Feel they're navigating relationships, not menus
4. Remember conversations as human moments
5. Discover mechanics through natural play

## Critical Warnings

### Avoid These Pitfalls

1. **Don't show mechanical labels**
   - Hide verbs, states, and systems
   
2. **Don't create decision paralysis**
   - Strictly enforce 5-choice maximum
   
3. **Don't reduce NPCs to functions**
   - Every NPC needs personality beyond mechanics
   
4. **Don't allow inconsistent state**
   - All changes atomic or none happen

## Final Design Statement

Conversations in Wayfarer aren't a way to access mechanics - they ARE the mechanics. Every promise reshapes the queue. Every favor trades in tokens. Every secret unlocks new paths. The game happens in dialogue, making every conversation a strategic puzzle wrapped in a human moment.

This isn't a game with conversations. It's a game that IS conversations.