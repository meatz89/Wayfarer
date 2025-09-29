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

Wayfarer is a **conversation-based RPG** built with **C# ASP.NET Core** and **Blazor Server**. The architecture follows **clean architecture principles** with strict **dependency inversion** and **single responsibility** patterns.

### Core Design Philosophy
- **GameWorld as Single Source of Truth**: All game state flows through GameWorld
- **Static Content Loading**: JSON content parsed once at startup without DI dependencies
- **Facade Pattern**: Business logic coordinated through specialized facades
- **Authoritative UI Pattern**: GameScreen owns all UI state, children communicate upward
- **No Shared Mutable State**: Services provide operations, not state storage

---

## CONTENT PIPELINE ARCHITECTURE

### 1. JSON Content Structure

**Location**: `src/Content/Core/*.json`

```
01_foundation.json  → Base configuration, conversation types, card decks
02_cards.json       → All conversation card definitions
03_npcs.json        → NPC definitions with requests and personalities
04_connections.json → Relationship and interaction rules
05_gameplay.json    → Core gameplay mechanics
```

**Content Relationships**:
- Cards reference conversation types via `conversationTypeId`
- NPCs have requests that specify `conversationTypeId`
- Card decks group cards via `cardIds` arrays
- All relationships use string IDs for loose coupling

### 2. Static Parser Layer

**Location**: `src/Content/*Parser.cs`

**Parser Responsibilities**:
```csharp
ConversationCardParser  → JsonElement → ConversationCard
NPCParser              → JsonElement → NPC
LocationParser         → JsonElement → Location/LocationSpot
```

**CRITICAL PARSER PRINCIPLES**:
- **PARSE AT THE BOUNDARY**: JSON artifacts NEVER pollute domain layer
- **NO JsonElement PASSTHROUGH**: Parsers MUST convert to strongly-typed objects
- **NO Dictionary<string, object>**: Use proper typed properties on domain models
- **STATELESS**: Parsers are static classes with no side effects

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
public List<NPC> NPCs { get; set; }
public List<Location> Locations { get; set; }
public CardDefinitionCollection AllCardDefinitions { get; set; }
public ConversationTypeCollection ConversationTypes { get; set; }
public Player GetPlayer() → Single player instance
```

**State Operations**:
```csharp
GetPlayerResourceState() → Current player resources
GetAvailableStrangers() → NPCs available at location/time
RefreshStrangersForTimeBlock() → Time-based NPC availability
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
├── ConversationFacade (4-resource conversation system)
├── LocationFacade (Movement and spot management)
├── ResourceFacade (Health, hunger, coins)
├── TimeFacade (Time progression and segments)
├── TravelFacade (Route management and travel)
├── TokenFacade (Relationship tokens)
├── NarrativeFacade (Messages and observations)
├── ObligationFacade (Letter queue management)
└── ExchangeFacade (NPC trading system)
```

### Facade Responsibilities

**GameFacade** - Pure orchestrator for UI-Backend communication
- Delegates ALL business logic to specialized facades
- Coordinates cross-facade operations
- Handles UI-specific orchestration
- NO business logic - only coordination

**Specialized Facades** - Domain-specific business logic
- `ConversationFacade`: Initiative/Momentum/Doubt/Cadence conversation mechanics
- `ExchangeFacade`: Separate NPC trading system (not conversations)
- `LocationFacade`: Movement validation and location-specific actions
- Each facade encapsulates ONE business domain

### Subsystem Organization

**Location**: `src/Subsystems/[Domain]/`

```
Conversation/   → Card mechanics, momentum, conversation sessions
Exchange/       → NPC trading, inventory validation
Location/       → Movement, spot properties, location actions
Market/         → Economic systems, arbitrage calculation
Narrative/      → Message system, observation rewards
Obligation/     → Letter queue, deadline tracking, displacement
Resource/       → Health, hunger, coins, stamina management
Time/           → Segment progression, time block transitions
Token/          → Relationship tokens, connection tracking
Travel/         → Route discovery, travel validation
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
LocationContent.razor      → Location exploration UI
ConversationContent.razor  → 4-resource conversation interface
ExchangeContent.razor      → NPC trading interface
ObligationQueueContent.razor → Letter queue management
TravelContent.razor        → Route selection and travel
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
ConversationContext  → Complete conversation state
ExchangeContext      → NPC trading session state
TravelContext        → Route and destination state
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

**1. Conversation System Integration**
- ConversationFacade manages 4-resource system (Initiative/Momentum/Doubt/Cadence)
- Integrates with TokenFacade for relationship management
- Integrates with ObligationFacade for letter generation
- Integrates with NarrativeFacade for dialogue generation

**2. Location System Integration**
- LocationFacade coordinates with NPCService for character placement
- Integrates with TravelFacade for route validation
- Coordinates with TimeFacade for time-based availability

**3. Resource System Integration**
- ResourceFacade manages health/hunger/coins/stamina
- Integrates with TimeFacade for time-based resource changes
- Coordinates with TravelFacade for travel costs

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