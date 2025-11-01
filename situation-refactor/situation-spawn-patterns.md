# Situation Spawn Patterns

## ARCHITECTURAL CONTEXT

**STRATEGIC LAYER HIERARCHY:**
- **Scene** = Ephemeral spawning orchestrator that creates multiple Situations in various configurations
- **Situation** = Persistent narrative context containing 2-4 actions (LocationAction/ConversationOption/TravelCard)
- **Spawn Flow**: Scene spawns Situations → Situation completion can spawn new Scenes or Situations

**PATTERN APPLICATION:**
- Scenes use these patterns to orchestrate MULTIPLE Situations (sequential, parallel, branching)
- Situations use these patterns to spawn follow-up Situations or Scenes
- Templates define patterns, code instantiates with concrete entities from GameWorld

**LAYER SEPARATION:**
- STRATEGIC: Scene/Situation/Actions (these patterns apply here)
- TACTICAL: SituationCard (victory conditions inside challenges - separate system)

---

Spawn patterns define how situations cascade and connect across the game world. Templates use these patterns to create dynamic narrative chains without hardcoded content.

## Pattern 1: Linear Progression

**Pattern:** Sequential story beats (A → B → C)

**Structure:**
- Situation spawns single follow-up on completion
- Each step builds on previous narrative
- Success/failure both progress (different paths)
- Creates guided narrative arc

**Example: Investigation Chain**
```
Template: investigation_start
→ Spawns: investigation_evidence
  → Spawns: investigation_confrontation
    → Spawns: investigation_resolution

Each step reveals more information, building toward conclusion
```

**Decision space:** Player choices affect WHICH follow-up spawns (success vs failure paths), but always progress forward

**Use cases:** Mystery arcs, tutorial sequences, character introductions, main story beats

---

## Pattern 2: Hub-and-Spoke

**Pattern:** Central situation spawns multiple parallel options

**Structure:**
- One situation spawns several child situations simultaneously
- All children available at once
- Children independent (no prerequisite order)
- Completing all children may unlock final convergence

**Example: Merchant's Problems**
```
Template: merchant_needs_help
→ Spawns (parallel):
  - recover_stolen_goods
  - negotiate_with_thieves_guild
  - investigate_competitor

Player chooses which to tackle first, all available
Completing all three may spawn: merchant_gratitude_rewards
```

**Decision space:** Which problem to solve first? Resource allocation across multiple paths? Pursue all or focus?

**Use cases:** Side quest hubs, faction requests, exploration branches, player agency moments

---

## Pattern 3: Branching Consequences

**Pattern:** Success and failure lead to different futures

**Structure:**
- Situation has two distinct completion paths (success/failure)
- Success spawns positive consequence chain
- Failure spawns negative consequence chain
- Both paths are valid, create different opportunities

**Example: Rescue Attempt**
```
Template: rescue_hostage

Success Path:
→ Spawns: grateful_ally_favor
  → Spawns: ally_introduces_contacts

Failure Path:
→ Spawns: hostage_lost_guilt
  → Spawns: seeking_redemption_quest

Both paths continue story, different tones
```

**Decision space:** Player accepts consequences of choices. Failure isn't game over, just different story.

**Use cases:** High-stakes decisions, moral dilemmas, relationship forks, permanent consequences

---

## Pattern 4: Discovery Chain

**Pattern:** Finding clues reveals hidden situations

**Structure:**
- Initial situation spawns at known location
- Completing it spawns follow-up at NEW location (previously unknown/inaccessible)
- Each discovery unlocks further exploration
- Location discovery drives progression

**Example: Hidden Passage**
```
Template: warehouse_investigation
→ Spawns: hidden_passage_discovered (new location revealed)
  → Spawns: smugglers_den_infiltration (deeper location)
    → Spawns: crime_boss_confrontation (final location)

Each step reveals new area of game world
```

**Decision space:** Thoroughness rewarded with discovery. Explore deeply vs move on?

**Use cases:** Exploration gameplay, secret areas, investigation depth, world expansion

---

## Pattern 5: Escalating Stakes

**Pattern:** Each situation increases difficulty and rewards

**Structure:**
- First situation has low requirements, low rewards
- Completing it spawns harder version with better rewards
- Player can opt out at any level
- Risk/reward escalation creates tension

**Example: Underground Fighting**
```
Template: amateur_bout (easy, small reward)
→ Spawns: veteran_match (medium, good reward)
  → Spawns: championship_fight (hard, great reward)
    → Spawns: death_match (extreme, legendary reward)

Each step optional, player chooses when to stop
```

**Decision space:** Push luck for better rewards or take winnings and leave? Risk management.

**Use cases:** Arena systems, gambling, challenge towers, risk/reward loops

---

## Pattern 6: Timed Cascade

**Pattern:** Completing situation before deadline spawns urgent path, after deadline spawns consequence path

**Structure:**
- Situation has time limit (days/segments)
- Completing before deadline: spawns ideal follow-up
- Missing deadline: spawns degraded follow-up (still progresses)
- Time pressure creates urgency

**Example: Medical Emergency**
```
Template: npc_sick

If completed within 2 days:
→ Spawns: npc_recovered_gratitude

If completed after 2 days:
→ Spawns: npc_died_funeral
  → Spawns: family_blames_player

Both paths continue, different emotional tone
```

**Decision space:** Prioritize urgent vs important? Accept time-cost trade-offs?

**Use cases:** Deadlines, emergencies, ticking clocks, priority management

---

## Pattern 7: Reputation Threshold

**Pattern:** Completing N situations of type X unlocks special situation

**Structure:**
- Multiple situations share category/tag
- Tracking counts completions of that category
- Reaching threshold spawns unique opportunity
- Rewards specialization and consistency

**Example: Faction Loyalty**
```
Template (repeatable): faction_favor_mission

After completing 3 faction missions:
→ Spawns: faction_lieutenant_promotion

After completing 7 total:
→ Spawns: faction_inner_circle_invitation

Cumulative investment unlocks deeper access
```

**Decision space:** Specialize in one faction or diversify across many? Long-term investment.

**Use cases:** Faction progression, skill mastery, reputation systems, relationship depth

---

## Pattern 8: Resource Sink Gate

**Pattern:** Situation requires spending accumulated resource to unlock next tier

**Structure:**
- Complete situation only if player has threshold resource amount
- Completing costs resources but spawns valuable follow-up
- Acts as progression gate (can't rush without resources)
- Creates resource pressure

**Example: Academic Advancement**
```
Template: university_entrance_exam (requires 500 coins)
→ Spawns: university_courses (requires 100 coins per course)
  → Spawns: thesis_defense (requires 50 knowledge)
    → Spawns: scholar_recognition

Each step drains resources but unlocks new opportunities
```

**Decision space:** Hoard resources or invest in advancement? Opportunity cost.

**Use cases:** Class progression, economic gates, investment systems, tier unlocks

---

## Pattern 9: Converging Paths

**Pattern:** Multiple independent situations converge to unlock finale

**Structure:**
- Several situations spawn independently
- Each completion tracks toward shared goal
- Completing ALL spawns convergence situation
- Parallel progress toward single outcome

**Example: Investigation Threads**
```
Independent Templates:
- question_witness_a
- search_crime_scene
- investigate_alibi
- follow_money_trail

When ALL four completed:
→ Spawns: pieces_together_revelation
  → Spawns: confront_culprit

Must pursue all threads to reach conclusion
```

**Decision space:** Which thread to pursue next? Must complete all eventually.

**Use cases:** Mystery investigations, gather-the-party quests, multi-aspect challenges

---

## Pattern 10: Mutually Exclusive Paths

**Pattern:** Completing situation A prevents situation B from spawning

**Structure:**
- Two situations available simultaneously
- Completing one removes/blocks the other
- Permanent choice between paths
- Creates regret/commitment

**Example: Faction Allegiance**
```
Template: thieves_guild_invitation (accept thieves)
Template: merchants_guild_invitation (accept merchants)

Accepting thieves:
→ Spawns: thieves_guild_missions
→ BLOCKS: merchants_guild_invitation

Accepting merchants:
→ Spawns: merchants_guild_missions
→ BLOCKS: thieves_guild_invitation

Cannot join both, permanent choice
```

**Decision space:** Which path to commit to? Accept lost opportunities?

**Use cases:** Faction exclusivity, permanent decisions, meaningful choices, replayability

---

## Pattern 11: Recursive Loops

**Pattern:** Completing situation can re-spawn itself with variations

**Structure:**
- Situation spawns modified version of itself on completion
- Parameters change (difficulty, rewards, narrative details)
- Can continue indefinitely or until condition met
- Creates repeatable content with progression

**Example: Patrol Encounters**
```
Template: highway_patrol

On completion:
→ Spawns: highway_patrol (same template, higher difficulty tier)

Parameters scale each loop:
- Enemy strength increases
- Rewards improve
- Narrative acknowledges repetition ("They're getting bolder...")

Stops when player leaves region or completes regional quest
```

**Decision space:** Keep farming for rewards or move on? Optimization vs exploration.

**Use cases:** Grinding loops, procedural encounters, challenge scaling, emergent difficulty

---

## Pattern 12: Delayed Spawn

**Pattern:** Completing situation spawns follow-up after time delay

**Structure:**
- Situation completes immediately
- Follow-up spawns X days later
- Creates anticipation and world continuity
- Simulates off-screen events

**Example: Message Delivery**
```
Template: send_message_to_capital

Completes immediately
↓
(3 days pass)
↓
Spawns: messenger_returns_with_reply
  → Spawns: capital_responds (content based on original message)

World feels alive with events happening independently
```

**Decision space:** Player continues other activities while waiting. World feels reactive.

**Use cases:** Message systems, travel time, NPC reactions, world simulation

---

## Pattern 13: Conditional Multi-Spawn

**Pattern:** Situation spawns different combinations based on completion state

**Structure:**
- Situation tracks how it was completed (which approach used, resources spent, etc.)
- Different completion methods spawn different follow-up combinations
- Creates branching based on player method, not just success/failure
- Rewards playstyle diversity

**Example: Defuse Conflict**
```
Template: tavern_brawl

If resolved via Intimidation:
→ Spawns: criminals_fear_player + tavern_owner_grateful

If resolved via Persuasion:
→ Spawns: criminals_respect_player + tavern_becomes_safe_house

If resolved via Violence:
→ Spawns: guards_investigate + reputation_damaged

Same situation, three different outcome combinations
```

**Decision space:** How to solve problem? Method matters as much as success.

**Use cases:** Approach diversity, playstyle expression, methodical consequences

---

## Pattern 14: Failure-Only Spawn

**Pattern:** Only failure spawns follow-up (success ends chain)

**Structure:**
- Succeeding completes situation cleanly (no spawn)
- Failing spawns complication that must be addressed
- Creates "success is closure, failure is story" dynamic
- Failing isn't punishment, it's content

**Example: Stealth Infiltration**
```
Template: sneak_past_guards

Success: Clean entry, no spawn (mission continues elsewhere)

Failure:
→ Spawns: alarm_raised
  → Spawns: escape_pursuit
    → Spawns: hide_from_search

Failure creates more story beats
```

**Decision space:** Risk stealth for clean success or accept failure cascade?

**Use cases:** Stealth systems, heist gameplay, cascading problems, failure as content

---

## Pattern 15: Prerequisite Network

**Pattern:** Situation only spawns when multiple conditions met

**Structure:**
- Template defines multiple spawn requirements
- Must complete situations A AND B AND have resource C
- Creates complex unlock conditions
- Rewards thorough preparation

**Example: Ancient Ritual**
```
Template: perform_ritual

Spawn Requirements:
- Completed: gather_sacred_herbs
- Completed: learn_ritual_words
- Have: ancient_tome (item)
- Location: sacred_grove (discovered)
- Time: Full moon (specific day)

Only spawns when ALL conditions met
```

**Decision space:** Orchestrate multiple threads to enable unlock. Preparation rewarded.

**Use cases:** Ritual systems, complex unlocks, quest convergence, preparation gameplay

---

## Meta-Patterns: Combining Templates

Templates combine to create rich narrative structures:

**Linear + Branching:**
```
A → B (success) → C
  → D (failure) → E
Both paths progress story, different tones
```

**Hub + Convergence:**
```
Hub → [A, B, C] (parallel)
When all complete → Finale
```

**Escalation + Exclusive:**
```
Path 1: Tier 1 → Tier 2 → Tier 3
Path 2: Alternative progression
Choosing Path 1 blocks Path 2
```

**Discovery + Timed:**
```
Find Location → Urgent situation spawns
Must complete before location changes/closes
```

Templates are PATTERNS, not content. Code instantiates them with concrete entities from GameWorld.

---

## Crisis Rhythm Pattern

**Pattern:** Escalating tension through preparation-test cycles

**Concept:** Scenes follow the rhythm **Build → Build → Build → TEST**, where regular situations allow preparation and Crisis situations test preparation quality.

**Structure:**
- Scene contains 3-5 Situations
- First 2-4 situations have `type: "Normal"` (build phase)
- Final situation has `type: "Crisis"` (test phase)
- Crisis has high stat requirement OR expensive alternative OR risky gamble
- Failure has permanent consequences

**Example: Merchant Guild Dispute**
```
Situation 1 (Normal): Observe argument
  Choices: Listen (+1 Insight) or Move closer (+1 Authority)

Situation 2 (Normal): Gather information
  Choices: Ask merchants (+1 Diplomacy) or Investigate (+1 Insight)

Situation 3 (Normal): Choose side
  Choices: Support seller (+1 Rapport) or Support buyer (+1 Authority)

Situation 4 (Crisis): Guild confrontation
  Choice A: Assert authority (Authority 4+, 2 energy) ← Easy if prepared
  Choice B: Pay off guild (20 coins) ← Expensive alternative
  Choice C: Threaten (Physical challenge) ← Risky gamble
  Choice D: Walk away (Scene fails, NPC bond -3) ← Permanent failure
```

**Player Experience:**
1. **Situations 1-3:** "I'm building toward something..."
2. **Approaching Crisis:** "Things escalating, need to be ready"
3. **Crisis:** "This is it - do I have the stats?"
4. **Resolution:** "Preparation paid off!" OR "Should have invested more..."

**Decision Space:**
- Which stat to build during preparation?
- Build one stat high (4+) or spread across multiple (3 each)?
- Accept expensive alternative if unprepared?
- Risk gamble or accept failure?

**Strategic Depth:**
- Every regular choice matters (building stats for crisis)
- Can't prepare for all crises (resource limits)
- Must choose which scenes to prioritize
- Unprepared path is expensive but not impossible

**Use Cases:**
- Social encounters (Diplomacy/Authority crises)
- Investigation scenes (Insight/Cunning crises)
- Physical challenges (Strength/Endurance crises)
- Any scene where preparation should matter

**Integration with Other Patterns:**

**Linear + Crisis:**
```
Normal Situation 1 → Normal Situation 2 → Crisis Situation
Build stats linearly, test at end
```

**Hub + Crisis:**
```
Hub (Normal) → [Path A, Path B, Path C] (Normal, parallel)
  → Convergence (Crisis - tests which paths completed)
```

**Branching + Crisis:**
```
Preparation (Normal)
  → Crisis Choice
    Success Path → Positive outcome
    Failure Path → Negative outcome
Both paths valid, different opportunities
```

**Implementation Notes:**
- `SituationType` enum marks situations as Normal or Crisis
- Parser validates and defaults to Normal if not specified
- UI can detect Crisis situations for visual treatment
- Backward compatible (existing content defaults to Normal)

**See:** `CRISIS_RHYTHM_SYSTEM.md` for complete documentation and design rationale.
