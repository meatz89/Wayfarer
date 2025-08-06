# Wayfarer Game Architecture

## Critical Architecture Principles

### 1. NO SILENT BACKEND ACTIONS

**CRITICAL**: Nothing should happen silently in the backend. This is fundamental to the game's design philosophy.

#### The Rule
- If automatic, the player MUST be notified via MessageSystem
- If manual, the player MUST click a button to initiate
- All game state changes must be visible and intentional

#### Why This Matters
1. **Player Agency**: Players should understand every action and consequence
2. **Debugging**: Visible actions make it easier to track game flow
3. **Trust**: Players trust systems they can see and understand
4. **Game Design**: The pressure of visible obligations is core to the experience

#### Examples
- ✅ CORRECT: Letter expiration shows message: "💀 Elena's letter expired! Lost 2 Trust tokens"
- ❌ WRONG: Tokens silently generate special letters in background
- ✅ CORRECT: Player clicks "Accept Introduction" to unlock new NPC
- ❌ WRONG: Reaching token threshold automatically unlocks content

### 2. GameWorld Initialization (NEVER CHANGE THIS)

**CRITICAL**: GameWorld MUST be initialized through a static GameWorldInitializer class. This is the foundation of the entire game startup process.

#### Why This Pattern Exists

1. **Prevents Circular Dependencies**: During ServerPrerendered mode, Blazor components are rendered on the server before the SignalR connection is established. If GameWorld creation requires dependency injection, it creates circular dependencies that cause the application to hang.

2. **Clean Startup**: GameWorld is the root aggregate of the entire game state. It must be created cleanly without dependencies.

3. **Singleton Guarantee**: By creating GameWorld through a static method and registering it as a singleton, we ensure only one instance exists throughout the application lifetime.

#### The Pattern

```csharp
// In GameWorldInitializer.cs
public static class GameWorldInitializer
{
    public static GameWorld CreateGameWorld()
    {
        // Create GameWorld with default content directory
        // No dependency injection needed
    }
}

// In ServiceConfiguration.cs
services.AddSingleton<GameWorld>(_ =>
{
    // Call static initializer - no DI dependencies
    return GameWorldInitializer.CreateGameWorld();
});
```

#### What Will Break If You Change This

1. **ServerPrerendered Mode Will Hang**: The application will freeze when trying to load the page
2. **Circular Dependencies**: Services that depend on GameWorld won't be able to resolve
3. **Startup Failures**: The entire application startup sequence will fail

#### Tests That Enforce This

- `GameWorldInitializationTests.cs` - Verifies GameWorldInitializer remains static
- `ArchitectureTests.cs` - Ensures no service locator patterns
- `StartupValidationTests.cs` - Validates startup doesn't hang

### 2. Dependency Flow Direction

**Rule**: All dependencies flow INWARD towards GameWorld, never outward from it.

```
UI Components (Blazor)
    ↓
IGameFacade (Single Interface)
    ↓
GameFacade (Implementation)
    ↓
UIServices (Domain Translation)
    ↓
Managers/Services
    ↓
Repositories
    ↓
GameWorld (No Dependencies)
```

GameWorld is the single source of truth and has NO dependencies on any services, managers, or external components.

### 3. GameFacade Architecture Pattern

**Rule**: UI components MUST only interact with the backend through IGameFacade.

#### The Pattern

The GameFacade pattern provides THE ONLY way UI components and test controllers should communicate with the game backend. This architectural pattern was implemented to solve multiple critical issues:

1. **Eliminated Circular Dependencies**: Previously, UI components directly injected 30+ services, creating complex dependency graphs that caused startup hangs
2. **Improved Testability**: UI components can now be tested with simple mock IGameFacade implementations
3. **Enforced Clean Architecture**: Clear separation between presentation and domain layers

#### Implementation Details

```csharp
// Before: MainGameplayView had 30+ service injections
@inject GameWorld GameWorld
@inject TravelManager TravelManager
@inject CommandExecutor CommandExecutor
@inject NPCRepository NPCRepository
// ... 26 more injections

// After: Single facade injection
@inject IGameFacade GameFacade
```

#### ViewModels for Data Transfer

All data returned from GameFacade uses ViewModels to prevent domain objects from leaking to the UI layer:

- **ConversationViewModel** - Conversation state and choices
- **TravelDestinationViewModel** - Available travel destinations
- **TravelRouteViewModel** - Route details with token requirements
- **InventoryViewModel** - Player inventory state
- **LetterQueueViewModel** - Letter queue management
- **LocationActionsViewModel** - Available actions at current location
- **NPCRelationshipViewModel** - NPC relationship tracking
- **ObligationViewModel** - Player obligations and debts

#### Benefits Achieved

1. **Startup Performance**: Eliminated circular dependency resolution that caused hangs
2. **Test Coverage**: UI components can be unit tested in isolation
3. **Maintainability**: Backend changes don't break UI components
4. **Consistency**: All UI components interact with backend the same way
5. **Documentation**: Single interface documents all available UI operations

### 4. Navigation Architecture

**Rule**: GameUIBase is the ONLY navigation handler in the application.

- GameUIBase (at @page "/") controls all navigation
- MainGameplayView is a regular component, NOT a navigation handler
- No NavigationService with events or delegates
- Simple component-based navigation using CurrentView property

### 5. No Service Locator Pattern

**Rule**: Never use GetRequiredService outside of ServiceConfiguration.

Bad:
```csharp
public class SomeService
{
    public void DoSomething(IServiceProvider provider)
    {
        var gameWorld = provider.GetRequiredService<GameWorld>(); // NO!
    }
}
```

Good:
```csharp
public class SomeService
{
    private readonly GameWorld _gameWorld;
    
    public SomeService(GameWorld gameWorld) // Inject through constructor
    {
        _gameWorld = gameWorld;
    }
}
```

## Testing Strategy

### 1. Unit Tests
- Test individual components in isolation
- Verify GameWorld can be created without dependencies
- Ensure static initialization pattern
- Mock IGameFacade for UI component testing

### 2. Architecture Tests
- Enforce architectural rules through reflection
- Prevent accidental breaking changes
- Validate dependency directions
- Ensure UI components only depend on IGameFacade

### 3. Integration Tests
- Test the full startup sequence
- Verify no hangs during prerendering
- Ensure all services can be resolved
- Test GameFacade delegates correctly to UIServices

### 4. E2E Tests
- Validate the entire game can start
- Check critical services are available
- Ensure UI renders properly
- Test complete UI workflows through IGameFacade

## Common Pitfalls to Avoid

1. **Making GameWorldInitializer Non-Static**: This will break ServerPrerendered mode
2. **Adding Dependencies to GameWorld**: This violates the core architecture
3. **Using Events in Navigation**: This creates circular dependencies
4. **Service Locator Anti-Pattern**: Always use constructor injection
5. **Direct Service Injection in UI**: UI components must ONLY use IGameFacade
6. **Exposing Domain Objects to UI**: Always use ViewModels for data transfer
7. **Bypassing GameFacade**: Never access backend services directly from UI

## Monitoring Architecture Health

Run these commands regularly:

```bash
# Run architecture tests
dotnet test --filter "FullyQualifiedName~ArchitectureTests"

# Run initialization tests
dotnet test --filter "FullyQualifiedName~GameWorldInitializationTests"

# Run startup validation
dotnet test --filter "FullyQualifiedName~StartupValidationTests"

# Run GameFacade pattern tests
dotnet test --filter "FullyQualifiedName~GameFacadeTests"

# Check for direct service usage in UI
grep -r "@inject.*Service" src/Pages/ --include="*.razor"
grep -r "@inject.*Manager" src/Pages/ --include="*.razor"
grep -r "@inject.*Repository" src/Pages/ --include="*.razor"
```

## When to Revisit This Architecture

This architecture should ONLY be changed if:

1. Moving away from Blazor Server to a different framework
2. Fundamentally changing how game state is managed
3. Complete rewrite of the game engine

Even then, the principle of static initialization for the root aggregate should be maintained.

## Token System Architecture

### Connection Token Types

The game uses four connection token types that represent HOW you relate to NPCs, not WHO they are:

1. **Trust** - Personal bonds and emotional connections
2. **Commerce** - Business relationships and trade networks  
3. **Status** - Social standing and noble connections
4. **Shadow** - Underground and illicit connections

### Information Discovery System

The game features a two-phase progression system for discovering game content:

1. **Learn Existence** - First discover that something exists (NPC, location, mechanic)
2. **Gain Access** - Then earn the right to interact with it through tokens, permissions, or capabilities

### Special Letters

Four types of special letters provide unique mechanics:

1. **Introduction Letters** (Trust) - Introduce you to new NPCs in your trust network
2. **Access Permits** (Commerce) - Grant access to restricted commercial locations
3. **Endorsements** (Status) - Vouch for your standing in noble circles
4. **Information Letters** (Shadow) - Reveal hidden knowledge and opportunities

### Tier System

Everything in the game has tiers 1-5 with triple-gated access:

1. **Knowledge Gate** - Must know it exists
2. **Permission Gate** - Must have access rights (tokens, permits, endorsements)
3. **Capability Gate** - Must have resources/skills to actually use it

### Standing Obligations

Each token type has unique debt mechanics when going negative:

- **Trust Debt** - Personal betrayals affecting letter deadlines
- **Commerce Debt** - Business leverage affecting letter positions
- **Status Debt** - Social obligations restricting refusal options
- **Shadow Debt** - Dangerous entanglements with severe consequences

## UI Completeness Requirements

### Action Pipeline Audit Results

As of 2025-07-30, an action pipeline audit revealed that approximately **30% of backend game mechanics lack UI exposure**. This violates our core architecture principle that all game features must be player-accessible.

### Architecture Principle: Complete UI Coverage

**Rule**: Every backend command, manager, and game mechanic MUST have corresponding UI exposure.

#### Why This Matters

1. **Player Experience**: Hidden features are effectively non-existent from the player's perspective
2. **Design Validation**: Features without UI cannot be tested or validated through play
3. **Code Waste**: Backend code without UI is dead code that adds maintenance burden

#### Required UI Elements for Each Command

```csharp
// For every CommandType enum value:
public enum CommandType
{
    Work,           // ✅ Exposed in LocationActions
    Converse,       // ✅ Exposed in ConversationView
    Travel,         // ✅ Exposed in TravelSelection
    GatherResources,// ❌ NO UI - Need "Gather" button at FEATURE locations
    BorrowMoney,    // ❌ NO UI - Need borrowing interface in conversations
    Browse,         // ❌ NO UI - Need browse action at shops
    // ... etc
}
```

#### UI Coverage Checklist

1. **Command Discovery**: Every command must be discoverable through UI
2. **Cost Display**: All costs (time, stamina, coins, tokens) must be visible
3. **Error Feedback**: Failed commands must show clear error messages
4. **Progress Indication**: Long-running commands need progress feedback
5. **Result Display**: Command outcomes must be clearly communicated

#### Missing UI Elements (High Priority)

1. **Economic Commands**:
   - GatherResourcesCommand - Resource gathering at FEATURE locations
   - BorrowMoneyCommand - Borrowing interface in NPC conversations
   - BrowseCommand - Discovery interface at shops/markets

2. **Social Commands**:
   - ShareLunchCommand - Meal sharing with NPCs
   - KeepSecretCommand - Secret-keeping interactions
   - PersonalErrandCommand - Personal favor system
   - EquipmentSocializeCommand - Equipment-based social interactions

3. **System Features**:
   - Route Discovery - Active exploration interface
   - Standing Obligations - Interaction and resolution mechanics
   - Transport Methods - Selection beyond hardcoded "Walking"

### Testing UI Completeness

```csharp
// Test that verifies all commands have UI
[Test]
public void AllCommandTypes_ShouldHaveUI()
{
    var allCommands = Enum.GetValues<CommandType>();
    var uiExposedCommands = GetUIExposedCommands();
    
    var missingUI = allCommands.Except(uiExposedCommands);
    
    Assert.That(missingUI, Is.Empty, 
        $"Commands without UI: {string.Join(", ", missingUI)}");
}
```

### Enforcement

1. **Code Review**: No new commands without corresponding UI
2. **Testing**: UI exposure tests must pass before merge
3. **Documentation**: Update UI documentation when adding commands
4. **Audit**: Quarterly review of command-UI mapping

This architecture principle ensures that Wayfarer's rich backend mechanics are fully accessible to players, maximizing the value of implemented features.