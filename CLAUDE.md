# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## DOCUMENTATION GUIDELINES

### **NEW SESSION STARTUP CHECKLIST**
**CRITICAL**: Every new session must follow this exact sequence:

1. ✅ **READ CLAUDE.MD FIRST** - Understand architectural patterns and game design principles
2. ✅ **READ SESSION-HANDOFF.MD** - Get current progress, discoveries, and immediate next steps
3. ✅ **READ IMPLEMENTATION-PLAN-REVISED.MD** - Understand feature priorities and requirements
4. ✅ **READ USERSTORIES.MD** - Understand game design requirements and anti-patterns
5. ✅ **ONLY THEN begin working** - Never start coding without understanding current state

### **DOCUMENTATION ARCHITECTURE**

**CLAUDE.MD** (This file) - **ARCHITECTURAL REFERENCE**
- Core architectural patterns and principles
- Game design principles and constraints
- Code writing guidelines and standards
- High-level system overview
- Key file locations and responsibilities
- **NEVER** add session progress, temporary fixes, or detailed implementation notes

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

### CODE WRITING PRINCIPLES
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated
- You must run all tests and execute the game and do quick smoke tests before every commit

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
Repositories must be stateless and access GameWorld.WorldState dynamically.
- ✅ Correct: `private readonly GameWorld _gameWorld` + `_gameWorld.WorldState`
- ❌ Wrong: `private WorldState` caching

#### **GameWorld Single Source of Truth**
GameWorld.WorldState is the authoritative source for all game state.
- All game state changes must go through WorldState
- GameWorld contains no business logic, only state management

#### **Service Configuration**
- Production: Use `ConfigureServices()` for full AI stack
- Testing: Use `ConfigureTestServices()` for economic-only functionality
- No duplicate service registrations

#### **Method Signatures**
All APIs must be location-aware and consistent:
- ✅ Correct: `GetItemPrice(string locationId, string itemId, bool buying)`
- ❌ Wrong: Legacy item-based overloads without location context

### CURRENT SYSTEM STATUS

**For current progress, session handoffs, and next steps, see:** `session-handoff.md`
**For implementation roadmap, see:** `implementation-plan-revised.md`
**For game design requirements, see:** `UserStories.md`

**Overall Compliance**: 🟢 **FULLY COMPLIANT** - All major architectural patterns enforced
**Game Design Compliance**: 🟢 **ALIGNED** - POC follows game vs app principles

### CURRENT POC STATUS
- ✅ **Foundation Systems**: Time blocks, stamina constraints, dynamic pricing complete
- ✅ **Contract System**: Time-pressured delivery contracts with deadline enforcement
- ✅ **Route Condition Variations**: Weather/time/event-based route changes with strategic depth
- 📋 **Remaining Systems**: Discovery/progression system, code style cleanup
- ❌ **Rejected Features**: Automated planning tools, profit calculators, optimization assistants

### KEY LOCATIONS IN CODEBASE

#### **Core Game Management**
- `src/GameState/GameWorldManager.cs` - Central coordinator, UI gateway
- `src/GameState/GameWorld.cs` - Single source of truth for game state

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

### TESTING APPROACH

#### **Economic Testing (No AI)**
Use `ConfigureTestServices()` for testing core economic functionality without AI dependencies.

#### **Full Integration Testing**
Use `ConfigureServices()` for full system testing including AI narrative features.

#### **Test-Driven Development**
Always write failing tests before fixing bugs to ensure proper understanding and validation.

### COMMON PATTERNS TO MAINTAIN

#### **Error-Free Initialization**
- All location properties must be properly initialized to prevent null reference exceptions
- UI screens must check `IsGameDataReady()` before rendering
- Player location and spot must never be null after initialization

#### **Consistent Data Access**
- Always access current location via `GameWorld.WorldState.CurrentLocation`  
- Never use the legacy `GameWorld.CurrentLocation` property (always null)
- Use location-aware method signatures throughout

#### **Service Dependency Management**
- AI services are optional for economic functionality
- Use nullable dependencies and factory patterns for services that might not be available
- Test configuration should provide minimal viable services only

## GAME SYSTEMS REFERENCE

### CONTRACT SYSTEM (`src/GameState/Contract.cs`, `src/Game/MainSystem/ContractSystem.cs`)

#### **Core Architecture**
- **Contract**: Time-bound delivery contracts with item/location requirements
- **ContractRepository**: Stateless access to contract data with availability filtering
- **ContractSystem**: Business logic for completion, failure detection, and time pressure
- **Integration**: Fully integrated with TimeManager for deadline enforcement

#### **Key Features**
- **Time Pressure**: Contracts have StartDay/DueDay deadlines creating urgency
- **Completion Requirements**: Must be at DestinationLocation with RequiredItems
- **Automatic Failure**: Contracts fail when CurrentDay > DueDay
- **Contract Chaining**: UnlocksContractIds/LocksContractIds for dependency management
- **Payment System**: Coins awarded on completion, penalties applied on failure

#### **Game Design Alignment**
- **Time Optimization**: Players must balance travel time vs contract deadlines
- **Resource Management**: Contract payments drive economic decisions
- **Discovery**: No automatic contract suggestions - players choose from available pool
- **Meaningful Choices**: Accept contracts vs explore other opportunities

#### **Current Implementation Status**
- ✅ **Core System**: Complete with full time pressure mechanics
- ✅ **UI Integration**: Functional contract display and completion interface
- ✅ **Test Coverage**: Comprehensive test suite for deadline enforcement
- ✅ **Content Loading**: JSON template system for contract configuration
- ⚠️ **Enhancement Ready**: Foundation solid for advanced time pressure features

### TIME MANAGEMENT SYSTEM (`src/GameState/TimeManager.cs`)

#### **Core Structure**
- **Daily Limit**: 5 time blocks per day (MaxDailyTimeBlocks)
- **Time Windows**: Dawn, Morning, Afternoon, Evening, Night
- **Constraint Enforcement**: Actions consume time blocks, creating strategic pressure
- **New Day Reset**: Time blocks reset on day advancement

#### **Integration Points**
- **Contract System**: Deadline enforcement based on CurrentDay
- **Travel System**: Routes consume time blocks, affecting contract viability
- **Rest System**: Recovery requires time block consumption
- **Action System**: All major actions consume time resources

#### **Game Design Impact**
- **Strategic Pressure**: Limited time blocks force prioritization decisions
- **Contract Urgency**: Time pressure makes deadline planning critical
- **Resource Scarcity**: Time as a finite resource creates meaningful choices