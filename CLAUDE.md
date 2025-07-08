# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Development Commands

### Build and Run
```bash
# Build the application
dotnet build

# Run the application in development mode
dotnet run --project src/Wayfarer.csproj

# Run with specific launch profile
dotnet run --project src/Wayfarer.csproj --launch-profile Wayfarer
```

### Local Development URLs
- HTTPS: https://localhost:7232
- HTTP: http://localhost:5010

### Docker Development (AI Model)
```bash
# Start Ollama container with Gemma model
cd src/Build
docker-compose up -d

# Initialize the model (run once)
docker exec -it build_ollama_1 /app/scripts/init-model.sh
```

## Architecture Overview

### Core Game Vision
Wayfarer is a **resource management RPG** where every decision has cascading consequences. The game revolves around the design principle that **"everything costs something else"** - creating interconnected systems where optimizing one resource (coins, stamina, time) forces trade-offs in others.

### System Architecture

#### 1. **Game State Management** (`src/GameState/`)
- **GameWorld**: Central game state container
- **GameWorldManager**: Primary orchestrator coordinating all systems
- **Player**: Character stats, inventory, skills, progression
- **WorldState**: Current location, time, visited locations

#### 2. **Action System** (`src/Game/ActionSystem/`)
- **ActionDefinition**: Templates for location-based actions
- **ActionFactory**: Creates executable actions from templates
- **ActionProcessor**: Validates and executes actions with costs/effects

#### 3. **AI Narrative System** (`src/Game/AiNarrativeSystem/`)
- **AIGameMaster**: Orchestrates AI content generation
- **AIClient**: Handles Ollama/Gemma communication
- **EncounterChoiceResponseParser**: Converts AI responses to game choices
- **MemoryFileAccess**: Stores game memories for AI context

#### 4. **Encounter System** (`src/Game/EncounterSystem/`)
- **EncounterManager**: Manages encounter flow and choice processing
- **SkillCheckResolver**: Handles success/failure resolution
- **ChoiceProjectionService**: Previews potential choice outcomes

#### 5. **Main Game Systems** (`src/Game/MainSystem/`)
- **LocationSystem**: Manages locations and location spots
- **TravelManager**: Enhanced route options with cost-benefit analysis
- **TimeManager**: Tracks time progression and daily cycles (5 blocks/day constraint)
- **Inventory**: Limited inventory slots (3 items) with weight penalties
- **ContractSystem**: Timed delivery contracts with deadlines

#### 6. **Market & Trading System** (`src/GameState/`)
- **MarketManager**: Location-specific dynamic pricing with arbitrage opportunities
- **ArbitrageOpportunity**: Data structure for profitable trading routes
- **TradeManager**: Handles buy/sell operations with location-aware pricing

#### 7. **Route Selection Enhancement** (`src/GameState/`)
- **RouteComparisonData**: Comprehensive route analysis with efficiency scoring
- **RouteRecommendation**: Strategic route suggestions based on optimization strategy
- **OptimizationStrategy**: Multiple approaches (Efficiency, CheapestCost, LeastStamina, FastestTime)

### Core Resource Systems
- **Time**: Actions consume time blocks (dawn/morning/afternoon/evening/night)
- **Stamina**: Actions drain stamina; exhaustion limits available actions
- **Coins**: Required for transport, items, and some actions
- **Inventory**: 3-slot limit forces strategic item management

### Game Flow
```
Player selects action → ActionProcessor validates → 
EncounterManager creates AI encounter → AI generates narrative & choices → 
Player makes choice → SkillCheckResolver processes → Effects applied → 
World state updates → Available actions refresh
```

## Content System

### JSON Templates (`src/Content/Templates/`)
- **locations.json**: Location definitions
- **location_Spots.json**: Location spot definitions
- **actions.json**: Action templates
- **items.json**: Item definitions
- **contracts.json**: Contract templates
- **cards.json**: Skill card definitions

### Content Loading Process
1. `ContentLoader` loads JSON templates or save files
2. `GameWorldInitializer` sets up initial game state
3. Parsers convert JSON to game objects
4. `GameWorldManager` coordinates system initialization

## UI Structure (`src/Pages/`)
- **GameUI.razor**: Main UI coordinator managing screen transitions
- **MainGameplayView.razor**: Primary game interface
- **EncounterView.razor**: AI-generated encounter interactions
- **TravelSelection.razor**: Route selection interface
- **Market.razor**: Trading interface
- **Inventory.razor**: Inventory management
- **ContractUI.razor**: Contract management

## Key Development Patterns

### Dependency Injection
All systems use constructor injection for loose coupling and testability.

### Template-Based Content
Game content is defined in JSON templates loaded at runtime.

### State Machine Pattern
Game flow managed through clear states (location → action → encounter → resolution).

### AI Integration
- AI responses are parsed into structured game choices
- Context includes location, time, player history, and objectives
- Memory system maintains narrative continuity

## Important Implementation Notes

### Resource Management
The game's core tension comes from interconnected resource systems:
- **Route Selection**: Multiple transport options (walk free vs. pay for speed)
- **Market Arbitrage**: Location-specific item prices
- **Contract Pressure**: Time-limited deliveries
- **Inventory Constraints**: 3-slot limit forces immediate decisions

### AI System
- Uses Ollama with Gemma model for narrative generation
- Requires Docker container for local development
- AI responses must be parsed into valid game choices
- Context management is crucial for narrative coherence

### Performance Considerations
- Content is loaded once at startup
- Game state updates are batched
- UI polling is used for state changes
- Memory system files are cached for AI context

## Common Development Tasks

When implementing new features:
1. Define JSON templates for content
2. Create parser classes for new content types
3. Add system classes in appropriate Game/ subdirectory
4. Update GameWorldManager for system coordination
5. Create UI components in Pages/
6. Test with existing save system

When working with AI features:
1. Ensure Ollama container is running
2. Update prompt templates in Data/Prompts/
3. Test AI response parsing thoroughly
4. Consider memory/context implications

## Implementation Status

### ✅ **Completed Systems (Priority 1-3)**
- **Time Block Constraint System**: Full daily limit enforcement (5 blocks/day)
- **Dynamic Location-Based Pricing**: MarketManager with arbitrage opportunities
- **Enhanced Route Selection Interface**: Cost-benefit analysis with optimization strategies
- **Core Resource Management**: Time, stamina, coins, inventory weight penalties
- **AI Narrative System**: Ollama/Gemma integration with encounter generation

### 🔄 **Partially Implemented**
- **Contract System**: Basic structure exists, needs deadline/penalty enforcement
- **Market UI**: Basic trading interface, needs integration with dynamic pricing
- **Inventory Management**: 3-slot limit enforced, needs weight penalty UI updates

### ❌ **Missing/Needs Enhancement**
- **Advanced Market Analysis**: Profit calculator tools for trading decisions
- **Route Optimization Integration**: Combining travel with market opportunities
- **Advanced Contract UI**: Time pressure visualization and penalty warnings

## Integration Map: UI-Backend Connections

### **Frontend-Backend Data Flow Architecture**

#### **1. TravelSelection.razor ↔ TravelManager**
```csharp
// Frontend calls backend methods
var routeComparisons = TravelManager.GetRouteComparisonData(fromId, toId);
var recommendation = TravelManager.GetOptimalRouteRecommendation(fromId, toId, strategy);

// Data flows:
Frontend: RouteOption selection → Backend: RouteComparisonData calculation
Frontend: Strategy selection → Backend: RouteRecommendation generation
Frontend: Travel button → Backend: TravelManager.TravelToLocation()
```

#### **2. MainGameplayView.razor ↔ GameWorldManager**
```csharp
// State synchronization pattern
[Inject] private GameWorldManager GameManager { get; set; }
[Inject] private GameWorld GameWorld { get; set; }

// Weight calculations flow to UI
int totalWeight = GameManager.CalculateTotalWeight();
string weightStatus = UI calculated from weight values
```

#### **3. Market.razor ↔ MarketManager**
```csharp
// Trading operations (discovered in codebase)
MarketManager handles location-specific pricing
ArbitrageOpportunity provides profit calculations
Frontend displays buy/sell prices per location
```

#### **4. Dependency Injection Pattern**
All UI components use `@inject` for backend service access:
- `GameWorldManager`: Primary game state orchestrator
- `TravelManager`: Route and travel operations  
- `MarketManager`: Trading and pricing operations
- `ItemRepository`: Item data and operations
- `GameWorld`: Direct state access for display

### **State Management Architecture**

#### **UI State Updates**
- **Reactive Updates**: UI components inject services and call methods directly
- **Polling Pattern**: Some components use StateVersion tracking for updates
- **Event-Driven**: Button clicks → backend method calls → state changes → UI refresh

#### **Data Binding Patterns**
```csharp
// Direct property binding to GameWorld
@GameWorld.GetPlayer().Coins
@GameWorld.CurrentTimeBlock

// Service method calls for calculated values  
@GameManager.CalculateTotalWeight()
@TravelManager.GetRouteComparisonData()
```

## Session Progress

### **Recent Architectural Discoveries**
1. **Route Selection System**: Full TDD implementation with efficiency scoring algorithms
2. **Market System**: Dynamic pricing with arbitrage opportunity calculations
3. **UI Integration**: Direct service injection pattern with reactive updates
4. **Resource Management**: Weight penalties properly integrated across travel system

### **Key Integration Points Identified**
- UI components directly inject and call backend services
- State changes flow through GameWorld and GameWorldManager
- RouteComparisonData provides rich UI display data
- MarketManager enables location-aware pricing decisions

## Next Priorities

### **High Priority Integration Enhancements**
1. **Market UI Enhancement**: Integrate dynamic pricing display with arbitrage opportunities
2. **Contract System UI**: Add time pressure visualization and deadline warnings  
3. **Route-Market Integration**: Combine travel decisions with trading opportunities
4. **Advanced Inventory UI**: Show weight penalties and capacity management

### **Technical Debt & Improvements**
1. **Error Handling**: Add try-catch blocks for service calls in UI components
2. **Loading States**: Implement loading indicators for backend operations
3. **State Management**: Consider more efficient update patterns for complex calculations
4. **Testing**: Add integration tests for UI-backend data flow