# General Game Design Principles for Wayfarer

## Principle 1: Single Source of Truth with Explicit Ownership

**Every entity type has exactly one owner.**

- GameWorld owns all runtime entities via flat lists
- Parent entities reference children by ID, never inline at runtime
- Child entities reference parents by ID for lifecycle queries
- NO entity owned by multiple collections simultaneously

**Authoring vs Runtime:**
- **Authoring (JSON):** Nested objects for content creator clarity
- **Runtime (GameWorld):** Flat lists for single source of truth
- Parsers flatten nesting into lists, establish ID references

**Test:** Can you answer "who controls this entity's lifecycle?" with one word? If not, ownership is ambiguous.

## Principle 2: Strong Typing as Design Enforcement

**If you can't express it with strongly typed objects and lists, the design is wrong.**

- `List<Obstacle>` NOT `Dictionary<string, object>`
- `List<string> ObstacleIds` NOT `Dictionary<string, List<string>>`
- No `var`, no `object`, no `Dictionary<K,V>` AT ALL - FORBIDDEN
- No `HashSet<T>` - FORBIDDEN
- ONLY `List<T>` where T is entity or enum
- No `SharedData`, `Context`, `Metadata` dictionaries

**Why:** Dictionaries hide relationships. They enable lazy design where "anything can be anything." HashSets hide structure and ordering. Strong typing with Lists forces clarity about what connects to what and why.

**Test:** Can you draw the object graph with boxes and arrows where every arrow has a clear semantic meaning? If not, add structure.

## Principle 3: Ownership vs Placement vs Reference

**Three distinct relationship types. Don't conflate them.**

**OWNERSHIP (lifecycle control):**
- Parent creates child
- Parent destroys child
- Child cannot exist without parent
- Example: Investigation owns Obstacles, Obstacle owns Goals

**PLACEMENT (presentation context):**
- Entity appears at a location for UI/narrative purposes
- Entity's lifecycle independent of placement
- Placement is metadata on the entity
- Example: Goal has `locationId` field, appears at that location

**REFERENCE (lookup relationship):**
- Entity A needs to find Entity B
- Entity A stores Entity B's ID
- No lifecycle dependency
- Example: Location has `obstacleIds`, queries GameWorld.Obstacles

**Common Error:** Making Location own Obstacles because goals appear there. Location is PLACEMENT context, not OWNER.

**Test:** If Entity A is destroyed, should Entity B be destroyed? 
- Yes = Ownership
- No = Placement or Reference

## Principle 4: Inter-Systemic Rules Over Boolean Gates

**Strategic depth emerges from shared resource competition, not linear unlocks.**

**Boolean Gates (Cookie Clicker):**
- "If completed A, unlock B"
- No resource cost
- No opportunity cost
- No competing priorities
- Linear progression tree
- NEVER USE THIS PATTERN

**Inter-Systemic Rules (Strategic Depth):**
- Systems compete for shared scarce resources
- Actions have opportunity cost (resource spent here unavailable elsewhere)
- Decisions close options
- Multiple valid paths with genuine trade-offs
- ALWAYS USE THIS PATTERN

**Examples:**
- ❌ "Complete conversation to discover investigation" (boolean gate)
- ✅ "Reach 10 Momentum to play GoalCard that discovers investigation" (resource cost)
- ❌ "Have knowledge token to unlock location" (boolean gate)
- ✅ "Spend 15 Focus to complete Mental challenge that grants knowledge" (resource cost)

**Test:** Does the player make a strategic trade-off (accepting one cost to avoid another)? If no, it's a boolean gate.

## Principle 5: Typed Rewards as System Boundaries

**Systems connect through explicitly typed rewards applied at completion, not through continuous evaluation or state queries.**

**Correct Pattern:**
```
System A completes action
→ Applies typed reward
→ Reward has specific effect on System B
→ Effect applied immediately
```

**Wrong Pattern:**
```
System A sets boolean flag
→ System B continuously evaluates conditions
→ Detects flag change
→ Triggers effect
```

**Why:** Typed rewards are explicit connections. Boolean gates are implicit dependencies. Explicit connections maintain system boundaries.

**Examples:**
- ✅ GoalCard.Rewards.DiscoverInvestigation (typed reward)
- ✅ GoalCard.Rewards.PropertyReduction (typed reward)
- ❌ Investigation.Prerequisites.CompletedGoalId (boolean gate)
- ❌ Continuous evaluation of HasKnowledge() (state query)

**Test:** Is the connection a one-time application of a typed effect, or a continuous check of boolean state? First is correct, second is wrong.

## Principle 6: Resource Scarcity Creates Impossible Choices

**For strategic depth to exist, all systems must compete for the same scarce resources.**

**Shared Resources (Universal Costs):**
- Time (segments) - every action costs time, limited total
- Focus - Mental system consumes, limited pool
- Stamina - Physical system consumes, limited pool
- Health - Physical system risks, hard to recover

**System-Specific Resources (Tactical Only):**
- Momentum/Progress/Breakthrough - builder resources within challenges
- Initiative/Cadence/Aggression - tactical flow mechanics
- These do NOT create strategic depth (confined to one challenge)

**The Impossible Choice:**
"I can afford to do A OR B, but not both. Both paths are valid. Both have genuine costs. Which cost will I accept?"

**Test:** Can the player pursue all interesting options without trade-offs? If yes, no strategic depth exists.

## Principle 7: One Purpose Per Entity

**Every entity type serves exactly one clear purpose.**

- Goals: Define tactical challenges available at locations/NPCs
- GoalCards: Define victory conditions within challenges
- Obstacles: Provide strategic information about challenge difficulty
- Investigations: Structure multi-phase mystery progression
- Locations: Host goals and provide spatial context
- NPCs: Provide social challenge context and relationship tracking

**Anti-Pattern: Multi-Purpose Entities**
- "Goal sometimes defines a challenge, sometimes defines a conversation topic, sometimes defines a shop transaction"
- NO. Three purposes = three entity types.

**Test:** Can you describe the entity's purpose in one sentence without "and" or "or"? If not, split it.

## Principle 8: Verisimilitude in Entity Relationships

**Entity relationships should match the conceptual model, not implementation convenience.**

**Correct (matches concept):**
- Investigations spawn Obstacles (discovering a mystery reveals challenges)
- Obstacles contain Goals (a challenge has multiple approaches)
- Goals appear at Locations (approaches happen at places)

**Wrong (backwards):**
- Obstacles spawn Investigations (a barrier creating a mystery makes no sense)
- Locations contain Investigations (places don't own mysteries)
- NPCs own Routes (people don't own paths)

**Test:** Can you explain the relationship in natural language without feeling confused? If the explanation feels backwards, it is backwards.

## Principle 9: Elegance Through Minimal Interconnection

**Systems should connect at explicit boundaries, not pervasively.**

**Correct Interconnection:**
- System A produces typed output
- System B consumes typed input
- ONE connection point (the typed reward)
- Clean boundary

**Wrong Interconnection:**
- System A sets flags in SharedData
- System B queries multiple flags
- System C also modifies those flags
- System D evaluates combinations
- Tangled web of implicit dependencies

**Test:** Can you draw the system interconnections with one arrow per connection? If you need a web of arrows, the design has too many dependencies.

## Principle 10: Perfect Information with Hidden Complexity

**All strategic information visible to player. All tactical complexity hidden in execution.**

**Strategic Layer (Always Visible):**
- What goals are available
- What each goal costs (resources)
- What each goal rewards
- What requirements must be met
- What the current world state is

**Tactical Layer (Hidden Until Engaged):**
- Specific cards in deck
- Exact card draw order
- Precise challenge flow
- Tactical decision complexity

**Why:** the single player of this single-player rpg make strategic decisions based on perfect information, then execute tactically with skill-based play.

**Test:** Can the player make an informed decision about WHETHER to attempt a goal before entering the challenge? If not, strategic layer is leaking tactical complexity.

---

## Meta-Principle: Design Constraint as Quality Filter

**When you find yourself reaching for:**
- Dictionaries of generic types
- Boolean flags checked continuously
- Multi-purpose entities
- Ambiguous ownership
- Hidden costs

**STOP. The design is wrong.**

These aren't implementation problems to solve with clever code. They're signals that the game design itself is flawed. Strong typing and explicit relationships aren't constraints that limit you - they're filters that catch bad design before it propagates.

**Good design feels elegant. Bad design requires workarounds.**