# 8. Essential Glossary

## Why This Document Exists

This glossary defines essential game design terms for quick reference.

---

## Core Concepts

### A-Story
The infinite main narrative spine. Phase 1 (A1-A10) is authored tutorial. Phase 2 (A11+) is procedurally generated continuation that never ends.

### Archetype
Reusable mechanical pattern defining situation structure (choice count, path types, cost formulas) independent of narrative content. Same archetype + different entity properties = infinite variations.

### Atmospheric Action Layer
Always-available core actions (Travel, Work, Rest, Move) forming persistent gameplay baseline. Scene-based actions layer on top, never replacing atmospheric scaffolding.

### Build
Player's character specialization emerging from stat allocation choices. Not a pre-selected class—identity emerges from resource investment decisions.

### Four-Choice Archetype
The pattern guaranteeing every A-story situation offers four path types: stat-gated (free for specialists), resource (costs coins/items), challenge (skill test), and fallback (always available).

---

## Gameplay Terms

### Impossible Choice
Core design principle. Every decision forces trade-offs between multiple valid alternatives. No optimal path exists—only the path you choose.

### Mental Math Design
Design principle (DDR-007). All numeric values small enough for players to calculate in their heads without external tools. Values like +2 or -5, not +147 or -2,340.

### Deterministic Arithmetic
Design principle (DDR-007). Strategic outcomes are predictable from inputs with no randomness. "Insight 5 required" means exactly that—not "70% chance with Insight 5."

### Absolute Modifier
Numeric adjustment that stacks additively. Two +3 bonuses always equal +6. Contrast with multipliers which compound unexpectedly. Part of Intentional Numeric Design (DDR-007).

### Perfect Information
Design principle. All costs, requirements, and rewards visible before selection. Strategic layer has no hidden gotchas. Extended by DDR-007: information must be not just visible but mentally calculable.

### Requirement Inversion
Foundational principle: stat requirements affect COST, not ACCESS. Unlike traditional RPGs with boolean gates ("Level 5 required"), Wayfarer uses resource arithmetic—everyone can progress, but high-stat players pay less. This ensures no soft-locks while rewarding specialization.

### Scene-Situation-Choice Flow
Narrative structure. Scene (container) → Situation (decision point) → Choice (four paths). All content follows this hierarchy.

### Strategic Layer
The WHAT and WHERE layer. Player sees all information. Decides WHETHER to attempt. Entities are persistent.

### Tactical Layer
The HOW layer. Card-based challenges. Hidden complexity (draw order). Tests execution skill. Sessions are temporary.

---

## Resources

### Universal Resources
Resources competing across all systems: Time (blocks per day), Coins (currency), Focus (Mental pool), Stamina (Physical pool), Resolve (Willpower gate—see below), Health (survival threshold).

### Resolve (Sir Brante Willpower Pattern)
The willpower resource governing meaningful story choices. Unlike other resources:
- **Starts at 0** (must earn before spending)
- **Can go negative** (minimum -10, locks out costly choices)
- **Large swings** (+5/+10 gains, -5/-10 costs)
- **Dual nature** every costly choice requires Resolve >= 0 AND costs Resolve

This creates the "willpower gate"—players cannot make difficult choices until they've built resolve through earlier positive choices. See [04_systems.md §4.1](04_systems.md#resolve-the-willpower-gate-sir-brante-pattern) for full design rationale.

### Tactical Resources
Resources existing only within challenge sessions: Builder resources (Momentum/Progress/Breakthrough), Session resources (Initiative/Attention/Exertion), Threshold resources (Doubt/Exposure/Danger).

---

## The Five Stats

| Stat | Domain | Governs |
|------|--------|---------|
| **Insight** | Mental | Investigation, observation, puzzle-solving |
| **Cunning** | Mental | Deception, misdirection, reading situations |
| **Rapport** | Social | Building trust, friendly persuasion |
| **Diplomacy** | Social | Formal negotiation, authority appeal |
| **Authority** | Physical | Intimidation, command presence |

---

## Challenge Systems

### Mental Challenge
Card-based investigation. Build Progress through Leads and Details. Session resource: Attention. Threshold: Exposure.

### Physical Challenge
Card-based obstacle resolution. Overcome through Exertion. Session resource: Exertion. Threshold: Danger.

### Social Challenge
Card-based persuasion. Build Momentum through dialogue. Session resource: Initiative. Threshold: Doubt.

---

## Story Categories

A-Story, B-Story, and C-Story are distinguished by a **combination of properties**, not a single axis. All three use identical Scene-Situation-Choice structure; category determines rules and validation.

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
| **Spawn Trigger** | Previous A-scene completes | A-story choice success | Player acceptance | Natural emergence from journey |
| **Declinable** | No | No | Yes | No |
| **Purpose** | Main narrative | Depth + reward mastery | Income + anti-soft-lock | World texture |

> **Note:** B-Consequence and B-Sought both use the `SideStory` enum value. They differ in spawn trigger and player agency, not in technical category.

### A-Story
The infinite main narrative spine. Scenes chain sequentially (A1 → A2 → A3...). Every situation requires a fallback choice guaranteeing forward progress. Primary purpose is world expansion—creating new venues, districts, regions, routes, and NPCs. Can never fail. **Player cannot decline**—when an A-scene activates, engagement is mandatory. Phase 1 (A1-A10) is authored tutorial; Phase 2 (A11+) is procedurally generated.

**Building → Checking Rhythm (Sir Brante Pattern):**
A-stories alternate between two phases:
- **Building Phase** — Situations that grow player stats through choices. Investment decisions accumulate.
- **Checking Phase** — Hard stat checks that test the player's accumulated investment. Success unlocks rewards.

When player **succeeds** at a hard stat check, a B-story thread spawns as reward—continuing the narrative with the same characters and locations, deepening the story the A-story established.

### B-Story

B-Stories provide resources that fund A-story travel. They come in **two distinct types** serving different purposes:

#### B-Consequence (Earned Reward)

**Reward threads** spawned when player succeeds at hard A-story stat checks. These are the Sir Brante pattern—certain scenes only unlock because the player made specific choices that had specific requirements.

**Characteristics:**
- **Spawn Trigger:** A-story choice success (stat check met, challenge won)
- **Cannot be declined:** Just happens as consequence of success
- **Continues A-story narrative:** Same NPCs, same locations, deeper story
- **Premium rewards:** Major resources for demonstrated mastery
- **One-time per trigger:** Each A-story success can spawn its B-consequence once

**Why This Works:**
- Rewards stat investment (building has purpose)
- Creates narrative continuity (A-story characters return)
- Skilled players earn resources automatically
- Feels earned, not randomly assigned

#### B-Sought (Player-Initiated)

**Quest content** the player actively seeks out when they need resources. Found through exploration, NPC conversations, job boards, and world discovery.

**Characteristics:**
- **Spawn Trigger:** Player acceptance (accepts quest, takes contract, agrees to help)
- **Can be declined:** Player chooses whether to engage
- **Independent narrative:** New characters, new situations
- **Basic rewards:** Reliable income for effort invested
- **System-repeatable:** Job boards always have work available

**Sources:**
- **Job Boards:** Always-available contracts (delivery, investigation, escort)
- **NPC Offers:** Characters met during A/B stories offer work
- **Exploration:** Discovering locations reveals opportunities
- **Obligations:** Accepted contracts tracked as active quests

**Why This Exists:**
- Prevents soft-lock (player can always earn resources)
- Rewards exploration (finding work is itself gameplay)
- Player controls pacing (seek work when needed)
- Traditional RPG quest familiarity

#### The Two Types Together

| Aspect | B-Consequence | B-Sought |
|--------|---------------|----------|
| **Player Experience** | "My success unlocked this" | "I need coins, let me find work" |
| **Skill Reward** | Mastery → automatic rewards | Effort → earned income |
| **Narrative Role** | Deepens A-story threads | Expands world knowledge |
| **Economic Role** | Premium income for skilled | Safety net for all |

**Design Intent:** Skilled players who invest in stats and succeed at checks receive B-Consequence scenes automatically—they never need to grind. Players who struggle can always find B-Sought content through job boards. Both paths lead to A-story progression; mastery is rewarded but never required.

### C-Story
Single-scene encounters providing **world texture**. These are not spawned by explicit game mechanics—they **emerge naturally from the journey** that A and B stories create.

**Natural Emergence:**
- **Travel routes** — Moving between A-story locations creates route encounters (terrain-themed)
- **Location visits** — Being in a place creates opportunity for location flavor
- **NPC interactions** — Meeting characters through A/B stories creates incidental moments

**Player cannot decline or willingly spawn**—these are surprises that flesh out the world. The journey to story IS content. A and B stories create the context; C-stories fill the texture naturally.

**Examples:** Forest ambush on route to distant A-story, inn gossip while resting, merchant haggling at market visited during B-story.

**Key Insight:** C-stories are not a separate system. They are the natural consequence of A/B story journeys—terrain, locations, and NPCs responding to player presence.

### Why These Categories?

The categories form an interconnected narrative and economic ecosystem:

| Category | Player Experience | Causal Relationship |
|----------|------------------|---------------------|
| **A-Story** | "The main quest continues" | Creates B-Consequence (success) and C-stories (journey) |
| **B-Consequence** | "My success unlocked this" | Continues A-story with same NPCs/locations |
| **B-Sought** | "I need resources, let me find work" | Available through exploration and job boards |
| **C-Story** | "The journey itself" | Emerges from travel, locations, NPCs |

**The story causality:**
```
A-Story (Building → Checking)
    │
    ├── SUCCESS at check → B-Consequence spawns (mandatory)
    │                      └── Same NPCs, deeper story, premium rewards
    │
    ├── JOURNEY creates → C-Stories emerge naturally
    │                     └── Route, location, NPC texture
    │
    └── RESOURCE NEED → Player seeks B-Sought (optional)
                        └── Job boards, NPC offers, exploration
```

**Economic Design:**
- **Mastery Path:** Invest stats → succeed at checks → B-Consequence rewards fund travel automatically
- **Fallback Path:** Use fallbacks → need resources → seek B-Sought work → earn travel funds
- **Both paths work:** Skilled players never grind; struggling players never soft-lock

**Key Insight:** B-Consequence rewards mastery (making grinding unnecessary for skilled players). B-Sought prevents soft-lock (ensuring resources are always obtainable). Together they create fair progression where skill is rewarded but never required.

---

## Content Terms

### Categorical Property
Strongly-typed entity attribute with intentional domain meaning. Two types exist:
- **Identity Dimensions** (what entity IS): Privacy, Safety, Activity, Purpose for locations; Profession, Personality for NPCs
- **Capabilities** (what entity CAN DO): Crossroads enables Travel; Commercial enables Work; SleepingSpace enables Rest

All categorical properties are enums with specific game effects—never generic strings.

### Explicit Property Principle
Architecture pattern requiring explicit strongly-typed properties instead of string-based generic routing. `InsightRequired` (explicit int property) catches typos at compile-time; `Type="Insight"` (string routing) fails silently at runtime. Applies to requirements, state modifications, and entity relationships. See [arc42/08_crosscutting_concepts.md](../arc42/08_crosscutting_concepts.md) §8.19 for technical details.

### Frieren Principle
Design philosophy: The game never ends. The journey is the point, not arrival. Success measured by journey quality, not reaching destination.

---

## Location Properties

Locations have two types of categorical properties that determine scene activation and gameplay.

### Identity Dimensions (What Location IS)

| Dimension | Values | Game Effect |
|-----------|--------|-------------|
| **Privacy** | Public, SemiPublic, Private | Witness presence, social stakes |
| **Safety** | Dangerous, Neutral, Safe | Threat level, guard presence |
| **Activity** | Quiet, Moderate, Busy | Population density, NPC availability |
| **Purpose** | Transit, Dwelling, Commerce, Civic, etc. | Primary functional role |

### Capabilities (What Location CAN DO)

| Capability | Enables |
|------------|---------|
| **Crossroads** | Travel action (route selection) |
| **Commercial** | Work action (earn coins) |
| **SleepingSpace** | Rest action (restore health/stamina) |
| **Restful** | Enhanced restoration quality |
| **Market** | Trading with pricing modifiers |

Scene activation uses these properties for categorical matching—scenes activate at locations matching their filter criteria.

---

## Cross-References

- **Technical Terms**: See [arc42/12_glossary.md](../arc42/12_glossary.md) for implementation terminology
