# Letter Queue UI Design and Implementation Principles

**CRITICAL**: This document captures the essential design and implementation principles for Wayfarer's letter queue UI system.

**ARCHITECTURAL MANDATE**: All UI components MUST use the GameFacade pattern. Direct service injection is FORBIDDEN. See [GAME-FACADE-ARCHITECTURE.md](/mnt/c/git/wayfarer/GAME-FACADE-ARCHITECTURE.md) for details.

## Core UI Design Principles

### 0. Distance-Based Information Principle
**Principle**: NPCs are only interactable when at the same location. Over distance, show only remembered information (tokens, past interactions).

**Implementation**:
- **At Same Location**: Show full NPC details, availability, current offers, interaction options
- **At Distance**: Show only token counts, debt status, letter history
- **Never Show**: Real-time NPC status when not present (availability, current mood, etc.)

**Why**: Creates realistic information constraints and emphasizes the importance of being present.

### 0.1 Consistent Container Widths
**Principle**: All main content containers should use the same full-width styling for visual consistency.

**Implementation**:
```css
.letter-queue-container,
.obligation-container,
.board-container,
.location-container,
.rest-container {
    max-width: 1200px;
    margin: 0 auto;
    padding: 1.5rem;
    background-color: var(--bg-panel);
    border: 1px solid var(--oak-dark);
    border-radius: 5px;
}
```

**Why**: Inconsistent widths make the UI feel unpolished and confuse visual hierarchy.

### 0.2 Tooltips Over Text Blocks
**Principle**: Use tooltips for guides and help text, not large explanatory sections that consume screen space.

**Implementation**:
- Help icons (?) with comprehensive tooltips
- Hover tooltips on interactive elements
- Remove large guide sections from main views

**Why**: Maximizes usable screen space for actual gameplay elements.

### 0.3 Contextual Actions at Point of Use
**Principle**: Actions should be available where they make narrative sense, not scattered across screens.

**Implementation**:
- **Rest Screen**: ALL rest options (basic rest + location-specific like inn, tavern, church)
- **Location Screen**: NPC interactions, spot navigation
- **Market Screen**: Trading actions only
- **Letter Board**: Public letter offers only

**Why**: Reduces cognitive load by grouping related actions together.

### 1. Queue-Centric Information Architecture
**Principle**: The 8-slot letter queue is the primary UI element - all other information supports queue decision-making.

**Implementation**:
```razor
@inject IGameFacade GameFacade

<!-- Queue always visible and central -->
<div class="letter-queue-container">
    @{
        var letterQueue = GameFacade.GetLetterQueue();
    }
    @for (int i = 1; i <= 8; i++)
    {
        <div class="queue-slot" data-position="@i">
            @if (letterQueue.GetLetterAtPosition(i) != null)
            {
                <LetterCard Letter="letterQueue.GetLetterAtPosition(i)" Position="i" />
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
@inject IGameFacade GameFacade

<div class="npc-relationship-card">
    <div class="npc-info">
        <h3>@npcRelationship.Name</h3>
        <p>Location: @npcRelationship.CurrentLocation</p>
    </div>
    <div class="token-display">
        @foreach (var token in npcRelationship.Tokens)
        {
            <div class="token-count">
                <TokenIcon Type="@token.Type" />
                <span>@token.Count</span>
            </div>
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

## Critical UI Layout Learnings

### **LEARNED: Connection Token Misplacement**
**Problem**: Connection tokens were displayed on the Letter Queue screen where they don't belong conceptually.
**Solution**: Connection tokens belong to NPC relationships, not the letter queue. Move token display to Character Relationship Screen only.
**Principle**: **Information Architecture Belongs to Context** - Display data where it's conceptually relevant, not where it's convenient.

### **LEARNED: Navigation Consistency Violation**
**Problem**: Letter Queue screen had no navigation back to main menu, violating navigation pattern used elsewhere.
**Solution**: All screens must have consistent navigation - main navigation in MainGameplayView.razor, back buttons on individual screens.
**Principle**: **Navigation Patterns Must Be Consistent** - Every screen needs a way back to the main interface.

### **LEARNED: Text Readability in Constrained Layouts**
**Problem**: Letter card text was cut off and unreadable due to height constraints and poor flex layout.
**Solution**: Use `justify-content: flex-start` instead of `space-between`, increase min-height, add proper line-height and word-wrapping.
**Principle**: **Readability Trumps Aesthetic Layout** - Text must be fully readable even if it makes cards slightly larger.

### **LEARNED: Action UI Grouping and Spatial Efficiency**
**Problem**: Queue management actions were in separate containers taking up too much vertical space.
**Solution**: Use grid layout to group related actions horizontally, compact button text, use consistent small button sizes.
**Principle**: **Group Related Actions Spatially** - Actions that serve the same purpose should be visually grouped and use space efficiently.

### **REQUIRED Navigation Pattern**
```razor
<!-- Main Screen: MainGameplayView.razor -->
case CurrentViews.SpecificScreen:
    <div class="location-container">
        <div class="location-header">
            <h2 class="location-title">Screen Name</h2>
            <div class="location-actions">
                <button class="nav-button" @onclick="() => CurrentScreen = CurrentViews.LocationScreen">
                    Back to Location
                </button>
            </div>
        </div>
    </div>
    <SpecificScreenComponent />
    break;
```

### **FORBIDDEN UI Patterns Based on User Feedback**
- ❌ **Information in Wrong Context** - Don't show relationship data on task screens
- ❌ **Inconsistent Navigation** - Every screen must have clear navigation back
- ❌ **Unreadable Text Due to Layout** - Never sacrifice text readability for visual layout
- ❌ **Vertically Stacked Actions** - Group related actions horizontally when possible
- ❌ **Phantom CSS Variables** - Using undefined CSS variables breaks layouts completely

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

## Navigation Architecture Principles

### Core Navigation Hierarchy

#### Three-Tier System
1. **Entry Layer** (GameUI.razor)
   - Content Validation → Character Creation → Main Game
   - Single responsibility: Get player into the game

2. **Primary Navigation Layer** (MainGameplayView.razor)
   - Letter Queue (Home/Hub)
   - Location Context
   - Character Context
   - System Functions

3. **Contextual Sub-Screens**
   - Location → Travel, Market, Rest, Board
   - Character → Status, Relations, Obligations
   - Queue → No sub-screens (it's the hub)

### Navigation Design Principles

#### **Queue-Centric Navigation**
**Principle**: Letter Queue is the primary interface and conceptual home.
- Always accessible with single click/keypress
- Default screen after character creation
- Visual prominence in navigation
- Other screens are "excursions" from the queue

#### **Contextual Grouping**
**Principle**: Related functions are grouped under logical contexts.
- **Location Context**: All activities tied to physical presence
- **Character Context**: All personal/relationship management
- **Queue Context**: Core gameplay loop

#### **Minimal Navigation Depth**
**Principle**: No screen should be more than 2 clicks from the queue.
- Primary Nav → Context Screen → Sub-Screen (maximum depth)
- Back button always returns to parent context

#### **Consistent Navigation Patterns**
**Principle**: Same navigation behavior everywhere.
```razor
<!-- Every sub-screen follows this pattern -->
<div class="location-container">
    <div class="location-header">
        <h2 class="location-title">@ScreenTitle</h2>
        <div class="location-actions">
            <button class="nav-button" @onclick="NavigateBack">
                Back to @ParentContext
            </button>
        </div>
    </div>
</div>
```

### Navigation UI Components

#### Primary Navigation Bar
- **Position**: Top of screen, always visible
- **Style**: Prominent but not overwhelming
- **Contents**: [Queue] [Location] [Character] [System]
- **Active State**: Clear visual indicator of current context

#### Contextual Navigation
- **Position**: Below primary nav, only when in context
- **Style**: Subordinate to primary nav
- **Contents**: Context-specific sub-screens
- **Behavior**: Disappears in queue view

#### Quick Access Features
- **Breadcrumbs**: Show current location in hierarchy
- **Mini-Queue**: Collapsible queue summary on non-queue screens

### Navigation State Management

```csharp
public class NavigationService
{
    private Stack<CurrentViews> _history = new();
    private CurrentViews _current;
    
    public void NavigateTo(CurrentViews screen)
    {
        _history.Push(_current);
        _current = screen;
        OnNavigationChanged?.Invoke(screen);
    }
    
    public void NavigateBack()
    {
        if (_history.Count > 0)
        {
            _current = _history.Pop();
            OnNavigationChanged?.Invoke(_current);
        }
    }
    
    public NavigationContext GetContext(CurrentViews screen)
    {
        return screen switch
        {
            CurrentViews.LetterQueueScreen => NavigationContext.Queue,
            CurrentViews.LocationScreen or 
            CurrentViews.TravelScreen or 
            CurrentViews.MarketScreen => NavigationContext.Location,
            CurrentViews.CharacterHub or
            CurrentViews.RelationshipScreen => NavigationContext.Character,
            _ => NavigationContext.System
        };
    }
}
```

### Screen Transition Principles

#### **Preserve User Context**
- Maintain selected items/NPCs when navigating
- Return to same scroll position
- Remember expanded/collapsed states

#### **Clear Visual Feedback**
- Animate transitions (subtle slide/fade)
- Show loading states for data fetches
- Highlight navigation path taken

#### **Platform Requirements**
- **Target**: Web app for Chrome browser only
- **No mobile/tablet support**
- **No keyboard shortcuts**

This navigation architecture reinforces that **letter queue management** is the core game experience while providing efficient access to supporting systems. The UI architecture supports the **letter queue management** experience by providing clear information for strategic decision-making while preserving the **puzzle-solving challenge** that makes the game engaging.

## NPC Introduction and Network Unlock Principles

### Core Principle: Narrative Introduction, Not Mechanical Unlock
**CRITICAL**: NPCs don't "unlock" other NPCs - they **introduce** you through personal connections and dialogue.

**Implementation Requirements**:
- **Face-to-face introduction**: Player must be at NPC's location when introduction happens
- **Personal dialogue**: NPCs explain why they're introducing you ("Elena trusts you with her personal letters")
- **Introduction narrative**: Show the actual introduction happening, not just "NPC unlocked"
- **Relationship context**: Display why this NPC trusts you enough to make introductions

**Anti-Patterns**:
- ❌ "You have unlocked Sarah" - mechanical and gamey
- ❌ Silent automatic unlocks when reaching token thresholds
- ❌ NPCs appearing in lists without introduction context
- ❌ "Achievement unlocked" style notifications

**Correct Patterns**:
- ✅ Elena says: "I'd like you to meet my friend Sarah. She often needs sensitive documents delivered."
- ✅ Show introduction happening at NPC's location with dialogue
- ✅ New NPC explains how they know the introducer
- ✅ Player learns about new NPC through conversation, not UI notification

### UI Implementation for Introductions

```razor
<!-- When player has sufficient relationship for introduction -->
<div class="npc-introduction-opportunity">
    <div class="introduction-prompt">
        <p class="npc-dialogue">
            "You know, I've been meaning to introduce you to someone. 
            My friend Sarah could really use someone reliable like you."
        </p>
        <button class="accept-introduction">
            "I'd be happy to meet them"
        </button>
    </div>
</div>

<!-- During introduction -->
<div class="introduction-scene">
    <div class="introducer-dialogue">
        <span class="speaker">Elena:</span>
        "Sarah, this is the courier I mentioned. They've been incredibly reliable with my personal correspondence."
    </div>
    <div class="new-npc-dialogue">
        <span class="speaker">Sarah:</span>
        "Elena speaks highly of you. I have some academic letters that require... discretion."
    </div>
</div>
```

### Character Relationship Screen Integration
- Show which NPCs can introduce you to others (when relationship is strong enough)
- Display introduction opportunities as relationship milestones
- Track introduction history ("Introduced by Elena")
- Show network connections between NPCs visually

## Game vs App UI Design Principles
**UI should support discovery and decision-making, not replace player thinking or overwhelm with information.**

### CONTEXTUAL INFORMATION PRINCIPLES
- ✅ **SHOW RELEVANT, NOT COMPREHENSIVE** - Display only information immediately relevant to player's current context
- ✅ **PROGRESSIVE DISCLOSURE** - Start with essential info, allow drilling down for details when needed
- ❌ **NO INFORMATION OVERLOAD** - Don't show all possible information at once
- ❌ **NO STRATEGIC CATEGORIZATION** - Don't artificially separate information into "strategic" vs "non-strategic"

### DECISION-FOCUSED DESIGN
- ✅ **DECISION SUPPORT** - Present information that helps players make immediate decisions
- ✅ **CONTEXTUAL RELEVANCE** - Show information based on what the player is currently doing
- ❌ **NO OPTIMIZATION HINTS** - Don't tell players what the "best" choice is
- ❌ **NO AUTOMATED ANALYSIS** - Don't provide "Investment Opportunities" or "Trade Indicators"

### SPATIAL EFFICIENCY
- ✅ **EFFICIENT SPACE USE** - Every pixel should serve a purpose
- ✅ **VISUAL HIERARCHY** - Use icons, colors, and layout to convey information quickly
- ❌ **NO VERBOSE TEXT** - Don't use 15+ lines of text when 3-4 lines suffice
- ❌ **NO REDUNDANT SECTIONS** - Don't repeat the same information in multiple places

### FORBIDDEN UI PATTERNS
- ❌ **"Strategic Market Analysis" sections** - Violates NO AUTOMATED CONVENIENCES principle
- ❌ **"Equipment Investment Opportunities"** - Tells players what to buy, removing discovery
- ❌ **"Trade Opportunity Indicators"** - Automated system solving optimization puzzles
- ❌ **"Profitable Items" lists** - Removes the challenge of finding profit opportunities
- ❌ **"Best Route" recommendations** - Eliminates route planning gameplay
- ❌ **Verbose NPC schedules** - Information overload that doesn't help decisions

### REQUIRED UI PATTERNS
- ✅ **Basic availability indicators** - Simple 🟢/🔴 status without detailed explanations
- ✅ **Item categories for filtering** - Help players find what they're looking for
- ✅ **Current status information** - What's happening right now
- ✅ **Essential action information** - What the player can do immediately
- ✅ **Click-to-expand details** - Full information available when specifically requested

### FRONTEND PERFORMANCE PRINCIPLES
- **NEVER use caching in frontend components** - Components should be stateless and reactive
- **Reduce queries by optimizing when objects actually change** - Focus on state change detection, not caching
- **Log at state changes, not at queries** - Debug messages should track mutations, not reads
- **Use proper reactive patterns** - Let Blazor's change detection handle rendering optimization

**Remember**: The game is about **personal relationships and trust**, not mechanical progression systems. Every introduction should feel like a meaningful expansion of your social network through earned trust.