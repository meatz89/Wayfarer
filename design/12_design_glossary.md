# Design Section 12: Game Design Glossary

## 12.1 Purpose

This glossary provides canonical definitions for game design terms used across Wayfarer documentation. When terminology conflicts arise, this glossary is authoritative for **game design concepts**. For technical implementation terms, see arc42 section 12_glossary.md.

**Scope:** Player-facing concepts, gameplay mechanics, design patterns, and narrative structures. Excludes: Technical architecture, code patterns, implementation details.

**Last Updated:** 2025-11

---

## 12.2 Core Gameplay Terms

### A-Story

**Definition:** The primary narrative progression thread providing infinite structure without resolution.

The main storyline consisting of sequential tutorial scenes (A1-A10, currently A1-A3 implemented) followed by procedurally generated continuation (A11 → ∞). Unlike traditional "main quest" with ending, A-story never resolves - player travels eternally, arriving at new places, meeting new people, each place suggesting somewhere else worth visiting. The journey itself is the point, not reaching any destination.

**Example:** A1 teaches lodging negotiation at starting inn. A2 introduces route travel. A3 establishes pursuit framework. A11+ generates infinite arrival-meeting-departure cycles across escalating tiers (local → regional → continental → cosmic).

**Distinguishing from:** B-Story (major side content), C-Story (minor side content), Main Quest (traditional term implying ending). A-story is infinite main thread, not finite quest chain.

**Cross-reference:** DDR-001 (Infinite A-Story design decision), Section 03 (narrative structure), Frieren Principle

---

### B-Story

**Definition:** Major side storylines providing depth and player choice alongside infinite A-story.

Substantial narrative threads parallel to A-story with multiple scenes, character arcs, and meaningful rewards. Unlike A-story (mandatory for progression), B-stories are optional pursuits offering specialized content, deeper character relationships, faction involvement, or thematic exploration. Typically 3-8 scenes per B-story, authored content with some procedural elements.

**Example:** "The Missing Merchants" B-story investigating trade route disappearances across 5 scenes. "Elena's Past" B-story building relationship with inn owner through conversation scenes. Both run parallel to A-story pursuit, player chooses when/if to engage.

**Distinguishing from:** A-Story (infinite mandatory spine), C-Story (minor content). B-stories are substantial optional content with multi-scene arcs.

**Cross-reference:** Section 03 (narrative structure), Tag-Based Dependencies

---

### Build

**Definition:** Player's character specialization emerging from stat allocation and playstyle choices.

The accumulation of player decisions regarding stat advancement (which of five stats to prioritize), resulting in specialized capability profile. Not a pre-selected class but emergent identity from resource allocation. Different builds excel at different challenge types and create distinct play experiences. Limited total stat points force specialization - cannot maximize all stats.

**Example:** "Investigator" build (Insight 4, Cunning 3, others 1-2) dominates Mental challenges, weak in Social. "Diplomat" build (Rapport 4, Diplomacy 3, others 1-2) dominates Social, weak in Mental. "Generalist" build (all stats 2-3) handles everything moderately, dominates nothing.

**Distinguishing from:** Class (pre-selected template), Loadout (equipment configuration). Build is emergent from stat choices, not predetermined category.

**Cross-reference:** DDR-003 (Unified Stat System), Specialization, Five Stats

---

### C-Story

**Definition:** Minor side content providing world flavor and optional rewards.

Small narrative moments typically contained in single scene or simple 2-situation sequence. Less substantial than B-stories, often opportunistic encounters or small favors. Provides world texture, relationship moments, resource opportunities without major narrative investment. Can be procedurally generated more easily than A/B content due to smaller scope.

**Example:** "Help merchant fix wagon wheel" (single scene). "Deliver message to NPC across town" (2 situations). Minor content appearing organically during travel or exploration, completed quickly.

**Distinguishing from:** A-Story (infinite spine), B-Story (major side content). C-stories are small optional moments.

**Cross-reference:** Section 03 (narrative structure)

---

### Calendar Day

**Definition:** 24-hour game time period containing time segments, bounded by sleep cycle.

The fundamental temporal unit structuring daily rhythm. Each calendar day begins with waking (full time segments available), proceeds through activities consuming segments, and ends with sleep advancing to next day. Player cannot extend day beyond segment capacity - when segments exhausted, must sleep or leave activities incomplete. Day counter tracks progression, affects scene expiration, NPC schedules, world state.

**Example:** Day 1: Wake at Market Square (6 segments available). Accept delivery (4 segments consumed). Spend remaining 2 segments on food/lodging. Sleep advances to Day 2. Some scenes expire if not completed by Day X.

**Distinguishing from:** Time Block (subdivision of day), Real-time (player time). Calendar day is game-world temporal unit.

**Cross-reference:** Time Block, Daily Rhythm, Time Segment

---

### Delivery Loop

**Definition:** Core gameplay cycle of accepting jobs, traveling routes, earning coins, spending on survival.

The fundamental economic and pacing loop structuring moment-to-moment play. Player accepts delivery job (package from Location A to Location B), travels route encountering situations, reaches destination earning coins (15-25 typically), spends on mandatory survival (food/lodging 12-18 coins), nets small profit (3-7 coins). Tight margins create pressure - cannot afford everything, must optimize to accumulate savings.

**Example:** Accept job at Market Square (deliver to Port District, 20 coins reward, 4 route segments). Travel route (encounter 4 situations, spend resources). Arrive at Port, collect 20 coins. Purchase food (8 coins) and lodging (7 coins). Net profit: 5 coins toward equipment upgrade (needs 50 total).

**Distinguishing from:** Quest Loop (narrative completion), Grind Loop (repetitive farming). Delivery loop is economic pressure cycle driving strategic decisions.

**Cross-reference:** DDR-008 (Delivery Courier design), DDR-004 (Tight Economic Margins), Core Game Loop

---

### Economic Pressure

**Definition:** Tight resource margins forcing prioritization and optimization.

The design pattern where delivery earnings barely cover survival costs (3-7 coins profit per day), creating strategic tension. Player cannot afford everything - must choose between equipment upgrades, emergency funds, relationship investments, service purchases. Optimization skill determines prosperity. Every coin spent is coin unavailable elsewhere. Drives impossible choices - all options valid, insufficient resources to pursue all.

**Example:** Saved 40 coins toward 50-coin equipment upgrade. Emergency: NPC needs help, costs 15 coins. Accept and delay upgrade? Refuse and damage relationship? Or take risky challenge path hoping for reward? All valid, all have costs.

**Distinguishing from:** Resource Scarcity (general principle), Difficulty (challenge hardness). Economic pressure specifically refers to tight coin margins driving decisions.

**Cross-reference:** DDR-004 (Tight Economic Margins), Impossible Choice, Resource Arithmetic

---

### Five Stats

**Definition:** Unified character progression system (Insight/Rapport/Authority/Diplomacy/Cunning) spanning all challenge types.

The core player capabilities determining card access, choice availability, and build identity. Every card in every challenge type binds to one of five stats - playing cards grants XP to bound stat, advancing stat unlocks deeper cards. Same stats manifest differently per challenge: Insight as pattern recognition (Mental) or reading motivations (Social), Authority as decisive conclusions (Mental) or commanding conversation (Social). Limited total XP forces specialization - cannot max all five.

**Example:** Insight stat (1-8 scale, typical progression). Insight 2 player accesses Depth-2 Mental cards (basic observation), locked out of Depth-4 cards (advanced deduction). Playing Mental cards grants Insight XP. Reaching Insight 3 unlocks new card tier. Same Insight stat also affects Social challenges (reading between lines). Stats begin at 1 and are extendable beyond 8 for long-term play.

**Distinguishing from:** Class Stats (different per class), Separate Stats (different per system). Five stats are unified across all systems.

**Cross-reference:** DDR-003 (Unified Stat System), Build, Specialization, Stat Manifestation

---

### Guaranteed Progression

**Definition:** Design requirement ensuring forward progress from every game state, preventing soft-locks.

The non-negotiable principle (TIER 1) that every A-story situation must have at least one choice with zero requirements, cannot fail, and advances progression. Player cannot become stuck - worst case is slow/poor outcome but always moves forward. Critical for infinite game where player cannot "restart" 50 hours deep. Implemented via Four-Choice Archetype's fallback path.

**Example:** Situation requires negotiation. Choice 1: Rapport 3 (player has 2, disabled). Choice 2: 15 coins (player has 3, disabled). Choice 3: Social challenge (costs Resolve, player exhausted). Choice 4: "Wait patiently for them to be ready" (no requirements, always available, minimal reward, advances progression).

**Distinguishing from:** Easy Mode (difficulty reduction), Safety Net (recoverable failure). Guaranteed progression means always forward path, not easier path.

**Cross-reference:** DDR-007 (Four-Choice Pattern), No Soft-Lock, Fallback Path, TIER 1 Principles

---

### Impossible Choice

**Definition:** Decision presenting multiple valid options with insufficient resources to pursue all, forcing prioritization.

The core player experience emerging from resource scarcity. All choices are viable approaches with genuine benefits, but player lacks resources to afford multiple simultaneously. Must choose which cost to accept - not "which is correct" but "which sacrifice am I willing to make." No optimal solution, only different trade-offs revealing player values and creating emergent narrative through constraints.

**Example:** Situation offers 4 choices. Stat-gated (best outcome, free but requires Rapport 3 - player has 2). Money-gated (good outcome, costs 15 coins - player has 8). Challenge (variable outcome, costs Resolve - player exhausted). Fallback (poor outcome, free). All valid, insufficient resources for best paths. Which cost acceptable?

**Distinguishing from:** Hard Choice (high difficulty), Moral Dilemma (ethical conflict). Impossible choice is resource-based constraint forcing prioritization.

**Cross-reference:** Resource Arithmetic, Economic Pressure, Orthogonal Resource Costs

---

### Infinite Journey

**Definition:** Narrative philosophy treating travel as eternal state without destination.

The conceptual foundation of A-story design. Player is perpetual traveler - no final destination, no ultimate goal, no winning condition. Each arrival leads to departure suggesting next place. Journey itself is content, not obstacle between content. Matches "eternal wanderer" fantasy where meaning comes from experiences along way, not reaching endpoint. Enables infinite content by removing pressure for resolution.

**Example:** Complete A-story scene at City. Final choice: NPC mentions another city worth visiting. Player departs, travels route, arrives at new city, meets new people, situations arise, cycle continues. No pressure to "finish" - can pursue side content for weeks before continuing A-story. Journey never ends because that's the point.

**Distinguishing from:** Open World (explorable space), Endgame Loop (post-ending content). Infinite journey is philosophical framework, not feature.

**Cross-reference:** DDR-001 (Infinite A-Story), Frieren Principle, A-Story

---

### Orthogonal Resource Costs

**Definition:** Design pattern where different choices consume different resource types, preventing dominance.

The balancing principle ensuring no choice strictly dominates others. Each choice path costs different resource category - stat-gated uses permanent character build, money-gated consumes currency, challenge-gated spends time/session resources, fallback accepts poor outcome. Since resources are orthogonal (spending coins doesn't affect stats, using Resolve doesn't cost money), player chooses based on availability and priorities, not objective superiority.

**Example:** Four choices with orthogonal costs. Choice A: Rapport 3 (stat, irreversible allocation). Choice B: 15 coins (currency, replenishable but scarce). Choice C: Social challenge (Resolve + time, opportunity cost). Choice D: Poor outcome (narrative cost, suboptimal result). No universal best - depends on player's current resources and build.

**Distinguishing from:** Balanced Costs (same magnitude different resources), Varied Costs (different amounts same resource). Orthogonal costs use fundamentally different resource categories.

**Cross-reference:** DDR-007 (Four-Choice Pattern), Impossible Choice, Resource Arithmetic, Design Principle 11

---

### Perfect Information

**Definition:** Player visibility of all strategic costs, requirements, and rewards before commitment.

The design principle (TIER 2) ensuring players make informed decisions at strategic layer. Before selecting choice or entering challenge, player sees exact numeric requirements (Rapport 3, not "high Rapport"), exact costs (15 coins, not "expensive"), exact rewards (Health +20, not "restoration"). Can calculate precisely whether affordable and whether worthwhile. No mystery gates - transparency enables planning and strategic optimization.

**Example:** Choice displays "Requires: Rapport 3, Your Rapport: 2, Gap: 1 point. Costs: 5 coins. Rewards: Elena relationship +1, Room unlocked." Player sees exact requirements, exact gap, exact costs, exact rewards. Can plan: "Need 1 more Rapport to access this option" or "Have 8 coins, can afford 5-coin cost."

**Distinguishing from:** Tutorial (teaching), Full Information (includes tactical layer). Perfect information is strategic transparency only.

**Cross-reference:** DDR-005 (Strategic-Tactical Separation), Strategic Layer, Resource Arithmetic, TIER 2 Principles

---

### Resource Arithmetic

**Definition:** Numeric comparison of player resources against action requirements instead of boolean flag checks.

The fundamental mechanical pattern replacing boolean gates with arithmetic. Actions require numeric thresholds (Insight >= 3, Coins >= 15), player has numeric values (Insight 2, Coins 8), system compares mathematically. Player sees exact gaps ("need 1 more Insight"), enabling planning. No hidden flags or mystery conditions - pure arithmetic transparency. Enables perfect information, supports specialization, creates resource competition.

**Example:** Card requires Initiative 15. Player Initiative 12. System: 12 < 15 → disabled, show "Requires 15 Initiative, you have 12, need 3 more." Player can calculate: "If I play these two Foundation cards (+4 Initiative each), I'll have 20, can afford this card."

**Distinguishing from:** Boolean Gates (has/hasn't flags), Level Gating (arbitrary thresholds). Resource arithmetic is transparent numeric comparison.

**Cross-reference:** Requirement Inversion Principle, Perfect Information, DDR-002 (Tag-Based vs Boolean), Resource Scarcity

---

### Resource Flow

**Definition:** The pattern of resource acquisition, expenditure, and competition structuring strategic decisions.

The economic model where resources enter system (delivery earnings, challenge rewards, scene completion), deplete through use (costs, survival spending, challenge resources), and compete for allocation (coins toward equipment OR emergency OR services). Flow creates pressure - cannot accumulate indefinitely, must spend to survive, spending closes other options. Tight flow (barely enough) creates strategic depth. Generous flow (excess) removes tension.

**Example:** Daily flow: Wake with 0 coins. Complete delivery (+20 coins). Purchase food (-8 coins). Rent lodging (-7 coins). Net: 5 coins. Want equipment upgrade (50 coins) but also emergency fund (20 coins). Flow barely covers survival, accumulation requires many days, creates prioritization pressure.

**Distinguishing from:** Resource Pool (static amount), Economy (broader system). Resource flow is acquisition-spending-competition cycle.

**Cross-reference:** Economic Pressure, Resource Arithmetic, Tight Economic Margins

---

### Route Opacity

**Definition:** Limited player knowledge of route segment count and content before first travel, creating risk assessment challenge.

The uncertainty mechanic where unknown routes don't reveal exact segment count or encounter types before departure. Player knows destination, reward, approximate danger rating, but not precise challenges. First travel reveals route structure (fixed segments flip face-up, become learnable), but initial attempt requires estimating resource sufficiency with incomplete information. Risk assessment skill matters - commit with marginal resources hoping route is short? Or prepare extensively risking over-investment?

**Example:** New route "Market Square to Port District" shows: Reward 20 coins, Danger Medium, Distance Unknown. Player has Health 40/100, Stamina 30/60, 5 coins. Attempt route hoping it's 3-4 segments (affordable)? Or rest/prepare spending resources (safe but expensive)? First travel reveals 5 segments - correct assessment mattered.

**Distinguishing from:** Fog of War (visibility), Random Content (unpredictability). Route opacity is information asymmetry driving risk assessment.

**Cross-reference:** Route Learning, Risk Assessment, Core Game Loop

---

### Route Learning

**Definition:** Process of discovering and memorizing fixed route segments through repeated travel, enabling optimization.

The mastery mechanic where unknown route segments are face-down cards revealing on first encounter. Fixed environmental segments (terrain challenges, tollgates, obstacles) remain consistent on repeat travel - once revealed, player knows exact costs and can optimize resource allocation. Event segments remain face-down (random each time), cannot be learned. Mastering routes (learning all fixed segments) rewards repeated play of same paths, supporting courier profession fantasy.

**Example:** First travel: 5 face-down segments, unknown costs, must estimate. Segment 1 flips: "Steep Hill" (3 Stamina OR 2 time). Second travel: Segment 1 face-up showing "Steep Hill." Player knows cost before entering, can plan resource allocation. After 3 travels, 4/5 segments learned (one event segment always random). Route mastered, profitable.

**Distinguishing from:** Map Exploration (revealing geography), Memorization (player memory). Route learning is game state recording of discovered content.

**Cross-reference:** Route Opacity, Delivery Loop, Core Game Loop

---

### Specialization

**Definition:** Player focus on subset of capabilities creating expertise in specific areas and vulnerability in others.

The emergent character identity from limited resource allocation. Cannot maximize all stats/skills/relationships - must choose priorities. Specialization creates builds with peaks of excellence (dominate chosen challenge type) and valleys of weakness (struggle in neglected areas). Different specializations lead to different play experiences - not better/worse, but distinct strategic profiles. Game validates all specializations via Four-Choice Pattern ensuring always path forward regardless of build.

**Example:** Specialize in Insight + Cunning (Investigator build). Insight 4, Cunning 3, others 1-2. Dominates Mental challenges (high-depth card access, effective investigation). Struggles in Social challenges (low Rapport/Authority, limited social card access). When encountering social situations, forced into money-gated or fallback paths.

**Distinguishing from:** Class Selection (predetermined role), Min-Maxing (optimization), Character Progression (advancement). Specialization is emergent from resource constraints.

**Cross-reference:** Build, Five Stats, DDR-003 (Unified Stat System), Vulnerability, Impossible Choice

---

### Strategic Layer

**Definition:** Gameplay layer handling narrative progression and player decision-making with complete transparency.

The outer gameplay shell where player makes WHAT decisions - which scene to pursue, which route to travel, which choice to select. Characterized by perfect information (all costs/rewards/requirements visible), persistent entities (scenes exist until completed), state machine progression (no victory thresholds), turn-based pacing (player controls tempo). Concerns narrative context, resource planning, risk assessment before committing to tactical execution.

**Example:** At location, player sees 3 active scenes (Scene A: Investigation, 3 situations, Medium difficulty. Scene B: Service, 2 situations, Easy difficulty. Scene C: Delivery, 4 situations, High difficulty). Player evaluates requirements, costs, rewards for each. Selects Scene B (has resources for easy path). Scene presents first situation with 4 choices showing exact costs. Strategic layer activities.

**Distinguishing from:** Tactical Layer (card-based execution), Overworld (traversal), Meta Layer (character building). Strategic layer is decision-making with perfect info.

**Cross-reference:** DDR-005 (Strategic-Tactical Separation), Tactical Layer, Perfect Information, Scene-Situation-Choice flow

---

### Sweet Spot

**Definition:** Optimal stat range balancing accessibility and efficiency, avoiding excessive investment.

The strategic concept that moderate stat values (typically 3-4 in 0-5 scale) handle most checks comfortably without over-investment. Higher values unlock marginal benefits at increasing cost - going from 3 → 4 unlocks some content, but 4 → 5 unlocks diminishing amounts while consuming scarce XP better spent elsewhere. Too low (1-2) locks out content. Too high (5 in everything) impossible due to scarcity. Sweet spot finds balance - enough for chosen specialty, avoid over-investment.

**Example:** Insight progression. Insight 2: Access basic Mental cards, pass easy checks. Insight 3: Access intermediate cards, comfortable most situations. Insight 4: Access advanced cards, rarely blocked. Insight 5: Access expert cards (rare content), marginal benefit for high XP cost. Sweet spot 3-4 for Insight specialists, 2-3 for generalists.

**Distinguishing from:** Optimal Build (perfect allocation), Threshold (minimum requirement). Sweet spot is cost-benefit balance point.

**Cross-reference:** Specialization, Five Stats, Diminishing Returns

---

### Tactical Layer

**Definition:** Gameplay layer handling card-based execution with hidden complexity and emergent depth.

The inner gameplay shell handling HOW execution after strategic commitment. Player enters via StartChallenge action type (bridge from strategic), engages in card-based gameplay with hidden variables (draw order, exact flow), builds resources toward victory thresholds (Progress/Momentum/Breakthrough), exits returning outcome to strategic layer. Characterized by hidden complexity (perfect information impossible), temporary sessions (created/destroyed per engagement), resource accumulation (victory thresholds), real-time pacing (session pressure).

**Example:** Strategic layer: Select "Negotiate with innkeeper" choice (StartChallenge type). Bridge: Create SocialSession, extract SituationCards (victory thresholds: Momentum 8 = basic, 12 = optimal). Tactical layer: Draw Social cards, spend Initiative to play, accumulate Momentum, manage Doubt threshold. Reach Momentum 12. Bridge: Apply optimal reward. Strategic layer: Scene advances.

**Distinguishing from:** Strategic Layer (decision-making), Minigame (disconnected subsystem), Combat System (one challenge type). Tactical layer is execution after strategic commitment.

**Cross-reference:** DDR-005 (Strategic-Tactical Separation), Strategic Layer, Mental/Physical/Social Challenges, Bridge

---

### Time Block

**Definition:** Subdivision of calendar day representing 2-4 hour gameplay window, fundamental time unit for activity costs.

The atomic temporal currency structuring daily activities. Each calendar day provides fixed segment count (typically 6 segments = ~12-15 hours waking time). Activities cost segments: Deliveries 3-5 segments, NPC conversations 1-2 segments, equipment shopping 1 segment, sleep requires 0 segments but advances day. Segments are finite non-replenishing resource within day - when exhausted, must sleep or abandon activities. Universal cost enables activity comparison and prioritization.

**Example:** Wake with 6 segments. Accept delivery (4 segments). Travel route (included in delivery). Arrive with 2 segments remaining. Purchase food (1 segment). Rent lodging (1 segment). 0 segments remain. Must sleep advancing to next day. Cannot squeeze additional activities.

**Distinguishing from:** Calendar Day (24-hour cycle), Real-Time (player duration), Turn (discrete action). Time block is in-game temporal currency.

**Cross-reference:** Calendar Day, Daily Rhythm, Resource Flow

---

### Vulnerability

**Definition:** Areas of weakness created by specialization, where build excels in chosen domain but struggles elsewhere.

The necessary consequence of specialization in resource-scarce system. Focusing stat points in Insight/Cunning (Investigator) creates Mental dominance but Social vulnerability - limited Rapport/Authority means social situations challenging. Game acknowledges via Four-Choice Pattern (always fallback available), but vulnerability is intentional design creating meaningful build differences and strategic adaptation requirements. No perfect build avoiding all vulnerabilities - acceptance of weakness enables identity.

**Example:** Investigator build (Insight 4, Cunning 3, Rapport 1, Authority 1). Encounters Social scene requiring Rapport 3. Vulnerability: Cannot access stat-gated optimal path (Rapport 1 < 3 requirement). Forced into money-gated (expensive), challenge-gated (risky without social stats), or fallback (poor outcome). Vulnerability shapes experience.

**Distinguishing from:** Weakness (general low capability), Disadvantage (penalty), Build Flaw (mistake). Vulnerability is intentional specialization consequence.

**Cross-reference:** Specialization, Build, Four-Choice Pattern, Impossible Choice

---

## 12.3 Narrative Terms

### Archetype

**Two Meanings** - Always specify context to avoid confusion.

**Scene Archetype (SpawnPattern):** Template defining situation count, flow structure, and transitions within multi-situation scene. Examples: Linear (3 situations, sequential Always transitions), HubAndSpoke (4 situations, central hub + 3 branches), Branching (conditional OnSuccess/OnFailure transitions). Defines STRUCTURE, not content.

**Situation Archetype (ChoicePattern):** Template generating choice structure (typically 4 choices with stat/money/challenge/fallback pattern). Examples: Negotiation (diplomatic exchange), Confrontation (authority challenge), Investigation (information gathering). Defines CHOICES, not narrative.

**Example (Scene):** "inn_lodging" scene archetype uses Linear pattern: 3 situations (negotiate → execute → depart), Always transitions. Structure template applied to multiple service contexts.

**Example (Situation):** "negotiation" situation archetype generates 4 choices (Rapport-gated instant, money-gated instant, Social challenge, decline fallback). Choice pattern applied across diplomatic contexts.

**Distinguishing from:** Template (broader term), Pattern (more specific term). Archetype is reusable structural pattern.

**Cross-reference:** Archetype Composition, Design Principle 12 (Reusability), Catalogue Pattern

---

### Archetype Composition

**Definition:** Design pattern where Scene Archetypes compose multiple Situation Archetypes into complete multi-situation flows.

The two-tier generation system. Tier 1 (Situation Archetype) generates mechanical choice structure without scene-specific rewards. Tier 2 (Scene Archetype) composes multiple Situation Archetypes and enriches with context-specific rewards. Negotiation mechanics identical across all service contexts - only rewards differ. Bug fix in negotiation propagates to all scenes using it. Balance adjustment affects uniformly.

**Example:** "inn_lodging" Scene Archetype composes 3 Situation Archetypes: (1) "service_negotiation" generates 4 choices, (2) "service_execution_rest" generates rest mechanics, (3) "service_departure" generates exit options. Scene enriches Situation rewards: negotiation success → RoomLocationUnlocked reward, execution success → Health/Stamina restoration, departure → scene completion.

**Distinguishing from:** Inheritance (code reuse), Nesting (containment), Templating (filling slots). Archetype composition is two-tier generation with reward enrichment.

**Cross-reference:** Archetype, DDR-006 (Categorical Scaling), Catalogue Pattern, Design Principle 12

---

### Categorical Property

**Definition:** Descriptive/relative entity attribute enabling dynamic difficulty scaling without absolute values.

The content generation pattern where JSON specifies descriptive qualities (NPCDemeanor: Friendly, Quality: Premium, PowerDynamic: Equal) instead of numeric thresholds. Parser translates categorical → concrete via universal formulas (Friendly = 0.6× stat threshold, Premium = 1.6× coin cost). Enables AI content generation without knowing global game state - AI describes categorically ("friendly innkeeper running premium establishment"), system translates to balanced numbers. Same archetype + different properties = contextually appropriate difficulty.

**Example:** Negotiation archetype base: StatThreshold 5, CoinCost 8. Entity properties: NPCDemeanor Friendly (0.6×), Quality Premium (1.6×). Scaled values: StatThreshold 3 (5 × 0.6), CoinCost 13 (8 × 1.6). Friendly innkeeper negotiation easier but premium lodging expensive - narratively coherent, mechanically balanced.

**Distinguishing from:** Difficulty Setting (player-controlled), Dynamic Difficulty (skill-based adjustment), Procedural Values (random generation). Categorical properties are author/AI-specified descriptive qualities.

**Cross-reference:** DDR-006 (Categorical Scaling), Catalogue Pattern, Entity-Driven Generation

---

### Entity-Driven Generation

**Definition:** Content generation approach deriving mechanical difficulty from entity properties rather than hardcoded scene specifics.

The design philosophy where scene difficulty emerges from entity categorical properties (NPC demeanor, location quality, power dynamics) rather than scene-specific tuning. Same archetype applied to different entities produces different difficulties automatically - friendly NPCs scale easier, hostile harder, premium services cost more, basic services cheaper. Enables reusable archetypes across infinite entity combinations. Content scales naturally with entity context.

**Example:** "negotiation" archetype applied to 3 entities. Entity A (Friendly NPC, Basic quality): StatThreshold 3, CoinCost 5 (easy, cheap). Entity B (Neutral NPC, Standard quality): StatThreshold 5, CoinCost 8 (baseline). Entity C (Hostile NPC, Luxury quality): StatThreshold 7, CoinCost 19 (hard, expensive). Same archetype, different difficulties from entity properties.

**Distinguishing from:** Procedural Generation (random creation), Context-Sensitive (environment reactions). Entity-driven generation is difficulty derivation from entity properties.

**Cross-reference:** Categorical Property, Archetype Composition, DDR-006 (Categorical Scaling)

---

### Frieren Principle

**Definition:** Narrative design philosophy treating travel as eternal state without endpoint, enabling infinite journey.

The conceptual foundation named after anime "Frieren: Beyond Journey's End" depicting eternal traveler. Rejects traditional narrative arc (setup → climax → resolution) in favor of perpetual journey pattern (arrival → experience → departure → arrival...). No final destination, no ultimate climax, no resolution - cycle continues infinitely. Meaning emerges from journey itself, not reaching goal. Eliminates hardest narrative problem (how to end satisfyingly) by rejecting endings. Enables infinite content by removing closure pressure.

**Example:** Traditional RPG: "Save the kingdom" → Defeat villain → Kingdom saved → Game ends → Post-game awkward. Frieren Principle: "Seek scattered colleagues" → Meet someone → Help/learn → They mention next place → Travel → Arrive → Meet someone new → Cycle continues infinitely. No pressure to "finish" because journey IS content.

**Distinguishing from:** Open World (explorable space without linear path), Sandbox (player-directed activities). Frieren Principle is philosophical framework treating travel as purpose, not means.

**Cross-reference:** DDR-001 (Infinite A-Story), Infinite Journey, A-Story, Eternal Traveler Fantasy

---

### Narrative Philosophy

**Definition:** Set of storytelling principles structuring how Wayfarer presents fiction and player role.

The guiding concepts shaping narrative presentation: (1) Journey is point, not obstacle between points (Frieren Principle). (2) Player is courier, not hero - small role in bigger world. (3) NPCs are people with own concerns, not quest dispensers. (4) Connections form naturally through conversation/experience, not cutscenes. (5) Reputation emerges from actions, not alignment score. (6) Geographic variety reflects endless travel. (7) Thematic variation within arrival-meeting-helping-departing cycle.

**Example:** Arrive at new town. Innkeeper is running inn (her life), not waiting for you. She has regulars, concerns, relationships. You're another customer. If you help her, she remembers. If you're rude, she's cold next time. Your reputation spreads organically - other NPCs mention "that helpful courier" or "that rude traveler." Natural world, not hero-centric.

**Distinguishing from:** Narrative Structure (technical organization), Story (specific events). Narrative philosophy is design principles for storytelling.

**Cross-reference:** Frieren Principle, Verisimilitude, Player Mental State

---

### Placement Filter

**Definition:** Categorical entity selection criteria for scene spawning, avoiding hardcoded IDs.

The procedural-friendly targeting system where scenes specify requirements categorically (needs "Friendly NPC at Standard inn") rather than concretely (needs "elena_innkeeper" at "rusty_tankard"). Spawning evaluates all entities matching filter, selects viable candidates, spawns scene at chosen entity. Enables same scene template spawning infinite times at different entities. AI can generate scenes with categorical filters without knowing specific entity IDs.

**Example:** Scene template specifies PlacementFilter: EntityType = NPC, Role = Innkeeper, Demeanor = Friendly, VenueQuality = Standard. Parser evaluates: Elena (innkeeper, friendly, standard) matches. Marcus (merchant, neutral, premium) doesn't match. Scene spawns at Elena. Next playthrough, different world generation creates different NPCs - filter finds equivalent match, scene spawns there.

**Distinguishing from:** Entity Reference (concrete ID), Targeting (selection), Spawn Condition (temporal/state requirements). Placement filter is categorical entity selection.

**Cross-reference:** Categorical Property, Entity-Driven Generation, Procedural Content, Spawn Condition

---

### Property-Driven Variation

**Definition:** Design approach where entity property combinations generate mechanically distinct variations from single archetype.

The content multiplication technique where combining categorical properties produces vast variety. 21 archetypes × 3 NPCDemeanors × 4 Qualities × 3 PowerDynamics × 3 EnvironmentQualities = 2,268 mechanical variations from property combinations alone. Add narrative variety from entity-specific context = effectively infinite content. Same archetype + different property sets = different difficulties, different feels, different player experiences.

**Example:** "negotiation" archetype with varying properties. (Friendly, Basic, Equal) = easy, cheap, approachable. (Hostile, Luxury, Submissive) = hard, expensive, tense. (Neutral, Standard, Dominant) = baseline, moderate, professional. Hundreds of variations from one archetype by varying property combinations. Each feels distinct despite shared structure.

**Distinguishing from:** Random Variation (unpredictable changes), Procedural Content (algorithmic generation), Parameterization (variable values). Property-driven variation is combinatorial multiplication via categorical properties.

**Cross-reference:** Categorical Property, Entity-Driven Generation, Archetype Composition, DDR-006

---

### Spawn Condition

**Definition:** Requirements for scene instantiation based on player/world/entity state, enabling conditional content.

The dependency system determining when scenes become active. Combines temporal conditions (day ranges, expiration dates), state conditions (RequiresTags, player resources), and entity conditions (placement availability). Scene spawning evaluates all conditions - if met, scene instantiates and enters world. Supports both linear progression (A1 → A2 → A3) and branching (B-stories based on player choices/relationships). Separate from placement (where scene appears) and activation (when player encounters it).

**Example:** Scene SpawnConditions: RequiresTags ["tutorial_complete", "met_merchant"], Day >= 5, Day <= 20 (expires after day 20), Player.Coins >= 10. Scene won't spawn until: tutorial finished, merchant met, day 5 reached, player has 10 coins. Once spawned, expires day 20 if not completed. Conditional content.

**Distinguishing from:** Placement Filter (where to spawn), Activation (when player encounters), Requirements (action prerequisites). Spawn conditions determine when scene enters world.

**Cross-reference:** Tag-Based Dependencies, DDR-002 (Tag-Based Scene Dependencies), Scene Instantiation

---

### Tag-Based Dependency

**Definition:** Progression system where scenes require/grant abstract tags representing knowledge, capabilities, or story milestones.

The flexible dependency architecture replacing hardcoded chains. Each scene defines RequiresTags (player needs these to spawn scene) and GrantsTags (player gains these on completion). Spawning evaluates tags against player's accumulated set - if player has required tags, scene eligible to spawn. Enables branching (multiple scenes requiring same tags, player chooses), cross-storyline dependencies (B-story grants tag unlocking A-story path), procedural content (AI generates scenes with categorical tag requirements).

**Example:** A1 completes → GrantsTags: ["tutorial_complete", "knows_innkeeper"]. A2 requires ["tutorial_complete"]. B-Story-1 requires ["knows_innkeeper", "has_5_coins"]. Player can pursue A2 OR B-Story-1 after A1 - both requirements met. Tags enable flexible graph, not linear chain.

**Distinguishing from:** Quest Chain (linear sequence), Boolean Flags (has/hasn't checks), State Machine (discrete states). Tag-based uses accumulated set enabling flexible branching.

**Cross-reference:** DDR-002 (Tag-Based Scene Dependencies), Spawn Condition, Branching, Flexible Progression

---

### Tier Escalation

**Definition:** Narrative scope expansion from local concerns to cosmic stakes as A-story progresses.

The pacing pattern where procedural A-story escalates thematic scope over time. Early scenes (Tier 0-1) involve local concerns - innkeepers, merchants, town disputes. Mid-game scenes (Tier 2) expand to regional politics, guild conflicts, territorial issues. Late-game scenes (Tier 3) reach continental crises, kingdom-level stakes, major factions. End-game scenes (Tier 4) touch cosmic mysteries, reality-bending phenomena, world-shaping forces. Same arrival-meeting-helping-departure cycle, escalating stakes.

**Example:** Tier 0: Help innkeeper with supply shortage (local). Tier 1: Resolve merchant guild dispute (town-level). Tier 2: Investigate regional trade route sabotage (multi-city). Tier 3: Mediate kingdom succession crisis (continental). Tier 4: Discover ancient power threatening reality (cosmic). Same structure, escalating scope.

**Distinguishing from:** Difficulty Curve (mechanical challenge), Level Scaling (stat increases), Power Creep (ability inflation). Tier escalation is narrative scope expansion.

**Cross-reference:** A-Story, Infinite Journey, Procedural Continuation, Design Goal (engagement over time)

---

### Verisimilitude

**Definition:** Design quality where mechanical systems align with fictional reality, creating believable world feel.

The principle that game mechanics should match player's conceptual model of fiction. Investigations take time across visits (pauseable Mental challenges). Physical obstacles are immediate tests (one-shot Physical challenges). Relationships build through interaction (StoryCubes accumulate per NPC). Entity relationships should feel natural (Scenes spawn from Obligations, not Locations own Scenes). When mechanics match fiction, systems feel intuitive and immersive. When mechanics contradict fiction, systems feel arbitrary and confusing.

**Example - Good Verisimilitude:** Investigation challenge pauseable - leave location mid-investigation, progress persists, return later to continue. Matches fiction: real investigations span time, not solved in single visit.

**Example - Poor Verisimilitude:** Physical obstacle pauseable - pause mid-climb, leave, return days later, resume from same point. Breaks fiction: climbing is immediate physical test, not pauseable across days.

**Distinguishing from:** Realism (simulation accuracy), Immersion (player engagement), Internal Consistency (rule adherence). Verisimilitude is mechanics-fiction alignment.

**Cross-reference:** DDR-010 (Pauseable Mental), Design Principle 8 (Verisimilitude in Relationships), Narrative Philosophy, TIER 3 Principles

---

## 12.4 Progression Terms

### Bond (NPC Relationship)

**Definition:** Accumulated trust and familiarity with specific NPC, tracked via StoryCubes (0-10 scale).

The per-entity relationship progression tracking how well player knows individual NPC. Each NPC has separate StoryCubes value starting at 0 (strangers). Social challenges with that NPC build cubes (typically +1 per successful conversation). Higher cubes reduce Social Doubt with that NPC (familiarity makes conversation easier), unlock special dialogue options, affect NPC behavior toward player. Requires separate investment per NPC - knowing Elena doesn't help with Marcus. Persistent across game.

**Example:** Elena starts StoryCubes 0. First conversation (Social challenge): Success → Elena StoryCubes 1. Second conversation: Success → Elena StoryCubes 2. Third conversation: Elena recognizes you (cube threshold 2), Doubt starts lower (familiarity bonus), special dialogue option available ("Remember when we..."). Marcus still StoryCubes 0 (separate relationship).

**Distinguishing from:** Reputation (global standing), Friendship Flag (binary), Relationship Level (abstract tier). Bond is numeric per-NPC progression with mechanical effects.

**Cross-reference:** StoryCubes, Social Challenge, Per-Entity Resources, Relationship Progression

---

### Route Learning

**(Definition provided in Core Gameplay Terms section - see above)**

---

### Stat Gating

**Definition:** Choice or content locked behind stat threshold requirement, accessible only when player meets numeric minimum.

The progression pattern where options require stat thresholds (Insight >= 3, Rapport >= 4) to access. Player with insufficient stat sees option greyed with exact requirement displayed ("Requires Insight 3, you have 2, need 1 more"). Once player advances stat (via playing cards, earning XP), option becomes accessible. Creates build-based progression - different stats gate different content, specialization unlocks chosen paths earlier. Critically different from Boolean Gate - shows exact numeric gap, enables planning, not arbitrary flag.

**Example:** Situation offers 4 choices. Choice 1: "Deduce pattern" requires Insight 3 (player Insight 2, disabled). Choice 2: "Charm merchant" requires Rapport 3 (player Rapport 4, enabled). Player can access Rapport path (built Rapport), cannot access Insight path (neglected Insight). Build determines accessible content.

**Distinguishing from:** Boolean Gate (hidden flag check), Level Gate (arbitrary level), Skill Check (test mechanic). Stat gating is transparent numeric threshold with perfect information.

**Cross-reference:** Five Stats, Resource Arithmetic, Perfect Information, Build, Specialization

---

### Sweet Spot

**(Definition provided in Core Gameplay Terms section - see above)**

---

### Vulnerability

**(Definition provided in Core Gameplay Terms section - see above)**

---

## 12.5 Challenge Terms

### Action Pair

**Definition:** Two complementary card types creating tactical rhythm and resource flow within challenge system.

The core tactical pattern where each challenge type has two action categories driving gameplay loop. Mental: ACT (generate Leads) / OBSERVE (follow Leads drawing Details). Physical: EXECUTE (lock Options) / ASSESS (trigger locked Options). Social: SPEAK (advance via Statements) / LISTEN (reset and draw). Action pairs create rhythm - alternating between generation and consumption, between buildup and payoff, between risk and consolidation. Tactical depth emerges from pair timing and resource management.

**Example (Mental):** ACT card "Search room" (costs Attention, generates 2 Leads). Current Leads: 2. OBSERVE action "Follow investigative threads" (draw Details equal to Leads). Draw 2 Details (observation cards). Details build Progress toward victory. ACT → OBSERVE rhythm drives investigation.

**Distinguishing from:** Card Types (broader category), Combo (specific sequence), Synergy (interaction bonus). Action pair is fundamental tactical rhythm.

**Cross-reference:** Mental Challenge, Physical Challenge, Social Challenge, Flow Mechanic

---

### Builder Resource

**Definition:** Primary progress-tracking resource accumulating toward victory threshold in tactical challenges.

The "good" resource player accumulates to win. Mental challenges: Progress (investigation advancement). Physical challenges: Breakthrough (obstacle surmounting). Social challenges: Momentum (conversation progress). Cards generate builder resource through play. Reaching threshold (typically 8-15 depending on difficulty) triggers victory and returns to strategic layer with success rewards. Builder resource tracks "how close to winning."

**Example (Social):** SituationCard specifies Momentum 12 = victory threshold. Start with 0 Momentum. Play SPEAK cards generating Momentum (+2, +3, +4 depending on card). Current Momentum: 9/12. Play Statement card (+3 Momentum). Momentum reaches 12. Victory threshold met. Challenge won, return to strategic layer, apply success rewards.

**Distinguishing from:** Victory Points (scoring), Progress Bar (UI), Experience Points (permanent advancement). Builder resource is temporary tactical victory tracker.

**Cross-reference:** Threshold Resource, Session Resource, Victory Threshold, Mental/Physical/Social Challenges

---

### Challenge

**Three Meanings** - Always specify context to avoid confusion.

**Tactical Challenge System:** Mental/Physical/Social subsystems constituting tactical layer. Separate gameplay mode with deck, cards, resources, victory thresholds. Entered via StartChallenge action type, returns outcome to strategic layer.

**Challenge PathType:** ChoiceTemplate.PathType.Challenge value indicating choice spawns tactical challenge. Distinguished from InstantSuccess (immediate effects) and Navigate (movement).

**Challenge Situation:** Informal usage referring to narrative situation involving difficulty/risk. Not technical term - prefer "difficult situation" to avoid confusion.

**Example (System):** "Mental challenge system uses Progress/Attention/Exposure with pauseable sessions."

**Example (PathType):** "Choice has PathType Challenge, ChallengeType Social, spawns Social challenge."

**Example (Informal):** "Player faces challenge situation requiring diplomacy" - better phrased "player faces difficult diplomatic situation."

**Distinguishing from:** Each other (three distinct meanings). Always specify which Challenge meaning intended.

**Cross-reference:** Tactical Layer, Mental/Physical/Social Challenges, PathType, Strategic-Tactical Bridge

---

### Flow Mechanic

**Definition:** System-specific resource flow pattern creating tactical identity and differentiating challenge types.

The unique tactical characteristic distinguishing Mental/Physical/Social challenges beyond shared resource structure. Mental: Leads system (ACT generates Leads, OBSERVE draws Details equal to Leads - cannot observe what not investigated). Physical: Aggression spectrum (Overcautious ← Balanced → Reckless, affects card costs/effects). Social: Cadence pacing (conversation rhythm affecting Initiative generation and Doubt accumulation). Flow mechanics make systems FEEL different despite parallel structure.

**Example (Mental):** Leads flow - no Leads generated yet. OBSERVE draws 0 cards (cannot observe nothing). Play ACT card generating 3 Leads. Now OBSERVE draws 3 Details. Investigation flow: investigate THEN observe results.

**Example (Physical):** Aggression flow - start Balanced (0). Play Reckless card (shift right). Now at Reckless (+1). Reckless stance: aggressive cards cost less Initiative, cautious cards cost more. Flow creates stance management.

**Distinguishing from:** Resource Management (general), Card Interaction (specific combos), System Mechanic (broader category). Flow mechanic is system-defining pattern.

**Cross-reference:** Mental Challenge, Physical Challenge, Social Challenge, Action Pair

---

### Mental Challenge

**Definition:** Investigation-focused tactical challenge using observation and deduction to accumulate Progress.

The pauseable tactical system for location-based investigations. Player explores crime scenes, searches rooms, analyzes evidence through card play. Resources: Progress (builder, persists), Attention (session budget from permanent Focus), Exposure (threshold, persists - investigative footprint), Leads (observation flow - generated by ACT cards). Action Pair: ACT (investigative actions generating Leads) / OBSERVE (draw Details equal to Leads count). Pauseable - can leave mid-investigation, state persists, return with fresh Attention.

**Example:** Enter "Investigate Mill Fire" Mental challenge. Start: Progress 0/12, Attention 18/18, Exposure 0/10, Leads 0. Play ACT card "Search debris" (Attention -4, Leads +2, Exposure +1). Current: Progress 0/12, Attention 14/18, Exposure 1/10, Leads 2. OBSERVE action draws 2 Details. Details generate Progress. Reach Progress 12 = victory. Or leave location (pause), return later continuing from current state.

**Distinguishing from:** Physical Challenge (one-shot obstacles), Social Challenge (NPC conversations), Investigation (general activity). Mental challenge is specific tactical system.

**Cross-reference:** DDR-009 (Three Challenge Types), DDR-010 (Pauseable Sessions), Tactical Layer, Builder/Threshold/Session Resources

---

### One-Shot Session

**Definition:** Challenge model requiring completion or failure in single continuous attempt, cannot pause and resume.

The session pattern for Physical challenges matching fiction of immediate physical tests. Player attempts obstacle (climb cliff, cross chasm, overcome barrier) in single session. Must complete (reach Breakthrough threshold) or fail (reach Danger threshold) before leaving. Cannot pause mid-climb, leave location, return days later to resume. Personal resources persist (Health/Stamina carry between challenges), but session state doesn't. Failed challenge can be re-attempted (new session, fresh start), but cannot pause mid-attempt.

**Example:** Attempt "Scale City Wall" Physical challenge. Start: Breakthrough 0/10, Exertion 20/20, Danger 0/8. Play EXECUTE cards building combo. Danger accumulates (risky moves). Reach Danger 8 before Breakthrough 10 = automatic failure. Challenge ends (one-shot), cannot pause. Can re-attempt later (new session, reset to 0/0/0).

**Distinguishing from:** Pauseable Session (Mental challenges), Session-Bounded (Social challenges), Permadeath (character loss). One-shot session is single-attempt challenge pattern.

**Cross-reference:** DDR-010 (Pauseable vs One-Shot), Physical Challenge, Pauseable Session, Session-Bounded

---

### Pauseable Session

**Definition:** Challenge model allowing player to leave mid-session with state persisting for later resumption.

The session pattern for Mental challenges matching fiction of investigations spanning time. Player can investigate location, make progress, then leave (other obligations, resource exhaustion, strategic choice). Progress/Exposure persist exactly where left off. Attention resets (return with fresh mental energy). Can resume investigation later from saved state, incrementally accumulating Progress across multiple visits until victory threshold reached. No forced ending - high Exposure makes investigation harder but doesn't cause failure.

**Example:** Investigating crime scene. Session 1: Reach Progress 5/12, Exposure 3/10, Attention exhausted. Leave location (pause). Session 2 (next day): Resume from Progress 5/12, Exposure 3/10, Attention reset to 18/18. Continue investigation. Session 3: Finally reach Progress 12, victory. Investigation completed across 3 visits.

**Distinguishing from:** One-Shot Session (Physical), Session-Bounded (Social), Save System (meta-feature). Pauseable session is in-fiction investigation spanning time.

**Cross-reference:** DDR-010 (Pauseable Mental Challenges), Mental Challenge, One-Shot Session, Verisimilitude

---

### Physical Challenge

**Definition:** Obstacle-focused tactical challenge using strength and precision to achieve Breakthrough.

The one-shot tactical system for location-based obstacles. Player confronts physical barriers - cliffs to climb, chasms to cross, doors to break through card play. Resources: Breakthrough (builder), Exertion (session budget from permanent Stamina), Danger (threshold - accumulating risk), Aggression (balance spectrum - Overcautious ← Balanced → Reckless). Action Pair: EXECUTE (lock Options as combo preparation) / ASSESS (trigger locked Options as powerful combo). One-shot - must complete or fail in single attempt, cannot pause mid-obstacle.

**Example:** Enter "Climb Cliff Face" Physical challenge. Start: Breakthrough 0/10, Exertion 20/20, Danger 0/8. Play EXECUTE card "Lock handholds" (Exertion -5, locks Option). Play EXECUTE card "Lock footwork" (Exertion -4, locks Option). ASSESS action triggers 2 locked Options (Breakthrough +6, Danger +3). Current: Breakthrough 6/10, Danger 3/8. Continue until Breakthrough 10 (victory) or Danger 8 (failure).

**Distinguishing from:** Mental Challenge (pauseable investigations), Social Challenge (NPC conversations), Combat System (not combat-focused). Physical challenge is specific tactical system.

**Cross-reference:** DDR-009 (Three Challenge Types), DDR-010 (One-Shot Sessions), Tactical Layer, Builder/Threshold/Session Resources

---

### Session Resource

**Definition:** Temporary tactical capacity enabling card play within challenge session, resetting between sessions.

The "action economy" resource determining how many cards player can play per session. Mental: Attention (derived from permanent Focus stat, mental capacity for observation). Physical: Exertion (derived from permanent Stamina stat, physical capacity for action). Social: Initiative (generated via Foundation cards during session, conversation action economy). Cards cost session resource to play. When session resource exhausted, player must leave (Mental/Social) or risk Danger (Physical). Resets on new session.

**Example (Mental):** Player Focus 6 → Attention 18 per session (6 × 3 conversion). Enter Mental challenge. Play ACT card (Attention -4). Play another ACT card (Attention -4). Current Attention 10/18. Play OBSERVE (free). Play Detail cards (Attention -2 each). Attention depleted. Cannot play more cards. Must leave (state persists) or accept limited options. Next session: Attention resets to 18.

**Distinguishing from:** Builder Resource (victory progress), Threshold Resource (failure risk), Action Points (generic term). Session resource is specific tactical capacity.

**Cross-reference:** Builder Resource, Threshold Resource, Mental/Physical/Social Challenges, Attention/Exertion/Initiative

---

### Session-Bounded

**Definition:** Challenge model requiring completion within single interaction window, can end voluntarily or forced, but cannot pause and resume later.

The session pattern for Social challenges matching fiction of real-time conversations. Player has conversation with NPC happening now - can speak, listen, accumulate Momentum, but conversation exists in continuous time. Can leave voluntarily (end conversation early, suboptimal outcome). Can be forced to leave (MaxDoubt reached, NPC frustration ends conversation). Cannot pause mid-conversation, leave, return days later to resume from same sentence. Relationship persists (StoryCubes carry between conversations), but conversation state doesn't.

**Example:** Conversation with Elena. Start: Momentum 0/12, Initiative 8, Doubt 0/10. Play SPEAK cards accumulating Momentum and Doubt. Doubt reaches 8/10 (Elena getting frustrated). Nearly winning (Momentum 10/12) but one more Doubt = forced end. Risk it? Or LISTEN (reset, continue later)? If Doubt reaches 10: Conversation force-ended, no victory. Can start new conversation later, but this conversation lost.

**Distinguishing from:** Pauseable Session (Mental), One-Shot Session (Physical - different constraints), Turn-Based (not real-time). Session-bounded is continuous conversation window.

**Cross-reference:** DDR-010 (Session Models), Social Challenge, Pauseable Session, One-Shot Session, Verisimilitude

---

### Social Challenge

**Definition:** Conversation-focused tactical challenge using rapport and persuasion to accumulate Momentum.

The session-bounded tactical system for NPC-based conversations. Player engages dialogue with NPC through card play - building argument, establishing rapport, persuading toward goal. Resources: Momentum (builder), Initiative (session economy from Foundation cards), Doubt (threshold - NPC frustration/skepticism), Cadence (conversation pacing). Action Pair: SPEAK (advance conversation via Statements) / LISTEN (reset and draw new cards). Session-bounded - must complete in single conversation, MaxDoubt ends interaction, can leave voluntarily but loses session.

**Example:** Enter "Persuade Guard" Social challenge. Start: Momentum 0/12, Initiative 10, Doubt 0/10. Play Foundation card (+4 Initiative). Play SPEAK card "Reasonable appeal" (Initiative -6, Momentum +3, Doubt +1). Current: Momentum 3/12, Initiative 8, Doubt 1/10. Continue building Momentum. Reach Momentum 12 before Doubt 10 = victory. Or Doubt 10 = forced conversation end, failure.

**Distinguishing from:** Mental Challenge (pauseable investigations), Physical Challenge (one-shot obstacles), Dialogue System (broader term). Social challenge is specific tactical system.

**Cross-reference:** DDR-009 (Three Challenge Types), DDR-010 (Session-Bounded), Tactical Layer, Builder/Threshold/Session Resources, StoryCubes

---

### Threshold Resource

**Definition:** Accumulating danger/risk resource threatening failure if maximum reached during tactical challenge.

The "bad" resource player tries to avoid accumulating. Mental: Exposure (investigative footprint making scene aware of investigation). Physical: Danger (accumulating risk from reckless moves). Social: Doubt (NPC frustration/skepticism toward player). Cards generate threshold resource as cost/consequence of actions. Reaching maximum (typically 8-10) triggers failure condition and returns to strategic layer with failure rewards. Threshold resource tracks "how close to losing."

**Example (Physical):** SituationCard specifies Danger 8 = failure threshold. Start with 0 Danger. Play risky EXECUTE card (Breakthrough +4, Danger +2). Play another risky card (Breakthrough +3, Danger +3). Current Danger: 5/8. Play ultra-risky card (Breakthrough +5, Danger +4). Danger reaches 9/8. Exceeds threshold. Automatic failure, injury applied, challenge lost.

**Distinguishing from:** Failure State (game over), Countdown (time limit), Penalty (negative effect). Threshold resource is accumulating failure risk.

**Cross-reference:** Builder Resource, Session Resource, Victory Threshold, Mental/Physical/Social Challenges, Auto-Trigger

---

### Understanding

**Definition:** Global persistent resource unlocking card tier access across all three challenge systems.

The cross-challenge advancement resource representing player's accumulated expertise with tactical systems. Playing any card in any challenge type grants Understanding XP. Understanding level (0-10+ scale) unlocks card tiers universally - Understanding 3 unlocks Tier-3 Mental AND Physical AND Social cards. Single progression path benefiting all three systems. Separate from stats (which gate individual cards based on stat binding) - Understanding gates tier access, stats gate depth access.

**Example:** Start Understanding 0 (Tier-0 cards only). Play Mental cards earning Understanding XP. Reach Understanding 1 (Tier-1 unlocked). Now can use Tier-1 Mental, Physical, AND Social cards. Play Physical cards earning more XP. Reach Understanding 2. Tier-2 unlocked across all systems. Single progression benefits all tactical gameplay.

**Distinguishing from:** Stats (Insight/Rapport/etc - card depth binding), Experience (general term), Level (character level). Understanding is specific cross-challenge tier progression.

**Cross-reference:** Five Stats, Mental/Physical/Social Challenges, Cross-Challenge Progression, Card Tiers

---

### Victory Threshold

**Definition:** Builder resource target value triggering challenge success when reached, completing tactical session.

The winning condition for tactical challenges. SituationCard extracted from parent Situation specifies threshold value (Progress 12, Breakthrough 10, Momentum 15, etc). Player accumulates builder resource through card play. When builder resource reaches/exceeds threshold, victory triggers automatically (no player choice - threshold crossing = win). Challenge session ends, returns to strategic layer, applies success rewards from SituationCard, Scene advances.

**Example:** SituationCard specifies "Momentum 12 = Basic Victory (RestoreStamina +10), Momentum 18 = Optimal Victory (RestoreStamina +20, Elena +1)." Player accumulates Momentum. Reaches Momentum 12: Basic Victory triggered (cannot continue to 18 - threshold crossing ends session immediately). Stamina restored +10, return to strategic layer, Scene advances to next Situation.

**Distinguishing from:** Win Condition (general), Goal (objective), Score (points). Victory threshold is specific numeric target for tactical challenges.

**Cross-reference:** Builder Resource, Threshold Resource, SituationCard, Tactical Layer, Auto-Trigger

---

## 12.6 Design Pattern Terms

### Anti-Pattern (Game Design)

**Definition:** Commonly recurring design approach that appears reasonable but produces poor player experience.

The game design patterns to avoid. Boolean Gate (unlocking via flags instead of resource arithmetic). Generic Property Modification (string-based property routing instead of typed properties). ID Encoding (data in ID strings instead of properties). Cookie Clicker Pattern (linear unlocking without trade-offs). Soft-Lock (trapped game state). Hidden Requirements (mystery gates violating perfect information). Anti-patterns may seem simpler initially but create long-term problems: poor strategic depth, maintenance nightmares, player frustration, architectural violations.

**Example (Boolean Gate):** "if (player.HasRope) { EnableClimbing(); }" - appears simple but: no strategic depth (rope is "free" unlock), no opportunity cost, no specialization, checklist completion not choices. Correct: Resource arithmetic with trade-offs.

**Example (Soft-Lock):** Player can spend all coins on equipment, then encounter situation requiring 10 coins with no way to earn more. Trapped, cannot progress. Violates TIER 1 principle. Correct: Four-Choice Pattern with fallback.

**Distinguishing from:** Bug (implementation error), Bad Practice (poor code), Incorrect Implementation (wrong solution). Anti-pattern is structurally flawed design approach.

**Cross-reference:** Boolean Gate, Soft-Lock, Design Principles, CLAUDE.md Forbidden Patterns

---

### Archetype Reusability

**Definition:** Design goal enabling single archetype to apply across many fictional contexts without modification.

The content multiplication principle where mechanical patterns separate from narrative specifics. "Negotiation" archetype defines 4-choice structure (stat/money/challenge/fallback), cost formulas, PathType routing - but NOT specific narrative or rewards. Same archetype generates inn lodging, bathhouse access, healer services, passage rights, information purchase by varying entity properties and reward enrichment. Reusability test: can archetype apply to 5+ different contexts unmodified? If yes, properly reusable.

**Example:** "negotiation" archetype used for: (1) Innkeeper lodging (reward: room access), (2) Guard access control (reward: location unlock), (3) Merchant information (reward: tags granted), (4) Healer services (reward: health restoration), (5) Guide employment (reward: route reveal). Same mechanical structure, different narratives/rewards. One archetype, infinite applications.

**Distinguishing from:** Code Reuse (technical), Template (broader), Modularity (architectural). Archetype reusability is game design content multiplication.

**Cross-reference:** Design Principle 12 (Archetype Reusability), Design Principle 13 (Abstraction Level), Archetype Composition

---

### Boolean Gate

**Definition:** Progression anti-pattern using true/false checks to hide/reveal content instead of resource arithmetic.

The anti-pattern violating Requirement Inversion Principle. Content exists but hidden behind boolean flags. Checking if player has item/completed quest/unlocked area (true/false). Reveals pre-existing content when flag flips. Creates Cookie Clicker gameplay: checklist completion without strategic depth, no opportunity cost (unlocks "free"), no specialization (eventually do everything), linear progression (A unlocks B unlocks C).

**Example:** `if (player.CompletedQuest("phase_1")) { phase2Goal.IsHidden = false; }` - Phase 2 existed all along, just hidden. Flag check reveals it. No resource cost, no trade-off, no strategic decision. Correct alternative: Completing Phase 1 CREATES Phase 2 goal (entity spawning), or requires resource arithmetic (Phase 2 costs 15 coins player must afford).

**Distinguishing from:** Resource Arithmetic (numeric comparison), State-Based Content (valid content for state), Entity Spawning (content creation). Boolean gate is flag-based hiding/revealing.

**Cross-reference:** Resource Arithmetic, DDR-002 (Tag-Based vs Boolean), Requirement Inversion Principle, Anti-Pattern, CLAUDE.md Forbidden Patterns

---

### Branching Choice Pattern

**Definition:** Design pattern presenting 2-4 meaningful options with distinct consequences, revealing player values through constraint.

The choice structure philosophy where every decision forces prioritization. All options are valid approaches with genuine benefits, but insufficient resources to pursue all. No "correct" answer - different choices for different builds, values, situations. Choices reveal character through what player sacrifices. Branching distinguishes from Linear (one path) and Binary (two extremes). Wayfarer uses Four-Choice Archetype: stat-gated (optimal), money-gated (reliable), challenge (skill), fallback (guaranteed).

**Example:** Scene offers 4 choices. (1) Rapport-gated: Best outcome, free if qualified, rewards relationship build. (2) Money-gated: Good outcome, costs 15 coins, always available with money. (3) Social challenge: Variable outcome, costs Resolve+time, tactical skill. (4) Fallback: Poor outcome, free, patient/humble approach. All valid, insufficient resources for all. Which sacrifice acceptable? Choice reveals priorities.

**Distinguishing from:** Binary Choice (two extremes), Multiple Choice (quantity focus), Dialogue Tree (conversation structure). Branching choice is value-revealing prioritization pattern.

**Cross-reference:** Four-Choice Archetype, Impossible Choice, Design Principle 11 (Balanced Choices), Orthogonal Costs

---

### Catalogue Pattern

**Definition:** Parse-time translation from categorical JSON properties to concrete runtime types, enabling AI content generation.

The three-phase content pipeline. (1) JSON Authoring: Categorical properties ("Friendly", "Premium", "Hostile"). (2) Parse-Time Translation: Catalogues convert categorical → concrete via formulas (Friendly = 0.6× threshold). (3) Runtime: Use concrete properties only (NO catalogue calls, NO string matching). Enables AI generation without global game state knowledge - AI specifies categorically, system translates to balanced numbers. One-time cost at load, zero runtime cost.

**Example:** JSON: `{ "npcDemeanor": "Friendly", "quality": "Premium" }`. Parse-time: Catalogue translates (Friendly 0.6×, Premium 1.6×). Template stores concrete values (StatThreshold: 3, CoinCost: 13). Runtime: Direct property access `if (player.Rapport >= choice.RequiredRapport)`. NO catalogue imports at runtime.

**Distinguishing from:** Factory Pattern (object creation), Translation Layer (broader), Preprocessor (compile-time). Catalogue pattern is parse-time categorical → concrete translation.

**Cross-reference:** DDR-006 (Categorical Scaling), Categorical Property, Three-Phase Pipeline, CLAUDE.md Catalogue Pattern

---

### Four-Choice Archetype

**Definition:** Standard A-story situation pattern presenting stat-gated, money-gated, challenge, and fallback paths.

The guaranteed progression pattern preventing soft-locks. Every A-story situation offers four choices with orthogonal costs: (1) Stat-gated (requires threshold, free, best rewards), (2) Money-gated (costs coins, reliable, good rewards), (3) Challenge (costs session resource, variable rewards, tactical skill), (4) Fallback (no requirements, minimal rewards, patient approach). All four spawn next A-scene (different entry states, same progression). Ensures always forward path regardless of player build/resources.

**Example:** Situation presents: (1) "Charm innkeeper" (Rapport 3, best outcome). (2) "Pay premium rate" (15 coins, good outcome). (3) "Negotiate challenge" (Social challenge, variable). (4) "Wait patiently for kindness" (no requirements, poor outcome but ADVANCES). Player lacking Rapport/coins/Resolve can choose fallback - slow but never stuck.

**Distinguishing from:** Three-Choice (missing fallback or challenge), Variable Choice (inconsistent), Binary (two options). Four-choice is specific pattern guaranteeing progression.

**Cross-reference:** DDR-007 (Four-Choice Design Decision), Guaranteed Progression, Orthogonal Costs, No Soft-Lock, TIER 1 Principles

---

### Investigation Pattern

**Definition:** Multi-visit challenge pattern where player accumulates progress incrementally across sessions, matching investigation fiction.

The Mental challenge structure supporting pauseable sessions. Player investigates location, makes progress, can leave anytime (state persists), returns later with fresh resources, continues investigation. Progress/Exposure persist (investigation footprint), Attention resets (mental energy). Matches fiction: real investigations span time, gather evidence over multiple visits, aren't solved in single moment. Distinct from Physical (one-shot) and Social (session-bounded) patterns.

**Example:** Crime scene investigation. Visit 1: Gather evidence (Progress 4/12, Exposure 2/10, Attention exhausted). Leave location, do other activities. Visit 2: Return with Attention 18. Continue investigation (Progress 8/12, Exposure 5/10). Visit 3: Solve case (Progress 12 reached). Investigation completed over 3 visits matching investigative fiction.

**Distinguishing from:** Mystery (narrative genre), Puzzle (specific mechanic), Quest (broader structure). Investigation pattern is specific pauseable challenge structure.

**Cross-reference:** Mental Challenge, Pauseable Session, DDR-010 (Session Models), Verisimilitude

---

### Service Flow Pattern

**Definition:** Three-situation scene structure (negotiate → execute → depart) for service-based interactions.

The multi-phase service archetype composing negotiation (secure access), execution (receive service), and departure (exit procedures). Negotiation uses four-choice pattern (stat/money/challenge/fallback) determining service quality/cost. Execution delivers service effects (rest restoration, bathhouse benefits, healing). Departure handles exit (immediate rush or careful preparation). Reusable across all service types - inn lodging, bathhouse visits, healer treatments, guide employment - same structure, different execution/rewards.

**Example:** Inn lodging flow. (1) Negotiate: Four choices determining room access and cost (Rapport-gated free, pay 10 coins, Social challenge, wait for charity). (2) Execute: Rest in room (restore Health/Stamina based on quality). (3) Depart: Leave immediately (free, rushed) or prepare carefully (1 time block, thorough). Same pattern applies to bathhouse (replace rest with cleansing) or healer (replace rest with treatment).

**Distinguishing from:** Quest Chain (linear sequence), Transaction (single exchange), Multi-Stage Challenge (tactical). Service flow is narrative three-phase pattern.

**Cross-reference:** Archetype Composition, Scene Archetype, Specialized Archetypes, Design Principle 12

---

## 12.7 Resource Terms

### Persistent Resource

**Definition:** Player resource carrying between challenges and game sessions, requiring restoration through rest or services.

The long-term resource pool tracking player condition across gameplay. Health (injury/vitality), Stamina (physical exhaustion), Focus (mental exhaustion) persist - depleted during challenges, restored through rest/services/items. Depletion creates strategic pressure: cannot chain challenges infinitely, must manage condition, must invest in restoration. Separate from Session Resources (reset per challenge) and Builder Resources (temporary tactical progress).

**Example:** Start day with Health 100/100, Stamina 60/60, Focus 18/18. Complete Physical challenge (Stamina -20, Health -10). Current: Health 90, Stamina 40. Complete Mental challenge (Focus -10). Current: Focus 8. Low resources - cannot afford more challenges without restoration. Must rent lodging (restore all) or risk exhaustion.

**Distinguishing from:** Session Resource (resets per challenge), Builder Resource (tactical progress), HP/MP (generic terms). Persistent resources are specific long-term condition tracking.

**Cross-reference:** Health/Stamina/Focus, Resource Flow, Economic Pressure, Rest/Recovery

---

### Resource Conversion

**Definition:** Translation between different resource types, typically with conversion costs or inefficiencies.

The exchange mechanic enabling resource fluidity. Coins buy Health restoration (money → health). Stamina converts to Exertion for Physical challenges (permanent → session). Focus converts to Attention for Mental challenges (permanent → session). Conversions typically lossy (15 coins → 30 Health, not 1:1) or contextual (Stamina 60 → Exertion 20, conversion ratio). Creates strategic flexibility (can substitute resources) while maintaining scarcity (inefficient conversions prevent perfect optimization).

**Example:** Player has Stamina 40, needs Health restoration. Option A: Rest at inn (8 coins → restore both Health and Stamina). Option B: Buy healing potion (5 coins → restore 30 Health only). Option C: Visit healer (15 coins + 2 time blocks → restore 50 Health + remove injuries). Different conversion rates for different contexts - player chooses based on priorities.

**Distinguishing from:** Resource Trade (exchange), Cost (expenditure), Economy (broader system). Resource conversion is type-to-type translation with rates.

**Cross-reference:** Resource Flow, Resource Arithmetic, Economic Pressure, Tight Margins

---

### Scarcity Philosophy

**Definition:** Design principle that strategic depth emerges from insufficient resources to pursue all desirable options.

The foundational philosophy that meaningful choices require constraints. If player can afford everything, no strategic decisions exist (Cookie Clicker pattern). If resources barely cover essentials, optimization matters, prioritization required, trade-offs forced. Impossible choices emerge: all options valid, insufficient resources for all, must choose which sacrifice acceptable. Applies to coins (tight economic margins), stats (cannot max all five), time (finite segments per day), relationships (separate investment per NPC).

**Example:** Delivery earns 20 coins. Survival costs 15 coins (food + lodging). Net profit 5 coins. Want equipment upgrade (50 coins = 10 days saving), but also emergency fund (20 coins), NPC gift (10 coins), service purchase (25 coins). Cannot afford all. Scarcity forces prioritization: which matters most? Strategic depth emerges from constraint.

**Distinguishing from:** Difficulty (challenge hardness), Balance (fairness), Economy (system structure). Scarcity philosophy is intentional constraint creating depth.

**Cross-reference:** DDR-004 (Tight Economic Margins), Impossible Choice, Resource Arithmetic, Economic Pressure, Design Principle 6

---

### Tactical Resource

**Definition:** Challenge-specific resource used within tactical sessions, not carrying between challenges.

The session-bounded tactical currency. Mental: Progress/Attention/Exposure/Leads. Physical: Breakthrough/Exertion/Danger/Aggression. Social: Momentum/Initiative/Doubt/Cadence. These resources exist only within tactical challenge session - created at session start, determine session outcomes, destroyed at session end. Not persistent like Health/Stamina. Enable tactical depth without affecting strategic resource pools. Each challenge type has distinct tactical resources supporting unique mechanics.

**Example:** Enter Social challenge. Tactical resources initialized: Momentum 0, Initiative 10, Doubt 0. Play cards affecting tactical resources (Momentum +3, Doubt +1). Reach Momentum 12 = victory. Exit challenge. Tactical resources destroyed - Momentum doesn't carry to next challenge. Strategic resources (Health/Stamina/Focus) persist.

**Distinguishing from:** Persistent Resource (carries between challenges), Builder Resource (specific tactical type), Session Resource (specific tactical type). Tactical resource is challenge-temporary category.

**Cross-reference:** Builder Resource, Threshold Resource, Session Resource, Mental/Physical/Social Challenges, Tactical Layer

---

### Universal Resource

**Definition:** Resource type affecting all gameplay systems, creating cross-system competition.

The scarce resource competing across all activities. Time (time blocks per day), Focus (mental capacity for Mental challenges AND NPC conversations), Stamina (physical capacity for Physical challenges AND route travel), Health (vitality affecting all activities), Coins (universal currency). Universal resources create strategic tension: spend Focus on Mental challenge OR NPC conversation? Spend coins on equipment OR lodging? Cannot optimize all systems simultaneously - must prioritize based on goals.

**Example:** Time blocks are universal. Delivery costs 4 blocks. NPC conversation costs 2 blocks. Equipment shopping costs 1 block. Day has 6 blocks total. Can afford delivery (4) + shopping (1) + food (1) = 6 blocks. OR delivery (4) + conversation (2) = 6 blocks. Cannot do all three. Time scarcity forces priority: income OR relationship OR preparation?

**Distinguishing from:** Persistent Resource (long-term), Strategic Resource (decision-layer), Currency (money). Universal resource is cross-system competing resource.

**Cross-reference:** Resource Flow, Scarcity Philosophy, Time Block, Coins, Health/Stamina/Focus, Design Principle 6

---

## 12.8 Terminology Distinctions

This section clarifies commonly confused terms.

### Scene vs Situation

**Scene:** Persistent narrative container in GameWorld.Scenes, owns embedded Situations list, tracks current active Situation, manages state machine (Provisional/Active/Completed/Expired). Multi-situation arc typically 2-4 situations per scene.

**Situation:** Narrative moment embedded in Scene, presents 2-4 choices to player, advances on choice selection, does NOT exist in separate collection.

**Key Difference:** Scene is container persisting in world. Situation is moment within container. Query pattern: `GameWorld.Scenes.SelectMany(s => s.Situations)` NOT `GameWorld.Situations` (doesn't exist).

**Example:** "Secure Lodging" scene (persistent) contains 3 situations (embedded): (1) Negotiate with innkeeper, (2) Rest in room, (3) Depart inn. Scene exists in GameWorld. Situations exist in Scene.Situations property.

**Cross-reference:** ARCHITECTURE.md Entity Ownership, HIGHLANDER Principle, CLAUDE.md Ownership Hierarchy

---

### Choice vs Action

**ChoiceTemplate:** Immutable template embedded in SituationTemplate, defines PathType (Instant/Challenge/Navigate), requirements, costs, rewards. Exists at parse-time/spawn-time.

**Action:** Ephemeral entity created at query-time from ChoiceTemplate, stored in GameWorld.LocationActions/NPCActions/PathCards, rendered in UI, represents executable choice.

**Key Difference:** ChoiceTemplate is template (immutable, embedded). Action is instance (ephemeral, flat GameWorld collection, UI-renderable). ChoiceTemplate → Action happens at query-time (lazy instantiation).

**Example:** SituationTemplate contains ChoiceTemplate "Negotiate diplomatically" (template, immutable). When player enters location, SceneFacade queries active Situation, instantiates LocationAction from template, adds to GameWorld.LocationActions (renderable). Player clicks action, executes choice.

**Cross-reference:** Three-Tier Timing (parse/spawn/query), ARCHITECTURE.md Action Execution, Lazy Instantiation

---

### Template vs Instance

**Template:** Immutable blueprint loaded at parse-time, stored in GameWorld.SceneTemplates/SituationTemplates, reusable across multiple spawns. Defines structure, not state.

**Instance:** Mutable runtime entity spawned from template, stored in GameWorld.Scenes/embedded Situations, tracks progression state, exists until completed/expired.

**Key Difference:** Template is recipe (reusable, immutable, parse-time). Instance is product (unique, mutable, runtime). One template → many instances.

**Example:** SceneTemplate "inn_lodging" (immutable) spawns at 3 different inns → 3 Scene instances (Scene A at Rusty Tankard, Scene B at Golden Lion, Scene C at Traveler's Rest). Same template, different instances with separate state.

**Cross-reference:** Parse-Time vs Runtime, ARCHITECTURE.md Procedural Generation, Entity Lifecycle

---

### Archetype vs Pattern

**Archetype:** Reusable mechanical template generating concrete content. Scene Archetype (situation flow structure) or Situation Archetype (choice generation pattern). Specific to Wayfarer content generation system.

**Pattern:** General design approach or recurring structure. Can be architectural (Catalogue Pattern), gameplay (Four-Choice Pattern), or narrative (Service Flow Pattern). Broader term than archetype.

**Key Difference:** Archetype is content generation template. Pattern is design approach/structure. All archetypes are patterns, not all patterns are archetypes.

**Example (Archetype):** "negotiation" Situation Archetype generates 4 choices. Specific content generation template.

**Example (Pattern):** Four-Choice Pattern is design approach ensuring progression. General principle applying beyond specific archetype.

**Cross-reference:** Archetype, Design Pattern, Content Generation

---

### Placement vs Ownership

**Ownership:** Parent creates and destroys child, lifecycle dependency. Scene owns Situations (embedded in Scene.Situations). GameWorld owns Scenes (collection). If parent destroyed, children destroyed.

**Placement:** Entity appears at location/NPC for UI/narrative purposes, NO lifecycle dependency. Scene placed at Location (PlacementType = Location, PlacementId = location ID). Location NOT owner - Location deleted doesn't destroy Scene.

**Key Difference:** Ownership = lifecycle control. Placement = context/presentation. Test: If A destroyed, should B be destroyed? Yes = ownership, No = placement.

**Example:** Scene A placed at Location X. Location X deleted. Scene A persists (ownership: GameWorld.Scenes). Scene B contains Situation Y. Scene B deleted. Situation Y deleted (ownership: Scene.Situations embedded).

**Cross-reference:** Design Principle 3 (Ownership vs Placement vs Reference), ARCHITECTURE.md Entity Hierarchy, CLAUDE.md Critical Investigation

---

### Stat Gating vs Boolean Gate

**Stat Gating:** Transparent numeric comparison (Insight >= 3) enabling player planning. Player sees exact requirement, exact gap, can calculate what needed. Resource arithmetic pattern.

**Boolean Gate:** Hidden flag check (player.HasHighInsight) hiding content behind mystery condition. Player doesn't see threshold, cannot plan, arbitrary unlock. Anti-pattern violating Requirement Inversion.

**Key Difference:** Stat gating is transparent arithmetic (perfect information). Boolean gate is opaque flag (mystery unlock). Stat gating enables strategy, boolean gate creates checklist.

**Example (Stat Gating):** "Requires Insight 3, you have 2, need 1 more." Exact, plannable, strategic.

**Example (Boolean Gate):** "You need to be more insightful." How insightful? Mystery. Cannot plan.

**Cross-reference:** Resource Arithmetic, Boolean Gate Anti-Pattern, DDR-002 (Tag-Based vs Boolean), Perfect Information

---

### Strategic Decision vs Tactical Execution

**Strategic Decision:** WHAT to attempt - which scene to pursue, which route to travel, which choice to select. Made at strategic layer with perfect information (all costs/rewards/requirements visible). Planning, risk assessment, resource allocation.

**Tactical Execution:** HOW to win - which cards to play, which resources to build, when to push or consolidate. Made at tactical layer with hidden complexity (draw order unknown). Skill demonstration, adaptation, resource management.

**Key Difference:** Strategic = WHAT (informed decision with perfect info). Tactical = HOW (execution skill with emergent complexity). Strategic happens first, tactical happens second. Bridge separates.

**Example (Strategic):** "Should I attempt this Social challenge? I have 30 Resolve, challenge costs 20, if I fail I lose relationship progress. Risk acceptable?" Decision made with full information.

**Example (Tactical):** "Which Social card should I play now? Initiative limited, Doubt rising, Momentum almost at threshold. Push for victory or consolidate position?" Execution within challenge.

**Cross-reference:** Strategic Layer, Tactical Layer, DDR-005 (Layer Separation), Bridge

---

### A-Story vs Main Quest

**A-Story:** Wayfarer's infinite procedural main thread. Never ends, never resolves, journey itself is content. Sequential tutorial (A1-A10) then procedural continuation (A11 → ∞). Player controls WHEN to pursue, not IF. Optional pacing.

**Main Quest:** Traditional RPG term implying ending. Linear progression toward climax and resolution. Finite content, natural stopping point after completion. Post-ending gameplay awkward.

**Key Difference:** A-Story is infinite spine (never ends). Main Quest is finite arc (ends). A-Story eliminates ending pressure, Main Quest requires ending. A-Story supports eternal traveler fantasy, Main Quest requires resolution.

**Example (A-Story):** Complete A-story scene → suggests next place → travel → arrive → new scene → cycle continues infinitely. Never pressure to "finish."

**Example (Main Quest):** Complete quest chain → defeat final boss → kingdom saved → game ends → post-game content feels hollow.

**Cross-reference:** DDR-001 (Infinite A-Story), Frieren Principle, Infinite Journey, Narrative Philosophy

---

### Economic Pressure vs Difficulty

**Economic Pressure:** Strategic constraint from tight resource margins forcing prioritization. Delivery earnings barely cover survival (3-7 coins profit). Cannot afford everything, must optimize, trade-offs required. Strategic depth from scarcity.

**Difficulty:** Challenge hardness or complexity. How hard is this Physical obstacle? How complex is this Mental investigation? Tactical execution skill required, not strategic resource management.

**Key Difference:** Economic pressure is strategic scarcity (resource decisions). Difficulty is tactical challenge (execution skill). Can have high pressure with low difficulty (easy challenges, tight resources), or low pressure with high difficulty (hard challenges, generous resources).

**Example (Economic Pressure):** "I can afford lodging OR equipment, not both. Which sacrifice acceptable?" Strategic resource decision.

**Example (Difficulty):** "This Physical challenge is hard - high Breakthrough threshold, harsh Danger accumulation." Tactical execution challenge.

**Cross-reference:** DDR-004 (Tight Economic Margins), Economic Pressure, Resource Scarcity, Challenge Difficulty

---

## 12.9 Summary

This glossary provides canonical game design term definitions for Wayfarer. For technical implementation terms (parsing, facades, entities, architecture patterns), see arc42 section 12_glossary.md.

**Term Categories:**
- **Core Gameplay (18 terms)**: Player-facing concepts, core loops, economic systems
- **Narrative (10 terms)**: Story structure, content generation, fictional frameworks
- **Progression (5 terms)**: Advancement systems, learning mechanics, build development
- **Challenge (14 terms)**: Tactical systems, resource types, session models
- **Design Patterns (6 terms)**: Reusable structures, anti-patterns, architectural approaches
- **Resources (5 terms)**: Resource types, flows, scarcity principles
- **Distinctions (9 terms)**: Commonly confused term pairs with clarifications

**Total Terms Defined:** 67 game design concepts

**Usage Guidelines:**
- When term appears in design documentation, this glossary is authoritative
- For technical terms, defer to arc42 section 12_glossary.md
- When confusion arises, consult Distinctions section
- Cross-references connect related concepts

---

## Related Documentation

**Game Design Documentation:**
- **01_game_vision.md** - Vision and goals informing terminology
- **02_core_principles.md** - Design principles referenced in definitions
- **11_design_decisions.md** - DDRs implementing these concepts

**Technical Documentation (arc42):**
- **12_glossary.md** - Technical implementation terms
- **05_building_block_view.md** - System architecture implementing game design
- **06_runtime_view.md** - Execution flow for game design concepts

**Source Documentation:**
- **DESIGN_PHILOSOPHY.md** - Foundational principles
- **WAYFARER_CORE_GAME_LOOP.md** - Gameplay loop details
- **REQUIREMENT_INVERSION_PRINCIPLE.md** - Resource arithmetic philosophy
