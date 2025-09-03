# Wayfarer Technical Architecture Document

## Table of Contents
1. [Project Structure](#project-structure)
2. [Core Architectural Patterns](#core-architectural-patterns)
3. [Critical Files and Orchestrators](#critical-files-and-orchestrators)
4. [Service Registration and DI](#service-registration-and-di)
5. [UI Component Hierarchy](#ui-component-hierarchy)
6. [Subsystem Architecture](#subsystem-architecture)
7. [Data Flow Pipelines](#data-flow-pipelines)
8. [State Management](#state-management)
9. [Architectural Rules and Anti-Patterns](#architectural-rules-and-anti-patterns)
10. [Testing and Development](#testing-and-development)

---

## Project Structure

### Directory Tree Structure
```
/mnt/c/git/wayfarer/
├── src/                                   # Main application source
│   ├── Content/                           # Content loading and validation
│   │   ├── DTOs/                         # Data Transfer Objects for JSON parsing
│   │   ├── Utilities/
│   │   │   └── EnumParser.cs            # Case-insensitive enum parsing
│   │   ├── Validation/
│   │   │   ├── ContentValidationPipeline.cs
│   │   │   ├── IContentValidator.cs
│   │   │   └── Validators/
│   │   │       ├── BaseValidator.cs     # Base class with TryGetPropertyCaseInsensitive
│   │   ├── GameWorldInitializer.cs      # CRITICAL: Static GameWorld creation
│   │   ├── PackageLoader.cs             # CRITICAL: JSON package orchestrator
│   │   ├── SkeletonGenerator.cs         # Lazy content placeholder system
│   │   ├── CardEffectProcessor.cs       # Card effect parsing
│   │
│   ├── Repositories
│   │
│   ├── GameState/                       # CRITICAL: Core game state / game systems
│   │   ├── Constants/
│   │   │   └── GameConstants.cs
│   │   ├── Intents/
│   │   │   └── PlayerIntent.cs         # Intent-based architecture
│   │   ├── Operations/                  # Atomic state operations
│   │   ├── StateContainers/            # Strongly typed state containers
│   │   ├── StrongTypes/                # Type-safe data structures
│   │   ├── GameWorld.cs                # CRITICAL: Single source of truth
│   │   ├── WorldState.cs               # World data collections
│   │   ├── ConversationContext.cs      # Context for conversation UI
│   │   ├── ConversationCard.cs         # Unified card type
│   │   └── [60+ other state classes]
│   │
│   ├── Models/
│   │   ├── Package.cs                  # Content package model
│   │
│   ├── Pages/                          # Blazor UI components
│   │   ├── Components/                 # Child components
│   │   │   ├── ConversationContent.razor(.cs)  # Screen content component
│   │   │   ├── LocationContent.razor(.cs)       # Screen content component
│   │   │   ├── MessageDisplay.razor(.cs)        # Toast notifications
│   │   │   ├── ObligationQueueContent.razor(.cs) # Screen content component
│   │   │   ├── TravelContent.razor(.cs)         # Screen content component
│   │   ├── GameScreen.razor(.cs)       # CRITICAL: Authoritative parent component
│   │   ├── GameUI.razor(.cs)           # Root navigation handler @ "/"
│   │
│   ├── Services/                       # High-level services
│   │   ├── GameFacade.cs              # CRITICAL: Single UI-Backend interface
│   │   ├── DevModeService.cs
│   │
│   ├── Subsystems/                     # CRITICAL: Specialized subsystems
│   │   ├── Conversation/
│   │   │   ├── ConversationFacade.cs   # Facade for conversation subsystem
│   │   │   ├── ConversationOrchestrator.cs # Main conversation logic
│   │   ├── Location/
│   │   │   ├── LocationFacade.cs       # Facade for location subsystem
│   │   ├── Obligation/
│   │   │   ├── ObligationFacade.cs     # Facade for obligation subsystem
│   │   ├── Resource/
│   │   │   ├── ResourceFacade.cs       # Facade for resource subsystem
│   │   ├── Time/
│   │   │   ├── TimeFacade.cs           # Facade for time subsystem
│   │   ├── Travel/
│   │   │   ├── TravelFacade.cs         # Facade for travel subsystem
│   │   ├── Market/
│   │   │   ├── MarketFacade.cs         # Facade for market subsystem
│   │   ├── Token/
│   │   │   ├── TokenFacade.cs          # Facade for token subsystem
│   │   └── Narrative/
│   │       ├── NarrativeFacade.cs      # Facade for narrative subsystem
│   │
│   ├── Tests/                          # Unit tests (limited)
│   │
│   ├── UIHelpers/
│   │
│   ├── ViewModels/
│   │   ├── GameFacadeViewModels.cs     # ViewModels for GameFacade returns
│   │
│   ├── wwwroot/                        # Static web assets
│   │   ├── css/
│   │   └── data/
│   │       └── [static data files]
│   │
│   ├── App.razor                       # Blazor app root
│   ├── _Imports.razor                  # Global using statements
│   ├── GlobalUsings.cs                 # Global C# using statements
│   ├── Program.cs                      # Application entry point
│   └── ServiceConfiguration.cs         # CRITICAL: DI configuration
│
├── Wayfarer.Tests.Project/              # Test project (separate)
├── docs/                                # Design documentation
├── ui-mockups/                          # HTML mockups for UI design
└── Content/                             # JSON content packages
    └── Core/
        └── core_game_package.json      # Main game content
```

---

## Core Architectural Patterns

### 1. Facade Pattern with Subsystem Isolation
**Pattern**: Each major system has a Facade that provides a simplified interface to complex subsystem operations.

**Implementation**:
- `/src/Services/GameFacade.cs` - Master orchestrator for all UI-Backend communication
- `/src/Subsystems/Conversation/ConversationFacade.cs` - Conversation subsystem interface
- `/src/Subsystems/Location/LocationFacade.cs` - Location subsystem interface
- `/src/Subsystems/Obligation/ObligationFacade.cs` - Obligation queue interface
- `/src/Subsystems/Resource/ResourceFacade.cs` - Resource management interface
- `/src/Subsystems/Time/TimeFacade.cs` - Time system interface
- `/src/Subsystems/Travel/TravelFacade.cs` - Travel system interface
- `/src/Subsystems/Token/TokenFacade.cs` - Token system interface
- `/src/Subsystems/Narrative/NarrativeFacade.cs` - Narrative generation interface

**Key Principle**: GameFacade ONLY orchestrates - it contains NO business logic itself.

### 2. GameWorld as Single Source of Truth
**Pattern**: All game state lives in GameWorld with zero external dependencies.

**Implementation**:
- `/src/GameState/GameWorld.cs` - Contains ALL game state
- `/src/GameState/WorldState.cs` - World data collections
- `/src/GameState/Player.cs` - Player state

**Key Rules**:
- GameWorld has NO dependencies on services or managers
- GameWorld is created via static `GameWorldInitializer.CreateGameWorld()`
- All systems read from and write to GameWorld
- NO state storage in services or managers

### 3. Static Initialization Pattern
**Pattern**: GameWorld initialization happens statically to avoid circular dependencies.

**Implementation**:
```csharp
// ServiceConfiguration.cs:22-29
services.AddSingleton<GameWorld>(_ =>
{
    // Call GameWorldInitializer statically - no DI dependencies needed
    GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();
    return gameWorld;
});
```

**Key Files**:
- `/src/Content/GameWorldInitializer.cs` - Static creation logic
- `/src/Content/PackageLoader.cs` - Content loading orchestration
- `/src/ServiceConfiguration.cs` - Service registration

### 4. Authoritative Parent Component Pattern (UI)
**Pattern**: GameScreen is the authoritative parent managing all screen state.

**Implementation**:
- `/src/Pages/GameScreen.razor(.cs)` - Authoritative parent
- `/src/Pages/GameUI.razor(.cs)` - Root navigation handler @ "/"
- Child components receive parent via `CascadingValue`

**Component Hierarchy**:
```
App.razor
└── GameUI.razor (@ "/") [INavigationHandler]
    └── GameScreen.razor [Authoritative Parent]
        ├── LocationContent.razor
        ├── ConversationContent.razor
        ├── ObligationQueueContent.razor
        └── TravelContent.razor
```

### 5. Context Object Pattern
**Pattern**: Complex operations use dedicated Context classes for atomic data passing.

**Implementation**:
- `/src/GameState/ConversationContext.cs` - Conversation state context
- `/src/GameState/SceneContext.cs` - Scene transition context

**Usage Example**:
```csharp
// GameScreen creates context atomically
CurrentConversationContext = await GameFacade.CreateConversationContext(npcId, type);
// Passes to child component
<ConversationContent Context="@CurrentConversationContext" />
```

### 6. Intent-Based Architecture (Not Command Pattern)
**Pattern**: Player actions are expressed as intents, not commands.

**Implementation**:
- `/src/GameState/Intents/PlayerIntent.cs` - Intent definitions
- Commands folder is DEPRECATED

### 7. Skeleton Pattern for Lazy Loading
**Pattern**: Missing content gets skeleton placeholders that are replaced when real content loads.

**Implementation**:
- `/src/Content/SkeletonGenerator.cs` - Creates deterministic skeletons
- Skeletons tracked in `GameWorld.SkeletonRegistry`

---

## Critical Files and Orchestrators

### Entry Points
1. **`/src/Program.cs`** - Application entry point
   - Configures Blazor Server
   - Calls `ServiceConfiguration.ConfigureServices()`
   - Maps routes

2. **`/src/ServiceConfiguration.cs`** - CRITICAL: DI configuration
   - Registers all services in correct order
   - Creates GameWorld via static initializer
   - Wires up subsystem facades

3. **`/src/Content/GameWorldInitializer.cs`** - CRITICAL: GameWorld creation
   - Static method `CreateGameWorld()`
   - Calls PackageLoader
   - No DI dependencies

### Master Orchestrators
4. **`/src/Services/GameFacade.cs`** - CRITICAL: UI-Backend interface
   - Single entry point for ALL UI operations
   - Delegates to subsystem facades
   - Pure orchestration, NO business logic

5. **`/src/Content/PackageLoader.cs`** - CRITICAL: Content loading
   - Loads JSON packages
   - Handles phased loading
   - Creates skeletons for missing content

6. **`/src/Pages/GameScreen.razor.cs`** - CRITICAL: UI orchestrator
   - Authoritative parent for all screens
   - Manages screen navigation
   - Owns all UI state

### Core State Container
7. **`/src/GameState/GameWorld.cs`** - CRITICAL: State container
   - Single source of truth
   - Zero dependencies
   - Contains all game state

---

## Service Registration and DI

### Registration Order (Critical!)
```csharp
// ServiceConfiguration.cs order:

1. DevModeService                    // Early for other services
2. IContentDirectory                  // Configuration
3. GameConfiguration                  // Game config
4. IGameRuleEngine                   // Rule engine
5. GameWorld (static creation)       // CRITICAL: No DI dependencies
6. ContentValidator                  // Validation
7. NPCVisibilityService             // Core service
8. Repositories                      // Data access
9. Core Systems                      // CharacterSystem, MessageSystem, etc.
10. TimeSystem                       // Time management
11. Managers                         // TravelManager, MarketManager, etc.
12. Subsystem Components             // Individual managers
13. Subsystem Facades               // Facade interfaces
14. GameFacade                      // Master orchestrator
15. UI Services                     // LoadingStateService, etc.
```

### Critical Registration Pattern
```csharp
// GameWorld MUST be created statically:
services.AddSingleton<GameWorld>(_ =>
{
    GameWorld gameWorld = GameWorldInitializer.CreateGameWorld();
    return gameWorld;
});

// Subsystems follow namespace pattern:
services.AddSingleton<Wayfarer.Subsystems.LocationSubsystem.LocationManager>();
services.AddSingleton<Wayfarer.Subsystems.LocationSubsystem.LocationFacade>();
```

---

## UI Component Hierarchy

### Navigation Flow
```
Browser Request → /_Host (fallback)
              → App.razor
              → MainLayout.razor (empty)
              → GameUI.razor @ "/" (INavigationHandler)
              → GameScreen.razor (Authoritative Parent)
              → Content Components
```

### Component Responsibilities

#### Root Components
- **`App.razor`** - Blazor app root, Router configuration
- **`_Imports.razor`** - Global using statements
- **`MainLayout.razor`** - Empty layout wrapper

#### Navigation Components
- **`GameUI.razor(.cs)`** - Root navigation handler
  - Implements INavigationHandler
  - Registered with NavigationService
  - Renders GameScreen

#### Authoritative Parent
- **`GameScreen.razor(.cs)`** - CRITICAL: Screen orchestrator
  - Properties: `CurrentScreen`, `ContentVersion`, `CurrentConversationContext`
  - Methods: `StartConversation()`, `HandleNavigation()`, `RefreshUI()`
  - Cascades itself to children via `<CascadingValue Value="@this">`

#### Content Components (Children)
- **`LocationContent.razor(.cs)`** - Location screen content
  - Receives GameScreen via CascadingParameter
  - Calls `GameScreen.StartConversation(npcId, type)`

- **`ConversationContent.razor(.cs)`** - Conversation screen content
  - Receives ConversationContext as Parameter
  - Handles card selection and SPEAK/LISTEN

- **`ObligationQueueContent.razor(.cs)`** - Queue management screen
  - Shows delivery obligations
  - Handles queue manipulation

- **`TravelContent.razor(.cs)`** - Travel selection screen
  - Shows available routes
  - Handles travel initiation

#### Support Components
- **`MessageDisplay.razor`** - Toast notifications
- **`AttentionDisplay.razor`** - Attention bar UI
- **`CardDisplay.razor`** - Card rendering
- **`TokenDisplay.razor`** - Token counts
- **`NPCDeckViewer.razor`** - Dev mode deck viewer

### Component Communication Pattern
```csharp
// Parent exposes public methods:
public class GameScreenBase
{
    public async Task StartConversation(string npcId, ConversationType type)
    {
        CurrentConversationContext = await GameFacade.CreateConversationContext(npcId, type);
        CurrentScreen = ScreenMode.Conversation;
        StateHasChanged();
    }
}

// Child calls parent methods:
[CascadingParameter] public GameScreenBase GameScreen { get; set; }

protected async Task OnNpcClick(string npcId)
{
    await GameScreen.StartConversation(npcId, ConversationType.Standard);
}
```

---

## Subsystem Architecture

### Subsystem Structure Pattern
Each subsystem follows this structure:
```
Subsystems/[Name]/
├── [Name]Facade.cs         # Public interface
├── [Component]Manager.cs   # Business logic components
├── [Helper].cs            # Support classes
└── [Models].cs            # Subsystem-specific models
```

### Conversation Subsystem
**Location**: `/src/Subsystems/Conversation/`

**Components**:
- `ConversationFacade.cs` - Public interface
- `ConversationOrchestrator.cs` - Main conversation flow
- `AtmosphereManager.cs` - Atmosphere state management
- `CardDeckManager.cs` - Deck operations
- `ComfortBatteryManager.cs` - Comfort state (-3 to +3)
- `WeightPoolManager.cs` - Weight pool persistence
- `DialogueGenerator.cs` - Text generation
- `ExchangeHandler.cs` - Quick exchange cards

**Key Operations**:
```csharp
CreateConversationContext(npcId, type)
ProcessCardSelection(context, cardId)
ProcessListen(context)
GetAvailableConversationTypes(npc)
```

### Location Subsystem
**Location**: `/src/Subsystems/Location/`

**Components**:
- `LocationFacade.cs` - Public interface
- `LocationManager.cs` - Location state
- `LocationSpotManager.cs` - Spot management
- `NPCLocationTracker.cs` - NPC positioning
- `LocationActionManager.cs` - Location actions
- `MovementValidator.cs` - Movement rules
- `LocationNarrativeGenerator.cs` - Location text

**Key Operations**:
```csharp
GetCurrentLocation()
GetCurrentLocationSpot()
MoveToSpot(spotName)
GetNPCsAtCurrentSpot()
GetLocationScreen()
```

### Obligation Subsystem
**Location**: `/src/Subsystems/Obligation/`

**Components**:
- `ObligationFacade.cs` - Public interface
- `DeliveryManager.cs` - Letter delivery
- `QueueManipulator.cs` - Queue operations
- `DisplacementCalculator.cs` - Displacement costs
- `DeadlineTracker.cs` - Deadline management
- `MeetingManager.cs` - Meeting obligations

**Key Operations**:
```csharp
AddLetterToQueue(letter, position)
DisplaceObligation(fromPos, toPos)
CompleteDelivery(obligationId)
GetQueueViewModel()
```

### Resource Subsystem
**Location**: `/src/Subsystems/Resource/`

**Components**:
- `ResourceFacade.cs` - Public interface
- `CoinManager.cs` - Coin operations
- `HealthManager.cs` - Health state
- `HungerManager.cs` - Hunger mechanics
- `ResourceCalculator.cs` - Resource math

**Key Operations**:
```csharp
GetPlayerResources()
SpendCoins(amount)
ModifyHealth(delta)
GetAttention(timeBlock)
```

### Time Subsystem
**Location**: `/src/Subsystems/Time/`

**Components**:
- `TimeFacade.cs` - Public interface
- `TimeProgressionManager.cs` - Time advancement
- `TimeBlockCalculator.cs` - Time block logic
- `TimeDisplayFormatter.cs` - Time formatting

**Key Operations**:
```csharp
AdvanceTime(hours)
GetCurrentTimeBlock()
GetFormattedTimeDisplay()
ProcessDayTransition()
```

### Travel Subsystem
**Location**: `/src/Subsystems/Travel/`

**Components**:
- `TravelFacade.cs` - Public interface
- `RouteManager.cs` - Route operations
- `PermitValidator.cs` - Permit checking
- `TravelTimeCalculator.cs` - Travel duration
- `RouteDiscoveryManager.cs` - Route discovery

**Key Operations**:
```csharp
GetAvailableRoutes()
TravelToDestination(routeId)
ValidatePermit(route)
CalculateTravelTime(route)
```

### Token Subsystem
**Location**: `/src/Subsystems/Token/`

**Components**:
- `TokenFacade.cs` - Public interface
- `ConnectionTokenManager.cs` - Token state
- `TokenEffectProcessor.cs` - Token effects
- `RelationshipTracker.cs` - NPC relationships
- `TokenUnlockManager.cs` - Token unlocks

**Key Operations**:
```csharp
GetTokenCount(npcId, tokenType)
AddTokens(npcId, tokenType, amount)
BurnTokensForDisplacement(npcId, amount)
CalculateSuccessBonus(tokens, cardType)
```

### Narrative Subsystem
**Location**: `/src/Subsystems/Narrative/`

**Components**:
- `NarrativeFacade.cs` - Public interface
- `NarrativeRenderer.cs` - Text generation
- `ObservationManagerWrapper.cs` - Observation integration
- `EventNarrator.cs` - Event narration

**Key Operations**:
```csharp
GenerateLocationNarrative(location)
AddSystemMessage(message, type)
CreateObservationCard(template)
```

---

## Data Flow Pipelines

### 1. JSON Content → GameWorld Pipeline
```
JSON Files (core_game_package.json)
    ↓
PackageLoader.LoadPackageAsync()
    ↓
Specialized Parsers (NPCParser, LocationParser, etc.)
    ↓
DTOs → Domain Models
    ↓
GameWorld Collections (NPCs, Locations, Cards, etc.)
```

**Key Files**:
- `/src/Content/PackageLoader.cs` - Orchestrator
- `/src/Content/DTOs/*.cs` - Data transfer objects
- `/src/Content/*Parser.cs` - Specialized parsers
- `/src/Content/GameWorldInitializer.cs` - Integration

### 2. UI Action → State Change Pipeline
```
UI Component (e.g., LocationContent.razor)
    ↓
GameScreen.StartConversation(npcId, type)
    ↓
GameFacade.CreateConversationContext()
    ↓
ConversationFacade.[Operation]()
    ↓
Subsystem Managers
    ↓
GameWorld State Update
    ↓
UI Refresh (StateHasChanged)
```

### 3. Time Advancement Pipeline
```
GameFacade.AdvanceTime(hours)
    ↓
TimeFacade.AdvanceTime()
    ↓
TimeProgressionManager.ProcessTimeAdvancement()
    ↓
Multiple System Effects:
  - AttentionManager refresh
  - Hunger increase
  - Deadline updates
  - NPC movement
    ↓
GameWorld State Updates
    ↓
MessageSystem notifications
```

### 4. Card Play Pipeline
```
ConversationContent.OnCardSelected(cardId)
    ↓
GameFacade.PlayCard(context, cardId)
    ↓
ConversationFacade.ProcessCardSelection()
    ↓
ConversationOrchestrator.ExecuteCard():
  - WeightPoolManager.SpendWeight()
  - CardEffectProcessor.ProcessEffect()
  - ComfortBatteryManager.ModifyComfort()
  - AtmosphereManager.SetAtmosphere()
    ↓
ConversationContext Update
    ↓
UI Refresh
```

---

## State Management

### GameWorld State Structure
```csharp
public class GameWorld
{
    // Core State
    public int CurrentDay { get; set; }
    public TimeBlocks CurrentTimeBlock { get; set; }
    public Player Player { get; private set; }
    public WorldState WorldState { get; private set; }
    
    // Collections
    public List<Location> Locations { get; set; }
    public List<NPC> NPCs { get; set; }
    
    // Card System (Unified)
    public Dictionary<string, ConversationCard> AllCardDefinitions { get; set; }
    public Dictionary<string, List<string>> NPCConversationDeckMappings { get; set; }
    public Dictionary<string, List<ConversationCard>> NPCGoalDecks { get; set; }
    public Dictionary<string, List<ConversationCard>> NPCExchangeDecks { get; set; }
    
    // Initialization Data (eliminates SharedData)
    public string InitialLocationId { get; set; }
    public PlayerInitialConfig InitialPlayerConfig { get; set; }
    
    // Skeleton Registry
    public Dictionary<string, string> SkeletonRegistry { get; set; }
}
```

### State Update Pattern
```csharp
// Managers read state, apply logic, write back
public class TokenMechanicsManager
{
    private readonly GameWorld _gameWorld;
    
    public void AddTokens(string npcId, TokenType type, int amount)
    {
        // Read from GameWorld
        NPC npc = _gameWorld.NPCs.FirstOrDefault(n => n.ID == npcId);
        
        // Apply business logic
        npc.Relationship.AddTokens(type, amount);
        
        // State is already updated in GameWorld
        // No separate storage needed
    }
}
```

### UI State Refresh Pattern
```csharp
// GameScreen handles all refresh
public async Task RefreshUI()
{
    await RefreshResourceDisplay();
    await RefreshTimeDisplay();
    await RefreshLocationDisplay();
    ContentVersion++; // Force re-render
    StateHasChanged();
}

// Child components call parent
await GameScreen.RefreshUI();
```

---

## Architectural Rules and Anti-Patterns

### ✅ ALWAYS DO

1. **GameWorld as Single Source of Truth**
   - ALL state lives in GameWorld
   - Managers read/write GameWorld ONLY
   - No state storage in services

2. **Static GameWorld Creation**
   ```csharp
   // CORRECT
   services.AddSingleton<GameWorld>(_ => 
       GameWorldInitializer.CreateGameWorld());
   ```

3. **Facade Pattern for Subsystems**
   - GameFacade delegates to subsystem facades
   - Facades delegate to managers
   - Pure orchestration, no business logic in facades

4. **Direct Parent-Child Communication**
   ```csharp
   // CORRECT
   [CascadingParameter] public GameScreenBase GameScreen { get; set; }
   await GameScreen.StartConversation(npcId, type);
   ```

5. **Context Objects for Complex Operations**
   ```csharp
   // CORRECT
   ConversationContext context = await GameFacade.CreateConversationContext(npcId, type);
   <ConversationContent Context="@context" />
   ```

6. **Unified Card System**
   - ALL cards are ConversationCard type
   - No LetterCard, ExchangeCard classes
   - Category property determines card type

7. **Case-Insensitive JSON Parsing**
   ```csharp
   // CORRECT - Inherit from BaseValidator
   public class NPCValidator : BaseValidator
   {
       // Use TryGetPropertyCaseInsensitive helper
   }
   ```

### ❌ NEVER DO

1. **NEVER Use GetRequiredService in GameWorld Creation**
   ```csharp
   // WRONG - Causes circular dependencies
   services.AddSingleton<GameWorld>(sp => {
       var timeManager = sp.GetRequiredService<TimeManager>();
       return new GameWorld(timeManager);
   });
   ```

2. **NEVER Store State in Services**
   ```csharp
   // WRONG
   public class LocationFacade
   {
       private Location _currentLocation; // NO!
   }
   ```

3. **NEVER Create SharedData Dictionaries**
   ```csharp
   // WRONG
   InitContext.SharedData["cards"] = cards;
   
   // CORRECT
   gameWorld.AllCardDefinitions = cards;
   ```

4. **NEVER Use Complex Event Chains**
   ```csharp
   // WRONG
   EventCallback<(string, object)> OnComplexEvent
   
   // CORRECT
   public async Task HandleSpecificAction(string param)
   ```

5. **NEVER Put Game Logic in UI**
   ```csharp
   // WRONG - UI calculating costs
   int attentionCost = conversation.Type == ConversationType.Deep ? 3 : 2;
   
   // CORRECT - Backend determines
   int attentionCost = GameFacade.GetAttentionCost(conversation.Type);
   ```

6. **NEVER Use Inheritance/Interfaces**
   ```csharp
   // WRONG
   public interface IManager { }
   public class LocationManager : BaseManager, IManager { }
   
   // CORRECT
   public class LocationManager { }
   ```

7. **NEVER Keep Deprecated Code**
   ```csharp
   // WRONG
   public void OldMethod() { } // Deprecated
   public void NewMethod() { }
   
   // CORRECT - Delete old, update all callers
   public void Method() { }
   ```

8. **NEVER Use Suffixes for Versions**
   ```csharp
   // WRONG
   ConversationManagerNew
   ConversationManagerV2
   
   // CORRECT - Replace completely
   ConversationManager
   ```

9. **NEVER Use Try-Catch for Flow Control**
   ```csharp
   // WRONG
   try { 
       var result = risky();
   } catch {
       return default;
   }
   
   // CORRECT - Let exceptions bubble
   var result = risky(); // Fail fast
   ```

10. **NEVER Hardcode Content**
    ```csharp
    // WRONG
    CardTemplates.CreatePromiseCard("Deliver this letter");
    
    // CORRECT
    Load from cards.json with id "letter_card_1"
    ```

---

## Testing and Development

### Testing Strategy
- **Primary**: Playwright E2E tests through actual UI
- **Location**: External test scripts (not in src)
- **Port**: Configured in `Properties/launchSettings.json`
- **Principle**: Test actual player experience, not backend

### Development Tools
1. **TestController** (`/src/Controllers/TestController.cs`)
   - Dev-only endpoints for testing
   - Disabled in production

2. **NPCDeckViewer** (`/src/Pages/Components/NPCDeckViewer.razor`)
   - Dev mode screen for viewing NPC decks
   - Accessible via ScreenMode.DeckViewer

3. **DevModeService** (`/src/Services/DevModeService.cs`)
   - Development mode utilities
   - Feature flags for testing

### Build and Run
```bash
# Build
cd /mnt/c/git/wayfarer/src
dotnet build

# Run with specific port
ASPNETCORE_URLS="http://localhost:5099" dotnet run

# Run tests (Playwright)
npx playwright test
```

### Common Development Tasks

#### Adding a New Subsystem
1. Create folder `/src/Subsystems/NewSystem/`
2. Create facade `NewSystemFacade.cs`
3. Create managers for business logic
4. Register in `ServiceConfiguration.cs` (correct order!)
5. Add facade injection to `GameFacade`
6. Delegate operations from GameFacade

#### Adding New Content
1. Create DTOs in `/src/Content/DTOs/`
2. Create parser in `/src/Content/`
3. Add to `PackageLoader` loading phases
4. Create validator in `/src/Content/Validation/Validators/`
5. Add to `GameWorld` collections

#### Adding UI Screen
1. Create content component in `/src/Pages/Components/`
2. Add ScreenMode enum value
3. Add case in `GameScreen.razor` switch
4. Implement parent method in `GameScreenBase`
5. Add navigation from existing screens

---

## Critical Integration Points

### GameFacade Method Categories
```csharp
// State Access
GetGameSnapshot(), GetPlayer(), GetCurrentLocation()

// Location Operations  
MoveToSpot(), GetNPCsAtCurrentSpot()

// Conversation Operations
CreateConversationContext(), ProcessCardSelection()

// Obligation Operations
GetLetterQueue(), CompleteDelivery()

// Resource Operations
GetPlayerResources(), SpendCoins()

// Time Operations
AdvanceTime(), GetTimeInfo()

// Travel Operations
GetTravelDestinations(), TravelToDestination()
```

### ViewModels Location
All ViewModels returned by GameFacade are in:
- `/src/ViewModels/GameFacadeViewModels.cs`
- `/src/ViewModels/GameViewModels.cs`
- `/src/ViewModels/LetterQueueViewModel.cs`
- `/src/ViewModels/TravelViewModel.cs`

### CSS Architecture
```
Loading Order:
1. common.css        - Global resets
2. game-base.css     - Core game styles
3. [screen].css      - Screen-specific styles

Key Classes:
.game-container      - Main container
.resources-bar       - Resource display
.conversation-screen - Conversation UI
.location-content   - Location UI
```

### JSON Content Structure
```json
{
  "metadata": { },
  "gameConfig": { },
  "cards": [ ],        // All ConversationCard definitions
  "locations": [ ],    // Location definitions
  "locationSpots": [ ],// Spot definitions  
  "npcs": [ ],        // NPC definitions with deckIds
  "routes": [ ],      // Travel routes
  "items": [ ],       // Item definitions
  "observations": [ ]  // Observation templates
}
```

---

## Performance Considerations

### Singleton Services
- All game services are Singleton for performance
- State persists across requests
- No per-request initialization overhead

### Lazy Content Loading
- Skeleton system prevents blocking on missing content
- Content loads asynchronously via packages
- Deterministic skeleton generation for consistency

### UI Optimization
- ContentVersion forces targeted re-renders
- @key attributes prevent unnecessary DOM updates
- StateHasChanged() called strategically

### Memory Management
- GameWorld is singleton (one instance)
- Collections are pre-allocated where possible
- Skeleton registry tracks placeholders for cleanup

---

## Summary

This architecture implements a clean, maintainable structure with:

1. **Clear Separation**: UI → GameFacade → Subsystem Facades → Managers → GameWorld
2. **Single Responsibility**: Each component has ONE clear purpose
3. **No Coupling**: Systems communicate through GameWorld, not each other
4. **Type Safety**: Strong types instead of dictionaries
5. **Fail Fast**: No defensive programming, clear errors
6. **Content Driven**: All game content from JSON, no hardcoding
7. **Testable**: E2E testing through actual UI

The key to maintaining this architecture is:
- ALWAYS go through GameFacade for UI operations
- NEVER store state outside GameWorld
- ALWAYS use static creation for GameWorld
- NEVER add complex inheritance hierarchies
- ALWAYS maintain the facade pattern for subsystems
- NEVER put game logic in UI components

This document should be the first thing read when starting any session to quickly understand the codebase structure and maintain architectural consistency.