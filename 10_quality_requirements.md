# Arc42 Section 10: Quality Requirements

## 10.1 Quality Tree

The Wayfarer quality model follows a three-tier hierarchy prioritizing requirements critical to player experience and system integrity.

```
Wayfarer Quality Goals
│
├─ TIER 1: Non-Negotiable (System Integrity)
│  ├─ No Soft-Locks Ever
│  ├─ Single Source of Truth
│  └─ Playability Over Compilation
│
├─ TIER 2: Core Experience (Player Value)
│  ├─ Strategic Depth Through Impossible Choices
│  ├─ Perfect Information at Strategic Layer
│  └─ Elegance Over Complexity
│
└─ TIER 3: Architectural Quality (Long-Term Maintainability)
   └─ Verisimilitude in All Systems
```

---

## 10.2 Quality Scenarios

Each quality goal translates into concrete, testable scenarios. Scenarios follow the format: Context → Stimulus → Response.

---

### QS-001: No Soft-Locks Ever (TIER 1)

**Quality Goal**: Player must ALWAYS have at least one viable path forward, regardless of previous choices or resource state.

#### Scenario 1.1: Zero-Resource A-Story Progression

**Context:**
- Player at A-story scene final situation
- Player resources: Coins 0, Stamina 0, Focus 0, Resolve 0
- Player stats: All below any threshold requirements
- No items in inventory

**Stimulus:**
- Player needs to advance A-story to next scene

**Response:**
- System presents at least one choice with:
  - Zero resource requirements
  - Zero stat requirements
  - Guaranteed success (Instant action OR Challenge with assured victory)
  - Advances to next A-scene
- **Metric**: 100% of A-story situations have ≥1 zero-requirement choice
- **Validation**: Automated test suite verifies all A-story JSON

#### Scenario 1.2: Challenge Failure Still Progresses

**Context:**
- Player in A-story situation with challenge-path choice
- Player selects challenge despite low stats
- Tactical challenge fails (didn't reach threshold)

**Stimulus:**
- Challenge ends with failure outcome

**Response:**
- OnFailureReward applied (may have costs, e.g., "pay extra 5 coins")
- Scene advances to next situation OR spawns next A-scene
- Player continues forward (different entry state, but progresses)
- **Metric**: 100% of A-story challenge paths have OnFailureReward that advances
- **Validation**: Parser validation rejects challenge without OnFailureReward

#### Scenario 1.3: Infinite Generation Never Soft-Locks

**Context:**
- Player 50+ hours into game
- Procedural A-story generation active
- Generated scene with multiple situations

**Stimulus:**
- System generates next A-scene via catalogues

**Response:**
- Generated scene passes structural validation:
  - Every situation has ≥1 zero-requirement choice
  - Final situation's all paths spawn next A-scene
  - No circular dependencies (Scene A requires Scene B, Scene B requires Scene A)
- **Metric**: 100% of generated scenes pass validation before spawn
- **Validation**: ContentValidator.ValidateSceneStructure() must pass

---

### QS-002: Single Source of Truth (TIER 1)

**Quality Goal**: Every piece of game state has exactly ONE canonical storage location. No redundant or parallel state tracking.

#### Scenario 2.1: Player Location Consistency

**Context:**
- Player navigates to new location
- Multiple systems need to know player location (UI, SceneFacade, SpawnFacade)

**Stimulus:**
- GameFacade.NavigateToLocation(newLocationId)

**Response:**
- GameWorld.Player.CurrentLocationId updated (ONLY place storing location)
- All systems query GameWorld for location, never cache independently
- **Metric**: Zero properties named "CurrentLocationId" outside GameWorld.Player
- **Validation**: Code search for "CurrentLocationId" returns only GameWorld.Player property

#### Scenario 2.2: Scene State Synchronization

**Context:**
- Scene has multiple situations
- Scene.CurrentSituation points to active situation
- Situation state changes (completes, advances)

**Stimulus:**
- Player completes choice, situation advances

**Response:**
- Scene.CurrentSituation updated via Scene.AdvanceToNextSituation() (single method)
- No parallel "activeSituationId" stored elsewhere
- UI queries Scene.CurrentSituation for display
- **Metric**: Zero duplicate situation tracking outside Scene entity
- **Validation**: No properties named "ActiveSituation" or "CurrentSituation" in services/facades

#### Scenario 2.3: Resource State Authority

**Context:**
- Player resources (Coins, Health, Stamina) modified during gameplay
- Multiple systems need current values (UI, ResourceFacade, choice validation)

**Stimulus:**
- Choice execution applies resource costs

**Response:**
- GameWorld.Player.Resources updated (ONLY authoritative source)
- ResourceFacade operates on GameWorld.Player (no internal state)
- UI queries GameWorld.Player for display (no cached copies)
- **Metric**: Zero resource properties outside GameWorld.Player
- **Validation**: ResourceFacade class has zero fields (stateless service)

---

### QS-003: Playability Over Compilation (TIER 1)

**Quality Goal**: Code that compiles but cannot be played/tested is unacceptable. Every feature must be reachable and testable.

#### Scenario 3.1: New Scene Reachability

**Context:**
- Developer adds new scene JSON to content package
- Scene has correct spawn conditions and situations
- Game compiles successfully

**Stimulus:**
- QA tester attempts to reach scene from game start

**Response:**
- Tester can trace exact path: Start → Actions → Scene activation
- All intermediate scenes/situations reachable without external tools
- **Metric**: 100% of authored scenes reachable via gameplay path
- **Validation**: Manual QA checklist + automated graph traversal test

#### Scenario 3.2: Challenge Deck Completability

**Context:**
- Developer creates new challenge deck for tactical session
- Deck has victory condition (SituationCard with threshold)
- Code compiles successfully

**Stimulus:**
- QA tester starts challenge session with deck

**Response:**
- Deck contains cards that can reach threshold:
  - Momentum-building cards for Social challenges
  - Progress-building cards for Mental challenges
  - Breakthrough-building cards for Physical challenges
- **Metric**: All challenge decks mathematically solvable
- **Validation**: DeckValidator checks card effects sum to threshold minimum

#### Scenario 3.3: Missing Dependency Detection

**Context:**
- JSON references entity by ID (locationId, npcId, cardId)
- Referenced entity doesn't exist in loaded content

**Stimulus:**
- Game starts, content loading begins

**Response:**
- Parser throws PackageLoadException with clear message:
  - "Scene 'inn_lodging_001' references NPC 'elena' which doesn't exist"
- Game doesn't load with broken references (fail-fast)
- **Metric**: Zero broken references reach runtime
- **Validation**: All ID references validated at parse-time

---

### QS-004: Strategic Depth Through Impossible Choices (TIER 2)

> **For game design philosophy of impossible choices and resource economy**, see [design/05_resource_economy.md](design/05_resource_economy.md).

**Quality Goal**: System enforces resource scarcity requiring strategic prioritization. Player cannot pursue all available options simultaneously.

#### Scenario 4.1: Resource Competition Validation

**Context:**
- Player has finite resources (time blocks, coins, stamina)
- Multiple actions available requiring those resources
- System tracks action availability and completion

**Stimulus:**
- Player allocates resources across competing actions

**Response:**
- System prevents pursuing all options (insufficient resources)
- UI displays exact costs and current resource levels
- Validation logic enforces resource constraints
- **Metric**: Average player completes 40-60% of available daily actions
- **Validation**: Telemetry tracks action completion rates, resource utilization patterns

#### Scenario 4.2: Stat Distribution Analysis

**Context:**
- Player progresses through game, advancing stats
- System tracks stat advancement across all players
- Multiple stat-gated choices exist throughout game

**Stimulus:**
- System collects player stat distributions at various progression points

**Response:**
- Stat advancement system enforces specialization (cannot max all stats)
- Telemetry shows variance in stat distributions across player population
- **Metric**: End-game stat distributions show specialization patterns (high variance, not uniform)
- **Validation**: Statistical analysis of player stat profiles

#### Scenario 4.3: Economy Pressure Monitoring

**Context:**
- Player earns and spends coins throughout gameplay
- System tracks coin reserves, income, expenses
- Multiple competing coin sinks exist

**Stimulus:**
- System monitors player economy state over time

**Response:**
- Economy maintains pressure (player rarely has surplus for all options)
- Resource management remains meaningful throughout gameplay
- **Metric**: Player coin reserves remain under 2× typical expense threshold
- **Validation**: Telemetry tracks coin balances, spending patterns, reserve ratios

---

### QS-005: Perfect Information at Strategic Layer (TIER 2)

**Quality Goal**: Player can see exact costs, requirements, and rewards before committing to strategic choices. No hidden gotchas.

#### Scenario 5.1: Choice Cost Visibility

**Context:**
- Player at location viewing available choices
- Multiple choices with different resource costs

**Stimulus:**
- UI displays choices to player

**Response:**
- All costs shown BEFORE selection:
  - "Pay 15 coins" (exact amount)
  - "Stamina -3" (exact deduction)
  - "Requires Rapport 6" (exact threshold, current value shown)
- Current resources displayed: "You have: 12 coins, Stamina 5/10"
- Gap visible: "Requires Rapport 6, you have 4" (need 2 more)
- **Metric**: 100% of strategic choices show exact costs/requirements
- **Validation**: UI review checklist

#### Scenario 5.2: Reward Transparency

**Context:**
- Player selecting choice with visible rewards
- Rewards include resource gains, unlocks, narrative outcomes

**Stimulus:**
- UI displays choice with rewards

**Response:**
- All rewards visible BEFORE execution:
  - "Gain: Coins +10, Understanding +1"
  - "Unlocks: Private Room access"
  - "OnSuccess: Room unlocked / OnFailure: Pay extra 5 coins"
- No "mystery boxes" or "???" rewards
- **Metric**: 100% of rewards shown explicitly (no hidden outcomes)
- **Validation**: UI review checklist

#### Scenario 5.3: Challenge Entry Transparency

**Context:**
- Player considering challenge-path choice
- Choice crosses to tactical layer

**Stimulus:**
- UI displays challenge choice

**Response:**
- Strategic costs shown: "Entry cost: Stamina -2"
- Conditional rewards shown:
  - "OnSuccess: Unlock private room, gain Coins +10"
  - "OnFailure: Pay extra 5 coins"
- Victory condition shown: "Reach Momentum 8 to succeed"
- Player can calculate: "Worth -2 Stamina for chance at +10 coins?"
- **Metric**: 100% of challenge choices show entry costs AND both outcomes
- **Validation**: UI review checklist

---

### QS-006: Elegance Over Complexity (TIER 2)

**Quality Goal**: Systems achieve goals with minimal interconnection. Clear boundaries, minimal coupling.

#### Scenario 6.1: Adding New Tactical System

**Context:**
- Developer wants to add fourth tactical system (e.g., "Spiritual" challenges)
- Existing: Social, Mental, Physical systems

**Stimulus:**
- Implement new tactical system

**Response:**
- Changes required:
  - Create new facade (SpiritualFacade) following pattern
  - Add enum value (ChallengeType.Spiritual)
  - Create cards following existing card structure
  - NO changes to Scene/Situation layer (bridge already generic)
  - NO changes to other tactical systems (parallel, not interdependent)
- **Metric**: <100 lines of integration code (system self-contained)
- **Validation**: Code review verifies no cross-system dependencies

#### Scenario 6.2: Refactoring Resource System

**Context:**
- Developer wants to change how Stamina works
- Current: Simple numeric depletion
- New: Tiered thresholds with different recovery rates

**Stimulus:**
- Refactor ResourceFacade

**Response:**
- Changes contained to:
  - ResourceFacade implementation
  - GameWorld.Player.Resources property structure
  - UI display components (reading new structure)
- NO changes to:
  - Scene/Situation definitions (use costs abstractly)
  - Challenge systems (read resources, don't define behavior)
  - Navigation systems (independent of resources)
- **Metric**: <5% of codebase modified for resource system change
- **Validation**: Git diff analysis

#### Scenario 6.3: Content Pipeline Independence

**Context:**
- Parser loads JSON, creates entities, populates GameWorld
- Developer wants to change JSON format (add new field)

**Stimulus:**
- Add categorical property to JSON schema

**Response:**
- Changes required:
  - Update DTO class with new property
  - Update Parser to read new property
  - Update Catalogue to translate new property
  - Update Entity class with concrete property
- NO changes to:
  - GameWorld collections
  - Service facades (use entity properties)
  - UI components (render entity properties)
- **Metric**: Changes contained to Parsers/ folder + single entity class
- **Validation**: Dependency analysis shows no ripple effects

---

### QS-007: Verisimilitude in All Systems (TIER 3)

**Quality Goal**: Entity relationships and system behavior match player's conceptual model. No backwards explanations.

#### Scenario 7.1: Spatial Hierarchy Comprehension

**Context:**
- New player explores game world
- World structure: Venue → Location → (within-venue instant navigation)

**Stimulus:**
- Player navigates between locations

**Response:**
- Mental model matches implementation:
  - "Inn" venue contains "Common Room", "Private Room", "Stables"
  - Moving within inn is instant (conceptually same building)
  - Traveling to different inn requires route (conceptually different place)
- Explanation feels natural: "Locations within venue are sub-spaces"
- **Metric**: <10% of playtesters confused by spatial navigation
- **Validation**: Playtester feedback survey

#### Scenario 7.2: Scene Ownership Intuition

**Context:**
- Player engages with scene at location
- Scene has multiple situations sequenced across locations

**Stimulus:**
- Player completes situation, scene advances to new location

**Response:**
- Mental model: "This scene continues at new location"
- NOT confused by: "Why is scene at location?" (placement, not ownership)
- Explanation matches code: Scene placed at Locations, not owned by them
- **Metric**: <5% of playtesters ask "Why is this scene appearing here?"
- **Validation**: Playtester comprehension survey

#### Scenario 7.3: Challenge Ephemeral Nature

**Context:**
- Player engages in tactical challenge (conversation, investigation, obstacle)
- Challenge session active with temporary resources

**Stimulus:**
- Challenge completes (success or failure)

**Response:**
- Mental model: "Challenge was a moment in time, now it's done"
- Understand: Session resources (Momentum, Progress) don't persist
- Persistent resources (Understanding, NPC bonds) carry forward
- Explanation natural: "Conversations happen and end, relationships persist"
- **Metric**: <5% of playtesters expect Momentum to persist between conversations
- **Validation**: Playtester mental model survey

---

## 10.3 Quality Metrics Summary

### Automated Validation (Continuous Integration)

| Quality Goal | Metric | Threshold | Enforcement |
|---|---|---|---|
| No Soft-Locks | A-story scenes with zero-req choice | 100% | Parser validation |
| Single Source of Truth | Duplicate state properties | 0 | Code review checklist |
| Playability | Scenes reachable from start | 100% | Graph traversal test |
| Strategic Depth | Player action completion rate | 40-60% | Telemetry analysis |
| Perfect Information | Choices showing costs/rewards | 100% | UI review checklist |
| Elegance | Cross-system dependencies | <5% | Dependency analysis |
| Verisimilitude | Playtester confusion rate | <10% | Survey feedback |

### Manual Validation (Release Checklist)

- **QA Playthrough**: Can tester reach all authored content from game start?
- **Challenge Solvability**: Are all challenge decks mathematically completable?
- **Resource Pressure**: Does economy maintain strategic tension (not too easy/hard)?
- **Mental Model Alignment**: Do playtesters' descriptions match architecture?

### Telemetry Monitoring (Post-Release)

- **Soft-Lock Detection**: Any players stuck >30 minutes without forward progress?
- **Choice Distribution**: Are stat-gated choices accessed by ≥30% of players?
- **Resource Utilization**: Are players maintaining strategic reserves or always broke?
- **Session Length**: Average 30-120 minute sessions (not too short/long)?

---

## 10.4 Quality Goal Trade-Offs

When quality goals conflict, resolve via principle priority (see ADR-006):

| Conflict | Resolution | Rationale |
|---|---|---|
| No Soft-Locks vs Resource Scarcity | Add zero-cost fallback choices | TIER 1 wins, scarcity preserved via poor fallback rewards |
| Perfect Information vs Tactical Surprise | Layer separation | Strategic = perfect info, Tactical = hidden complexity |
| Elegance vs Playability | Accept complexity for critical features | TIER 2 > TIER 3 |
| Single Source of Truth vs Performance | Ephemeral cache + authoritative ID | TIER 1 satisfied if one is cache |
| Verisimilitude vs Implementation Cost | Hierarchical spatial model | TIER 3 quality worth implementation complexity |

---

## Related Documentation

- **01_introduction_and_goals.md** - Quality goals and stakeholder concerns
- **09_architecture_decisions.md** - ADRs implementing quality requirements
- **02_constraints.md** - Constraints affecting quality achievement
- **08_crosscutting_concepts.md** - Patterns ensuring quality across system
