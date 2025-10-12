# WAYFARER SYSTEM ARCHITECTURE

**CRITICAL: This document MUST be read and understood before making ANY changes to the Wayfarer codebase.**

## TABLE OF CONTENTS

1. [System Overview](#system-overview)
2. [Content Pipeline Architecture](#content-pipeline-architecture)
3. [GameWorld State Management](#gameworld-state-management)
4. [Service & Subsystem Layer](#service--subsystem-layer)
5. [UI Component Architecture](#ui-component-architecture)
6. [Data Flow Patterns](#data-flow-patterns)
7. [Critical Architectural Principles](#critical-architectural-principles)
8. [Component Dependencies](#component-dependencies)

---

## SYSTEM OVERVIEW

Wayfarer is a **low-fantasy tactical RPG** with **three parallel challenge systems** (Social, Mental, Physical) built with **C# ASP.NET Core** and **Blazor Server**. The architecture follows **clean architecture principles** with strict **dependency inversion** and **single responsibility** patterns.

### Core Design Philosophy
- **GameWorld as Single Source of Truth**: All game state flows through GameWorld with zero external dependencies
- **Three Parallel Tactical Systems**: Social (conversations), Mental (investigations), Physical (obstacles) with equivalent depth
- **Strategic-Tactical Bridge**: Goals are first-class entities that connect strategic planning to tactical execution
- **Static Content Loading**: JSON content parsed once at startup without DI dependencies
- **Facade Pattern**: Business logic coordinated through specialized facades
- **Authoritative UI Pattern**: GameScreen owns all UI state, children communicate upward
- **No Shared Mutable State**: Services provide operations, not state storage

---

## CONTENT PIPELINE ARCHITECTURE

### 1. JSON Content Structure

**Location**: `src/Content/Core/*.json`

**Package Loading Order** (numbered files loaded alphabetically):
```
01_foundation.json     → Player stats, time blocks, base configuration
03_npcs.json          → NPC definitions with personalities and initial tokens
04_connections.json   → Routes and travel connections
05_goals.json         → Goal definitions (strategic-tactical bridge)
06_gameplay.json      → Venues, locations, game rules
07_equipment.json     → Items and equipment definitions
08_social_cards.json  → Social challenge cards (conversations)
09_mental_cards.json  → Mental challenge cards (investigations)
10_physical_cards.json → Physical challenge cards (obstacles)
12_challenge_decks.json → Deck configurations for all three systems
13_investigations.json  → Multi-phase investigation templates
14_knowledge.json      → Knowledge entries and discovery system
```

**Content Relationships**:
- **Goals**: First-class entities with `npcId` OR `locationId` for assignment
- **Challenge Decks**: Reference card IDs for Social/Mental/Physical systems
- **Investigations**: Reference goals via phase definitions
- **Cards**: Bind to unified 5-stat system (Insight/Rapport/Authority/Diplomacy/Cunning)
- All relationships use string IDs for loose coupling

### 2. Static Parser Layer

**Location**: `src/Content/*Parser.cs`

**Parser Responsibilities**:
```csharp
SocialCardParser     → SocialCardDTO → SocialCard
MentalCardParser     → MentalCardDTO → MentalCard
PhysicalCardParser   → PhysicalCardDTO → PhysicalCard
NPCParser            → NPCDTO → NPC
VenueParser          → VenueDTO → Venue
LocationParser       → LocationDTO → Location
GoalParser           → GoalDTO → Goal
InvestigationParser  → InvestigationDTO → Investigation
KnowledgeParser      → KnowledgeDTO → Knowledge
```

**CRITICAL PARSER PRINCIPLES**:
- **PARSE AT THE BOUNDARY**: JSON artifacts NEVER pollute domain layer
- **NO JsonElement PASSTHROUGH**: Parsers MUST convert to strongly-typed objects
- **NO Dictionary<string, object>**: Use proper typed properties on domain models
- **JSON FIELD NAMES MUST MATCH C# PROPERTIES**: No JsonPropertyName attributes to hide mismatches
- **STATELESS**: Parsers are static classes with no side effects
- **SINGLE PASS**: Each parser converts DTO to domain entity in one operation

### 3. Content Loading Orchestration

**Location**: `src/Content/PackageLoader.cs` & `GameWorldInitializer.cs`

**Loading Sequence**:
```
Startup → GameWorldInitializer.CreateGameWorld()
       → PackageLoader.LoadContent()
       → Static Parsers (for each content type)
       → Domain Objects → GameWorld Population
```

**Initialization Architecture**:
- `GameWorldInitializer` is **STATIC** - no DI dependencies
- Creates GameWorld **BEFORE** service registration completes
- Prevents circular dependencies during ServerPrerendered mode
- Content loaded once at startup, never reloaded

---

## GAMEWORLD STATE MANAGEMENT

**Location**: `src/GameState/GameWorld.cs`

### GameWorld Responsibilities

**State Collections**:
```csharp
// Core Entities
public List<NPC> NPCs { get; set; }
public List<Venue> Venues { get; set; }
public List<LocationEntry> Locations { get; set; }
private Player Player { get; set; }

// Three Parallel Tactical Systems - Card Templates
public List<SocialCard> SocialCards { get; set; }
public List<MentalCard> MentalCards { get; set; }
public List<PhysicalCard> PhysicalCards { get; set; }

// Three Parallel Tactical Systems - Challenge Decks
public Dictionary<string, SocialChallengeDeck> SocialChallengeDecks { get; }
public Dictionary<string, MentalChallengeDeck> MentalChallengeDecks { get; }
public Dictionary<string, PhysicalChallengeDeck> PhysicalChallengeDecks { get; }

// Strategic-Tactical Bridge
public Dictionary<string, Goal> Goals { get; }

// Investigation System
public List<Investigation> Investigations { get; }
public InvestigationJournal InvestigationJournal { get; }
public Dictionary<string, Knowledge> Knowledge { get; }

// Player Stats System
public List<PlayerStatDefinition> PlayerStatDefinitions { get; set; }
public StatProgression StatProgression { get; set; }
```

**State Operations**:
```csharp
GetPlayer() → Single player instance
GetPlayerResourceState() → Current player resources
GetLocation(string locationId) → Location by ID
GetAvailableStrangers() → NPCs available at venue/time
RefreshStrangersForTimeBlock() → Time-based NPC availability
ApplyInitialPlayerConfiguration() → Apply starting conditions from JSON
```

### CRITICAL GAMEWORLD PRINCIPLES

**1. Zero External Dependencies**
- GameWorld NEVER depends on services, managers, or external components
- All dependencies flow **INWARD** toward GameWorld, never outward
- GameWorld does **NOT** create any managers or services

**2. Single Source of Truth**
- ALL game state lives in GameWorld - no parallel state in services
- Services read/write to GameWorld but don't maintain their own copies
- NO SharedData dictionaries or TempData storage

**3. No Business Logic**
- GameWorld contains **STATE**, not **BEHAVIOR**
- Business logic belongs in services/facades, not GameWorld
- GameWorld provides data access, not game rules

---

## SERVICE & SUBSYSTEM LAYER

**Location**: `src/Services/GameFacade.cs` & `src/Subsystems/*/`

### Service Architecture Hierarchy

```
GameFacade (Pure Orchestrator)
├── THREE PARALLEL TACTICAL SYSTEMS
│   ├── SocialFacade (Social challenges - conversations with NPCs)
│   ├── MentalFacade (Mental challenges - investigations at locations)
│   └── PhysicalFacade (Physical challenges - obstacles at locations)
├── SUPPORTING SYSTEMS
│   ├── LocationFacade (Movement and spot management)
│   ├── ResourceFacade (Health, hunger, coins, stamina)
│   ├── TimeFacade (Time progression and segments)
│   ├── TravelFacade (Route management and travel)
│   ├── TokenFacade (Relationship tokens)
│   ├── NarrativeFacade (Messages and observations)
│   └── ExchangeFacade (NPC trading system)
└── INVESTIGATION & CONTENT
    ├── InvestigationActivity (Multi-phase investigation management)
    ├── InvestigationDiscoveryEvaluator (Discovery trigger evaluation)
    ├── KnowledgeService (Knowledge grants and discovery)
    └── GoalCompletionHandler (Goal completion and rewards)
```

### Facade Responsibilities

**GameFacade** - Pure orchestrator for UI-Backend communication
- Delegates ALL business logic to specialized facades
- Coordinates cross-facade operations (e.g., completing goals triggers investigations)
- Handles UI-specific orchestration
- NO business logic - only coordination

**Three Parallel Tactical System Facades** - Equivalent depth challenge systems
- `SocialFacade`: Initiative/Momentum/Doubt/Cadence conversation mechanics with NPCs
- `MentalFacade`: Progress/Attention/Exposure/Leads investigation mechanics at locations
- `PhysicalFacade`: Breakthrough/Exertion/Danger/Aggression obstacle mechanics at locations
- Each follows same architectural pattern: Builder/Threshold/Session resources + Binary actions
- All three systems use unified 5-stat progression (Insight/Rapport/Authority/Diplomacy/Cunning)

**Supporting Facades** - Game systems that integrate with tactical challenges
- `ExchangeFacade`: Separate NPC trading system (instant exchanges, not conversations)
- `LocationFacade`: Movement validation, location properties, spot management
- `ResourceFacade`: Permanent resources (Health, Stamina, Hunger, Focus, Coins)
- Each facade encapsulates ONE business domain

### Subsystem Organization

**Location**: `src/Subsystems/[Domain]/`

```
THREE PARALLEL TACTICAL SYSTEMS (Equivalent Depth)
Social/         → Social challenges: Card mechanics, momentum, conversation sessions
Mental/         → Mental challenges: Investigation mechanics, leads, observation system
Physical/       → Physical challenges: Obstacle mechanics, aggression, combo execution

SUPPORTING SYSTEMS
Exchange/       → NPC trading, inventory validation
Location/       → Movement, spot properties, location actions
Resource/       → Health, hunger, coins, stamina, focus management
Time/           → Segment progression, time block transitions
Token/          → Relationship tokens, connection tracking
Travel/         → Route discovery, travel validation
Narrative/      → Message system, observation rewards

INVESTIGATION & CONTENT SYSTEMS
Investigation/  → Multi-phase investigation lifecycle, goal spawning
Knowledge/      → Knowledge discovery, secrets, world state changes
Goals/          → Strategic-tactical bridge, victory conditions
```

---

## UI COMPONENT ARCHITECTURE

**Location**: `src/Pages/*.razor` & `src/Pages/Components/*.razor`

### Authoritative Page Pattern

**GameScreen.razor** - Single authoritative parent
- Owns ALL screen state and manages child components directly
- Provides outer structure (resources bar, headers, time display)
- Child components rendered INSIDE GameScreen's container
- Children call parent methods directly via CascadingValue

**Child Components** - Screen-specific content only
```
THREE PARALLEL TACTICAL SYSTEMS
ConversationContent.razor  → Social challenges (conversations with NPCs)
MentalContent.razor        → Mental challenges (investigations at locations)
PhysicalContent.razor      → Physical challenges (obstacles at locations)

SUPPORTING SCREENS
LocationContent.razor      → Location exploration UI
ExchangeContent.razor      → NPC trading interface
TravelContent.razor        → Route selection and travel

INVESTIGATION MODALS
InvestigationDiscoveryModal.razor  → Investigation discovery notifications
InvestigationActivationModal.razor → Investigation intro completion
InvestigationProgressModal.razor   → Phase completion notifications
InvestigationCompleteModal.razor   → Investigation completion rewards
```

### Component Communication Pattern

**Direct Parent-Child Communication**:
```csharp
// Child receives parent reference
<CascadingValue Value="@this">
  <LocationContent OnActionExecuted="RefreshUI" />
</CascadingValue>

// Child calls parent methods directly
GameScreen.StartConversation(npcId, requestId)
GameScreen.NavigateToQueue()
GameScreen.HandleTravelRoute(routeId)
```

**Context Objects for Complex State**:
```csharp
THREE PARALLEL TACTICAL SYSTEMS
SocialChallengeContext   → Complete conversation state (Social challenge)
MentalSession            → Investigation state (Mental challenge)
PhysicalSession          → Obstacle state (Physical challenge)

SUPPORTING CONTEXTS
ExchangeContext          → NPC trading session state
TravelDestinationViewModel → Route and destination display state
LocationScreenViewModel  → Location exploration state
```

### CRITICAL UI PRINCIPLES

**1. UI is Dumb Display Only**
- NO game logic in Razor components
- NO attention costs or availability logic in UI
- Backend determines ALL game mechanics through facades

**2. No Shared Mutable State**
- Services provide operations, NOT state storage
- NavigationCoordinator handles navigation ONLY, not data passing
- State lives in components, not services

**3. Screen Component Constraints**
- Screen components NEVER define their own game-container or headers
- GameScreen provides outer structure, children provide content only
- Screen components wrapped in semantic classes like 'conversation-content'

---

## DATA FLOW PATTERNS

### Complete Pipeline Flow

```
JSON Files (Content Definition)
    ↓ [Static Parsers]
Domain Models (Strongly Typed Objects)
    ↓ [GameWorldInitializer]
GameWorld (Single Source of Truth)
    ↓ [Service Facades]
Business Logic Operations
    ↓ [Context Objects]
UI Components (User Interface Display)
    ↓ [User Actions]
Service Facades (State Updates)
    ↓ [GameWorld Updates]
State Persistence
```

### Request/Response Flow

**User Action → UI Response**:
```
1. User clicks UI element
2. Component calls GameScreen method
3. GameScreen calls GameFacade method
4. GameFacade orchestrates subsystem facades
5. Facades execute business logic
6. Facades update GameWorld state
7. GameScreen refreshes UI with new state
```

### Context Creation Pattern

**Complex Operations Use Dedicated Contexts**:
```csharp
// Context created atomically BEFORE navigation
ConversationContext context = await GameFacade.CreateConversationContext(npcId, requestId);

// Context contains ALL data needed for operation
context.NpcInfo = npc data
context.LocationInfo = current location
context.PlayerResources = resource state
context.Session = conversation session

// Context passed as single parameter to child component
<ConversationContent Context="@context" />
```

---

## CRITICAL ARCHITECTURAL PRINCIPLES

### 1. Dependency Inversion
- **All dependencies flow INWARD toward GameWorld**
- GameWorld has zero external dependencies
- Services depend on GameWorld, not vice versa
- UI depends on services, services never depend on UI

### 2. Single Responsibility
- Each facade handles exactly ONE business domain
- GameWorld provides state access, not business logic
- UI components render state, don't contain game rules
- Parsers convert data formats, don't store state

### 3. Immutable Content Pipeline
- JSON content loaded once at startup
- Static parsers create immutable domain objects
- Content never reloaded or modified at runtime
- Domain models are data containers, not active objects

### 4. Clean Architecture Boundaries
```
UI Layer          → GameScreen, Components (Blazor)
Application Layer → GameFacade, Specialized Facades
Domain Layer      → GameWorld, Domain Models
Infrastructure    → Parsers, JSON Files
```

### 5. No Abstraction Over-Engineering
- **NO interfaces unless absolutely necessary**
- **NO inheritance hierarchies** - use composition
- **NO abstract base classes** - keep code direct
- Concrete classes only, straightforward implementations

### 6. State Isolation
- **NO parallel state tracking** across multiple objects
- When duplicate state found, identify single source of truth
- Other objects delegate to canonical source
- **NO caching layers** that can become stale

---

## COMPONENT DEPENDENCIES

### Core Dependencies Graph

```
GameScreen
├── Requires: GameFacade
├── Manages: All screen navigation
└── Children: LocationContent, ConversationContent, etc.

GameFacade
├── Requires: GameWorld + All Specialized Facades
├── Provides: Orchestration layer
└── Dependencies: ConversationFacade, LocationFacade, etc.

Specialized Facades
├── Require: GameWorld + Domain-specific managers
├── Provide: Business logic for specific domains
└── Update: GameWorld state through operations

GameWorld
├── Requires: Nothing (zero external dependencies)
├── Provides: Single source of truth for all state
└── Contains: All domain model collections
```

### Service Registration Pattern

```csharp
// GameWorld created BEFORE service registration
GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();

// Services registered with GameWorld dependency
services.AddSingleton(gameWorld);
services.AddScoped<ConversationFacade>();
services.AddScoped<GameFacade>();
```

### Critical Integration Points

**1. Three Parallel Tactical Systems Integration**
- **SocialFacade**: Social challenges (conversations) at NPCs
  - Initiative/Momentum/Doubt/Cadence resource system
  - Integrates with TokenFacade for relationship tokens
  - Integrates with KnowledgeService for knowledge/secrets
  - Personality rules (Proud, Devoted, Mercantile, Cunning, Steadfast)

- **MentalFacade**: Mental challenges (investigations) at Locations
  - Progress/Attention/Exposure/Leads resource system
  - Pauseable session model (can leave and return)
  - Location properties (Delicate, Obscured, Layered, Time-Sensitive, Resistant)

- **PhysicalFacade**: Physical challenges (obstacles) at Locations
  - Breakthrough/Exertion/Danger/Aggression resource system
  - One-shot session model (must complete in single attempt)
  - Challenge types (Combat, Athletics, Finesse, Endurance, Strength)

**2. Goals as Strategic-Tactical Bridge**
- Goals are first-class entities in `GameWorld.Goals` dictionary
- Goals assigned to NPCs (`NPC.ActiveGoals`) for Social challenges
- Goals assigned to Locations (`Location.ActiveGoals`) for Mental/Physical challenges
- Investigations dynamically spawn goals when phase requirements met
- GoalCards define tiered victory conditions (8/12/16 momentum thresholds)

**3. Investigation System Integration**
- InvestigationActivity manages multi-phase lifecycle (Potential → Discovered → Active → Complete)
- Discovery triggers: ImmediateVisibility, EnvironmentalObservation, Conversational, Item, Obligation
- Dynamic goal spawning: Phase completion spawns next phase's goals at NPCs/Locations
- Knowledge system: Investigations grant knowledge that unlocks future phases

**4. Unified 5-Stat Progression**
- All cards across all three systems bind to: Insight/Rapport/Authority/Diplomacy/Cunning
- Stat levels determine card depth access (Level 1: depths 1-2, Level 3: depths 1-4, etc.)
- Playing cards grants XP to bound stat
- Stats manifest differently per system (Insight = pattern recognition in Mental, structural analysis in Physical, reading people in Social)

**5. Location System Integration**
- LocationFacade coordinates NPC placement at locations
- Integrates with TravelFacade for route validation
- Coordinates with TimeFacade for time-based availability
- Manages ActiveGoals for Mental/Physical challenges

**6. Resource System Integration**
- ResourceFacade manages permanent resources (Health, Stamina, Focus, Hunger, Coins)
- **Mental challenges cost Focus** (concentration depletes)
- **Physical challenges cost Health + Stamina** (injury risk + exertion)
- **Social challenges cost nothing permanent** (but take time)
- Integrates with TimeFacade for time-based resource changes

---

## DEVELOPMENT GUIDELINES

### Before Making Any Changes

1. **Read this entire architecture document**
2. **Identify which layer you're modifying** (Content/Domain/Service/UI)
3. **Trace dependencies** using search tools to find all references
4. **Understand the impact radius** of your changes
5. **Verify architectural principles** are maintained

### Adding New Features

1. **Determine the business domain** - which facade should own it?
2. **Check if GameWorld state** needs new properties
3. **Design the service interface** following existing patterns
4. **Create context objects** for complex UI operations
5. **Follow the UI component hierarchy** - GameScreen → Child Components

### Modifying Existing Systems

1. **Never break the dependency flow** - dependencies always flow inward
2. **Never add shared mutable state** - use GameWorld as single source
3. **Never put business logic in UI** - keep facades responsible for rules
4. **Never bypass the facade layer** - UI must go through GameFacade

---

**This architecture ensures clean separation of concerns, predictable data flow, and maintainable code structure while supporting the complex conversation-based RPG mechanics of Wayfarer.**