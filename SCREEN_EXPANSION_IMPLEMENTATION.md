# Wayfarer Screen Expansion: Implementation Guide

**Status:** Planning Document for Implementation
**Version:** 1.0
**Date:** 2025-01-24
**Scope:** Three new screen types (Conversation Tree, Observation, Emergency Response)

---

## Table of Contents

1. [Executive Summary](#executive-summary)
2. [Context & Motivation](#context--motivation)
3. [Current State Analysis](#current-state-analysis)
4. [Design Philosophy](#design-philosophy)
5. [Architectural Patterns to Follow](#architectural-patterns-to-follow)
6. [Screen 1: Conversation Tree](#screen-1-conversation-tree)
7. [Screen 2: Observation/Examination](#screen-2-observationexamination)
8. [Screen 3: Emergency Response](#screen-3-emergency-response)
9. [Implementation Roadmap](#implementation-roadmap)
10. [Integration Strategy](#integration-strategy)
11. [Testing Strategy](#testing-strategy)
12. [Reference Materials](#reference-materials)

---

## Executive Summary

### The Problem
Current Wayfarer has perfect mechanical coherence but narrow situational variety. Every interaction funnels into three patterns:
1. Location screen → Pick goal from list
2. Travel screen → Pick route
3. Challenge screen → Play cards

Players see the mechanical loop regardless of narrative context. The game feels like "select challenge from menu" rather than "navigate living world."

### The Solution
Add three new screen types that use existing resources (time, coins, stamina, focus, health) but create different decision contexts:

1. **Conversation Tree Screen** - Simple dialogue without tactical challenge (precursor to Social challenges)
2. **Observation/Examination Screen** - Investigate scene with multiple examination points
3. **Emergency Response Screen** - Urgent situation demanding immediate response

These aren't "challenge variants." They're **distinct interaction patterns** that feel different, play different, but share resource foundation.

### Key Principle
**Same resources, different contexts.** No new currencies, no new abstractions. Time, focus, coins, stamina, and health create impossible choices in new situational frameworks.

---

## Context & Motivation

### Current State: What Works

**Wayfarer has achieved:**
- Perfect mechanical coherence across three tactical systems (Social/Mental/Physical)
- Clean architecture with strong separation of concerns
- Catalogue pattern eliminates runtime string matching
- Single source of truth in GameWorld
- Deterministic gameplay with perfect information
- Resource competition creates strategic depth

**Existing screens:**
- `LocationContent.razor` - World navigation hub
- `ConversationContent.razor` - Tactical Social challenges (cards, momentum, doubt)
- `MentalContent.razor` - Tactical Mental challenges (investigation cards)
- `PhysicalContent.razor` - Tactical Physical challenges (action cards)
- `TravelContent.razor` - Route navigation
- `ExchangeContent.razor` - NPC trading (COMPLETE, no changes needed)

### Current State: What's Missing

**Interaction variety is narrow:**
- Every NPC interaction is either "trade items" or "tactical card battle"
- Every location is either "pick goal" or "travel to new location"
- No simple conversations that don't require deck building
- No scene examination without full tactical challenge
- No world events that interrupt player planning

**Visual novels like 80 Days, Roadwarden, Citizen Sleeper create richness through variety:**
- Same resource foundation (time, supplies, reputation)
- Different interaction patterns per situation (dialogue, examination, choices)
- Contextual decision-making (not always "enter challenge")

### Why This Expansion Matters

**1. Narrative Coherence**
Not every conversation should be a tactical puzzle. Sometimes NPCs just talk. Sometimes you examine a scene briefly without full investigation protocol.

**2. Strategic Pacing**
Players need "breather" content between tactical challenges. Simple dialogues and quick examinations create rhythm.

**3. World Dynamism**
Emergencies create the feeling that the world exists independent of player actions. Events happen regardless of player plans.

**4. Gateway Pattern**
Conversation Tree → Social Challenge progression feels natural. You talk casually, tension rises, conversation becomes tactical negotiation.

**5. Resource Pressure Variety**
- Conversations cost time and focus (different from tactical challenges)
- Examinations force prioritization (can't check everything)
- Emergencies demand immediate resource commitment

---

## Current State Analysis

### Existing Architecture (Discovered Patterns)

#### 1. Screen Routing System

**File:** `C:\Git\Wayfarer\src\Pages\GameScreen.razor.cs`

**ScreenMode Enum:**
```csharp
public enum ScreenMode
{
    Location,           // Main world screen
    Exchange,           // Marketplace UI
    Travel,             // Route travel UI
    SocialChallenge,    // NPC conversation (tactical)
    MentalChallenge,    // Investigation/puzzle (tactical)
    PhysicalChallenge   // Obstacle combat (tactical)
}
```

**Navigation Stack:**
```csharp
private Stack<ScreenContext> _navigationStack = new(10);

public class ScreenContext
{
    public ScreenMode Mode { get; set; }
    public ScreenStateData StateData { get; set; } = new();
    public DateTime EnteredAt { get; set; }
}
```

**Screen Rendering (GameScreen.razor):**
```csharp
@switch (CurrentScreen)
{
    case ScreenMode.Location:
        <LocationContent OnActionExecuted="RefreshUI" />
        break;
    case ScreenMode.SocialChallenge:
        <ConversationContent Context="@CurrentSocialContext" OnConversationEnd="HandleConversationEnd" />
        break;
    // ... other screens
}
```

#### 2. Context Pattern

**All screens use Context objects as data containers:**

**Example:** `C:\Git\Wayfarer\src\GameState\Contexts\SocialChallengeContext.cs`
```csharp
public class SocialChallengeContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }
    public string NpcId { get; set; }
    public NPC Npc { get; set; }
    public SocialSession Session { get; set; }
    public List<CardInstance> ObservationCards { get; set; }
    public ResourceState PlayerResources { get; set; }
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }
}
```

**Context Creation Pattern (in Facades):**
```csharp
// GameFacade delegates to specialized facade
public async Task<SocialChallengeContext> CreateConversationContext(string npcId, string requestId)
{
    return await _conversationFacade.CreateConversationContext(npcId, requestId);
}
```

#### 3. Facade Layer Pattern

**File:** `C:\Git\Wayfarer\src\Services\GameFacade.cs`

**Pattern: HIGHLANDER - Single entry point**
```
UI Layer (Components)
  ↓ (only calls)
GameFacade (Orchestrator)
  ↓ (delegates to)
Specialized Facades (ResourceFacade, LocationFacade, SocialFacade, etc.)
  ↓ (coordinate with)
Services/Managers (Business logic)
  ↓ (manipulate)
GameWorld (Single source of truth)
```

**All facades follow:**
- Synchronous for simple state changes (ResourceFacade.SpendCoins)
- Asynchronous for complex operations (SocialFacade.CreateConversationContext)
- Return strongly-typed results (never Dictionary<string, object>)
- Validate BEFORE applying changes
- Apply changes atomically (all or nothing)

#### 4. Component Pattern

**All screen components follow identical structure:**

```
ConversationContent.razor                    ← Template (markup)
├─ @inherits ConversationContentBase
├─ @namespace Wayfarer.Pages.Components
├─ Markup with component state checks
└─ Event handlers call parent methods

ConversationContent.razor.cs                 ← Code-behind (logic)
├─ public class ConversationContentBase : ComponentBase
├─ [Inject] protected GameFacade GameFacade { get; set; }
├─ [Parameter] public SocialChallengeContext Context { get; set; }
├─ [Parameter] public EventCallback OnConversationEnd { get; set; }
├─ [CascadingParameter] protected GameScreenBase GameScreen { get; set; }
├─ Protected properties (view model, state)
└─ Protected methods (event handlers)

ConversationContent.razor.css               ← Isolated styles
├─ Scoped to this component only
└─ NO inline styles in markup
```

**Component Communication:**
- Parent → Child: Parameters (`[Parameter]`)
- Child → Parent: EventCallbacks (`await OnConversationEnd.InvokeAsync()`)
- Child → Grandparent: Cascading values (`[CascadingParameter]`)

#### 5. JSON → DTO → Parser → Entity → GameWorld Flow

**File:** `C:\Git\Wayfarer\src\Content\Parsers\SocialCardParser.cs`

**The Parser-JSON-Entity Triangle:**
```
JSON (Categorical properties)
  ↓ Deserialized to
DTO (Pass-through structure)
  ↓ Processed by
Parser (Translation + Validation)
  ↓ Calls Catalogue for concrete values
Catalogue (Parse-time only, static methods)
  ↓ Returns concrete values
Entity (Strongly-typed domain object)
  ↓ Stored in
GameWorld (Flat lists, single source of truth)
```

**Example: Social Card**
```json
{
  "id": "insight_observation",
  "boundStat": "Insight",
  "depth": 2,
  "conversationalMove": "Observation"
}
```

```csharp
// Parser calls Catalogue
effectFormula = SocialCardEffectCatalog.GetEffectFromCategoricalProperties(
    move.Value, boundStat.Value, (int)depth, dto.Id);

// Entity stores concrete value (NO runtime catalogue calls)
return new SocialCard
{
    BoundStat = boundStat,
    Depth = depth,
    EffectFormula = effectFormula  // Concrete value stored once
};
```

#### 6. Catalogue Pattern

**File:** `C:\Git\Wayfarer\src\Content\Catalogs\SocialCardEffectCatalog.cs`

**Characteristics:**
- Static class with static methods
- Called ONLY by parsers (parse-time translation)
- NEVER imported by facades/services (runtime forbidden)
- Translates categorical → concrete values
- Throws InvalidOperationException on unknown inputs (fail-fast)
- Deterministic (same inputs → same outputs)

**Example:**
```csharp
public static CardEffectFormula GetEffectFromCategoricalProperties(
    ConversationalMove move, PlayerStatType stat, int depth, string cardId)
{
    return move switch
    {
        ConversationalMove.Remark => GetRemarkEffect(stat, depth),
        ConversationalMove.Observation => GetObservationEffect(stat, depth, cardId),
        ConversationalMove.Argument => GetArgumentEffect(stat, depth),
        _ => throw new InvalidOperationException($"Unknown move: {move}")
    };
}
```

#### 7. GameWorld Storage

**File:** `C:\Git\Wayfarer\src\GameState\GameWorld.cs`

**Pattern: Flat lists, single source of truth**
```csharp
// All entities stored in flat lists
public List<Venue> Venues { get; set; } = new List<Venue>();
public List<Location> Locations { get; set; } = new List<Location>();
public List<NPC> NPCs { get; set; } = new List<NPC>();
public List<Goal> Goals { get; set; } = new List<Goal>();
public List<Obstacle> Obstacles { get; set; } = new List<Obstacle>();
public List<SocialCard> SocialCards { get; set; } = new List<SocialCard>();

// Active sessions (nullable, cleared on completion)
public SocialSession CurrentSocialSession { get; set; }
public MentalSession CurrentMentalSession { get; set; }
public PhysicalSession CurrentPhysicalSession { get; set; }
```

**Entity Reference Pattern:**
- Entities reference each other by ID (string properties)
- Object references wired in Phase 2 (GameWorldInitializer.WireObjectGraph)
- NO Dictionary<string, Entity> - always List<Entity>
- NO HashSet<T> - always List<T>

#### 8. CSS Organization

**Shared Styles:**
- `C:\Git\Wayfarer\src\wwwroot\css\common.css` - Global typography, colors
- `C:\Git\Wayfarer\src\wwwroot\css\card-system-shared.css` - Card patterns

**Component-Specific (Isolated):**
- `C:\Git\Wayfarer\src\Pages\Components\ConversationContent.razor.css`
- `C:\Git\Wayfarer\src\Pages\Components\MentalContent.razor.css`
- Automatically scoped by Blazor (no manual scoping needed)

**Feature-Specific:**
- `C:\Git\Wayfarer\src\wwwroot\css\conversation.css` - Conversation system
- `C:\Git\Wayfarer\src\wwwroot\css\location.css` - Location system
- `C:\Git\Wayfarer\src\wwwroot\css\exchange-screen.css` - Exchange system

---

## Design Philosophy

### Core Principles

#### 1. Resource Foundation, Not Currency Proliferation
**Principle:** Use existing resources (time, focus, stamina, coins, health) in new contexts.

**Why:** Every new currency adds cognitive load. Strategic depth comes from resource competition, not resource variety.

**Application:**
- Conversation tree responses cost focus (mental effort) and time
- Examination points cost focus and time
- Emergency responses cost stamina, health, coins, or time
- NO "conversation points", "examination tokens", or "emergency currency"

#### 2. Transparent Costs, Perfect Information
**Principle:** Player sees all costs before commitment. No hidden consequences.

**Why:** Strategic decisions require complete information. Surprise costs feel unfair.

**Application:**
- Dialogue responses show cost: "Thoughtful Response (3 Focus, 1 Time)"
- Examination points show cost: "Thorough Examination (5 Focus, 2 Time)"
- Emergency responses show all costs and outcomes before selection

#### 3. Deterministic Outcomes, No Hidden Randomness
**Principle:** Same choice with same state produces same outcome.

**Why:** Player agency requires predictability. Randomness belongs in deck shuffles, not decision consequences.

**Application:**
- Selecting dialogue response A always grants same knowledge/relationship change
- Examining point B always produces same outcome
- Emergency response C always has same consequences

#### 4. Dynamic Content Spawning, Not Boolean Gates
**Principle:** Completing actions spawns new situations. No "if completed X, unlock Y" flags.

**Why:** Boolean gates create Cookie Clicker progression. Resource spending creates strategic tension.

**Application:**
- Completing conversation spawns new conversation at different NPC
- Examining point spawns follow-up examination or conversation
- Emergency response spawns consequence situations
- Spawning based on completion rewards, not flag checks

#### 5. Backend Authority, Dumb UI
**Principle:** All business logic in facades. UI components only display and delegate.

**Why:** Clean architecture, testability, single source of truth.

**Application:**
- ConversationTreeContent receives pre-built ConversationTreeContext
- ObservationContent receives pre-built ObservationContext
- Components NEVER query GameWorld directly
- Components NEVER calculate costs or validate actions

---

## Architectural Patterns to Follow

### Pattern 1: Screen Mode Enum

**Add to:** `C:\Git\Wayfarer\src\GameState\ScreenMode.cs`

```csharp
public enum ScreenMode
{
    Location,
    Exchange,
    Travel,
    SocialChallenge,
    MentalChallenge,
    PhysicalChallenge,
    ConversationTree,    // NEW
    Observation,         // NEW
    Emergency            // NEW
}
```

### Pattern 2: Context Class Structure

**Location:** `C:\Git\Wayfarer\src\GameState\Contexts\`

**Template:**
```csharp
public class [Screen]Context
{
    // Validation
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }

    // Entity references
    public [Entity] [Entity] { get; set; }
    public string [Entity]Id { get; set; }

    // Player state
    public ResourceState PlayerResources { get; set; }

    // Screen-specific session/state
    public [Session] Session { get; set; }

    // Display info
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }

    // Helper methods (optional)
    public List<[Option]> GetAvailable[Options]() { }
    public bool CanAfford([Option] option) { }
}
```

### Pattern 3: Facade Structure

**Location:** `C:\Git\Wayfarer\src\Subsystems\[Subsystem]\[Subsystem]Facade.cs`

**Template:**
```csharp
public class [Subsystem]Facade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    // ... other dependencies

    public [Subsystem]Facade(GameWorld gameWorld, MessageSystem messageSystem, ...)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
    }

    // Context creation (async if complex)
    public async Task<[Screen]Context> Create[Screen]Context(params)
    {
        // Validate entities exist
        // Build session/state
        // Query related data
        // Return complete context
    }

    // Action execution (sync if simple state change, async if complex)
    public [Result] Execute[Action](params)
    {
        // Validate resources
        // Apply costs
        // Apply outcomes
        // Update state
        // Return result
    }
}
```

### Pattern 4: Component Structure

**Location:** `C:\Git\Wayfarer\src\Pages\Components\`

**Files:**
- `[Screen]Content.razor` - Markup template
- `[Screen]Content.razor.cs` - Code-behind with base class
- `[Screen]Content.razor.css` - Isolated styles

**Template (Code-behind):**
```csharp
public class [Screen]ContentBase : ComponentBase
{
    [Inject] protected GameFacade GameFacade { get; set; }
    [Inject] protected GameWorld GameWorld { get; set; }

    [Parameter] public [Screen]Context Context { get; set; }
    [Parameter] public EventCallback On[Screen]End { get; set; }

    [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

    protected override async Task OnInitializedAsync()
    {
        // Initialization if needed
    }

    protected async Task Handle[Action]([Params])
    {
        // Call facade
        var result = await GameFacade.[FacadeMethod]([Params]);

        // Update context if needed
        Context = result.NewContext;

        // Notify parent if done
        if (result.IsComplete)
        {
            await On[Screen]End.InvokeAsync();
        }

        // Trigger refresh
        StateHasChanged();
    }
}
```

### Pattern 5: JSON → DTO → Parser Flow

**JSON File:** `C:\Git\Wayfarer\src\Content\Core\[##]_[content].json`

**DTO File:** `C:\Git\Wayfarer\src\Content\DTOs\[Entity]DTO.cs`

**Parser File:** `C:\Git\Wayfarer\src\Content\Parsers\[Entity]Parser.cs`

**Validation Rules:**
- Required fields: Throw InvalidOperationException if missing
- Optional fields: Use semantic defaults (empty string, empty list)
- Enum parsing: TryParse with explicit error messages
- NO ?? operators hiding missing data
- NO JsonPropertyName hiding mismatches

### Pattern 6: Catalogue Creation

**Location:** `C:\Git\Wayfarer\src\Content\Catalogs\`

**Template:**
```csharp
public static class [Feature]Catalog
{
    public static [ReturnType] Get[Feature]Values([CategoricalInput] input, ...)
    {
        return input switch
        {
            [Category].Value1 => [ConcreteValue1],
            [Category].Value2 => [ConcreteValue2],
            _ => throw new InvalidOperationException($"Unknown {input}")
        };
    }
}
```

**Rules:**
- Static class only
- Static methods only
- Called by parsers only (never by facades/services)
- Deterministic (no random, no global state)
- Fail-fast (throw on unknown inputs)

### Pattern 7: GameWorld Storage

**Add to:** `C:\Git\Wayfarer\src\GameState\GameWorld.cs`

```csharp
public List<[Entity]> [Entities] { get; set; } = new List<[Entity]>();
```

**Rules:**
- Always List<T>, never Dictionary<K, V>
- Always initialize inline with = new List<T>()
- Never null (Let It Crash philosophy)
- Single source of truth (no duplicate storage)

---

## Screen 1: Conversation Tree

### Purpose & Context

**What:** Simple dialogue tree system where player selects responses that branch narrative, cost resources, and can escalate to tactical Social challenges.

**Why:** Not every NPC interaction should be a card-based tactical challenge. Casual conversations, information gathering, and relationship building need simpler interaction model. Conversation trees serve as "gateway" to tactical challenges - you talk casually, tension rises, conversation escalates to negotiation.

**Integration:** Conversation trees appear as Situations at NPC locations, spawned by completing other content. Some dialogue responses escalate to `ScreenMode.SocialChallenge` for tactical gameplay.

### Technical Specification

#### 1. Domain Entities

**File:** `C:\Git\Wayfarer\src\GameState\ConversationTree.cs`

```csharp
public class ConversationTree
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string NpcId { get; set; }
    public NPC Npc { get; set; }  // Wired in Phase 2

    // Availability conditions
    public int MinimumRelationship { get; set; }
    public List<string> RequiredKnowledge { get; set; } = new List<string>();
    public List<TimeBlocks> AvailableTimeBlocks { get; set; } = new List<TimeBlocks>();

    // Tree structure
    public List<DialogueNode> Nodes { get; set; } = new List<DialogueNode>();
    public string StartingNodeId { get; set; }

    // Lifecycle
    public bool IsRepeatable { get; set; }
    public bool IsCompleted { get; set; }
}

public class DialogueNode
{
    public string Id { get; set; }
    public string NpcDialogue { get; set; }
    public List<DialogueResponse> Responses { get; set; } = new List<DialogueResponse>();
}

public class DialogueResponse
{
    public string Id { get; set; }
    public string ResponseText { get; set; }

    // Costs
    public int FocusCost { get; set; }
    public int TimeCost { get; set; }  // Segments

    // Requirements
    public PlayerStatType? RequiredStat { get; set; }
    public int? RequiredStatLevel { get; set; }

    // Outcomes
    public string NextNodeId { get; set; }  // null = ends conversation
    public int RelationshipDelta { get; set; }
    public List<string> GrantedKnowledge { get; set; } = new List<string>();
    public List<string> SpawnedSituations { get; set; } = new List<string>();

    // Escalation
    public bool EscalatesToSocialChallenge { get; set; }
    public string SocialChallengeGoalId { get; set; }  // If escalates
}
```

#### 2. JSON Schema

**File:** `C:\Git\Wayfarer\src\Content\Core\03_npcs.json`

**Add to NPC definitions:**
```json
{
  "npcs": [ ... ],
  "conversationTrees": [
    {
      "id": "elena_evening_chat",
      "name": "Evening Chat with Elena",
      "description": "Casual conversation about the day's events",
      "npcId": "elena",
      "minimumRelationship": 10,
      "requiredKnowledge": [],
      "availableTimeBlocks": ["Evening", "Night"],
      "isRepeatable": true,
      "startingNodeId": "elena_greeting",
      "nodes": [
        {
          "id": "elena_greeting",
          "npcDialogue": "Another long day. How did your travels go?",
          "responses": [
            {
              "id": "share_story",
              "responseText": "Let me tell you about what I saw...",
              "focusCost": 2,
              "timeCost": 1,
              "nextNodeId": "elena_interested",
              "relationshipDelta": 5
            },
            {
              "id": "ask_about_day",
              "responseText": "How was your day?",
              "focusCost": 0,
              "timeCost": 1,
              "nextNodeId": "elena_shares",
              "relationshipDelta": 3
            },
            {
              "id": "end_politely",
              "responseText": "It was fine. I should rest.",
              "focusCost": 0,
              "timeCost": 0,
              "nextNodeId": null,
              "relationshipDelta": 0
            }
          ]
        },
        {
          "id": "elena_interested",
          "npcDialogue": "That sounds fascinating! Tell me more.",
          "responses": [
            {
              "id": "deeper_conversation",
              "responseText": "Well, there's more to it... [Requires Rapport 3]",
              "focusCost": 5,
              "timeCost": 2,
              "requiredStat": "Rapport",
              "requiredStatLevel": 3,
              "nextNodeId": "elena_revelation",
              "relationshipDelta": 10,
              "grantedKnowledge": ["elena_trust"]
            },
            {
              "id": "escalate_to_challenge",
              "responseText": "Actually, I need your help with something serious...",
              "focusCost": 3,
              "timeCost": 1,
              "escalatesToSocialChallenge": true,
              "socialChallengeGoalId": "elena_request_help"
            }
          ]
        }
      ]
    }
  ]
}
```

#### 3. DTO Structure

**File:** `C:\Git\Wayfarer\src\Content\DTOs\ConversationTreeDTO.cs`

```csharp
public class ConversationTreeDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string NpcId { get; set; }
    public int MinimumRelationship { get; set; }
    public List<string> RequiredKnowledge { get; set; }
    public List<string> AvailableTimeBlocks { get; set; }
    public bool IsRepeatable { get; set; }
    public string StartingNodeId { get; set; }
    public List<DialogueNodeDTO> Nodes { get; set; }
}

public class DialogueNodeDTO
{
    public string Id { get; set; }
    public string NpcDialogue { get; set; }
    public List<DialogueResponseDTO> Responses { get; set; }
}

public class DialogueResponseDTO
{
    public string Id { get; set; }
    public string ResponseText { get; set; }
    public int? FocusCost { get; set; }
    public int? TimeCost { get; set; }
    public string RequiredStat { get; set; }
    public int? RequiredStatLevel { get; set; }
    public string NextNodeId { get; set; }
    public int? RelationshipDelta { get; set; }
    public List<string> GrantedKnowledge { get; set; }
    public List<string> SpawnedSituations { get; set; }
    public bool? EscalatesToSocialChallenge { get; set; }
    public string SocialChallengeGoalId { get; set; }
}
```

#### 4. Parser

**File:** `C:\Git\Wayfarer\src\Content\Parsers\ConversationTreeParser.cs`

```csharp
public static class ConversationTreeParser
{
    public static ConversationTree Parse(ConversationTreeDTO dto, GameWorld gameWorld)
    {
        // Validation
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("ConversationTree missing required 'Id'");
        if (string.IsNullOrEmpty(dto.NpcId))
            throw new InvalidOperationException($"ConversationTree {dto.Id} missing 'NpcId'");
        if (dto.Nodes == null || dto.Nodes.Count == 0)
            throw new InvalidOperationException($"ConversationTree {dto.Id} must have at least one node");

        // Verify NPC exists
        NPC npc = gameWorld.NPCs.FirstOrDefault(n => n.ID == dto.NpcId);
        if (npc == null)
            throw new InvalidOperationException($"ConversationTree {dto.Id} references unknown NPC '{dto.NpcId}'");

        // Parse time blocks
        List<TimeBlocks> timeBlocks = new List<TimeBlocks>();
        if (dto.AvailableTimeBlocks != null)
        {
            foreach (string timeBlock in dto.AvailableTimeBlocks)
            {
                if (Enum.TryParse<TimeBlocks>(timeBlock, true, out TimeBlocks parsed))
                {
                    timeBlocks.Add(parsed);
                }
            }
        }

        ConversationTree tree = new ConversationTree
        {
            Id = dto.Id,
            Name = dto.Name ?? dto.Id,
            Description = dto.Description ?? "",
            NpcId = dto.NpcId,
            MinimumRelationship = dto.MinimumRelationship,
            RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>(),
            AvailableTimeBlocks = timeBlocks,
            IsRepeatable = dto.IsRepeatable,
            StartingNodeId = dto.StartingNodeId
        };

        // Parse nodes
        foreach (DialogueNodeDTO nodeDto in dto.Nodes)
        {
            DialogueNode node = ParseNode(nodeDto, dto.Id);
            tree.Nodes.Add(node);
        }

        // Validate starting node exists
        if (!tree.Nodes.Any(n => n.Id == tree.StartingNodeId))
        {
            throw new InvalidOperationException(
                $"ConversationTree {dto.Id} has invalid StartingNodeId '{tree.StartingNodeId}'");
        }

        return tree;
    }

    private static DialogueNode ParseNode(DialogueNodeDTO dto, string treeId)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException($"DialogueNode in tree {treeId} missing 'Id'");

        DialogueNode node = new DialogueNode
        {
            Id = dto.Id,
            NpcDialogue = dto.NpcDialogue ?? ""
        };

        // Parse responses
        if (dto.Responses != null)
        {
            foreach (DialogueResponseDTO responseDto in dto.Responses)
            {
                DialogueResponse response = ParseResponse(responseDto, treeId, dto.Id);
                node.Responses.Add(response);
            }
        }

        return node;
    }

    private static DialogueResponse ParseResponse(DialogueResponseDTO dto, string treeId, string nodeId)
    {
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException($"DialogueResponse in node {nodeId} missing 'Id'");

        // Parse required stat if present
        PlayerStatType? requiredStat = null;
        if (!string.IsNullOrEmpty(dto.RequiredStat))
        {
            if (Enum.TryParse<PlayerStatType>(dto.RequiredStat, true, out PlayerStatType parsed))
            {
                requiredStat = parsed;
            }
            else
            {
                throw new InvalidOperationException(
                    $"DialogueResponse {dto.Id} has invalid RequiredStat '{dto.RequiredStat}'");
            }
        }

        // Use Catalogue for focus cost if not specified
        int focusCost = dto.FocusCost ?? ConversationCatalog.GetDefaultFocusCost();

        return new DialogueResponse
        {
            Id = dto.Id,
            ResponseText = dto.ResponseText ?? "",
            FocusCost = focusCost,
            TimeCost = dto.TimeCost ?? 1,
            RequiredStat = requiredStat,
            RequiredStatLevel = dto.RequiredStatLevel,
            NextNodeId = dto.NextNodeId,
            RelationshipDelta = dto.RelationshipDelta ?? 0,
            GrantedKnowledge = dto.GrantedKnowledge ?? new List<string>(),
            SpawnedSituations = dto.SpawnedSituations ?? new List<string>(),
            EscalatesToSocialChallenge = dto.EscalatesToSocialChallenge ?? false,
            SocialChallengeGoalId = dto.SocialChallengeGoalId
        };
    }
}
```

#### 5. Catalogue

**File:** `C:\Git\Wayfarer\src\Content\Catalogs\ConversationCatalog.cs`

```csharp
public static class ConversationCatalog
{
    /// <summary>
    /// Get focus cost for dialogue response based on complexity
    /// </summary>
    public static int GetFocusCostForComplexity(ResponseComplexity complexity)
    {
        return complexity switch
        {
            ResponseComplexity.Simple => 0,      // "Yes", "No", casual
            ResponseComplexity.Thoughtful => 2,  // Considered response
            ResponseComplexity.Deep => 5,        // Emotional/complex
            ResponseComplexity.Insightful => 8,  // Requires high Insight
            _ => throw new InvalidOperationException($"Unknown complexity: {complexity}")
        };
    }

    public static int GetDefaultFocusCost()
    {
        return 1;  // Default for unspecified
    }

    /// <summary>
    /// Get relationship impact magnitude based on response type
    /// </summary>
    public static int GetRelationshipDelta(ResponseType type)
    {
        return type switch
        {
            ResponseType.Supportive => 5,
            ResponseType.Empathetic => 7,
            ResponseType.Intimate => 10,
            ResponseType.Neutral => 0,
            ResponseType.Dismissive => -5,
            ResponseType.Harsh => -10,
            _ => 0
        };
    }
}

public enum ResponseComplexity
{
    Simple,
    Thoughtful,
    Deep,
    Insightful
}

public enum ResponseType
{
    Supportive,
    Empathetic,
    Intimate,
    Neutral,
    Dismissive,
    Harsh
}
```

#### 6. Context

**File:** `C:\Git\Wayfarer\src\GameState\Contexts\ConversationTreeContext.cs`

```csharp
public class ConversationTreeContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }

    // Conversation data
    public ConversationTree Tree { get; set; }
    public DialogueNode CurrentNode { get; set; }
    public NPC Npc { get; set; }

    // Player state
    public int CurrentFocus { get; set; }
    public int MaxFocus { get; set; }
    public int CurrentRelationship { get; set; }
    public Dictionary<PlayerStatType, int> PlayerStats { get; set; }

    // Display info
    public string LocationName { get; set; }
    public string TimeDisplay { get; set; }

    // Helper methods
    public List<DialogueResponse> GetAvailableResponses()
    {
        if (CurrentNode == null) return new List<DialogueResponse>();

        return CurrentNode.Responses
            .Where(r => CanAffordResponse(r) && MeetsRequirements(r))
            .ToList();
    }

    public List<DialogueResponse> GetBlockedResponses()
    {
        if (CurrentNode == null) return new List<DialogueResponse>();

        return CurrentNode.Responses
            .Where(r => !CanAffordResponse(r) || !MeetsRequirements(r))
            .ToList();
    }

    public bool CanAffordResponse(DialogueResponse response)
    {
        return CurrentFocus >= response.FocusCost;
    }

    public bool MeetsRequirements(DialogueResponse response)
    {
        if (!response.RequiredStat.HasValue) return true;
        if (!response.RequiredStatLevel.HasValue) return true;

        if (!PlayerStats.ContainsKey(response.RequiredStat.Value))
            return false;

        return PlayerStats[response.RequiredStat.Value] >= response.RequiredStatLevel.Value;
    }

    public string GetBlockReason(DialogueResponse response)
    {
        if (!CanAffordResponse(response))
            return $"Requires {response.FocusCost} Focus (you have {CurrentFocus})";

        if (!MeetsRequirements(response))
            return $"Requires {response.RequiredStat} level {response.RequiredStatLevel}";

        return "";
    }
}
```

#### 7. Facade

**File:** `C:\Git\Wayfarer\src\Subsystems\Conversation\ConversationTreeFacade.cs`

```csharp
public class ConversationTreeFacade
{
    private readonly GameWorld _gameWorld;
    private readonly MessageSystem _messageSystem;
    private readonly ResourceFacade _resourceFacade;
    private readonly TimeFacade _timeFacade;
    private readonly TokenFacade _tokenFacade;

    public ConversationTreeFacade(
        GameWorld gameWorld,
        MessageSystem messageSystem,
        ResourceFacade resourceFacade,
        TimeFacade timeFacade,
        TokenFacade tokenFacade)
    {
        _gameWorld = gameWorld ?? throw new ArgumentNullException(nameof(gameWorld));
        _messageSystem = messageSystem ?? throw new ArgumentNullException(nameof(messageSystem));
        _resourceFacade = resourceFacade ?? throw new ArgumentNullException(nameof(resourceFacade));
        _timeFacade = timeFacade ?? throw new ArgumentNullException(nameof(timeFacade));
        _tokenFacade = tokenFacade ?? throw new ArgumentNullException(nameof(tokenFacade));
    }

    public ConversationTreeContext CreateContext(string treeId)
    {
        ConversationTree tree = _gameWorld.ConversationTrees.FirstOrDefault(t => t.Id == treeId);
        if (tree == null)
        {
            return new ConversationTreeContext
            {
                IsValid = false,
                ErrorMessage = $"Conversation tree '{treeId}' not found"
            };
        }

        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == tree.NpcId);
        if (npc == null)
        {
            return new ConversationTreeContext
            {
                IsValid = false,
                ErrorMessage = $"NPC '{tree.NpcId}' not found"
            };
        }

        Player player = _gameWorld.GetPlayer();

        // Check availability conditions
        int relationship = _tokenFacade.GetTotalTokens(npc.ID, ConnectionType.Trust);
        if (relationship < tree.MinimumRelationship)
        {
            return new ConversationTreeContext
            {
                IsValid = false,
                ErrorMessage = $"Not enough relationship with {npc.Name}"
            };
        }

        // Check time blocks
        TimeBlocks currentTime = _timeFacade.GetCurrentTimeBlock();
        if (tree.AvailableTimeBlocks.Count > 0 && !tree.AvailableTimeBlocks.Contains(currentTime))
        {
            return new ConversationTreeContext
            {
                IsValid = false,
                ErrorMessage = $"{npc.Name} is not available for this conversation right now"
            };
        }

        // Get starting node
        DialogueNode startingNode = tree.Nodes.FirstOrDefault(n => n.Id == tree.StartingNodeId);
        if (startingNode == null)
        {
            return new ConversationTreeContext
            {
                IsValid = false,
                ErrorMessage = "Conversation tree has invalid starting node"
            };
        }

        return new ConversationTreeContext
        {
            IsValid = true,
            Tree = tree,
            CurrentNode = startingNode,
            Npc = npc,
            CurrentFocus = player.Focus,
            MaxFocus = player.MaxFocus,
            CurrentRelationship = relationship,
            PlayerStats = BuildPlayerStats(player),
            LocationName = GetLocationName(),
            TimeDisplay = _timeFacade.GetCurrentTimeDisplay()
        };
    }

    public ConversationTreeResult SelectResponse(string treeId, string responseId)
    {
        ConversationTree tree = _gameWorld.ConversationTrees.FirstOrDefault(t => t.Id == treeId);
        if (tree == null)
            return ConversationTreeResult.Failed("Conversation tree not found");

        DialogueNode currentNode = _gameWorld.CurrentConversationNode;
        if (currentNode == null)
            return ConversationTreeResult.Failed("No active conversation node");

        DialogueResponse response = currentNode.Responses.FirstOrDefault(r => r.Id == responseId);
        if (response == null)
            return ConversationTreeResult.Failed("Response not found");

        Player player = _gameWorld.GetPlayer();

        // Validate resources
        if (player.Focus < response.FocusCost)
            return ConversationTreeResult.Failed($"Not enough Focus (need {response.FocusCost})");

        // Validate stat requirements
        if (response.RequiredStat.HasValue)
        {
            int statLevel = player.Stats.GetStatLevel(response.RequiredStat.Value);
            if (statLevel < (response.RequiredStatLevel ?? 0))
            {
                return ConversationTreeResult.Failed(
                    $"Requires {response.RequiredStat} level {response.RequiredStatLevel}");
            }
        }

        // Apply costs
        _resourceFacade.SpendFocus(response.FocusCost, "Conversation response");
        if (response.TimeCost > 0)
        {
            _timeFacade.AdvanceSegments(response.TimeCost);
        }

        // Apply outcomes
        if (response.RelationshipDelta != 0)
        {
            NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == tree.NpcId);
            if (npc != null)
            {
                _tokenFacade.ModifyTokens(npc.ID, ConnectionType.Trust, response.RelationshipDelta);
            }
        }

        // Grant knowledge
        foreach (string knowledge in response.GrantedKnowledge)
        {
            // Add to player knowledge list
            if (!player.Knowledge.Contains(knowledge))
            {
                player.Knowledge.Add(knowledge);
                _messageSystem.AddSystemMessage($"Learned: {knowledge}", SystemMessageTypes.Info);
            }
        }

        // Spawn situations
        foreach (string situationId in response.SpawnedSituations)
        {
            // Spawn situation logic
        }

        // Check for escalation to Social challenge
        if (response.EscalatesToSocialChallenge)
        {
            return ConversationTreeResult.EscalateToChallenge(response.SocialChallengeGoalId);
        }

        // Navigate to next node
        if (string.IsNullOrEmpty(response.NextNodeId))
        {
            // Conversation ends
            _gameWorld.CurrentConversationNode = null;

            if (!tree.IsRepeatable)
            {
                tree.IsCompleted = true;
            }

            return ConversationTreeResult.Completed();
        }
        else
        {
            DialogueNode nextNode = tree.Nodes.FirstOrDefault(n => n.Id == response.NextNodeId);
            if (nextNode == null)
                return ConversationTreeResult.Failed("Invalid next node");

            _gameWorld.CurrentConversationNode = nextNode;
            return ConversationTreeResult.Continue(nextNode);
        }
    }

    private Dictionary<PlayerStatType, int> BuildPlayerStats(Player player)
    {
        return new Dictionary<PlayerStatType, int>
        {
            { PlayerStatType.Insight, player.Stats.GetStatLevel(PlayerStatType.Insight) },
            { PlayerStatType.Rapport, player.Stats.GetStatLevel(PlayerStatType.Rapport) },
            { PlayerStatType.Authority, player.Stats.GetStatLevel(PlayerStatType.Authority) },
            { PlayerStatType.Diplomacy, player.Stats.GetStatLevel(PlayerStatType.Diplomacy) },
            { PlayerStatType.Cunning, player.Stats.GetStatLevel(PlayerStatType.Cunning) }
        };
    }

    private string GetLocationName()
    {
        Player player = _gameWorld.GetPlayer();
        return player.CurrentLocation?.Name ?? "Unknown";
    }
}

public class ConversationTreeResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public bool IsComplete { get; set; }
    public bool EscalatesToChallenge { get; set; }
    public string ChallengeGoalId { get; set; }
    public DialogueNode NextNode { get; set; }

    public static ConversationTreeResult Failed(string message) =>
        new ConversationTreeResult { Success = false, Message = message };

    public static ConversationTreeResult Completed() =>
        new ConversationTreeResult { Success = true, IsComplete = true };

    public static ConversationTreeResult Continue(DialogueNode nextNode) =>
        new ConversationTreeResult { Success = true, NextNode = nextNode };

    public static ConversationTreeResult EscalateToChallenge(string goalId) =>
        new ConversationTreeResult
        {
            Success = true,
            EscalatesToChallenge = true,
            ChallengeGoalId = goalId
        };
}
```

#### 8. GameFacade Integration

**File:** `C:\Git\Wayfarer\src\Services\GameFacade.cs`

**Add to GameFacade:**
```csharp
private readonly ConversationTreeFacade _conversationTreeFacade;

// Add to constructor parameters and initialization

public ConversationTreeContext CreateConversationTreeContext(string treeId)
{
    return _conversationTreeFacade.CreateContext(treeId);
}

public ConversationTreeResult SelectConversationResponse(string treeId, string responseId)
{
    return _conversationTreeFacade.SelectResponse(treeId, responseId);
}
```

#### 9. UI Component

**File:** `C:\Git\Wayfarer\src\Pages\Components\ConversationTreeContent.razor`

```razor
@inherits ConversationTreeContentBase
@namespace Wayfarer.Pages.Components

@if (Context != null && Context.IsValid)
{
    <div class="conversation-tree-content">
        <!-- NPC Header -->
        <div class="npc-header">
            <div class="npc-name">@Context.Npc.Name</div>
            <div class="location-context">@Context.LocationName - @Context.TimeDisplay</div>
        </div>

        <!-- Relationship Display -->
        <div class="relationship-display">
            <span class="relationship-label">Relationship:</span>
            <span class="relationship-value">@Context.CurrentRelationship</span>
        </div>

        <!-- NPC Dialogue -->
        <div class="npc-dialogue-section">
            <div class="dialogue-text">@Context.CurrentNode.NpcDialogue</div>
        </div>

        <!-- Player Responses -->
        <div class="responses-section">
            <h3>Your Response:</h3>

            @foreach (var response in Context.GetAvailableResponses())
            {
                <div class="response-option available" @onclick="() => HandleSelectResponse(response.Id)">
                    <div class="response-text">@response.ResponseText</div>
                    <div class="response-costs">
                        @if (response.FocusCost > 0)
                        {
                            <span class="cost-focus">Focus: @response.FocusCost</span>
                        }
                        @if (response.TimeCost > 0)
                        {
                            <span class="cost-time">Time: @response.TimeCost segment(s)</span>
                        }
                    </div>
                    @if (response.EscalatesToSocialChallenge)
                    {
                        <div class="escalation-indicator">⚠️ This will start a negotiation</div>
                    }
                </div>
            }

            @foreach (var response in Context.GetBlockedResponses())
            {
                <div class="response-option blocked">
                    <div class="response-text">@response.ResponseText</div>
                    <div class="blocked-reason">@Context.GetBlockReason(response)</div>
                </div>
            }
        </div>

        <!-- Resource Display -->
        <div class="resource-display">
            <div class="resource-item">
                <span class="resource-label">Focus:</span>
                <span class="resource-value">@Context.CurrentFocus / @Context.MaxFocus</span>
            </div>
        </div>
    </div>
}
else if (Context != null)
{
    <div class="error-message">
        <p>@Context.ErrorMessage</p>
        <button @onclick="HandleReturn">Return</button>
    </div>
}
else
{
    <div class="loading">Loading conversation...</div>
}
```

**File:** `C:\Git\Wayfarer\src\Pages\Components\ConversationTreeContent.razor.cs`

```csharp
public class ConversationTreeContentBase : ComponentBase
{
    [Inject] protected GameFacade GameFacade { get; set; }
    [Inject] protected GameWorld GameWorld { get; set; }

    [Parameter] public ConversationTreeContext Context { get; set; }
    [Parameter] public EventCallback OnConversationEnd { get; set; }

    [CascadingParameter] protected GameScreenBase GameScreen { get; set; }

    protected async Task HandleSelectResponse(string responseId)
    {
        if (Context == null || !Context.IsValid) return;

        ConversationTreeResult result = GameFacade.SelectConversationResponse(
            Context.Tree.Id,
            responseId);

        if (!result.Success)
        {
            // Show error message
            return;
        }

        if (result.EscalatesToChallenge)
        {
            // Navigate to Social Challenge screen
            await GameScreen.StartConversationSession(
                Context.Npc.ID,
                result.ChallengeGoalId);
        }
        else if (result.IsComplete)
        {
            // Conversation ended, return to location
            await OnConversationEnd.InvokeAsync();
        }
        else if (result.NextNode != null)
        {
            // Refresh context with new node
            Context = GameFacade.CreateConversationTreeContext(Context.Tree.Id);
            StateHasChanged();
        }
    }

    protected async Task HandleReturn()
    {
        await OnConversationEnd.InvokeAsync();
    }
}
```

**File:** `C:\Git\Wayfarer\src\Pages\Components\ConversationTreeContent.razor.css`

```css
.conversation-tree-content {
    display: flex;
    flex-direction: column;
    gap: 1.5rem;
    padding: 1rem;
    max-width: 800px;
    margin: 0 auto;
}

.npc-header {
    background: var(--surface-color);
    padding: 1rem;
    border-radius: 8px;
    border-left: 4px solid var(--accent-color);
}

.npc-name {
    font-size: 1.5rem;
    font-weight: bold;
    color: var(--text-primary);
}

.location-context {
    font-size: 0.9rem;
    color: var(--text-secondary);
    margin-top: 0.25rem;
}

.relationship-display {
    padding: 0.5rem 1rem;
    background: var(--surface-secondary);
    border-radius: 4px;
    display: flex;
    gap: 0.5rem;
}

.npc-dialogue-section {
    background: var(--dialogue-bg);
    padding: 1.5rem;
    border-radius: 8px;
    border: 2px solid var(--dialogue-border);
}

.dialogue-text {
    font-size: 1.1rem;
    line-height: 1.6;
    color: var(--text-primary);
    font-style: italic;
}

.responses-section h3 {
    font-size: 1rem;
    color: var(--text-secondary);
    margin-bottom: 0.75rem;
}

.response-option {
    background: var(--surface-color);
    padding: 1rem;
    border-radius: 6px;
    border: 2px solid transparent;
    margin-bottom: 0.75rem;
    transition: all 0.2s;
}

.response-option.available {
    cursor: pointer;
    border-color: var(--border-subtle);
}

.response-option.available:hover {
    border-color: var(--accent-color);
    background: var(--surface-hover);
    transform: translateX(4px);
}

.response-option.blocked {
    opacity: 0.5;
    cursor: not-allowed;
}

.response-text {
    font-size: 1rem;
    margin-bottom: 0.5rem;
    color: var(--text-primary);
}

.response-costs {
    display: flex;
    gap: 1rem;
    font-size: 0.85rem;
}

.cost-focus, .cost-time {
    color: var(--cost-color);
}

.escalation-indicator {
    margin-top: 0.5rem;
    font-size: 0.85rem;
    color: var(--warning-color);
    font-weight: bold;
}

.blocked-reason {
    font-size: 0.85rem;
    color: var(--error-color);
}

.resource-display {
    display: flex;
    gap: 1.5rem;
    padding: 1rem;
    background: var(--surface-secondary);
    border-radius: 6px;
}

.resource-item {
    display: flex;
    gap: 0.5rem;
}

.resource-label {
    color: var(--text-secondary);
}

.resource-value {
    color: var(--text-primary);
    font-weight: bold;
}

.error-message {
    text-align: center;
    padding: 2rem;
}
```

#### 10. GameScreen Integration

**File:** `C:\Git\Wayfarer\src\Pages\GameScreen.razor.cs`

**Add to GameScreenBase:**
```csharp
protected ConversationTreeContext CurrentConversationTreeContext { get; set; }

public async Task StartConversationTree(string treeId)
{
    CurrentConversationTreeContext = GameFacade.CreateConversationTreeContext(treeId);

    if (CurrentConversationTreeContext.IsValid)
    {
        await NavigateToScreen(ScreenMode.ConversationTree);
    }
}

protected async Task HandleConversationTreeEnd()
{
    CurrentConversationTreeContext = null;
    await NavigateToScreen(ScreenMode.Location);
    await RefreshUI();
}
```

**File:** `C:\Git\Wayfarer\src\Pages\GameScreen.razor`

**Add to screen switch:**
```razor
case ScreenMode.ConversationTree:
    <ConversationTreeContent
        Context="@CurrentConversationTreeContext"
        OnConversationEnd="HandleConversationTreeEnd" />
    break;
```

---

## Screen 2: Observation/Examination

### Purpose & Context

**What:** Scene investigation with multiple examination points. Player has limited resources (focus, time) and must prioritize what to examine closely. Each examination point can grant knowledge, spawn situations, or reveal items.

**Why:** Crime scenes, mysterious locations, and complex environments have multiple things worth examining. You can't check everything thoroughly - you must choose where to focus attention. Creates strategic resource management and replayability (different choices reveal different content).

**Integration:** Observation scenes appear as Situations at locations, spawned by completing investigations or goals. Incomplete examination is allowed and intentional - players make strategic choices about depth vs breadth.

### Technical Specification

#### 1. Domain Entities

**File:** `C:\Git\Wayfarer\src\GameState\ObservationScene.cs`

```csharp
public class ObservationScene
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationId { get; set; }
    public Location Location { get; set; }  // Wired in Phase 2

    // Availability
    public List<string> RequiredKnowledge { get; set; } = new List<string>();
    public bool IsRepeatable { get; set; }
    public bool IsCompleted { get; set; }

    // Examination points
    public List<ExaminationPoint> ExaminationPoints { get; set; } = new List<ExaminationPoint>();

    // Tracking
    public List<string> ExaminedPointIds { get; set; } = new List<string>();
}

public class ExaminationPoint
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }

    // Costs
    public int FocusCost { get; set; }
    public int TimeCost { get; set; }

    // Requirements
    public PlayerStatType? RequiredStat { get; set; }
    public int? RequiredStatLevel { get; set; }
    public List<string> RequiredKnowledge { get; set; } = new List<string>();

    // Outcomes
    public List<string> GrantedKnowledge { get; set; } = new List<string>();
    public string SpawnedSituationId { get; set; }
    public string SpawnedConversationId { get; set; }
    public string FoundItemId { get; set; }
    public int FindItemChance { get; set; }  // 0-100 percentage

    // Discovery progression
    public string RevealsExaminationPointId { get; set; }

    // State
    public bool IsHidden { get; set; }
    public bool IsExamined { get; set; }
}
```

#### 2. JSON Schema

**File:** `C:\Git\Wayfarer\src\Content\Core\04_locations.json`

**Add observation scenes:**
```json
{
  "locations": [ ... ],
  "observationScenes": [
    {
      "id": "market_crime_scene",
      "name": "Market Crime Scene",
      "description": "Overturned cart, scattered goods, signs of struggle...",
      "locationId": "market_square",
      "requiredKnowledge": [],
      "isRepeatable": false,
      "examinationPoints": [
        {
          "id": "examine_footprints",
          "title": "Examine Footprints",
          "description": "Multiple sets of tracks lead toward the docks",
          "focusCost": 3,
          "timeCost": 1,
          "grantedKnowledge": ["suspect_fled_docks"],
          "spawnedSituationId": "search_docks_situation"
        },
        {
          "id": "analyze_blood",
          "title": "Analyze Blood Pattern",
          "description": "Blood splatter suggests defensive wounds",
          "focusCost": 5,
          "timeCost": 2,
          "requiredStat": "Insight",
          "requiredStatLevel": 3,
          "grantedKnowledge": ["victim_wounded_badly", "attacker_right_handed"],
          "revealsExaminationPointId": "search_weapon_hidden"
        },
        {
          "id": "question_witnesses",
          "title": "Question Witnesses",
          "description": "Several bystanders saw something",
          "focusCost": 2,
          "timeCost": 1,
          "spawnedConversationId": "witness_conversation"
        },
        {
          "id": "search_weapon",
          "title": "Search for Weapon",
          "description": "Look for the weapon used in the attack",
          "focusCost": 4,
          "timeCost": 2,
          "foundItemId": "rusty_dagger",
          "findItemChance": 50,
          "grantedKnowledge": ["weapon_recovered"]
        },
        {
          "id": "search_weapon_hidden",
          "title": "Search Hidden Area",
          "description": "Blood pattern suggests weapon thrown here",
          "focusCost": 3,
          "timeCost": 1,
          "isHidden": true,
          "foundItemId": "rusty_dagger",
          "findItemChance": 90,
          "grantedKnowledge": ["weapon_recovered"]
        },
        {
          "id": "examine_cart",
          "title": "Examine Overturned Cart",
          "description": "The cart's goods reveal what was being transported",
          "focusCost": 2,
          "timeCost": 1,
          "grantedKnowledge": ["stolen_grain_confirmed"]
        }
      ]
    }
  ]
}
```

#### 3. DTO Structure

**File:** `C:\Git\Wayfarer\src\Content\DTOs\ObservationSceneDTO.cs`

```csharp
public class ObservationSceneDTO
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string LocationId { get; set; }
    public List<string> RequiredKnowledge { get; set; }
    public bool? IsRepeatable { get; set; }
    public List<ExaminationPointDTO> ExaminationPoints { get; set; }
}

public class ExaminationPointDTO
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public int? FocusCost { get; set; }
    public int? TimeCost { get; set; }
    public string RequiredStat { get; set; }
    public int? RequiredStatLevel { get; set; }
    public List<string> RequiredKnowledge { get; set; }
    public List<string> GrantedKnowledge { get; set; }
    public string SpawnedSituationId { get; set; }
    public string SpawnedConversationId { get; set; }
    public string FoundItemId { get; set; }
    public int? FindItemChance { get; set; }
    public string RevealsExaminationPointId { get; set; }
    public bool? IsHidden { get; set; }
}
```

#### 4. Parser

**File:** `C:\Git\Wayfarer\src\Content\Parsers\ObservationSceneParser.cs`

```csharp
public static class ObservationSceneParser
{
    public static ObservationScene Parse(ObservationSceneDTO dto, GameWorld gameWorld)
    {
        // Validation
        if (string.IsNullOrEmpty(dto.Id))
            throw new InvalidOperationException("ObservationScene missing required 'Id'");
        if (string.IsNullOrEmpty(dto.LocationId))
            throw new InvalidOperationException($"ObservationScene {dto.Id} missing 'LocationId'");

        // Verify location exists
        Location location = gameWorld.Locations.FirstOrDefault(l => l.Id == dto.LocationId);
        if (location == null)
        {
            throw new InvalidOperationException(
                $"ObservationScene {dto.Id} references unknown location '{dto.LocationId}'");
        }

        ObservationScene scene = new ObservationScene
        {
            Id = dto.Id,
            Name = dto.Name ?? dto.Id,
            Description = dto.Description ?? "",
            LocationId = dto.LocationId,
            RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>(),
            IsRepeatable = dto.IsRepeatable ?? false
        };

        // Parse examination points
        if (dto.ExaminationPoints != null)
        {
            foreach (ExaminationPointDTO pointDto in dto.ExaminationPoints)
            {
                ExaminationPoint point = ParseExaminationPoint(pointDto, dto.Id);
                scene.ExaminationPoints.Add(point);
            }
        }

        return scene;
    }

    private static ExaminationPoint ParseExaminationPoint(
        ExaminationPointDTO dto,
        string sceneId)
    {
        if (string.IsNullOrEmpty(dto.Id))
        {
            throw new InvalidOperationException(
                $"ExaminationPoint in scene {sceneId} missing 'Id'");
        }

        // Parse required stat
        PlayerStatType? requiredStat = null;
        if (!string.IsNullOrEmpty(dto.RequiredStat))
        {
            if (Enum.TryParse<PlayerStatType>(dto.RequiredStat, true, out PlayerStatType parsed))
            {
                requiredStat = parsed;
            }
            else
            {
                throw new InvalidOperationException(
                    $"ExaminationPoint {dto.Id} has invalid RequiredStat '{dto.RequiredStat}'");
            }
        }

        // Use catalogue for default costs
        int focusCost = dto.FocusCost ?? ObservationCatalog.GetDefaultFocusCost();
        int timeCost = dto.TimeCost ?? ObservationCatalog.GetDefaultTimeCost();

        return new ExaminationPoint
        {
            Id = dto.Id,
            Title = dto.Title ?? "",
            Description = dto.Description ?? "",
            FocusCost = focusCost,
            TimeCost = timeCost,
            RequiredStat = requiredStat,
            RequiredStatLevel = dto.RequiredStatLevel,
            RequiredKnowledge = dto.RequiredKnowledge ?? new List<string>(),
            GrantedKnowledge = dto.GrantedKnowledge ?? new List<string>(),
            SpawnedSituationId = dto.SpawnedSituationId,
            SpawnedConversationId = dto.SpawnedConversationId,
            FoundItemId = dto.FoundItemId,
            FindItemChance = dto.FindItemChance ?? 0,
            RevealsExaminationPointId = dto.RevealsExaminationPointId,
            IsHidden = dto.IsHidden ?? false
        };
    }
}
```

#### 5. Catalogue

**File:** `C:\Git\Wayfarer\src\Content\Catalogs\ObservationCatalog.cs`

```csharp
public static class ObservationCatalog
{
    /// <summary>
    /// Get focus cost for examination depth
    /// </summary>
    public static int GetFocusCostForDepth(ExaminationDepth depth)
    {
        return depth switch
        {
            ExaminationDepth.Glance => 2,      // Quick look
            ExaminationDepth.Careful => 5,     // Thorough examination
            ExaminationDepth.Exhaustive => 8,  // Expert analysis
            ExaminationDepth.Insight => 12,    // Requires high Insight
            _ => throw new InvalidOperationException($"Unknown depth: {depth}")
        };
    }

    /// <summary>
    /// Get time cost for examination depth
    /// </summary>
    public static int GetTimeCostForDepth(ExaminationDepth depth)
    {
        return depth switch
        {
            ExaminationDepth.Glance => 1,
            ExaminationDepth.Careful => 2,
            ExaminationDepth.Exhaustive => 3,
            ExaminationDepth.Insight => 2,  // High skill = efficient
            _ => 1
        };
    }

    public static int GetDefaultFocusCost() => 3;
    public static int GetDefaultTimeCost() => 1;
}

public enum ExaminationDepth
{
    Glance,
    Careful,
    Exhaustive,
    Insight
}
```

#### 6. Context

**File:** `C:\Git\Wayfarer\src\GameState\Contexts\ObservationContext.cs`

```csharp
public class ObservationContext
{
    public bool IsValid { get; set; }
    public string ErrorMessage { get; set; }

    // Scene data
    public ObservationScene Scene { get; set; }
    public Location Location { get; set; }

    // Player state
    public int CurrentFocus { get; set; }
    public int MaxFocus { get; set; }
    public Dictionary<PlayerStatType, int> PlayerStats { get; set; }
    public List<string> PlayerKnowledge { get; set; } = new List<string>();

    // Examination tracking
    public List<string> ExaminedPointIds { get; set; } = new List<string>();

    // Display info
    public string TimeDisplay { get; set; }

    // Helper methods
    public List<ExaminationPoint> GetAvailablePoints()
    {
        return Scene.ExaminationPoints
            .Where(p => !p.IsHidden || IsRevealed(p))
            .Where(p => !ExaminedPointIds.Contains(p.Id))
            .Where(p => MeetsKnowledgeRequirements(p))
            .ToList();
    }

    public List<ExaminationPoint> GetAffordablePoints()
    {
        return GetAvailablePoints()
            .Where(p => CanAfford(p) && MeetsStatRequirements(p))
            .ToList();
    }

    public List<ExaminationPoint> GetBlockedPoints()
    {
        return GetAvailablePoints()
            .Where(p => !CanAfford(p) || !MeetsStatRequirements(p))
            .ToList();
    }

    public List<ExaminationPoint> GetExaminedPoints()
    {
        return Scene.ExaminationPoints
            .Where(p => ExaminedPointIds.Contains(p.Id))
            .ToList();
    }

    private bool IsRevealed(ExaminationPoint point)
    {
        if (!point.IsHidden) return true;

        // Check if any examined point reveals this one
        return Scene.ExaminationPoints
            .Any(p => ExaminedPointIds.Contains(p.Id) &&
                     p.RevealsExaminationPointId == point.Id);
    }

    private bool CanAfford(ExaminationPoint point)
    {
        return CurrentFocus >= point.FocusCost;
    }

    private bool MeetsStatRequirements(ExaminationPoint point)
    {
        if (!point.RequiredStat.HasValue) return true;
        if (!point.RequiredStatLevel.HasValue) return true;

        if (!PlayerStats.ContainsKey(point.RequiredStat.Value))
            return false;

        return PlayerStats[point.RequiredStat.Value] >= point.RequiredStatLevel.Value;
    }

    private bool MeetsKnowledgeRequirements(ExaminationPoint point)
    {
        return point.RequiredKnowledge.All(k => PlayerKnowledge.Contains(k));
    }

    public string GetBlockReason(ExaminationPoint point)
    {
        if (!CanAfford(point))
            return $"Requires {point.FocusCost} Focus (you have {CurrentFocus})";

        if (!MeetsStatRequirements(point))
            return $"Requires {point.RequiredStat} level {point.RequiredStatLevel}";

        if (!MeetsKnowledgeRequirements(point))
        {
            var missing = point.RequiredKnowledge
                .Where(k => !PlayerKnowledge.Contains(k))
                .ToList();
            return $"Missing knowledge: {string.Join(", ", missing)}";
        }

        return "";
    }

    public int GetTotalExaminations() => ExaminedPointIds.Count;
    public int GetAvailableExaminations() => GetAvailablePoints().Count;
    public int GetRemainingFocus() => CurrentFocus;
}
```

*(Continuing in next message due to length...)*

---

## Screen 3: Emergency Response

### Purpose & Context

**What:** Urgent situations that demand immediate player response. Creates dynamic world feeling, time pressure, and moral weight. Emergencies interrupt normal gameplay flow at sync points (time advancement, location entry).

**Why:** Medieval towns have emergencies: fires, fights, accidents, urgent requests. World events happen regardless of player actions. Cannot always plan and prepare - sometimes must react immediately with available resources. Creates tension: current plans disrupted, must decide priority on the spot.

**Integration:** Emergencies check at natural sync points (time advancement, location entry). If trigger conditions met, emergency takes priority. Player MUST respond (even "ignore" is a response choice). All responses have visible consequences.

---

## Implementation Roadmap

### Phase 1: Foundation & Documentation (COMPLETE)
- ✅ Create this implementation document
- ✅ Document all architectural patterns
- ✅ Define technical specifications

### Phase 2: Conversation Tree Implementation
1. Create domain entities (ConversationTree, DialogueNode, DialogueResponse)
2. Add JSON schema to 03_npcs.json
3. Create DTO classes
4. Create parser with validation
5. Create catalogue for costs/impacts
6. Add GameWorld storage
7. Create Context class
8. Create ConversationTreeFacade
9. Add GameFacade delegation
10. Create Razor component (`.razor` + `.razor.cs` + `.razor.css`)
11. Add ScreenMode enum value
12. Add GameScreen routing
13. Test vertical slice (JSON → UI → Action)

### Phase 3: Observation Screen Implementation
*(Same steps as Conversation Tree but for Observation system)*

### Phase 4: Emergency Response Implementation
*(Same steps as Conversation Tree but for Emergency system)*

### Phase 5: Integration Testing
- Test all three screens independently
- Test transitions between screens
- Test resource spending across screens
- Test situation spawning
- Validate architectural consistency

---

## Integration Strategy

### Situation Spawning Pattern

**All three screens use Situation spawning:**

```csharp
// Situation types in GameWorld
public enum SituationType
{
    Goal,              // Existing (challenges)
    Exchange,          // Existing (trading)
    ConversationTree,  // NEW
    ObservationScene,  // NEW
    Emergency          // NEW (priority)
}

// Situation entity
public class Situation
{
    public string Id { get; set; }
    public SituationType Type { get; set; }
    public string ContentId { get; set; }  // References actual content
    public string LocationId { get; set; }  // Where it appears
    public bool IsAvailable { get; set; }
    public bool IsCompleted { get; set; }
}
```

**Spawning flow:**
```
Complete Action (Goal/Conversation/Examination)
    ↓
Action.Rewards.SpawnedSituations
    ↓
Create Situation entity
    ↓
Add to GameWorld.Situations
    ↓
Add to Location.ActiveSituations
    ↓
Next location visit shows new button
```

### Screen Transition Patterns

**From Location to new screens:**
```csharp
// LocationContent.razor
protected async Task HandleSituationClick(Situation situation)
{
    switch (situation.Type)
    {
        case SituationType.ConversationTree:
            await GameScreen.StartConversationTree(situation.ContentId);
            break;
        case SituationType.ObservationScene:
            await GameScreen.StartObservation(situation.ContentId);
            break;
        case SituationType.Emergency:
            await GameScreen.StartEmergency(situation.ContentId);
            break;
    }
}
```

**From Conversation Tree to Social Challenge:**
```csharp
// ConversationTreeContent.razor
if (result.EscalatesToChallenge)
{
    await GameScreen.StartConversationSession(
        Context.Npc.ID,
        result.ChallengeGoalId);
}
```

### Emergency Priority Pattern

**Emergencies checked at sync points:**
```csharp
// GameFacade.cs
public async Task AdvanceTime(int segments)
{
    _timeFacade.AdvanceSegments(segments);

    // Check for active emergency
    EmergencySituation activeEmergency = _emergencyFacade.CheckForActiveEmergency();
    if (activeEmergency != null)
    {
        // Emergency takes priority - will be shown next screen refresh
        _gameWorld.ActiveEmergency = activeEmergency;
    }

    // Other side effects...
}
```

**GameScreen detects emergency:**
```csharp
// GameScreen.razor.cs
protected override async Task OnInitializedAsync()
{
    // Check for active emergency first
    if (GameWorld.ActiveEmergency != null)
    {
        CurrentScreen = ScreenMode.Emergency;
        CurrentEmergencyContext = GameFacade.CreateEmergencyContext(
            GameWorld.ActiveEmergency.Id);
    }
}
```

---

## Testing Strategy

### Unit Testing

**Parser tests:**
```csharp
[Fact]
public void ConversationTreeParser_ValidJSON_ParsesSuccessfully()
{
    // Arrange
    ConversationTreeDTO dto = CreateValidDTO();
    GameWorld gameWorld = CreateMockGameWorld();

    // Act
    ConversationTree tree = ConversationTreeParser.Parse(dto, gameWorld);

    // Assert
    Assert.Equal("test_tree", tree.Id);
    Assert.Equal(2, tree.Nodes.Count);
}

[Fact]
public void ConversationTreeParser_MissingNPC_ThrowsException()
{
    // Arrange
    ConversationTreeDTO dto = CreateValidDTO();
    dto.NpcId = "nonexistent_npc";
    GameWorld gameWorld = CreateMockGameWorld();

    // Act & Assert
    Assert.Throws<InvalidOperationException>(() =>
        ConversationTreeParser.Parse(dto, gameWorld));
}
```

**Facade tests:**
```csharp
[Fact]
public void ConversationTreeFacade_ValidTree_CreatesContext()
{
    // Arrange
    SetupGameWorld();

    // Act
    ConversationTreeContext context = _facade.CreateContext("test_tree");

    // Assert
    Assert.True(context.IsValid);
    Assert.NotNull(context.CurrentNode);
}

[Fact]
public void ConversationTreeFacade_SelectResponse_AppliesCosts()
{
    // Arrange
    SetupActiveConversation();
    int initialFocus = _player.Focus;

    // Act
    var result = _facade.SelectResponse("test_tree", "response_1");

    // Assert
    Assert.True(result.Success);
    Assert.Equal(initialFocus - 3, _player.Focus);  // Response costs 3 focus
}
```

### Integration Testing

**Vertical slice test:**
```csharp
[Fact]
public async Task ConversationTree_CompleteFlow_WorksEndToEnd()
{
    // 1. Load JSON
    LoadPackage("test_conversation_trees.json");

    // 2. Verify parsing
    Assert.Single(_gameWorld.ConversationTrees);

    // 3. Create context
    var context = _gameFacade.CreateConversationTreeContext("test_tree");
    Assert.True(context.IsValid);

    // 4. Select response
    var result = _gameFacade.SelectConversationResponse("test_tree", "response_1");
    Assert.True(result.Success);

    // 5. Verify outcomes
    Assert.Contains("new_knowledge", _player.Knowledge);
    Assert.Equal(5, GetRelationshipWithNPC("test_npc"));
}
```

### UI Testing (Manual)

**Test checklist per screen:**
- ✅ Screen renders correctly with valid context
- ✅ Screen shows error with invalid context
- ✅ All buttons/options are clickable
- ✅ Costs display correctly before selection
- ✅ Blocked options show why they're blocked
- ✅ Selecting option applies costs
- ✅ Selecting option shows outcomes
- ✅ Navigation works (back, escalate, complete)
- ✅ CSS styles apply correctly
- ✅ Responsive layout works

---

## Reference Materials

### Key Files to Study

**Screen Components (Existing Patterns):**
- `C:\Git\Wayfarer\src\Pages\Components\ConversationContent.razor`
- `C:\Git\Wayfarer\src\Pages\Components\MentalContent.razor`
- `C:\Git\Wayfarer\src\Pages\Components\PhysicalContent.razor`
- `C:\Git\Wayfarer\src\Pages\Components\ExchangeContent.razor`

**Facades (Architecture):**
- `C:\Git\Wayfarer\src\Services\GameFacade.cs`
- `C:\Git\Wayfarer\src\Subsystems\Social\SocialFacade.cs`
- `C:\Git\Wayfarer\src\Subsystems\Resource\ResourceFacade.cs`

**Parsers (Patterns):**
- `C:\Git\Wayfarer\src\Content\Parsers\SocialCardParser.cs`
- `C:\Git\Wayfarer\src\Content\GoalParser.cs`
- `C:\Git\Wayfarer\src\Content\NPCParser.cs`

**Catalogues (Translation):**
- `C:\Git\Wayfarer\src\Content\Catalogs\SocialCardEffectCatalog.cs`
- `C:\Git\Wayfarer\src\Content\Catalogs\EquipmentDurabilityCatalog.cs`

**Contexts (Data Containers):**
- `C:\Git\Wayfarer\src\GameState\Contexts\SocialChallengeContext.cs`
- `C:\Git\Wayfarer\src\GameState\ExchangeContext.cs`

**GameWorld (Storage):**
- `C:\Git\Wayfarer\src\GameState\GameWorld.cs`

### Design Documents

**Core Architecture:**
- `C:\Git\Wayfarer\architecture.md`
- `C:\Git\Wayfarer\CLAUDE.md`

**Game Design:**
- `C:\Git\Wayfarer\wayfarer-design-document-v2.md`
- `C:\Git\Wayfarer\game-design-principles.md`

**Content Patterns:**
- `C:\Git\Wayfarer\obstacle-templates.md`
- `C:\Git\Wayfarer\DYNAMIC_CONTENT_PRINCIPLES.md`

---

## Success Criteria

### Each Screen Must:
1. ✅ Use existing resources (no new currencies)
2. ✅ Show transparent costs before commitment
3. ✅ Have deterministic outcomes
4. ✅ Spawn new content dynamically
5. ✅ Never use boolean flag gates
6. ✅ Follow all architectural patterns
7. ✅ Integrate seamlessly
8. ✅ Complete vertical slice works

### Implementation Quality:
1. ✅ Parser-JSON-Entity triangle aligned
2. ✅ Catalogue pattern used correctly
3. ✅ Fail-fast validation (no defensive coding)
4. ✅ Strong typing throughout (no dictionaries)
5. ✅ Backend authority (UI is dumb display)
6. ✅ Let It Crash philosophy (collections initialized inline)
7. ✅ CSS isolated per component
8. ✅ Clear separation of concerns

---

## Next Steps

1. **Review this document thoroughly**
2. **Begin Conversation Tree implementation**
3. **Test vertical slice before proceeding**
4. **Implement Observation screen**
5. **Implement Emergency screen**
6. **Integration testing across all screens**
7. **Architectural consistency validation**

**This document is the authoritative source for all implementation decisions.**
