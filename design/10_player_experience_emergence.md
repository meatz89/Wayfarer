# Design Section 10: Player Experience Emergence

## Purpose

This document defines the fundamental principles that create meaningful player experience through procedural content. Unlike balance rules (which ensure mechanical fairness) or design patterns (which provide reusable templates), these are **psychological laws** - principles about how choices create identity, how scarcity creates weight, how sacrifice creates emotion.

These principles apply to ALL situations - not just A1-A3 tutorial, but every procedurally generated scene across 100+ hours. They are the **generative rules** that produce experiences of identity formation, impossible choice, and emotional investment, regardless of specific content.

---

## 10.1 The Seven Laws of Experience Emergence

These laws are non-negotiable. Violating them creates generic RPG experiences instead of identity-forming narratives.

### Law 1: Identity Before Validation (Capability Precedes Requirements)

**Principle**: Players must BUILD identity through choices before the game VALIDATES that identity through requirements.

**Why This Matters**:
- Asking "who are you?" before player knows creates anxiety and paralysis
- Offering stat gains lets player discover preferences organically
- Requirements appearing AFTER capability feels like recognition, not barrier
- Player learns "I am someone who connects with people" by choosing connection repeatedly, THEN sees "Rapport 2" unlock special paths

**Implementation Rules**:
- A1-A3: Zero stat requirements, only stat-granting choices
- Player accumulates 2-4 stat points through preference expression
- A4+: First requirements appear at thresholds player has already reached (Rapport 2 when player has Rapport 2-3)
- Requirements never block progression (fallback always available)
- Stat gates say "you can do this special thing because of who you've become" not "you're locked out until you grind"

**Procedural Generation Implications**:
- Early content (player has 0-3 total stats): Generate choices offering stat gains, no requirements
- Mid content (player has 4-10 total stats): Generate requirements at 60-80% of player's highest stat
- Late content (player has 10+ total stats): Generate requirements matching current capability
- AI should NEVER generate stat requirement higher than player's capability plus 2

**Anti-Pattern Detection**:
- ❌ "Pick your starting class" menu (asking who player is before they know)
- ❌ Stat requirements in A1-A3 (validating identity that doesn't exist yet)
- ❌ Locked content with no path to unlock (creates "come back when stronger" thinking)
- ✓ Choices offering different stat gains (player discovers preference)
- ✓ Requirements appearing after player has built that stat (validates past choices)
- ✓ Fallback ensuring requirements are optional optimization, not gates

---

### Law 2: Orthogonal Expression (Choices Represent Personality Axes, Not Power Levels)

**Principle**: Choices must represent DIFFERENT WAYS OF BEING, not better/worse approaches to same goal.

**Why This Matters**:
- "Who are you when you need something from someone?" has no right answer
- Charm vs Authority vs Cunning are personality traits, not power rankings
- Player selecting based on instinct reveals identity
- No optimal path = every choice is genuine self-expression
- Player thinks "this is who I am" not "this is the best option"

**Implementation Rules**:
- Four-choice archetype: Stat/Money/Challenge/Fallback are orthogonal axes
- Charm builds relationships, Authority commands respect, Cunning sees angles - all valid, none superior
- Rewards scale with COST, not with TYPE
- No choice should be universally optimal
- Different builds should prefer different paths in SAME situation

**Procedural Generation Implications**:
- AI generates choices representing different personality expressions
- Charm path: Empathy, connection, relationship building
- Authority path: Command, directness, clarity of intent
- Cunning path: Strategy, angles, clever solutions
- Fallback path: Patience, economic commitment, time investment
- Each path MUST give proportional rewards for its costs
- No path should be "the smart choice" - only "the choice that matches this player's values"

**Anti-Pattern Detection**:
- ❌ One choice clearly superior (dominant strategy kills expression)
- ❌ "Correct answer" to social situations (turns expression into puzzle)
- ❌ Charm always better than Authority (type preference becomes power ranking)
- ❌ Fallback as "failure option" (delegitimizes patient/economic playstyles)
- ✓ Charm specialist prefers charm path, Authority specialist prefers authority path
- ✓ Wealthy player uses coins, patient player uses time, specialist uses stats
- ✓ All paths reach same base outcome (progression guaranteed, style chosen)

---

### Law 3: Scarcity Gradient (Pressure Before Power, Scale Maintains Tension)

**Principle**: Economic pressure must be established when stakes are low, then scaled proportionally so scarcity never disappears.

**Why This Matters**:
- 10 coins matters when you have 20, not when you have 200
- Spending half your wealth creates anxiety regardless of absolute value
- Survival pressure teaches "every coin is precious"
- Scaling maintains that lesson across 100+ hours
- Player feels uncertainty about tomorrow's costs, making today's spending weigh heavy

**Implementation Rules**:
- A1-A3: Start with 20-30 coins, room costs 10 coins (33-50% of wealth)
- Daily survival costs approximately match delivery earnings (tight margins)
- Some deliveries result in NET LOSS (teaches optimization necessity)
- As wealth increases, costs scale proportionally (maintain 20-40% meaningful spend)
- Never give player so much wealth that spending becomes trivial

**Procedural Generation Implications**:
- AI scales costs to player's current wealth
- Meaningful spend = 20-40% of typical reserve
- A1-A3: 10-15 coins (when player has 20-40)
- A7-A12: 20-30 coins (when player has 50-80)
- A13-A20: 35-50 coins (when player has 100-150)
- A21+: 60-100 coins (when player has 200-300)
- Survival costs scale with progression (food/lodging cost more in expensive regions)
- Margins stay tight (profit never becomes guaranteed)

**Anti-Pattern Detection**:
- ❌ Wealth accumulation without cost scaling (player becomes invulnerable to economic pressure)
- ❌ Guaranteed profit loops (removes survival tension)
- ❌ Trivial costs late game (10 coins means nothing when you have 500)
- ❌ "Save up and never worry again" possibility (breaks scarcity permanently)
- ✓ Player sweating over whether to spend 30 coins at hour 1
- ✓ Player sweating over whether to spend 60 coins at hour 50
- ✓ Economic crises possible at any wealth level (bandits demanding 60% of reserve)
- ✓ Margins stay tight throughout (optimization always matters)

---

### Law 4: Sacrifice Architecture (Every Choice Closes Doors)

**Principle**: Player can choose ONE path per situation. Cannot maximize all dimensions. Choosing means losing.

**Why This Matters**:
- Loss creates emotional weight
- Mourning unchosen paths creates investment in chosen path
- "I could have been empathetic but I chose command" creates character
- Inability to have everything forces prioritization of values
- Specialization emerges from repeated sacrifice in same direction

**Implementation Rules**:
- One choice per situation (no "do both" option)
- Choosing Charm in A1 means NOT choosing Authority or Cunning
- Player sees unchosen paths clearly (greyed out after selection, not hidden)
- Unchosen stat gains permanently lost (cannot repeat situation for different reward)
- Build identity through consistent sacrifice in same direction
- Balanced builds possible but require conscious sacrifice of depth for breadth

**Procedural Generation Implications**:
- AI generates mutually exclusive choices
- Selecting one resolves situation (others disappear)
- No "come back and try different approach" for same situation
- Different situations offer different primary stats (if player skipped Rapport in A1, A2 offers another Rapport opportunity)
- Specialization emerges from player's pattern of sacrifices over time
- AI tracks which stats player has consistently chosen/avoided

**Anti-Pattern Detection**:
- ❌ "Try all options" design (no sacrifice = no weight)
- ❌ Replayable situations with different rewards (player optimizes instead of chooses)
- ❌ "Correct" path with clearly inferior alternatives (false choice)
- ❌ Ability to max all stats (removes specialization pressure)
- ✓ Player regrets unchosen paths (emotional investment in choice)
- ✓ Specialization emerges organically (repeated sacrifices in same direction)
- ✓ Different players have dramatically different stat distributions
- ✓ "Road not taken" feeling (player wonders about alternative self)

---

### Law 5: Emergent Tutorial (Experience Is Pedagogical, Not Instructional)

**Principle**: Game teaches by showing consequences, never by explaining mechanics.

**Why This Matters**:
- "Rest costs time, but time passes anyway when staying overnight" learned by experiencing the trade-off, not reading tooltip
- Player discovers "coins can substitute for stats" by seeing both paths available
- Understanding emerges from pattern recognition across situations
- Tutorial feels invisible (player learns while playing, not while reading)
- Respects player intelligence (shows, doesn't tell)

**Implementation Rules**:
- No tooltips explaining "this is the fallback choice"
- No explicit "tutorial mode" or different rules for early game
- A1-A3 teaches through structure:
  - A1 Situation 1: Teaches orthogonal personality axes (charm/authority/cunning)
  - A1 Situation 2: Teaches growth-vs-recovery trade-off (study vs sleep fully)
  - A1 Situation 3: Teaches time opportunity cost (thank vs leave)
  - A2 Situation 3: Teaches stat validation (Rapport 2 unlocks better path)
- Player learns by experiencing visible costs and rewards
- Patterns repeat until player recognizes them

**Procedural Generation Implications**:
- AI never generates explanatory text ("this choice requires Rapport 2 because...")
- Only shows mechanical facts: "Requires Rapport 2, you have 1" (gap visible)
- Early situations should teach ONE concept each
- Repeat core patterns across multiple situations
- Let player discover: "Oh, higher costs give better rewards" by seeing it consistently
- Let player discover: "Oh, I can build relationships OR save time, not both"

**Anti-Pattern Detection**:
- ❌ Tutorial tooltips ("This is the tutorial section...")
- ❌ Different rules for early game ("Tutorial mode: unlimited retries")
- ❌ Explicit explanation of mechanics ("Stat requirements unlock special paths")
- ❌ "Class selection" menu (abstract choice before context)
- ✓ Player learns "time is precious" by experiencing opportunity cost
- ✓ Player learns "specialization matters" by seeing stat gates unlock as they specialize
- ✓ Player discovers patterns through repetition, not instruction
- ✓ "Aha!" moments when player realizes how systems work

---

### Law 6: Capability Emergence (Past Choices Echo Forward)

**Principle**: Current capabilities must visibly result from past choices. Player sees continuity of identity.

**Why This Matters**:
- "You have Rapport 2" is meaningless without context
- "You can do this because you chose empathy in A1, connection in A2, and patience in A3" creates narrative
- Player sees self becoming someone specific, not accumulating numbers
- Validation of past choices creates investment in future choices
- Identity feels earned, not assigned

**Implementation Rules**:
- When stat gate unlocks, player remembers earning that stat
- Rapport 2 unlocks charm path in A2 BECAUSE player chose charm in A1
- Tags flow forward: "InnkeeperTrusts" from A1 affects A5 innkeeper interaction
- NPCs remember past interactions (relationship values persist)
- Equipment purchased stays owned (investment visible)
- Route knowledge gained stays learned (world expands permanently)

**Procedural Generation Implications**:
- AI tracks all past choices via tags
- Tags affect future content generation
- NPC dialogue reflects relationship history
- Stat requirements appear for stats player has actually built
- World state changes persist (locations unlocked, NPCs met, relationships formed)
- Player sees CONSEQUENCES of earlier choices, not just EFFECTS

**Anti-Pattern Detection**:
- ❌ Stat requirements appearing for stats player never built (why would I need Cunning 4 when I chose Rapport every time?)
- ❌ Reset world state (relationships forgotten, progress lost)
- ❌ Disconnected situations (no consequence from past, no setup for future)
- ❌ Abstract stat gains ("+1 Insight" without context of where it came from)
- ✓ Player traces current capability to specific past choices
- ✓ Tags create narrative continuity ("InnkeeperTrusts" echoes across scenes)
- ✓ Relationships accumulate (bond levels increase over time)
- ✓ World remembers (NPCs react differently based on history)

---

### Law 7: Verisimilitude in Fiction (Mechanics Emerge From Narrative, Not Arbitrary)

**Principle**: Every mechanical cost and requirement must make sense in the fiction. Player should never ask "why?" and get answer "because game balance."

**Why This Matters**:
- "Resting costs time" makes sense
- "Resting costs time but time passes anyway" breaks verisimilitude
- "Study while resting gives less restoration" makes sense (mental effort reduces physical recovery)
- Player trusts game when mechanics match fiction
- Immersion breaks when arbitrary gates appear
- "This NPC requires Rapport 4" must be justified by NPC personality, not random number

**Implementation Rules**:
- Friendly NPCs have lower Rapport requirements than hostile NPCs
- Authorities require Authority stat, not Cunning
- Merchants respond to coins, scholars respond to Insight
- Costs reflect actual opportunity costs (see Rule 7 in balance philosophy)
- Requirements reflect narrative context (stubborn NPC needs higher persuasion)
- Challenge types match situation (social situation = social challenge, not physical)

**Procedural Generation Implications**:
- AI derives requirements from NPC personality properties
- Friendly innkeeper: Rapport 2-3 (low barrier, warm person)
- Hostile guard: Authority 5-6 (high barrier, respects only power)
- Cunning thief: Cunning 4+ (recognizes fellow strategist)
- Analytical scholar: Insight 5+ (values intellectual capability)
- Categorical properties (Friendly/Hostile, Generous/Greedy, Trusting/Suspicious) drive mechanical thresholds
- Fiction generates numbers, not the reverse

**Anti-Pattern Detection**:
- ❌ "Requires Rapport 4" on hostile NPC who hates everyone (fiction contradicts mechanic)
- ❌ Arbitrary cost that makes no narrative sense
- ❌ "Because game balance" as explanation for requirement
- ❌ Merchants who don't care about money (fiction violation)
- ✓ Friendly NPCs have lower requirements (personality drives threshold)
- ✓ Costs reflect actual trade-offs (time spent here can't be spent elsewhere)
- ✓ Requirements make sense given context (guard respects authority)
- ✓ Player never asks "why does this cost that?" - answer is obvious from fiction

---

## 10.2 Sequencing Psychology: The Order of Experience Matters

These laws create experiences, but WHEN they're applied determines whether player hooks or bounces.

### The First 30 Seconds: Establish Stakes and Scarcity

**What Must Happen**:
- Player needs something (shelter, information, passage)
- Player lacks resources to trivially obtain it (coins limited, stats absent)
- Uncertainty about future (don't know what tomorrow costs)
- Human situation, not mechanical challenge (NPC interaction, not combat)

**Why This Order**:
- Human situations are universally accessible (everyone understands needing shelter)
- Scarcity creates immediate tension (10 coins matters when you have 20)
- Uncertainty creates weight (spending now might regret later)
- Social challenge reveals personality (combat reveals reflexes)

**Procedural Rule**:
- First situation MUST be NPC interaction requiring social navigation
- First cost MUST be 30-50% of starting wealth (substantial but affordable)
- First choice MUST offer orthogonal personality expressions
- No stat requirements (player hasn't built identity yet)

---

### The First 5 Minutes: Identity Formation Through Expression

**What Must Happen**:
- 2-3 choices offering different stat gains
- Each choice represents different personality axis
- No optimal path (all equally valid)
- Player selects based on instinct, not optimization

**Why This Order**:
- Identity emerges through expression, not selection
- Player discovers "I'm someone who connects with people" by choosing connection
- Early choices low-stakes (can't fail, just expresses preference)
- No punishment for "wrong" choice (all paths valid)

**Procedural Rule**:
- A1 situations offer stat gains, never require stats
- Charm/Authority/Cunning/Diplomacy/Insight all appear as options
- No choice clearly superior
- Fallback represents patience/economics, not failure

---

### The First 15 Minutes: Teach Core Trade-Offs Through Experience

**What Must Happen**:
- Growth vs Recovery (study vs sleep fully)
- Relationships vs Time (thank Elena vs leave quickly)
- Coins vs Stats (pay for outcome vs use capability)
- Challenge vs Certainty (risk better reward vs guarantee adequate reward)

**Why This Order**:
- Core trade-offs learned experientially before optimization required
- Player feels "oh, I can't have everything" naturally
- Understanding emerges from pattern recognition
- No explicit tutorial needed

**Procedural Rule**:
- Each core trade-off presented at least once in A1-A3
- Trade-offs repeat across situations (pattern recognition)
- Consequences visible immediately (player sees what they lost/gained)
- No explaining (show, don't tell)

---

### The First Hour: Validation of Emerging Identity

**What Must Happen**:
- First stat requirements appear (A4+)
- Requirements match stats player has built
- Stat-gated path unlocks BECAUSE of past choices
- Player sees "I can do this because I chose X, Y, Z earlier"

**Why This Order**:
- Validation after formation (not before)
- Requirements feel like recognition, not barrier
- Continuity of identity (past echoes forward)
- Investment in specialization (player sees payoff)

**Procedural Rule**:
- First stat requirements appear only after player has built those stats
- Requirements at 60-80% of player's highest stat (accessible but meaningful)
- Alternative paths for different specializations (Authority 3 OR Rapport 3)
- Fallback still guarantees progression (validation is bonus, not gate)

---

## 10.3 Procedural Content Generation: Applying Laws to Infinite Situations

How does AI generate situations that follow these laws without manually designing each one?

### Situation Template Structure

**Every procedurally generated situation must have**:

1. **Context Determination**:
   - What does player need? (Information/Access/Service/Relationship)
   - Who is involved? (NPC with personality properties: Friendly/Hostile, Generous/Greedy, Trusting/Suspicious)
   - Where is this? (Location with atmosphere and stakes)
   - What's the progression? (A-story phase number determines requirement scaling)

2. **Outcome Definition**:
   - Base outcome ALL paths share (progression guaranteed)
   - Proportional rewards for higher-cost paths (stat-gated best, fallback minimal)
   - Tags to apply (state changes for future reference)
   - Next scenes to spawn (continuation)

3. **Four-Path Generation**:
   - **Path 1 (Stat-Gated)**: Which stat makes narrative sense given NPC personality? What threshold matches progression level?
   - **Path 2 (Money-Gated)**: What coin cost is 20-40% of expected player wealth at this progression?
   - **Path 3 (Challenge)**: Which challenge type (Mental/Physical/Social) matches narrative context? What session resource cost?
   - **Path 4 (Fallback)**: What zero-requirement approach makes narrative sense? Time cost 3-5 blocks?

4. **Balance Validation**:
   - All 8 balance rules checked
   - Edge cases validated (worst-case player can progress)
   - Verisimilitude confirmed (fiction justifies mechanics)

5. **Experience Validation**:
   - All 7 experience laws checked
   - Orthogonality confirmed (no dominant strategy)
   - Sacrifice confirmed (choosing one closes others)
   - Capability matches requirements (no impossible gates)

### AI Generation Workflow

**Input**: Current game state (player stats, wealth, progression phase, active tags, NPC relationships)

**Process**:

1. **Determine Situation Need** (from A-story progression or B-story context)
2. **Select NPC Archetype** (from catalogue with personality properties)
3. **Derive Requirements** from personality properties:
   - Friendly → Rapport 2-3
   - Hostile → Authority 5-6
   - Cunning → Cunning 4+
   - Scholarly → Insight 5+
4. **Scale Requirements** to progression phase:
   - A4-A6: Requirements 2-3
   - A7-A12: Requirements 4-5
   - A13-A20: Requirements 6-7
5. **Calculate Coin Costs** from player wealth (20-40% of reserve)
6. **Generate Four Paths** following templates
7. **Validate Balance** (8 rules)
8. **Validate Experience** (7 laws)
9. **Output**: Complete situation with context, choices, outcomes, tags

**Example**:

```
Input:
- Player: Rapport 3, Insight 2, Authority 1, Cunning 1, Diplomacy 2
- Wealth: 45 coins
- Progression: A5
- Location: Small town market
- Need: Information about traveling merchant

AI Generates:

Situation: "Question the Merchant About Routes"

Context: Market square, mid-morning, friendly merchant selling maps

Path 1 (Stat-Gated):
- "Build rapport through shared travel stories"
- Requires: Rapport 2 (player has 3, accessible)
- Outcome: Detailed information, relationship +2, free map (bonus)
- Tag: "MerchantTrusts"

Path 2 (Money-Gated):
- "Purchase premium route information"
- Cost: 15 coins (33% of 45 coin reserve, meaningful)
- Outcome: Complete information, relationship +1

Path 3 (Challenge):
- "Engage in friendly conversation, persuade gently"
- Cost: 3 Resolve (A5 standard)
- Success: Detailed information, relationship +2, +1 Understanding
- Failure: Basic information, relationship +0

Path 4 (Fallback):
- "Offer to help with stall, build trust over time"
- Cost: 3 time blocks
- Outcome: Complete information, relationship +1

Validation:
- ✓ Player has Rapport 3, gate at 2 (accessible)
- ✓ 15 coins meaningful (33% of wealth)
- ✓ All paths reach base outcome (information received)
- ✓ Stat-gated path has best rewards (validates specialization)
- ✓ Fiction supports mechanics (friendly merchant, lower Rapport requirement)
- ✓ Orthogonal costs (stat/money/challenge/time)
- ✓ Choosing one closes others (sacrifice)
```

---

## 10.4 Anti-Pattern Recognition: Detecting Experience Failures

How do we detect when procedurally generated content violates experience laws?

### Automated Validation Questions

**Law 1 Violation (Identity Before Validation)**:
- Is there a stat requirement in A1-A3? → ❌ FAIL
- Is there a requirement higher than player's capability + 2? → ❌ FAIL
- Is progression blocked by stat gate? → ❌ FAIL

**Law 2 Violation (Orthogonal Expression)**:
- Is one choice clearly superior? → ❌ FAIL
- Do all choices have same cost type? → ❌ FAIL
- Is fallback just "worse version" of stat path? → ❌ FAIL

**Law 3 Violation (Scarcity Gradient)**:
- Is coin cost less than 15% of player wealth? → ❌ FAIL (trivial)
- Is coin cost more than 50% of player wealth? → ❌ FAIL (devastating)
- Has player accumulated wealth without cost scaling? → ❌ FAIL

**Law 4 Violation (Sacrifice Architecture)**:
- Can player "do both" options? → ❌ FAIL
- Can player retry situation for different reward? → ❌ FAIL
- Are choices presented sequentially instead of simultaneously? → ❌ FAIL

**Law 5 Violation (Emergent Tutorial)**:
- Does text explain mechanics? → ❌ FAIL
- Are there tutorial tooltips? → ❌ FAIL
- Do early situations teach through instruction instead of experience? → ❌ FAIL

**Law 6 Violation (Capability Emergence)**:
- Does requirement appear for stat player never built? → ❌ FAIL
- Do tags from past choices affect this situation? → If no, ✓ WARNING
- Does NPC remember past interaction? → If no, ✓ WARNING

**Law 7 Violation (Verisimilitude)**:
- Does friendly NPC require higher stat than hostile NPC? → ❌ FAIL
- Does cost make no narrative sense? → ❌ FAIL
- Does requirement contradict NPC personality? → ❌ FAIL

---

## 10.5 The Meta-Goal: Identity Formation Over Power Accumulation

**Traditional RPG**: Player accumulates power to overcome challenges

**Wayfarer**: Player discovers identity through choices that close possibilities

**The Shift**:
- Not "what build is optimal?" but "who am I becoming?"
- Not "how do I max all stats?" but "which stats matter to me?"
- Not "what's the correct answer?" but "what do I value?"
- Not "am I strong enough?" but "am I the kind of person who does this?"

**Procedural Implementation**:
- Every situation asks "who are you in this context?"
- Every choice represents different answer to that question
- Choosing means sacrificing alternatives (you cannot be all things)
- Specialization emerges from pattern of sacrifices
- Game validates emerging identity through requirements that match capability

**Experience Goal**:
- Player finishes session thinking "I'm becoming a diplomat" not "I gained 3 stat points"
- Player regrets unchosen paths (emotional investment in choices)
- Player sees past choices echoing forward (continuity of self)
- Player feels weight of decisions (scarcity and sacrifice matter)
- Player trusts the game (verisimilitude and fairness)

---

## 10.6 Summary: The Laws Enable Emergence

These seven laws are sufficient to generate meaningful player experience across infinite procedural content:

1. **Identity Before Validation**: Build capability before requiring it
2. **Orthogonal Expression**: Choices represent personality, not power
3. **Scarcity Gradient**: Pressure scales with power
4. **Sacrifice Architecture**: Choosing means losing
5. **Emergent Tutorial**: Experience teaches, not instruction
6. **Capability Emergence**: Past echoes forward
7. **Verisimilitude in Fiction**: Mechanics emerge from narrative

Applied correctly, these laws create:
- Identity formation through expression
- Emotional weight through sacrifice
- Specialization through repeated choices
- Investment through continuity
- Trust through verisimilitude

The player doesn't need to know these laws exist. They simply experience the emergent properties: choices that matter, resources that feel precious, identity that develops organically, and a world that remembers and responds.

This is the difference between a game with meaningful choices and a game with choice menus.

---

**Document Status:** Production-ready experience design specification
**Last Updated:** 2025-11
**Maintained By:** Design team
