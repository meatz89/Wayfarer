# 12. Glossary

Domain and technical terms used throughout this documentation.

---

## Domain Terms

| Term | Definition |
|------|------------|
| **ArchetypeIntensity** | Content categorization: Recovery, Standard, Demanding. Drives difficulty scaling; orthogonal to position-based arc structure. |
| **A-Story** | Infinite main narrative (mandatory). Enum: MainStory. See [gdd/08_glossary.md](../gdd/08_glossary.md) |
| **B-Consequence** | Reward from A-story success (mandatory). Enum: SideStory. See [gdd/08_glossary.md](../gdd/08_glossary.md) |
| **B-Sought** | Player-initiated quests (opt-in). Enum: SideStory. See [gdd/08_glossary.md](../gdd/08_glossary.md) |
| **C-Story** | Journey texture (mandatory). Enum: Encounter. See [gdd/08_glossary.md](../gdd/08_glossary.md) |
| **Story Category** | A/B/C classification. Property matrix in [gdd/08_glossary.md](../gdd/08_glossary.md) |
| **Atmospheric Action** | Persistent gameplay scaffolding (Travel, Work, Rest) that prevents soft-locks; always available |
| **Bridge** | The ActionType mechanism crossing from strategic to tactical layer via StartChallenge |
| **Categorical Property** | Strongly-typed enum describing entity identity or capability; parsed from JSON strings with fail-fast validation |
| **Challenge** | Tactical card-based gameplay session (Social, Mental, or Physical) |
| **Choice** | Player-facing option within a Situation; has costs, requirements, and rewards |
| **Connection State** | NPC relationship level (Disconnected → Guarded → Neutral → Receptive → Trusting) |
| **Fallback Choice** | Zero-requirement option ensuring forward progress; prevents soft-locks |
| **Four-Choice Pattern** | Standard situation structure: stat-gated, money-gated, challenge, fallback |
| **Net Challenge** | Query-time scaling factor: LocationDifficulty - (PlayerStrength / 5), clamped to [-3, +3]. Negative = player overpowered, positive = underpowered. Applied to ALL stat requirements via ApplyStatAdjustment(), stacked with NPC demeanor. Creates RPG-like quest level scaling based on hex distance from world center. |
| **Impossible Choice** | Design goal: player must choose between multiple suboptimal paths, revealing character through constraint |
| **Obligation** | Quest definition triggering scene spawning; drives narrative progression |
| **Perfect Information** | Strategic layer principle: all costs/rewards visible before commitment |
| **Building → Crisis Structure** | A-Story arc model (Sir Brante pattern): positions 1 to N-1 are Building (stat grants), position N is Crisis (stat-gated test). Position determines structure. |
| **Challenge → Payoff Structure** | B-Story arc model: positions 1 to N-1 are Challenge (resource costs, skill tests), position N is Payoff (guaranteed resource reward). Resource acquisition cycle. |
| **Scene** | Mutable instance created from SceneTemplate at spawn-time; contains Situation instances (created at activation-time); lifecycle: Deferred → Active → Completed |
| **SceneTemplate** | Immutable archetype containing SituationTemplates; created at parse-time; JSON uses `sceneTemplates` key, NEVER `scenes` |
| **Situation** | Mutable instance created from SituationTemplate at activation-time; contains resolved entity references (Location, NPC, Route) |
| **SituationTemplate** | Immutable archetype embedded in SceneTemplate; contains PlacementFilters (categorical); created at parse-time |
| **SituationCard** | Tactical victory condition defining threshold and rewards |
| **Soft-Lock** | Game state with no forward progress; TIER 1 violation, never acceptable |
| **Venue** | Top-level location cluster containing multiple Locations |

---

## Technical Terms

| Term | Definition |
|------|------------|
| **Catalogue** | Static class translating categorical properties to concrete values at parse-time |
| **ChoiceTemplate** | Template defining choice requirements and consequence reference; used by scene-based actions |
| **CompoundRequirement** | Unified class for ALL resource prerequisites; uses OR-path logic; HIGHLANDER: only class for availability checks |
| **Consequence** | Unified ValueObject for ALL costs and rewards; negative values = costs, positive = rewards; HIGHLANDER: only class for resource outcomes |
| **DTO** | Data Transfer Object; intermediate structure between JSON and domain entity |
| **Dual-Tier Actions** | Union type pattern: LocationAction supports atmospheric (parse-time Consequence) OR scene-based (query-time Consequence via ChoiceTemplate); both use Consequence class, differ in creation timing |
| **EntityResolver** | Pattern for find-or-create queries using categorical filters |
| **Facade** | Stateless service encapsulating business logic for one domain area |
| **GameWorld** | Single source of truth; state container with zero external dependencies |
| **HIGHLANDER** | "There can be only one" — single canonical storage for each piece of state |
| **Hex Position** | AxialCoordinates (Q, R) defining spatial location on hex grid |
| **Hybrid Responsibility Pattern** | ValueObject provides pure query methods (HasAnyEffect, GetProjectedState); Service handles mutations |
| **InstantiationState** | Scene/Situation lifecycle: Deferred → Instantiated |
| **LocationActionCatalog** | Generator creating atmospheric actions for all locations at parse-time |
| **Object Reference** | Direct entity relationship (not ID string); e.g., `NPC.Location` |
| **Parse-Time Translation** | One-time categorical → concrete conversion at startup |
| **Pattern Discrimination** | Runtime check (`ChoiceTemplate == null`) determining which action tier applies |
| **PlacementFilter** | Strongly-typed filter for entity resolution; combines identity dimensions (Privacy, Safety, Activity, Purpose) and capabilities |
| **PlacementProximity** | Enum controlling spatial resolution strategy; includes categorical search (SameVenue, SameDistrict) and special cases (RouteDestination) |
| **PlayerStateProjection** | Read-only projection of player state after applying a Consequence; used for Perfect Information display |
| **Query-Time** | Third timing tier; actions created when player enters context |
| **RequirementProjection** | Projection of requirement satisfaction status showing which OR-paths are satisfied |
| **RouteDestination** | Special PlacementProximity value (=6) that places a situation at the destination location of a route resolved earlier in the same scene; enables arrival situations to materialize at journey endpoints |
| **Scene-Based Action** | Ephemeral action from ChoiceTemplate; exists only when scene active |
| **Spawn-Time** | Second timing tier; scenes/situations created when triggered |
| **Template** | Immutable archetype (SceneTemplate, ChoiceTemplate); first timing tier |
| **Template ID** | Identifier for immutable archetypes; allowed (unlike entity instance IDs) |
| **Union Type** | Entity supporting multiple patterns via discriminator property (e.g., LocationAction) |

---

## Architectural Terms

| Term | Definition |
|------|------------|
| **Blackbox** | Building block described only by inputs/outputs; internal structure hidden |
| **Clean Architecture** | Dependencies flow inward: UI → Facades → GameWorld |
| **Fail-Fast** | Errors surface immediately at point of failure; no defensive hiding |
| **Idempotent** | Safe to execute multiple times; required for Blazor double-render |
| **Layer Separation** | Strategic (perfect info) vs Tactical (hidden complexity) |
| **Ownership** | Entity contains another; deleting owner deletes owned (Scene → Situation) |
| **Placement** | Entity placed at location; location doesn't own entity (Scene at Location) |
| **Reference** | Entity refers to another; neither owns the other (NPC → Location) |
| **Single Source of Truth** | Each state has exactly one canonical storage location |
| **Stateless Service** | Facade containing logic but no state; operates on GameWorld |
| **Four-Tier Timing** | Parse-time (SceneTemplates) → Spawn-time (Scene instances, Deferred, empty Situations) → Activation-time (Situation instances, resolved entities) → Query-time (ephemeral actions). NO Scene instances at parse-time. See §8.4 |
| **Whitebox** | Internal decomposition of a higher-level blackbox |

---

## Challenge System Terms

| Term | Definition |
|------|------------|
| **Social Challenge** | Conversation with NPC; Momentum/Initiative/Doubt resources |
| **Mental Challenge** | Investigation/puzzle; Progress/Attention/Exposure resources |
| **Physical Challenge** | Obstacle/combat; Breakthrough/Exertion/Danger resources |
| **Momentum** | Builder resource in Social challenges; accumulates toward threshold |
| **Progress** | Builder resource in Mental challenges |
| **Breakthrough** | Builder resource in Physical challenges |
| **Threshold** | Victory condition value; reaching it completes SituationCard |

---

## Categorical Property Terms

| Term | Definition |
|------|------------|
| **Identity Dimension** | Categorical property describing what an entity IS (atmosphere, context); uses List matching (any-of) |
| **Capability** | Categorical property describing what an entity CAN DO (game mechanics); uses Flags enum (all-of) |
| **LocationPrivacy** | Identity dimension: Public, SemiPublic, Private — determines witness presence and social stakes |
| **LocationSafety** | Identity dimension: Dangerous, Neutral, Safe — determines physical threat level |
| **LocationActivity** | Identity dimension: Quiet, Moderate, Busy — determines population density |
| **LocationPurpose** | Identity dimension: Transit, Dwelling, Commerce, Civic, Defense, Governance, Worship, Learning, Entertainment, Generic |
| **LocationCapability** | Flags enum: Crossroads, Commercial, SleepingSpace, Restful, Indoor, Outdoor, Market, etc. — enables game mechanics |
| **LocationRole** | Identity dimension: Generic, Hub, Connective, Landmark, Hazard, Rest — functional/narrative role of a Location |
| **VenueType** | Identity dimension for Venues (NOT Locations): Inn, Tavern, Shop, Guild, Temple, Market — establishment type |
| **TerrainType** | Identity dimension: Forest, Wilderness, Mountain, Swamp, Beach, River, Lake, Cave — natural geography |
| **SettlementScale** | Identity dimension: Farm, Mine, Outpost, Village, Town, City, Port — population center size |
| **StructureType** | Identity dimension: Road, Bridge, Crossroads, Tower, Castle, Ruin, Crypt — built/constructed features |
| **Professions** | NPC identity dimension: Innkeeper, Merchant, Guard, Scholar, etc. — occupational role |
| **PersonalityType** | NPC identity dimension: behavioral archetype affecting dialogue and costs |
| **NPCSocialStanding** | NPC identity dimension: influence tier (Notable, Authority) |
| **NPCStoryRole** | NPC identity dimension: narrative function (Obstacle, Facilitator) |
| **Orthogonal Dimension** | Independent categorical axis; multiple dimensions compose without interdependence |

---

## Abbreviations

| Abbreviation | Expansion |
|--------------|-----------|
| **ADR** | Architecture Decision Record |
| **DI** | Dependency Injection |
| **DTO** | Data Transfer Object |
| **NPC** | Non-Player Character |
| **UI** | User Interface |
