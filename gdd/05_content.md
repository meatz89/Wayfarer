# 5. Content Structure

## Why This Document Exists

This document explains HOW content is organized and WHY the archetype-based system enables infinite balanced content. The separation of mechanical pattern from narrative flavor is the key insight.

---

## 5.1 Story Categories: A/B/C

Story categories are distinguished by a **combination of properties**, not a single axis. All three use identical Scene-Situation-Choice structure; category determines rules and validation.

### Story Category Property Matrix

| Property | A-Story | B-Consequence | B-Sought | C-Story |
|----------|---------|---------------|----------|---------|
| **Structure** | Infinite scene chain | One scene, 3-8 situations | One scene, 3-8 situations | One scene, 1-2 situations |
| **Repeatability** | One-time (sequential) | One-time per trigger | System-repeatable | System-repeatable |
| **Fallback Required** | Yes (every situation) | No | No | No |
| **Can Fail** | Never | Yes | Yes | Yes |
| **Resource Flow** | Sink (travel costs) | Source (premium) | Source (basic) | Texture (minor) |
| **Typical Scope** | World expansion | Venue depth (continues A) | Venue/Location | Location flavor |
| **Player Agency** | Mandatory | Mandatory (earned) | Opt-in | Mandatory |
| **Spawn Trigger** | Previous A-scene completes | A-story choice success | Player acceptance | Natural journey emergence |
| **Declinable** | No | No | Yes | No |

> **Note:** Both B-types use `SideStory` enum. See [08_glossary.md](08_glossary.md) §Story Categories for full definitions.

### A-Story: The Infinite Main Thread

The primary narrative spine that never ends. Scenes chain sequentially (A1 → A2 → A3...). Every situation requires a fallback choice guaranteeing forward progress. Primary purpose is world expansion—creating new venues, districts, regions, routes, and NPCs. **Player cannot decline**—when an A-scene activates, engagement is mandatory.

**Phase 1: Tutorial Instantiation (A1-A10)**
- Uses the SAME scene structure as procedural content
- Each scene is an arc (Building situations → final Crisis)
- 30-60 minutes of guided introduction

**Phase 2: Procedural Continuation (A11+)**
- Uses the SAME scene structure as tutorial
- Template selected via intensity history + anti-repetition
- Never resolves, always deepens

**Critical Principle (HIGHLANDER):** Tutorial and procedural content flow through IDENTICAL structure. The ONLY difference is context source:

| Content | Context Source | Scene Structure |
|---------|----------------|-----------------|
| Tutorial | Authored in SceneSpawnReward | Building → Crisis arc |
| Procedural | Computed from intensity history | Building → Crisis arc |

**Why infinite:** Eliminates ending pressure. No post-game awkwardness. Player chooses when to engage. The journey IS the destination.

### B-Story: Two Types Serving Different Purposes

B-stories provide resources that fund A-story travel. They come in **two distinct types**:

#### B-Consequence (Earned Reward)

**Spawns automatically when player succeeds at hard A-story stat checks.** This is the Sir Brante pattern—certain scenes only unlock because the player made specific choices that had specific requirements.

- **Cannot be declined** — just happens as consequence of success
- **Continues A-story narrative** — same NPCs, same locations, deeper story
- **Premium rewards** — major resources for demonstrated mastery
- **One-time per trigger** — each A-story success can spawn its B-consequence once

#### B-Sought (Player-Initiated)

**Quest content the player actively seeks out.** Found through exploration, NPC conversations, job boards, and world discovery.

- **Can be declined** — player chooses whether to engage
- **Independent narrative** — new characters, new situations
- **Basic rewards** — reliable income for effort invested
- **System-repeatable** — job boards always have work available

**Discovery Sources:**
- **Job Board** — Lists available contracts at venues
- **NPC Quest Giver** — Dialogue option to accept commission
- **Exploration** — Discovering locations reveals opportunities

#### Why Both Types Exist

| Type | Purpose | Player Experience |
|------|---------|-------------------|
| B-Consequence | Rewards mastery | "My investment paid off" |
| B-Sought | Prevents soft-lock | "I can always find work" |

**Design Intent:** Skilled players receive B-Consequence automatically—they never need to grind. Struggling players can always find B-Sought work. Both paths fund A-story progression.

### C-Story: Natural Journey Emergence

Single-scene encounters providing **world texture**. C-stories are not spawned by explicit game mechanics—they **emerge naturally from the journey** that A and B stories create.

**Natural Emergence Sources:**
- **Travel routes** — Moving between A-story locations creates route encounters (terrain-themed)
- **Location visits** — Being in a place creates opportunity for location flavor
- **NPC interactions** — Meeting characters through A/B stories creates incidental moments

**Player cannot decline or willingly spawn**—these are surprises that flesh out the world. The journey to story IS content. A and B stories create the context; C-stories fill the texture naturally.

**Examples:** Forest ambush on route to distant A-story, inn gossip while resting, merchant haggling at market visited during B-story.

**Key Insight:** C-stories are not a separate system. They are the natural consequence of A/B story journeys—terrain, locations, and NPCs responding to player presence.

### Why These Categories?

The categories form an interconnected narrative and economic ecosystem:

| Category | Player Experience | Economic Role |
|----------|------------------|---------------|
| **A-Story** | "The main quest continues" | Resource SINK (travel costs) |
| **B-Consequence** | "My success unlocked this" | Primary SOURCE (premium) |
| **B-Sought** | "I need work, let me find some" | Reliable SOURCE (basic) |
| **C-Story** | "The journey itself" | World TEXTURE (minor) |

**The Economic Design:**
- **Mastery Path:** Invest stats → succeed at checks → B-Consequence rewards fund travel automatically
- **Fallback Path:** Use fallbacks → need resources → seek B-Sought work → earn travel funds
- **Safety Net:** Atmospheric Work always available (last resort)

**Key Insight:** B-Consequence rewards mastery (making grinding unnecessary for skilled players). B-Sought prevents soft-lock (ensuring resources are always obtainable). Together they create fair progression where skill is rewarded but never required.

---

## 5.2 Scene-Situation-Choice Flow

All narrative content follows the same structure:

```
Scene (Container)
├── Situation 1
│   ├── Choice A (4 paths)
│   ├── Choice B (4 paths)
│   └── ...
├── Situation 2
│   └── ...
└── Situation N (Resolution)
```

**Scene:** Complete narrative arc with beginning, development, resolution
**Situation:** Decision point within scene presenting choices
**Choice:** The four-choice archetype (stat-gated, resource, challenge, fallback)

---

## 5.3 The Archetype System

### The Core Insight

Separate reusable mechanical patterns from narrative content. Same archetype + different entity properties = infinite contextually-appropriate variations.

### What an Archetype Defines

**Structure:**
- Number of choices (typically 4)
- Path types per choice
- Base cost formulas
- Base reward formulas

**What an Archetype Does NOT Define:**
- Narrative text (AI-generated from game context at activation-time)
- Specific entity IDs (uses categorical filters)
- Absolute numeric values (scaled by properties)

### Two-Pass Generation Pipeline

All content flows through a two-pass pipeline. Hand-authored narrative text is forbidden.

**Pass 1: Mechanical Generation (Parse-Time)**
- Archetypes generate choice structures from categorical properties
- Catalogues translate properties to concrete costs/rewards
- Result: Mechanically complete entity with scaled values

**Pass 2: AI Narrative Enrichment (Activation-Time)**
- AI receives complete game context (NPC, Location, Player, World State)
- AI generates narrative flavor from context
- Result: Narrative text persisted to entity, displayed in UI

**Why Two Passes:**
- Mechanics are deterministic and testable (Pass 1)
- Narrative emerges from context, not pre-written (Pass 2)
- Same mechanical archetype + different context = different narrative
- Enables infinite unique content from finite patterns

**Forbidden Shortcuts:**
- Hand-writing narrative text in JSON content
- Fixing mechanical issues by editing JSON values
- Different archetypes for tutorial vs procedural
- Bypassing the generation pipeline with hardcoded content

### Example: Negotiation Archetype

The same "negotiation" archetype works for:
- Securing lodging from innkeeper
- Gaining passage through checkpoint
- Acquiring information from scholar
- Purchasing goods from merchant
- Requesting healing from priest

Entity properties (NPC demeanor, location quality, power dynamic) scale difficulty. Same mechanical structure, contextually appropriate experience.

### Mathematical Variety

21 situation archetypes × 4 NPC demeanors × 4 quality tiers × 3 power dynamics = **1,008 mechanically distinct variations** from finite patterns.

### Archetype Reusability Principle

**Every archetype must work in ANY context through categorical scaling.**

| Context | Same Archetype Produces |
|---------|------------------------|
| Tutorial (Tier 0, Friendly, Basic) | Easy requirements, low costs, modest rewards |
| Mid-game (Tier 2, Neutral, Standard) | Medium requirements, balanced costs/rewards |
| Late-game (Tier 3, Hostile, Premium) | Hard requirements, high costs, high rewards |

**Forbidden:**
- Checking story sequence (`AStorySequence == N`) in archetypes
- Different code paths for tutorial vs non-tutorial
- Hardcoded values that don't scale with context

**Required:**
- All difficulty scaling via categorical properties (NPCDemeanor, Quality, PowerDynamic)
- Same four-choice structure regardless of context
- Context-agnostic archetype code

---

## 5.4 Categorical Property Scaling

### How Context Affects Difficulty

Properties on entities modify base archetype values:

**NPCDemeanor:** Friendly (reduced costs) → Neutral (base costs) → Suspicious (increased costs) → Hostile (heavily increased costs)

**Quality:** Poor (reduced costs) → Standard (base costs) → Fine (increased costs) → Exceptional (heavily increased costs)

**PowerDynamic:** Subordinate (reduced costs) → Equal (base costs) → Superior (increased costs) → Authority (heavily increased costs)

### Why This Enables AI Content

AI authors can write compelling narrative without knowing game state:
1. AI writes descriptive properties ("suspicious innkeeper", "fine quality inn")
2. Catalog translates properties to absolute adjustments
3. Archetype applies adjustments to base costs
4. Result: Balanced content without global knowledge

This approach implements DDR-007 (Intentional Numeric Design): properties affect costs through absolute additions and subtractions, never through multipliers or percentages. This ensures players can mentally calculate final costs and maintain transparency in game math.

---

## 5.5 The Atmospheric Action Layer

### Always-Available Actions

Regardless of scene state, players can always:
- **Travel** — Initiate route to another venue, earn delivery coins
- **Work** — Perform odd jobs for immediate income
- **Rest** — Consume resources to restore pools
- **Move** — Navigate between adjacent locations

**Why this matters:** Scene-based content layers ON TOP of atmospheric actions. When scenes complete, atmospheric actions remain. Player is NEVER trapped with "nothing to do."

### Scene-Based Actions

Temporary actions available only while scenes are active:
- Appear when scenes spawn
- Present narrative choices
- Disappear when scenes complete
- Layer alongside atmospheric actions, not replacing them

---

## 5.6 Story Category Rules

### A-Story Guarantees

A-Story scenes have strict requirements to prevent soft-locks:

| Guarantee | Enforcement |
|-----------|-------------|
| **Fallback exists** | Every situation has at least one choice with NO requirements |
| **Progression assured** | ALL final situation choices spawn next A-scene |
| **World expansion** | Scenes create new venues/districts/regions/routes/NPCs |

These guarantees are enforced at parse-time via **Centralized Invariant Enforcement**. The parser—not individual archetypes—applies category-wide rules. This ensures no archetype can accidentally omit an invariant.

See [arc42/08_crosscutting_concepts.md §8.18](../arc42/08_crosscutting_concepts.md) and [ADR-016](../arc42/09_architecture_decisions.md) for technical details.

**Why A-Story is special:** The Frieren principle—infinite, never-ending. Primary purpose is world expansion, creating new places to explore, new people to meet. Player must ALWAYS be able to progress.

### Scene Lifecycle (UNIFIED PATH)

**Two concepts:**

| Concept | What It Is | Created When |
|---------|------------|--------------|
| **SceneTemplate** | Immutable archetype with SituationTemplates | Parse-time (from JSON) |
| **Scene** | Mutable instance (Situations created at activation) | Spawn-time (after DI available) |

**Critical principle:** There is NO difference between authored and procedural scene creation. Both follow the same mechanism:

```
SceneTemplate + Context → SceneInstantiator.CreateDeferredScene() → Scene (Deferred)
```

| Content | SceneTemplate Source | Context Source | Scene Creation |
|---------|---------------------|----------------|----------------|
| **Authored (A1-A10)** | JSON (sceneTemplates) | Authored in template | Spawn-time |
| **Procedural (A11+)** | Generated + registered | Derived from GameWorld | Spawn-time |

**How scenes spawn:**

| Scene | Trigger | Implementation |
|-------|---------|----------------|
| **A1 (Starter)** | Game start | `SpawnStarterScenes()` finds template with `IsStarter=true` |
| **A2-A10 (Authored)** | Final choice reward | `SceneSpawnReward { SpawnNextMainStoryScene = true }` |
| **A11+ (Procedural)** | Final choice reward | Same mechanism, procedural template generation fallback |

**HIGHLANDER:** ONE path for all scenes. JSON contains SceneTemplates only, never Scene instances.

See [arc42/08_crosscutting_concepts.md §8.4](../arc42/08_crosscutting_concepts.md) for the complete pipeline.

### B/C Story Flexibility

B and C stories have relaxed rules compared to A-story:

- Can require stats, resources, or completed prerequisites
- Can include challenge paths without fallbacks
- Can fail (scene marked Failed, not Completed)
- Focus on resource generation and narrative depth

See §5.1 for the complete property matrix distinguishing A/B/C stories.

---

## 5.7 The Travel Cost Gate (Endless Story Design)

### The Design Problem

A-Story must never soft-lock (fallback always exists), yet players must not mindlessly click through the main story without engagement. How do we create meaningful pacing without hard gates?

### The Solution: Distance Creates Resource Demand

A-Story scenes spawn at locations that increase in distance from the world origin. Travel to those locations costs resources. B/C stories earn those resources.

| Element | Rule |
|---------|------|
| **A-Story distance** | Scene N spawns at distance N hexes from origin |
| **Travel cost** | 1 resource unit per hex (Stamina, Coins, or both) |
| **Resource source** | B/C stories (deliveries, obligations, work) |

### The Loop

```
A-Story Scene N (one scene = one arc):
    Building Situation 1 → Building Situation 2 → ... → Crisis Situation (final)
    ↓
Take stat-gated choice in Crisis → B-Consequence spawns (reward thread)
    ↓
B-Consequence: Continue narrative with same NPCs, earn premium rewards
    ↓
A-Story Scene N+1 spawns at distance N+1
    ↓
Travel journey → C-Stories emerge naturally (route/location/NPC texture)
    ↓
Arrive at A-Story N+1
    ↓
Repeat forever
```

### Story Category Relationships

Per the property matrix in §5.1:
- **A-Story** is a resource SINK (travel costs); each scene contains Building situations → final Crisis situation
- **B-Consequence** is REWARD for A-story success (spawns automatically on stat check success)
- **B-Sought** is FALLBACK income (player seeks out via job boards, NPC offers)
- **C-Story** is world TEXTURE (emerges naturally from journey)
- **Atmospheric Work** is the SAFETY NET (always available last resort)

**Story Causality:**
1. **A-Story** creates B-Consequence (success rewards) and C-stories (journey texture)
2. **B-Consequence** continues A-story narrative with same characters/locations
3. **B-Sought** provides reliable income through player initiative
4. **C-Stories** are the natural consequence of travel, location visits, NPC interactions

**Key Insight:** B-Consequence rewards mastery (skilled players never grind). B-Sought prevents soft-lock (struggling players always have options). They are not competing systems—they serve different players at different times.

### Route Encounters Are C-Stories

When traveling to an A-Story location, the route contains C-story encounters:
- Each route segment presents a situation
- These are C-stories (route scope), not part of the A-Story
- They cost/reward resources, affecting arrival state
- Player must handle them to reach the destination

**The journey to story IS content, not empty travel.**

### Terrain Shapes Route Cost and Encounters

The hex grid uses categorical terrain properties that affect both travel cost and encounter themes.

> **Note:** Values below represent design intent for relative scaling. Actual implementation uses Catalogue pattern (parse-time translation). See code for authoritative values.

**Terrain Stamina Cost (Design Intent):**

| Terrain | Relative Cost | Design Intent |
|---------|--------------|---------------|
| Road | Lowest | Easy travel corridors |
| Plains | Low | Baseline open terrain |
| Water | Low | Easy with boat |
| Forest | Medium | Moderate difficulty |
| Mountains | High | Hard terrain |
| Swamp | High | Hard terrain |

**Terrain Encounter Themes:**

| Terrain | C-Story Themes | Example Encounters |
|---------|----------------|-------------------|
| Forest | Ambush, wildlife, foraging | Bandit toll, wolf pack, berry discovery |
| Road | Trade, law, tolls | Merchant caravan, patrol checkpoint, cart hire |
| Plains | Travelers, weather, exposure | Fellow travelers, storm shelter, lost direction |
| Mountains | Climbing, isolation, altitude | Rockfall hazard, mountain shrine, altitude sickness |
| Swamp | Disease, navigation, creatures | Quicksand, fever symptoms, hostile fauna |
| Water | Weather, piracy, drowning | Storm at sea, pirate encounter, capsized boat |

**Route Choice = Impossible Choice:**

> **Implementation Status:** Multiple route alternatives is design intent. Current HexRouteGenerator finds single optimal path per transport type. Route choice UI and alternative path presentation is future work.

Multiple route options create strategic trade-offs aligned with design pillars:

| Route Type | Time | Stamina | Coins | Encounters |
|------------|------|---------|-------|------------|
| Safe (Roads) | Long | Low | Low/Tolls | Few, lawful |
| Fast (Forest) | Medium | High | Free | Moderate, ambush risk |
| Direct (Mountain) | Short | Very High | Free | Many, harsh conditions |
| Paid (Transport) | Short | Low | High | Varies by service |

Player cannot optimize all dimensions. A tired player (low stamina) must choose slow-safe OR pay coins for transport. A resource-poor player must risk the dangerous shortcut.

**Transport Options:**

| Transport | Coin Cost | Terrain Restrictions | Effect |
|-----------|-----------|---------------------|--------|
| Walking | 0 | None | Baseline |
| Cart | Coins | Penalized in forest | Slower but cargo |
| Horseback | Coins | Limited in forest/mountain | Faster |
| Boat | Coins | Water only | Required for water |

### No Soft-Lock Guarantee

Even with zero resources, player can always progress:

| State | Guaranteed Path |
|-------|-----------------|
| Zero coins | Work action (C-story) available at any location |
| Zero stamina | Rest action restores stamina |
| Negative Resolve | Fallback choices have no Resolve requirement |

The gate is **economic pressure**, not **boolean lockout**. Under-prepared players take longer but are never stuck.

### Why This Works

| Principle | How Travel Gate Honors It |
|-----------|---------------------------|
| **No Soft-Locks** | B/C stories always available; player can always earn travel resources |
| **Impossible Choices** | Route choice: fast/dangerous vs slow/safe vs paid/easy. Cannot optimize all. |
| **Perfect Information** | Terrain, distance, and travel cost visible before route commitment |
| **Earned Scarcity** | Resources earned through B/C engagement, not given freely |
| **Infinite Journey** | Distance scales linearly forever; loop never ends |
| **Terrain Variety** | Different terrains create thematically appropriate C-story encounters |

### Distance Scaling Principle

A-Story scene N spawns at approximately distance N from world origin. Travel cost scales linearly with distance. B/C story rewards also scale with location difficulty (distance from origin), maintaining the preparation-to-reward ratio.

**The principle:** Farther A-story scenes require more resources to reach. More distant locations offer proportionally better B-story rewards. The ratio stays balanced—preparation requirements match available opportunities.

---

## 5.8 Scene Progression

Situations are ordered within scenes. Player experiences them sequentially.

### Flow

1. Scene activates → Situation Instances created from templates
2. Scene starts at first Situation Instance
3. Player completes situation (selects choice)
4. Scene index advances to next Situation Instance
5. Player returns to location view
6. Player navigates to next situation's location
7. Scene resumes when player enters matching location
8. Repeat until final situation complete
9. Scene marked Completed, spawn rewards applied

**Multi-location scenes:** Player must travel between situation locations. Scene waits for player to arrive at correct location before resuming.

---

## 5.9 Situation Presentation Patterns

How situations appear depends on which entities they reference. Three distinct patterns exist.

### Pattern 1: Location-Only (Modal)

When situation has Location but NO NPC:

1. Player enters location matching situation's Location
2. **IMMEDIATE:** Modal appears with situation choices
3. Player MUST select a choice (no escape)
4. After selection: costs/rewards applied, return to location view

**Feel:** Sir Brante style. Enter a place, confronted with decision. Cannot leave until resolved.

### Pattern 2: Location + NPC (Conversation Entry)

When situation has BOTH Location AND NPC:

1. Player enters location matching situation's Location
2. Player uses "Look Around" action
3. NPCs at location displayed
4. For each NPC with active situation: conversation option appears
5. Player clicks conversation option (voluntary entry)
6. Modal appears with situation choices (mandatory decision)
7. After selection: costs/rewards applied, return to location view

**Feel:** You spot someone. You can approach them or not. Once you engage, you must see it through.

### Pattern 3: Route Segments (Travel)

When situation has Route:

1. Player initiates travel on route
2. Route divided into segments (each segment = one situation)
3. At each segment: modal appears with situation choices
4. Player MUST select a choice (no turning back mid-journey)
5. After selection: costs/rewards applied, continue to next segment
6. After final segment: arrive at destination

**Feel:** Journey encounters. The road presents challenges. You handle each as it comes.

### Filter Determines Presentation

| LocationFilter | NpcFilter | RouteFilter | Presentation |
|----------------|-----------|-------------|--------------|
| Set | null | null | Location modal (immediate) |
| Set | Set | null | NPC conversation (Look Around → click → modal) |
| Set | null | Set | Route segment (during travel) |

LocationFilter is always required. NpcFilter and RouteFilter determine presentation mode.

### Mandatory Decision Principle

Once a situation is entered (by any path), player MUST make a choice. No backing out. This creates weight—approaching an NPC or entering a location has consequences.

The four-choice archetype guarantees at least one choice is always available (fallback), so player is never stuck.

---

## 5.10 Choice Patterns by Category

### A-Story: Fallback Required

Every A-Story situation MUST have at least one choice with NO requirements:

| Choice Type | Requirement | Cost | Outcome |
|-------------|-------------|------|---------|
| Stat-gated | PrimaryStat ≥ threshold | None | Best |
| Money-gated | None | Coins | Good |
| Challenge | None | Resolve | Variable |
| **Fallback** | **None** | Time | Worst |

The fallback guarantees forward progress. A-Story validation rejects situations without one.

### B/C Stories: Flexible

B and C stories can have any choice structure:

- All choices can have requirements
- Challenge-only situations allowed
- Failure states permitted
- No mandatory fallback

This enables narrative tension, gating, and consequences that A-Story cannot have.

---

## 5.11 Text Generation Rules

### Two-Phase Creation Model

Entity creation happens in two phases. Names and descriptions generated in Phase 2 persist for the remainder of the game.

| Phase | What Happens | Output |
|-------|--------------|--------|
| **1. Mechanical** | Structure, references, categorical properties | Generic identifiers |
| **2. Narrative** | AI examines complete context | PERSISTENT names stored on entities |

### NO PLACEHOLDERS

Placeholder syntax like `{NPCName}`, `{LocationName}` is FORBIDDEN in templates.

| Approach | When to Use |
|----------|-------------|
| **Generic text** | Works standalone without substitution |
| **Null** | AI generates contextually appropriate text in Phase 2 |

**Rationale:** Phase 2 (narrative finalization) generates all contextual text after mechanical creation completes. Templates contain generic text or null—never placeholders. Generated names persist and display consistently throughout the game.

**See also:** arc42 §8.27 for technical details on two-phase creation.

---

## Cross-References

- **Technical Architecture**: See [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) for Dual-Tier Action system, Catalogue Pattern, and Template vs Instance Lifecycle
- **Core Loop**: See [03_core_loop.md](03_core_loop.md) for how situations fit into the SHORT loop
