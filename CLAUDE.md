# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## DOCUMENTATION GUIDELINES

**CRITICAL WORKFLOW REMINDERS:**
1. ✅ **ALWAYS read existing 'claude.md' first** - Understand current architecture state
2. ✅ **ALWAYS update 'claude.md' after discovering new information** - Maintain comprehensive documentation  
3. ✅ **NEVER proceed without updating documentation** - When new insights are discovered
4. ✅ **Document architectural changes immediately** - Track all relationships and patterns
5. ✅ **VERIFY DOCUMENTATION IN EVERY COMMIT** - Follow post-commit validation workflow
6. 🧹 **KEEP CLAUDE.MD HIGH-LEVEL** - This should contain architectural patterns, core systems, and essential guidance. Do NOT add detailed session progress notes, step-by-step fixes, or temporary status updates. Move those to separate session notes or remove them after completion.

## DEVELOPMENT GUIDELINES

### CODE WRITING PRINCIPLES
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated

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

**Overall Compliance**: 🟢 **FULLY COMPLIANT** - All major architectural patterns enforced

#### **✅ Fully Working Systems:**
- **UI Screens**: Travel, Market, Rest, Contracts all functional
- **Data Flow**: UI → GameWorldManager gateway pattern enforced
- **State Management**: Stateless repositories, GameWorld single source of truth
- **Service Configuration**: Clean configuration, test infrastructure working
- **Method APIs**: Consistent location-aware signatures throughout

#### **🧪 Test Infrastructure**
- `ConfigureTestServices()` provides AI-free economic functionality for testing
- All core UI functionality tests passing
- Tests use mock/null services for AI components

### KEY LOCATIONS IN CODEBASE

#### **Core Game Management**
- `src/GameState/GameWorldManager.cs` - Central coordinator, UI gateway
- `src/GameState/GameWorld.cs` - Single source of truth for game state

#### **Repository Pattern**
- `src/Content/LocationRepository.cs` - Stateless location data access
- `src/Content/ActionRepository.cs` - Stateless action data access  
- `src/Content/ItemRepository.cs` - Stateless item data access

#### **Business Logic**
- `src/GameState/TravelManager.cs` - Travel and routing logic
- `src/GameState/MarketManager.cs` - Trading and pricing logic
- `src/GameState/TradeManager.cs` - Transaction processing
- `src/GameState/RestManager.cs` - Rest and recovery logic

#### **Service Configuration**
- `src/ServiceConfiguration.cs` - Dependency injection setup

#### **UI Components**
- `src/Pages/MainGameplayView.razor` - Main game screen coordinator
- `src/Pages/Market.razor` - Trading interface
- `src/Pages/TravelSelection.razor` - Travel planning interface

### TESTING APPROACH

#### **Economic Testing (No AI)**
Use `ConfigureTestServices()` for testing core economic functionality without AI dependencies.

#### **Full Integration Testing**
Use `ConfigureServices()` for full system testing including AI narrative features.

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