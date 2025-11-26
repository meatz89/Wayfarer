# 8. Crosscutting Concepts

This section documents patterns and practices that apply across multiple building blocks, providing conceptual integrity throughout the architecture.

---

## 8.1 HIGHLANDER Principle

**"There can be only one."**

Every piece of game state has exactly one canonical storage location. No redundant tracking, no parallel state, no caching that could desync.

**Implementation:**
- Player location stored only in `Player.CurrentPosition` (hex coordinates)
- Scene's current situation stored only in `Scene.CurrentSituation` (object reference)
- Entity relationships use direct object references, never ID strings alongside objects

**Consequences:**
- No "which is correct?" ambiguity when state disagrees
- Single update point for each state change
- Queries always hit source of truth

**Violation Example:** Storing both `CurrentLocationId` string and `CurrentLocation` object creates two representations. When they disagree, the system has no way to determine correctness.

---

## 8.2 Catalogue Pattern (Parse-Time Translation)

Content authors write categorical properties; catalogues translate to concrete mechanical values at parse-time only.

```mermaid
flowchart LR
    JSON["JSON\n{demeanor: Friendly}"]
    Catalogue["Catalogue\nFriendly → 0.6x"]
    Entity["Entity\nStatThreshold = 3"]

    JSON -->|parse-time| Catalogue
    Catalogue -->|one-time| Entity
```

| Component | Responsibility |
|-----------|----------------|
| **JSON** | Categorical descriptions (Friendly, Premium, Hostile) |
| **Catalogue** | Translation formulas (multipliers, base values) |
| **Entity** | Concrete values only (integers, no categories) |

**Consequences:**
- AI generates balanced content without knowing game math
- Single formula change rebalances all affected content
- Zero runtime overhead (translation complete at startup)

**Forbidden:** Runtime catalogue lookups, string matching on property names, Dictionary<string, int> for costs.

---

## 8.3 Entity Identity Model

Domain entities have no instance IDs. Relationships use direct object references. Queries use categorical properties.

**Allowed:**
- Template IDs (SceneTemplate.Id) — immutable archetypes
- Object references (NPC.Location) — direct relationships
- Categorical filters (find Location where Purpose=Lodging)

**Forbidden:**
- Entity instance IDs (Scene.Id, NPC.Id, Location.Id)
- ID strings alongside object references
- ID parsing for routing logic

**Why:** Procedural generation requires categorical matching ("find a Friendly innkeeper"), not hardcoded references ("find NPC elena_001"). IDs create brittleness; categories enable infinite content.

---

## 8.4 Three-Tier Timing Model

Content instantiates lazily across three timing tiers to minimize memory usage.

| Tier | When | What | Mutability |
|------|------|------|------------|
| **Templates** | Parse-time | SceneTemplate, ChoiceTemplate | Immutable |
| **Instances** | Spawn-time | Scene, Situation | Mutable |
| **Actions** | Query-time | LocationAction, NPCAction | Ephemeral |

**Flow:**
1. Templates created once at startup, stored permanently
2. Instances spawn when triggered (obligation activates, procedural generation fires)
3. Actions materialize when player enters relevant context, deleted after execution

**Consequence:** Memory contains only currently accessible content. Scene with 5 situations × 3 choices = 15 potential actions, but only 3 exist at any moment.

---

## 8.5 Fail-Fast Philosophy

Errors surface immediately at point of failure with clear stack traces. No defensive coding that hides problems.

**Required:**
- Initialize all entity properties inline (empty List, empty string)
- Access properties directly without null checks
- Let null references crash with clear stack traces

**Forbidden:**
- Null-coalescing operators (??) hiding missing data
- TryGetValue patterns deferring errors
- Default return values masking lookup failures

**Rationale:** A crash with clear stack trace is debuggable. Silent null propagation creates mystery bugs discovered far from root cause.

---

## 8.6 Backend/Frontend Separation

Backend returns domain semantics (WHAT). Frontend decides presentation (HOW).

**Backend provides:**
- Domain enums (ActionType, ResourceType, ConnectionState)
- Plain values (integers, booleans, strings)
- State validity (is action available, does player meet requirements)

**Frontend decides:**
- CSS classes and styling
- Icon selection
- Display text generation
- Visual formatting

**Forbidden in Backend:**
- CssClass properties in ViewModels
- IconName properties in ViewModels
- Display string generation in services

**Rationale:** Changing presentation never touches game logic. Changing mechanics never requires UI updates beyond data flow.

---

## 8.7 Idempotent Initialization

Blazor ServerPrerendered mode executes initialization twice. All startup code must be idempotent.

**Pattern:**
```csharp
if (!_initialized)
{
    // Perform initialization
    _initialized = true;
}
```

**Applies to:**
- Component OnInitializedAsync
- State mutations during startup
- Event subscriptions
- Resource allocation

**Consequence:** Double-execution is safe; state mutations guarded by flags.

---

## 8.8 Dual-Tier Action Architecture (CRITICAL)

**LocationAction is a UNION TYPE supporting two intentional patterns via pattern discrimination.**

This architecture prevents soft-locks by ensuring atmospheric actions always exist as a baseline, while scene-based actions layer dynamic narrative content on top.

```mermaid
flowchart TB
    subgraph "Action Resolution"
        Check{ChoiceTemplate<br/>== null?}
        Atmospheric["Atmospheric Action<br/>(Tier 1)"]
        SceneBased["Scene-Based Action<br/>(Tier 2)"]
    end

    subgraph "Atmospheric (Permanent)"
        LC[LocationActionCatalog]
        DirectProps["Direct Properties<br/>Costs, Rewards"]
        GW1[(GameWorld.LocationActions)]
    end

    subgraph "Scene-Based (Ephemeral)"
        SF[SceneFacade]
        Template["ChoiceTemplate<br/>CostTemplate, RewardTemplate"]
        Query["Query-time creation"]
    end

    Check -->|Yes| Atmospheric
    Check -->|No| SceneBased
    Atmospheric --> DirectProps
    LC --> GW1
    SceneBased --> Template
    SF --> Query
```

| Tier | Pattern | Source | Storage | Properties Used |
|------|---------|--------|---------|-----------------|
| **Tier 1: Atmospheric** | ChoiceTemplate == null | LocationActionCatalog at parse-time | GameWorld.LocationActions (permanent) | `Costs`, `Rewards` directly |
| **Tier 2: Scene-Based** | ChoiceTemplate != null | SceneFacade at query-time | Not stored (ephemeral) | `ChoiceTemplate.CostTemplate`, `RewardTemplate` |

**Why Both Patterns Exist:**

*Atmospheric actions are simple and permanent:*
- Work always costs time, always gives coins
- Rest always recovers health/stamina
- Travel always opens route selection
- ChoiceTemplate would be overkill for constants

*Scene-based actions are complex and dynamic:*
- Costs vary by context (NPC personality, location tier)
- Requirements use OR paths (need stat X OR stat Y)
- Rewards spawn scenes, modify relationships
- Direct properties would be insufficient

**Pattern Discrimination:**
```csharp
if (action.ChoiceTemplate == null)
{
    // Atmospheric: use action.Costs, action.Rewards
    return ValidateAtmosphericAction(action, player);
}
else
{
    // Scene-based: use action.ChoiceTemplate
    return ValidateChoiceTemplate(action.ChoiceTemplate, player);
}
```

**Critical Warning:** Do NOT delete `Costs`/`Rewards` properties from LocationAction. They are REQUIRED for atmospheric actions. This is not legacy code—both patterns are intentional architecture.

---

## 8.9 Entity Ownership Hierarchy

Entities follow strict ownership patterns determining lifecycle and responsibility.

```mermaid
flowchart TB
    subgraph "Ownership (Embedded)"
        Scene --> Situation
        Situation --> ChoiceRef["ChoiceTemplate (ref)"]
    end

    subgraph "Placement (Location-based)"
        Location -.->|placed at| Scene
        Location -.->|present at| NPC
    end

    subgraph "Reference (Lookup)"
        NPC -.->|works at| WorkLoc[Location]
        NPC -.->|lives at| HomeLoc[Location]
    end
```

| Relationship | Type | Meaning |
|--------------|------|---------|
| Scene → Situation | **Ownership** | Scene OWNS situations; deleting scene deletes situations |
| Scene → Location | **Placement** | Scene placed AT location; location doesn't own scene |
| NPC → Location | **Reference** | NPC references location; neither owns the other |

**Key Principle:** Situations are EMBEDDED in Scenes (no separate collection). This prevents orphaned situations and simplifies scene lifecycle.

---

## 8.10 Categorical Property Architecture

**Every categorical property is strongly-typed with intentional domain meaning.**

Entities are selected via categorical filters, not generic strings. All categorical properties map to strongly-typed enums with specific game effects.

### Two Distinct Concepts for Entity Selection

**Identity Dimensions (What the entity IS):**
- Describe atmosphere, context, and character
- Multiple orthogonal dimensions compose to create archetypes
- Empty list = don't filter (any value matches)
- Non-empty list = entity must have ONE OF the specified values

**Capabilities (What the entity CAN DO):**
- Enable specific game mechanics
- Flags enum with bitwise operations
- Entity must have ALL specified capabilities to match

### Location Categorical Dimensions

| Dimension | Enum | Values | Domain Meaning |
|-----------|------|--------|----------------|
| **Privacy** | `LocationPrivacy` | Public, SemiPublic, Private | Social exposure and witness presence |
| **Safety** | `LocationSafety` | Dangerous, Neutral, Safe | Physical threat level |
| **Activity** | `LocationActivity` | Quiet, Moderate, Busy | Population density |
| **Purpose** | `LocationPurpose` | Transit, Dwelling, Commerce, Civic, Defense, Governance, Worship, Learning, Entertainment, Generic | Primary functional role |

### Location Capabilities (Flags Enum)

| Capability | Game Mechanic Effect |
|------------|---------------------|
| `Crossroads` | Enables Travel action (route selection UI) |
| `Commercial` | Enables Work action (earn coins) |
| `SleepingSpace` | Enables Rest action (restore health/stamina) |
| `Restful` | Enhanced restoration quality |
| `Indoor`/`Outdoor` | Environmental context (weather affects gameplay) |
| `Market` | Pricing modifier (1.1x) |
| `LodgingProvider` | Accommodation services available |

### NPC Categorical Dimensions

| Dimension | Enum | Purpose |
|-----------|------|---------|
| **Professions** | `Professions` | Occupational role (Innkeeper, Merchant, Guard) |
| **PersonalityTypes** | `PersonalityType` | Behavioral archetype (Innocent, Cunning, Authoritative) |
| **SocialStandings** | `NPCSocialStanding` | Influence tier (Notable, Authority) |
| **StoryRoles** | `NPCStoryRole` | Narrative function (Obstacle, Facilitator) |
| **KnowledgeLevels** | `NPCKnowledgeLevel` | Information access (Informed, Expert) |

### Parser Validation (Fail-Fast)

All categorical strings are validated at parse-time:

```csharp
if (Enum.TryParse<LocationCapability>(capabilityString, true, out LocationCapability capability))
    capabilities |= capability;
else
    throw new InvalidDataException($"Invalid LocationCapability: '{capabilityString}'");
```

**Consequences:**
- Invalid enum values fail immediately at startup, not runtime
- No generic strings pass through unvalidated
- Content authors must use exact enum value names
- Typos and invalid values are impossible to deploy

### JSON to Entity Mapping

| JSON Field | DTO Property | Entity Property | Type |
|------------|--------------|-----------------|------|
| `privacyLevels` | `List<string>` | `List<LocationPrivacy>` | Parsed enum list |
| `capabilities` | `List<string>` | `LocationCapability` | Parsed flags enum |
| `professions` | `List<string>` | `List<Professions>` | Parsed enum list |

**Key Files:**
- `src/GameState/LocationPrivacy.cs` — Privacy enum with XML doc
- `src/GameState/LocationCapability.cs` — Capabilities flags enum
- `src/GameState/PlacementFilter.cs` — Filter entity with all dimensions
- `src/Content/Parsers/SceneTemplateParser.cs:264-499` — Enum parsing with validation

---

## 8.11 Location Accessibility Architecture

**Dual-model accessibility ensures TIER 1 No Soft-Locks while supporting scene-gated dependent locations.**

See [ADR-012](09_architecture_decisions.md#adr-012-dual-model-location-accessibility) for decision rationale.

### The Problem

Locations fall into two categories with different accessibility requirements:
- **Authored locations:** Defined in base game JSON (inns, taverns, checkpoints)—must always be reachable
- **Scene-created locations:** Created dynamically by scenes during gameplay (private rooms, meeting chambers)—should only be accessible after narrative progression

A naive implementation (scene-grants-access for ALL locations) blocked authored locations when no scene was active at them—violating TIER 1.

### The Solution: Explicit LocationOrigin Enum

`Location.Origin` enum provides explicit, type-safe discriminator:

```csharp
public enum LocationOrigin
{
    Authored,      // Base game content - always accessible
    SceneCreated   // Created by scene - requires scene access
}
```

| Origin Value | Location Type | Accessibility Rule |
|--------------|---------------|-------------------|
| `Authored` | Base game content | **ALWAYS accessible** (No Soft-Locks) |
| `SceneCreated` | Scene-generated | Accessible when active scene's current situation is at location |

**Clean Architecture:** Uses explicit enum instead of null-as-domain-meaning pattern. The separate `Provenance` property provides forensic metadata (which scene, when) but is NOT used for accessibility decisions.

### Implementation

**LocationAccessibilityService.IsLocationAccessible():**
```csharp
// AUTHORED: Always accessible per TIER 1
if (location.Origin == LocationOrigin.Authored)
    return true;

// SCENE-CREATED: Accessible when situation is at location
return _gameWorld.Scenes
    .Where(scene => scene.State == SceneState.Active)
    .Any(scene => scene.CurrentSituation?.Location == location);
```

### Service Interaction

```
MovementValidator.ValidateMovement()
    └─ IsSpotAccessible(targetLocation)
        └─ LocationAccessibilityService.IsLocationAccessible(location)
            ├─ Origin == Authored → return true
            └─ Origin == SceneCreated → CheckSceneGrantsAccess()
```

### Example: Inn Lodging Scene

1. **Scene activates** at Common Room (authored location—always accessible)
2. **Situation 1** (Negotiate): Player talks to innkeeper at Common Room
3. **Player completes situation 1** by selecting a choice
4. **Scene advances**: `CurrentSituationIndex` moves to Situation 2
5. **Situation 2** (Rest): Location = Private Room (scene-created)
6. **Private Room becomes accessible**: Active scene's current situation is now at Private Room
7. **Player moves** to Private Room (accessibility check passes)
8. **Scene displays** Situation 2 choices

### Why Not GrantsLocationAccess Property?

A proposed `SituationTemplate.GrantsLocationAccess` property was removed as dead code:
- If situation is at scene-created location, player MUST access it to engage
- Setting `GrantsLocationAccess = false` would guarantee a soft-lock
- Therefore the property can NEVER meaningfully be false
- Situation presence at location implies access (no explicit property needed)

### Key Files

| File | Purpose |
|------|---------|
| `src/GameState/LocationOrigin.cs` | Explicit discriminator enum |
| `src/Subsystems/Location/LocationAccessibilityService.cs` | Dual-model accessibility logic |
| `src/Subsystems/Location/MovementValidator.cs` | Delegates accessibility checks |
| `src/Content/Location.cs` | `Origin` and `Provenance` property definitions |
| `src/GameState/SceneProvenance.cs` | Forensic tracking structure (not used for accessibility) |

---

## Related Documentation

- [04_solution_strategy.md](04_solution_strategy.md) — Strategies these concepts implement
- [09_architecture_decisions.md](09_architecture_decisions.md) — ADRs documenting why these patterns were chosen
- [02_constraints.md](02_constraints.md) — Constraints driving these concepts
