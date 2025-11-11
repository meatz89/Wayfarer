# Section 9: Design Patterns and Reusable Structures

## 9.1 Overview

Wayfarer's content architecture relies on reusable patterns that enable infinite variation while maintaining structural guarantees. This section documents the patterns that repeat across the game, distinguishing between foundational patterns (how systems work), content patterns (how content is structured), and anti-patterns (what to avoid).

This section documents:
- **Core Design Patterns**: Strategic-Tactical separation, Four-Choice Archetype, Orthogonal Resource Costs
- **Content Design Patterns**: Service flows, investigation structures, branching choices, multi-phase mysteries
- **Reusability Patterns**: Archetype composition, property-driven generation, placement filters, spawn conditions
- **Anti-Patterns to Avoid**: Boolean gates, overlapping costs, hidden gotchas, soft-lock paths
- **Pattern Selection Guide**: When to use which pattern
- **Pattern Testing**: How to validate pattern application

## 9.2 Core Design Patterns

### 9.2.1 Strategic-Tactical Separation

**Problem**: How to balance perfect information (player can plan) with emergent gameplay (execution matters)?

**Solution**: Two distinct layers with explicit bridge. Strategic layer provides perfect information for planning. Tactical layer provides hidden complexity for execution.

**Pattern Structure**:

**Strategic Layer** (WHAT and WHERE):
- Flow: Obligation → Scene → Situation → Choice
- Information: All visible (costs, rewards, requirements, outcomes)
- Entities: Persistent (Scenes exist in GameWorld, track state)
- Decision: WHETHER to attempt (player calculates affordability)

**Tactical Layer** (HOW):
- Flow: Challenge Session → Card Play → Resource Accumulation → Victory/Failure
- Information: Hidden complexity (card draw order, exact challenge flow)
- Entities: Temporary (sessions exist during challenge only, destroyed after)
- Execution: HOW to execute (player demonstrates skill)

**The Bridge** (ChoiceTemplate.ActionType):
- **Instant**: Stay in strategic layer (apply costs/rewards immediately)
- **Navigate**: Stay in strategic layer (move to location/NPC)
- **StartChallenge**: Cross to tactical layer (spawn Mental/Physical/Social challenge session)

**Why This Works**:
- Player plans strategically (I can afford this challenge, my Insight is high enough)
- Player executes tactically (plays cards to maximize Progress, manages Attention)
- Failure at tactical layer returns to strategic layer with consequences (injury, resource depletion)
- Strategic choice remains meaningful (bad plan = tactical struggle or failure)

**Example**:

Strategic layer: Scene presents "Investigate mill interior"
- Cost: 2 time blocks entry, 12 Focus to make progress
- Requirement: Insight ≥ 3 for optimal approach
- Success outcome: Gain 2 Understanding, spawn next investigation phase
- Failure outcome: Partial progress, can retry after rest

Player calculates: "I have 18 Focus, Insight 5. I can afford this and I'm qualified for optimal approach. I'll attempt."

Tactical layer: Mental challenge begins
- Player draws cards (bound to Insight, Cunning)
- Plays ACT cards (spend Attention, generate Leads, build Progress)
- Plays OBSERVE cards (follow Leads, draw Details)
- Accumulates Progress toward threshold (15 Progress needed)
- Manages Exposure risk (staying too long increases difficulty)

Outcome: Reached 17 Progress (success), used 14 Focus (efficient), gained 2 Understanding

Return to strategic layer with results applied. Next decision: Continue story, rest, pursue side content?

### 9.2.2 Four-Choice Archetype (Guaranteed Progression)

**Problem**: How to ensure player can always progress (no soft-locks) while maintaining strategic choice?

**Solution**: Every A-story situation presents four choice types with orthogonal resource costs. At least one choice ALWAYS available and ALWAYS progresses.

**Pattern Structure**:

**Choice 1: Stat-Gated Path (Optimal)**
- PathType: InstantSuccess
- Requirement: Player stat ≥ threshold (Rapport 5, Authority 4, Insight 6, etc.)
- Cost: Free (no consumable resources)
- Outcome: Best rewards (player invested in character build)
- Purpose: Reward specialization and preparation

**Choice 2: Money-Gated Path (Reliable)**
- PathType: InstantSuccess
- Requirement: None (always visible)
- Cost: Coins (expensive but affordable via B-story income)
- Outcome: Good rewards (reliable, efficient resolution)
- Purpose: Reward economic accumulation from side content

**Choice 3: Challenge Path (Risky)**
- PathType: Challenge (Social, Mental, or Physical)
- Requirement: None (always visible)
- Cost: Resolve/Stamina/Focus (session resource)
- Outcome: Variable (success = excellent rewards, failure = setback BUT STILL ADVANCES)
- Purpose: Tactical gameplay integration, skill expression

**Choice 4: Fallback Path (Patient)**
- PathType: InstantSuccess or Navigate
- Requirement: None (always visible, always selectable)
- Cost: Time (wait days, help with needs, persistent gentle effort)
- Outcome: Minimal rewards, poor efficiency, but GUARANTEED progression
- Purpose: Prevent soft-locks, reflect realistic relationship building

**Critical Guarantee**: Choice 4 MUST have zero requirements, cannot fail, and MUST advance progression. This prevents soft-locks.

**Why This Works**:
- Player chooses HOW to progress, not IF they progress
- Different builds prefer different paths (stat specialist uses stat paths, economic player uses money paths)
- Orthogonal costs create genuine trade-offs (stat/money/time/risk)
- Fallback ensures forward progress even in worst-case scenarios

**Example**:

Scene: "Gain innkeeper's trust for information"

**Choice 1: Rapport Path** (Stat-Gated)
- Requirement: Rapport ≥ 4
- Cost: Free
- Outcome: Innkeeper shares information willingly, relationship +2, next scene spawns
- Player: High Rapport build, path unlocked, obvious choice

**Choice 2: Money Path** (Reliable)
- Requirement: None
- Cost: 15 coins (generous "tip" for information)
- Outcome: Innkeeper shares information, relationship +1, next scene spawns
- Player: Low Rapport build with coins, reliable alternative

**Choice 3: Social Challenge** (Risky)
- Requirement: None
- Cost: 3 Resolve (enter Social challenge)
- Success: Information gained, relationship +2, Understanding +1, next scene spawns
- Failure: Information gained but incomplete, relationship unchanged, next scene spawns (different entry state)
- Player: Skilled tactician, wants best outcome via skill demonstration

**Choice 4: Help Path** (Guaranteed)
- Requirement: None
- Cost: 4 time blocks (help clean common room, listen to stories)
- Outcome: Innkeeper gradually shares information, relationship +1, next scene spawns
- Player: Zero coins, low Rapport, will use time to build trust naturally

All four paths advance to next scene. Different costs. Different experiences. No soft-lock possible.

### 9.2.3 Orthogonal Resource Costs

**Problem**: How to ensure all choices remain viable (not dominated by strictly better alternatives)?

**Solution**: Each choice costs DIFFERENT resource type. Player selects based on current resource availability and strategic priorities.

**Pattern Structure**:

**Resource Types**:
1. **Character Build** (permanent investment): Stats, specialized capabilities
2. **Consumable Economy** (depletable): Coins, items
3. **Session Resources** (tactical capacity): Resolve, Stamina, Focus
4. **Time** (opportunity cost): Segments, days
5. **Narrative** (story consequence): Relationships, reputation

**Orthogonality Test**: Do two choices cost the same resource type? If yes, one dominates (false choice).

**CORRECT Example**:
- Choice A: Rapport 5 required (character build)
- Choice B: 15 coins cost (consumable economy)
- Choice C: 3 Resolve cost (session resource)
- Choice D: 3 time blocks cost (opportunity cost)

All four cost DIFFERENT resources. Player selects based on what they have and what they can spare. No universal best choice.

**INCORRECT Example** (FORBIDDEN):
- Choice A: 5 coins → Basic outcome
- Choice B: 10 coins → Better outcome
- Choice C: Social challenge → Variable outcome
- Choice D: Free → Poor outcome

Choices A and B both cost coins. If player has 10 coins, Choice B strictly dominates Choice A (better outcome, player can afford). Choice A becomes dead option. False choice.

**Why This Works**:

Shared resource = Direct comparison:
- Player compares numerical efficiency
- Higher-cost option must provide proportionally better value
- One option usually dominates
- Dominated options rarely chosen

Orthogonal resources = Strategic trade-off:
- Player compares availability and priorities
- No direct numerical comparison possible
- Context determines best choice
- All options remain viable in different situations

**Example**:

Scene: "Secure private meeting space"

Orthogonal design (CORRECT):
- Choice A: Authority 5 → Command private room (character build)
- Choice B: 20 coins → Rent private room (consumable economy)
- Choice C: Social challenge → Negotiate private room (session resource + skill)
- Choice D: 4 time blocks → Help owner, earn private room (opportunity cost)

Player situation 1: Authority 6, 5 coins, 10 Resolve, 8 segments available
- Choice A: Unlocked, free, instant (obvious choice)

Player situation 2: Authority 2, 25 coins, 10 Resolve, 8 segments available
- Choice B: Affordable, reliable (likely choice)

Player situation 3: Authority 2, 5 coins, 10 Resolve, 8 segments available
- Choice C or D: Can't use A (locked) or B (too expensive), uses C (tactical skill) or D (time abundant)

All four choices viable in different contexts. Orthogonal costs create genuine strategic variety.

### 9.2.4 Tag-Based Dependencies (State-Based Content Spawning)

**Problem**: How to create narrative progression without hardcoded linear chains?

**Solution**: Content spawns when player state includes required tags. State accumulated through rewards, creating organic flow.

**Pattern Structure**:

**SpawnConditions**:
- RequiredTags: List of tags player must have
- ForbiddenTags: List of tags player must NOT have
- RequiredSceneCompletions: List of scene IDs player must have completed
- Other filters: Location, time, cooldowns

**StateApplicationReward**:
- TagsToApply: List of tags added to player state
- TagsToRemove: List of tags removed from player state

**Content Flow**:
1. Player completes Scene A
2. Scene A rewards include: TagsToApply = ["investigated_mill", "knows_corruption"]
3. Scene B has SpawnConditions: RequiredTags = ["investigated_mill"]
4. Scene B now eligible (player has required tag)
5. Scene B spawns when other conditions met (location, cooldown)

**Why This Works**:
- No hardcoded A→B chains (flexible, reorderable)
- State-based eligibility (content appears when contextually appropriate)
- Multiple paths to same state (different routes acquire same tags)
- Graceful handling of non-linear progression

**Example**:

**Scene A**: "Investigate mill exterior"
- Completion rewards: TagsToApply = ["investigated_mill_exterior"]

**Scene B**: "Investigate mill interior"
- SpawnConditions: RequiredTags = ["investigated_mill_exterior"]
- Won't spawn until exterior investigated
- But if player investigated exterior via DIFFERENT scene, still eligible

**Scene C**: "Confront mill owner"
- SpawnConditions: RequiredTags = ["investigated_mill_exterior", "discovered_evidence"]
- Requires BOTH tags (investigation + evidence)
- Could acquire "discovered_evidence" from multiple sources

**Scene D**: "Report findings to constable"
- SpawnConditions: RequiredTags = ["knows_corruption"], ForbiddenTags = ["allied_with_corrupt"]
- Eligible if player knows about corruption but hasn't allied with corrupt faction
- State branching via forbidden tags

This creates web of possibilities, not linear railroad. State determines eligibility, player explores naturally.

## 9.3 Content Design Patterns

### 9.3.1 Service Flow Pattern (3-4 Situation Linear Arc)

**Problem**: How to structure service transactions (lodging, healing, bathing) with consistent mechanical pattern?

**Solution**: Three-phase flow: Negotiation → Execution → Departure. Linear transitions, service context self-contained.

**Pattern Structure**:

**Phase 1: Negotiation** (service_negotiation archetype)
- Location: Service provider's public area (inn common room, healer's waiting room)
- Four choices: Stat-gated discount, standard payment, challenge path, help-based free
- Outcome: Secure access to service, generate dependent resource (room key, treatment token)
- Transition: Always → Phase 2 (all choices succeed in securing service)

**Phase 2: Execution** (service_execution_rest archetype)
- Location: Service location (private room, treatment chamber)
- Context: Use generated resource (room key grants access)
- Four variants: Balanced rest, physical focus, mental focus, special service
- Outcome: Resource restoration (Health/Stamina/Focus recovered)
- Transition: Always → Phase 3 (service consumed, restoration applied)

**Phase 3: Departure** (service_departure archetype)
- Location: Service location or provider area
- Two choices: Immediate departure, careful/grateful departure
- Outcome: Minor relationship effects, return to public area
- Transition: Scene completes, player returns to location

**Why This Works**:
- Consistent three-phase structure across all service types
- Phase 1 uses four-choice pattern (guaranteed progression)
- Phase 2 provides restoration choice (player optimizes recovery type)
- Phase 3 allows relationship building (grateful departure improves NPC bond)
- Self-contained (generates resources needed for own execution)

**Example: Inn Lodging Service**

**Situation 1: Negotiate Room** (common room)
- Rapport path: Friendly negotiation → Discount (3 coins)
- Money path: Standard payment (8 coins)
- Social challenge: Persuade for better rate
- Help path: Clean common room for free room
- All choices: Generate "room_key" dependent resource
- Transition: Always → Situation 2

**Situation 2: Rest in Room** (private room, requires room_key)
- Balanced rest: Restore Health 15, Stamina 15, Focus 10
- Physical focus: Restore Health 20, Stamina 25, Focus 5
- Mental focus: Restore Health 10, Stamina 10, Focus 20
- Special service: Restore all + Understanding +1 (premium)
- Transition: Always → Situation 3

**Situation 3: Morning Departure** (private room)
- Quick departure: Leave immediately, no relationship change
- Grateful departure: Thank innkeeper, relationship +1, gather morning rumors
- Transition: Scene completes

**Reusability**: Same pattern for healer services (negotiate treatment, receive treatment, depart), bathhouse services (negotiate access, bathe with variant focus, depart), guide services (negotiate hire, receive guidance during travel, part ways).

### 9.3.2 Investigation Pattern (Hub-and-Spoke Structure)

**Problem**: How to structure investigative content with multiple leads converging to solution?

**Solution**: Central hub situation with 3-4 parallel investigation threads. Player gathers evidence from multiple sources, threads converge to conclusion situation.

**Pattern Structure**:

**Hub Situation**: "Begin investigation"
- Present investigation context and goal
- Four choices: Three investigation paths + one fallback
- Each path leads to parallel investigation situation
- Player explores in any order

**Spoke Situations**: "Investigate Lead A/B/C"
- Each accessible after hub (OnSuccess transitions)
- Can be completed in any order
- Each grants evidence tag or understanding increment
- Each returns to hub OR advances to convergence

**Convergence Situation**: "Draw conclusions"
- SpawnConditions: Requires evidence from 2+ spokes
- Player analyzes gathered evidence
- Four choices: Different deductive approaches based on evidence quality
- Leads to next story phase

**Why This Works**:
- Non-linear (player chooses investigation order)
- Completionist-friendly (can gather all evidence or just enough)
- Evidence accumulation visible (tags show what player knows)
- Convergence natural (player has context for conclusions)

**Example: Mill Investigation**

**Hub**: "Arrive at mill, assess situation"
- Choice A: Investigate exterior (Insight 4)
- Choice B: Interview workers (Rapport 3)
- Choice C: Examine records (search administrative office)
- Choice D: Watch from distance (slow, guaranteed info)
- Transitions: A→Exterior, B→Workers, C→Records, D→Observation

**Spoke 1**: "Investigate exterior" (Physical inspection)
- Examine structural damage
- Find evidence of sabotage
- Grant tag: "discovered_sabotage_evidence"
- Transition: Return to hub OR proceed to convergence

**Spoke 2**: "Interview workers" (Social inquiry)
- Talk with mill workers about recent events
- Learn about tensions with owner
- Grant tag: "knows_worker_grievances"
- Transition: Return to hub OR proceed to convergence

**Spoke 3**: "Examine records" (Mental analysis)
- Review financial documents
- Discover irregular payments
- Grant tag: "found_financial_irregularities"
- Transition: Return to hub OR proceed to convergence

**Convergence**: "Determine cause" (requires 2+ evidence tags)
- If has all 3 tags: Full picture, optimal conclusion
- If has 2 tags: Partial picture, adequate conclusion
- If has 1 tag: Minimal info, poor conclusion
- All advance story, quality varies

### 9.3.3 Branching Choice Pattern (OnSuccess/OnFailure Divergence)

**Problem**: How to create meaningful consequence divergence based on player choices?

**Solution**: Situation choices specify different transitions for success vs failure outcomes. Both branches advance story, but player experiences different narrative based on performance.

**Pattern Structure**:

**Situation with Branching**:
- Choice with Challenge path
- OnSuccess transition: Route to "Success branch" situation
- OnFailure transition: Route to "Failure branch" situation
- Both branches eventually converge OR lead to distinct story arcs

**Success Branch**:
- Player succeeded at challenge
- Narrative reflects competence
- Better rewards, easier subsequent situations
- May unlock optimal paths later

**Failure Branch**:
- Player failed at challenge
- Narrative reflects setback
- Reduced rewards, harder subsequent situations
- Still progresses (no soft-lock), alternative approach required

**Convergence** (optional):
- Branches rejoin at later situation
- Narrative acknowledges different approaches
- State reflects earlier success/failure (tags, resources, relationships)

**Why This Works**:
- Consequences visible and meaningful
- Player sees direct result of tactical performance
- No soft-lock (failure branch still progresses)
- Replayability (different branches on different playthroughs)

**Example: Confront Magistrate**

**Situation 1**: "Decide approach"
- Choice A: Authority 6 → Command respect (instant success)
- Choice B: 25 coins → Bribe for cooperation (instant success)
- Choice C: Social challenge → Persuade through conversation
  - OnSuccess: Transition to "Magistrate Cooperates"
  - OnFailure: Transition to "Magistrate Refuses"
- Choice D: Leave for now (fallback)

**Success Branch**: "Magistrate Cooperates"
- Magistrate shares information willingly
- Gain 3 Understanding, magistrate relationship +2
- Unlock "Official Support" tag (helps in later scenes)
- Transition: Next story phase with ally

**Failure Branch**: "Magistrate Refuses"
- Magistrate hostile, refuses cooperation
- Gain 1 Understanding (learned hostility tells story)
- Magistrate relationship -1, lose time finding alternative
- Transition: Next story phase with obstacle

**Convergence**: "Proceed with investigation"
- If has "Official Support" tag: Easier path available (magistrate provides resources)
- If lacks tag: Harder path required (must work around magistrate)
- Both reach same story phase, different difficulty

### 9.3.4 Multi-Phase Mystery Pattern (Obligation → Scenes → Completion)

**Problem**: How to structure long-term narrative arcs that span multiple locations and provide sense of progression?

**Solution**: Obligation entity spawns Scenes sequentially. Each Scene completion advances phase counter. Mystery deepens with each phase, never fully resolves (infinite continuation).

**Pattern Structure**:

**Obligation**: Persistent mystery container
- Tracks CurrentPhase (1, 2, 3... infinite)
- Spawns Scene based on phase
- Never completes (infinite procedural continuation)

**Phase 1-3**: Authored tutorial scenes
- Hand-crafted, teach mechanics
- Introduce mystery framework
- Establish pursuit goal
- Trigger procedural continuation at completion

**Phase 4+**: Procedural scenes
- Generated via archetype selection
- Reference earlier phases organically
- Deepen mystery rather than resolve
- Escalate scope (local → regional → continental → cosmic)

**Scene Completion**:
- Increments Obligation.CurrentPhase
- Spawns next Scene (phase + 1)
- Applies rewards (unlock locations, grant tags)
- Mystery continues

**Why This Works**:
- Long-term narrative spine (always "next scene" available)
- Progressive complexity (early scenes simple, later scenes complex)
- Geographic variety (each scene different location/context)
- Never ends (infinite procedural generation)

**Example: The Infinite Journey**

**Obligation**: "The Wayfarer's Path"
- CurrentPhase: 1 (at game start)
- Description: "You travel. You arrive. You meet. You continue."

**Phase 1** (A1): "Arrive at roadside inn" (authored)
- Tutorial: Lodging service, basic negotiation
- Completion: Phase → 2, unlock Westmarch region

**Phase 2** (A2): "Meet constable in Westmarch" (authored)
- Tutorial: Investigation basics, evidence gathering
- Completion: Phase → 3, unlock Northreach region

**Phase 3** (A3): "Encounter traveling scholar" (authored)
- Tutorial: Complex multi-situation arc, relationship building
- Completion: Phase → 4, trigger procedural continuation

**Phase 4** (A4): "Seek elder in mountain village" (procedural)
- Generated: arrival archetype + mountain context
- Deepens: References earlier journey organically
- Completion: Phase → 5, generate A5

**Phase 5+**: Infinite continuation
- Each scene references player journey
- Mystery deepens (what drives the endless road?)
- Scope escalates (local → regional → continental)
- Never resolves (journey itself IS the point)

### 9.3.5 Arrival Pattern (Travel Rhythm)

**Problem**: How to structure the repeating cycle of arrival, exploration, engagement, departure?

**Solution**: Four-phase pattern that creates natural rhythm for travel-based game. Each arrival feels similar (consistent structure) yet unique (different context).

**Pattern Structure**:

**Phase 1: Arrival** (1 situation)
- Player reaches new location after travel
- Assess situation, understand context
- Choose where to go within venue (spots)
- Transition: Always → Phase 2 (exploration)

**Phase 2: Exploration** (1-2 situations)
- Look around, meet people, gather information
- Multiple paths available (who to talk to, what to examine)
- Build understanding of local context
- Transition: Always → Phase 3 (engagement)

**Phase 3: Engagement** (2-3 situations)
- Core interaction (help with problem, negotiate service, investigate mystery)
- Choice consequences shape narrative
- Branching possible (success/failure branches)
- Transition: Always → Phase 4 (departure)

**Phase 4: Departure** (1 situation)
- Resolve local situation, prepare to leave
- Hear about next destination (natural hook)
- Choose when to leave vs stay longer
- Transition: Scene completes, return to location

**Why This Works**:
- Familiar rhythm (player knows pattern)
- Variety within structure (different engagement content)
- Natural hooks (each departure suggests next arrival)
- Pacing (rise and fall of tension)

**Example: Inn Arrival Arc**

**Phase 1: Arrive at inn**
- Weather-beaten from travel, seek shelter
- Assess inn (quality, occupants, atmosphere)
- Choose: Enter common room, check stable first, observe from outside
- Transition: Always → Phase 2

**Phase 2: Explore common room**
- Look around, notice occupants
- Talk with innkeeper, travelers, locals
- Gather information about region, rumors, opportunities
- Transition: Always → Phase 3

**Phase 3: Secure lodging** (service flow)
- Negotiate room (four-choice pattern)
- Rest and recover
- Morning conversations
- Transition: Always → Phase 4

**Phase 4: Prepare departure**
- Innkeeper mentions neighboring town
- "Traveler headed north mentioned strange happenings"
- Choose: Leave immediately, stay another day, ask more questions
- Transition: Scene completes, A-story continues

## 9.4 Reusability Patterns

### 9.4.1 Archetype Composition (Scene Archetypes Compose Situation Archetypes)

**Problem**: How to build complex multi-situation scenes from reusable components?

**Solution**: Scene archetypes define structure (situation count, transition pattern). Each situation uses situation archetype (4-choice pattern, path types, costs/rewards). Scene = composition of situation archetypes.

**Pattern Structure**:

**Scene Archetype**:
- Defines: Situation count (3, 4, 5)
- Defines: Situation sequence (linear, branching, hub-and-spoke)
- Defines: Transition pattern (Always, OnSuccess/OnFailure, conditional)
- Defines: Required entities (NPC types, location properties)
- References: Situation archetype IDs (which archetypes for which situations)

**Situation Archetype**:
- Defines: Choice count (4 for A-story, 2-4 for B-story)
- Defines: Path types (stat/money/challenge/fallback distribution)
- Defines: Cost formulas (how to calculate stat thresholds, coin costs)
- Defines: Reward formulas (what resources granted)
- Generates: ChoiceTemplates (concrete choices at parse-time)

**Composition**:
```
inn_lodging scene archetype:
  Situation 1: service_negotiation archetype
  Situation 2: service_execution_rest archetype
  Situation 3: service_departure archetype

Each situation archetype generates 4 ChoiceTemplates
Total: 12 choices across 3 situations
```

**Why This Works**:
- Scene archetypes reusable (inn_lodging applies to all inns)
- Situation archetypes reusable (service_negotiation applies to all services)
- Composition creates variety (3 × 4 = 12 combinations just from situation order)
- Add entity properties (Friendly vs Hostile, Basic vs Premium) = infinite variations

**Example**:

**Scene Archetype**: "inn_lodging" (service flow)
- Situation 1: "service_negotiation" archetype (4 choices)
- Situation 2: "service_execution_rest" archetype (4 rest variants)
- Situation 3: "service_departure" archetype (2 choices)

**Scene Archetype**: "healer_treatment" (service flow)
- Situation 1: "service_negotiation" archetype (4 choices, same as inn)
- Situation 2: "service_execution_healing" archetype (4 healing variants)
- Situation 3: "service_departure" archetype (2 choices, same as inn)

**Reuse**: Both scenes use service_negotiation and service_departure. Only middle situation differs (rest vs healing). Massive reuse via composition.

### 9.4.2 Property-Driven Generation (Same Archetype + Different Properties = Contextual Variation)

**Problem**: How to generate infinite variations without infinite archetypes?

**Solution**: Archetypes define mechanical structure (choice count, path types, formulas). Entity properties scale values contextually (Friendly = easier, Premium = expensive). Same archetype + different properties = appropriate difficulty for context.

**Pattern Structure**:

**Archetype Defines**:
- BaseStatThreshold (e.g., 5)
- BaseCoinCost (e.g., 8)
- BaseChallengeThreshold (e.g., 15)
- PathTypes (stat/money/challenge/fallback)

**Entity Properties Scale**:
- NPCDemeanor: Friendly = 0.6×, Hostile = 1.4×
- Quality: Basic = 0.6×, Premium = 1.6×, Luxury = 2.4×
- PowerDynamic: Dominant = 0.6×, Submissive = 1.4×
- EnvironmentQuality: Basic = 1.0×, Standard = 2.0×, Premium = 3.0×

**Generation Process**:
1. Select archetype (service_negotiation)
2. Resolve entities (Elena the innkeeper)
3. Read entity properties (Friendly, Standard quality)
4. Apply scaling formulas
5. Generate concrete ChoiceTemplates with scaled values

**Result**: Same archetype, different numbers, contextually appropriate.

**Why This Works**:
- Archetypes reusable (21 archetypes cover all situation types)
- Properties provide variation (3 × 4 × 3 × 3 = 108 property combinations)
- Scaling maintains balance (Friendly ALWAYS easier than Hostile)
- AI can author properties (describes "Friendly premium inn", doesn't calculate numbers)

**Example**:

**service_negotiation archetype** applied to two contexts:

**Context 1**: Friendly innkeeper, Basic quality
```
BaseStatThreshold = 5
BaseCoinCost = 8

Scaling:
  NPCDemeanor.Friendly = 0.6×
  Quality.Basic = 0.6×

Result:
  StatThreshold = 5 × 0.6 = 3 (easy)
  CoinCost = 8 × 0.6 = 5 (cheap)
```

**Context 2**: Hostile merchant, Luxury quality
```
BaseStatThreshold = 5
BaseCoinCost = 8

Scaling:
  NPCDemeanor.Hostile = 1.4×
  Quality.Luxury = 2.4×

Result:
  StatThreshold = 5 × 1.4 = 7 (hard)
  CoinCost = 8 × 2.4 = 19 (expensive)
```

Same archetype (service_negotiation). Different properties. Appropriate difficulty for context.

### 9.4.3 Placement Filters (Pure Configuration Determines WHERE Patterns Appear)

**Problem**: How to control where content appears without hardcoding locations?

**Solution**: Scene templates define placement filters (location properties, NPC types, venue types). Content spawns wherever filters match. No hardcoded location IDs.

**Pattern Structure**:

**Placement Configuration**:
- PlacementType: Location, NPC, Route, or None
- PlacementFilters: Categorical properties required
  - LocationProperties: Secure, Dangerous, Sacred, Commerce, Governance
  - NPCPersonality: Authority, Merchant, Scholar, Warrior, Mystic
  - VenueType: Inn, Palace, Temple, Market, Dungeon
  - Quality: Basic, Standard, Premium, Luxury
  - Tier: 1-4 (local → cosmic scope)

**Spawn Logic**:
1. Scene eligible (spawn conditions met)
2. Query entities matching placement filters
3. Select from matching entities (anti-repetition preferred)
4. Spawn scene at selected entity
5. Scene appears in entity's context

**Why This Works**:
- Content portable (not tied to specific locations)
- Reusable (same scene template spawns at multiple matching locations)
- AI-friendly (describes categories, not concrete IDs)
- Enables procedural world expansion (new locations auto-compatible)

**Example**:

**Scene Template**: "Seek official permission"
- PlacementType: Location
- PlacementFilters:
  - LocationProperties: [Governance]
  - VenueType: [Palace, Courthouse, Administrative]
  - Tier: 2+ (regional importance required)

**Matching Locations**:
- Westmarch Constable's Office (Governance, Administrative, Tier 1) - NO (Tier too low)
- Northreach Governor's Hall (Governance, Palace, Tier 2) - YES
- Capital Palace (Governance, Palace, Tier 3) - YES

Scene can spawn at either Northreach or Capital Palace. Selection based on: Player's current region, anti-repetition (haven't used recently), story context.

**Flexibility**: Add new Tier 2+ Governance location anywhere in world, scene auto-eligible to spawn there. Content scales with world expansion.

### 9.4.4 Spawn Conditions (Pure Configuration Determines WHEN Patterns Eligible)

**Problem**: How to control when content appears without hardcoded triggers?

**Solution**: Scene templates define spawn conditions (required tags, scene completions, cooldowns). Content becomes eligible when conditions met. State accumulation determines timing.

**Pattern Structure**:

**Spawn Conditions**:
- RequiredTags: List of player tags required
- ForbiddenTags: List of player tags that block eligibility
- RequiredSceneCompletions: List of scene IDs player must have completed
- CooldownDays: Minimum days since last scene of this type
- MinimumDay: Earliest day number when eligible
- MaximumDay: Latest day number when eligible (expiration)

**Eligibility Check**:
```
Scene is eligible when:
  - Player has ALL required tags
  - Player has NONE of forbidden tags
  - Player completed ALL required scenes
  - Cooldown elapsed (X days since last instance)
  - Current day >= MinimumDay
  - Current day <= MaximumDay (if specified)
  - Placement context available (matching location/NPC exists)
```

**Why This Works**:
- State-based eligibility (no hardcoded triggers)
- Flexible sequencing (tags from multiple sources work)
- Anti-repetition (cooldowns prevent spam)
- Expiration (time pressure for limited opportunities)

**Example**:

**Scene A**: "Investigate mill exterior"
- Spawn Conditions: RequiredTags = ["arrived_in_westmarch"]
- Rewards: TagsToApply = ["investigated_mill"]

**Scene B**: "Investigate mill interior"
- Spawn Conditions:
  - RequiredTags = ["investigated_mill"]
  - CooldownDays = 2 (can't immediately re-enter)

**Scene C**: "Confront mill owner"
- Spawn Conditions:
  - RequiredTags = ["investigated_mill", "found_evidence"]
  - ForbiddenTags = ["allied_with_owner"]

**Scene D**: "Testify at trial"
- Spawn Conditions:
  - RequiredTags = ["confronted_owner"]
  - MinimumDay = 7 (trial scheduled for day 7+)
  - MaximumDay = 14 (trial date passes after day 14)

Flow:
1. Player arrives Westmarch → Receives "arrived_in_westmarch" tag
2. Scene A eligible → Completes → Receives "investigated_mill" tag
3. Scene B eligible after 2 days → Completes → Receives "found_evidence" tag
4. Scene C eligible (has both required tags, lacks forbidden tag) → Completes → Receives "confronted_owner" tag
5. Scene D eligible after day 7, expires after day 14 → Time pressure to complete

## 9.5 Anti-Patterns to Avoid

### 9.5.1 Boolean Gates (Completion Unlocks)

**Anti-Pattern**: Content doesn't exist until unlock condition met. Player completes task, content appears.

**Why Wrong**:
- No strategic planning (can't see what's ahead)
- Checklist completion (just complete tasks to unlock next)
- No resource competition (unlocking is "free")
- Linear progression (A unlocks B unlocks C)
- No player agency (path predetermined)

**Example** (FORBIDDEN):
```
if (player.CompletedQuest("tutorial")) {
    UnlockQuest("main_story_1");
}

if (player.KilledBoss("dragon")) {
    UnlockArea("dragon_castle");
}
```

**Correct Alternative**: Requirement Inversion Pattern
- Content exists from start (or spawns via rewards)
- Requirements filter visibility/selectability
- Player sees locked content with exact requirements shown
- Resource arithmetic (stat thresholds, coin costs) not boolean checks

**Example** (CORRECT):
```
Scene mainStory1 = gameWorld.Scenes.First(s => s.Id == "main_story_1");

// Scene exists, spawn conditions filter visibility
if (mainStory1.SpawnConditions.IsEligible(player)) {
    // Player can select
} else {
    // Show locked with requirements: "Need tag: completed_tutorial"
}
```

### 9.5.2 Overlapping Resource Costs (False Choices)

**Anti-Pattern**: Multiple choices cost same resource type. One option dominates, others become dead options.

**Why Wrong**:
- Direct numerical comparison (which gives better value per coin?)
- One option dominates (better efficiency)
- Dead options (never chosen by informed player)
- No strategic trade-off (just pick highest efficiency)

**Example** (FORBIDDEN):
```
Choice A: Pay 5 coins → Basic outcome
Choice B: Pay 8 coins → Better outcome
Choice C: Pay 12 coins → Best outcome

Player with 12 coins: Choice C dominates (best outcome, can afford)
Choices A and B are dead options (never chosen)
```

**Correct Alternative**: Orthogonal Resource Costs
- Each choice costs DIFFERENT resource type
- No direct comparison possible
- Context determines best choice
- All options remain viable

**Example** (CORRECT):
```
Choice A: Rapport 5 required → Best outcome (character build)
Choice B: Pay 12 coins → Good outcome (consumable economy)
Choice C: Social challenge → Variable outcome (session resource + skill)
Choice D: Wait 3 days → Minimal outcome (opportunity cost)

Player choice depends on:
  - Do I have Rapport 5? (Choice A available?)
  - Do I have 12 coins? (Choice B affordable?)
  - Do I have Resolve for challenge? (Choice C viable?)
  - Do I have time to wait? (Choice D acceptable?)

No universal best choice. Strategic priorities determine selection.
```

### 9.5.3 Hidden Gotchas (Surprise Consequences)

**Anti-Pattern**: Player commits to choice, THEN discovers consequences. No way to calculate decision before commitment.

**Why Wrong**:
- Violates perfect information principle
- Player can't make strategic decisions
- Feels unfair (punished without warning)
- Discourages experimentation (fear of hidden penalties)

**Example** (FORBIDDEN):
```
Choice: "Accept magistrate's offer"
Display: "Magistrate offers assistance"
Hidden: Accepting applies "corrupted" tag, blocks good endings

Player commits without seeing consequence.
Later discovers they locked themselves out of content.
Feels cheated.
```

**Correct Alternative**: Perfect Information Display
- Show ALL costs before commitment
- Show ALL rewards/consequences
- Show exact requirements
- Player calculates decision with full knowledge

**Example** (CORRECT):
```
Choice: "Accept magistrate's offer"
Display:
  Immediate: Magistrate provides assistance, unlock shortcuts
  Consequences: Gain "allied_with_corrupt" tag, lose "moral_authority" tag
  Future Impact: Blocks "righteous_path" endings, enables "pragmatic_path" endings
  Resources: -5 Reputation, +2 Understanding

Player sees exact trade-off:
  Gain assistance and pragmatic paths
  Lose moral authority and righteous paths
  Informed decision with eyes open
```

### 9.5.4 Soft-Lock Paths (Unwinnable States)

**Anti-Pattern**: Player choices can create unwinnable state where progression impossible.

**Why Wrong**:
- Catastrophic in infinite game (no "restart" at hour 50)
- Punishes experimentation
- Requires foreknowledge (guides, wikis)
- Violates TIER 1 principle (No Soft-Locks)

**Example** (FORBIDDEN):
```
Scene requires 10 coins to progress.
Player spent all coins on equipment.
No way to earn more coins before scene expires.
Progression blocked.
Soft-lock.
```

**Correct Alternative**: Four-Choice Pattern with Guaranteed Path
- Every A-story situation has zero-requirement path
- Fallback path ALWAYS available
- Fallback path ALWAYS progresses
- Player chooses efficiency, not viability

**Example** (CORRECT):
```
Scene: "Gain access to archives"

Choice A: Authority 5 → Instant access (optimal)
Choice B: 15 coins → Buy access (reliable)
Choice C: Social challenge → Persuade access (risky)
Choice D: Help librarian for 3 days → Earn access (guaranteed)

Player with zero coins, Authority 2, low Resolve:
  Choices A, B, C unavailable or unaffordable
  Choice D ALWAYS available:
    - Zero requirements
    - Costs time (opportunity cost, not gate)
    - Minimal rewards
    - GUARANTEED progression

No soft-lock possible.
```

### 9.5.5 Power Creep (Later Content Strictly Better)

**Anti-Pattern**: Progression through statistical increases. Later content higher numbers, easier than earlier content with same effort.

**Why Wrong**:
- Invalidates earlier content (trivial by comparison)
- Removes challenge (player becomes invulnerable)
- No trade-offs (later = better always)
- Ends in either infinite power or arbitrary cap

**Example** (FORBIDDEN):
```
Early game inn: 8 coins, restores 15 Health
Mid game inn: 8 coins, restores 25 Health (strictly better)
Late game inn: 8 coins, restores 40 Health (strictly better)

Player always chooses latest inn.
Earlier inns obsolete.
```

**Correct Alternative**: Proportional Scaling with Trade-Offs
- Costs scale with rewards
- Premium options available but expensive
- Basic options remain viable (lower cost, adequate outcome)
- Trade-offs persist across progression

**Example** (CORRECT):
```
Basic inn: 5 coins, restores 15 Health (cheap, adequate)
Standard inn: 12 coins, restores 25 Health (moderate cost, better)
Premium inn: 40 coins, restores 40 Health (expensive, best)

All three viable throughout game:
  - Basic: When tight on coins
  - Standard: When comfortable buffer
  - Premium: When need fast full recovery

Player income scales (earning 50 coins/B-story late game vs 10 early game)
But premium inn 40 coins still meaningful (80% of B-story earnings)
Margins stay tight proportionally.
```

### 9.5.6 Arbitrary Gating (Requirements Without Verisimilitude)

**Anti-Pattern**: Requirements don't make narrative sense. Arbitrary gates for pacing/balance, not fiction.

**Why Wrong**:
- Breaks immersion (fiction doesn't justify mechanics)
- Feels gamey (clearly artificial gate)
- Confusing (why does this require that?)

**Example** (FORBIDDEN):
```
Scene: "Enter the open market"
Requirement: Must have completed "Defeat dragon" scene

Fiction: Market is open, anyone can enter.
Mechanics: Blocked until dragon defeated.
Justification: None (arbitrary pacing gate).
```

**Correct Alternative**: Verisimilitude in Gating
- Requirements make narrative sense
- Fiction justifies mechanics
- Player understands why requirement exists

**Example** (CORRECT):
```
Scene: "Gain audience with king"
Requirement: Reputation ≥ 8 OR letter of introduction

Fiction: King is important, guards filter petitioners.
Mechanics: Need social standing OR official introduction.
Justification: Verisimilitude (realistic gatekeeping).

Alternative paths:
  - Build reputation (help people, earn standing)
  - Get letter from noble (side quest)
  - Bribe guards (expensive, risky)
  - Wait for public audience (slow, guaranteed)
```

## 9.6 Pattern Selection Guide

### 9.6.1 Creating New Service Type

**Pattern**: Service Flow Pattern (Negotiation → Execution → Departure)

**Steps**:
1. Define service type (lodging, healing, bathing, training, etc.)
2. Use service_negotiation archetype (Situation 1)
3. Create service_execution_[TYPE] archetype (Situation 2)
   - Define 4 variants (balanced, physical-focused, mental-focused, special)
   - Define resource restoration formulas
4. Use service_departure archetype (Situation 3)
5. Define dependent resource (room_key, treatment_token, bathhouse_pass)

**Result**: Three-situation linear flow, four-choice pattern, self-contained service.

### 9.6.2 Creating Investigative Content

**Pattern**: Investigation Pattern (Hub-and-Spoke Structure)

**Steps**:
1. Create hub situation (begin investigation)
2. Create 3-4 spoke situations (parallel leads)
3. Define spawn conditions (each spoke accessible after hub)
4. Define evidence tags (what each spoke grants)
5. Create convergence situation (requires 2+ evidence tags)
6. Define conclusion paths (quality varies by evidence gathered)

**Result**: Non-linear investigation, player explores in any order, evidence accumulation visible.

### 9.6.3 Creating Story Progression

**Pattern**: Tag-Based Dependencies with Obligation Structure

**Steps**:
1. Create Obligation (persistent mystery container)
2. Define Phase 1-N scenes (sequential progression)
3. Each scene completion grants tags
4. Next scene requires previous scene's tags
5. Final authored scene triggers procedural continuation
6. Procedural scenes reference player journey

**Result**: Long-term narrative spine, state-based progression, infinite continuation.

### 9.6.4 Creating Tactical Content

**Pattern**: Unified Stat System with Challenge Type

**Steps**:
1. Determine challenge type (Mental, Physical, Social)
2. Define builder resource (Progress, Breakthrough, Momentum)
3. Define threshold resource (Exposure, Danger, Doubt)
4. Define session resource (Attention, Exertion, Initiative)
5. Define victory thresholds (Progress ≥ 15, Doubt < 10)
6. Create card deck (bound to stats, appropriate for challenge type)

**Result**: Tactical session with resource management, victory thresholds, stat-bound cards.

### 9.6.5 Need Guaranteed Progression

**Pattern**: Four-Choice Archetype

**Steps**:
1. Define stat-gated path (Rapport/Authority/Insight/Diplomacy/Cunning ≥ threshold)
2. Define money-gated path (coin cost)
3. Define challenge path (session resource cost, variable outcome)
4. Define guaranteed path (zero requirements, time cost, minimal rewards)
5. Ensure Choice 4 cannot fail and advances progression

**Result**: No soft-lock possible, player chooses HOW to progress.

## 9.7 Pattern Testing

### 9.7.1 Test for Orthogonal Resource Costs

**Question**: Does each choice cost DIFFERENT resource type?

**Method**: List resource cost for each choice.

**Pass Criteria**: All four choices cost different resources.

**Example Pass**:
- Choice A: Character build (Rapport 5)
- Choice B: Consumable economy (12 coins)
- Choice C: Session resource (3 Resolve)
- Choice D: Opportunity cost (3 time blocks)

All different. Pass.

**Example Fail**:
- Choice A: Consumable economy (5 coins)
- Choice B: Consumable economy (10 coins)
- Choice C: Session resource (3 Resolve)
- Choice D: Opportunity cost (3 time blocks)

Choices A and B both cost coins. Fail.

### 9.7.2 Test for Guaranteed Progression

**Question**: Can player always progress regardless of resources?

**Method**: Assume worst case: Zero coins, minimum stats, no Resolve/Stamina/Focus. Can player still progress?

**Pass Criteria**: At least one choice has zero requirements AND cannot fail AND advances progression.

**Example Pass**:

Situation: "Gain innkeeper's trust"
- Choice A: Rapport 5 (player has 1) - LOCKED
- Choice B: 15 coins (player has 0) - UNAFFORDABLE
- Choice C: Social challenge (player has 0 Resolve) - UNAFFORDABLE
- Choice D: Help for 3 days (player has time) - AVAILABLE

Choice D: Zero requirements, cannot fail, advances to next scene. Pass.

**Example Fail**:

Situation: "Gain innkeeper's trust"
- Choice A: Rapport 5 (player has 1) - LOCKED
- Choice B: 15 coins (player has 0) - UNAFFORDABLE
- Choice C: Social challenge (player has 0 Resolve) - UNAFFORDABLE
- Choice D: Leave (returns to world, no progression) - AVAILABLE

No choice progresses story if player lacks resources. Fail.

### 9.7.3 Test for Perfect Information

**Question**: Can player calculate exact outcome before commitment?

**Method**: Review choice display. Does it show: Costs, rewards, requirements, consequences?

**Pass Criteria**: All information visible before selection. No hidden surprises.

**Example Pass**:
```
Choice: "Negotiate with authority"
Requirements: Authority ≥ 5 (you have 3) - LOCKED
Alternative: Pay 20 coins → Bypass requirement
Outcome: Gain access to archives, relationship +1
Cost: 20 coins (you have 25, will have 5 remaining)

Player sees: Locked by stat, can bypass with coins, knows exact cost and outcome.
```

Pass.

**Example Fail**:
```
Choice: "Accept magistrate's offer"
Display: "Magistrate offers help"
Hidden: Gain "corrupted" tag, lock out good endings

Player doesn't see consequence until after commitment.
```

Fail.

### 9.7.4 Test for Verisimilitude

**Question**: Do requirements make narrative sense?

**Method**: Ask: "Does the fiction justify this requirement?"

**Pass Criteria**: Requirements match conceptual model. Player understands why requirement exists.

**Example Pass**:

Scene: "Gain audience with governor"
Requirement: Reputation ≥ 6

Fiction: Governor is important official, guards filter petitioners based on standing.
Justification: Makes sense.

Pass.

**Example Fail**:

Scene: "Enter public market"
Requirement: Must complete "Dragon Quest"

Fiction: Market is open to anyone.
Justification: None (arbitrary gate).

Fail.

### 9.7.5 Test for Build Viability

**Question**: Can all builds complete content?

**Method**: Test with extreme builds (high in one stat, low in others). Can they progress?

**Pass Criteria**: All builds can progress, but via different paths.

**Example Pass**:

Scene: "Gain information from scholar"

High Insight build (Insight 7, Rapport 2):
- Insight path unlocked (optimal)
- Money path available
- Challenge path available
- Fallback path available

Low Insight build (Insight 2, Rapport 7):
- Rapport path unlocked (alternative optimal)
- Money path available
- Challenge path available
- Fallback path available

Both builds progress, different paths. Pass.

**Example Fail**:

Scene: "Solve ancient puzzle"

High Insight build (Insight 7, Rapport 2):
- Insight path unlocked (only option)

Low Insight build (Insight 2, Rapport 7):
- No alternative paths
- Blocked

One build can't progress. Fail.

## 9.8 Summary

Wayfarer's reusable patterns:

**Core Patterns**: Strategic-Tactical Separation (perfect info vs execution), Four-Choice Archetype (guaranteed progression), Orthogonal Resource Costs (genuine trade-offs), Tag-Based Dependencies (state-driven content).

**Content Patterns**: Service Flow (negotiation/execution/departure), Investigation (hub-and-spoke), Branching Choices (OnSuccess/OnFailure), Multi-Phase Mystery (Obligation → Scenes), Arrival Pattern (travel rhythm).

**Reusability**: Archetype Composition (scenes compose situations), Property-Driven Generation (categorical scaling), Placement Filters (WHERE patterns appear), Spawn Conditions (WHEN patterns eligible).

**Anti-Patterns**: Boolean Gates (violates perfect information), Overlapping Costs (false choices), Hidden Gotchas (surprise consequences), Soft-Lock Paths (unwinnable states), Power Creep (invalidates earlier content), Arbitrary Gating (fiction doesn't justify).

**Pattern Selection**: Service = Service Flow, Investigation = Hub-and-Spoke, Story = Tag-Based Dependencies, Tactical = Challenge System, Progression = Four-Choice Archetype.

**Testing**: Orthogonal costs (different resources?), Guaranteed progression (zero-requirement path?), Perfect information (all visible?), Verisimilitude (fiction justifies?), Build viability (all builds progress?).

The result: Reusable patterns creating infinite variations, structural guarantees preventing soft-locks, meaningful choices through orthogonal costs, and perfect information enabling strategic planning.
