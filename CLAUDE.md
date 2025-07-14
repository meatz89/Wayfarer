# CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE BEFORE WRITING TO IT ⚠️**

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### **NEW SESSION STARTUP CHECKLIST**
**CRITICAL**: Every new session must follow this exact sequence:

1. ✅ **READ CLAUDE.MD FIRST** - Understand architectural patterns and game design principles
2. ✅ **READ SESSION-HANDOFF.MD** - Get current progress, discoveries, and immediate next steps
3. ✅ **READ GAME-ARCHITECTURE.md** - Acquire a deep understanding of the architecture guidelines and principles
4. ✅ **READ INTENDED-GAMEPLAY.md** - Acquire a deep understanding of what we want the game experience to feel like for the player
5. ✅ **READ LOGICAL-SYSTEM-INTERACTIONS.MD** - Critical design guidelines for system changes
6. ✅ **ONLY THEN begin working** - Never start coding without understanding current state

### **DOCUMENTATION MAINTENANCE RULES**

1. ✅ **ALWAYS update session-handoff.md** - Document all discoveries, progress, and next steps
2. ✅ **ONLY update claude.md** - When architectural patterns change or new principles are discovered
3. ✅ **NEVER add temporary status** - Session progress goes in session-handoff.md, not claude.md
4. ✅ **Document user feedback immediately** - Critical constraints and discoveries go in session-handoff.md
5. ✅ **Keep files focused** - Each file has a specific purpose and audience
6. ✅ **Reference related files** - Always point to where related information can be found

## PROJECT: WAYFARER

**Wayfarer** is a medieval life simulation RPG built as a Blazor Server application (.NET 8.0). It features a sophisticated, AI-driven narrative system with turn-based resource management gameplay focused on economic strategy, travel optimization, and contract fulfillment.

## COMMON DEVELOPMENT TASKS

### Build, Run, and Test
```bash
# Build the project
dotnet build

# Run the application
dotnet run --project src/Wayfarer.csproj

# Run all tests
dotnet test

# Run specific test file
dotnet test --filter "FullyQualifiedName~TestClassName"

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"
```

### AI Service Setup (Optional)
```bash
# Default AI provider is Ollama (local)
# Start Ollama service with docker-compose
docker-compose up -d

# Alternative: Configure other AI providers in appsettings.json
```

## HIGH-LEVEL ARCHITECTURE

### Core Design Philosophy: Categorical Interconnection

The game follows a **categorical interconnection** principle where gameplay emerges from logical system interactions rather than arbitrary mathematical modifiers:

1. **All entities have unique types/categories** that interact with other system categories
2. **Game rules emerge from category interactions** instead of hardcoded bonuses/penalties
3. **Constraints require multiple systems** - no single system creates arbitrary restrictions
4. **Categories enable discovery gameplay** - players learn through experimentation
5. **All categories must be visible in UI** - players cannot strategize about invisible systems

### Architectural Patterns

#### Repository-Mediated Access (CRITICAL)
**ALL game state access MUST go through entity repositories.**

```csharp
// ❌ WRONG: Direct access
gameWorld.WorldState.Items.Add(item);

// ✅ CORRECT: Repository-mediated
itemRepository.AddItem(item);
```

#### UI Access Patterns
- **For Actions (State Changes)**: UI → GameWorldManager → Specific Manager
- **For Queries (Reading State)**: UI → Repository → GameWorld.WorldState

#### Stateless Repositories
Repositories MUST be completely stateless and only delegate to GameWorld.

*See GAME-ARCHITECTURE.md for detailed enforcement rules and patterns.*

### Game Initialization Pipeline

```
JSON Files → GameWorldSerializer → GameWorldInitializer → GameWorld → Repositories
```

**Key JSON Content Files** (`src/Content/Templates/`):
- `locations.json` - Game locations with properties
- `location_spots.json` - Specific spots within locations
- `routes.json` - Travel routes with terrain categories
- `items.json` - Items with equipment/item categories
- `contracts.json` - Available contracts
- `actions.json` - Player actions at locations

### Project Structure

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

### Testing Architecture

- **Framework**: xUnit
- **Test Isolation**: Each test class uses its own JSON content
- **NEVER use production JSON in tests** - create test-specific data

*See GAME-ARCHITECTURE.md for detailed testing patterns and requirements.*

### Code Quality

The project uses multiple code analyzers:
- Microsoft.CodeAnalysis.Analyzers
- Roslynator.Analyzers
- SonarAnalyzer.CSharp

Analysis is configured in `wayfarer.ruleset` with enforcement during build.

### Key Design Principles

1. **Emergent Mechanics**: Never hardcode restrictions. All constraints emerge from mathematical interactions between simple systems (time, stamina, coins).
2. **Player Agency**: Players always retain choice but face natural consequences.
3. **Discovery Through Play**: Systems are discoverable through experimentation, not tutorials.
4. **Gameplay in Player's Head**: Fun comes from optimization puzzles, not automated conveniences.
5. **No Hidden Mechanics**: All categories and relationships must be visible in UI.

### Categorical Interconnection Requirements

**All entities must have unique types/categories** that interact with other system categories:
- Items: `EquipmentCategory` (Climbing_Equipment, Weather_Protection, Navigation_Tools, etc.)
- Routes: `TerrainCategory` (Requires_Climbing, Exposed_Weather, Wilderness_Terrain, etc.)

**Game rules emerge from category interactions** instead of hardcoded bonuses/penalties:
- Weather + Terrain → Access requirements
- Equipment + Terrain → Capability enablement  
- NPC Profession + Location Type → Service availability

**Core Design Rules:**
- **NEVER** use arbitrary mathematical modifiers (efficiency multipliers, percentage bonuses, etc.)
- **ALWAYS** implement logical blocking/enabling instead of sliding scale penalties
- **ENSURE** all entity categories are visible and understandable in the UI

*See GAME-ARCHITECTURE.md for detailed categorical system implementations.*

### CODE WRITING PRINCIPLES
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated
- You must run all tests and execute the game and do quick smoke tests before every commit
- **Never keep legacy code for compatibility**
- **NEVER use suffixes like "New", "Revised", "V2", etc.** - Replace old implementations completely and use the correct final name immediately. Delete old code, don't leave it behind.
- **CRITICAL: NEVER USE REFLECTION** - If you find ANY reflection usage in the codebase:
  1. **IMMEDIATELY create highest priority TODO** to remove the reflection
  2. **STOP all other work** - reflection makes code unmaintainable and breaks refactoring
  3. **Fix it properly** - make fields public, add proper accessors, or redesign the architecture
  4. **NO EXCEPTIONS** - There is never a valid reason to use reflection in production code
- **CRITICAL: IMMEDIATE LEGACY CODE ELIMINATION** - If you discover ANY legacy code, compilation errors, or deprecated patterns during development, you MUST immediately:
  1. **CREATE HIGH-PRIORITY TODO ITEM** to fix the legacy code
  2. **STOP current work** and fix the legacy code immediately
  3. **NEVER ignore or postpone** legacy code fixes
  4. **NEVER say "these are just dependency fixes"** - fix them now or create immediate todo items
- **CRITICAL: ARCHITECTURAL BUG DISCOVERY** - If you discover architectural bugs (e.g., duplicate state storage, inconsistent data access patterns), you MUST:
  1. **IMMEDIATELY create highest priority TODO** to fix the architectural issue
  2. **STOP all other work** - architectural bugs corrupt the entire system
  3. **NEVER work around architectural bugs** - fix them at the source
  4. **Document the fix in GAME-ARCHITECTURE.md** for future reference

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


*See GAME-ARCHITECTURE.md for detailed initialization flow and system dependencies.*

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

### Important Documentation

- `INTENDED-GAMEPLAY.md` - Game design philosophy and categorical systems
- `LOGICAL-SYSTEM-INTERACTIONS.md` - System interaction guidelines
- `/docs/` - Extensive game design documentation (40+ documents)