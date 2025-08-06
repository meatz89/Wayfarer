# Wayfarer Session Handoff - Parallel Degradation Design
## Session Date: 2025-01-06 (Major Design Pivot)


# Wayfarer: Complete Game Design Document

## Table of Contents
1. [Core Game Philosophy](#core-game-philosophy)
2. [Fundamental Mechanics](#fundamental-mechanics)
3. [The Journey: Design Evolution](#the-journey-design-evolution)
4. [Final System Architecture](#final-system-architecture)
5. [UI/UX Philosophy](#uiux-philosophy)
6. [Conversation System Integration](#conversation-system-integration)
7. [Implementation Framework](#implementation-framework)
8. [Production Reality](#production-reality)

## Core Game Philosophy

Wayfarer is a medieval letter carrier game about being trapped between impossible social obligations. It is NOT about:
- Becoming a master wayfarer
- Saving the world
- Life-changing events
- Magic or fantasy elements

It IS about:
- A few ordinary days in the life of a letter carrier
- Managing impossible delivery deadlines
- Navigating complex social relationships
- Finding moments of human connection amid pressure
- Making agonizing choices about who to disappoint

### Design Principles
1. **Intentional Design**: Every system element serves exactly one clear purpose
2. **Ludonarrative Verisimilitude**: Mechanics are thoughtful abstractions of realistic medieval actions
3. **Emergent Complexity**: Simple systems interact to create deep strategic decisions
4. **No Arbitrary Mechanics**: Everything emerges from logical relationships, not mathematical modifiers
5. **Deterministic Systems**: No dice rolls or RNG - player choices determine outcomes

## Fundamental Mechanics

### 1. The Queue System (Core Mechanic)

**The Iron Rule**: You can only deliver from position 1.

```
Queue Positions: [1] [2] [3] [4] [5] [6] [7] [8]
Delivery Rule: Only position 1 can be delivered
Entry Rule: New letters enter at lowest available position
Reorder Rule: Can burn tokens to skip letters
```

**Queue Pressure**: Each letter has:
- Type (Trust/Commerce/Status/Shadow)
- Sender & Recipient
- Deadline (creates time pressure)
- Stakes (REPUTATION/WEALTH/SAFETY/SECRET)

### 2. Token System (Relationship Currency)

Tokens represent HOW you relate to someone, not WHO they are:

**Trust** - Personal bonds, emotional connections
- Built through: Personal favors, shared experiences
- Enables: Deep conversations, personal revelations
- Debt creates: Complete relationship severance

**Commerce** - Professional reliability
- Built through: Successful deliveries, fair trades
- Enables: Better rates, exclusive contracts
- Debt creates: Financial leverage (forces queue priority)

**Status** - Social standing
- Built through: Proper conduct, noble service
- Enables: Court access, exclusive areas
- Debt creates: Social exclusion (area restrictions)

**Shadow** - Shared secrets, complicity
- Built through: Kept secrets, illegal tasks
- Enables: Underground access, criminal protection
- Debt creates: Coercive control (forced letters)

### 3. Time as Universal Resource

- Everything costs time (measured in hours/minutes)
- No abstract time units
- NPCs have schedules
- Routes have travel times
- Conversations take realistic durations

### 4. Information System

Information exists as discrete, tradeable units:
- Route knowledge (how to get places)
- NPC locations (where to find people)
- Service availability (what's offered where)
- Requirements (what's needed for access)

### 5. Standing Obligations

Permanent rule modifications activated by relationships:
- "Heart's Promise" (Trust 5+): Elena's letters always jump queue
- "Merchant's Favor" (Commerce debt): Marcus's letters enter at position 3
- "Noble's Courtesy" (Status 5+): Lord's letters pay double

## The Journey: Design Evolution

### Phase 1: Initial Concept
Started as simple queue management with tokens as progression gates. Problems:
- Tokens were just keys to unlock content
- No meaningful choices in conversations
- Information was binary (known/unknown)

### Phase 2: Attention as Resource
Introduced 3 attention points per scene to create focus/breadth trade-offs:
- 0 attention = surface interaction
- 1 attention = deeper engagement
- Focus on one thing means missing others

### Phase 3: Parallel Degradation
Moved from triggered cascades to simultaneous timelines:
- Everything happens whether you observe it or not
- Actions consume opportunities, don't trigger responses
- Time advances all states in parallel

### Phase 4: Mechanical Integration
Final breakthrough - conversations as the interface to ALL game systems:
- Every dialogue choice manipulates queue/tokens/obligations
- No separate "conversation game"
- Mechanical effects clearly shown but narratively framed

## Final System Architecture

### Core Verb System (9 Sacred Verbs)

1. **DELIVER** - Execute queue position 1
2. **ACCEPT** - Add letter to queue
3. **MOVE** - Change location
4. **REORDER** - Rearrange queue (burns tokens)
5. **OBSERVE** - Examine environment/person
6. **ENGAGE** - Interact with NPC
7. **TRADE** - Exchange resources
8. **CHALLENGE** - Apply pressure/leverage
9. **WAIT** - Let time pass

### Context Multiplication

Same verbs produce different outcomes based on:
- Token levels (relationship depth)
- Queue pressure (urgency)
- Resource availability
- Information held

Example:
```
ENGAGE + Trust 0 = Polite deflection
ENGAGE + Trust 5 = Deep personal revelation
ENGAGE + Trust 5 + High Pressure = Desperate plea for help
```

## UI/UX Philosophy

### Three Layers of Awareness

1. **Peripheral Awareness** (Always visible)
   - Next deadline as pressure
   - Queue weight as physical sensation
   - Environmental hints at screen edge

2. **Sharp Focus** (When attending)
   - Character dominates screen
   - Relationship indicators visible
   - Choices emerge from context

3. **Deep Investigation** (Spending attention)
   - Hidden information revealed
   - Complex options unlocked
   - True character state exposed

### Key UI Principles

- **No permanent HUD** - Information appears when relevant
- **Physical actions** - "Reach for satchel" not "Open inventory"
- **Narrative framing** - "Letters press against your hip" not "Queue: 3/8"
- **Consequence preview** - See what choices cost before selecting
- **Attention as focus** - Spending attention literally changes what you see

### Example Scene: Elena at Tavern

**State Analysis:**
- Elena has Trust letter with REPUTATION stakes
- Deadline tomorrow (urgent but not critical)
- Player has Trust 5 with Elena
- Lord Blackwood's letter in position 1 (due in 2 hours)

**Generated Choices:**

```
0 Attention Options:
1. "I'll deliver it right after Lord Blackwood's"
   - Keeps queue order
   - No immediate cost
   - Elena remains anxious

2. "I can rearrange my deliveries"
   - Move her letter to position 1
   - Burns 1 Status with Lord Blackwood
   - 30 minutes to reorder
   - Elena gains hope

1 Attention Options:
3. "Tell me about Lord Aldwin"
   - Learn: "Controls harbor tariffs"
   - Learn: "Marriage was mother's arrangement"
   - Opens new dialogue branch

4. "I know people who could help"
   - Unlock: Introduction to Cousin Garrett
   - Creates: Future obligation
   - Elena shares noble route info
```

### Letter Stakes Drive NPC States

```
NPC Emotional State = Letter Type + Stakes + Deadline + Relationship

Examples:
Trust + REPUTATION + Urgent = Anxious, confiding
Commerce + WEALTH + Critical = Desperate, offering anything
Status + SECRET + Soon = Evasive, paranoid
Shadow + SAFETY + Overdue = Threatening, dangerous
```

## Production Reality

### Scope Definition
- 5 core NPCs with full mechanical integration
- 10-15 locations with distinct actions
- 16 narrative templates (4 letter types × 4 stakes)
- 9 verbs with contextual variations

## The Beautiful Truth

Wayfarer succeeds because:
1. **The queue IS the game** - Everything serves queue pressure
2. **Conversations ARE mechanical** - No separate dialogue system
3. **Attention creates focus** - Can't see/do everything
4. **Transparency builds trust** - Players see consequences
5. **Simple rules, complex outcomes** - 9 verbs × contexts = endless variety

The game is about ordinary people navigating impossible social obligations through the simple act of delivering letters. Every mechanical system reinforces this core fantasy without abstraction or arbitrary gamification.


## 🔥 CRITICAL DESIGN PIVOT: Parallel Degradation with Visible Mechanics

After extensive iteration and production analysis, we've pivoted to a **parallel degradation system** where multiple timelines decay simultaneously with complete transparency. Time is the only resource, and all mechanics are visible.

### 🎯 NEW DESIGN VISION

The refined approach:
1. **Everything degrades visibly** - Letter deadlines, NPC patience, route conditions, stamina, weather
2. **Time as the only resource** - Every action costs 15/30/60/90 minutes
3. **No AI generation needed** - Fixed timelines with deterministic outcomes
4. **218 hours to build** - Down from 300+ hours with previous designs
5. **Complete transparency** - Players see all countdowns and consequences upfront

### Current Implementation Status

#### ✅ What's Working
- **GameFacade** updated with CreateConversationViewModel populating literary properties
- **UI Components** created (AttentionDisplay, PeripheralAwareness, etc.)
- **ContextTagCalculator** and **RumorManager** integrated
- Build succeeds with only warnings

#### ❌ Previous Compilation Errors (NOW FIXED)
- ContextTagCalculator methods updated to use correct APIs
- ConversationChoice properties mapped correctly
- GameFacade dependencies added (ContextTagCalculator, RumorManager)

---

## 🕐 PARALLEL DEGRADATION MECHANICS

### The 5 Core Timelines (All Visible)

#### 1. Letter Deadlines
```
Visual: "Noble's Letter: 2 days remaining"
Degradation: -1 day per day
Consequence: "Failed: Status -3 permanent"
```

#### 2. NPC Patience
```
Visual: "Elena will wait: 3 more hours"
Degradation: Real-time countdown
Consequence: "Elena left: Commerce route closed"
```

#### 3. Route Conditions
```
Visual: "Forest path floods at: Dusk"
Degradation: Time/weather based
Consequence: "Route impassable for 2 days"
```

#### 4. Personal Stamina
```
Visual: "Energy for: 4 hours travel"
Degradation: -1 per hour traveled
Consequence: "Exhausted: Actions +50% time"
```

#### 5. Environmental Windows
```
Visual: "Market closes in: 2 hours"
Degradation: Fixed schedule
Consequence: "Missed today's trading"
```

### Time System (The Only Resource)
```
1 Day = 12 active hours
Actions cost: 15, 30, 60, or 90 minutes
No abstract currencies
No hidden mechanics
```

---

## 📐 CRITICAL DESIGN PRINCIPLES

### Mechanics Generate Narrative (Not Vice Versa)
- **NO** hardcoded story content
- **NO** unique NPC dialogue per situation
- **YES** systematic generation from mechanical state
- **YES** template combinations based on mechanics

### Everything Is Deterministic
- Same mechanical state → same narrative output
- Player can learn and predict systems
- Randomness only in dice/cards, not content selection

### Board Game Feel
- Clear resource management (attention, stamina, tokens)
- Visible state tracking (pressure, momentum, activity)
- Strategic decisions with trade-offs
- No hidden narrative branches

---

## 🛠️ IMPLEMENTATION APPROACH

### Phase 1: Core Mechanical Systems
1. **PressureDifferentialSystem** class for conversations
2. **ActivityTokenSystem** class for locations
3. **SegmentedTravelSystem** class for journeys

### Phase 2: Template Generators
1. **BodyLanguageGenerator** from pressure levels
2. **AtmosphereGenerator** from activity tokens
3. **TravelNarrativeGenerator** from segment state

### Phase 3: UI Integration
1. Map mechanical values to UI displays
2. Show mechanics transparently (pressure bars, token counts)
3. Keep literary descriptions as OUTPUT of mechanics

---

## 📊 PRODUCTION REQUIREMENTS (218 Hours Total)

### Content Creation (48 hours)
- **150 timeline events** (5 NPCs × 10 beats × 3 texts = 25 hours)
- **100 consequences** (mechanical only = 8 hours)
- **20 countdown templates** (UI strings = 2 hours)
- **40 location states** (10 locations × 4 times = 10 hours)
- **20 NPC reactions** (5 NPCs × 4 states = 3 hours)

### System Implementation (170 hours)
- **Time system & UI** (70 hours)
- **Timeline tracking** (40 hours)
- **Consequence engine** (20 hours)
- **Testing & polish** (40 hours)

### 3-Hour Demo Scope
- 2 NPCs, 3 locations, 3 days
- 6 timeline events total
- 2 hours content creation
- Immediately playable

---

## ⚠️ ELIMINATED COMPLEXITY

- ❌ NO hidden token systems
- ❌ NO attention economy
- ❌ NO verb selection
- ❌ NO branching dialogue
- ❌ NO AI generation
- ❌ NO complex relationships
- ❌ NO abstract currencies

## ✅ WHAT WE ARE BUILDING

- ✅ Visible countdown timers for everything
- ✅ Time as the only resource to manage
- ✅ Fixed NPC timelines with clear consequences
- ✅ Deterministic outcomes (no surprises)
- ✅ 218 hours to complete (vs 1000+ for branching)

---

## 🎯 Next Implementation Steps

1. **Build Time Management System**
   - Create time block tracker (12 hours/day)
   - Implement action cost calculator
   - Build countdown timer UI

2. **Implement Timeline System**
   - Create NPC timeline definitions
   - Build event trigger system
   - Implement consequence application

3. **Create Parallel Degradation UI**
   - Visual countdown displays
   - Timeline visualization
   - Clear consequence warnings

4. **Write Minimal Content**
   - 150 timeline events
   - 20 countdown templates
   - 40 location states

---

## 📋 GitHub Kanban Board Status
Check the project board: https://github.com/users/meatz89/projects/2

**User Stories #27-36 Status:**
- #27 ✅ Attention system - Backend complete, UI partial
- #28 🚧 Partial information - Rumor backend complete, UI needed
- #29 ❌ Physical queue - Not started
- #30 ✅ Rumor system - Backend complete
- #31 🚧 Binding obligations - High attention costs implemented
- #32 ✅ Peripheral awareness - Component created
- #33 ✅ Feeling tags - Backend complete
- #34 ✅ Body language - Component created
- #35 ✅ Internal thoughts - Component created
- #36 🚧 Narrative costs - Partially implemented

#### 🚨 NEXT IMMEDIATE TASKS

1. **Fix ContextTagCalculator.cs** - Use correct methods from managers
2. **Fix LiteraryConversationScreen.cs** - Use correct GameFacade method
3. **Create/verify GameFacade interface** - Fix architecture violation
4. **Update ConversationViewModel** - Add literary UI properties
5. **Run build and E2E test** - Verify everything works

---

## 🏗️ Architecture Reminders

### CRITICAL: GameFacade Pattern
- **UI components MUST only use GameFacade** - Never inject services directly
- **GameWorld has NO dependencies** - Single source of truth
- **NO @code blocks in .razor files** - Use code-behind (.razor.cs)
- **Delete legacy code entirely** - No compatibility layers

### SceneContext Integration
The conversation system now uses `SceneContext` (not ConversationContext):
- Contains AttentionManager instance
- Populated with context tags by ContextTagCalculator
- Passed to all narrative generation methods

---

## 📂 Key Files Created/Modified

### New Files (This Session - 2025-01-27)
- `/src/GameState/Letter.cs` - Added StakeType enum and Stakes property
- `/src/GameState/NPCEmotionalStateCalculator.cs` - Calculates NPC states from letter queue
- `/src/Game/ConversationSystem/VerbContextualizer.cs` - Hidden verb system for choices
- `/src/GameState/LiteraryUIConfiguration.cs` - Service registration for literary UI
- `/src/GameState/LetterQueue.cs` - Added Letters property and helper methods

### Modified Files (This Session)
- `/src/ServiceConfiguration.cs` - Added AddLiteraryUISystem()
- `/src/GameState/AttentionManager.cs` - Already implemented correctly
- `/src/Pages/Components/PeripheralAwareness.razor(.cs)` - Enhanced for queue pressure
- `/src/Pages/Components/BodyLanguageDisplay.razor(.cs)` - Added NPCEmotionalState support
- `/src/Pages/Components/AttentionDisplay.razor(.cs)` - Already exists and works
- `/src/Game/ConversationSystem/ConversationManager.cs` - Added BaseVerb property

### Previously Created Files
- `/src/GameState/AttentionManager.cs` - Attention point system
- `/src/GameState/SceneTags.cs` - All tag enums
- `/src/GameState/ContextTagCalculator.cs` - Tag generation
- `/src/GameState/Rumor.cs` - Rumor data model
- `/src/GameState/RumorManager.cs` - Rumor tracking
- `/src/Game/AiNarrativeSystem/AttentionCost.cs` - Renamed from FocusCost
- `/LITERARY-UI-IMPLEMENTATION.md` - Complete documentation

---

## 🎨 Phase 2: UI Components to Build

### LiteraryConversationScreen Components

1. **LiteraryConversationScreen.razor + .razor.cs**
   - Replace ConversationView entirely
   - Inject only GameFacade
   - Get ConversationViewModel from facade
   - Display narrative text without streaming effect

2. **AttentionDisplay.razor + .razor.cs**
   ```csharp
   @inject GameFacade GameFacade
   
   // In code-behind:
   private int CurrentAttention => ConversationVM?.AttentionRemaining ?? 3;
   ```

3. **PeripheralAwareness.razor + .razor.cs**
   - Show deadline pressure from SceneContext.MinutesUntilDeadline
   - Display binding obligations if OBLIGATION_ACTIVE tag present
   - Environmental hints based on FeelingTags

4. **InternalThoughtChoice.razor + .razor.cs**
   - Choices as italicized text
   - Show AttentionCost as symbols (◆)
   - Disable if not affordable

5. **BodyLanguageDisplay.razor + .razor.cs**
   - Convert RelationshipTags to descriptions
   - TRUST_HIGH → "Deep trust flows between you"
   - No numeric displays

---

## 🔧 Implementation Pattern

### Example Component Structure

```csharp
// LiteraryConversationScreen.razor
@page "/conversation"
@inherits LiteraryConversationScreenBase

<div class="literary-conversation">
    <AttentionDisplay />
    <PeripheralAwareness Context="@SceneContext" />
    
    <div class="narrative-content">
        @CurrentNarrative
    </div>
    
    <div class="choices">
        @foreach(var choice in Choices)
        {
            <InternalThoughtChoice Choice="@choice" OnSelected="@HandleChoice" />
        }
    </div>
</div>

// LiteraryConversationScreen.razor.cs
public partial class LiteraryConversationScreenBase : ComponentBase
{
    [Inject] private GameFacade GameFacade { get; set; }
    
    private ConversationViewModel ConversationVM { get; set; }
    private SceneContext SceneContext { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        ConversationVM = await GameFacade.GetCurrentConversation();
        // SceneContext is part of ConversationVM
    }
}
```

---

## ⚠️ Common Pitfalls to Avoid

1. **DON'T inject services directly** - Only use GameFacade
2. **DON'T use @code blocks** - Always use code-behind
3. **DON'T show numbers** - Everything must be narrative
4. **DON'T keep FocusCost** - It's been completely removed
5. **DON'T create RenderFragments** - Use proper components

---

## 📝 Testing Checklist

Before marking any component complete:
- [ ] Uses only GameFacade
- [ ] Has proper code-behind file
- [ ] No numeric displays
- [ ] Integrates with SceneContext
- [ ] Respects attention costs
- [ ] Follows mockup design

---

## 🚀 Quick Commands

```bash
# Build the project
cd /mnt/c/git/wayfarer/src
dotnet build

# Run the game
dotnet run
# Navigate to http://localhost:5011

# Check GitHub issues
gh issue list --repo meatz89/Wayfarer --state open

# Update issue progress
gh issue comment [number] --repo meatz89/Wayfarer --body "Progress update"
```

---

## 📖 Reference Documents

- **PARALLEL-DEGRADATION-DESIGN.md** - NEW complete design specification
- **LITERARY-MECHANICS-DESIGN.md** - Previous iteration (for reference)
- **BOARD-GAME-MECHANICS-DESIGN.md** - Board game mechanics approach
- **CLAUDE.md** - Core architectural principles

---

## 🎓 Key Learnings from This Session

### Architecture Discoveries
1. **ConnectionTokenManager** exists (not TokenManager) - manages all token relationships
2. **Player uses CurrentLocationSpot** not CurrentLocationId - access location via spot
3. **GameFacade is the single UI entry point** - Never expose GameWorld directly
4. **LetterQueue** is an array-based system with 8 fixed slots
5. **AttentionManager** already perfectly implemented with narrative descriptions

### Implementation Patterns
1. **NO partial classes** - User explicitly forbids them
2. **NO extension classes** - Add properties directly to existing classes
3. **Services must be registered** - Use configuration extensions like AddLiteraryUISystem()
4. **LetterQueueManager.GetActiveLetters()** - Added to get non-null accepted letters
5. **UI components use GameFacade.GetPlayer()** - Not GetGameWorld()

### Letter-Driven Narrative System
1. **StakeType enum** defines 4 stakes: REPUTATION, WEALTH, SAFETY, SECRET
2. **16 narrative contexts** from TokenType × StakeType combinations  
3. **NPCEmotionalState** emerges from letter properties (DESPERATE, HOSTILE, CALCULATING, WITHDRAWN)
4. **4 hidden verbs** (PLACATE, EXTRACT, DEFLECT, COMMIT) never shown to players
5. **VerbContextualizer** transforms verbs into contextual narrative choices

### Compilation Issues Resolved
1. Fixed duplicate GetAllLetters() methods in LetterQueue
2. Added BaseVerb property directly to ConversationChoice (no partial)
3. Updated VerbContextualizer to use ConnectionTokenManager
4. Fixed Player.CurrentLocationSpot reference (not CurrentLocationId)
5. PeripheralAwareness now uses GameFacade.GetPlayer() correctly

## Next Session Focus

1. **Fix remaining build errors** - InformationHierarchy and other UI issues
2. **Complete E2E testing** - Verify the literary UI system works end-to-end
3. **Implement AI narrative generation** - Connect VerbContextualizer to AI system
4. **Build LiteraryConversationScreen** - The main UI component for literary conversations
5. **Test with actual gameplay** - Ensure the queue-driven narrative feels right

The backend for literary UI is now complete. Focus next on fixing compilation errors and building the frontend components.

