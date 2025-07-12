# CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE BEFORE WRITING TO IT ⚠️**

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## DOCUMENTATION GUIDELINES

### **NEW SESSION STARTUP CHECKLIST**
**CRITICAL**: Every new session must follow this exact sequence:

1. ✅ **READ CLAUDE.MD FIRST** - Understand architectural patterns and game design principles
2. ✅ **READ SESSION-HANDOFF.MD** - Get current progress, discoveries, and immediate next steps
3. ✅ **READ INTENDED-GAMEPLAY.md** - Acquire a deep understanding of what we want the game experience to feel like for the player
4. ✅ **READ LOGICAL-SYSTEM-INTERACTIONS.MD** - Critical design guidelines for system changes
5. ✅ **READ USERSTORIES.MD** - Game design requirements and anti-patterns
6. ✅ **ONLY THEN begin working** - Never start coding without understanding current state

### **DOCUMENTATION ARCHITECTURE**

**CLAUDE.MD** (This file) - **ARCHITECTURAL REFERENCE**
- Core architectural patterns and principles
- Game design principles and constraints
- Code writing guidelines and standards
- High-level system overview
- Key file locations and responsibilities
- **NEVER** add session progress, temporary fixes, or detailed implementation notes

**LOGICAL-SYSTEM-INTERACTIONS.MD** - **CRITICAL DESIGN GUIDELINES**
- **MANDATORY**: Always follow these logical interaction principles
- Replaces arbitrary math with logical system interactions
- Design patterns for emergent constraints
- Validation checklist for all implementations
- **READ THIS BEFORE ANY SYSTEM CHANGES**

**GAME-ARCHITECTURE.MD** - **CRITICAL ARCHITECTURAL PATTERNS**
- **MANDATORY**: System dependency patterns and failure modes discovered through debugging
- Time window initialization requirements and location spot dependencies
- Repository pattern compliance and single source of truth enforcement
- JSON content parsing validation and enum matching requirements
- Test architecture compliance patterns
- **READ THIS BEFORE ANY SYSTEM MODIFICATIONS**
- **CONTINUOUSLY UPDATE**: Add new architectural findings, dependency discoveries, and failure patterns as they are discovered
- **NEVER LOSE KNOWLEDGE**: Document all critical debugging discoveries to prevent repeating the same architectural mistakes

**SESSION-HANDOFF.MD** - **CURRENT SESSION STATE**
- Current progress and completed features
- Critical discoveries and constraints from user feedback
- Technical implementation patterns learned
- Immediate next priorities and blockers
- Test execution status
- Files that need attention
- **UPDATE THIS EVERY SESSION** with new discoveries and progress

**IMPLEMENTATION-PLAN-REVISED.MD** - **FEATURE ROADMAP**
- POC feature priorities and requirements
- Game design goals for each feature
- Success criteria and anti-patterns to avoid
- Technical complexity estimates
- Dependencies between features

**USERSTORIES.MD** - **GAME DESIGN REQUIREMENTS**
- User-facing feature requirements
- Game vs app design principles
- Examples of good vs bad implementations
- Player experience goals

### **DOCUMENTATION MAINTENANCE RULES**

1. ✅ **ALWAYS update session-handoff.md** - Document all discoveries, progress, and next steps
2. ✅ **ONLY update claude.md** - When architectural patterns change or new principles are discovered
3. ✅ **NEVER add temporary status** - Session progress goes in session-handoff.md, not claude.md
4. ✅ **Document user feedback immediately** - Critical constraints and discoveries go in session-handoff.md
5. ✅ **Keep files focused** - Each file has a specific purpose and audience
6. ✅ **Reference related files** - Always point to where related information can be found

## DEVELOPMENT GUIDELINES


**Key Implementation Requirements:**

1. **All entities must have unique types/categories**: Every entity (items, routes, locations, NPCs) should belong to meaningful categories that can interact with other system categories
   - Items: `EquipmentCategory` (Climbing_Equipment, Weather_Protection, Navigation_Tools, etc.)
   - Routes: `TerrainCategory` (Requires_Climbing, Exposed_Weather, Wilderness_Terrain, etc.)
   - Locations: Geographic and social categories that affect NPC availability and trading
   - NPCs: Profession and social categories that determine knowledge and schedules

2. **Game rules should emerge from category interactions**: Instead of hardcoded bonuses/penalties, create logical relationships between categories
   - Weather + Terrain → Access requirements
   - Equipment + Terrain → Capability enablement  
   - NPC Profession + Location Type → Service availability
   - Time + NPC Schedule → Social interaction windows

3. **Constraints should require multiple systems**: No single system should create arbitrary restrictions
   - ✅ Good: "Mountain routes need climbing gear, but only accessible in good weather, and guides are only available on market days"
   - ❌ Bad: "Mountain routes cost +50% stamina"

4. **Categories enable discovery gameplay**: Players learn system relationships through experimentation
   - Trying to travel in fog without navigation tools → blocked → learn navigation tools enable fog travel
   - Attempting to trade with nobles without proper attire → blocked → learn social categories matter
   - Weather changes block previously accessible routes → learn weather-terrain interactions

5. **All entity categories must be visible in frontend UI**: For players to formulate strategies, they must be able to see and understand the categories that influence game rules
   - Items must display their `EquipmentCategory` (Climbing_Equipment, Weather_Protection, etc.)
   - Routes must show their `TerrainCategory` (Requires_Climbing, Exposed_Weather, etc.)
   - Locations should indicate their geographic/social categories
   - NPCs should reveal their profession and social categories through interaction
   - Weather conditions and their effects on terrain types must be discoverable
   - **Players cannot strategize about systems they cannot see or understand**

**Core Design Rules:**
- **NEVER** use arbitrary mathematical modifiers (efficiency multipliers, percentage bonuses, etc.)
- **ALWAYS** implement logical blocking/enabling instead of sliding scale penalties
- **REQUIRE** logical justification for all constraints based on system interactions
- **ENSURE** all entity categories are visible and understandable in the UI
- **VALIDATE** all designs against the logical interaction checklist

**UI Visibility Requirements:**
- **Strategic Information Access**: Players must be able to inspect all relevant categories and types that affect gameplay decisions
- **Category Display**: Show equipment categories, terrain requirements, NPC professions, location types, etc.
- **Relationship Hints**: When blocked, provide enough information for players to understand what systems are interacting
- **No Hidden Mechanics**: Avoid invisible systems that affect gameplay without player awareness
- **Discovery-Friendly**: Information should be discoverable through exploration and experimentation, not automatically revealed

### CODE WRITING PRINCIPLES
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated
- You must run all tests and execute the game and do quick smoke tests before every commit
- **Never keep legacy code for compatibility**
- **NEVER use suffixes like "New", "Revised", "V2", etc.** - Replace old implementations completely and use the correct final name immediately. Delete old code, don't leave it behind.
- **CRITICAL: IMMEDIATE LEGACY CODE ELIMINATION** - If you discover ANY legacy code, compilation errors, or deprecated patterns during development, you MUST immediately:
  1. **CREATE HIGH-PRIORITY TODO ITEM** to fix the legacy code
  2. **STOP current work** and fix the legacy code immediately
  3. **NEVER ignore or postpone** legacy code fixes
  4. **NEVER say "these are just dependency fixes"** - fix them now or create immediate todo items

### GAME DESIGN PRINCIPLES (Critical for Games vs Apps)
**Games create interactive optimization puzzles for the player to solve, not automated systems that solve everything for them.**

- ✅ **GAMEPLAY IS IN THE PLAYER'S HEAD** - Fun comes from systems interacting in clever ways that create optimization challenges
- ✅ **DISCOVERY IS GAMEPLAY** - Players must explore, experiment, and learn to find profitable trades, efficient routes, optimal strategies
- ❌ **NO AUTOMATED CONVENIENCES** - Don't create `GetProfitableItems()` or `GetBestRoute()` methods that solve the puzzle for the player
- ❌ **NO GAMEPLAY SHORTCUTS** - No "Trading Opportunities" UI panels that tell players exactly what to do
- ✅ **EMERGENT COMPLEXITY** - Simple systems (pricing, time blocks, stamina) that interact to create deep strategic decisions
- ✅ **MEANINGFUL CHOICES** - Every decision should involve sacrificing something valuable (time vs money vs stamina)
- ✅ **PLAYER AGENCY** - Players discover patterns, build mental models, develop personal strategies through exploration

**Example**: Instead of showing "Buy herbs at town_square (4 coins) → Sell at dusty_flagon (5 coins) = 1 profit", let players discover this by visiting locations, checking prices, and building their own understanding of the market.

### FRONTEND PERFORMANCE PRINCIPLES
- **NEVER use caching in frontend components** - Components should be stateless and reactive
- **Reduce queries by optimizing when objects actually change** - Focus on state change detection, not caching
- **Log at state changes, not at queries** - Debug messages should track mutations, not reads
- **Use proper reactive patterns** - Let Blazor's change detection handle rendering optimization

## PROJECT OVERVIEW: WAYFARER

**Wayfarer** is a medieval life simulation RPG built as a Blazor Server application. It features a sophisticated, AI-driven narrative system with turn-based resource management gameplay focused on economic strategy, travel optimization, and contract fulfillment.

### CORE ARCHITECTURAL PATTERNS

#### **UI → GameWorldManager Gateway Pattern**
All UI components must route actions through GameWorldManager instead of injecting managers directly.
- ✅ Correct: UI → GameWorldManager → Specific Manager
- ❌ Wrong: UI → Direct Manager Injection

#### **Stateless Repositories** 
Repositories MUST be completely stateless and only delegate to GameWorld - NO data storage or caching allowed.
- ✅ Correct: `private readonly GameWorld _gameWorld` (ONLY allowed private field)
- ✅ Correct: Every method accesses `_gameWorld.WorldState.Property` directly
- ❌ Wrong: `private List<Entity> _cachedEntities` or any state storage
- ❌ Wrong: `private WorldState` caching or direct WorldState access
- ❌ Wrong: Any private fields other than GameWorld dependency

**ENFORCEMENT**: Repository classes may ONLY have `private readonly GameWorld _gameWorld` field. All data comes from GameWorld on every method call.

#### **Repository-Mediated Access Only (CRITICAL ARCHITECTURAL PRINCIPLE)**
**ALL game state access MUST go through entity repositories - NEVER through direct GameWorld property access.**

**MANDATORY ENFORCEMENT RULES:**
1. **ONLY repositories may access GameWorld.WorldState properties**
2. **Business logic MUST use repositories, never GameWorld properties**  
3. **Tests MUST use repositories, never GameWorld properties**
4. **UI components MUST use repositories, never GameWorld properties**
5. **GameWorld properties exist ONLY for repository implementation**
6. **Test setup MUST use repositories for data creation, never direct WorldState access**

**VIOLATION EXAMPLES - FORBIDDEN:**
```csharp
❌ gameWorld.WorldState.AddCharacter(npc);           // Direct WorldState access
❌ gameWorld.WorldState.Items.Add(item);             // Direct collection access
❌ var locations = gameWorld.WorldState.locations;   // Direct property access
❌ gameWorld.WorldState.Contracts.AddRange(contracts); // Direct collection manipulation
```

**CORRECT PATTERNS - REQUIRED:**
```csharp
✅ npcRepository.AddNPC(npc);                        // Repository-mediated
✅ itemRepository.AddItem(item);                     // Repository-mediated
✅ var locations = locationRepository.GetAllLocations(); // Repository query
✅ contractRepository.AddContracts(contracts);      // Repository-mediated
```

**TEST ARCHITECTURE - SPECIAL CASE:**
Tests differ from production ONLY in GameWorld construction. The GameWorldManager → Repository → GameWorld flow remains identical:

```csharp
// ✅ CORRECT TEST PATTERN:
// 1. Construct GameWorld differently (TestGameWorldInitializer vs GameWorldInitializer)
GameWorld gameWorld = TestGameWorldInitializer.CreateTestWorld(scenario);

// 2. Use IDENTICAL repository access patterns as production
NPCRepository npcRepository = new NPCRepository(gameWorld);
npcRepository.AddNPC(testNPC);  // Repository-mediated, just like production

// 3. GameWorldManager behavior is IDENTICAL to production
GameWorldManager manager = new GameWorldManager(gameWorld, ...repositories...);
```

**THIS APPLIES TO ALL CODE - NO EXCEPTIONS:**
- Production business logic
- Test setup and teardown
- UI component data access
- Manager classes
- Service classes

#### **Repository-Based ID Resolution Pattern**
Repositories are responsible for ID-to-object lookup, not initialization or business logic.
- **GameWorldInitializer**: Only loads raw JSON data, no relationship building
- **Repositories**: Provide `GetEntityById(string id)` methods for all lookups
- **Business Logic**: Uses repositories to resolve IDs when needed for rules/mechanics
- **NEVER** do direct LINQ lookups in business logic - always use repository methods
- Validation of missing/invalid IDs handled at repository level

**⚠️ CURRENT VIOLATIONS IDENTIFIED:**
- **GameWorldInitializer.cs**: `ConnectLocationsToSpots()` and `ConnectRoutesToLocations()` methods violate separation of concerns
- **MarketManager.cs**: Direct LINQ `_gameWorld.WorldState.Items?.FirstOrDefault(i => i.Id == itemId)` instead of using ItemRepository
- **Multiple files**: 10+ files contain direct LINQ lookups that should use repository methods
- **TravelManager.cs**: Should use LocationRepository for location lookups instead of direct access

#### **Service Configuration**
- Production: Use `ConfigureServices()` for full AI stack
- Testing: Use `ConfigureTestServices()` for economic-only functionality
- No duplicate service registrations

### PROJECT STATUS

**Current progress and session handoffs:** `session-handoff.md`
**Game design requirements:** `UserStories.md`
**Logical interaction principles:** `LOGICAL-SYSTEM-INTERACTIONS.md`
**Architectural patterns and discoveries:** `GAME-ARCHITECTURE.md`
**UI styling patterns and conventions:** `STYLING-PATTERNS.md`

### GAME INITIALIZATION PIPELINE

#### **Critical Architecture Discovery: Proper JSON-to-GameWorld Flow**

**MANDATORY INITIALIZATION SEQUENCE** - All tests and development must follow this exact pattern:

```
JSON Files → GameWorldSerializer (Parsers) → GameWorldInitializer → GameWorld → Repositories
```

**❌ WRONG: Manual Object Creation in Tests**
```csharp
// NEVER do this - bypasses the entire content system
var gameWorld = new GameWorld();
var location = new Location("test", "Test");
gameWorld.Locations.Add(location);
```

**✅ CORRECT: Use GameWorldInitializer**
```csharp
// ALWAYS use this - follows production initialization flow
GameWorldInitializer initializer = new GameWorldInitializer("Content");
GameWorld gameWorld = initializer.LoadGame();
```

#### **JSON Content Files (src/Content/Templates/)**
- `locations.json` - All game locations with properties
- `location_spots.json` - Specific spots within locations  
- `routes.json` - Travel routes with terrain categories and costs
- `items.json` - All items with equipment and item categories
- `contracts.json` - Available contracts with requirements
- `actions.json` - Player actions available at locations
- `gameWorld.json` - Initial game state configuration

#### **Initialization Flow Details**

**Phase 1: Content Loading**
1. `GameWorldInitializer` reads all JSON files using `GameWorldSerializer`
2. Entities are parsed with proper categories and relationships
3. Content validation ensures data integrity

**Phase 2: Entity Relationship Building**  
1. Locations connected to LocationSpots via `LocationId`
2. Routes connected to origin/destination locations  
3. Contracts validated against existing locations
4. Items categorized with `EquipmentCategory` and `ItemCategory`
5. **ID Resolution Methods Called**: `ConnectLocationsToSpots()`, `ConnectRoutesToLocations()`

**Phase 3: GameWorld Assembly**
1. All content loaded into `GameWorld.WorldState` collections
2. Player initialized with starting location and inventory
3. `TimeManager` created and linked to `GameWorld`
4. Current location and spot set (never null after initialization)

**Phase 4: Repository Creation**
1. Repositories created with `GameWorld` dependency
2. All repositories access data through `GameWorld` (single source of truth)
3. No direct `WorldState` access from business logic

#### **ServiceConfiguration Integration**
```csharp
// Production initialization pattern (src/ServiceConfiguration.cs)
GameWorldInitializer gameWorldInitializer = new GameWorldInitializer("Content");
GameWorld gameWorld = gameWorldInitializer.LoadGame();
services.AddSingleton(gameWorld);

// Repositories depend on initialized GameWorld
services.AddSingleton<LocationRepository>();
services.AddSingleton<ItemRepository>();
// etc.
```

#### **Testing Requirements**
- **NEVER** manually create game objects in tests
- **ALWAYS** use `GameWorldInitializer` for test setup
- Tests must verify JSON content loads correctly into GameWorld
- Integration tests must validate complete initialization pipeline

#### **Critical Pattern: ID-based Serialization vs Object References**

**FUNDAMENTAL ARCHITECTURE**: The game uses a dual-reference system for entity relationships:

**JSON Serialization Layer (String IDs)**
```json
{
  "routes": [
    {
      "id": "road_to_market",
      "origin": "dusty_flagon",           // String ID reference
      "destination": "town_square",       // String ID reference
      "requiredItems": ["climbing_gear"]  // String ID references
    }
  ],
  "contracts": [
    {
      "id": "deliver_tools",
      "destinationLocation": "town_square", // String ID reference
      "requiredItems": ["tools"]            // String ID references
    }
  ]
}
```

**Runtime Code Layer (Object References)**
```csharp
public class RouteOption
{
    public string Origin { get; set; }        // String ID for serialization
    public string Destination { get; set; }   // String ID for serialization
    
    // Runtime: Code resolves IDs to actual objects when needed
    public Location GetOriginLocation(GameWorld gameWorld) =>
        gameWorld.WorldState.locations.First(loc => loc.Id == Origin);
    
    public Location GetDestinationLocation(GameWorld gameWorld) =>
        gameWorld.WorldState.locations.First(loc => loc.Id == Destination);
}
```

**Key Principles:**

1. **JSON Storage = String IDs Only**
   - All relationships stored as string references in JSON
   - Enables easy editing and version control of content files
   - Prevents circular reference issues in serialization

2. **Runtime Resolution = Object References**
   - Code resolves string IDs to actual object instances when needed
   - GameWorld provides lookup methods for ID-to-object resolution
   - Business logic operates on actual objects, not IDs

3. **Validation During Initialization**
   - GameWorldInitializer validates all ID references resolve to real objects
   - Broken references caught at startup, not during gameplay
   - ConnectLocationsToSpots(), ConnectRoutesToLocations() establish relationships

4. **Repository Pattern for Resolution**
   - Repositories provide ID-to-object lookup methods
   - `LocationRepository.GetLocationById(string id)`
   - `ItemRepository.GetItemById(string id)`
   - Consistent pattern across all entity types

**Example Relationship Flows:**

```csharp
// Contract references destination by ID
contract.DestinationLocation = "town_square";  // JSON storage

// Runtime: Business logic resolves to actual object
Location destination = locationRepository.GetLocationById(contract.DestinationLocation);
bool canComplete = destination != null && player.CurrentLocation.Id == destination.Id;

// Route references locations by ID  
route.Origin = "dusty_flagon";
route.Destination = "town_square";

// Runtime: Travel system resolves to objects
Location origin = gameWorld.GetLocationById(route.Origin);
Location dest = gameWorld.GetLocationById(route.Destination);
```

**Benefits of This Pattern:**
- **Data Integrity**: Validation ensures all references are valid
- **Performance**: Object lookups cached, not repeated string searches
- **Maintainability**: Content creators work with readable IDs in JSON
- **Flexibility**: Can change object implementations without breaking serialization

#### **Repository-Based ID Resolution Architecture**

**PRINCIPLE**: ID-to-object resolution is the responsibility of repositories, not the initializer.

**Architecture Pattern:**

1. **JSON files contain string IDs** - Maintains clean serialization format
2. **Entities keep string IDs** - For serialization and cross-references  
3. **Repositories provide ID-to-object lookup** - This is their core responsibility
4. **Business logic uses repositories for resolution** - When they need object references

**Component Responsibilities:**

- **GameWorldInitializer**: Loads raw JSON data into GameWorld collections only
- **Repositories**: Provide `GetEntityById(string id)` methods for ID-to-object resolution
- **Business Logic**: Uses repositories to resolve IDs to objects when needed for rules/mechanics
- **GameWorld**: Single source of truth containing all loaded entities

**Benefits:**
- **Separation of Concerns**: Clear responsibility boundaries
- **Single Responsibility**: Each component has one focused job
- **Validation**: Repositories handle missing/invalid ID references
- **Performance**: Repositories can implement caching strategies
- **Testability**: Dependencies are explicit and mockable
- **Maintainability**: Changes to lookup logic contained in repositories

### KEY LOCATIONS IN CODEBASE

#### **Core Game Management**
- `src/GameState/GameWorldManager.cs` - Central coordinator, UI gateway
- `src/GameState/GameWorld.cs` - Single source of truth for game state
- `src/Content/GameWorldInitializer.cs` - JSON content loading and game initialization

#### **Content System**
- `src/Content/GameWorldSerializer.cs` - JSON parsing and serialization
- `src/Content/Templates/` - All JSON content files

#### **Repository Pattern**
- `src/Content/LocationRepository.cs` - Stateless location data access
- `src/Content/ActionRepository.cs` - Stateless action data access  
- `src/Content/ItemRepository.cs` - Stateless item data access
- `src/Game/MainSystem/ContractRepository.cs` - Stateless contract data access

#### **Business Logic**
- `src/GameState/TravelManager.cs` - Travel and routing logic
- `src/GameState/MarketManager.cs` - Trading and pricing logic
- `src/GameState/TradeManager.cs` - Transaction processing
- `src/GameState/RestManager.cs` - Rest and recovery logic
- `src/Game/MainSystem/ContractSystem.cs` - Contract management and completion logic

#### **Service Configuration**
- `src/ServiceConfiguration.cs` - Dependency injection setup

#### **UI Components**
- `src/Pages/MainGameplayView.razor` - Main game screen coordinator
- `src/Pages/Market.razor` - Trading interface
- `src/Pages/TravelSelection.razor` - Travel planning interface
- `src/Pages/ContractUI.razor` - Contract display and completion interface
- `src/Pages/RestUI.razor` - Rest and recovery interface

### COMMON PATTERNS TO MAINTAIN

#### **Error-Free Initialization**
- All location properties must be properly initialized to prevent null reference exceptions
- UI screens must check `IsGameDataReady()` before rendering
- Player location and spot must never be null after initialization

#### **Consistent Data Access**
- Access current location via `GameWorld.CurrentLocation` or `GameWorld.WorldState.CurrentLocation` (both delegate to WorldState)
- Use location-aware method signatures throughout

#### **Service Dependency Management**
- AI services are optional for economic functionality
- Use nullable dependencies and factory patterns for services that might not be available
- Test configuration should provide minimal viable services only

# Development Principles: Emergent Design

## Core Principle: Emergent Mechanics
**Never hardcode restrictions or bonuses.** All gameplay constraints must emerge from mathematical interactions between simple atomic systems (time, stamina, coins, etc.). Players should experience strategic pressure through resource scarcity and efficiency trade-offs, not arbitrary limitations. If you're tempted to write `if (condition) { player.CanNotDoX = true; }` or add magic bonuses, instead create mathematical relationships that naturally discourage or encourage the behavior through consequences.

## Design Framework: Experience vs Mechanics vs Agency
Always distinguish between three layers:

**Player Experience**: What the player feels ("I can't afford to trade during rush deliveries")  
**Hardcoded Mechanics**: What the code actually enforces (tight deadlines, travel time, resource costs)  
**Player Agency**: What choices remain available (can still trade, but will miss deadline and lose payment)

**Goal**: Complex strategic experiences should arise from simple mathematical rules interacting, not from programmed restrictions. Players should always retain choice, but face natural consequences that guide rational decision-making. The game should feel designed and intentional while being mathematically inevitable.

## GAME INITIALIZATION
- Ensure game initialization leverages JSON files, parsers, GameWorldInitialization, Repositories, and GameWorld classes
- Create comprehensive tests to validate that all content from JSON files is correctly saved into GameWorld