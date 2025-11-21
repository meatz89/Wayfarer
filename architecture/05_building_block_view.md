# Arc42 Section 5: Building Block View

## 5.1 Level 1: System Overview

Wayfarer is a **low-fantasy tactical RPG** with **three parallel challenge systems** (Social, Mental, Physical) built with **C# ASP.NET Core** and **Blazor Server**. The architecture follows **clean architecture principles** with strict **dependency inversion** and **single responsibility** patterns.

### Core Design Philosophy

- **GameWorld as Single Source of Truth**: All game state flows through GameWorld with zero external dependencies
- **Three Parallel Tactical Systems**: Social (conversations), Mental (investigations), Physical (obstacles) with equivalent depth
- **Strategic-Tactical Bridge**: Scenes spawn Situations with Choices, ChoiceActionType.StartChallenge routes to tactical systems
- **Static Content Loading**: JSON content parsed once at startup without DI dependencies
- **Facade Pattern**: Business logic coordinated through specialized facades
- **Authoritative UI Pattern**: GameScreen owns all UI state, children communicate upward
- **No Shared Mutable State**: Services provide operations, not state storage

---

## 5.2 Level 2: Container View

The Wayfarer system decomposes into four major subsystems:

```
┌─────────────────────────────────────────────────────────────────┐
│                         USER BROWSER                            │
│                  (Blazor Client via SignalR)                    │
└────────────────────────────┬────────────────────────────────────┘
                             │ WebSocket
┌────────────────────────────▼────────────────────────────────────┐
│                    UI COMPONENT LAYER                           │
│  GameScreen.razor (Authoritative Parent) + Child Components     │
│  (Dumb display, no business logic, calls facades)              │
└────────────────────────────┬────────────────────────────────────┘
                             │ Direct calls
┌────────────────────────────▼────────────────────────────────────┐
│                  SERVICE/SUBSYSTEM LAYER                        │
│  GameFacade (Orchestrator) + Specialized Facades               │
│  (Business logic, stateless, operates on GameWorld)            │
└────────────────────────────┬────────────────────────────────────┘
                             │ Read/Write
┌────────────────────────────▼────────────────────────────────────┐
│                      GAMEWORLD                                  │
│  (Single source of truth, zero dependencies, state only)       │
└────────────────────────────┬────────────────────────────────────┘
                             │ Populated by
┌────────────────────────────▼────────────────────────────────────┐
│                   CONTENT PIPELINE                              │
│  JSON Files → Parsers → DTOs → Domain Entities                 │
│  (Static, parse-time translation, catalogue scaling)           │
└─────────────────────────────────────────────────────────────────┘
```

**Dependency Flow**: All dependencies point INWARD toward GameWorld:
- UI → Services → GameWorld ← Content Pipeline
- GameWorld has ZERO outward dependencies

---

## 5.3 GameWorld (Level 3 Whitebox)

**Location**: `src/GameState/GameWorld.cs`

### Purpose

GameWorld is the **single source of truth** for all game state. It contains state collections and provides data access operations. GameWorld holds NO business logic - it's a pure state container.

### State Collections

GameWorld maintains collections for core game entities including character entities, venue entities, location entries, and a single player instance. For the three parallel tactical systems, it stores card template collections for each challenge type and challenge deck collections organizing cards by difficulty and context. The strategic layer is represented through scene template collections defining reusable scene archetypes and active scene instances currently in play.

For persistent gameplay scaffolding, GameWorld stores static atmospheric actions that provide ongoing gameplay options like travel, work, rest, and movement between locations. These are distinct from ephemeral scene-based actions which are passed by object reference without requiring storage. The player stats system is represented through stat definition collections describing available character attributes and a stat progression structure tracking advancement.

All collections use strongly-typed list containers holding domain entities with object references rather than ID lookups.

### State Operations

GameWorld exposes methods for retrieving the single player instance, accessing current player resource states including health and stamina, and locating specific location entries through identifier lookup. For character management, it provides methods to retrieve available stranger characters based on current venue and time context, refresh stranger availability when time blocks transition, and apply initial player configuration from loaded content data at game start.

### Critical Principles

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

## 5.4 Service Layer (Level 3 Whitebox)

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
└── SCENES & CONTENT
    ├── SceneInstantiator (Scene spawning from templates)
    ├── SpawnFacade (Scene spawn condition evaluation)
    ├── ContentGenerationFacade (Dynamic package creation)
    └── RewardApplicationService (Reward application after choices)
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

SCENE & SPAWN SYSTEMS
Spawn/          → Scene spawn condition evaluation, template instantiation
ProceduralContent/ → Procedural scene generation, AI content integration
Catalogues/     → Parse-time categorical property translation
```

---

## 5.5 UI Components (Level 3 Whitebox)

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

**Direct Parent-Child Communication**: Child components receive a reference to their parent screen through cascading values, enabling them to invoke parent methods directly for actions like navigation, initiating challenges, or executing game actions. The parent passes callback delegates to children for refresh notifications and state synchronization.

**Context Objects for Complex State**: The three parallel tactical systems each maintain dedicated context objects that encapsulate complete challenge state including resources, active cards, and progression tracking. Supporting systems use specialized context objects for trading sessions, travel destinations with route information, and location exploration state. These context objects are passed between components to maintain state consistency without requiring shared mutable storage.

### Critical UI Principles

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

## 5.6 Content Pipeline (Level 3 Whitebox)

**Location**: `src/Content/*Parser.cs` & `PackageLoader.cs`

### Purpose

The content pipeline loads JSON content files, translates categorical properties to concrete values, and populates GameWorld collections at startup.

### Loading Sequence

```
Startup → GameWorldInitializer.CreateGameWorld()
       → PackageLoader.LoadContent()
       → Static Parsers (for each content type)
       → Domain Objects → GameWorld Population
```

### Static Parser Layer

**Parser Responsibilities**:
Each parser converts DTO to domain entity:
- SocialCardParser: SocialCardDTO → SocialCard
- MentalCardParser: MentalCardDTO → MentalCard
- PhysicalCardParser: PhysicalCardDTO → PhysicalCard
- NPCParser: NPCDTO → NPC
- LocationParser: LocationDTO → Location
- SceneParser: SceneDTO → Scene
- SituationParser: SituationDTO → Situation

**Critical Parser Principles**:
- **PARSE AT THE BOUNDARY**: JSON artifacts NEVER pollute domain layer
- **NO JsonElement PASSTHROUGH**: Parsers MUST convert to strongly-typed objects
- **NO Dictionary Properties**: Use proper typed properties on domain models
- **JSON FIELD NAMES MUST MATCH C# PROPERTIES**: Direct mapping without JsonPropertyName
- **STATELESS**: Parsers are static classes with no side effects
- **SINGLE PASS**: Each parser converts DTO to domain entity in one operation
- **CATEGORICAL TO MECHANICAL TRANSLATION**: Parsers translate via catalogues

### Categorical Properties → Dynamic Scaling (AI Content Generation)

**The Problem: AI-Generated Runtime Content**

AI-generated content (procedural generation, LLM-created entities, user-generated content) CANNOT specify absolute mechanical values because AI doesn't know:
- Current player progression level (Level 1 versus Level 10)
- Existing game balance (what items/cards/challenges already exist)
- Global difficulty curve (early game versus late game tuning)
- Economy state (coin inflation, resource scarcity)

**The Solution: Relative Categorical Properties Plus Dynamic Scaling Catalogues**

AI generates JSON with categorical properties providing relative descriptions. Parser reads current game state (player level, difficulty mode). Catalogue translates categorical properties to absolute values applying scaling based on game state. Domain entity receives scaled mechanical values appropriate to current progression.

**Example: Equipment Durability**

Content defines equipment with relative categorical durability descriptors rather than absolute numeric values. During parsing, the system reads current player progression level and difficulty settings, then consults the durability catalogue to translate the categorical descriptor into concrete values. The catalogue applies scaling formulas that increase use counts and costs proportionally with player level, ensuring that equipment remains appropriately challenging throughout the game. The critical principle maintains relative ordering where lower durability categories always remain weaker than higher durability categories regardless of absolute scaling factors.

**Example: Card Effects**

Content defines tactical cards using categorical properties describing the conversational move type, bound character stat, and abstract depth level. During parsing, the system invokes the card effect catalogue with these categorical properties plus current player level. The catalogue calculates base effect values appropriate to the move type and depth, then applies progression scaling multipliers. This ensures identical categorical properties produce stronger effects at higher player levels while maintaining consistent relative power between different card types.

**Why This Architecture Exists**:
- **AI Content Generation**: AI describes entities relatively (Fragile rope, Cunning NPC) without needing absolute game values knowledge
- **Dynamic Difficulty Scaling**: Same content scales automatically as player progresses
- **Consistent Relative Balance**: Fragile ALWAYS weaker than Sturdy regardless of current scaling
- **Future-Proof**: Supports procedural generation, LLM content generation, user mods, runtime content creation
- **Centralized Balance**: Change ONE catalogue formula, affects ALL entities of that category

**Catalogue Implementation**:
- Located in `src/Content/Catalogues/` as static classes
- Context-aware scaling functions take categorical enum + scaling context
- Calculate base values for each category via switch expression
- Apply dynamic scaling multipliers based on game state
- Return scaled values as tuple or structured result
- Throw exceptions for unknown categorical values (fail-fast)

**Existing Catalogues**:
- SocialCardEffectCatalog
- MentalCardEffectCatalog
- PhysicalCardEffectCatalog
- EquipmentDurabilityCatalog

**When to Use Categorical Properties**:

Ask these questions for ANY numeric property in DTO:
1. Could AI generate this entity at runtime without knowing global game state?
2. Should this value scale with player progression or difficulty?
3. Is this RELATIVE (compared to similar entities) rather than ABSOLUTE?

If YES to any question: Create categorical enum + scaling catalogue
If NO: Consider if truly design-time constant (rare - most values should scale)

**Anti-Pattern: Hardcoded Absolute Values in JSON**

Content that specifies absolute numeric values for mechanical properties breaks AI generation capabilities and prevents proper difficulty scaling. Values like specific use counts, fixed costs, or concrete effect magnitudes lock the content to a single progression level and prevent the system from adapting to player advancement.

The correct pattern uses categorical descriptors that express relative concepts. Durability categories, move type classifications, and abstract depth levels enable both AI authoring and dynamic scaling. The parser and catalogue system translate these categories into appropriate absolute values based on runtime game state.

### Initialization Architecture

- `GameWorldInitializer` is **STATIC** - no DI dependencies
- Creates GameWorld **BEFORE** service registration completes
- Prevents circular dependencies during ServerPrerendered mode
- Content loaded once at startup, never reloaded

---

## 5.7 Entity Ownership Hierarchy

### Ownership Model

**GameWorld** is the single source of truth containing all game entities. It directly owns:
- **Scenes collection**: Strategic layer progression containers
- Each **Scene** owns its embedded **Situations collection** via direct object containment (NOT ID references)
- Scenes track their **CurrentSituation** via direct object reference
- Scenes manage **SpawnRules** defining situation flow
- Scenes reference their placement entities via direct **object properties** (Location, Npc, Route)

**Situations** are EMBEDDED IN SCENES, not a separate GameWorld collection:
- Each Situation is owned by its parent Scene
- Situations reference their **Template** containing ChoiceTemplates
- Situations have **SystemType** property (Social/Mental/Physical) for bridge routing
- Situations store **SituationCards list** defining tactical victory conditions

**SituationCards** are EMBEDDED IN SITUATIONS as tactical victory conditions:
- Stored in Situation's SituationCards list property
- Challenges extract and READ these cards when spawned
- Define threshold values (momentum/progress/breakthrough)
- Grant rewards on achievement

**Locations and NPCs** exist in GameWorld's collections as PLACEMENT CONTEXT ONLY:
- Scenes appear at Locations and NPCs but are NOT owned by them
- This is placement not ownership - Location lifecycle is independent from Scene lifecycle

### Ownership Diagram

```
GameWorld (Single Source of Truth)
 ├─ Scenes (GameWorld.Scenes collection - OWNED)
 │   └─ Situations (Scene.Situations property - EMBEDDED)
 │       ├─ ChoiceTemplates (Situation.Template.ChoiceTemplates - REFERENCED)
 │       └─ SituationCards (Situation.SituationCards list - EMBEDDED)
 ├─ Locations (GameWorld.Locations collection - placement context, NOT owners)
 ├─ NPCs (GameWorld.NPCs collection - placement context, NOT owners)
 └─ Routes (GameWorld.Routes collection - placement context, NOT owners)
```

### Layer Separation Examples

**Strategic Layer Flow (No Challenge)**:
1. Player at location sees active Situation presenting Choice with **Instant** action type
2. Player selects Choice
3. System applies costs (deducts resources)
4. System applies rewards (grants items/access)
5. Scene advances to next Situation
6. Flow remains entirely within strategic layer

**Bridge to Tactical Layer**:
1. Player at location sees Situation presenting Choice with **StartChallenge** action type
2. Player selects Choice
3. System spawns challenge session of specified type (Social/Mental/Physical)
4. Challenge session extracts **SituationCards** from parent Situation defining victory thresholds
5. Player enters tactical card-based gameplay

**Tactical Layer Flow**:
1. Challenge session active with initial resource state (Initiative, Momentum, Doubt for Social)
2. Player plays tactical cards that modify resources through card effects
3. Resources accumulate until reaching SituationCard threshold
4. Threshold reached → SituationCard rewards applied
5. Challenge ends, return to strategic layer
6. Scene advances to next Situation

### Complete Flow Example: Securing Lodging

```
1. SceneInstantiator spawns Scene containing multiple Situations
2. Scene placed at Location via PlacementFilter resolution
3. First Situation becomes active
4. Player sees multiple ChoiceTemplates with different action types:
   - Instant payment
   - Social negotiation challenge
   - Physical theft challenge
5. Player selects negotiation choice with StartChallenge action type
6. SocialFacade creates challenge session extracting SituationCards for victory conditions
7. Player plays tactical Social cards building Momentum resource
8. Momentum reaches threshold defined in SituationCard
9. System applies SituationCard rewards
10. Challenge ends, returns to strategic layer
11. Scene progresses to next Situation
12. Player navigates to new location
13. Next Situation activates at new context
```

### Forbidden Patterns

❌ **Layer Confusion**: Showing SituationCards in strategic progression flow (Obligation → Scene → Situation → SituationCard). SituationCards are tactical victory conditions, not strategic progression elements.

❌ **Wrong Collection Assignment**: Treating SituationCards as strategic choices by assigning them to Situation.ChoiceTemplates. These are completely different concepts belonging to different layers.

❌ **Wrong Ownership**: GameWorld owning separate Situations collection. Situations are owned by their parent Scene through embedded collection, not stored separately in GameWorld.

❌ **Layer Misclassification**: Describing Situations as part of tactical layer. Situations are strategic layer entities. Tactical layer consists of Challenge sessions only.

❌ **Wrong Entity Type**: Treating SituationCard as separate reusable Card entity inheriting from base Card class. SituationCards are inline victory condition definitions, not playable cards.

❌ **Legacy Entities**: Referencing Obstacle, Goal, or GoalCard entities. These were deleted from codebase and replaced by Scene/Situation architecture.

### Correct Patterns

✅ **Scene Ownership**: Scene owns Situations directly through embedded collection property. Scene tracks CurrentSituation via direct object reference. Scene manages SpawnRules defining situation flow. Scene references placement entities via direct object properties (Location, Npc, Route).

✅ **Situation Structure**: Situation stores SystemType property for bridge routing metadata. Situation references Template containing ChoiceTemplates. Situation stores SituationCards list defining tactical victory conditions.

✅ **ChoiceTemplate Bridge**: ChoiceTemplate bridges layers via ActionType property. ChallengeType property specifies which tactical system if StartChallenge. ChallengeId specifies which deck to use. OnSuccessReward and OnFailureReward define conditional outcomes applied after challenge completion.

✅ **SituationCard Purpose**: SituationCard defines tactical victory condition with threshold property universal across all challenge types. Rewards property defines what player receives on achievement. IsAchieved property tracks runtime completion state.

✅ **GameWorld Collections**: GameWorld owns Scenes collection only for strategic layer. NO separate Situations collection exists - Scenes own them. NO Challenges collection exists - challenges are temporary sessions created and destroyed per engagement.

### Key Architectural Rules

**Strategic Layer Rules**:
1. Scene → Situation → Choice is the COMPLETE strategic flow
2. Situations do NOT progress to SituationCards (different layer)
3. Perfect information: All costs, requirements, rewards visible before selection
4. State machine: Scene.AdvanceToNextSituation() manages progression
5. Persistent: Scenes exist until completed/expired

**Tactical Layer Rules**:
1. Challenge sessions are TEMPORARY (created/destroyed per engagement)
2. SituationCards define victory conditions with thresholds
3. Three parallel systems: Social/Mental/Physical with equivalent depth
4. Hidden complexity: Card draw, exact challenge flow not visible before entry
5. Return to strategic layer on completion with success/failure outcome

**Bridge Rules**:
1. ChoiceTemplate.ActionType determines if bridge crosses
2. Only StartChallenge crosses to tactical layer
3. Instant and Navigate stay in strategic layer
4. One-way: Strategic spawns tactical, tactical returns outcome to strategic
5. SituationCards extracted when challenge spawns, NOT before

---

## Related Documentation

- **04_solution_strategy.md** - Fundamental decisions driving this structure
- **06_runtime_view.md** - Dynamic behavior of these building blocks
- **08_crosscutting_concepts.md** - Patterns used across all components
- **03_context_and_scope.md** - System boundaries and external interfaces
