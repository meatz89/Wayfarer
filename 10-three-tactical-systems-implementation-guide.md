# THREE TACTICAL SYSTEMS - COMPLETE IMPLEMENTATION GUIDE

## EXECUTIVE SUMMARY

**Current State**: Investigation implemented as FOURTH tactical system (WRONG)
**Target State**: THREE tactical systems (Social/Mental/Physical), Investigation orchestrates them

**Core Error**: Investigation was built as a card-based tactical system parallel to Conversations. The architecture specifies only THREE tactical systems. Investigation should be a strategic activity that spawns Mental/Physical/Social tactical sessions across multiple phases.

---

## ARCHITECTURAL FOUNDATION

### The Three Tactical Systems

**Social System (Conversations)** - Already implemented ✓
- Resources: Initiative (action economy), Momentum (progress), Doubt (consequence), Cadence (rhythm), Understanding (sophistication)
- Actions: LISTEN (draw), SPEAK (play)
- Deck Owner: NPC owns conversation deck
- Session: ConversationSession
- Facade: ConversationFacade
- UI: ConversationContent.razor
- Status: COMPLETE, used as template pattern

**Mental System (Investigation Mechanics)** - Needs to be built
- Resources: Attention (action economy), Progress (toward goal), Exposure (consequence), ObserveActBalance (rhythm), Understanding (sophistication)
- Actions: OBSERVE (draw), ACT (play)
- Deck Owner: Location owns investigation deck
- Session: MentalSession (rename from InvestigationSession)
- Facade: MentalFacade (create new)
- UI: MentalContent.razor (create new)
- Status: WRONG IMPLEMENTATION EXISTS (InvestigationFacade) - DELETE and rebuild

**Physical System (Athletic/Combat Challenges)** - Needs to be built
- Resources: Position (action economy), Progress (toward goal), Danger (consequence), AssessExecuteBalance (rhythm), Understanding (sophistication)
- Actions: ASSESS (draw), EXECUTE (play)
- Deck Owner: Challenge owns physical deck
- Session: PhysicalSession (create new)
- Facade: PhysicalFacade (create new)
- UI: PhysicalContent.razor (create new)
- Status: DOES NOT EXIST

**Investigation (Strategic Activity)** - Needs correct implementation
- NOT a fourth tactical system
- Orchestrates Mental/Physical/Social phases
- Multi-phase progression with rewards
- Spawns appropriate tactical sessions based on phase type
- Class: InvestigationActivity (create new, NOT a facade)
- Status: WRONG IMPLEMENTATION (InvestigationFacade as tactical) - DELETE and rebuild as strategic orchestrator

### Universal Card Pattern

ALL cards across all three systems share base properties:
- `Id`, `Title` - Core identity
- `CardType` - System type (Conversation, Mental, Physical)
- `Depth` - Tier level (1-8, gates by Understanding thresholds)
- `BoundStat` - Which player stat (Insight/Rapport/Authority/Diplomacy/Cunning)
- `Persistence` - Statement vs Echo (counts toward progression or repeatable)
- `EffectFormula` - Categorical effects system

System-specific properties:
- **Social**: InitiativeCost, Delivery (Cadence effect), ConversationalMove
- **Mental**: AttentionCost, Method (Balance effect), ClueType
- **Physical**: PositionCost, Exertion (Balance effect), Approach

### Parallel Facade Pattern

All three tactical systems have identical facade structure:
```csharp
public class [System]Facade
{
    // Start tactical challenge
    public async Task<[System]Session> Start[System]Async(deckOwnerId, player)

    // Draw action (Listen/Observe/Assess)
    public async Task<DrawResult> ExecuteDrawAsync(player)

    // Play action (Speak/Act/Execute)
    public async Task<PlayResult> ExecutePlayAsync(cardInstance, player)
}
```

---

## CODEBASE INVENTORY

### Files to DELETE COMPLETELY (Scorched Earth)

```
src/Subsystems/Investigation/InvestigationFacade.cs (516 lines, 73 comments, 8 TODOs)
src/GameState/InvestigationCard.cs (123 lines)
src/GameState/InvestigationCardInstance.cs (14 lines)
src/Content/DTOs/InvestigationCardDTO.cs (if exists)
```

**Reason**: Wrong implementation of Investigation as fourth tactical system. Must be completely removed.

**Validation**: After deletion, grep must return ZERO results for:
- `grep -r "InvestigationFacade" src/`
- `grep -r "InvestigationCard[^T]" src/` (allow InvestigationCardType temporarily)
- `grep -r "InvestigationCardInstance" src/`

### Files to RENAME and REFACTOR

```
src/GameState/InvestigationSession.cs → src/GameState/MentalSession.cs
src/Content/Parsers/InvestigationContentParser.cs → src/Content/Parsers/MentalCardParser.cs
src/Content/Core/10_investigation_cards.json → src/Content/Core/mental_cards.json
```

### Files to KEEP and EXTEND

```
src/GameState/InvestigationTemplate.cs - Add TacticalSystemType to phases
src/GameState/InvestigationPhase.cs - Add phase orchestration properties
src/Content/Core/11_investigations.json - Restructure for multi-tactical phases
```

### Existing REUSABLE Components (Do Not Recreate)

```
src/GameState/SessionCardDeck.cs - Card deck management (REUSE)
src/wwwroot/css/card-system-shared.css - Universal card styles (VALIDATE & EXTEND)
src/Pages/Components/GameTooltip.razor - Generic tooltip (REUSE)
src/Pages/Components/ConversationContent.razor + .cs - Template pattern (COPY)
src/Subsystems/Conversation/ConversationFacade.cs - Facade pattern (COPY)
```

---

## PHASE 0: SCORCHED EARTH + FOUNDATION

### Step 0.1: Delete Wrong Implementation

**Execute deletions:**
```bash
cd src
rm Subsystems/Investigation/InvestigationFacade.cs
rm GameState/InvestigationCard.cs
rm GameState/InvestigationCardInstance.cs
rm Content/DTOs/InvestigationCardDTO.cs  # if exists
```

**Update ServiceConfiguration.cs:**

Find and DELETE this line:
```csharp
services.AddSingleton<Wayfarer.Subsystems.Investigation.InvestigationFacade>();
```

**Build and fix compilation errors:**

The build will break. Fix ALL errors by:
1. Finding references to deleted classes
2. Commenting out the broken code temporarily (you'll rebuild it correctly)
3. Do NOT try to fix with new implementations yet

**Validate complete deletion:**
```bash
grep -r "InvestigationFacade" src/
grep -r "InvestigationCard[^T]" src/
grep -r "InvestigationCardInstance" src/
```

All three must return ZERO results.

### Step 0.2: Validate card-system-shared.css Completeness

**Location**: `src/wwwroot/css/card-system-shared.css`

**Check for these critical sections:**

1. **Context Header** (lines ~1-42): `.context-header`, `.context-name`, `.context-status`, `.context-rule`
2. **Core Resources** (lines ~44-97): `.resources-display`, `.resource-item`, `.resource-bar`, `.resource-fill`
3. **Rhythm Tracker** (lines ~99-147): `.rhythm-container`, `.rhythm-scale`, `.rhythm-segment`
4. **Action Economy** (lines ~149-197): `.action-economy-display`, `.action-economy-dots`
5. **Goals/Progress** (lines ~199-250): `.goals-section`, `.goal-card`
6. **Narrative** (lines ~252-268): `.narrative-section`, `.narrative-text`
7. **Actions** (lines ~270-313): `.primary-actions`, `.action-button`
8. **Pile Counters** (lines ~315-334): `.pile-display`, `.pile-count`
9. **Card Grid** (lines ~336-377): `.card-grid`, `.card`, `.card.selected`, `.card.disabled`
10. **Stat Colors** (lines ~379-429): `.card.insight`, `.card.rapport`, etc.
11. **Card Layout** (lines ~431-513): `.card-content`, `.card-header`, `.card-name`, `.card-cost-group`
12. **Tooltips** (lines ~515-541): `.tooltip`

**If any section missing**: Extract from `ConversationContent.razor.css` or `conversation.css` into shared file.

### Step 0.3: Extract Shared Blazor Components

**DO NOT create from scratch. Extract from existing ConversationContent.razor.**

#### Component 1: TacticalCard.razor

**Location**: Create `src/Pages/Components/Shared/TacticalCard.razor`

**Extract from ConversationContent.razor lines ~740-800** (card rendering section in GetAllDisplayCards loop)

**Structure**:
```razor
@namespace Wayfarer.Pages.Components.Shared

<div class="card @GetCardClasses()" @onclick="OnCardClick">
    <div class="card-content">
        <div class="card-header">
            <div class="card-name">@CardName</div>
            <div class="card-cost-group">
                <span class="card-cost-label">@CostLabel</span>
                <div class="card-cost-value">@CostValue</div>
            </div>
        </div>

        <div class="card-top-row">
            <div class="card-flags">
                <span class="stat-badge @StatBadgeClass">@StatName</span>
                @SystemSpecificBadges
            </div>
        </div>

        <div class="card-text">@CardText</div>
        <div class="card-effect">@EffectText</div>
    </div>
</div>

@code {
    [Parameter] public CardInstance Card { get; set; }
    [Parameter] public bool IsSelected { get; set; }
    [Parameter] public EventCallback OnCardClick { get; set; }
    [Parameter] public RenderFragment SystemSpecificBadges { get; set; }

    // Properties extracted from Card parameter
    private string CardName => Card?.ConversationCardTemplate?.Title ?? "";
    private string CardText => Card?.ConversationCardTemplate?.DialogueText ?? "";
    // ... etc (copy from ConversationContent methods)
}
```

#### Component 2: ResourceBar.razor

**Location**: Create `src/Pages/Components/Shared/ResourceBar.razor`

**Extract from ConversationContent.razor resource display sections**

**Structure**:
```razor
@namespace Wayfarer.Pages.Components.Shared

<div class="resource-item">
    <div class="resource-label">@Label</div>
    <div class="resource-value">@CurrentValue / @MaxValue</div>
    <div class="resource-bar">
        <div class="resource-fill @BarTypeClass" style="width: @PercentageWidth%"></div>
    </div>
</div>

@code {
    [Parameter] public string Label { get; set; }
    [Parameter] public int CurrentValue { get; set; }
    [Parameter] public int MaxValue { get; set; }
    [Parameter] public string BarType { get; set; } = "normal"; // normal, progress, consequence

    private string BarTypeClass => BarType == "progress" ? "progress" : BarType == "consequence" ? "consequence" : "";
    private double PercentageWidth => MaxValue > 0 ? (CurrentValue / (double)MaxValue) * 100 : 0;
}
```

#### Component 3: RhythmScale.razor

**Location**: Create `src/Pages/Components/Shared/RhythmScale.razor`

**Extract from ConversationContent.razor Cadence display section**

**Structure**:
```razor
@namespace Wayfarer.Pages.Components.Shared

<div class="rhythm-container">
    <div class="rhythm-label">@LeftLabel</div>
    <div class="rhythm-scale">
        @for (int i = MinValue; i <= MaxValue; i++)
        {
            <div class="rhythm-segment @GetSegmentClass(i) @(i == CurrentValue ? "current" : "")"></div>
        }
    </div>
    <div class="rhythm-label">@RightLabel</div>
</div>

@code {
    [Parameter] public string LeftLabel { get; set; }
    [Parameter] public string RightLabel { get; set; }
    [Parameter] public int CurrentValue { get; set; }
    [Parameter] public int MinValue { get; set; } = -5;
    [Parameter] public int MaxValue { get; set; } = 5;

    private string GetSegmentClass(int value)
    {
        if (value < 0) return "negative";
        if (value == 0) return "neutral";
        return "positive";
    }
}
```

#### Component 4: ActionEconomyDisplay.razor

**Location**: Create `src/Pages/Components/Shared/ActionEconomyDisplay.razor`

**Extract from ConversationContent.razor Initiative display section**

**Structure**:
```razor
@namespace Wayfarer.Pages.Components.Shared

<div class="action-economy-display">
    <div class="action-economy-counter">
        <div class="action-economy-label">@Label</div>
        <div class="action-economy-value">@CurrentValue</div>
        <div class="action-economy-dots">
            @for (int i = 0; i < MaxValue; i++)
            {
                <div class="action-economy-dot @(i < CurrentValue ? "available" : "")"></div>
            }
        </div>
    </div>
</div>

@code {
    [Parameter] public string Label { get; set; }
    [Parameter] public int CurrentValue { get; set; }
    [Parameter] public int MaxValue { get; set; }
}
```

### Step 0.4: Refactor ConversationContent to Use Shared Components

**File**: `src/Pages/Components/ConversationContent.razor`

**Replace inline card rendering** (around line ~740):
```razor
<!-- OLD: Inline card HTML -->
@foreach (var cardInfo in GetAllDisplayCards())
{
    <div class="card...">
        <!-- 50+ lines of inline card HTML -->
    </div>
}

<!-- NEW: Use TacticalCard component -->
@foreach (var cardInfo in GetAllDisplayCards())
{
    <TacticalCard Card="@cardInfo.Card"
                  IsSelected="@(SelectedCard?.InstanceId == cardInfo.Card.InstanceId)"
                  OnCardClick="@(() => ToggleCardSelection(cardInfo.Card))">
        <SystemSpecificBadges>
            <span class="card-delivery @GetCardDeliveryClass(cardInfo.Card)">@GetCardDelivery(cardInfo.Card)</span>
            <span class="card-move @GetCardMoveClass(cardInfo.Card)">@GetCardMove(cardInfo.Card)</span>
        </SystemSpecificBadges>
    </TacticalCard>
}
```

**Replace resource bars**:
```razor
<!-- OLD: Inline resource HTML -->
<div class="resource-item">
    <div class="resource-label">MOMENTUM</div>
    <div class="resource-value">@GetCurrentMomentum()</div>
    <div class="resource-bar">...</div>
</div>

<!-- NEW: Use ResourceBar component -->
<ResourceBar Label="MOMENTUM"
             CurrentValue="@GetCurrentMomentum()"
             MaxValue="16"
             BarType="progress" />
```

**Validation**: Build and run conversation system. Must work EXACTLY as before.

---

## PHASE 1: MENTAL TACTICAL SYSTEM

### Step 1.1: Rename InvestigationSession → MentalSession

**File**: `src/GameState/InvestigationSession.cs`

**Rename file**: `InvestigationSession.cs` → `MentalSession.cs`

**Change class name**: `InvestigationSession` → `MentalSession`

**ADD new properties**:
```csharp
public class MentalSession
{
    // Existing properties (keep these)
    public string SessionId { get; init; } = Guid.NewGuid().ToString();
    public string InvestigationId { get; set; }  // Which investigation template
    public int CurrentPhaseIndex { get; set; } = 0;
    public int CurrentProgress { get; set; } = 0;
    public int CurrentExposure { get; set; } = 0;
    public int ObserveActBalance { get; set; } = 0;

    // ADD these new properties
    public int CurrentAttention { get; set; } = 0;  // Action economy (0-10)
    public int MaxAttention { get; set; } = 10;
    public int CurrentUnderstanding { get; set; } = 0;  // Sophistication (unlocks tiers)
    public HashSet<int> UnlockedTiers { get; set; } = new HashSet<int> { 1 };  // Tier 1 always unlocked
    public Dictionary<ClueType, int> ClueTypeCounts { get; set; } = new Dictionary<ClueType, int>();

    // CHANGE these to use CardInstance instead of InvestigationCardInstance
    public SessionCardDeck Deck { get; set; }  // Reuse existing SessionCardDeck class

    // ADD tier unlock methods (copy from ConversationSession.cs lines 206-229)
    public int GetUnlockedMaxDepth() => UnlockedTiers.Max() * 2;
    public void CheckAndUnlockTiers() { /* Copy from ConversationSession */ }

    // ADD clue tracking methods (parallel to Statement tracking)
    public int GetClueCount(ClueType clueType) => ClueTypeCounts.TryGetValue(clueType, out int count) ? count : 0;
    public void IncrementClueCount(ClueType clueType) { /* ... */ }
}

// ADD ClueType enum
public enum ClueType
{
    Physical,      // Tangible evidence, objects, traces
    Testimonial,   // Witness statements, interviews
    Deductive,     // Logical connections, reasoning
    Intuitive      // Hunches, feelings, insights
}
```

### Step 1.2: Rename InvestigationCard → MentalCard

**File**: `src/GameState/InvestigationCard.cs`

**Rename file**: `InvestigationCard.cs` → `MentalCard.cs`

**Complete refactor**:
```csharp
using System.Collections.Generic;

public class MentalCard
{
    // Core identity (same)
    public string Id { get; init; }
    public string Name { get; init; }
    public string Description { get; init; }

    // CHANGE: Remove InvestigationCardType, use CardType enum instead
    public CardType CardType { get; init; } = CardType.Mental;  // Use existing CardType enum

    // Universal properties (same)
    public int Depth { get; init; }
    public PlayerStatType BoundStat { get; init; }
    public List<string> Tags { get; init; } = new List<string>();

    // ADD Mental-specific properties
    public int AttentionCost { get; init; } = 0;  // Action economy cost
    public Method Method { get; init; } = Method.Standard;  // Rhythm effect
    public ClueType ClueType { get; init; }  // Card category

    // Keep existing (already correct)
    public CardCosts Costs { get; init; }
    public CardRequirements Requirements { get; init; }
    public CardEffects Effects { get; init; }
    public CardDanger Danger { get; init; }

    // ADD method to get Attention generation (parallel to ConversationCard.GetInitiativeGeneration)
    public int GetAttentionGeneration()
    {
        // Foundation cards (Depth 1-2) generate +1 Attention
        return Depth <= 2 ? 1 : 0;
    }
}

// ADD Method enum (parallel to Delivery)
public enum Method
{
    Careful,   // Negative Balance shift, safe, slow
    Standard,  // Neutral Balance
    Bold,      // Positive Balance shift, risky, fast
    Reckless   // Extreme positive Balance, very risky
}

// ClueType enum already added in Step 1.1

// Keep all other enums and classes (CardCosts, CardRequirements, etc.) - already correct
```

### Step 1.3: Extend CardInstance for Polymorphism

**File**: `src/GameState/CardInstance.cs`

**ADD Mental and Physical template support**:
```csharp
public class CardInstance
{
    public string InstanceId { get; init; } = Guid.NewGuid().ToString();

    // Polymorphic templates - ONE instance class supports all three systems
    public ConversationCard ConversationCardTemplate { get; init; }
    public MentalCard MentalCardTemplate { get; init; }  // ADD THIS
    public PhysicalCard PhysicalCardTemplate { get; init; }  // ADD THIS (Phase 2)

    // Existing properties (keep)
    public string SourceContext { get; init; }
    public CardContext Context { get; set; }
    public bool IsPlayable { get; set; } = true;

    // ADD helper to get card type
    public CardType GetCardType()
    {
        if (ConversationCardTemplate != null) return CardType.Conversation;
        if (MentalCardTemplate != null) return CardType.Mental;
        if (PhysicalCardTemplate != null) return CardType.Physical;
        return CardType.Conversation;  // fallback
    }

    // Existing constructors (keep)
    public CardInstance() { }

    public CardInstance(ConversationCard template, string sourceContext = null)
    {
        ConversationCardTemplate = template;
        SourceContext = sourceContext;
    }

    // ADD constructor for Mental cards
    public CardInstance(MentalCard template, string sourceContext = null)
    {
        MentalCardTemplate = template;
        SourceContext = sourceContext;
    }
}
```

### Step 1.4: Create MentalCardParser

**File**: Create `src/Content/Parsers/MentalCardParser.cs`

**Copy from**: `src/Content/Parsers/InvestigationContentParser.cs` (which you read earlier)

**Changes**:
- Class name: `InvestigationContentParser` → `MentalCardParser`
- Return type: `InvestigationCard` → `MentalCard`
- DTO type: `InvestigationCardDTO` → `MentalCardDTO` (rename DTO file too)
- ADD method: `ParseMethod(string methodString)` - convert to Method enum
- ADD method: `ParseClueType(string clueTypeString)` - convert to ClueType enum
- KEEP methods: `ParseCosts`, `ParseRequirements`, `ParseEffects`, `ParseDanger` (already generic)

### Step 1.5: Create MentalFacade

**File**: Create `src/Subsystems/Mental/MentalFacade.cs`

**Copy from**: `src/Subsystems/Conversation/ConversationFacade.cs` (entire file, ~800 lines)

**Method renames**:
- `StartConversation` → `StartInvestigation`
- `ExecuteListen` → `ExecuteObserve`
- `ExecuteSpeak` → `ExecuteAct`

**Resource mapping**:
- `CurrentInitiative` → `CurrentAttention`
- `Cadence` → `ObserveActBalance`
- `ConversationSession` → `MentalSession`
- `ConversationCard` → `MentalCard`

**Balance mechanics** (OBSERVE action):
```csharp
// Apply Balance effect from OBSERVE
session.ObserveActBalance = Math.Max(-10, session.ObserveActBalance - 1);  // OBSERVE reduces balance

// Draw count based on Balance
int drawCount = 3;  // Base draw
if (session.ObserveActBalance < 0)
{
    drawCount += Math.Abs(session.ObserveActBalance);  // Bonus draws when cautious
}
```

**Balance mechanics** (ACT action):
```csharp
// Apply Balance effect from card's Method
switch (card.MentalCardTemplate.Method)
{
    case Method.Careful: session.ObserveActBalance -= 1; break;  // Reduce balance
    case Method.Standard: /* no change */ break;
    case Method.Bold: session.ObserveActBalance += 1; break;  // Increase balance
    case Method.Reckless: session.ObserveActBalance += 2; break;  // Large increase
}
```

### Step 1.6: Create MentalContent UI

**File**: Create `src/Pages/Components/MentalContent.razor` and `.cs`

**Copy from**: `src/Pages/Components/ConversationContent.razor` (entire file, 1244 lines)

**Changes**:

1. **Replace NPC dialogue section** with Location narrative:
```razor
<!-- OLD (Conversation): -->
<div class="npc-dialogue">@LastDialogue</div>

<!-- NEW (Mental): -->
<div class="location-narrative">@LocationNarrative</div>
```

2. **Replace action buttons**:
```razor
<!-- OLD: LISTEN / SPEAK -->
<div class="action-button" @onclick="ExecuteListen">LISTEN</div>
<div class="action-button" @onclick="ExecuteSpeak">SPEAK</div>

<!-- NEW: OBSERVE / ACT -->
<div class="action-button" @onclick="ExecuteObserve">OBSERVE</div>
<div class="action-button" @onclick="ExecuteAct">ACT</div>
```

3. **Replace resource displays**:
```razor
<!-- Use ResourceBar components -->
<ResourceBar Label="ATTENTION" CurrentValue="@GetCurrentAttention()" MaxValue="@GetMaxAttention()" />
<ResourceBar Label="PROGRESS" CurrentValue="@GetCurrentProgress()" MaxValue="20" BarType="progress" />
<ResourceBar Label="EXPOSURE" CurrentValue="@GetCurrentExposure()" MaxValue="10" BarType="consequence" />

<!-- Use RhythmScale for Balance -->
<RhythmScale LeftLabel="Cautious" RightLabel="Reckless"
             CurrentValue="@GetObserveActBalance()" MinValue="-5" MaxValue="5" />
```

4. **Replace system-specific badges**:
```razor
<!-- In TacticalCard component usage -->
<TacticalCard Card="@card" ...>
    <SystemSpecificBadges>
        <span class="card-method @GetMethodClass(card)">@GetMethod(card)</span>
        <span class="card-cluetype @GetClueTypeClass(card)">@GetClueType(card)</span>
    </SystemSpecificBadges>
</TacticalCard>
```

**Code-behind changes** (`.cs` file):
- Replace `ConversationFacade` → `MentalFacade`
- Replace `ConversationSession` → `MentalSession`
- Replace resource getter methods (Initiative → Attention, etc.)
- ADD methods: `GetMethod(card)`, `GetClueType(card)`, `GetMethodClass(card)`, `GetClueTypeClass(card)`

### Step 1.7: Create MentalContent CSS

**File**: Create `src/Pages/Components/MentalContent.razor.css`

**Mental-specific styles only** (NOT universal card styles):
```css
/* Location narrative (replaces NPC dialogue) */
.location-narrative {
    padding: 15px;
    background: #f4f0e8;
    border-left: 3px solid #8b7355;
    border-radius: 2px;
    font-size: 15px;
    line-height: 1.5;
}

/* Method badges */
.card-method {
    padding: 2px 6px;
    border-radius: 2px;
    font-size: 8px;
    font-weight: bold;
    text-transform: uppercase;
    color: #faf4ea;
}

.card-method.method-careful {
    background: #5a7a8a;  /* Blue - cautious */
}

.card-method.method-standard {
    background: #7a8b5a;  /* Green - normal */
}

.card-method.method-bold {
    background: #d4a76a;  /* Orange - risky */
}

.card-method.method-reckless {
    background: #d4704a;  /* Red - very risky */
}

/* ClueType badges */
.card-cluetype {
    padding: 2px 6px;
    border-radius: 2px;
    font-size: 8px;
    font-weight: bold;
    text-transform: uppercase;
    color: #faf4ea;
}

.card-cluetype.cluetype-physical {
    background: #8b4726;  /* Brown */
}

.card-cluetype.cluetype-testimonial {
    background: #6a5a8a;  /* Purple */
}

.card-cluetype.cluetype-deductive {
    background: #5a7a8a;  /* Blue */
}

.card-cluetype.cluetype-intuitive {
    background: #d4af37;  /* Gold */
}

/* Exposure display (danger theme, different from Doubt) */
.resource-fill.exposure {
    background: #d4704a;  /* Red/orange danger */
}

/* Clue tracking display */
.clue-tracking-display {
    background: #f4e8d0;
    padding: 10px 20px;
    border-bottom: 1px solid #d4c5a0;
}

.clue-counts {
    display: flex;
    align-items: center;
    gap: 20px;
}

.clue-count {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 13px;
}

.clue-count .count-value {
    font-weight: bold;
    color: #5c4a3a;
}
```

### Step 1.8: Rename and Restructure JSON Content

**Rename file**:
```bash
mv src/Content/Core/10_investigation_cards.json src/Content/Core/mental_cards.json
```

**Validate JSON structure** matches MentalCardDTO:
```json
{
  "cards": [
    {
      "id": "examine_door",
      "name": "Examine Door",
      "description": "Carefully inspect the door for clues",
      "type": "Foundation",
      "depth": 1,
      "boundStat": "Insight",
      "attentionCost": 0,
      "method": "Careful",
      "clueType": "Physical",
      "costs": {
        "stamina": 0,
        "health": 0,
        "time": 1,
        "coins": 0
      },
      "effects": {
        "progress": 2,
        "exposure": 0,
        "discoveries": []
      }
    }
    // ... more cards
  ]
}
```

### Step 1.9: Update ServiceConfiguration and PackageLoader

**ServiceConfiguration.cs**:
```csharp
// ADD registration
services.AddSingleton<Wayfarer.Subsystems.Mental.MentalFacade>();
```

**PackageLoader.cs**:

Find the Investigation loading section and UPDATE:
```csharp
// OLD (delete):
// var investigationCards = LoadInvestigationCards(packagePath);

// NEW:
var mentalCards = LoadMentalCards(packagePath);
// Store in Location.InvestigationDecks
foreach (var location in gameWorld.Locations.Values)
{
    // Assign mental decks to locations based on location ID
    location.InvestigationDecks = location.InvestigationDecks ?? new Dictionary<string, List<MentalCard>>();
    // Filter cards by location...
}
```

### Step 1.10: Build and Validate

```bash
cd src
dotnet build
```

**Must succeed with ZERO errors.**

**Validation checklist**:
- [ ] MentalSession class exists
- [ ] MentalCard class exists
- [ ] MentalFacade class exists and registered
- [ ] MentalContent.razor exists
- [ ] mental_cards.json exists and loads
- [ ] Zero references to InvestigationFacade (grep returns 0)
- [ ] Zero references to InvestigationCard (grep returns 0)

---

## PHASE 2: PHYSICAL TACTICAL SYSTEM

### Complete Copy of Mental Pattern

**Repeat ALL steps from Phase 1**, but replace Mental → Physical:

1. Create `PhysicalSession.cs` (copy MentalSession)
2. Create `PhysicalCard.cs` (copy MentalCard)
3. Update `CardInstance.cs` - ADD PhysicalCardTemplate
4. Create `PhysicalCardParser.cs` (copy MentalCardParser)
5. Create `PhysicalFacade.cs` (copy MentalFacade)
6. Create `PhysicalContent.razor` + `.cs` (copy MentalContent)
7. Create `PhysicalContent.razor.css` (copy MentalContent.razor.css)
8. Create `physical_cards.json` (copy mental_cards.json structure)
9. Update ServiceConfiguration.cs - register PhysicalFacade
10. Update PackageLoader.cs - load physical_cards.json

**Resource mapping changes**:
- Attention → Position
- ObserveActBalance → AssessExecuteBalance
- Method → Exertion (Measured/Standard/Forceful/Desperate)
- ClueType → Approach (Force/Finesse/Endurance/Speed)

**Action name changes**:
- OBSERVE → ASSESS
- ACT → EXECUTE

**Build and validate** (same checklist as Phase 1).

---

## PHASE 3: INVESTIGATION STRATEGIC ACTIVITY

### Step 3.1: Create InvestigationActivity

**File**: Create `src/Activities/InvestigationActivity.cs`

**NOT a facade** - this is a strategic orchestrator:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Wayfarer.Activities
{
    /// <summary>
    /// Strategic activity that orchestrates multi-phase investigations.
    /// NOT a tactical system - spawns Mental/Physical/Social sessions based on phase type.
    /// </summary>
    public class InvestigationActivity
    {
        private readonly GameWorld _gameWorld;
        private readonly MentalFacade _mentalFacade;
        private readonly PhysicalFacade _physicalFacade;
        private readonly ConversationFacade _conversationFacade;
        private readonly KnowledgeService _knowledgeService;

        public InvestigationActivity(
            GameWorld gameWorld,
            MentalFacade mentalFacade,
            PhysicalFacade physicalFacade,
            ConversationFacade conversationFacade,
            KnowledgeService knowledgeService)
        {
            _gameWorld = gameWorld;
            _mentalFacade = mentalFacade;
            _physicalFacade = physicalFacade;
            _conversationFacade = conversationFacade;
            _knowledgeService = knowledgeService;
        }

        /// <summary>
        /// Start a new investigation
        /// </summary>
        public async Task<InvestigationState> StartInvestigationAsync(string investigationId, Player player)
        {
            if (!_gameWorld.InvestigationTemplates.TryGetValue(investigationId, out InvestigationTemplate template))
            {
                throw new InvalidOperationException($"Investigation '{investigationId}' not found");
            }

            InvestigationState state = new InvestigationState
            {
                InvestigationId = investigationId,
                Template = template,
                CurrentPhaseIndex = 0,
                CompletedPhases = new HashSet<int>(),
                InvestigationStatus = InvestigationStatus.InProgress
            };

            // Return first phase info for UI to spawn appropriate tactical session
            return state;
        }

        /// <summary>
        /// Spawn tactical session for current phase
        /// </summary>
        public async Task<TacticalSessionInfo> SpawnPhaseSessionAsync(InvestigationState state, Player player)
        {
            InvestigationPhase phase = state.Template.Phases[state.CurrentPhaseIndex];

            // Switch on phase type to spawn appropriate tactical session
            switch (phase.TacticalSystemType)
            {
                case TacticalSystemType.Mental:
                    var mentalSession = await _mentalFacade.StartInvestigationAsync(phase.DeckOwnerId, player);
                    return new TacticalSessionInfo
                    {
                        SystemType = TacticalSystemType.Mental,
                        Session = mentalSession,
                        PhaseGoal = phase.CompletionCriteria
                    };

                case TacticalSystemType.Physical:
                    var physicalSession = await _physicalFacade.StartChallengeAsync(phase.DeckOwnerId, player);
                    return new TacticalSessionInfo
                    {
                        SystemType = TacticalSystemType.Physical,
                        Session = physicalSession,
                        PhaseGoal = phase.CompletionCriteria
                    };

                case TacticalSystemType.Social:
                    var conversationSession = await _conversationFacade.StartConversationAsync(phase.DeckOwnerId, player);
                    return new TacticalSessionInfo
                    {
                        SystemType = TacticalSystemType.Social,
                        Session = conversationSession,
                        PhaseGoal = phase.CompletionCriteria
                    };

                default:
                    throw new InvalidOperationException($"Unknown tactical system type: {phase.TacticalSystemType}");
            }
        }

        /// <summary>
        /// Handle phase completion
        /// </summary>
        public async Task<PhaseCompletionResult> OnPhaseCompleteAsync(InvestigationState state, TacticalOutcome outcome, Player player)
        {
            InvestigationPhase completedPhase = state.Template.Phases[state.CurrentPhaseIndex];

            // Check if phase criteria met
            if (!outcome.Success)
            {
                return new PhaseCompletionResult
                {
                    PhaseFailed = true,
                    InvestigationFailed = true
                };
            }

            // Grant phase rewards
            if (completedPhase.Rewards != null)
            {
                foreach (string discovery in completedPhase.Rewards.Discoveries)
                {
                    await _knowledgeService.GrantDiscoveryAsync(player, discovery);
                }
                foreach (string knowledge in completedPhase.Rewards.Knowledge)
                {
                    await _knowledgeService.GrantKnowledgeAsync(player, knowledge);
                }
            }

            // Mark phase complete
            state.CompletedPhases.Add(state.CurrentPhaseIndex);

            // Check for next phase
            int nextPhaseIndex = state.CurrentPhaseIndex + 1;
            if (nextPhaseIndex >= state.Template.Phases.Count)
            {
                // Investigation complete
                return new PhaseCompletionResult
                {
                    InvestigationComplete = true,
                    FinalRewards = state.Template.FinalRewards
                };
            }

            // Check next phase requirements
            InvestigationPhase nextPhase = state.Template.Phases[nextPhaseIndex];
            if (CanAccessPhase(nextPhase, state, player))
            {
                state.CurrentPhaseIndex = nextPhaseIndex;
                return new PhaseCompletionResult
                {
                    NextPhaseAvailable = true,
                    NextPhaseIndex = nextPhaseIndex
                };
            }
            else
            {
                return new PhaseCompletionResult
                {
                    NextPhaseLocked = true,
                    LockReason = "Requirements not met"
                };
            }
        }

        private bool CanAccessPhase(InvestigationPhase phase, InvestigationState state, Player player)
        {
            // Check requirements (completed phases, discoveries, equipment, etc.)
            // ... implementation
            return true;  // Placeholder
        }
    }

    // Supporting classes
    public class InvestigationState
    {
        public string InvestigationId { get; set; }
        public InvestigationTemplate Template { get; set; }
        public int CurrentPhaseIndex { get; set; }
        public HashSet<int> CompletedPhases { get; set; }
        public InvestigationStatus InvestigationStatus { get; set; }
    }

    public class TacticalSessionInfo
    {
        public TacticalSystemType SystemType { get; set; }
        public object Session { get; set; }  // Polymorphic (Mental/Physical/ConversationSession)
        public string PhaseGoal { get; set; }
    }

    public class PhaseCompletionResult
    {
        public bool PhaseFailed { get; set; }
        public bool InvestigationFailed { get; set; }
        public bool InvestigationComplete { get; set; }
        public bool NextPhaseAvailable { get; set; }
        public int NextPhaseIndex { get; set; }
        public bool NextPhaseLocked { get; set; }
        public string LockReason { get; set; }
        public object FinalRewards { get; set; }
    }

    public enum InvestigationStatus
    {
        NotStarted,
        InProgress,
        Completed,
        Failed
    }

    public enum TacticalSystemType
    {
        Mental,
        Physical,
        Social
    }
}
```

### Step 3.2: Extend InvestigationPhase

**File**: `src/GameState/InvestigationPhase.cs`

**ADD properties**:
```csharp
public class InvestigationPhase
{
    // Existing properties (keep)
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Goal { get; set; }
    public int ProgressThreshold { get; set; }

    // ADD these properties
    public TacticalSystemType TacticalSystemType { get; set; }  // Mental, Physical, or Social
    public string DeckOwnerId { get; set; }  // Which entity owns the deck (locationId/npcId/challengeId)
    public string CompletionCriteria { get; set; }  // What defines success

    // Existing (keep)
    public List<string> CardDeckIds { get; set; }
    public PhaseRequirements Requirements { get; set; }
    public PhaseRewards Rewards { get; set; }
}
```

### Step 3.3: Restructure investigations.json

**File**: `src/Content/Core/11_investigations.json`

**New structure** with tactical system types:
```json
{
  "investigations": [
    {
      "id": "millers_daughter",
      "name": "The Miller's Daughter",
      "description": "The miller's daughter has vanished. Investigate the old mill.",
      "phases": [
        {
          "id": "phase_0_survey",
          "name": "Survey the Mill",
          "description": "Examine the exterior of the old mill",
          "tacticalSystemType": "Mental",
          "deckOwnerId": "location_old_mill_exterior",
          "completionCriteria": "Reach 10 Progress",
          "progressThreshold": 10,
          "rewards": {
            "discoveries": ["mill_layout"],
            "narrative": "You've mapped the mill's structure."
          }
        },
        {
          "id": "phase_1_entry_choice",
          "name": "Gain Entry",
          "description": "Choose how to enter the mill",
          "alternativePhases": [
            {
              "id": "phase_1a_pick_lock",
              "name": "Pick the Lock",
              "tacticalSystemType": "Mental",
              "deckOwnerId": "location_old_mill_door",
              "completionCriteria": "Reach 8 Progress",
              "requirements": {
                "equipment": ["lockpicks"]
              }
            },
            {
              "id": "phase_1b_force_door",
              "name": "Force the Door",
              "tacticalSystemType": "Physical",
              "deckOwnerId": "challenge_force_door",
              "completionCriteria": "Reach 8 Progress",
              "requirements": {
                "stats": {"Authority": 3}
              }
            }
          ]
        },
        {
          "id": "phase_2_search",
          "name": "Search Interior",
          "tacticalSystemType": "Mental",
          "deckOwnerId": "location_old_mill_interior",
          "completionCriteria": "Reach 15 Progress",
          "requirements": {
            "completedPhases": [1]
          },
          "rewards": {
            "discoveries": ["hidden_journal", "suspicious_footprints"]
          }
        },
        {
          "id": "phase_3_witness",
          "name": "Deal with Witness",
          "tacticalSystemType": "Social",
          "deckOwnerId": "npc_nosy_neighbor",
          "completionCriteria": "Reach Momentum 8",
          "optional": true,
          "requirements": {
            "completedPhases": [2],
            "minExposure": 5
          }
        }
      ],
      "finalRewards": {
        "knowledge": ["mystery_solved"],
        "questComplete": "millers_daughter_quest"
      }
    }
  ]
}
```

### Step 3.4: Register InvestigationActivity

**ServiceConfiguration.cs**:
```csharp
// ADD registration
services.AddSingleton<InvestigationActivity>();
```

---

## FINAL VALIDATION

### Build Check
```bash
cd src
dotnet build
```
**Must succeed with ZERO errors and ZERO warnings.**

### Grep Validation (HIGHLANDER)

All must return ZERO results:
```bash
grep -r "InvestigationFacade" src/
grep -r "InvestigationCard[^T]" src/
grep -r "InvestigationCardInstance" src/
grep -r "TODO" src/ --include="*.cs"
grep -r "FIXME" src/ --include="*.cs"
```

### Architecture Validation

- [ ] THREE tactical systems exist (Social ✓, Mental ✓, Physical ✓)
- [ ] Investigation is strategic orchestrator (NOT fourth tactical system ✓)
- [ ] SessionCardDeck reused (not recreated ✓)
- [ ] card-system-shared.css extended (not duplicated ✓)
- [ ] Shared Blazor components extracted (TacticalCard, ResourceBar, etc. ✓)
- [ ] ConversationContent uses shared components ✓
- [ ] Zero duplicate card instance classes ✓
- [ ] Zero commented legacy code ✓

---

## TROUBLESHOOTING

### Problem: Build errors after deletion

**Solution**: Comment out broken code temporarily. You'll rebuild it correctly in Phase 1.

### Problem: CardInstance doesn't work with Mental cards

**Solution**: Ensure you extended CardInstance with MentalCardTemplate property and constructor (Step 1.3).

### Problem: Shared components not rendering

**Solution**: Check namespace (`Wayfarer.Pages.Components.Shared`) and ensure `@using` directive in `_Imports.razor`.

### Problem: JSON not loading

**Solution**: Check PackageLoader.cs has mental_cards.json loading code and parser registration.

---

## SUMMARY

This guide implements the correct three tactical systems architecture:

1. **Social System** (Conversations) - Already exists, refactored to use shared components
2. **Mental System** (Investigation mechanics) - Built by refactoring wrong Investigation implementation
3. **Physical System** (Challenge mechanics) - Built by copying Mental pattern
4. **Investigation Activity** (Strategic orchestrator) - Replaces InvestigationFacade with correct multi-phase orchestration

**Key Principles Applied**:
- SCORCHED EARTH: Deleted wrong implementation completely (no commented code)
- HIGHLANDER: One CardInstance, one shared CSS, no duplicate components
- REFACTOR OVER CREATE: Extracted shared components from existing, copied patterns
- PARALLEL FACADES: All three systems use identical structure with different resources

**Files Changed**: ~30 files (deletions, renames, new files)
**Lines of Code**: ~5000 (mostly copied/refactored, minimal new code)
**Estimated Time**: 2-3 weeks for complete implementation
