# Wayfarer Implementation Plan
## From Design to Playable Game

### ✅ STATUS: CORE SYSTEMS IMPLEMENTED (Jan 10, 2025)
- ✅ All 10 core systems implemented and tested
- ✅ AI narrative framework defined  
- ⚠️ Visual polish needed to match mockups  
- 🔮 Ready for AI integration

### Document Purpose
This document defines EXACTLY what needs to be implemented to create a playable version of Wayfarer. No ambiguity, no "emerges from," no hand-waving. Every system, every interaction, every UI element is specified.

## Core Game Loop (The Minimum Viable Game)

### What The Player Does (30-Second Loop)
1. Wake up with letters in queue
2. See countdown timers for each letter
3. Choose which letter to deliver (only position 1)
4. Travel to delivery location (costs time)
5. Deliver letter or negotiate with NPCs
6. Manage queue through conversation
7. Watch consequences unfold
8. Repeat until day ends

### What Creates Pressure
- More letters than time to deliver
- Travel between locations costs time
- NPCs only available during certain hours
- Missing deadlines has permanent consequences
- Tomorrow brings more letters

## System Architecture

### 1. Core Data Models

#### Letter
```csharp
class Letter {
    Guid Id
    string SenderId  
    string RecipientId
    LetterType Type // Trust, Commerce, Status, Shadow
    StakeType Stakes // URGENT, VALUABLE, DANGEROUS
    int Weight // 1-3 slots in queue
    int HoursUntilDeadline // Countdown in hours
    string ContentHint // One-line description
    bool IsTravelPermit // Special letter type for routes
}
```

**Note**: Travel permits are special letters - see TRAVEL-SYSTEM-DESIGN.md

#### Queue
```csharp
class LetterQueue {
    Letter[] Slots // Fixed 8 slots
    int MaxWeight // Total weight capacity
    
    bool CanAdd(Letter)
    void Add(Letter) // Adds at lowest position
    void Deliver() // Removes position 1
    void Reorder(int from, int to) // Costs tokens
    Letter[] GetActiveLetters()
    int GetTotalWeight()
}
```

#### NPC
```csharp
class NPC {
    string Id
    string Name
    Location CurrentLocation
    Schedule Availability // When they're present
    EmotionalState State // Calculated from letters
    Dictionary<TokenType, int> Tokens // Relationship currency
}
```

#### Player State
```csharp
class PlayerState {
    LetterQueue Queue
    Location CurrentLocation
    int CurrentHour // Time of day (6-22)
    Dictionary<NPCId, Dictionary<TokenType, int>> Relationships
    List<Obligation> ActiveObligations
    List<Information> KnownInformation
}
```

### 2. Time System

#### Time Model
```csharp
class TimeSystem {
    int CurrentHour // 6 AM to 10 PM (16 active hours)
    int CurrentMinute // 0-59
    
    void AdvanceTime(int minutes)
    void ProcessHourlyEvents() // NPC movements, deadline checks
    bool IsNPCAvailable(NPC npc)
    int GetTimeUntilDeadline(Letter letter)
}
```

#### Actions and Time Costs
- Travel between adjacent locations: 30 minutes
- Travel across town: 60 minutes
- Quick conversation: 15 minutes
- Standard conversation: 30 minutes
- Deep conversation: 60 minutes
- Deliver letter: 5 minutes
- Rest: 60 minutes

### 3. Token System

#### Token Types
```csharp
enum TokenType {
    Trust,    // Personal bonds
    Commerce, // Professional reliability  
    Status,   // Social standing
    Shadow    // Kept secrets 
}
```

#### Token Rules
- Start with 0-3 tokens per NPC based on background
- Can go negative (creates debt)
- Debt has consequences (forced priorities, locked options)
- Maximum ±10 per relationship

#### Token Exchanges
- Deliver on time: +1 appropriate token
- Deliver late: -1 appropriate token
- Break promise: -3 appropriate token
- Complete obligation: +3 appropriate token

### 4. Conversation System

#### Core Verbs
```csharp
enum ConversationVerb {
    HELP,      // Accept letters, offer assistance
    NEGOTIATE, // Trade positions, time, resources
    INVESTIGATE // Learn information, discover options
}
```

#### Conversation Structure
```csharp
class Conversation {
    NPC Participant
    int AttentionPoints // Start with 3
    
    List<ConversationChoice> GenerateChoices() {
        // Max 5 choices based on:
        // - Current verb availability
        // - NPC emotional state
        // - Player tokens with NPC
        // - Queue pressure
        // - Known information
    }
    
    void ExecuteChoice(ConversationChoice choice) {
        // Apply ALL effects atomically:
        // - Queue changes
        // - Token changes
        // - Information reveals
        // - Obligation creation
        // - Time advancement
    }
}
```

#### Choice Generation Rules
1. Always include 1 free "exit" option
2. Show 1-2 contextual HELP options
3. Show 1-2 NEGOTIATE options if applicable
4. Show 1 INVESTIGATE option if attention available
5. Never exceed 5 total choices

#### Attention Economy
- Start each conversation with 3 attention points
- Costs:
  - Exit/basic response: 0 points
  - Standard actions: 1 point
  - Complex negotiations: 2 points
  - Deep investigation: 3 points
- Conversation ends when attention depleted

### 5. NPC Emotional States

#### State Calculation
```csharp
enum EmotionalState {
    DESPERATE,   // Has urgent letter OR deadline < 2 hours
    ANXIOUS,     // Has important letter OR deadline < 6 hours
    CALCULATING, // Has letter but no urgency
    NEUTRAL      // No letters in queue
}

EmotionalState CalculateState(NPC npc, LetterQueue queue) {
    var theirLetters = queue.GetLettersFrom(npc);
    // Apply rules based on stakes and deadlines
}
```

#### State Effects
- DESPERATE: More willing to trade, reveals information freely
- ANXIOUS: Focused on their problem, limited options
- CALCULATING: Careful trades, guards information
- NEUTRAL: Open to various interactions

### 6. Location System

#### Locations (Minimum 5)
```csharp
class Location {
    string Id
    string Name
    List<NPC> PresentNPCs // Based on time
    List<Action> AvailableActions
    Dictionary<Location, int> TravelTimes // To other locations
}
```

Required Locations:
1. **Your Room** - Start location, rest, queue management
2. **Market Square** - Central hub, multiple NPCs
3. **Noble District** - Formal deliveries, Status-focused
4. **Merchant Quarter** - Commerce-focused, trade hub
5. **City Gates** - Edge location, travelers

**Note: Travel System Details**
See TRAVEL-SYSTEM-DESIGN.md for complete route mechanics, transport NPCs, and travel permits

#### Location Actions
- Rest (restore energy if implemented)
- Observe (learn who's present)
- Wait (advance time)
- Travel (go to other location)
- Converse (if NPC present)

### 7. Information System

#### Information Types
```csharp
class Information {
    string Id
    InfoType Type // ROUTE, NPC_SCHEDULE, REQUIREMENT, SECRET
    string Description
    List<string> Unlocks // What this enables
}
```

Information unlocks:
- New conversation options
- Shorter travel routes
- NPC schedules (know when they're available)
- Service availability
- Hidden requirements

### 8. Obligation System

#### Standing Obligations
```csharp
class Obligation {
    string Id
    string NPCId
    ObligationType Type
    string Rule // "Elena's letters always position 2"
    Func<GameState, bool> Condition
    Action<GameState> Effect
}
```

Obligation types:
- Queue priority (letters enter at specific position)
- Delivery guarantee (must deliver within X hours)
- Information sharing (must report certain discoveries)
- Token maintenance (keep relationship above threshold)

## UI Requirements

### 1. Main Game Screen

#### Always Visible
- Current time with day/night indicator
- Next deadline countdown (most urgent)
- Current location name
- Available actions (max 5)

#### Queue Display
```
Visual Queue:
[1][2][3][4][5][6][7][8]
[■■][■][■■■][_][_][_][_][_]
[!2h][6h][1d][_][_][_][_][_]

Where:
■ = Weight blocks
!2h = Time until deadline
```

#### Peripheral Awareness
- Small notifications at screen edge:
  - "Marcus entering Market Square"
  - "Shop closing in 1 hour"
  - "Elena growing anxious"

### 2. Conversation Screen

#### Layout
```
[NPC Name and State]
[Body language description]
[Current dialogue/narration]

[Choice 1] [Cost indicators]
[Choice 2] [Cost indicators]
[Choice 3] [Cost indicators]
[Max 5 choices]

[Attention remaining: ●●○]
```

#### Choice Display
Each choice shows:
- Narrative text (what you say/do)
- Attention cost (dots)
- Primary effect preview (icon/text)
- Token change indicator (+/- with type)

### 3. Travel Screen

#### Route Selection
```
Current: Market Square
Destinations:

[Noble District]
- 30 min walk
- Lord Aldwin present (until 6 PM)

[Merchant Quarter]  
- 15 min walk
- Marcus always there
- Shops close at 8 PM

[City Gates]
- 45 min walk
- Travelers in morning only
```

**Note: Advanced Travel Mechanics**
See TRAVEL-SYSTEM-DESIGN.md for:
- Route unlocking with permits
- Transport NPCs (boat captains, carriage drivers)
- Travel method selection
- Route progression system

### 4. Consequence Display

#### Delivery Success
```
✓ Letter delivered to Lord Aldwin
+ Status token gained
+ New information: "Noble court schedule"
```

#### Deadline Missed
```
✗ Elena's letter expired
- Trust tokens lost (-3)
- Elena now HOSTILE
- Future letters from Elena start at position 7
```

## State Management Requirements

### 1. Transaction System

All state changes must be atomic:
```csharp
class GameTransaction {
    List<IStateChange> changes
    GameState checkpoint
    
    void Begin(GameState current)
    void AddChange(IStateChange change)
    GameState Commit() // All or nothing
    void Rollback() // On any failure
}
```

### 2. Save System

Must persist:
- Complete queue state
- All token relationships
- Current time
- NPC locations and states
- Active obligations
- Known information
- Delivery history (for consequences)

### 3. Validation Rules

Before any state change:
- Verify queue has space
- Check token requirements
- Validate time constraints
- Ensure NPC availability
- Confirm obligation compatibility

## Consequence System

### 1. Immediate Consequences

On delivery:
- Token changes with recipient
- Token changes with sender (if late)
- Information reveals
- New letters generated
- Obligation updates

On deadline miss:
- Token penalties
- NPC state changes
- Queue position penalties for future letters
- Relationship locks (some options unavailable)
- Obligation failures

### 2. Cascading Consequences

First miss: Individual NPC reacts
Second miss: Their network reacts
Third miss: Systemic changes (areas locked, services unavailable)

### 3. Permanent Changes

Some consequences persist across days:
- Broken trust (permanent token ceiling)
- Failed obligations (locked content)
- Revealed secrets (changed NPC behavior)
- Established routes (permanent unlocks)

## Content Requirements

### 1. NPC Definitions (5 minimum)

Each NPC needs:
- Name and description
- Starting location
- Movement schedule
- Starting token values
- Letter generation rules
- Conversation templates
- Emotional state responses

### 2. Letter Templates

For each combination of Type × Stakes (9 total):
- Sender/recipient patterns
- Deadline ranges
- Weight values
- Content hints
- Consequence descriptions

### 3. Conversation Content

For each NPC × Verb × State (45 combinations):
- Base dialogue text
- Choice options
- Success/failure responses
- Token change ranges
- Information reveals

### 4. Location Descriptions

For each location × time period (20 total):
- Atmospheric description
- Available services
- NPC presence rules
- Special actions

## Technical Implementation Order

### Phase 1: Core Systems
1. Game state models
2. Time system
3. Queue mechanics
4. Basic UI framework

### Phase 2: Interaction
1. Conversation system
2. Choice generation
3. Token exchanges
4. NPC emotional states

### Phase 3: Content
1. 5 NPCs with full conversations
2. 5 locations with travel
3. Letter generation
4. Consequence system

### Phase 4: Polish
1. Save/load system
2. UI polish
3. Balance testing
4. Tutorial flow

## Success Criteria

The game is ready when:

1. **Player can complete a full day**
   - Wake up with 5 letters
   - Deliver at least 3
   - See consequences for failures
   - Experience token changes
   - End day and start next

2. **Core loop creates tension**
   - Time pressure is constant
   - Choices have visible trade-offs
   - Consequences feel meaningful
   - Different strategies are viable

3. **Systems interconnect properly**
   - Queue drives NPC states
   - Tokens affect conversations
   - Information unlocks options
   - Obligations modify rules

4. **UI communicates clearly**
   - Queue state always visible
   - Deadlines create urgency
   - Choices show costs
   - Consequences are transparent

## What We're NOT Building (Yet)

- Complex AI narrative generation
- Procedural content
- Multiple save slots
- Difficulty settings
- Voice acting or sound
- Animations beyond basic transitions
- Mouse/touch controls (keyboard only)
- Accessibility features
- Localization
- Achievements

## The Commitment

This document represents the COMPLETE implementation requirement. Every system listed here must be built. Every interaction must work as specified. No feature creep. No scope expansion. Build exactly this, test it, and only then consider additions.

The game succeeds or fails on the tension created by having more letters than time to deliver them. Everything else serves that core pressure.

## Final Checklist

Before considering the game "implemented":

- [ ] 5 NPCs with unique personalities
- [ ] 5 locations with travel times
- [ ] 8-slot queue with weight system
- [ ] 3 token types with debt
- [ ] 3 conversation verbs
- [ ] Time system (16 active hours)
- [ ] Deadline pressure visible
- [ ] Consequences for missed deliveries
- [ ] Save/load functionality
- [ ] Complete UI as specified
- [ ] 3-day playable sequence
- [ ] No crashes or soft-locks
- [ ] Clear win/loss conditions
- [ ] Tutorial for core mechanics

Build this. Test this. Ship this. Then iterate.