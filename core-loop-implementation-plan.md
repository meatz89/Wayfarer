# Wayfarer Core Loop: Implementation Plan

**Design Source**: [refinement-core-gameplay-loop.md](./refinement-core-gameplay-loop.md)
**Architecture Principles**: [game-design-principles.md](./game-design-principles.md)
**Timeline**: 10 weeks to production-ready MVP
**Status**: ✅ Plan approved, 🚧 Phase 1 in progress

---

## Executive Summary

### Vision
Transform V2's abstract progression (Knowledge tokens, boolean gates) into transparent economic affordability (coins → equipment → context matching → survival) while preserving all existing architecture.

### Core Transformation
- **From**: "Complete Phase 1 to unlock Phase 2" (boolean gate)
- **To**: "Forest route costs 4 Health, buy waders for 12 coins to survive" (economic affordability)

### Architecture Impact
- **Entities extended**: 13 new fields across 7 entities
- **New classes**: 1 (RoutePath)
- **Entities removed**: 0 (extension only, no replacement)
- **Services extended**: 4 facades (Investigation, Obstacle, Equipment, Cube)
- **Legacy deleted**: Knowledge system, Unlock conditions, Global difficulty

---

## Data Flow Architecture

### Complete Pipeline: JSON → UI

```
Content (JSON)
    ↓ [Parsers validate & transform]
Domain Entities
    ↓ [GameWorld owns via flat lists]
Single Source of Truth
    ↓ [Facades orchestrate logic]
Service Layer
    ↓ [Blazor components render]
Frontend UI
```

### Three Reference Types (game-design-principles.md)

**OWNERSHIP** (Lifecycle Control):
- GameWorld owns Investigations (flat list)
- Investigation owns Obstacles (inline, spawned dynamically)
- Obstacle owns Goals (inline, multiple approaches)
- Goal owns GoalCards (inline, victory conditions)

**PLACEMENT** (Presentation Context):
- Goal.PlacementLocationId (where button appears, not who owns)
- Goal.PlacementNpcId (where button appears, not who owns)
- Equipment.LocationId (where sold, not who owns)

**REFERENCE** (Lookup Relationship):
- Location.ObstacleIds → references GameWorld.Obstacles
- NPC.ObstacleIds → references GameWorld.Obstacles
- RoutePath.OptionalObstacleId → references GameWorld.Obstacles

---

## Phase 1: Content Layer (JSON → Parsers) - 2 Weeks

**Status**: 🚧 IN PROGRESS

### Resource Value Scaling

**Health/Stamina/Focus**: 100 → 6 points
```json
// Before (01_foundation.json)
"maxHealth": 100,
"maxStamina": 100,
"maxFocus": 100

// After
"maxHealth": 6,
"maxStamina": 6,
"maxFocus": 6
```

**Obstacle Intensity**: 15-70 → 1-3
```json
// Before
"physicalDanger": 45,
"mentalComplexity": 30

// After
"physicalDanger": 2,
"mentalComplexity": 1
```

**Time Segments**: 23 → 16 (4 equal blocks)
```json
// Goal costs before
"costs": { "time": 4, "focus": 20 }

// After
"costs": { "time": 2, "focus": 1 }
```

### Context System Addition

**Obstacles with Contexts**:
```json
{
  "id": "cliff_ascent",
  "name": "Sheer Cliff Face",
  "contexts": ["Climbing", "Height"],
  "physicalDanger": 3,
  "goals": [...]
}
```

**Equipment with Applicable Contexts**:
```json
{
  "id": "rope",
  "name": "Rope",
  "buyPrice": 10,
  "applicableContexts": ["Climbing", "Height", "Securing"],
  "intensityReduction": 1
}
```

### Master Context List
```
Physical: Climbing, Water, Strength, Precision, Endurance, Cold, Height, Securing
Mental: Darkness, Mechanical, Spatial, Deduction, Search, Pattern, Memory
Social: Authority, Persuasion, Intimidation, Empathy, Negotiation, Etiquette
```

### Route Path Choices

**RouteSegment with AvailablePaths**:
```json
{
  "segmentNumber": 2,
  "availablePaths": [
    {
      "id": "shallow_ford",
      "description": "Wade through the creek at its shallowest point",
      "timeSegments": 1,
      "staminaCost": 2,
      "optionalObstacleId": "water_current_obstacle",
      "hiddenUntilExploration": 0
    },
    {
      "id": "rope_bridge",
      "description": "Cross via the old rope bridge upstream",
      "timeSegments": 2,
      "staminaCost": 1,
      "optionalObstacleId": "bridge_stability_obstacle",
      "hiddenUntilExploration": 0
    },
    {
      "id": "stepping_stones",
      "description": "Use the hidden stepping stones",
      "timeSegments": 1,
      "staminaCost": 1,
      "optionalObstacleId": null,
      "hiddenUntilExploration": 2
    }
  ]
}
```

### Investigation Types

**NPCCommissioned**:
```json
{
  "id": "elena_urgent_delivery",
  "name": "Urgent Letter Delivery",
  "obligationType": "NPCCommissioned",
  "patronNpcId": "elena",
  "deadlineSegment": 8,
  "completionRewardCoins": 25,
  "completionRewardStoryCubes": 2,
  "phases": [...]
}
```

**SelfDiscovered**:
```json
{
  "id": "mill_mystery",
  "name": "The Waterwheel Mystery",
  "obligationType": "SelfDiscovered",
  "deadlineSegment": null,
  "completionRewardItems": ["climbing_gear"],
  "spawnedObligationIds": ["forest_ruins_investigation"],
  "phases": [...]
}
```

### Coin Economy Scaling

```json
// NPC Obligations
"completionRewardCoins": 25,  // Was: 75 → Now: 25

// Equipment
"rope": { "buyPrice": 10 },         // Was: 30
"waders": { "buyPrice": 12 },       // Was: 35
"lantern": { "buyPrice": 15 },      // Was: 45
"climbing_gear": { "buyPrice": 30 } // Was: 90

// Work Goals (Safety Net)
"chop_wood": { "rewardCoins": 5 },      // Repeatable
"deliver_messages": { "rewardCoins": 8 } // Repeatable
```

### Files to Modify
- `Content/Core/01_foundation.json` - Resource pools
- `Content/Core/05_goals.json` - Goal costs, thresholds
- `Content/Core/07_equipment.json` - Add applicableContexts, scale prices
- `Content/Core/08_social_cards.json` - Scale card power
- `Content/Core/09_mental_cards.json` - Scale card power
- `Content/Core/10_physical_cards.json` - Scale card power
- `Content/Core/12_challenge_decks.json` - Scale thresholds
- `Content/Core/13_investigations.json` - Add obligationType, contexts to obstacles

### Legacy Removal - Content Layer
- ❌ Remove all `"knowledgeRequired"` fields
- ❌ Remove all `"unlockedBy"` fields
- ❌ Remove all `"prerequisites"` objects (replaced by GoalCard rewards)

---

## Phase 2: Parser Layer (JSON → Domain) - 1 Week

**Status**: ⏳ PENDING

### Parser Extensions

**InvestigationParser.cs**:
```csharp
// New fields to parse
- ObligationType (enum: NPCCommissioned | SelfDiscovered)
- PatronNpcId (string, nullable)
- DeadlineSegment (int?, nullable)
- CompletionRewardCoins (int)
- CompletionRewardItems (List<string>)
- SpawnedObligationIds (List<string>)
```

**ObstacleParser.cs**:
```csharp
// New field to parse
- Contexts (List<string>)

// Validation
- Validate contexts against master list
- Ensure at least 1 context per obstacle
```

**EquipmentParser.cs**:
```csharp
// New fields to parse
- ApplicableContexts (List<string>)
- IntensityReduction (int, default: 1)
- ExhaustAfterUses (int, default: 2)
- RepairCost (int)

// Validation
- Validate contexts against master list
```

**RouteParser.cs**:
```csharp
// Extend RouteSegment parsing
- AvailablePaths (List<RoutePathDTO>)

// New DTO: RoutePathDTO
{
  string Id
  string Description
  int TimeSegments
  int StaminaCost
  string OptionalObstacleId (nullable)
  int HiddenUntilExploration
}

// Validation
- At least 2 paths per segment
- OptionalObstacleId must exist in GameWorld.Obstacles
```

### Validation Rules

**Context Validation**:
- All equipment contexts must be in master list
- All obstacle contexts must be in master list
- Warn if obstacle has 0 contexts (should have at least 1)

**Obligation Validation**:
- If ObligationType == NPCCommissioned: PatronNpcId and DeadlineSegment REQUIRED
- If ObligationType == SelfDiscovered: DeadlineSegment must be null
- DeadlineSegment must be 8, 12, or 16 (valid checkpoints)

**Route Path Validation**:
- Each segment must have 2-3 paths
- At least 1 path must have hiddenUntilExploration == 0 (always visible)
- OptionalObstacleId references must resolve to actual obstacles

### Files to Modify
- `Content/Parsers/InvestigationParser.cs`
- `Content/Parsers/ObstacleParser.cs`
- `Content/Parsers/EquipmentParser.cs`
- `Content/Parsers/RouteParser.cs` (extend existing)
- `Content/DTOs/InvestigationDTO.cs`
- `Content/DTOs/ObstacleDTO.cs`
- `Content/DTOs/EquipmentDTO.cs`
- `Content/DTOs/RoutePathDTO.cs` (NEW)

### Legacy Removal - Parser Layer
- ❌ Delete `KnowledgeParser.cs`
- ❌ Delete `UnlockConditionParser.cs`
- ❌ Remove `ParseKnowledgeRequirements()` methods
- ❌ Remove `ParseUnlockConditions()` methods

---

## Phase 3: Domain Entity Layer - 1 Week

**Status**: ⏳ PENDING

### Entity Extensions

**Investigation.cs** (existing, extend):
```csharp
public class Investigation
{
    // EXISTING fields preserved...
    public string Id { get; set; }
    public string Name { get; set; }
    public List<InvestigationPhaseDefinition> PhaseDefinitions { get; set; }

    // NEW fields
    public ObligationType ObligationType { get; set; }
    public string PatronNpcId { get; set; }  // nullable
    public int? DeadlineSegment { get; set; }  // nullable
    public int CompletionRewardCoins { get; set; }
    public List<string> CompletionRewardItems { get; set; } = new();
    public List<string> SpawnedObligationIds { get; set; } = new();
}

public enum ObligationType
{
    NPCCommissioned,  // From Social goals, has deadline
    SelfDiscovered    // From Mental/Social discoveries, no deadline
}
```

**Obstacle.cs** (existing, extend):
```csharp
public class Obstacle
{
    // EXISTING fields preserved...
    public int PhysicalDanger { get; set; }
    public int MentalComplexity { get; set; }
    public int SocialDifficulty { get; set; }

    // NEW field
    public List<string> Contexts { get; set; } = new();
}
```

**Equipment.cs** (existing, extend):
```csharp
public class Equipment : Item
{
    // EXISTING fields preserved...
    public List<string> EnabledActions { get; set; } = new();

    // NEW fields
    public List<string> ApplicableContexts { get; set; } = new();
    public int IntensityReduction { get; set; } = 1;
    public EquipmentState CurrentState { get; set; } = EquipmentState.Active;
    public int ExhaustAfterUses { get; set; } = 2;
    public int UsesRemaining { get; set; }
    public int RepairCost { get; set; }
}

public enum EquipmentState
{
    Active,     // Ready to use
    Exhausted   // Needs repair
}
```

**Location.cs** (existing, extend):
```csharp
public class Location
{
    // EXISTING fields preserved...

    // NEW field (moved from Player to Location-specific tracking)
    public int InvestigationCubes { get; set; } = 0;  // 0-10 scale
}
```

**NPC.cs** (existing, extend):
```csharp
public class NPC
{
    // EXISTING fields preserved...

    // NEW field (moved from Player to NPC-specific tracking)
    public int StoryCubes { get; set; } = 0;  // 0-10 scale
}
```

**Route.cs** (existing, extend):
```csharp
public class Route
{
    // EXISTING fields preserved...

    // NEW field
    public int ExplorationCubes { get; set; } = 0;  // 0-10 scale
}
```

**RoutePath.cs** (NEW class):
```csharp
public class RoutePath
{
    public string Id { get; set; }
    public string Description { get; set; }
    public int TimeSegments { get; set; }
    public int StaminaCost { get; set; }
    public string OptionalObstacleId { get; set; }  // nullable, references Obstacle.Id
    public int HiddenUntilExploration { get; set; } = 0;  // Exploration cube threshold
}
```

**RouteSegment.cs** (existing, extend):
```csharp
public class RouteSegment
{
    // EXISTING fields preserved...
    public int SegmentNumber { get; set; }
    public SegmentType Type { get; set; }
    public string PathCollectionId { get; set; }
    public string EventCollectionId { get; set; }

    // NEW field
    public List<RoutePath> AvailablePaths { get; set; } = new();
}
```

### Ownership Relationships (Principle 3)

```
OWNERSHIP (GameWorld flat lists):
GameWorld.Investigations (List<Investigation>)
    └─ Investigation.PhaseDefinitions[].ObstaclesSpawned (inline obstacles)
        └─ Obstacle.Goals (inline goals)
            └─ Goal.GoalCards (inline victory conditions)

PLACEMENT (metadata for UI):
Goal.PlacementLocationId → references Location for button display
Goal.PlacementNpcId → references NPC for button display
Equipment.LocationId → references Location for vendor display

REFERENCE (lookup only):
Location.ObstacleIds → references GameWorld.Obstacles (doesn't own)
NPC.ObstacleIds → references GameWorld.Obstacles (doesn't own)
RoutePath.OptionalObstacleId → references GameWorld.Obstacles (doesn't own)
```

### Files to Modify
- `GameState/Investigation.cs`
- `GameState/Obstacle.cs`
- `GameState/Equipment.cs`
- `GameState/Location.cs`
- `GameState/NPC.cs`
- `GameState/Route.cs`
- `GameState/RouteSegment.cs`
- `GameState/RoutePath.cs` (NEW)

### Legacy Removal - Entity Layer
- ❌ Delete `GameState/Knowledge.cs`
- ❌ Delete `GameState/UnlockCondition.cs`
- ❌ Delete `GameState/GlobalDifficulty.cs`

---

## Phase 4: GameWorld Layer - 1 Week

**Status**: ⏳ PENDING

### GameWorld Structure

**Player State Extensions**:
```csharp
public class Player
{
    // EXISTING resources (scaled values)
    public int Health { get; set; } = 6;
    public int Stamina { get; set; } = 6;
    public int Focus { get; set; } = 6;
    public int Coins { get; set; }

    // EXISTING progression
    public int Understanding { get; set; } = 0;  // Global Mental expertise (0-10)

    // NEW: Active obligations tracking
    public List<string> ActiveObligationIds { get; set; } = new();

    // NEW: Context-specific mastery (replace global tracking)
    // Moved to entities themselves for proper ownership
    // Player just tracks which entities they've interacted with

    // EXISTING: Equipment ownership
    public List<string> OwnedEquipmentIds { get; set; } = new();
}
```

**GameWorld Methods**:
```csharp
// NEW: Obligation lifecycle
public List<Investigation> GetActiveObligations(Player player)
public void ActivateObligation(string investigationId, Player player)
public void CompleteObligation(string investigationId, Player player)
public List<Investigation> CheckDeadlineFailures(int currentSegment, Player player)
public void ApplyDeadlineConsequences(Investigation failedObligation, Player player)

// NEW: Equipment management
public void PurchaseEquipment(string equipmentId, Player player)
public void ExhaustEquipment(string equipmentId, Player player)
public void RepairEquipment(string equipmentId, Player player)
public void SellEquipment(string equipmentId, Player player)

// NEW: Context cube tracking
public int GetLocationCubes(string locationId)
public int GetNPCCubes(string npcId)
public int GetRouteCubes(string routeId)
public void GrantCubes(string entityId, CubeType type, int amount)
public void RemoveCubes(string npcId, int amount)
```

### Cube Storage Architecture

**Decision: Store cubes ON entities, not ON player**

**Rationale (Principle 1: Ownership)**:
- Location owns its InvestigationCubes (location expertise)
- NPC owns its StoryCubes (relationship depth)
- Route owns its ExplorationCubes (route familiarity)
- Player references entities but doesn't own their progression state

**Implementation**:
```csharp
// Cubes stored on entities themselves
Location.InvestigationCubes (0-10)
NPC.StoryCubes (0-10)
Route.ExplorationCubes (0-10)

// Player just tracks which entities they've interacted with
Player.VisitedLocationIds (List<string>)
Player.MetNPCIds (List<string>)
Player.TraveledRouteIds (List<string>)
```

### Files to Modify
- `GameState/GameWorld.cs`
- `GameState/Player.cs`

### Legacy Removal - GameWorld Layer
- ❌ Remove `Player.Knowledge` (List<string>) - replaced by Understanding counter
- ❌ Remove `GameWorld.UnlockConditions` - unlock system eliminated
- ❌ Remove `Player.GlobalFamiliarity` - replaced by entity-specific cubes

---

## Phase 5: Facade Layer - 2 Weeks

**Status**: ⏳ PENDING

### InvestigationFacade.cs (extend existing)

```csharp
// NEW methods
public List<Investigation> GetAvailableObligations(string playerId)
{
    // Return investigations visible to player
    // Filter by trigger prerequisites
}

public void ActivateObligation(string investigationId, string playerId)
{
    // Add to Player.ActiveObligationIds
    // Spawn Phase 1 obstacles
}

public void CompletePhase(string phaseId, string investigationId, string playerId)
{
    // Spawn next phase obstacles
    // Apply phase rewards (coins, items, cubes)
    // Check if investigation complete
}

public List<Investigation> CheckDeadlines(int currentSegment, string playerId)
{
    // Query active NPCCommissioned obligations
    // Return those where DeadlineSegment <= currentSegment && !IsComplete
}

public void ApplyDeadlineConsequences(Investigation failedObligation, string playerId)
{
    // Remove StoryCubes from patron NPC
    // Remove from Player.ActiveObligationIds
}
```

### ObstacleFacade.cs (NEW service)

```csharp
public class ObstacleFacade
{
    // Query obstacles by placement
    public List<Obstacle> GetObstaclesAtLocation(string locationId)
    {
        // Query GameWorld.Obstacles where Location.ObstacleIds contains obstacleId
        // REFERENCE relationship, not ownership
    }

    public List<Obstacle> GetObstaclesForNPC(string npcId)
    {
        // Query GameWorld.Obstacles where NPC.ObstacleIds contains obstacleId
    }

    // Equipment context matching
    public List<Equipment> GetApplicableEquipment(string obstacleId, List<string> playerEquipmentIds)
    {
        // Get obstacle contexts
        // Get player equipment
        // Match equipment.ApplicableContexts ∩ obstacle.Contexts
        // Return equipment with ANY matching context
    }

    public int CalculateEffectiveIntensity(string obstacleId, List<Equipment> applicableEquipment)
    {
        // Get obstacle base intensity (PhysicalDanger/MentalComplexity/SocialDifficulty)
        // Sum equipment.IntensityReduction for all applicable equipment
        // Return Math.Max(0, baseIntensity - totalReduction)
    }

    // Resolution paths
    public ObstacleResolutionOptions GetResolutionOptions(string obstacleId, string playerId)
    {
        return new ObstacleResolutionOptions
        {
            Challenge = GetChallengeOption(obstacleId),
            Equipment = GetEquipmentOptions(obstacleId, playerId),
            Coins = GetCoinBypassOption(obstacleId),
            AcceptDamage = GetDamageAcceptanceOption(obstacleId)
        };
    }
}
```

### EquipmentFacade.cs (NEW service)

```csharp
public class EquipmentFacade
{
    // Vendor operations
    public List<Equipment> GetVendorEquipment(string npcId)
    {
        // Query GameWorld.Equipment where LocationId == npc.Venue
    }

    // Transaction
    public bool PurchaseEquipment(string equipmentId, string playerId)
    {
        // Check player coins >= equipment.BuyPrice
        // Deduct coins
        // Add to Player.OwnedEquipmentIds
        // Initialize equipment.UsesRemaining = equipment.ExhaustAfterUses
    }

    // Application
    public void ApplyEquipment(string equipmentId, string obstacleId, string playerId)
    {
        // Verify equipment.ApplicableContexts ∩ obstacle.Contexts
        // Reduce obstacle effective intensity
        // Decrement equipment.UsesRemaining
        // If UsesRemaining == 0: Set CurrentState = Exhausted
    }

    // Maintenance
    public bool RepairEquipment(string equipmentId, string playerId)
    {
        // Check player coins >= equipment.RepairCost
        // Deduct coins
        // Set equipment.CurrentState = Active
        // Reset equipment.UsesRemaining = equipment.ExhaustAfterUses
    }

    public void SellEquipment(string equipmentId, string playerId)
    {
        // Grant player 50% of equipment.BuyPrice
        // Remove from Player.OwnedEquipmentIds
    }
}
```

### CubeFacade.cs (NEW service)

```csharp
public class CubeFacade
{
    // Query cubes
    public int GetLocationCubes(string locationId)
    {
        // Return Location.InvestigationCubes
    }

    public int GetNPCCubes(string npcId)
    {
        // Return NPC.StoryCubes
    }

    public int GetRouteCubes(string routeId)
    {
        // Return Route.ExplorationCubes
    }

    // Grant cubes (from goal completion)
    public void GrantLocationCubes(string locationId, int amount)
    {
        // Location.InvestigationCubes = Math.Min(10, current + amount)
    }

    public void GrantNPCCubes(string npcId, int amount)
    {
        // NPC.StoryCubes = Math.Min(10, current + amount)
    }

    public void GrantRouteCubes(string routeId, int amount)
    {
        // Route.ExplorationCubes = Math.Min(10, current + amount)
    }

    // Remove cubes (failed obligation consequence)
    public void RemoveNPCCubes(string npcId, int amount)
    {
        // NPC.StoryCubes = Math.Max(0, current - amount)
    }
}
```

### RouteFacade.cs (extend existing)

```csharp
// NEW methods
public List<RoutePath> GetAvailablePaths(string routeId, int segmentNumber, string playerId)
{
    // Get RouteSegment.AvailablePaths
    // Filter by HiddenUntilExploration <= Route.ExplorationCubes
    // Return visible paths only
}

public RoutePathCosts CalculatePathCosts(string routeId, int segmentNumber, string pathId, string playerId)
{
    // Get RoutePath
    // Calculate total: TimeSegments + StaminaCost + Obstacle effective intensity
    // Apply equipment context matching to obstacle (if exists)
    // Return comprehensive cost breakdown
}

public void CompletePathSegment(string routeId, int segmentNumber, string pathId, string playerId)
{
    // Deduct time segments from day
    // Deduct stamina from player
    // If obstacle exists: Present resolution options
}

public void CompleteRoute(string routeId, string playerId)
{
    // Grant Route.ExplorationCubes += 1 (max 10)
}
```

### Files to Create/Modify
- `Services/InvestigationFacade.cs` (extend existing)
- `Subsystems/Obligation/ObstacleFacade.cs` (NEW)
- `Subsystems/Obligation/EquipmentFacade.cs` (NEW)
- `Subsystems/Obligation/CubeFacade.cs` (NEW)
- `Subsystems/Travel/RouteFacade.cs` (extend existing)

### Legacy Removal - Facade Layer
- ❌ Delete `Services/KnowledgeFacade.cs`
- ❌ Delete `Services/UnlockService.cs`
- ❌ Delete `Services/GlobalProgressionService.cs`

---

## Phase 6: Frontend Layer (UI Components) - 2 Weeks

**Status**: ⏳ PENDING

### Component Structure

**Obligation Management**:
- `Pages/Components/ObligationList.razor` (NEW)
- `Pages/Components/ObligationCard.razor` (NEW)
- `Pages/Components/ObligationDeadlineTracker.razor` (NEW)

**Obstacle Resolution**:
- `Pages/Components/ObstacleDetail.razor` (NEW)
- `Pages/Components/ObstacleResolutionPaths.razor` (NEW)
- `Pages/Components/EquipmentApplicability.razor` (NEW)

**Equipment Management**:
- `Pages/Components/EquipmentInventory.razor` (NEW)
- `Pages/Components/EquipmentCard.razor` (NEW)
- `Pages/Components/VendorEquipment.razor` (NEW)

**Route Travel**:
- `Pages/Components/RouteSegmentChoice.razor` (NEW)
- `Pages/Components/PathChoiceCard.razor` (NEW)

**Resource Display**:
- `Pages/Components/ResourceBar6Point.razor` (NEW)
- `Pages/Components/CubeDisplay.razor` (NEW)

**Session Evaluation**:
- `Pages/Components/DayEndSummary.razor` (NEW)

### Component Details

**ObligationList.razor**:
```razor
<div class="obligations-container">
  @foreach (var obligation in ActiveObligations)
  {
    <ObligationCard Obligation="@obligation" />
  }
</div>

// Display logic
- NPCCommissioned: Red border, deadline countdown, coin icon
- SelfDiscovered: Blue border, "No deadline", item icons
```

**ObstacleDetail.razor**:
```razor
<div class="obstacle-detail">
  <h3>@Obstacle.Name</h3>
  <div class="contexts">
    @foreach (var context in Obstacle.Contexts)
    {
      <span class="context-tag context-@context.ToLower()">@context</span>
    }
  </div>

  <div class="resolution-paths">
    <h4>Resolution Options:</h4>
    <ObstacleResolutionPaths
      ObstacleId="@Obstacle.Id"
      PlayerEquipment="@PlayerEquipment" />
  </div>
</div>
```

**EquipmentCard.razor**:
```razor
<div class="equipment-card @GetStateClass()">
  <h4>@Equipment.Name</h4>
  <div class="contexts">
    @foreach (var context in Equipment.ApplicableContexts)
    {
      <span class="context-tag context-@context.ToLower()">@context</span>
    }
  </div>
  <div class="stats">
    <span>Reduces intensity: -@Equipment.IntensityReduction</span>
    <span>Uses: @Equipment.UsesRemaining / @Equipment.ExhaustAfterUses</span>
  </div>
  @if (Equipment.CurrentState == EquipmentState.Exhausted)
  {
    <button @onclick="RepairEquipment">Repair (@Equipment.RepairCost coins)</button>
  }
</div>
```

**ResourceBar6Point.razor**:
```razor
<div class="resource-bar-6point">
  @for (int i = 1; i <= 6; i++)
  {
    <div class="resource-segment @GetSegmentClass(i)"></div>
  }
  <span class="resource-label">@CurrentValue / @MaxValue @ResourceName</span>
</div>

// CSS classes
- resource-segment.filled (green)
- resource-segment.empty (dark gray)
- resource-segment.critical (red, when < 2)
```

**CubeDisplay.razor**:
```razor
<div class="cube-display">
  @for (int i = 1; i <= 10; i++)
  {
    <div class="cube @(i <= CurrentCubes ? "filled" : "empty")"></div>
  }
  <span class="cube-label">@CubeType: @CurrentCubes / 10</span>
  <span class="cube-effect">Effect: -@CurrentCubes difficulty</span>
</div>
```

**RouteSegmentChoice.razor**:
```razor
<div class="segment-choice">
  <h3>Segment @SegmentNumber: @SegmentDescription</h3>
  <div class="available-paths">
    @foreach (var path in VisiblePaths)
    {
      <PathChoiceCard
        Path="@path"
        OnSelect="@(() => SelectPath(path.Id))" />
    }
  </div>
</div>
```

**PathChoiceCard.razor**:
```razor
<div class="path-choice-card @GetOptimalClass()" @onclick="OnSelect">
  <h4>@Path.Description</h4>
  <div class="path-costs">
    <span>⏱️ @Path.TimeSegments segments</span>
    <span>💪 @Path.StaminaCost stamina</span>
    @if (!string.IsNullOrEmpty(Path.OptionalObstacleId))
    {
      <span>⚠️ Obstacle: @GetObstacleName()</span>
      @if (EquipmentReducesObstacle())
      {
        <span class="equipment-help">✓ Equipment applies</span>
      }
    }
  </div>
</div>
```

**DayEndSummary.razor**:
```razor
<div class="day-end-summary modal">
  <h2>Day Complete</h2>

  <section class="obligations-summary">
    <h3>Obligations</h3>
    @foreach (var completed in CompletedObligations)
    {
      <div class="obligation-complete">
        ✓ @completed.Name (+@completed.RewardCoins coins, +@completed.RewardCubes cubes)
      </div>
    }
    @foreach (var failed in FailedObligations)
    {
      <div class="obligation-failed">
        ✗ @failed.Name (Lost @failed.CubesRemoved cubes with @failed.PatronName)
      </div>
    }
  </section>

  <section class="economy-summary">
    <h3>Economy</h3>
    <p>Coins earned: +@CoinsEarned</p>
    <p>Coins spent: -@CoinsSpent</p>
    <p>Net: @(CoinsEarned - CoinsSpent)</p>
  </section>

  <section class="progression-summary">
    <h3>Progression</h3>
    <p>New obligations discovered: @NewObligations</p>
    <p>Equipment acquired: @NewEquipment</p>
    <p>Stats increased: @StatsIncreased</p>
    <p>Cubes gained: @CubesGained</p>
  </section>

  <section class="resources-summary">
    <h3>Resources</h3>
    <ResourceBar6Point ResourceName="Health" CurrentValue="@Health" MaxValue="6" />
    <ResourceBar6Point ResourceName="Focus" CurrentValue="@Focus" MaxValue="6" />
    <ResourceBar6Point ResourceName="Stamina" CurrentValue="@Stamina" MaxValue="6" />
  </section>

  <button @onclick="ContinueToNextDay">Continue to Next Day</button>
</div>
```

### Files to Create
- `Pages/Components/ObligationList.razor`
- `Pages/Components/ObligationCard.razor`
- `Pages/Components/ObligationDeadlineTracker.razor`
- `Pages/Components/ObstacleDetail.razor`
- `Pages/Components/ObstacleResolutionPaths.razor`
- `Pages/Components/EquipmentApplicability.razor`
- `Pages/Components/EquipmentInventory.razor`
- `Pages/Components/EquipmentCard.razor`
- `Pages/Components/VendorEquipment.razor`
- `Pages/Components/RouteSegmentChoice.razor`
- `Pages/Components/PathChoiceCard.razor`
- `Pages/Components/ResourceBar6Point.razor`
- `Pages/Components/CubeDisplay.razor`
- `Pages/Components/DayEndSummary.razor`

### Legacy Removal - UI Layer
- ❌ Delete `Pages/Components/KnowledgeDisplay.razor`
- ❌ Delete `Pages/Components/UnlockNotification.razor`
- ❌ Delete `Pages/Components/AbstractDifficultySlider.razor`

---

## Phase 7: CSS Layer (Visual Styling) - 1 Week

**Status**: ⏳ PENDING

### Stylesheet Organization

**New CSS Files**:
- `wwwroot/css/resources-6point.css` (NEW)
- `wwwroot/css/context-cubes.css` (NEW)
- `wwwroot/css/context-tags.css` (NEW)
- `wwwroot/css/obligations.css` (NEW)
- `wwwroot/css/route-paths.css` (NEW)

### Resource Bar Styling (resources-6point.css)

```css
/* 6-point resource bar */
.resource-bar-6point {
  display: grid;
  grid-template-columns: repeat(6, 1fr);
  gap: 4px;
  margin-bottom: 8px;
}

.resource-segment {
  height: 30px;
  width: 40px;
  border: 2px solid #333;
  border-radius: 4px;
  transition: all 0.3s ease;
}

.resource-segment.filled {
  background: linear-gradient(135deg, #4CAF50, #66BB6A);
  box-shadow: 0 2px 4px rgba(76, 175, 80, 0.4);
}

.resource-segment.empty {
  background: #222;
  border-color: #444;
}

.resource-segment.critical {
  background: linear-gradient(135deg, #f44336, #ef5350);
  box-shadow: 0 2px 8px rgba(244, 67, 54, 0.6);
  animation: pulse-critical 1s infinite;
}

@keyframes pulse-critical {
  0%, 100% { opacity: 1; }
  50% { opacity: 0.7; }
}

.resource-label {
  margin-top: 4px;
  font-size: 0.9em;
  color: #ccc;
  font-weight: 500;
}
```

### Context Cube Styling (context-cubes.css)

```css
/* Cube display grid */
.cube-display {
  display: grid;
  grid-template-columns: repeat(10, 1fr);
  gap: 3px;
  margin-bottom: 12px;
}

.cube {
  width: 24px;
  height: 24px;
  border-radius: 3px;
  transition: all 0.3s ease;
}

.cube.filled {
  background: linear-gradient(135deg, #2196F3, #42A5F5);
  box-shadow: 0 2px 6px rgba(33, 150, 243, 0.5);
  border: 1px solid #1976D2;
}

.cube.empty {
  background: #2a2a2a;
  border: 1px solid #555;
}

.cube-label {
  display: block;
  margin-top: 6px;
  font-size: 0.9em;
  font-weight: 600;
  color: #fff;
}

.cube-effect {
  display: block;
  font-size: 0.85em;
  color: #4CAF50;
  font-style: italic;
}
```

### Context Tag Styling (context-tags.css)

```css
/* Context tags */
.context-tag {
  display: inline-block;
  padding: 4px 10px;
  margin: 2px 4px;
  border-radius: 12px;
  font-size: 0.85em;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
  transition: all 0.2s ease;
}

.context-tag:hover {
  transform: scale(1.05);
  box-shadow: 0 2px 6px rgba(0,0,0,0.3);
}

/* Physical contexts */
.context-climbing { background: #8B4513; color: #fff; }
.context-water { background: #1E90FF; color: #fff; }
.context-strength { background: #C41E3A; color: #fff; }
.context-precision { background: #FFD700; color: #333; }
.context-endurance { background: #228B22; color: #fff; }
.context-cold { background: #4682B4; color: #fff; }
.context-height { background: #696969; color: #fff; }
.context-securing { background: #D2691E; color: #fff; }

/* Mental contexts */
.context-darkness { background: #2C2C2C; color: #fff; }
.context-mechanical { background: #708090; color: #fff; }
.context-spatial { background: #9370DB; color: #fff; }
.context-deduction { background: #FF8C00; color: #fff; }
.context-search { background: #DAA520; color: #333; }
.context-pattern { background: #20B2AA; color: #fff; }
.context-memory { background: #BA55D3; color: #fff; }

/* Social contexts */
.context-authority { background: #800020; color: #fff; }
.context-persuasion { background: #9C27B0; color: #fff; }
.context-intimidation { background: #8B0000; color: #fff; }
.context-empathy { background: #FF69B4; color: #fff; }
.context-negotiation { background: #4169E1; color: #fff; }
.context-etiquette { background: #DDA0DD; color: #333; }
```

### Obligation Styling (obligations.css)

```css
/* Obligation cards */
.obligation-card {
  border: 2px solid #444;
  border-radius: 8px;
  padding: 16px;
  margin-bottom: 12px;
  background: #1a1a1a;
  transition: all 0.3s ease;
}

.obligation-card:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(0,0,0,0.3);
}

/* NPCCommissioned styling */
.obligation-card.npc-commissioned {
  border-left: 4px solid #f44336;
  background: linear-gradient(90deg, rgba(244,67,54,0.1) 0%, #1a1a1a 10%);
}

/* SelfDiscovered styling */
.obligation-card.self-discovered {
  border-left: 4px solid #2196F3;
  background: linear-gradient(90deg, rgba(33,150,243,0.1) 0%, #1a1a1a 10%);
}

/* Deadline urgency */
.deadline-tracker {
  display: flex;
  align-items: center;
  margin-top: 8px;
  padding: 8px;
  border-radius: 4px;
  font-weight: 600;
}

.deadline-tracker.urgent {
  background: rgba(244, 67, 54, 0.2);
  border: 1px solid #f44336;
  color: #f44336;
}

.deadline-tracker.standard {
  background: rgba(255, 152, 0, 0.2);
  border: 1px solid #FF9800;
  color: #FF9800;
}

.deadline-tracker.relaxed {
  background: rgba(76, 175, 80, 0.2);
  border: 1px solid #4CAF50;
  color: #4CAF50;
}

.deadline-countdown {
  margin-left: auto;
  font-size: 1.2em;
  font-weight: 700;
}
```

### Route Path Styling (route-paths.css)

```css
/* Path choice cards */
.path-choice-card {
  border: 2px solid #444;
  border-radius: 8px;
  padding: 16px;
  margin: 12px 0;
  background: #1a1a1a;
  cursor: pointer;
  transition: all 0.3s ease;
}

.path-choice-card:hover {
  border-color: #2196F3;
  background: linear-gradient(135deg, rgba(33,150,243,0.1), #1a1a1a);
  transform: translateX(4px);
  box-shadow: -4px 0 12px rgba(33,150,243,0.3);
}

.path-choice-card.selected {
  border-color: #4CAF50;
  background: linear-gradient(135deg, rgba(76,175,80,0.15), #1a1a1a);
  box-shadow: 0 0 16px rgba(76,175,80,0.4);
}

.path-choice-card.optimal {
  border-color: #FFD700;
  position: relative;
}

.path-choice-card.optimal::before {
  content: "⭐ OPTIMAL";
  position: absolute;
  top: -12px;
  right: 8px;
  background: #FFD700;
  color: #333;
  padding: 2px 8px;
  border-radius: 12px;
  font-size: 0.75em;
  font-weight: 700;
}

.path-costs {
  display: flex;
  gap: 16px;
  margin-top: 12px;
  flex-wrap: wrap;
}

.path-costs span {
  padding: 4px 8px;
  background: #2a2a2a;
  border-radius: 4px;
  font-size: 0.9em;
}

.equipment-help {
  color: #4CAF50;
  font-weight: 600;
}
```

### Equipment Styling

```css
/* Equipment cards */
.equipment-card {
  border: 2px solid #444;
  border-radius: 8px;
  padding: 12px;
  margin: 8px;
  background: #1a1a1a;
  transition: all 0.3s ease;
}

.equipment-card.active {
  border-color: #4CAF50;
  background: linear-gradient(135deg, rgba(76,175,80,0.1), #1a1a1a);
}

.equipment-card.exhausted {
  border-color: #f44336;
  background: linear-gradient(135deg, rgba(244,67,54,0.1), #1a1a1a);
  opacity: 0.7;
}

.equipment-card.exhausted::after {
  content: "EXHAUSTED";
  display: block;
  color: #f44336;
  font-weight: 700;
  text-align: center;
  margin-top: 8px;
}
```

### Files to Create
- `wwwroot/css/resources-6point.css`
- `wwwroot/css/context-cubes.css`
- `wwwroot/css/context-tags.css`
- `wwwroot/css/obligations.css`
- `wwwroot/css/route-paths.css`
- `wwwroot/css/equipment.css`

### Legacy Removal - CSS Layer
- ❌ Remove `.knowledge-token` styles
- ❌ Remove `.unlock-notification` styles
- ❌ Remove `.abstract-difficulty` styles

---

## Phase 8: Integration Testing - 1 Week

**Status**: ⏳ PENDING

### Test Scenarios

**Scenario 1: Equipment Context Matching**
1. JSON: Rope has `["Climbing", "Height"]`, Cliff has `["Climbing", "Height"]`
2. Parser: Validates contexts, creates entities
3. GameWorld: Stores rope in Equipment list, cliff in Obstacles
4. Facade: EvaluateEquipmentApplicability(cliff, [rope]) → returns rope
5. UI: Shows "✓ Rope applies: Intensity 3 → 2"

**Expected**: Rope reduces cliff, doesn't reduce creek (Water context)

---

**Scenario 2: Localized Cube Mastery**
1. Complete Mental goal at Mill 5 times
2. Mill.InvestigationCubes increases: 0 → 1 → 2 → 3 → 4 → 5
3. Next Mill Mental challenge: Exposure 8 - 5 cubes = 3 (easier)
4. First Forest Mental challenge: Exposure 8 - 0 cubes = 8 (still hard)

**Expected**: Mill mastery doesn't transfer to Forest

---

**Scenario 3: Obligation Deadline Pressure**
1. Accept Elena obligation (deadline segment 8, reward 25 coins)
2. Accept Martha obligation (deadline segment 12, reward 40 coins)
3. Current segment: 3, remaining: 13 segments
4. Elena route: 3 segments, Elena goals: 2 segments = 5 segments total
5. Martha route: 4 segments, Martha goals: 3 segments = 7 segments total
6. Total needed: 12 segments, available: 13 segments
7. Player chooses: Complete both (rush) OR complete Elena + investigate Mill (one failure)

**Expected**: Impossible choice forces prioritization

---

**Scenario 4: Economic Affordability**
1. Player: 0 coins, no equipment
2. Forest route: PhysicalDanger 2 obstacles = 4 Health cost
3. Player Health: 6, can barely afford
4. Work goals 2 sessions: 40 coins earned
5. Buy waders (12 coins): 28 coins remaining
6. Forest route: Water obstacles 2 → 1 (waders reduce) = 2 Health cost

**Expected**: Economic investment enables safer progress

---

**Scenario 5: Route Path Choices**
1. Creek route segment 2: 3 paths available
2. Path A (ford): 1 segment, 2 stamina, Water obstacle 2
3. Path B (bridge): 2 segments, 1 stamina, Height obstacle 2
4. Path C (stones): 1 segment, 1 stamina, no obstacle (hidden until 2 ExplorationCubes)
5. Player has 0 cubes: Only paths A and B visible
6. Player chooses based on equipment (waders → Path A better, rope → Path B better)

**Expected**: Hidden path rewards exploration, equipment determines optimal choice

---

### Integration Test Checklist

**✅ Data Flow**:
- [ ] JSON → Parser (validation works)
- [ ] Parser → Entity (fields populate correctly)
- [ ] Entity → GameWorld (ownership correct)
- [ ] GameWorld → Facade (queries work)
- [ ] Facade → UI (data displays correctly)

**✅ Context System**:
- [ ] Equipment-obstacle matching works
- [ ] Intensity reduction calculates correctly
- [ ] UI shows applicability before commitment
- [ ] Non-matching equipment grayed out

**✅ Cube System**:
- [ ] Location cubes reduce Mental Exposure
- [ ] NPC cubes reduce Social Doubt
- [ ] Route cubes reveal hidden paths
- [ ] No transfer between entities

**✅ Obligation System**:
- [ ] NPCCommissioned creates deadline pressure
- [ ] SelfDiscovered allows exploration
- [ ] Deadline failures remove StoryCubes
- [ ] Completion rewards apply correctly

**✅ Economic Loop**:
- [ ] Obligations grant coins
- [ ] Coins buy equipment
- [ ] Equipment reduces obstacles
- [ ] Obstacles block routes
- [ ] Routes access locations
- [ ] Loop functional end-to-end

---

## Phase 9: Legacy Code Purge - 1 Week

**Status**: ⏳ PENDING

### Entities to DELETE

```
❌ GameState/Knowledge.cs (replaced by Player.Understanding)
❌ GameState/UnlockCondition.cs (unlocking eliminated)
❌ GameState/GlobalDifficulty.cs (replaced by context cubes)
```

### Services to DELETE

```
❌ Services/KnowledgeFacade.cs
❌ Services/UnlockService.cs
❌ Services/GlobalProgressionService.cs
```

### UI Components to DELETE

```
❌ Pages/Components/KnowledgeDisplay.razor
❌ Pages/Components/UnlockNotification.razor
❌ Pages/Components/AbstractDifficultySlider.razor
```

### Parser Methods to DELETE

```
❌ ParseKnowledgeRequirements()
❌ ParseUnlockConditions()
❌ ParseGlobalDifficulty()
```

### JSON Auditing

**Remove from all JSON files**:
```json
// DELETE these fields
"knowledgeRequired": [...]
"unlockedBy": "..."
"prerequisites": {...}
"globalDifficulty": 5
```

### Grep and Destroy

```bash
# Search for legacy patterns
grep -r "Knowledge" --include="*.cs" --include="*.razor"
grep -r "Unlock" --include="*.cs" --include="*.razor"
grep -r "GlobalDifficulty" --include="*.cs" --include="*.razor"

# Expected: Zero results after purge
```

### Verification Checklist

**✅ No Knowledge System**:
- [ ] Zero references to Knowledge.cs
- [ ] Zero Knowledge token checks
- [ ] Zero Knowledge facade calls

**✅ No Unlock System**:
- [ ] Zero UnlockCondition references
- [ ] Zero unlock service calls
- [ ] Zero "unlockedBy" JSON fields

**✅ No Global Difficulty**:
- [ ] Zero GlobalDifficulty references
- [ ] Zero global progression service calls
- [ ] All difficulty is context-specific

---

## Success Metrics

### Validation Criteria

**✅ Economic Affordability**:
- Player always knows cost before commitment
- Player always knows how to afford blocked content
- No "game says no" feedback

**✅ Context Matching**:
- Equipment applicability obvious from contexts
- UI shows matching before player commits
- Non-matching equipment clearly indicated

**✅ Localized Mastery**:
- Mill expertise doesn't help Forest
- Elena relationship doesn't help Martha
- Each entity requires separate mastery

**✅ Deadline Pressure**:
- Multiple obligations force prioritization
- Failures have consequences (StoryCube loss)
- Time scarcity creates impossible choices

**✅ No Legacy Code**:
- Zero Knowledge references
- Zero Unlock conditions
- Zero Global difficulty
- All boolean gates eliminated

---

## Timeline Summary

| Phase | Duration | Status | Description |
|-------|----------|--------|-------------|
| 1 | 2 weeks | 🚧 IN PROGRESS | Content layer (JSON scaling, contexts) |
| 2 | 1 week | ⏳ PENDING | Parser layer (validation, DTOs) |
| 3 | 1 week | ⏳ PENDING | Entity layer (field extensions) |
| 4 | 1 week | ⏳ PENDING | GameWorld layer (ownership structure) |
| 5 | 2 weeks | ⏳ PENDING | Facade layer (services) |
| 6 | 2 weeks | ⏳ PENDING | Frontend layer (UI components) |
| 7 | 1 week | ⏳ PENDING | CSS layer (styling) |
| 8 | 1 week | ⏳ PENDING | Integration testing |
| 9 | 1 week | ⏳ PENDING | Legacy code purge |

**Total: 12 weeks to production-ready MVP**

---

## Next Steps

1. ✅ Complete Phase 1 (JSON value scaling)
2. ⏳ Begin Phase 2 (Parser extensions)
3. ⏳ Test integration after Phase 3
4. ⏳ MVP content authoring (3-location loop)
5. ⏳ Playtest with real players
6. ⏳ Iterate based on feedback

---

## References

- **Design Document**: [refinement-core-gameplay-loop.md](./refinement-core-gameplay-loop.md)
- **Architecture Principles**: [game-design-principles.md](./game-design-principles.md)
- **V2 Design**: [wayfarer-design-document-v2.md](./wayfarer-design-document-v2.md)
- **Implementation Guide**: [Architecture.md](./Architecture.md)
- **CLAUDE.md**: [CLAUDE.md](./CLAUDE.md)

---

**Last Updated**: 2025-10-15
**Status**: Phase 1 in progress
**Approval**: ✅ Plan approved by user
