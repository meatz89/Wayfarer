# Wayfarer Literary Mechanics Design
## Updated: 2025-01-06

## Executive Summary

This document defines Wayfarer's conversation-as-mechanics system where ALL game interactions occur through dialogue. Conversations directly manipulate the letter queue, connection tokens, route/NPC unlocks, and standing obligations. The attention economy (0 vs 1 point) gates discovery and complexity.

## Core Design Principles

### 1. Conversations ARE the Game Interface
- Every mechanical action happens through dialogue
- No separate menus for queue/tokens/obligations
- NPCs are the interface to all game systems
- Dialogue choices have mechanical teeth

### 2. Binary Attention Economy
- 0 Attention = Surface mechanics (basic trades, simple actions)
- 1 Attention = Deep mechanics (discoveries, complex trades, obligations)
- Attention gates complexity, not just options
- Discovery requires investment

### 3. Every Choice Touches Multiple Systems
- Queue manipulation + token exchange
- Information reveal + route unlock
- Obligation creation + relationship shift
- No single-purpose interactions

## The Letter System

### Core Letter Properties
```csharp
public struct Letter {
    TokenType Type;      // Trust|Commerce|Status|Shadow
    StakeType Stakes;    // REPUTATION|WEALTH|SAFETY|SECRET  
    int Weight;          // 1-5 queue slots (Tetris-like decisions)
    int TTL;             // Days remaining (creates urgency)
    (NPCId, NPCId) Route; // Sender → Recipient
}
```

### Stakes System (4 Types)
- **REPUTATION**: Social consequences, standing at risk
- **WEALTH**: Financial consequences, economic impact
- **SAFETY**: Physical danger, threats to wellbeing
- **SECRET**: Hidden information, dangerous knowledge

### Letter Type × Stakes = 16 Narrative Contexts
```
Trust + REPUTATION = "Personal scandal"
Trust + WEALTH = "Family financial crisis"  
Trust + SAFETY = "Warning between friends"
Trust + SECRET = "Dangerous confession"

Commerce + REPUTATION = "Merchant credibility"
Commerce + WEALTH = "Urgent trade deal"
Commerce + SAFETY = "Dangerous cargo"
Commerce + SECRET = "Smuggling operation"

Status + REPUTATION = "Noble standing"
Status + WEALTH = "Inheritance dispute"
Status + SAFETY = "Challenge to duel"
Status + SECRET = "Court intrigue"

Shadow + REPUTATION = "Blackmail threat"
Shadow + WEALTH = "Thieves' guild dues"
Shadow + SAFETY = "Assassination warning"
Shadow + SECRET = "Deadly information"
```

### Weight System (Queue Management)
- Weight 1: Simple note (1 slot)
- Weight 2: Standard letter (2 slots)
- Weight 3: Important package (3 slots)
- Weight 4: Major correspondence (4 slots)
- Weight 5: Crisis documentation (5 slots)

Creates Tetris-like decisions: Take three small letters or one important package?

### TTL and Escalation
```
Days 1-2: Normal stakes
Days 3-4: Stakes compound (REPUTATION → REPUTATION + WEALTH)
Day 5+: Crisis cascade (affects multiple NPCs)
```

## NPC Emotional State System

### States Emerge from Letter Properties
```csharp
NPCState = CalculateFromLetter(npc.Letter);

DESPERATE: TTL < 2 days OR Stakes include SAFETY
HOSTILE: Player has overdue letters to them
CALCULATING: Balanced pressure, no immediate crisis
WITHDRAWN: No letters in queue, no active needs
```

### State Effects on Interaction
- **DESPERATE**: Actions cost -1 attention (min 0), reveals info freely
- **HOSTILE**: Actions cost +1 attention (max 2), some verbs locked
- **CALCULATING**: Standard costs, normal interactions
- **WITHDRAWN**: Limited options, scene ends quickly

### Body Language Generation
```csharp
string GenerateBodyLanguage(NPCState state, Stakes stakes) {
    // DESPERATE + REPUTATION = "fingers worrying shawl, eyes darting"
    // HOSTILE + WEALTH = "arms crossed, jaw clenched"
    // CALCULATING + SECRET = "measured breathing, careful words"
    // WITHDRAWN + SAFETY = "shoulders hunched, avoiding eye contact"
}
```

## Conversation Mechanics Framework

### Base Options (0 Attention)
```csharp
// Token Exchange
"I appreciate your patience" → Build: +1 token (costs 20 min)
"I need this favor" → Burn: -1 token for immediate help
"Let me introduce you" → Trade: Convert Trust to Commerce

// Queue Manipulation  
"I'll carry your letter" → Add to queue at position 6
"Give me more time" → Request deadline extension
"Your letter goes first" → Reorder queue (burns Status)

// Information Trade
"I know a shortcut" → Share route information
"Who handles imports?" → Request NPC information
"What do you need?" → Learn mechanical requirements
```

### Deep Options (1 Attention)
```csharp
// Obligation Creation
"Let's make this permanent" → Create standing rule
"You're released from this" → Remove obligation
"Talk to my friend instead" → Transfer obligation

// Complex Exchanges
"Package deal" → Multiple simultaneous trades
"I'll owe you later" → Create future obligation
"Remember last winter?" → Use leverage for better terms

// Discovery Actions
"Is there another way?" → Reveal hidden route
"Who else knows about this?" → Unlock NPC connection
"What aren't you telling me?" → Uncover secret mechanism
```

### Contextual Presentation
Each verb presents differently based on:
- NPC emotional state
- Relationship tokens
- Letter stakes
- Current pressure

```
PLACATE variations:
- Trust + Desperate → "Take her hand in comfort"
- Commerce + Angry → "Offer partial payment"
- Status + Hostile → "Acknowledge their position"
- Shadow + Suspicious → "Show empty hands"

EXTRACT variations:
- Trust + Open → "Ask about their troubles"
- Commerce + Calculating → "Negotiate information trade"
- Status + Desperate → "Demand what you're owed"
- Shadow + Secretive → "Probe for weaknesses"
```

### Choice Generation from State
```csharp
public List<ConversationChoice> GenerateChoices(GameState state, NPC npc) {
    var choices = new List<ConversationChoice>();
    
    // Queue-based choices
    if (npc.HasLetterInQueue()) {
        choices.Add(new PrioritizeLetterChoice(npc.Letter) {
            Text = "I'll deliver your letter first",
            QueueEffect = MoveToPosition(1),
            TokenCost = new TokenChange(Status, -1), // Burns status with others
            AttentionCost = 0  // Surface mechanic
        });
    }
    
    // Token-based choices (deep mechanics)
    if (player.GetTokens(npc, Trust) >= 3 && player.Attention >= 1) {
        choices.Add(new RequestFavorChoice() {
            Text = "Could you introduce me to the merchant guild?",
            UnlockEffect = new LocationUnlock("MerchantGuildHall"),
            TokenCost = new TokenChange(Trust, -3),
            AttentionCost = 1  // Requires focus
        });
    }
    
    // Information trading
    if (npc.HasInformation() && player.HasTradeableInfo()) {
        choices.Add(new TradeInformationChoice() {
            Text = "I heard about the carriage schedule...",
            InfoGained = npc.Information,
            InfoShared = player.SelectedInfo,
            AttentionCost = 0  // Basic trade
        });
    }
    
    // Obligation creation (deep mechanics)
    if (npc.NeedsHelp() && player.Attention >= 1) {
        choices.Add(new CreateObligationChoice() {
            Text = "I'll always prioritize your letters",
            ObligationCreated = new StandingObligation(npc, PriorityRule),
            TokenGain = new TokenChange(Trust, +5),
            AttentionCost = 1
        });
    }
    
    return FilterToMaxFive(choices);  // Never overwhelm
}
```

## Complete Integration Examples

### Elena Conversation (Urgent Letter)
```
PERIPHERAL VIEW (No Attention):
"Elena sits alone, fingers drumming nervously"

FOCUS ELENA - Surface Mechanics (0 Attention):
"I'll help with your letter" 
→ Add her letter to queue position 6
→ Cost: Queue slot
→ Gain: +1 Trust token

"Tell me about court routes"
→ Learn "Noble Carriage Service exists"
→ Cost: 20 minutes  
→ Gain: Route knowledge (still need Status 3 to use)

"Can you hold my merchant letter?"
→ Remove letter from queue temporarily
→ Cost: -1 Trust token
→ Gain: Free queue slot

DEEP FOCUS - Complex Mechanics (1 Attention):
"Let's make a permanent arrangement"
→ Create obligation "Elena's Priority"
→ Effect: Her letters always enter at position 2
→ Cost: Major commitment
→ Gain: She shares all noble routes

"I know about Lord Aldwin's debts"  
→ Leverage creates new options
→ Effect: Can demand payment or information
→ Cost: Relationship damage if used
→ Gain: Power over situation

"My patron knows people at court"
→ Unlock introduction chain
→ Effect: New NPC becomes available
→ Cost: Patron obligation activated
→ Gain: Access to noble network
```

### Location Investigation Mechanics
```
0 ATTENTION OBSERVATION:
- See who's present
- Notice obvious activities
- Identify available services

1 ATTENTION INVESTIGATION:
- Discover hidden routes (mechanical unlock)
- Find secret NPCs (adds to available interactions)
- Uncover leverage information (creates options)
- Spot obligation opportunities (new rules possible)
```

## Implementation Architecture

### Transaction System for Multi-System Changes
```csharp
public class ConversationChoice {
    // Visual presentation
    public string NarrativeText { get; set; }
    
    // Mechanical effects (usually 2-3 present)
    public QueueManipulation? QueueEffect { get; set; }
    public TokenChange? TokenEffect { get; set; }
    public Obligation? ObligationCreated { get; set; }
    public Information? InfoRevealed { get; set; }
    public Unlock? UnlockGranted { get; set; }
    
    // Cost
    public int AttentionCost { get; set; }
    
    // Atomic execution
    public GameState Execute(GameState current) {
        var transaction = new GameTransaction();
        
        if (QueueEffect != null) 
            transaction.Add(QueueEffect);
        if (TokenEffect != null) 
            transaction.Add(TokenEffect);
        if (ObligationCreated != null) 
            transaction.Add(ObligationCreated);
            
        return transaction.Execute(current); // All or nothing
    }
}
```

### Choice Filtering to Prevent Overload
```csharp
public class SmartChoiceFilter {
    const int MAX_VISIBLE_CHOICES = 5;
    const int CRITICAL_CHOICES = 2;  // Always show urgent/important
    
    public List<Choice> FilterChoices(allChoices, context) {
        var critical = allChoices.Where(c => c.IsUrgent || c.IsCritical).Take(2);
        var contextual = allChoices.Where(c => c.MatchesCurrentContext).Take(2);  
        var standard = allChoices.Where(c => c.IsAlwaysAvailable).Take(1);
        
        return critical.Concat(contextual).Concat(standard).Take(5);
    }
}
```

### UI Presentation Principles
```html
<!-- Never show mechanical labels directly -->
<div class="conversation-choice" @onclick="SelectChoice">
    <div class="narrative-text">
        <em>@choice.NarrativeText</em> <!-- Human moment, not mechanic -->
    </div>
    
    @if (choice.QueueEffect != null) {
        <div class="queue-consequence">
            <!-- Frame as relationship cost, not queue manipulation -->
            ↕️ This affects your other commitments
        </div>
    }
    
    @if (choice.TokenEffect?.Amount < 0) {
        <div class="token-consequence">
            <!-- Frame as favor/relationship, not currency -->
            ⚠ Calling in old favors
        </div>
    }
    
    @if (choice.ObligationCreated != null) {
        <div class="obligation-warning">
            <!-- Frame as promise, not game mechanic -->
            ⛓ This promise will bind you
        </div>
    }
    
    <!-- Attention represented visually, never numerically -->
    <div class="attention-indicator">
        @if (choice.AttentionCost == 1) {
            <span class="deep-focus">●</span> <!-- Requires focus -->
        }
    </div>
</div>
```

## Consequence System

### Stakes Resolution
```csharp
void ProcessExpiredLetter(Letter letter) {
    switch(letter.Stakes) {
        case REPUTATION:
            // Future letters start at worse queue positions
            FutureLetterPenalty += 2;
            break;
            
        case WEALTH:
            // Commerce opportunities dry up
            RemoveCommerceLetters(letter.Sender);
            break;
            
        case SAFETY:
            // Shadow letters increase (desperation)
            IncreaseShadowGeneration();
            break;
            
        case SECRET:
            // Information spreads to all NPCs
            BroadcastInformation(letter.Route);
            break;
    }
}
```

### Escalating Consequences
- First failure: Individual NPC reaction
- Second failure: Network effects (friends/associates react)
- Third failure: Systemic changes (entire contexts affected)

## Critical Design Decisions

### Why This Works
1. **Single Interface**: Everything happens through conversation
2. **Mechanical Depth**: Every choice affects multiple systems
3. **Emotional Truth**: Mechanics feel like human decisions
4. **Cognitive Clarity**: Max 5 choices prevents overload
5. **Discovery Reward**: Attention investment reveals depth

### Key Safeguards
- **Never more than 5 choices** (prevents decision paralysis)
- **Context filtering** (only show relevant options)
- **Progressive disclosure** (complexity hidden until needed)
- **Emotional framing** (mechanics as relationships)
- **Transaction atomicity** (all changes succeed or none)

### Edge Cases Handled
- Queue overflow during conversation → Deferred addition
- Circular obligations → DAG validation
- Token underflow → Reservation system
- State inconsistency → Atomic rollback
- Information paradoxes → Two-phase commit

## Example Complete Scene

```
LOCATION: Copper Kettle Tavern

PERIPHERAL (Free):
"Warm hearth light flickers across worried faces. Elena sits alone, 
fingers drumming. Guards shift near the door. Your satchel weighs 
heavy with three letters."

FOCUS ON ELENA (1 attention):
State: DESPERATE (Trust + REPUTATION + TTL:2)
"She leans forward, fingers worrying her shawl"

GENERATED CHOICES:
1. "I understand. Your letter is second in queue." [Truth - free]
2. "Your letter goes first, whatever the cost." [Promise - 1 att]
3. "Whose proposal are you refusing?" [Probe - 1 att]
4. [Take her trembling hand] [Comfort - 1 att]

PLAYER CHOOSES: "Your letter goes first"
MECHANICAL EFFECT: Reorder queue, burn Status token with Lord B
NARRATIVE RESULT: "Relief floods her features. 'You don't know what this means...'"

ATTENTION REMAINING: 1
Can make one more focused choice or observe peripherally
```

## Key Design Insights

1. **The Queue IS the Story**: Letters drive everything through mechanical properties
2. **Attention Creates Tension**: Limited focus forces hard choices
3. **States Not Scripts**: NPC emotions emerge from letter pressure
4. **Verbs Stay Hidden**: Players see actions, not mechanics
5. **Stakes Create Meaning**: Four simple stakes generate infinite situations

## Critical Success Factors

- **Consequences Must Be Visible**: Players need to see/feel the impact
- **Pressure Must Escalate**: Each scene should feel more urgent
- **Choices Must Matter**: Every verb selection has ripple effects
- **Mystery Preserves Interest**: Not knowing exact letter contents creates curiosity
- **Simplicity Enables Depth**: Few mechanics, endless combinations

## What This Achieves

- **Mechanical Elegance**: 5 letter properties + 4 verbs + 3 attention points
- **Narrative Richness**: AI generates context-appropriate content
- **Production Feasibility**: 300 hours vs 1000+ for authored content
- **Emergent Storytelling**: Every playthrough unique based on queue state
- **Literary Presentation**: Mechanics invisible, narrative prominent

The letter queue creates the pressure. The attention economy creates the tension. The verbs create the choices. The stakes create the meaning. Everything else emerges.