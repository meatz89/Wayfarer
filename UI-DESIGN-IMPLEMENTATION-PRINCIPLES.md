# Letter Queue UI Design and Implementation Principles

**CRITICAL**: This document captures the essential design and implementation principles for Wayfarer's letter queue UI system.

## Core UI Design Principles

### 1. Queue-Centric Information Architecture
**Principle**: The 8-slot letter queue is the primary UI element - all other information supports queue decision-making.

**Implementation**:
```razor
<!-- Queue always visible and central -->
<div class="letter-queue-container">
    @for (int i = 1; i <= 8; i++)
    {
        <div class="queue-slot" data-position="@i">
            @if (queue.GetLetter(i) != null)
            {
                <LetterCard Letter="queue.GetLetter(i)" Position="i" />
            }
        </div>
    }
</div>
```

**Why**: The queue visualizes the player's entire social life - everything else is context for queue decisions.

### 2. Token Cost Transparency
**Principle**: Connection token costs for queue manipulation must be immediately visible and clear.

**Examples**:
- **Purge Action**: "Remove bottom letter (3 tokens)" with current token count
- **Priority Action**: "Move to slot 1 (5 Trust tokens)" - disabled if insufficient tokens
- **Skip Action**: "Deliver out of order (1 Trust token)" - shows specific token type needed

**Anti-Pattern**: Hidden token costs that surprise players during actions

### 3. Per-NPC Token Relationship Display
**Principle**: Character Relationship Screen shows connection tokens player has with each specific NPC.

**Implementation**:
```razor
<div class="npc-relationship-card">
    <div class="npc-info">
        <h3>@npc.Name</h3>
        <p>Location: @npc.Location</p>
    </div>
    <div class="token-display">
        @foreach (var tokenType in Enum.GetValues<ConnectionType>())
        {
            var count = GetTokensWithNPC(npc.Id, tokenType);
            if (count > 0)
            {
                <div class="token-count">
                    <TokenIcon Type="tokenType" />
                    <span>@count</span>
                </div>
            }
        }
    </div>
</div>
```

**Why**: Players need to see exactly how much social capital they have with each NPC for strategic planning.

### 4. Deadline Urgency Visualization
**Principle**: Letter deadlines show visual urgency without strategic analysis.

**Implementation**:
```razor
<div class="letter-deadline @GetDeadlineClass(letter.Deadline)">
    <span class="deadline-text">@letter.Deadline days</span>
    <div class="deadline-bar" style="width: @GetDeadlinePercentage(letter.Deadline)%"></div>
</div>

@code {
    private string GetDeadlineClass(int deadline)
    {
        return deadline switch
        {
            1 => "deadline-critical",
            2 => "deadline-urgent", 
            <= 3 => "deadline-warning",
            _ => "deadline-normal"
        };
    }
}
```

**Why**: Visual cues help players prioritize without calculating optimal strategies for them.

### 5. No Queue Automation or Optimization
**Principle**: NEVER provide automated queue management or optimization suggestions.

**Violations to Avoid**:
```csharp
// ❌ WRONG: Automated queue optimization
public List<QueueOptimizationSuggestion> GetOptimalDeliveryOrder()
public RouteRecommendation GetBestRouteForQueue()
public List<TokenSpendingStrategy> GetTokenOptimizationStrategies()

// ✅ RIGHT: Information for player decision-making
public int GetTokenCost(QueueAction action)
public bool CanPerformAction(QueueAction action)
public List<Letter> GetExpiredLetters()
```

**Why**: Queue management IS the game - automation removes the strategic challenge.

## Letter Queue Screen Architecture

### Primary Screen Components

**1. Queue Display (Central)**
- **8-slot visual queue** with position numbers 1-8
- **Letter cards** showing sender, recipient, deadline, payment
- **Drag indicators** (visual only - no drag-and-drop functionality)
- **Empty slot placeholders** for positions without letters

**2. Queue Manipulation Panel**
- **Action buttons** with token costs clearly displayed
- **Token balance** showing current connection tokens by type
- **Action validation** - disable buttons when insufficient tokens
- **Confirmation prompts** for token spending actions

**3. Standing Obligations Panel**
- **Active obligations** with brief descriptions
- **Queue effects** showing how obligations modify behavior
- **Obligation conflicts** highlighted when multiple obligations clash
- **Acquisition history** - when/how obligations were gained

### Letter Card Design

**Essential Information (Always Visible)**:
- **Sender name** and **token type icon**
- **Deadline countdown** with visual urgency
- **Payment amount** in coins
- **Queue position** prominently displayed

**Contextual Information (On Hover/Click)**:
- **Recipient details** and location
- **Letter size** and inventory impact
- **Relationship history** with sender
- **Skip consequences** if letter is not delivered

### Token Display Patterns

**Player Token Balance**:
```razor
<div class="token-balance">
    @foreach (var tokenType in Enum.GetValues<ConnectionType>())
    {
        <div class="token-type">
            <TokenIcon Type="tokenType" />
            <span>@GetPlayerTokens(tokenType)</span>
        </div>
    }
</div>
```

**Action Token Costs**:
```razor
<button class="queue-action" @onclick="PurgeLetter" disabled="@(!CanAffordPurge())">
    Purge Bottom Letter
    <div class="token-cost">
        <span>3</span> <TokenIcon Type="ConnectionType.Any" />
    </div>
</button>
```

## Character Relationship Screen Architecture

### NPC Overview Display

**Essential Information**:
- **NPC name** and **connection token type**
- **Current location** with travel requirements
- **Relationship status** (Warm/Neutral/Cold/Frozen)
- **Per-NPC token count** by type

**Interaction Options** (Location-Dependent):
- **Available only at NPC location** - grayed out when not present
- **Social activities** that generate tokens (meals, conversations)
- **Letter offers** based on relationship level
- **Crisis moments** and forgiveness opportunities

### Relationship History Panel

**Skip Tracking**:
- **Last 3 skipped letters** with dates
- **Relationship damage** from expired letters
- **Forgiveness events** that reset relationship

**Delivery History**:
- **Consistent delivery streak** bonuses
- **Token earning pattern** from this NPC
- **Special letter chains** completed

## Standing Obligations Screen Architecture

### Obligation Management Display

**Active Obligations**:
- **Obligation name** and **acquisition date**
- **Benefit description** (e.g., "Noble letters enter at slot 5")
- **Constraint description** (e.g., "Cannot refuse noble letters")
- **Queue behavior changes** with examples

**Conflict Detection**:
- **Conflicting obligations** highlighted in red
- **Impossible scenarios** created by multiple obligations
- **Strategic implications** of each combination

### Obligation Effects Visualization

**Queue Behavior Changes**:
- **Letter entry positions** modified by obligations
- **Forced letter generation** (e.g., Shadow's Burden)
- **Token costs** modified by obligations
- **Relationship requirements** imposed by obligations

## CSS Architecture and Validation Requirements

### CRITICAL: CSS Class Validation Principle

Before using ANY CSS class in HTML/Razor components, you MUST:

1. **READ ALL CSS FILES** - Search and review every CSS file in the project (`/src/wwwroot/css/*.css`)
2. **VERIFY CLASS EXISTS** - Confirm the exact class name is defined in the CSS files
3. **CHECK FOR DUPLICATES** - Ensure you're not creating redundant styles that already exist
4. **MATCH EXISTING PATTERNS** - New classes must conform to existing naming conventions and style patterns
5. **NO PHANTOM CLASSES** - Never use Bootstrap, Framework, or other external CSS classes unless explicitly included in the project
6. **VALIDATE BEFORE USE** - If a class doesn't exist, either find an existing equivalent or create proper CSS definitions first

**VIOLATION IS CRITICAL** - Using undefined CSS classes creates broken layouts and "fucking mess" UIs. Always validate CSS existence before HTML implementation.

### CSS Architecture Principles

- ✅ **SEPARATE CSS FILES** - All CSS must be in dedicated .css files, never inline in Razor components
- ✅ **CHECK EXISTING STYLES FIRST** - ALWAYS read existing CSS files before creating new ones to reuse existing classes and patterns
- ✅ **COHERENT STYLE PATTERNS** - New styles must follow existing color variables, font families, and design patterns
- ✅ **USE CSS VARIABLES** - Always use existing CSS variables (--text-primary, --bg-panel, etc.) instead of hardcoded colors
- ✅ **MAINTAIN VISUAL HIERARCHY** - Follow established font size, spacing, and layout patterns from existing components
- ✅ **REUSE EXISTING CLASSES** - Look for existing CSS classes that can be reused before creating new ones
- ❌ **NO INLINE STYLES** - Never add `<style>` sections to Razor components; this violates UI architecture principles
- ❌ **NO DUPLICATE STYLES** - Never create new CSS that duplicates existing functionality

## Implementation Anti-Patterns

### Forbidden UI Elements

**❌ Queue Optimization Tools**:
- "Optimal delivery order" suggestions
- "Best route for current queue" recommendations
- "Token spending efficiency" calculators

**❌ Automated Queue Management**:
- Auto-sort letters by deadline
- Auto-purge expired letters
- Auto-spend tokens for optimization

**❌ Relationship Automation**:
- "Best NPCs to focus on" suggestions
- "Relationship investment opportunities" analysis
- "Token earning efficiency" calculations

### Required UI Patterns

**✅ Clear Information Display**:
- All costs and consequences visible before actions
- Current state always clear (tokens, deadlines, relationships)
- Historical information available when requested

**✅ Player Agency Support**:
- All actions require explicit player choice
- Consequences explained but not calculated
- Strategic decisions remain with player

**✅ Emergent Complexity**:
- Simple actions (purge, skip, extend) combine into complex strategies
- Visual feedback helps players understand patterns
- No hidden mechanics or surprise consequences

## Success Metrics

### UI Effectiveness Indicators

**Queue Management**:
- Players spend time studying queue before making decisions
- Token spending feels meaningful and strategic
- Deadline pressure creates genuine urgency

**Relationship Investment**:
- Players remember specific NPCs and their preferences
- Character Relationship Screen used for strategic planning
- Per-NPC token display influences travel decisions

**Obligation Complexity**:
- Players understand how obligations change their gameplay
- Standing Obligations Screen consulted for major decisions
- Obligation conflicts create interesting strategic dilemmas

This UI architecture supports the **letter queue management** experience by providing clear information for strategic decision-making while preserving the **puzzle-solving challenge** that makes the game engaging.