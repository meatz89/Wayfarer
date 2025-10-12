# POC Example: Reach the Miller's Cottage

## Investigation Overview

**Investigation:** "Deliver Message to Miller"
**Goal:** Reach the miller's cottage in the forest to deliver an urgent message
**Phases:** 3 (Town Gate → Forest Path → Miller's Property)

---

## Phase 1: Town Gate

### Context
The town guard won't let strangers leave through the north gate without authorization. You need to get past.

### Obstacle: "Suspicious Gatekeeper"
```
Location: Town North Gate
PhysicalDanger: 2 (guard will resist forcefully)
SocialDifficulty: 1 (can be negotiated with)
MentalComplexity: 0 (no puzzle here)
IsPermanent: true (guard always there)
```

### Available Approaches

**"Pay Gate Fee"** (Social approach)
- Requirements: SocialDifficulty ≤ 1 (available immediately)
- Challenge Type: Social conversation with guard
- Consequence: Remove (for this trip)
- During challenge: Cards cost 2-5 coins (player decides how much to spend)
- Resolution: Guard lets you through

**"Sneak Past"** (Physical approach)  
- Requirements: PhysicalDanger ≤ 2 (available immediately)
- Challenge Type: Physical/Finesse
- Consequence: Bypass
- Risk: 15-20 Health if caught during challenge
- Resolution: Find gap in patrol, slip through unnoticed

### Preparation Options

**"Ask About Miller"** (Informational - Social conversation with townsfolk)
- Grants: Knowledge card for guard conversation
- Card: "Mention Miller's Errand"
  - Text: "The townsfolk speak well of the miller. You're on legitimate business."
  - Effect: +3 Momentum, -1 Doubt
  - Bound to: Diplomacy, Depth 2
- Does NOT modify obstacle goals
- Gives you better talking points during negotiation

**"Get Official Pass"** (Structural - Social challenge at Town Hall)
- Removes: "Pay Gate Fee" goal
- Adds: "Show Official Pass" goal
  - Requirements: SocialDifficulty ≤0 OR completed "Get Official Pass"
  - Challenge: Simple 6 Momentum threshold
  - Consequence: Remove (free passage)
- Changes situation: You now have recognized authority

### Player Decision Point

**Fast but costly:** Pay gate fee now (5-10 coins in challenge)
**Risky:** Sneak past (save coins, risk Health)
**Informational prep:** Ask about miller, get card advantage in negotiation
**Structural prep:** Get official pass, replace expensive goal with free one

---

## Phase 2: Forest Path

### Context
The forest path to the miller's cottage is blocked by a fallen tree. Recent storms have made this area dangerous.

### Obstacle: "Fallen Oak"
```
Location: Forest Path
PhysicalDanger: 2 (heavy tree, risk of injury moving it)
SocialDifficulty: 0 (no one to talk to)
MentalComplexity: 2 (could find way around)
IsPermanent: false (removed once cleared)
Requirements: Phase 1 resolved (got past gate)
```

### Available Approaches

**"Climb Over"** (Physical approach)
- Requirements: PhysicalDanger ≤ 2 (available immediately)
- Challenge Type: Physical/Athletics
- Consequence: Remove
- Risk: 20-30 Stamina cost during challenge
- Resolution: Haul yourself over the massive trunk

**"Find Trail Around"** (Mental approach)
- Requirements: MentalComplexity ≤ 2 (available immediately)
- Challenge Type: Mental investigation
- Consequence: Bypass
- Cost: 10-15 Focus, 1 time segment
- Resolution: Discover old hunter's path through undergrowth

### Preparation Options

**"Study Tree"** (Informational - Mental investigation)
- Grants: Knowledge card for climbing challenge
- Card: "Use Weak Branches"
  - Text: "You identified branches that will bear your weight safely"
  - Effect: +3 Breakthrough, -2 Exertion cost
  - Bound to: Insight, Depth 3
- Does NOT modify obstacle goals
- Gives you tactical advantage if you climb

**"Find Woodcutter Tools"** (Structural - Mental investigation at nearby woodcutter's camp)
- Removes: "Climb Over" goal
- Adds: "Clear Path" goal
  - Requirements: PhysicalDanger ≤1 OR completed "Find Woodcutter Tools"
  - Challenge: Physical, 8 Breakthrough threshold
  - Consequence: Remove (clear branches, easy passage)
- Changes situation: You now have tools to clear obstacles

### Player Decision Point

**Quick physical:** Climb now (costs Stamina)
**Patient mental:** Find trail around (costs Focus and time)
**Informational prep:** Study tree, get card advantage for climbing
**Structural prep:** Find tools, replace difficult climb with easier clearing

---

## Phase 3: Miller's Property

### Context
The miller's cottage is surrounded by aggressive dogs. The miller is reclusive and doesn't welcome visitors.

### Obstacle: "Miller's Guard Dogs"
```
Location: Miller's Cottage Approach
PhysicalDanger: 3 (dogs will attack)
SocialDifficulty: 2 (miller might call them off)
MentalComplexity: 2 (could find safe approach)
IsPermanent: true (dogs always guard property)
Requirements: Phase 2 resolved (crossed forest)
```

### Available Approaches

**"Call Out to Miller"** (Social approach)
- Requirements: SocialDifficulty ≤ 2 (available immediately)
- Challenge Type: Social conversation (shouting distance)
- Consequence: Transform
- Effect: Miller calls off dogs, you enter as guest
- Result: Dogs remain but friendly, establishes relationship with miller

**"Distract Dogs"** (Mental approach)
- Requirements: MentalComplexity ≤ 2 (available immediately)
- Challenge Type: Mental/Cunning
- Consequence: Bypass
- Cost: 15 Focus during challenge
- Resolution: Lure dogs to far side of property, slip to door

### Locked Approaches

**"Show Official Credentials"** (Social approach)
- Requirements: SocialDifficulty ≤ 1 AND completed "Get Town Seal"
- Challenge Type: Simple Social, 6 Momentum threshold
- Consequence: Transform (better)
- Effect: Miller sees official business, welcomes you warmly
- Result: Dogs obey you, full property access, positive relationship

### Preparation Options

**"Observe Property"** (Informational - Mental investigation from tree line)
- Grants: Knowledge card for dog distraction
- Card: "Exploit Patrol Pattern"
  - Text: "You know when the dogs patrol the far fence"
  - Effect: +4 Progress, generate 2 Leads
  - Bound to: Cunning, Depth 3
- Does NOT modify obstacle goals
- Gives tactical advantage if you try distraction

**"Get Town Seal"** (Structural - Social task back in town, requires backtracking)
- Removes: "Call Out to Miller" goal
- Adds: "Show Official Credentials" goal (easier, better outcome)
- Changes situation: You have recognized official authority
- Cost: 2 time segments to return to town

**"Buy Meat from Butcher"** (Structural - visit butcher in town, requires backtracking)
- Removes: (none, adds new option)
- Adds: "Offer Meat to Dogs" goal
  - Requirements: PhysicalDanger ≤2 OR completed "Buy Meat from Butcher"
  - Challenge: Physical, 8 Breakthrough threshold
  - Consequence: Transform (dogs friendly)
- Changes situation: You have actual leverage with dogs
- Cost: 5 coins + 1 time segment

### Player Decision Point

**Direct social:** Call to miller now (establishes basic access)
**Clever mental:** Distract dogs (sneak past, no relationship)
**Prepared official:** Get town seal first (best outcome, but time cost)
**Resource-based:** Use meat to befriend dogs (if you prepared)

---

## Complete Player Journeys

### Journey A: Fast & Simple (Resource-Heavy)

**Phase 1:** Pay gate fee (5 coins, quick)
**Phase 2:** Climb over tree (25 Stamina, quick)  
**Phase 3:** Call to miller (establish basic relationship)

**Total Cost:** 5 coins, 25 Stamina, ~3 time segments
**Result:** Message delivered, basic miller relationship, straightforward
**Preparation style:** None, accepted immediate costs

### Journey B: Structural Preparation (Authority-Based)

**Phase 1:** Get official pass (unlock free passage, 1 segment)
**Phase 2:** Find tools (unlock easier climb, 2 segments)
**Phase 3:** Get town seal (unlock best social outcome, 2 segments)

**Total Cost:** ~5 time segments, minimal other resources
**Result:** Message delivered, strong miller relationship, official recognition
**Preparation style:** Changed objective situation through authority/equipment

### Journey C: Informational Preparation (Skill-Based)

**Phase 1:** Ask about miller (get card), then negotiate with card advantage
**Phase 2:** Study tree (get card), then climb with card advantage
**Phase 3:** Observe property (get card), then distract dogs with card advantage

**Total Cost:** ~3 segments prep, moderate challenge difficulty with card help
**Result:** Message delivered, tactical advantages used, moderate resources spent
**Preparation style:** Gathered information, used it skillfully in challenges

### Journey D: Mixed Approach (Adaptive)

**Phase 1:** Ask about miller (informational), free passage via card
**Phase 2:** Find tools (structural), easy climb unlocked
**Phase 3:** Observe property (informational), distract dogs with card help

**Total Cost:** 3-4 segments prep, mixed resources
**Result:** Message delivered, no deep relationships, efficient resource use
**Preparation style:** Used both information gathering and situation changing

---

## What This Demonstrates

**Two Preparation Types:**
- Informational: Grants cards for tactical advantage (doesn't change properties)
- Structural: Reduces properties, unlocks approaches (changes situation)
- Player chooses depth and type of preparation

**Knowledge Cards:**
- Appear in challenges when relevant
- Provide context to AI for narrative generation
- Player draws and plays them (gameplay, not passive bonus)
- Card text references preparation (verisimilitude)

**Gating Satisfaction:**
- Properties gate approaches (unlocking feels good)
- Structural preparation reduces properties (opens new options)
- Informational preparation doesn't unlock, but helps with available options

---

## What This Demonstrates

**Investigation Structure:**
- One investigation with clear goal (deliver message)
- Three phases, each is an obstacle at a location
- Sequential progression (must resolve each to continue)

**Obstacle Variety:**
- Phase 1: Low difficulty, teaches system (Fork template)
- Phase 2: Medium difficulty, preparation choice (Gauntlet template)
- Phase 3: High difficulty, multiple paths (Escalation template)

**Player Expression:**
- Fast vs thorough vs clever approaches
- Resource type choices (coins vs Stamina vs Focus vs time)
- Relationship outcomes (violent vs diplomatic vs sneaky)
- Preparation depth (minimal vs heavy)

**Mechanical Integration:**
- Each approach is full tactical challenge (Social/Mental/Physical)
- Properties gate which approaches visible
- Preparation reduces properties, unlocking options
- Consequences create different world states
- No soft-locks (always at least one desperate option)

**Verisimilitude:**
- Guard can be paid, sneaked past, or made friendly through reputation
- Tree can be climbed, bypassed, or made easier with tools
- Dogs can be negotiated with owner, distracted, or befriended
- Each obstacle feels like real barrier with real solutions

**Tutorial Value:**
- Phase 1 introduces approach selection (easy choice)
- Phase 2 introduces preparation value (optional optimization)
- Phase 3 introduces locked approaches and backtracking trade-offs
- Escalating complexity matches player learning
