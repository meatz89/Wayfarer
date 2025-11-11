# The Requirement Inversion Principle

## Why This Document Exists

Most developers, when designing progression systems, default to boolean gate patterns: "if player has X, unlock Y". This creates Cookie Clicker-style checklist completion, not strategic depth. This document teaches a fundamentally different architecture based on **The Life and Suffering of Sir Brante**, a masterclass in creating impossible choices through resource scarcity and state management.

**Target Audience:** Developers who think in terms of "requirements", "prerequisites", and "unlocks" - and need to learn a better way.

**📜 HISTORICAL NOTE (2025-01):**
This document was written during the Goal/Obstacle architecture era (pre-2025). Wayfarer has since migrated to Scene/Situation architecture. When reading this document:
- **"Goal"** → Read as **"Scene"**
- **"Obstacle"** → Read as **"Scene"** (multi-situation container)
- **Core Principle Unchanged:** Entities spawn into world immediately, requirements filter visibility/selectability, not spawning.

The principle remains architecturally sound. Only entity names have changed. For current implementation, see GLOSSARY.md and ARCHITECTURE.md.

---

## Part 1: The Problem - Boolean Gate Disease

### What Developers Do By Default

```
if (player.HasRope) {
    EnableClimbingAction();
}

if (player.CompletedQuest("phase_1")) {
    UnlockQuest("phase_2");
}

if (player.Knowledge.Contains("mill_sabotage")) {
    ShowInvestigationOption();
}
```

**The symptoms:**
- Properties named `hasX`, `completedY`, `isUnlocked`
- Methods like `CheckPrerequisites()`, `MeetsRequirements()`
- Content that exists but is hidden, waiting for flag to reveal it
- Language: "Requirements", "Prerequisites", "Unlock", "Lock"

### Why Boolean Gates Destroy Games

**No Strategic Depth:**
- Unlocks are "free" - no resource cost to reveal content
- Just checking: Did you do the thing? (Yes/No)
- Once unlocked, stays unlocked forever (no depletion)

**No Opportunity Cost:**
- Actions don't compete for shared resources
- Can pursue everything given enough time
- No forced choice between competing priorities
- Optimal path always exists (forums discover it)

**Linear Progression:**
- A → B → C → D (railroad)
- Later content strictly better than earlier
- No meaningful trade-offs
- Just a checklist to complete

**No Specialization:**
- Eventually max everything
- No builds, no identity
- All players converge to same endpoint
- No interesting failures or sacrifices

**Cookie Clicker Pattern:**
- Click enough → Unlock next tier
- Click more → Unlock next tier
- Progression without decisions
- Expansion without constraint

---

## Part 2: The Insight - How Sir Brante Actually Works

### Overview: A Multi-Layered Resource System

Sir Brante doesn't ask: "What has the player unlocked?"

Sir Brante asks: "What can the player afford given their current reality?"

**The architecture has FIVE distinct resource layers, THREE choice patterns, and STATE-BASED content. These are NOT interchangeable - each serves different purposes and behaves differently.**

---

## Layer 1: Resource Type Architecture

### Resource Type 1: Personal Stats (Capability Thresholds)

**Concrete Stats:**
- Valor, Eloquence, Diplomacy, Manipulation, Scheming, Theology
- Built from childhood stats: Determination + Path-specific stat → Adult stat
- Example: Determination 6 + Nobility 8 = Valor 14

**Numeric Ranges:**
- Childhood: 0-8 typical range
- Adolescence: Accumulate to 8-9 in main stats, 4-5 in others
- Adult (combined): 11-20 range
  - 13-14 considered "comfortable"
  - 16 considered "excessive" (too high causes problems!)
  - 20 is maximum for main stats

**How They Work - Arithmetic Comparison:**

**Scene presents choice:**
```
"Confront the magistrate with evidence"
Requires: Valor >= 14
Your Valor: 12
Result: DISABLED (but VISIBLE with exact requirement shown)
```

**Player sees:**
- "Requires Valor 14, you have 12"
- Gap is 2 points (perfect information)
- Can plan: "I need 2 more Valor to access this option"

**Scene presents different choice:**
```
"Negotiate a compromise through diplomacy"
Requires: Diplomacy >= 12
Your Diplomacy: 16
Result: ENABLED (you exceed requirement)
```

**Critical Understanding:**
- NOT: "hasHighValor" boolean → true/false
- YES: Valor 12 vs requirement 14 → arithmetic comparison
- Player calculates EXACTLY what they can afford
- No mystery gates - perfect information

**Sweet Spots Exist (Not Just "More Is Better"):**

Research finding from player guides:
- Determination/Perception in childhood: 5/5 or 5/6 ideal, 7/0 creates problems later
- Adult stats: 14 is "very comfortable", 16 is "excessive"
- Justice stat: 7-9 ideal, reaching 10 triggers early revolt (TOO HIGH)

**Why sweet spots matter:**
- Balanced development matters (7/0 split causes issues)
- Too high can trigger auto-consequences (thresholds)
- Can't max everything (scarcity forces choices)

### Resource Type 2: Per-Person Relationships (Individual Capital)

**What This Is:**
- Separate numeric score for EACH character
- Mother: Your relationship with Mother (independent)
- Father: Your relationship with Father (independent)
- Stephan: Your relationship with Stephan (independent)
- etc.

**Numeric Range:**
- Approximately -5 to +5 (tight bipolar scale)
- Negative: Hostility, anger, betrayal
- 0: Neutral, indifferent
- Positive: Trust, affection, loyalty
- Each point is PRECIOUS (small scale)

**Threshold Tiers:**

From research findings:
- **Relationship 2+**: Unlocks basic persuasion options
  - Example: Father persuasion (+1 Nobility, +1 Spirituality, +1 Unity, -1 Reputation, -5 Willpower)
- **Relationship 3+**: Unlocks multiple trade-off options with that person
  - Various choices that leverage the relationship
- **Relationship 4+**: Unlocks special training events
  - Grants +2 stat bonuses (valuable)
  - Represents deep trust enabling special help

**Asymmetric Costs (Relationship Triangulation):**

Real example from research:
```
"Demand that Mother be introduced to the guest"
Effect:
  Reputation +1 (social standing improves)
  Unity -1 (family cohesion damaged)
  Lydia (Mother) -1 (she's hurt by this)
Net: -1 (you damaged two relationships for one gain)
```

**Why this matters:**
- Actions affect MULTIPLE relationships simultaneously
- Helping one person can damage another (triangulation)
- Not 1:1 trades - asymmetric multi-way effects
- Forces choice: Which relationships matter most?

**Relationship Depletion Through Use:**

Critical pattern - asking favors COSTS the relationship:
```
Your relationship with Stephan: +5
Choose: "Ask Stephan to lie for you"
Effect: Relationship -2
After: Stephan +3
```

Next scene:
```
"Ask Stephan for financial help"
Requires: Stephan >= +4
Your relationship: +3
Result: DISABLED (you burned that capital)
```

**The strategic reality:**
- Relationships are CAPITAL you can SPEND
- Asking favors DEPLETES the resource
- Can't ask infinite favors - relationship degrades
- Must RATION who you ask and when
- Real relationship management, not boolean "isFriend" flag

### Resource Type 3: Family-Level Collective Resources (Group State)

**Three Collective Resources:**
- **Unity** (0-10): Family cohesion, how well family works together
- **Reputation** (0-10+): Social standing in society
- **Wealth** (0-10+): Economic resources, money available

**These Are COLLECTIVE (Not Personal):**
- Affect entire family as unit
- Actions by ANY family member can change them
- Represent family as entity, not individual capability

**Unity (Family Cohesion):**

Thresholds from research:
- **Unity 7+**: Enables "Unite the Family" event at mother's deathbed
  - Critical narrative moment requiring threshold
  - Can only occur if family cohesion high enough
- **Unity 10**: Prevents family member deaths in later crises
  - Protective threshold against worst outcomes
  - Requires significant investment to achieve

Example Unity costs:
```
"Hug Mother during crisis"
Effect: Unity +1 (from 6 to 7), Lydia +1 (from 2 to 3)
```

```
"Prioritize Father's advice over Mother"
Effect: Unity -1 (from 7 to 6), Lydia -1 (from 3 to 2), Reputation +1
```

**Reputation (Social Standing):**

Critical finding from research:
- Reputation can drop to 0 (Dishonored status)
- Example: Adopting Gloria into house → Reputation -10 (massive penalty)
- **Dishonored status has cascading consequences:**
  - Closes career paths
  - Blocks social opportunities
  - Other nobles refuse to interact
  - Permanent scar on family name

Recovery mechanism:
- Wealth 10 unlocks special events that can restore Reputation
- But building Wealth 10 requires sacrificing other progression
- Trade-off: Spend resources on Wealth OR on stats/relationships

**Wealth (Economic Power):**

Thresholds:
- **Wealth 10**: Unlocks special crisis mitigation options
  - Can buy way out of certain disasters
  - Access to opportunities requiring economic capital
  - Reputation recovery events require this

Example from research:
```
"Land Dispute - Accept Bribe"
Effect:
  Wealth +1 (5 to 6)
  Justice +1 (3 to 4) [helping commoners]
  Order -1 (8 to 7) [undermining law]
  Career -1 (5 to 4) [corruption discovered]
Net: 0 (lateral trade, not progression)
```

**Why Collective Resources Matter:**

They compete with personal progression:
- Build Unity → Spend actions on family, not stat training
- Build Wealth → Sacrifice immediate power for crisis insurance
- Build Reputation → May require compromising values

Can't maximize all:
- Time spent building Unity ≠ time building personal Valor
- Resources finite, must choose: Family or Self?

### Resource Type 4: Career-Path Meters (Path-Specific Progression)

**Path-Exclusive Stats:**

Each Lot (career path) has unique meters:
- **Noble Path**: Justice, Order, Career
- **Priest Path**: Church, Tolerance of Faiths, Inquisition Power
- **Lotless Path**: Rebellion, Underground Network, Popular Support

**These stats ONLY EXIST on their respective paths:**
- Justice stat doesn't exist for Priest
- Church stat doesn't exist for Noble
- Not "locked" - literally don't exist in other realities

**Dangerous Thresholds (Auto-Consequences):**

Critical research finding:
```
Justice stat reaches 10
→ AUTOMATIC CONSEQUENCE: Revolt breaks out early
→ No player choice, threshold triggers event
→ Player wanted justice, got TOO MUCH justice, catastrophe
```

```
Career stat at 7+ after specific event with certain character
→ AUTOMATIC ESCALATION: Career forced to 10
→ Triggers "Night of the Serpents" event
→ Unavoidable once threshold crossed
```

**Sweet Spots for Career Meters:**

From player guides:
- Justice 7-9 is ideal range
- Justice 10 triggers disaster (TOO HIGH)
- Order too high → Iron fist rule (authoritarianism)
- Faith too high → Persecution of Old Faith (extremism)

**The pattern:**
- Extremes in EITHER direction cause problems
- Moderation creates better outcomes than maximization
- Game punishes min-maxing through auto-consequences

**Cascading Failure Example from Research:**

Progressive Judge path:
```
1. Choose to convict corrupt Otton (justice choice)
   Requires: Justice 7+
   Effect: Justice increases

2. CONSEQUENCE: Career destroyed
   (You defied powerful interests)

3. CONSEQUENCE: Nobles hate you
   (You prosecuted one of them)

4. CONSEQUENCE: Reputation plummets
   (Social standing collapses)

5. CONSEQUENCE: Family disgraced
   (Your fall drags them down)

6. CONSEQUENCE: True Death
   (Cascading collapse leads to end)
```

Player quote: "Can't get Otton without screwing your career"

**The insight:**
- Some choices create unavoidable cascades
- Justice and Career are in tension
- Can't optimize both - must sacrifice something
- Game enforces realistic consequences of extremism

### Resource Type 5: Meta-Resources (System-Level Mechanics)

**Willpower (Meta-Stat, Spending and Recovery):**

**Range:** 0-10 (very tight, precious)

**Functions:**
1. **Bypass Stat Requirements** (Alternative Path):
```
Choice requires: Valor 18 OR Willpower 10
Your Valor: 12 (too low)
Your Willpower: 10 (meets alternate requirement)
Result: ENABLED via willpower bypass
```

2. **Pay Costs for Values-Driven Choices:**
```
"Refuse corruption and stand on principles"
Cost: Willpower -5
Effect: Maintains integrity but drains resolve
```

3. **Can Be REGAINED** (Unlike most resources):
- Certain story events restore willpower
- Represents renewed determination
- Not one-way depletion

**The strategic choice:**
- Spend willpower to access options you can't stat-afford?
- Or save willpower for critical moral choices later?
- Following your heart has mechanical cost

**Deaths (Limited Resets with Trade-Offs):**

**System:**
- 4 Deaths total in entire game
- 3 "Lesser Deaths" → Can revive with bonuses
- 1 "True Death" → Game Over, cannot recover

**Lesser Deaths Grant Bonuses:**
- Dying isn't just setback - you gain something
- Examples from research:
  - "Death grants +1 to chosen stat"
  - "Death unlocks special reputation event"
  - "Death provides family unity boost"
- Trade-off: Accept death consequence for power boost

**Strategic Use:**
- Deaths are RESOURCE (limited to 4)
- Can deliberately accept death for tactical gain
- But 4th death is final - permanent fail state
- Managing death budget across entire game

---

## Layer 2: Three Choice Pattern Types

**CRITICAL: Not all choices are structured the same. There are three distinct patterns, and which pattern appears depends on VERISIMILITUDE (what makes narrative sense).**

### Choice Pattern 1: BUILDING (Net Positive - True Progression)

**Structure:**
- Pure gains, no costs (or minimal costs)
- Single resource increases
- No trade-offs, no sacrifices

**Examples:**
```
"Study theology texts in library"
Effect: Theology +1
Cost: Time (1 segment)
Net: Positive (you gained capability)
```

```
"Practice swordsmanship with instructor"
Effect: Valor +1
Cost: Time
Net: Positive (skill improvement)
```

**Frequency:** Relatively rare (approximately 20-30% of choices)

**When This Pattern Appears (Verisimilitude):**
- Training scenes (learning makes sense)
- Study and practice (skill development)
- Peaceful advancement (no opposition)
- Relationship building through time (trust accumulation)

**Why This Exists:**
- You DO progress over time (not pure lateral movement)
- Represents genuine learning and growth
- But progress is SLOW (one point at a time)
- Must be earned through time investment

### Choice Pattern 2: TRADING (Lateral/Small Net - Priority Management)

**Structure:**
- Asymmetric multi-resource exchanges
- Gain in one area, lose in others
- Net can be negative, zero, or small positive
- Forces priority decisions

**Real Examples from Research:**

```
"Demand Mother be introduced to nobleman"
Effect:
  Reputation +1 (3 to 4) [social standing gains]
  Unity -1 (7 to 6) [family cohesion damaged]
  Lydia -1 (3 to 2) [mother hurt by this]
Net: -1 (damaged two relationships for one gain)
Pattern: Social climbing at family's expense
```

```
"Land Dispute - Accept Bribe"
Effect:
  Wealth +1 (5 to 6) [personal enrichment]
  Justice +1 (3 to 4) [helping commoners with money]
  Order -1 (8 to 7) [undermining rule of law]
  Career -1 (5 to 4) [corruption discovered]
Net: 0 (pure lateral trade)
Pattern: Corruption for good cause
```

```
"Justice for All - Confront superiors"
Effect:
  Career +2 (4 to 6) [professional advancement]
  Willpower -5 (10 to 5) [mentally exhausting]
Net: -3 (asymmetric! gained less than lost)
Pattern: Career success drains resolve
```

```
"Reject corruption and uphold integrity"
Effect:
  Willpower +5 (0 to 5) [moral strength renewed]
  Augustin -2 (1 to -1) [powerful ally turns hostile]
Net: +3 numerically, but lost critical relationship
Pattern: Integrity costs pragmatism
```

**Frequency:** Most common (approximately 50-60% of choices)

**When This Pattern Appears (Verisimilitude):**
- Political decisions (helping one faction angers another)
- Moral dilemmas (integrity vs expedience)
- Relationship triangulation (can't please everyone)
- Resource allocation (spend money OR time OR influence)
- Values conflicts (what matters more to you?)

**Why This Pattern Dominates:**
- Life is about managing competing priorities
- Can't optimize everything simultaneously
- Reveals character through what you sacrifice
- Creates IMPOSSIBLE CHOICES (all options valid, all have costs)

**The Strategic Question:**
Not: "Which choice is better?"
But: "Which cost am I willing to accept?"

### Choice Pattern 3: CRISIS (Net Negative - Damage Control)

**Structure:**
- All options hurt
- Question is: Which loss is least bad?
- No good outcomes available
- Managing disaster, not avoiding it

**Real Example from Research:**

```
"The Lot of Suffering"
[Child is punished severely]
Effect:
  Willpower -5 (0 to -5) [spirit broken]
  Lydia +1 [mother's sympathy]
Net: -4 (significant loss)
Pattern: Accepting suffering, small consolation
```

**Frequency:** Occasional but impactful (approximately 15-20%)

**When This Pattern Appears (Verisimilitude):**
- Emergencies requiring immediate response
- Failures cascading into consequences
- Disasters where all options bad
- Price paid for earlier mistakes
- Situations where "winning" isn't possible

**Why This Pattern Exists:**
- Sometimes life just hurts
- Not every situation has good option
- Consequences of earlier choices catch up
- Acceptance of loss is required
- Verisimilitude: Real tragedies happen

**Crisis + Specialization = Forced Bad Choices:**

```
Mother dying, needs medicine
Requires: Wealth 3 to buy medicine
Your Wealth: 1 (insufficient)

Available options:
1. "Let her die" - Preserve wealth, lose Mother
2. "Steal money" - Get wealth, lose Reputation, risk Career
3. "Beg nobles for help" - Requires Reputation 5, you have 2 (DISABLED)

No good option exists because you specialized elsewhere
```

**The pattern:**
- Specialized in stats, not wealth
- Crisis requires resource you don't have
- Forced into terrible choice
- Specialization created vulnerability

---

## Layer 3: Specialization Architecture

### The Lot Choice (State Transition - Changes Everything)

**Critical Decision at End of Adolescence:**
- Choose your Lot (social caste/career path)
- Three options: Noble, Priest, Lotless
- PERMANENT choice, defines entire adult life

**What Changes EVERYTHING:**

**1. Exclusive Stats Appear/Disappear:**
- Noble gets: Justice, Order, Career stats
- Priest gets: Church, Tolerance, Inquisition Power stats
- Lotless gets: Rebellion, Underground Network, Popular Support stats
- These stats ONLY EXIST on their respective paths

NOT: "Priest stats are locked for Nobles"
YES: "You are a Noble, Priest stats don't exist in Noble reality"

**2. Entirely Different Scenes:**
- Noble attends court sessions, legal proceedings, high society events
- Priest studies theology, performs rituals, investigates heresies
- Lotless joins underground, recruits rebels, plans insurgency

NOT: "Same scenes with different flavor text"
YES: "Completely different content - Noble scenes literally don't exist for Priest"

**3. Different Romantic Interests:**
- Noble path: Octavia Milianidas (high society romance)
- Priest path: Jeanne (crisis of faith storyline)
- Lotless path: Sophia (rebellion comrade)

NOT: "All three available but one is 'canon'"
YES: "Only one exists per path - others aren't in your story"

**4. Different Substories:**
- Noble: La Tari cult mystery, political intrigue
- Priest: Old Faith vs New Faith conflict, magical manifestations
- Lotless: Underground resistance, overthrowing empire

**This Is STATE-BASED CONTENT:**
- Your state (Lot choice) defines what exists
- Content isn't hidden behind gates
- Content literally doesn't exist for wrong state
- Verisimilitude: A priest doesn't live a noble's life

### Forced Specialization Through Scarcity

**The Math:**
- Research finding: 78 total stat points at adulthood
- Six stats total: Valor, Eloquence, Diplomacy, Manipulation, Scheming, Theology
- Range per stat: 11-20
- To max all six: Would need 120 points (20 × 6)
- You have: 78 points
- Conclusion: CANNOT max everything, MUST specialize

**Stat Distribution by Path:**

**Noble builds:**
- Valor 18, Diplomacy 16 (primary stats for Noble)
- Eloquence 11, Theology 10, Manipulation 12, Scheming 11 (neglected)
- Strong in authority and social standing
- Weak in faith and subterfuge

**Priest builds:**
- Eloquence 18, Theology 17 (primary stats for Priest)
- Valor 10, Manipulation 11, Diplomacy 13, Scheming 9 (neglected)
- Strong in persuasion and religious understanding
- Weak in combat and cunning

**Lotless builds:**
- Scheming 17, Manipulation 18 (primary stats for Lotless)
- Valor 12, Eloquence 13, Diplomacy 11, Theology 8 (neglected)
- Strong in subterfuge and manipulation
- Weak in direct authority and faith

**The Strategic Consequence:**
- Specializing gives peaks of excellence
- But creates valleys of vulnerability
- Crisis may require your weak stat
- Then you're helpless

### Specialization Creates Vulnerability (The Core Tension)

**How Scenes Present:**

```
Scene: "Confrontation with Corrupt Official"
Presents 4 options:

Option A: "Intimidate with authority"
  Requires: Valor 14
  Your Valor: 18 (ENABLED - you specialized here)

Option B: "Persuade through eloquent argument"
  Requires: Eloquence 16
  Your Eloquence: 10 (DISABLED - you neglected this)

Option C: "Manipulate with cunning scheme"
  Requires: Manipulation 15
  Your Manipulation: 11 (DISABLED - you neglected this)

Option D: "Retreat and find another way"
  No requirement (ENABLED - always available)
  But: Worst outcome (loses time, gains nothing)
```

**If you specialized in Valor:**
- Option A available (your strength)
- Options B and C disabled (your weaknesses)
- Can handle THIS crisis effectively

**Different scene, different requirements:**

```
Scene: "Theological Debate with Inquisitor"
Presents 4 options:

Option A: "Assert authority through force"
  Requires: Valor 16
  Your Valor: 18 (ENABLED)
  But: Wrong tool for situation, creates backlash

Option B: "Counter with theological knowledge"
  Requires: Theology 18
  Your Theology: 10 (DISABLED - crisis requires what you lack)

Option C: "Deflect with cunning wordplay"
  Requires: Manipulation 14
  Your Manipulation: 11 (DISABLED)

Option D: "Concede and back down"
  No requirement (ENABLED)
  Result: Humiliation, loss of reputation
```

**Now your specialization is a WEAKNESS:**
- Option A (your strength) creates worse outcome than silence
- Option B (actual solution) requires stat you don't have
- Forced into Option D (accepting loss)

**Player quote from research:**
"There are 4 choices, but I can only choose 1 (maybe 2 if lucky) because I don't meet most requirements"

**The Reality of Specialization:**
- Excel in chosen domain (enable specialty options)
- Vulnerable when wrong crisis appears (disabled, forced into bad choices)
- No build is universally strong (all have weaknesses)
- Player VALUES determine build, not optimization

---

## Layer 4: Threshold Systems and Auto-Consequences

### Sweet Spots vs Dangerous Extremes

**The Counterintuitive Truth: Higher Isn't Always Better**

Research findings on ideal ranges:

**Childhood Stats:**
- Determination/Perception: 5/5 or 5/6 IDEAL
- 7/0 split: Creates problems later (imbalanced)
- Both stats matter for combined adult stats

**Adolescence Stats:**
- Main stats: 8-9 IDEAL
- Other stats: 4-5 adequate
- Don't push too high in adolescence (save growth for adult events)

**Adult Stats:**
- 13-14 described as "very comfortable"
- 16 described as "excessive" (too high causes issues)
- 20 is absolute maximum (rarely beneficial)

**Career Meters:**
- Justice: 7-9 IDEAL, 10 triggers early revolt (TOO HIGH)
- Order: Moderate levels maintain stability, maxed creates iron fist
- Faith: Balance required, extremes cause persecution

**Why Sweet Spots Matter:**

1. **Balanced development beats min-maxing:**
   - Spreading points creates flexibility
   - One maxed stat, one zero stat → Worse than two moderate stats

2. **Extreme values trigger auto-consequences:**
   - Thresholds crossed = automatic events
   - Sometimes you don't want to cross threshold
   - More isn't always better

3. **Efficiency vs effectiveness:**
   - 14 in a stat handles most checks
   - Pushing to 18 uses points that could go elsewhere
   - Diminishing returns on high values

### Automatic Threshold Triggers (No Player Choice)

**What This Means:**
- Certain resource values trigger events automatically
- Player has NO CHOICE when threshold crossed
- Consequence is UNAVOIDABLE
- Represents realistic loss of control at extremes

**Real Examples from Research:**

```
Justice stat reaches 10
→ AUTOMATIC TRIGGER: Revolt breaks out early
→ No player input, event just happens
→ Unavoidable consequence of extremism
```

**Why this triggered:**
- You pursued justice relentlessly
- Each just decision increased Justice stat
- Hit threshold 10
- System: "Too much justice destabilizes society"
- Revolt erupts regardless of player intent

```
Career stat at 7+ after specific deal with character
→ AUTOMATIC ESCALATION: Career jumps to 10
→ Triggers "Night of the Serpents" event
→ Cannot be avoided once deal made
```

**The mechanism:**
- Certain combinations create runaway effects
- Career 7 + Specific Character Deal = Auto-escalation
- Game recognizes: "This path leads to extremism"
- Forces consequence automatically

```
Order stat maximized (20)
→ AUTOMATIC STATE: Iron Fist Rule
→ Authoritarian governance established
→ New scenes reflect this reality
```

```
Church stat maximized (20)
→ AUTOMATIC STATE: Persecution of Old Faith
→ Religious extremism triggers purges
→ Relationship consequences cascade
```

**Player quote from research:**
"Deals with certain characters push your stats too high and railroad you into dark paths"

**The Design Philosophy:**
- Extremism has costs (verisimilitude)
- Player can't perfectly control all outcomes
- Some consequences are unavoidable once set in motion
- Adds weight to decisions (can't just undo later)

### Cascading Failure Patterns

**What Cascading Failure Means:**
- One consequence triggers another
- Which triggers another
- Player watches helpless as dominoes fall
- No way to stop once cascade starts

**Real Example from Research - Progressive Judge Path:**

```
DECISION: "Convict Otton" (corrupt noble)
  Requires: Justice 7+
  Your Justice: 8 (ENABLED)
  Effect: Justice increased, Otton convicted

↓

CONSEQUENCE 1: Career destroyed
  (You defied powerful political interests)
  Career drops significantly

↓

CONSEQUENCE 2: Nobles turn hostile
  (You prosecuted one of their own)
  Multiple noble relationship scores plummet

↓

CONSEQUENCE 3: Reputation collapses
  (Social standing destroyed by noble hostility)
  Reputation drops to near 0

↓

CONSEQUENCE 4: Family disgraced
  (Your fall drags them down)
  Unity drops, family members suffer

↓

CONSEQUENCE 5: True Death
  (Cascading collapse leads to game over)
  Fourth death triggered
  Game ends
```

**Player quote:**
"You can't get Otton that way and not screw your career"

**The lesson:**
- Some paths have unavoidable cascades
- Game enforces realistic consequences
- Justice and Career are in TENSION (can't maximize both)
- Must accept sacrifice or avoid trigger entirely

**Another Pattern - Relationship Cascade:**

```
Low Wealth (2) + Mother Sick = Crisis

Available options all terrible:
1. "Let mother die" → Lydia dies, Unity -5, Willpower -10
2. "Steal money" → Wealth +3, Reputation -8, Career -5
3. "Beg nobles" → Requires Reputation 5 (you have 1, DISABLED)

Choose option 2 (steal)
↓
Reputation -8 (drops to 0, DISHONORED)
↓
Career -5 (fired from position)
↓
No income source
↓
Wealth cannot recover
↓
Next crisis: Can't afford food
↓
Starvation penalties
↓
True Death
```

**The pattern:**
- Weak area exploited (low Wealth)
- Forced into desperate choice
- Desperate choice creates new crisis
- Spiral continues until death

**Why cascades exist:**
- Verisimilitude (real failures compound)
- Punish neglecting areas
- Force acceptance that you can't save everything
- Create dramatic narrative weight

---

## Layer 5: How Different Systems Interact

### Multi-Resource Trade-Offs Create Triangulation

**Simple trade-offs:** A +1, B -1
**Complex triangulation:** A +2, B -1, C -3, D +1, E -2

Real examples from research show choices affect 3-5 resources simultaneously.

**Example 1: Political Choice**
```
"Support commoners in land dispute"
Effect:
  Justice +1 (helping the weak)
  Order -1 (undermining nobility)
  Career -1 (opposing powerful interests)
  Wealth +1 (bribe from commoners)

Pattern: Help the poor, anger the powerful, accept career damage
```

**Example 2: Family vs Society**
```
"Adopt Gloria into noble house"
Effect:
  Unity +2 (family grows)
  Reputation -10 (society scandalized)
  Lydia +1 (mother approves)
  Father -2 (father opposes)

Pattern: Choose family over social standing
```

**Example 3: Integrity vs Pragmatism**
```
"Reject corrupt official's offer"
Effect:
  Willpower +5 (moral strength)
  Augustin -2 (powerful ally becomes enemy)
  Career -2 (blocked from advancement)
  Order +1 (upholding law)

Pattern: Maintain principles, sacrifice advancement
```

**Why Multi-Resource Matters:**

**Creates genuine dilemmas:**
- Not binary good/bad
- Every choice has some gains, some losses
- Player must weigh: Which gains matter more? Which losses can I afford?

**Prevents optimal strategies:**
- No "always choose X" pattern
- Context determines best choice
- Different builds value different resources
- Impossible to maximize all simultaneously

**Reveals character:**
- What you sacrifice shows values
- Builds emerge from priority patterns
- Two players make different "correct" choices
- Narrative consequence of values

### Family Resources Gate Individual Opportunities

**The Mechanism:**
- Individual stat checks often have Family resource requirements
- Can't attempt option unless BOTH conditions met
- Forces investment in collective alongside personal

**Example:**
```
"Unite the Family" event (mother's deathbed)
Requires: Unity 7+ AND no secret plots AND all family alive

Check #1: Unity >= 7
  Unity is collective family resource
  Must have maintained family cohesion throughout game

Check #2: Haven't joined secret plots
  Certain earlier choices lock you out

Check #3: All family members alive
  If anyone died in earlier crises, fails

Result: All three required, missing any one = DISABLED
```

**Another Example:**
```
"Gain reputation through wealth"
Requires: Wealth 10

If Wealth 10:
  Unlocks special events that restore Reputation
  Can recover from Dishonored status

If Wealth < 10:
  Reputation loss is PERMANENT
  No recovery mechanism available
```

**The Strategic Tension:**

Build personal stats (Valor, Eloquence, etc.):
- Advantages: Access individual checks, excel in challenges
- Cost: Actions spent on self, not family
- Risk: Family resources stay low

Build family resources (Unity, Wealth, Reputation):
- Advantages: Unlock family events, crisis insurance
- Cost: Personal stats lower, weaker in individual checks
- Risk: Vulnerable in solo challenges

Can't maximize both:
- Time spent building Unity ≠ time training Valor
- Actions are finite resource
- Must choose: Family or Self?

**Different Builds, Different Vulnerabilities:**

**"Family First" build:**
- Unity 10, Reputation 8, Wealth 9
- Personal stats: All 12-13 (moderate)
- Strength: Family crises handled easily
- Weakness: Individual challenges difficult

**"Self-Made" build:**
- Valor 18, Eloquence 16, Diplomacy 15
- Family: Unity 4, Reputation 5, Wealth 3
- Strength: Individual challenges dominated
- Weakness: Family crises catastrophic

**Mixed Build:**
- Balanced across both
- Unity 7, Wealth 6, Reputation 6
- Personal stats: 14-15 in main, 10-11 in others
- Strength: Flexible, handles most situations
- Weakness: Never dominates anything

### Relationship Requirements Compound

**The Complexity:**
- Events can require MULTIPLE relationship thresholds
- Plus family resources
- Plus personal stats
- All must be met simultaneously

**Real Example from Research:**
```
Certain ending path requires:
  Stephan relationship 5+
  Robert relationship 5+
  Gloria relationship 1+
  Unity 10
  Reputation 7+
  Specific career choices made

Miss any one requirement = Entire path DISABLED
```

**Why This Is Hard:**
- Each relationship requires separate investment
- Some actions raise one, lower another (triangulation)
- Unity competes with other resources
- Must maintain all simultaneously through entire game

**Example Conflict:**
```
Scene: "Stephan asks you to lie for him"

Option A: "Lie for Stephan"
  Effect: Stephan +1, Reputation -2, Willpower -3

Option B: "Refuse to lie"
  Effect: Stephan -2, Reputation +1, Willpower +1

Your situation:
  - Need Stephan 5 for ending (currently 4)
  - Need Reputation 7 for ending (currently 6)
  - Need Willpower for other checks (currently 8)

Option A: Gets Stephan to 5 (✓) but drops Reputation to 4 (✗)
Option B: Drops Stephan to 2 (✗) but maintains Reputation (✓)

BOTH options close the ending path!
```

**The Impossible Choice:**
- Paths that require maintaining multiple high relationships
- Actions that help one relationship harm others
- NO way to maximize everything
- Must sacrifice some endings to pursue others

**Player quote from research:**
"Some situations have unavoidable deaths if you don't have an immaculate setup"

**The Design Truth:**
- Perfect optimization nearly impossible
- Requires foreknowledge (guides)
- Even then, trade-offs required
- Acceptance of loss is mandatory

---

## Part 3: The Architectural Patterns (How To Implement)

### Pattern 1: Resource Arithmetic (Tactical - What's Affordable)

**Use Case:** Player has resource pool, action has requirement, compare numerically.

**The Question:**
"Can I afford this action RIGHT NOW?"

**The Mechanism:**
```
if (player.Resource >= action.RequiredResource) {
    EnableAction();
} else {
    DisableButShowAction();
}
```

**UI Presentation:**
```
Action: "Intimidate the guard"
Requires: Valor 14
Your Valor: 12
Status: DISABLED
Gap: Need 2 more Valor
```

**Player sees:**
- Exact requirement (14)
- Exact current value (12)
- Exact gap (2)
- Can plan: "Where can I get +2 Valor?"

**Why Arithmetic, Not Boolean:**

Boolean approach:
```
if (player.HasHighValor) {
    EnableAction();  // What is "high"? Mystery.
}
```

Arithmetic approach:
```
if (player.Valor >= 14) {
    EnableAction();  // Exact threshold, no mystery.
}
```

**Benefits:**
- Perfect information (player calculates exactly)
- No mystery gates (all requirements visible)
- Enables planning (player knows what they need)
- Compiler enforces (int comparison, not string checking)

**Scale Matters:**

Large scale (0-100):
- Valor 78, requires 75
- Gap is 3 points
- Feels arbitrary, unclear value of each point

Small scale (0-20):
- Valor 12, requires 14
- Gap is 2 points
- Each point feels PRECIOUS
- Meaningful decision: Is +2 Valor worth the cost?

**Wayfarer Application:**

```
Card in hand during Social challenge
Initiative cost: 15
Player Initiative: 12
Status: Greyed out, shows "Requires 15 Initiative"
Gap: Need 3 more Initiative

Player can:
- Play Foundation cards to build Initiative
- Calculate: "If I play these two, I'll have 16"
- Plan exactly when card becomes affordable
```

### Pattern 2: Entity Spawning (Strategic - What Exists)

**Use Case:** Rewards CREATE new content entities that didn't exist before.

**The Question:**
"What opportunities exist in the world?"

**The Wrong Way (Boolean Gating):**
```
// ALL content exists from start, but hidden
Goal phase3Goal = gameWorld.AllGoals["phase_3"];
phase3Goal.IsLocked = true;  // Hidden behind gate

// Later, when "unlocking"
if (player.CompletedGoals.Contains("phase_2")) {
    phase3Goal.IsLocked = false;  // Reveal existing content
}
```

**The Right Way (Entity Spawning):**
```
// Phase 3 goal DOESN'T EXIST yet

// Reward system
public void ApplyGoalCardReward(GoalCardReward reward) {
    foreach (string goalId in reward.SpawnedGoals) {
        // CREATE new goal from template
        Goal newGoal = CreateFromTemplate(goalId);

        // ADD to world (now it exists)
        Location location = gameWorld.Locations[newGoal.LocationId];
        location.ActiveGoals.Add(newGoal);
    }
}

// UI simply displays what exists
public void RenderLocationGoals(Location location) {
    foreach (Goal goal in location.ActiveGoals) {
        ShowGoalButton(goal);  // No filtering, just show what exists
    }
}
```

**Key Differences:**

Boolean gating:
- Content exists from start (in code/data)
- Flag controls visibility
- Revealing hidden content
- IsLocked property

Entity spawning:
- Content doesn't exist initially
- Reward creates it
- Adding new content
- No lock property needed

**Why Entity Spawning Is Better:**

1. **Single Source of Truth:**
   - If it's in location.ActiveGoals, it exists and is visible
   - No dual state (exists but hidden)
   - No synchronization bugs

2. **Simpler UI Code:**
   - Just render what's in collection
   - No filtering logic
   - No "show if unlocked" checks

3. **Clearer Semantics:**
   - Empty collection = No goals here
   - Non-empty collection = Goals exist here
   - Matches conceptual model

4. **Enables Dynamic Content:**
   - AI can generate goals at runtime
   - Investigations can spawn varied obstacles
   - Rewards can create unique opportunities
   - Not limited to pre-authored content

**Wayfarer Application:**

```
Investigation Phase 2 completed

GoalCardReward:
  SpawnedGoals: ["phase_3_investigate_interior"]

Effect:
  Goal template "phase_3_investigate_interior" exists in catalog
  CreateGoalFromTemplate(template)
  Add to Location["mill_interior"].ActiveGoals

Result:
  Phase 3 goal NOW EXISTS (didn't before)
  UI shows new button at Mill Interior location
  Player sees new opportunity appeared
```

### Pattern 3: State-Based Content (Contextual - What Reality Am I In)

**Use Case:** Player state determines which content exists in their reality.

**The Question:**
"What scenes are valid for my current life situation?"

**The Wrong Way (Boolean Flags):**
```
if (player.IsNoble) {
    courtScenes.Locked = false;  // Unlock noble content
    rebelScenes.Locked = true;   // Lock rebel content
}
```

**The Right Way (State Defines Existence):**
```
public enum PlayerLot { Noble, Priest, Lotless }

public List<Scene> GetAvailableScenes(PlayerLot lot) {
    return gameWorld.Scenes.Where(scene =>
        scene.ValidLots.Contains(lot)
    ).ToList();
}

// Scene definition
public class Scene {
    public List<PlayerLot> ValidLots { get; set; }
}

// Noble scenes
new Scene {
    ValidLots = new List<PlayerLot> { PlayerLot.Noble }
    // This scene ONLY EXISTS for Nobles
}

// Priest scenes
new Scene {
    ValidLots = new List<PlayerLot> { PlayerLot.Priest }
    // This scene ONLY EXISTS for Priests
}
```

**Key Concept:**
- State is not a gate key
- State defines which REALITY you inhabit
- Different realities have different content
- Content isn't locked/unlocked - it exists or doesn't

**Examples:**

**Noble State:**
- Court scenes exist
- Legal proceeding scenes exist
- High society scenes exist
- Rebel scenes DON'T EXIST (you're not a rebel, not your reality)
- Priest scenes DON'T EXIST (you're not a priest, not your reality)

**Priest State:**
- Theological scenes exist
- Church ritual scenes exist
- Faith crisis scenes exist
- Court scenes DON'T EXIST (you're not a politician)
- Rebel scenes DON'T EXIST (you're not an insurgent)

**Why This Matters (Verisimilitude):**

A priest doesn't live a noble's life:
- Not "noble content is locked for priests"
- But "noble content doesn't exist in priest reality"
- Nobles attend court because that's what nobles DO
- Priests don't attend court because priests aren't nobles

**Wayfarer Application:**

```
Player specialization: High Insight/Cunning build

Scene: Investigation challenge
Available goal approaches:
  - "Systematic analysis" (requires Insight 14) → EXISTS (you have 16)
  - "Subtle observation" (requires Cunning 12) → EXISTS (you have 14)
  - "Forceful confrontation" (requires Authority 14) → DOESN'T EXIST (you have 8)

The Authority approach isn't "locked waiting for unlock"
It's not in your available options because you're not an Authority specialist
That's not your reality
```

### Pattern 4: Threshold Auto-Triggers (Consequence - No Player Choice)

**Use Case:** Resource crosses threshold, automatic consequence triggers.

**The Question:**
N/A - Player doesn't choose, system automatically responds to threshold.

**The Mechanism:**
```
public void UpdateResource(Resource resource, int delta) {
    resource.Value += delta;

    // Check thresholds
    foreach (Threshold threshold in resource.Thresholds) {
        if (resource.Value >= threshold.Value && !threshold.Triggered) {
            threshold.Triggered = true;
            TriggerEvent(threshold.EventId);
            // No player input, automatic consequence
        }
    }
}

// Example threshold
new Threshold {
    ResourceName = "Justice",
    Value = 10,
    EventId = "early_revolt",
    Description = "Justice extremism triggers social upheaval"
}
```

**Key Characteristics:**

1. **Automatic (Not Player Choice):**
   - Player doesn't decide if event triggers
   - System responds to threshold mechanically
   - Represents loss of control at extremes

2. **Irreversible:**
   - Once triggered, can't be untriggered
   - Threshold.Triggered flag prevents re-triggering
   - Permanent consequence

3. **Visible (Not Hidden):**
   - Player should see threshold approaching
   - UI shows: "Justice 9/10 - DANGER: 10 triggers revolt"
   - Perfect information about what happens at limit

**Why Auto-Triggers Exist:**

**Verisimilitude:**
- Real extremism has automatic consequences
- You can't perfectly control everything
- Some actions create unavoidable cascades

**Anti-Optimization:**
- Prevents pure min-maxing
- Forces moderation
- "More is better" isn't always true

**Dramatic Weight:**
- Crossing threshold is momentous
- No take-backs, no undo
- Player must live with consequences

**Wayfarer Application:**

```
Physical challenge session

Danger resource tracking:
  Danger: 8/10
  Threshold: 10 triggers injury and failure

Player plays risky card:
  "Reckless Leap"
  Effect: Breakthrough +3, Danger +2

After resolution:
  Danger: 10/10

AUTOMATIC TRIGGER:
  "You pushed too hard - the climb collapses"
  Health -15 (injury)
  Challenge: FAILED
  Return to Location (with injury consequence)

No player choice in failure - threshold crossed = automatic result
```

---

## Part 4: Why This Works (The Deep Truth)

### Creates Real Strategic Depth

**1. Heterogeneous Choice Types**

Player can't predict pattern:
- This scene: Building choice (pure gain)
- Next scene: Trading choice (multi-resource exchange)
- Next scene: Crisis choice (all options bad)

Can't develop single strategy:
- "Always maximize X" fails (some scenes don't have X)
- "Never sacrifice Y" fails (some scenes force it)
- Must be adaptive, flexible

**2. Asymmetric Exchanges**

Not 1:1 trades:
- "Justice for All": Career +2, Willpower -5 (gained 2, lost 5)
- "Reject Corruption": Willpower +5, Augustin -2 (gained, but lost ally)

Forces real math:
- Is +2 Career worth -5 Willpower?
- Depends on: What you need, what you can spare, what comes next
- Context-dependent optimization, not universal rules

**3. Specialization Creates Identity AND Vulnerability**

**Identity:**
- High Valor → You ARE an authority figure
- High Eloquence → You ARE a persuader
- Build defines playstyle and character

**Vulnerability:**
- High Valor, Low Cunning → Helpless in subtle situations
- High Eloquence, Low Valor → Helpless in confrontations
- Every build has Achilles heel

**No Perfect Build:**
- All builds sacrifice something
- All builds struggle sometimes
- Different builds for different values
- Your character IS your compromises

**4. Forced Sacrifice**

Research quote: "Can't get good ending in all areas"

You MUST accept:
- Some relationships will fail
- Some careers will be closed
- Some crises will go badly
- Some people will die

This isn't "skill issue":
- It's designed impossibility
- Resources too scarce to save everything
- Acceptance of loss is required

Creates emotional weight:
- Losses hurt because they were inevitable
- Couldn't save everyone, had to choose
- Character revealed through what you sacrificed

**5. Thresholds With Auto-Consequences**

Sweet spots exist:
- Justice 7-9 ideal, 10 causes disaster
- Moderation rewarded, extremism punished

Can't pure min-max:
- More isn't always better
- Maximizing can backfire
- Balance beats optimization

Loss of control:
- Cross threshold → Automatic consequence
- Can't undo, can't negotiate
- Must live with result
- Verisimilitude: Real extremism has costs

### Feels Like Managing Real Life

**Slow Net Progression (You Do Advance, But Painfully):**

Not Cookie Clicker:
- You don't just keep gaining
- Many scenes are lateral trades or losses
- Overall arc: Slow upward trend
- But path is jagged, not smooth

Advancement is EARNED:
- Through sacrifice (gave up X to get Y)
- Through suffering (accepted losses)
- Through time (slow point-by-point gains)
- Not freely granted

**Constant Priority Management:**

Not "Do everything eventually":
- Resources finite, time limited
- Must choose: Which fire to fight?
- Can't save everyone, must prioritize
- Triage, not completion

Real-world feeling:
- Life is about managing competing demands
- Can't optimize everything simultaneously
- Character revealed through priorities
- Some problems must be left unsolved

**Relationship Capital Depletion:**

Not binary friendship:
- Relationships are CAPITAL
- Asking favors COSTS the relationship
- Can't ask infinite favors
- Must ration who you ask, when

Verisimilitude:
- Real relationships deplete when used
- People get tired of being asked
- Trust is finite resource
- Must invest to maintain

**Specialization Creates Identity:**

You ARE something specific:
- Not "good at everything eventually"
- But "excellent here, weak there"
- Identity through constraints
- Character through limitations

**Acceptance of Loss Required:**

Not "perfect run possible":
- Some endings mutually exclusive
- Some people will die
- Some paths close permanently
- Cannot save everyone

Emotional maturity:
- Let go of perfectionism
- Accept consequences
- Live with regrets
- Real weight to decisions

### Verisimilitude Throughout

**Building Where Learning Makes Sense:**
- Study theology → Theology +1
- Practice swordsmanship → Valor +1
- Build relationship → Trust +1
- Peaceful progression in safe contexts

**Trading Where Factions Compete:**
- Help commoners → Anger nobles
- Uphold law → Lose flexibility
- Maintain integrity → Sacrifice advancement
- Political reality: Can't please everyone

**Crisis Where Emergencies Happen:**
- Mother dying → All options bad
- Revolt erupting → Choose side, lose others
- Family scandal → Damage control only
- Life sometimes just hurts

**State-Based Where Identity Matters:**
- Nobles live noble lives (court scenes)
- Priests live priest lives (theological scenes)
- Rebels live rebel lives (insurgency scenes)
- You are what you chose to become

**Auto-Triggers Where Consequences Inevitable:**
- Extreme justice → Society destabilizes
- Extreme order → Authoritarianism emerges
- Extreme faith → Persecution begins
- Real extremism has automatic costs

---

## Part 5: Application to Wayfarer

### Map Sir Brante Patterns to Wayfarer Systems

#### Resource Types in Wayfarer

**Personal Stats (Wayfarer Equivalent: Unified 5-Stat System)**

Sir Brante: Valor, Eloquence, Diplomacy, Manipulation, Scheming, Theology
Wayfarer: Insight, Rapport, Authority, Diplomacy, Cunning

Arithmetic comparison:
- Card requires Insight 3
- Player Insight 2
- Card greyed, shows "Requires Insight 3"
- Small scale (0-5 or 0-10), each point precious

**Per-Entity Resources (Wayfarer Equivalent: StoryCubes per NPC)**

Sir Brante: Relationship score per character (-5 to +5)
Wayfarer: StoryCubes per NPC (0-10)

Function:
- Build relationship through conversations
- Each cube reduces Social Doubt with that NPC
- Represents accumulated trust/familiarity
- Must invest separately in each relationship

**Collective Resources (Wayfarer: No Direct Equivalent)**

Sir Brante: Unity, Reputation, Wealth (family-level)
Wayfarer: N/A (solo adventure, no family)

Conceptual mapping:
- Could add Faction Reputation (collective standing with groups)
- Or Settlement Resources (town you're helping)
- But current design is personal-focused (no family)

**Career Meters (Wayfarer Equivalent: Challenge System Progression)**

Sir Brante: Justice, Order, Career, Church, etc. (path-specific)
Wayfarer:
- InvestigationCubes per location (Mental progression)
- Mastery Tokens per challenge type (Physical progression)
- StoryCubes per NPC (Social progression)

Sweet spots:
- Don't need to max all locations/NPCs
- Specializing in some creates expertise
- Neglecting others creates vulnerability

**Meta-Resources (Wayfarer Equivalent: Understanding, Health/Stamina)**

Sir Brante: Willpower (spend/regain), Deaths (limited resets)
Wayfarer:
- Understanding (tier unlock, persistent)
- Health/Stamina (depletion risk, recovery required)
- Focus (Mental capacity, finite per session)

#### Choice Patterns in Wayfarer

**Building Choices (Net Positive):**

Wayfarer contexts:
- Rest actions (recover Health/Stamina, pure gain)
- Training scenes (spend time to build stats, slow progression)
- Relationship building (invest time in NPC conversations)
- Peaceful investigation (no opposition, just learning)

Frequency: Should be minority (20-30%)
Verisimilitude: Where advancement makes narrative sense

**Trading Choices (Lateral/Small Net):**

Wayfarer contexts:
- Help NPC vs maintain schedule (Relationship vs Time)
- Thorough investigation vs time limit (Progress vs Time)
- Expensive equipment vs saving money (Preparation vs Economy)
- Use Focus vs use Stamina (Mental vs Physical approach)

Frequency: Should be majority (50-60%)
Verisimilitude: Competing priorities, realistic constraints

**Crisis Choices (Net Negative):**

Wayfarer contexts:
- Low resources force bad options (out of Health, must rest and lose time)
- Emergencies require resources you lack (need rope, don't have it)
- Cascading failures (injury → can't do Physical challenge → forced into worse alternative)
- Deadline pressure (must choose fast option with bad outcome vs safe option that fails deadline)

Frequency: Occasional but impactful (15-20%)
Verisimilitude: Sometimes situations are just bad

#### Specialization in Wayfarer

**The Stat Build:**

Player can't max all five stats (Insight/Rapport/Authority/Diplomacy/Cunning):
- Total XP limited
- Must prioritize 2-3 stats
- Creates build identity

**Specialist Build Examples:**

"Investigator" (Insight + Cunning):
- Dominates Mental challenges (observation, deduction)
- Weak in Social challenges (no Rapport/Authority)
- Perfect for investigation-focused play
- Vulnerable when forced into conversations

"Diplomat" (Rapport + Diplomacy):
- Dominates Social challenges (relationships, persuasion)
- Weak in Mental challenges (no Insight)
- Perfect for relationship-focused play
- Vulnerable in complex investigations

"Leader" (Authority + Diplomacy):
- Balanced Social (directive and balanced)
- Weak in Insight and Cunning
- Good at confrontation, weak at subtlety
- Vulnerable in investigations requiring subtle observation

**Specialization Creates Vulnerability:**

Scene presents investigation goal:
- Requires Insight 4 for best approach
- Your Insight: 2 (you specialized elsewhere)
- Forced into worse approach or avoid goal entirely
- Your build created weakness

Scene presents social goal:
- You have Rapport 4, Authority 1
- Goal has "Rapport 3" approach (ENABLED)
- Goal has "Authority 4" approach (DISABLED)
- Your build enabled one path, closed another

**Strategic Layer:**

Player must decide:
- Which challenge types to pursue (play to strengths)
- Which to avoid (accept can't do everything)
- How to mitigate weaknesses (equipment, preparation)
- When to accept suboptimal outcomes (forced into weak area)

### Implementation Guidelines for Wayfarer

**1. Use Arithmetic Comparison, Never Boolean Checks:**

Wrong:
```csharp
if (player.HasRope) {
    goal.IsUnlocked = true;
}
```

Right:
```csharp
if (player.Insight >= goal.RequiredInsight) {
    EnableGoal(goal);
} else {
    ShowGreyedGoal(goal, player.Insight, goal.RequiredInsight);
}
```

**2. Spawn Entities, Don't Reveal Hidden Content:**

Wrong:
```csharp
Goal phase3 = gameWorld.AllGoals["phase_3"];
phase3.IsHidden = false;  // Revealing existing content
```

Right:
```csharp
Goal phase3 = CreateGoalFromTemplate("phase_3");
location.ActiveGoals.Add(phase3);  // Creating new content
```

**3. Use State to Define Available Content:**

Wrong:
```csharp
if (player.CompletedIntro) {
    advancedGoals.Locked = false;
}
```

Right:
```csharp
List<Goal> available = gameWorld.Goals.Where(g =>
    g.ValidStates.Contains(player.CurrentState)
).ToList();
```

**4. Let Thresholds Auto-Trigger:**

Wrong:
```csharp
if (player.Danger >= 10) {
    AskPlayerIfTheyWantToFail();  // No, automatic
}
```

Right:
```csharp
if (player.Danger >= challenge.MaxDanger) {
    TriggerFailure();  // Automatic consequence
    ApplyInjury();
    EndChallenge(success: false);
}
```

**5. Use Small Scales for Weight:**

Wrong:
```csharp
public class Player {
    public int Insight { get; set; }  // Ranges 0-100 (meaningless scale)
}
```

Right:
```csharp
public class Player {
    public int Insight { get; set; }  // Ranges 0-5 or 0-10 (each point precious)
}
```

**6. Create Multi-Resource Trade-Offs:**

Wrong:
```csharp
// Simple gain
player.Insight += 1;
```

Right:
```csharp
// Complex trade
player.Insight += 1;
player.Focus -= 3;
player.Time -= 2;
npc.Relationship -= 1;  // Triangulation
```

**7. Mix Choice Patterns:**

Wrong:
```csharp
// All choices are building (pure gains)
foreach (var action in scene.Actions) {
    player.Stats[action.Stat] += action.Gain;
}
```

Right:
```csharp
// Different patterns
switch (action.Pattern) {
    case ActionPattern.Building:
        player.Stats[action.Stat] += action.Gain;
        break;
    case ActionPattern.Trading:
        ApplyMultiResourceEffects(action.Effects);  // Complex trade
        break;
    case ActionPattern.Crisis:
        ForceWorstOutcome(action.CrisisOptions);  // All bad
        break;
}
```

---

## Part 6: The Forbidden Vocabulary

### Language That Creates Boolean Gates (NEVER USE)

**"Requirements" / "Prerequisites"**
- Implies checking conditions
- Suggests content exists but gated
- Creates boolean thinking

**"Unlock" / "Lock"**
- Implies content hidden behind gate
- Suggests binary state (locked/unlocked)
- Wrong mental model

**"Has X" / "Completed Y"**
- Boolean property names
- True/false checks
- Gate pattern

**"If player has..." / "Check if..."**
- Condition checking language
- Boolean logic structure
- Wrong architecture

### Language That Creates Resource Systems (ALWAYS USE)

**"Costs X resource"**
- Resource depletion language
- Arithmetic concept
- Correct model

**"Requires Resource >= Threshold"**
- Arithmetic comparison
- Visible numeric check
- Perfect information

**"Creates new Goal entity"**
- Entity spawning language
- Addition to collection
- Content creation

**"State determines available scenes"**
- Context-based existence
- Validity concept
- Correct architecture

**"Arithmetic comparison"**
- Mathematical thinking
- Not boolean logic
- Type-safe

### Example Translations

**WRONG:** "Complete Phase 1 to unlock Phase 2"
**RIGHT:** "Phase 1 completion creates Phase 2 Goal at Location"

**WRONG:** "Requires knowledge of mill sabotage"
**RIGHT:** "Created when mill investigation completes"

**WRONG:** "Rope equipment unlocks climbing action"
**RIGHT:** "Finding rope creates climbing Goal entity"

**WRONG:** "Must have 10 Wealth to unlock reputation recovery"
**RIGHT:** "Wealth 10 creates reputation recovery event"

**WRONG:** "Check if player has completed prerequisites"
**RIGHT:** "Compare player resources against action costs"

**WRONG:** "Has high enough stat to unlock dialogue"
**RIGHT:** "Rapport 5 enables dialogue option via arithmetic comparison"

### Documentation Standards

**When Describing Actions:**
- ✅ "Action costs 15 Initiative"
- ✅ "Action requires Valor >= 14"
- ❌ "Action unlocks at high Valor"
- ❌ "Action has Valor prerequisite"

**When Describing Progression:**
- ✅ "Completing Goal creates new Obstacle"
- ✅ "Reward spawns Phase 3 entities"
- ❌ "Completing Goal unlocks next phase"
- ❌ "Progress gates later content"

**When Describing Systems:**
- ✅ "Player Insight 3 vs Card requirement 4 → Disabled"
- ✅ "Resource arithmetic determines availability"
- ❌ "System checks if player meets requirements"
- ❌ "Card locked until stat increases"

---

## Part 7: The Tests

### Test 1: Is This A Boolean Check?

**Ask:** "Am I checking true/false?"

If YES → **WRONG** (boolean gate)
If NO → Might be correct

Examples:

```csharp
// WRONG (boolean check)
if (player.HasRope) { ... }
if (player.CompletedQuest) { ... }
if (goal.IsUnlocked) { ... }

// RIGHT (arithmetic comparison)
if (player.Insight >= 3) { ... }
if (player.StoryCubes[npc] >= 5) { ... }
if (player.Health >= 30) { ... }
```

### Test 2: Does Content Exist Before Reward?

**Ask:** "Did this goal/scene/option exist before the reward was applied?"

If YES → **WRONG** (revealing hidden content)
If NO → Correct (spawning new content)

Examples:

```csharp
// WRONG (content pre-exists)
Goal goal = allGoals["phase_3"];
goal.IsHidden = false;  // Was hidden, now revealed

// RIGHT (content created)
Goal goal = CreateFromTemplate("phase_3");
location.ActiveGoals.Add(goal);  // Didn't exist, now does
```

### Test 3: Can Player Calculate Exactly?

**Ask:** "Can the player see exact requirement and exact gap?"

If NO → **WRONG** (mystery gate)
If YES → Correct (perfect information)

Examples:

```csharp
// WRONG (mystery)
UI: "You need to be stronger"
// How strong? Mystery.

// RIGHT (exact)
UI: "Requires Valor 14, you have 12"
// Exact gap: 2 points
```

### Test 4: Is There Opportunity Cost?

**Ask:** "Does choosing this action close other options?"

If NO → **WRONG** (free unlock, no strategy)
If YES → Correct (resource competition)

Examples:

```csharp
// WRONG (no opportunity cost)
CompleteQuest();
UnlockNextQuest();  // Free unlock, no cost

// RIGHT (opportunity cost)
player.Focus -= 15;  // Spent here, can't spend elsewhere
player.Time -= 2;    // Used time, closes time-dependent options
CreateGoal(nextGoal);
```

### Test 5: Are There Multiple Resource Types?

**Ask:** "Does system have varied resources with different behaviors?"

If NO → **WRONG** (oversimplified, no depth)
If YES → Correct (strategic variety)

Examples:

```csharp
// WRONG (single resource type)
public class Player {
    public int Experience { get; set; }  // Only one resource
}

// RIGHT (multiple resource types)
public class Player {
    public int Insight { get; set; }      // Personal stat
    public int Focus { get; set; }        // Session resource
    public int Health { get; set; }       // Permanent resource
    public Dictionary<string, int> StoryCubes { get; set; }  // Per-NPC
}
```

### Test 6: Do Choices Have Different Patterns?

**Ask:** "Are all choices structured the same (all gains, or all trades)?"

If YES → **WRONG** (monotonous, no variety)
If NO → Correct (heterogeneous patterns)

Examples:

```csharp
// WRONG (all choices are pure gains)
foreach (var action in actions) {
    player.Stats[action.Stat] += 1;  // Always +1, boring
}

// RIGHT (different patterns)
Action buildingAction = new Action {
    Pattern = ActionPattern.Building,
    Effects = new List<Effect> {
        new Effect { Resource = "Insight", Delta = +1 }
    }
};

Action tradingAction = new Action {
    Pattern = ActionPattern.Trading,
    Effects = new List<Effect> {
        new Effect { Resource = "Valor", Delta = +2 },
        new Effect { Resource = "Willpower", Delta = -5 },
        new Effect { Resource = "Reputation", Delta = -1 }
    }
};

Action crisisAction = new Action {
    Pattern = ActionPattern.Crisis,
    AllOptionsBad = true,
    Effects = new List<Effect> {
        new Effect { Resource = "Health", Delta = -10 }
    }
};
```

### Test 7: Can Player Max Everything?

**Ask:** "Is it possible to maximize all resources/stats?"

If YES → **WRONG** (no forced choice, no strategy)
If NO → Correct (specialization required)

Examples:

```csharp
// WRONG (can eventually max everything)
public class Player {
    // Unlimited progression, eventually 20 in all
    public int Insight { get; set; }
    public int Rapport { get; set; }
    public int Authority { get; set; }
    // Given enough time, player maxes all
}

// RIGHT (forced specialization)
public class Player {
    public int Insight { get; set; }
    public int Rapport { get; set; }
    public int Authority { get; set; }

    public int TotalXP { get; private set; }
    // Limited XP, must choose which stats to prioritize
    // Cannot max all - specialization forced
}
```

### Test 8: Do Thresholds Auto-Trigger?

**Ask:** "When resource crosses threshold, does system automatically respond?"

If NO → **WRONG** (player controls everything, no consequence)
If YES (where appropriate) → Correct (extremes have costs)

Examples:

```csharp
// WRONG (player chooses outcome)
if (player.Danger >= 10) {
    ShowPlayerChoice("Continue or retreat?");  // No, automatic
}

// RIGHT (automatic consequence)
if (player.Danger >= challenge.MaxDanger) {
    TriggerAutoFailure();  // Automatic, no choice
    ApplyInjury();
    EndChallenge(success: false);
}
```

---

## Conclusion: The Architectural Truth

### What Boolean Gates Create

**Cookie Clicker Progression:**
- Click → Gain resource → Unlock next tier
- Click more → Gain more → Unlock more
- No decisions, just time investment
- Checklist completion

**Emotional Result:**
"I'm grinding to unlock the next quest"

### What Resource Systems Create

**Strategic Management:**
- Choose priority → Spend resource → Close other options
- Accept trade-off → Sacrifice something → Gain something else
- Navigate crisis → Pick least bad option → Live with consequence
- Build identity → Specialize → Create vulnerability

**Emotional Result:**
"I'm sacrificing my relationship with Father to save Mother, and I'll live with that consequence forever"

### The Deep Difference

**Boolean Gates:**
- Content hiding
- Flag checking
- Linear unlocking
- Checklist completion
- Optimization possible
- Eventually do everything

**Resource Systems:**
- Content creation
- Arithmetic comparison
- Priority management
- Impossible choices
- No perfect build
- Must sacrifice something

### The Requirement Inversion

Traditional thinking: "What does player HAVE that lets them access content?"
Inverted thinking: "What can player AFFORD given their current reality?"

Traditional: Content exists behind gates, check flags to reveal
Inverted: Content created by rewards, resources determine affordability

Traditional: Boolean checks (has/hasn't, completed/not)
Inverted: Arithmetic comparison (resource vs cost)

Traditional: Linear progression (A unlocks B unlocks C)
Inverted: Resource competition (spend on A OR B OR C, can't do all)

### Final Test

**Before implementing any "requirement" system, ask:**

1. Am I checking a boolean? (If yes, wrong)
2. Am I hiding existing content? (If yes, wrong)
3. Is this "free" with no opportunity cost? (If yes, wrong)
4. Can player eventually do everything? (If yes, wrong)
5. Is there only one resource type? (If yes, wrong)
6. Are all choices the same pattern? (If yes, wrong)

**If ANY answer is "yes", you're creating boolean gates, not resource systems.**

Go back. Redesign. Invert.

Ask instead:
- What resources does this cost?
- What does completing this CREATE?
- What opportunity is CLOSED by spending here?
- What trade-offs force impossible choices?
- What specializations create identity and vulnerability?
- What patterns (building/trading/crisis) apply here?

This is how you create games that feel like **managing a real life with hard choices and lasting consequences**, not **completing a content checklist**.

This is the Requirement Inversion Principle.
