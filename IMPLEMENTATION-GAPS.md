# Implementation Gaps for Full Strategic Vision

## 🔴 CRITICAL MISSING SYSTEMS

### 1. Token Progression System
**Current**: Tokens exist but don't affect gameplay
**Needed**:
- Tokens unlock new cards in NPC decks (at thresholds like 3, 5, 10)
- Each token adds +5% success to matching card type
- Token requirements gate letter availability
- Tokens can be burned for queue manipulation

**Implementation**:
```csharp
// When delivering letter successfully
npc.AddTokens(letterType, 1);
if (npc.Tokens[letterType] >= 3) {
    npc.ConversationDeck.AddCard(GetUpgradedCard(letterType, 3));
}

// In card success calculation
var tokenBonus = tokens[card.Type] * 5;
successRate += tokenBonus;
```

### 2. Obligation Queue Displacement
**Current**: Queue is rigid, position 1 only
**Needed**:
- Deliver out of order by burning tokens
- Burned tokens add burden cards to NPC deck
- Each obligation specifies burn cost type

**Implementation**:
```csharp
public class Obligation {
    public ConnectionType DisplacementCost { get; set; }
    public int TokensBurnedOnDisplacement { get; set; } = 1;
}

// To deliver position 2 before position 1
var displaced = queue[0];
var cost = displaced.DisplacementCost;
player.BurnTokens(displaced.NPCId, cost, 1);
npc.ConversationDeck.AddBurdenCard(cost);
```

### 3. Letter Negotiation Terms
**Current**: Letters have fixed terms
**Needed**:
- Success: Better deadline, flexible position, standard pay
- Failure: Tight deadline, forced position 1, higher pay
- Crisis letters always try for position 1

**Implementation**:
```csharp
public class LetterCard {
    public LetterTerms SuccessTerms { get; set; }
    public LetterTerms FailureTerms { get; set; }
}

public class LetterTerms {
    public int DeadlineHours { get; set; }
    public QueuePosition Position { get; set; } // Fixed, Flexible, ForceFirst
    public int Payment { get; set; }
}
```

### 4. Observation Decay System
**Current**: Observations are permanent
**Needed**:
- Fresh (0-2h): Full effect
- Stale (2-6h): Half comfort  
- Expired (6+h): Must discard
- Cost 1 attention to observe

**Implementation**:
```csharp
public class ObservationCard {
    public DateTime ObservedAt { get; set; }
    public ObservationState GetState() {
        var age = DateTime.Now - ObservedAt;
        if (age < TimeSpan.FromHours(2)) return ObservationState.Fresh;
        if (age < TimeSpan.FromHours(6)) return ObservationState.Stale;
        return ObservationState.Expired;
    }
}
```

### 5. Access Permit System
**Current**: Routes have no access control
**Needed**:
- Routes require access permits (special letters)
- Permits take satchel space, no obligation
- Obtained via high-token cards (5+ shadow → "Grant Permit")

**Implementation**:
```csharp
public class AccessPermit : Letter {
    public string RouteId { get; set; }
    public override bool CreatesObligation => false;
}

// In NPC deck when shadow tokens >= 5
new ConversationCard {
    Name = "Request Smuggler's Pass",
    RequiredTokens = new() { [ConnectionType.Shadow] = 5 },
    SuccessEffect = () => player.Satchel.Add(new AccessPermit { RouteId = "nobles_gate" })
}
```

### 6. Card Unlock Progression
**Current**: All cards available from start
**Needed**:
- 0 tokens: Basic cards only
- 3 tokens: Intermediate cards added
- 5 tokens: Advanced cards added
- 10 tokens: Master cards added

**Implementation**:
```csharp
public class CardTemplate {
    public int RequiredTokens { get; set; }
    public ConnectionType TokenType { get; set; }
}

// During deck initialization
foreach (var template in cardTemplates) {
    if (npc.Tokens[template.TokenType] >= template.RequiredTokens) {
        deck.Add(CreateCard(template));
    }
}
```

## 🟡 MECHANICAL REFINEMENTS

### 7. Distinct Effect Pools
**Current**: Cards have mixed effects
**Needed**: Each card type has UNIQUE effect pool

**COMFORT**: Only comfort gain
**STATE**: Only state change
**CRISIS**: Only crisis resolution
**LETTER**: Only obligation creation
**BURDEN**: Only deck clogging
**OBSERVATION**: Only temporary knowledge

### 8. Work Actions Generate Coins
**Current**: No way to earn coins
**Needed**:
- Work: 2 attention → 8 coins
- Different work types at different locations
- Commerce spots offer better work

### 9. Rest Exchanges
**Current**: No way to recover attention
**Needed**:
- Tavern: 5 coins → Full attention
- Inn: 10 coins → Full attention + remove 20 hunger
- Camp: Free → Half attention

## 📊 PRIORITY ORDER

1. **Token progression** - Core to everything
2. **Queue displacement** - Major strategic choice
3. **Letter negotiation** - Risk/reward in conversations
4. **Observation decay** - Time pressure on knowledge
5. **Access permits** - Gates exploration
6. **Work/Rest** - Basic economy loop

Without these systems, the game is just picking cards randomly. With them, it becomes a deep strategic puzzle where every choice cascades through multiple systems.