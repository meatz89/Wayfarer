# The Miller's Daughter - Complete Content Specifications

## Scenario Overview

### Narrative Summary

Elena, the town scribe, desperately needs legal documents delivered to Farmer Aldric's remote farmstead. The journey is treacherous - the direct path crosses dangerous Widow's Creek with no bridge, while the alternative route passes through an abandoned mill with a dark decade-old secret. Heavy rains arrive in three days, making the creek impassable for a week and the documents worthless.

The player must navigate physical obstacles while uncovering the truth about Martha the cook's daughter who disappeared at the mill ten years ago. The investigation reveals a smuggling operation, murder, and corruption reaching into the town's current leadership.

### Core Design Goals

1. **Showcase New Systems**: Demonstrate investigation phases, travel obstacles, knowledge gates, equipment requirements, state persistence
2. **Multiple Valid Paths**: At least 3 different successful approaches (aggressive/cautious/thorough)
3. **Learning Through Failure**: Initial attempts likely fail, teaching requirements
4. **Narrative Emergence**: Player choices affect multiple NPCs differently
5. **Reasonable Scope**: 30-45 minutes first playthrough, 20 minutes optimal

### Success Conditions

- Deliver Elena's documents before rain (72 segments)
- Navigate travel obstacles successfully
- Optionally: Complete mill investigation for additional rewards
- Optionally: Resolve Martha's daughter mystery for relationship bonus
- No soft-locks possible regardless of choices

---

## NPCs

### Elena - The Town Scribe

**Role**: Quest giver, legal authority, provides initial obligation

**Stats**:
- Location: Elena's Office (Morning-Afternoon), Home (Evening-Night)
- Personality: Devoted (Doubt accumulates +2 instead of +1)
- Conversation Level: 2 (standard difficulty)
- Initial Tokens: 0

**First Meeting (Authored Scene)**:
```
The scribe's office is neat despite the volume of documents. Elena looks up from her
work with tired eyes. "You're the courier?" She assesses you carefully. "I need
documents delivered to Farmer Aldric's stead. It's... not an easy journey. The direct
path crosses Widow's Creek - dangerous even in good weather. The mill path is longer
but has a crossing, though locals avoid the mill itself. Heavy rains are coming in
three days." She pauses. "I wouldn't ask if it wasn't important. Aldric is elderly,
these documents are time-sensitive legal matters. Will you help?"
```

**Conversation Decks**:
- Information Gathering: Learn about routes, urgency, Aldric's situation
- Desperate Request: Accept obligation, express concern about difficulty
- Friendly Chat: Build initial rapport, learn about Elena's work

**Request Details**:
```
Obligation: Deliver Legal Documents
├── Destination: Farmer Aldric at Outlying Farmstead
├── Item Weight: 1 (weatherproof document case)
├── Payment: 8 coins on successful delivery
├── Deadline: 72 segments (before rain)
├── Urgency Reason: Documents become invalid if delayed
└── Special: Elena mentions both routes, player chooses
```

**Knowledge Rewards**:
- Momentum 8: "creek_has_no_bridge", "mill_route_exists"
- Momentum 12: "rain_coming_soon", "documents_time_sensitive"
- Momentum 16: "aldric_cannot_travel", "legal_matter_important"

**Token Progression**:
- 1-2 Trust: +1 signature card (Elena's Precision)
- 3-5 Trust: +2 signature cards (+ Elena's Authority)
- 6-9 Trust: +3 signature cards (+ Elena's Legal Knowledge)

**Post-Investigation Reactions**:
- If smuggling ledgers found: "This is evidence of serious crimes. I'll need to act on this."
- If Elder implicated: "Thomas Elder? I can't believe... but the evidence is clear."
- If Martha's daughter truth revealed: "Murder to hide smuggling. Justice is long overdue."

### Martha - The Tavern Cook

**Role**: Information provider, emotional anchor, provides mill backstory

**Stats**:
- Location: Tavern (all day, busy Midday-Afternoon)
- Personality: Steadfast (All effects capped at ±2)
- Conversation Level: 1 (easy difficulty)
- Initial Tokens: 0

**Personality Traits**:
- Warm but haunted by daughter's loss
- Knows local area intimately (grew up on nearby farm)
- Reluctant to discuss daughter but can be convinced
- Grateful if player investigates mill

**Information Gathering Conversations**:

**About the Creek** (Always available):
"Widow's Creek? Dangerous thing. Current's strong even now. One slip and you're swept
away. After the rains? Impassable for a week at least. Never try it without proper
preparation."

**About the Mill** (Rapport 8+):
"The old grain mill... abandoned for years now. My family farmed near there before we
moved to town. The mill itself, locals avoid it. Superstitious, mostly. The wheel
mechanism was dangerous even when it was operational."

**About Her Daughter** (Rapport 12+, Emotional moment):
"You really want to know? *long pause* Anna. My daughter. Ten years ago last spring.
She was curious, always exploring, always asking questions. Went to investigate the
mill one day. Never came back. We searched for weeks. No body, no trace. Just... gone.
The constable said she must have fallen in the creek, got swept away. But Anna could
swim. She knew that creek."

**Critical Knowledge** (Rapport 16+):
"If you're really going through the mill... be careful. The mechanism is unstable.
The upper floor was already rotting ten years ago. And... *whispers* I've heard
someone's been there recently. Seen lights at night. Could be vagrants, could be...
I don't know. Just be careful. Take a light source - it's dark as pitch inside."

**Knowledge Rewards by Threshold**:
```
Momentum 8:
├── "creek_dangerous_current"
├── "mill_route_has_crossing"
└── "mill_abandoned_years"

Momentum 12:
├── "daughter_anna_disappeared"
├── "anna_was_curious"
├── "mill_mechanism_dangerous"
└── "searched_but_no_body"

Momentum 16:
├── "upper_floor_rotting"
├── "someone_there_recently"
├── "need_light_source"
└── "mechanism_unstable"
```

**Post-Investigation Reactions**:
- If daughter's token found: *breaks down crying* "This... this was Anna's. She was there. She found something."
- If truth revealed: "Murdered. My baby was murdered. For what? Smuggling profits?"
- If Elder confronted: "Thomas Elder killed my daughter? That bastard spoke at her memorial!"

### Farmer Aldric - The Document Recipient

**Role**: Delivery target, provides granddaughter connection

**Stats**:
- Location: Outlying Farmstead
- Personality: Proud (Cards must be ascending Initiative order)
- Conversation Level: 1
- Initial Tokens: 0
- Availability: Afternoon-Evening only (works fields Morning-Midday)

**Character Details**:
- Elderly but sharp-minded
- Cannot travel due to bad hip
- Knows about documents' importance
- Unaware of granddaughter's investigation

**Delivery Conversation**:
"Ah, Elena's courier! I was starting to worry. These old bones don't travel well anymore,
and these documents... well, they're about the farm's deed. Some dispute with the
boundaries. If not filed before the tax assessment, I could lose half my fields."

**Payment**: 8 coins as promised + 1 Diplomacy token

**Optional Information** (if asked about area):
"The mill? Dangerous place. My granddaughter Sophia is obsessed with it lately. Says
she's researching local history for some chronicle she's writing. Worries me. She
visits from town every few days, always asking about the old times."

**Knowledge Gained**:
- "sophia_investigating_mill"
- "sophia_visits_regularly"
- "aldric_worried_about_sophia"

### Sophia - The Cook's Granddaughter

**Role**: Catalyst for confrontation, innocent investigator

**Stats**:
- Location: Mill (if player takes long in investigation)
- Personality: Devoted (like Elena)
- Not fully developed NPC (appears in scene only)

**Appearance Trigger**:
If player spends 4+ segments in mill investigation, Sophia arrives with Elder

**Confrontation Dialogue**:
Sophia: "This is where it happened, I'm sure of it. My aunt Anna found something here."
Elder: "Now Sophia, these are just old stories. Let me help you understand what really happened."
*Elder's voice is tense - he knows what's hidden*

### Thomas Elder - The Hidden Antagonist

**Role**: Town authority hiding dark past

**Stats**:
- Location: Town Hall (normally), Mill (during confrontation)
- Personality: Proud
- Conversation Level: 3 (difficult)
- Initial Tokens: 3 Status (respected figure)

**Background** (revealed through investigation):
- Involved in smuggling operation 10 years ago
- Murdered Anna when she discovered operation
- Now holds position of authority in town
- Has been monitoring Sophia's investigation

**Confrontation Behavior**:
- If player hidden: Can be overheard trying to manipulate Sophia
- If player reveals self: Attempts to discredit evidence
- If evidence shown: Panics, might become violent
- If Sophia defended: Backs down when outnumbered

---

## Locations

### Town Square

**Role**: Starting hub location

**Spots**:
- Elena's Office (available Morning-Afternoon)
- Market (Thomas sells equipment)
- Town paths to Outskirts and Tavern

**Time Properties**:
- Morning: Quiet, NPCs in regular places
- Midday: Busy, full activity
- Evening: Winding down
- Night: Mostly deserted

### Tavern

**Role**: Information gathering, Martha's location

**Spots**:
- Main Room (Martha available all day)
- Kitchen (Martha preparing Morning)
- Back Room (private conversations)

**Services**:
- Food: 2 coins for -30 hunger
- Information: Martha's conversations
- Rumors: Background on mill and town

### Town Outskirts

**Role**: Route departure point

**Available Routes**:
- Creek Path (direct to farmstead)
- Mill Path (to old mill)
- Return to Town Square

**Description**:
"The edge of town where buildings give way to fields and forest. Two paths diverge
here - the direct route following the creek, and the longer mill path through the
woods."

### Old Mill (Investigation Location)

**Role**: Primary investigation site, mystery center

**External Description**:
"The abandoned mill looms ahead, its water wheel frozen mid-rotation, covered in moss
and decay. The overgrown path shows no recent use. The building itself shows structural
damage - gaps in the walls, sagging roofline. The creek flows stronger here, near the
wheel mechanism."

**Investigation Phases**: See Investigation Structure section

**Atmosphere Notes**:
- Daylight: Dim inside even at noon, structural decay visible
- Night: Pitch black inside, requires light source
- Weather: Rain makes structure more dangerous
- Sounds: Creaking wood, water flow, occasional falling debris

### Outlying Farmstead

**Role**: Delivery destination, Aldric's home

**Description**:
"A well-maintained farmstead despite its remote location. Fields stretch in neat rows,
the house sturdy and welcoming. Smoke rises from the chimney."

**Areas**:
- Farmhouse (Aldric present Afternoon-Evening)
- Fields (Aldric working Morning-Midday)
- Barn (shelter available if needed)

**Completion Rewards**:
- 8 coins payment
- 1 Diplomacy token
- Knowledge about Sophia
- Offer of shelter if evening

---

## Investigation Structure: The Mill Mystery

### Phase 1: Approach and Entry

**Description**:
"The mill's entrance presents immediate challenges. The main door is partially collapsed,
blocked by fallen beams and accumulated debris. The structure groans ominously."

#### Choice 1A: Force the Front Door
- **Requirements**: Tools OR Stamina 50+
- **Success Outcome**:
  - Enter mill (Phase 2)
  - -10 Health if forced without tools (debris injury)
  - -20 Stamina
  - 1 segment
- **Failure** (insufficient stamina):
  - Cannot enter this way
  - Learn: "need_tools_or_high_stamina"

#### Choice 1B: Search for Alternative Entry
- **Requirements**: None (always available)
- **Outcome**:
  - 1 segment searching
  - Discover: "mill_side_entrance" (loading door)
  - Side door wedged but openable
  - If have rope: Open easily
  - Without rope: Force open (-10 stamina)

#### Choice 1C: Observe Structure First
- **Requirements**: Insight 2+ OR "investigation_experience"
- **Outcome**:
  - 1 segment observation
  - Learn: "upper_floor_unstable", "mechanism_dangerous", "recent_footprints"
  - No entry yet but knowledge gained

#### Choice 1D: Circle Building
- **Requirements**: None
- **Outcome**:
  - 1 segment
  - Find river-side access (wet, slippery)
  - Discover wheel mechanism accessible from outside
  - New approach options

### Phase 2: Interior Exploration

**Description (with light)**:
"Your lantern reveals the mill's cavernous interior. Decades of abandonment show in
rotted grain bags, rusted tools, and structural decay. The massive grinding mechanism
dominates the space, its wheel shaft running through the wall to the waterwheel outside.
A ladder leads to an upper loft with questionable flooring. Fresh disturbances in the
dust suggest recent visitors."

**Description (without light)**:
"Darkness inside is absolute. You can hear water, feel the mechanism's bulk, smell
mold and decay, but see almost nothing. Moving forward is dangerous without light."

#### Choice 2A: Quick Passage to Exit
- **Requirements**: None
- **Outcome**:
  - 1 segment
  - Pass through safely
  - Reach far exit and crossing point
  - No investigation progress
  - Can deliver documents but miss mystery

#### Choice 2B: Examine the Mechanism
- **Requirements**: Lantern OR daylight through gaps
- **Success Outcome**:
  - 1 segment examination
  - -15 Stamina (climbing around machinery)
  - Discover: "hidden_compartment_mechanism"
  - Observation: Mechanism could be released
  - Progress to Phase 3 option
- **Without Light**:
  - Can attempt but -20 Health (injury in darkness)
  - 50% chance miss compartment

#### Choice 2C: Investigate Upper Loft
- **Requirements**: Rope OR Stamina 40+ OR Insight 2+
- **With Rope**:
  - Secure climb, no danger
  - Find recent habitation evidence
  - Discover: "sophia_journal_notes"
  - Learn about investigation
- **Without Rope**:
  - 40% chance floor collapse
  - If collapse: -20 Health, loud noise
  - If success: Same discoveries
- **With Insight 2+**:
  - Test floor first, identify safe path
  - No danger, same discoveries

#### Choice 2D: Search for Historical Clues
- **Requirements**: "anna_disappeared" knowledge from Martha
- **Outcome**:
  - 2 segments thorough search
  - Find torn fabric (decade old, matches Anna's description)
  - Find old struggle marks
  - Discover: "anna_was_here"
  - Emotional weight increases

### Phase 3: The Mechanism Puzzle

**Triggered by**: Discovering hidden compartment in Phase 2

**Description**:
"The gear housing has an access panel, corroded but visible. The mechanism hasn't moved
in years, but disturbing it wrong could trigger structural collapse. Inside the compartment,
you can glimpse old papers and something metallic."

#### Choice 3A: Force Panel Open
- **Requirements**: Strength (Authority 3+ OR Tools)
- **Outcome**:
  - 1 segment
  - 50% chance of partial collapse
  - If collapse: -30 Health, huge noise, alerts anyone nearby
  - If success: Access compartment
  - Discover: "smuggling_ledgers", "anna_token"

#### Choice 3B: Carefully Work Panel
- **Requirements**: Stamina 20+
- **Outcome**:
  - 2 segments careful work
  - -20 Stamina
  - 20% minor collapse risk (-10 Health)
  - Success: Access compartment safely
  - Discover: "smuggling_ledgers", "anna_token"

#### Choice 3C: Stabilize First
- **Requirements**: Rope AND Tools
- **Outcome**:
  - 3 segments preparation
  - -30 Stamina (heavy work)
  - No collapse risk
  - Permanent improvement: "mill_mechanism_safe"
  - Access compartment
  - Discover: "smuggling_ledgers", "anna_token", "elder_implicated"

#### Choice 3D: Study Mechanism
- **Requirements**: Insight 3+ OR "engineering_knowledge"
- **Outcome**:
  - 1 segment analysis
  - Learn safe approach
  - Reduce all danger percentages by 30%
  - Can retry other approaches with better odds

### Phase 4: Discoveries and Evidence

**Compartment Contents**:

**The Ledgers**:
"Old shipping manifests and accounting ledgers, water-damaged but readable. They document
a smuggling operation from ten years ago. Names listed include Thomas Elder (now town
elder), several merchants, and... a final entry: 'A.C. discovered operation. Silenced.
Body in creek.' The initials match Anna Cook."

**Anna's Token**:
"A small pewter locket with 'A.C.' engraved. Inside, a tiny portrait of Martha, younger
and smiling. This was definitely Anna's."

**Knowledge Gained**:
- "smuggling_operation_existed"
- "elder_was_smuggler"
- "anna_was_murdered"
- "body_disposed_creek"
- "evidence_of_crimes"

### Phase 5: Confrontation (Conditional)

**Trigger**: Spend 4+ segments in mill investigation

**Setup**:
Voices outside. Sophia (Martha's granddaughter) approaching with Thomas Elder.

**Dialogue Overheard**:
- Sophia: "Thank you for coming, Elder Thomas. I think my aunt discovered something here."
- Elder: "Dangerous place, child. Your aunt was foolish to come alone."
- Sophia: "But the records show the mill was still operating then..."
- Elder: "Let me show you what really happened." (menacing undertone)

#### Choice 5A: Reveal Yourself
- **Outcome**:
  - Immediate confrontation
  - Show evidence to Sophia
  - Elder panics: "Those were destroyed!"
  - 30% chance Elder attacks (Authority check to intimidate)
  - If fight: -20 Health but Elder flees
  - Sophia safe, truth revealed

#### Choice 5B: Hide and Listen
- **Requirements**: Cunning 2+ OR hidden position
- **Outcome**:
  - Overhear Elder's manipulation attempt
  - He's leading her toward mechanism (danger!)
  - Can intervene when critical
  - Gain: "elder_confession_overheard"
  - Better position for confrontation

#### Choice 5C: Exit Quietly
- **Requirements**: Know alternative exit
- **Outcome**:
  - Escape with evidence
  - Leave Sophia in danger
  - Can report to Elena immediately
  - Moral weight: Abandoned innocent

#### Choice 5D: Create Distraction
- **Requirements**: Cunning 3+ OR tools
- **Outcome**:
  - Make noise elsewhere in mill
  - Elder goes to investigate
  - Whisper warning to Sophia
  - She pretends ignorance, leaves safely
  - Can confront Elder alone after

---

## Travel Routes

### Route 1: Creek Path (Direct)

**Distance**: 4 segments total if all obstacles passed

#### Obstacle 1: Rough Trail
- **Description**: "The path to the creek is overgrown and rough."
- **Approach A**: Push through (Stamina 20+) → -20 stamina, 1 segment
- **Approach B**: Clear path (Tools) → -10 stamina, path improved, 1 segment
- **Approach C**: Find better route (Cunning 2+) → No cost, 2 segments

#### Obstacle 2: Widow's Creek Crossing
- **Description**: "The creek runs fast and deep. No bridge, no obvious crossing."
- **Approach A**: Wade across (Stamina 40+) → 60% failure chance
  - Failure: -30 Health, -20 Stamina, swept downstream, documents wet (ruined if not waterproof case)
  - Must retreat and try different route
- **Approach B**: Search for shallow spot → 2 segments searching
  - Need Insight 2+ to find
  - If found: 40% failure (better than 60%)
  - Learn: "creek_shallow_crossing"
- **Approach C**: Use rope → Requires rope
  - Secure crossing, no failure chance
  - -20 Stamina, 1 segment
- **Approach D**: Turn back → Retreat to try mill route
  - Learn: "creek_too_dangerous"

#### Obstacle 3: Final Approach
- **Description**: "Past the creek, the farmstead is visible ahead."
- **No obstacle**: 1 segment travel to farmstead

### Route 2: Mill Path (Safer but Longer)

**Distance**: 6 segments base + investigation time

#### Obstacle 1: Forest Path
- **Description**: "The old path through the forest is overgrown, barely visible."
- **Approach A**: Cut through (Tools OR Stamina 30+) → 2 segments, -20 stamina
- **Approach B**: Follow carefully → 3 segments, -10 stamina
- **Approach C**: Use local knowledge → If have "forest_paths" → 1 segment

#### Obstacle 2: Mill Passage
- **Description**: "The mill blocks the path. You must go through or around."
- **Approach A**: Through mill → Enter investigation (variable time)
- **Approach B**: Around mill → 2 segments, requires creek knowledge
- **Approach C**: Over mill → Requires rope + Stamina 30+, dangerous

#### Obstacle 3: Upper Crossing
- **Description**: "Past the mill, a log crosses the creek."
- **Approach A**: Cross carefully → 1 segment, -10 stamina
- **Approach B**: Use rope for safety → No stamina cost, very safe
- **Success**: Reach farmstead approach

---

## Equipment Catalog

### Essential Equipment

#### Rope (2 coins, weight 2)
**Enables**:
- Mill: Safe loft climb, mechanism stabilization, secure panels
- Creek: Safer crossing (reduces failure 60% → 20%)
- General: Climbing obstacles, securing things
**Available**: Market (Thomas)

#### Lantern with Oil (1 coin, weight 1)
**Enables**:
- Mill: See inside (required for investigation)
- Night travel: Safe movement in darkness
- Detail work: Read documents, examine mechanisms
**Available**: Market (Thomas)
**Note**: Oil consumable (2 hours use)

#### Basic Tools (2 coins, weight 2)
**Enables**:
- Mill: Force doors safely, work panels, stabilize structure
- Travel: Clear paths, remove obstacles
- General: Prying, cutting, simple repairs
**Available**: Market (Thomas)

### Optional Equipment

#### Warm Cloak (3 coins, weight 1)
**Enables**:
- Creek: Reduces cold damage if swept away
- Weather: Protection from rain
- Social: +1 starting momentum with nobles
**Available**: Market (Thomas)

#### Trail Rations (1 coin, weight 1)
**Effect**: -30 hunger when consumed
**Useful**: Long investigation, multiple attempts
**Available**: Market (Thomas) or Tavern (Martha)

#### Medicine (3 coins, weight 1)
**Effect**: +20 Health when consumed
**Useful**: Recovery from failures
**Available**: Market (Thomas)

---

## Timeline Management

### Day 1 Schedule

**Morning Block (Segments 1-4)**:
- Segment 1: Start game, orientation
- Segment 2: Talk to Elena, accept obligation
- Segment 3: Talk to Martha for information
- Segment 4: Shop for equipment OR begin travel

**Midday Block (Segments 5-8)**:
- If shopping: Complete purchases, begin travel
- If traveling: Reach first obstacle
- If aggressive: Attempt creek, likely fail

**Afternoon Block (Segments 9-12)**:
- If prepared: Reach mill or complete creek
- If investigating: Begin mill phases
- If failed: Return to town, recuperate

**Evening Block (Segments 13-16)**:
- Complete delivery if successful
- OR continue investigation
- OR rest and prepare for Day 2

### Day 2 Schedule

**If failed Day 1**:
- Morning: Recover, gather information
- Midday: Acquire equipment
- Afternoon: Retry with preparation
- Evening: Likely successful completion

**If investigating deeply**:
- Continue mill investigation
- Encounter confrontation event
- Resolve mystery
- Complete delivery afterward

### Day 3 Schedule

**Last chance before rain**:
- Must complete by Evening
- Rain begins Night of Day 3
- Creek becomes impassable Day 4+

### Time Pressure Points

**Optimal Fast Path**: 8-10 segments (Morning to Midday Day 1)
- Skip investigation
- Direct creek with rope
- Immediate delivery

**Balanced Path**: 16-20 segments (Full Day 1)
- Get information
- Buy equipment
- Mill route with light investigation
- Complete delivery

**Thorough Path**: 30-40 segments (Day 1-2)
- Full information gathering
- Complete investigation
- Confrontation resolution
- Delivery with full knowledge

---

## Conversation Integration

### Elena Conversations

**Information Gathering Structure**:
```
Foundation Cards → Build Initiative
Standard Cards → Generate momentum toward thresholds
├── 8 Momentum: Learn about routes
├── 12 Momentum: Understand urgency
└── 16 Momentum: Full context and advice
```

**Request Acceptance**:
- Play request card at 8+ momentum
- Creates obligation in journal
- Provides document item (weight 1)
- Elena expresses gratitude and concern

### Martha Conversations

**Building Trust for Information**:
```
Rapport-focused approach recommended:
├── Foundation: Show empathy and patience
├── Standard: Build connection through shared concerns
├── 8 Momentum: Creek warnings, basic mill info
├── 12 Momentum: Daughter story (emotional moment)
└── 16 Momentum: Critical warnings and advice
```

**Stat Value**:
- Rapport 2+: Easier momentum building
- Insight 2+: Can ask targeted questions
- Authority: Less effective (Martha responds to kindness)

### Equipment Purchase (Thomas)

**Merchant Exchange**:
- No conversation required (0 segments)
- See all available items
- Simple coin → item trades
- Can bargain with Diplomacy 3+ (10% discount)

---

## Critical Decision Points

### Decision 1: Route Choice
**Creek**: Fast but dangerous, requires preparation or luck
**Mill**: Safer but longer, investigation opportunity
**Impact**: Determines obstacles faced and mystery exposure

### Decision 2: Preparation Level
**Minimal**: Rush immediately, learn through failure
**Moderate**: Get key equipment (rope + lantern)
**Thorough**: Full equipment + information gathering
**Impact**: Success probability and resource cost

### Decision 3: Investigation Depth
**Skip**: Quick passage, miss mystery
**Light**: Examine basics, find some clues
**Deep**: All phases, find evidence, trigger confrontation
**Impact**: World state changes, NPC relationships

### Decision 4: Confrontation Approach
**Reveal**: Direct confrontation, protect Sophia
**Hide**: Gather more evidence, calculated risk
**Flee**: Preserve self, abandon Sophia
**Impact**: Justice outcome, reputation effects

### Decision 5: Truth Handling
**Report to Elena**: Legal justice, official response
**Tell Martha**: Emotional closure, personal justice
**Confront Elder**: Direct action, dangerous
**Stay Silent**: Avoid complications
**Impact**: Town power structure, relationships

---

## Failure Recovery Paths

### Failed Creek Crossing
1. Swept downstream (-30 Health)
2. Return to town injured
3. Rest to recover (Evening block)
4. Learn: Need rope or different route
5. Day 2: Try mill path instead

### Failed Mill Entry
1. Cannot force door without tools
2. Search finds alternative (side entrance)
3. OR return to town for tools
4. Minimal time loss, learning gained

### Failed Investigation
1. Injury from collapse/fall
2. Retreat with partial knowledge
3. Prepare better (equipment/healing)
4. Return to continue from saved progress
5. Previous discoveries remembered

### Missed Deadline
1. Rain makes creek impassable
2. Must use mill route only
3. Documents become invalid
4. Reduced payment (4 coins instead of 8)
5. Elena disappointed but understanding

---

## Rewards Structure

### Successful Delivery
- 8 coins from Elena
- 1 Diplomacy token with Aldric
- Reputation: "Reliable Courier"
- Opens future Elena requests

### Mill Investigation Complete
- Evidence items (ledgers, token)
- Knowledge about town corruption
- Observation cards for Elena/Martha
- Reputation: "Investigator"

### Mystery Resolved
- Martha: +5 Trust tokens (huge bonus)
- Elena: +3 Authority tokens
- Elder: Exposed and arrested
- Town: Major reputation gain
- Unlocks: Courthouse investigation

### Perfect Run Rewards
- All above rewards
- "Master Courier" reputation
- Efficient path discovered
- Maximum relationship gains
- 15-20 coins total value

---

## Testing Scenarios

### Scenario 1: Aggressive Failure Path
1. Accept obligation immediately
2. No preparation, rush to creek
3. Fail crossing (no rope/stamina)
4. Return injured
5. Rest and recover
6. Get equipment
7. Try mill route Day 2
8. Success with learning

**Tests**: Failure recovery, learning mechanics, state saves

### Scenario 2: Optimal Efficiency Path
1. Get minimal info from Elena
2. Buy only rope
3. Take creek path
4. Use rope to cross safely
5. Deliver immediately
6. Return for payment

**Time**: 8-10 segments
**Tests**: Speed run viability, minimal resource path

### Scenario 3: Complete Investigation Path
1. Full info from Elena and Martha
2. Buy all equipment
3. Take mill route
4. Complete all investigation phases
5. Trigger confrontation
6. Resolve optimally
7. Deliver documents
8. Return with evidence

**Tests**: Full content, all systems integration

### Scenario 4: Partial Success Path
1. Some preparation
2. Mill route without light
3. Partial investigation (dangerous)
4. Some discoveries
5. Retreat when injured
6. Complete delivery
7. Return later for full investigation

**Tests**: State persistence, partial progress

---

## Balance Targets

### Economic Balance
- Starting coins: 15
- Essential equipment: 3-5 coins
- Full equipment: 8-10 coins
- Work option: 5 coins per block
- Total rewards: 15-20 coins

**Result**: Can afford basics, full prep requires work

### Time Balance
- Available: 72 segments (3 days)
- Minimum needed: 8 segments
- Comfortable: 20-30 segments
- Full investigation: 40-50 segments
- Buffer for failures: 20+ segments

**Result**: Generous time but pressure exists

### Health/Stamina Balance
- Starting: 100/100
- Creek failure: -30 health
- Mill dangers: -10 to -30 health
- Investigation costs: -20 to -40 stamina
- Recovery: Rest block or consumables

**Result**: Can survive 2-3 failures

### Knowledge Gates
- Total knowledge items: 20-25
- Essential for success: 3-5
- Helpful but optional: 10-15
- Deep lore: 5-10

**Result**: Layers of discovery

---

## Implementation Priority

### Must Have (Core Experience)
1. Elena NPC with obligation
2. Martha NPC with information
3. Creek obstacle (minimum 2 approaches)
4. Mill investigation (3 phases minimum)
5. Basic equipment (rope, lantern)
6. Delivery completion
7. State saves

### Should Have (Full Experience)
1. All 5 investigation phases
2. Confrontation event
3. All travel approaches
4. Full equipment catalog
5. Complete NPC reactions
6. Evidence discovery
7. Multiple outcomes

### Nice to Have (Polish)
1. Weather effects
2. Time-of-day variations
3. Authored scene variations
4. Achievement tracking
5. Speedrun mode
6. New Game+ bonuses
7. Hidden secrets

---

## Success Metrics

### Completion Metrics
- 80% players complete delivery (any method)
- 60% attempt mill investigation
- 40% fully complete investigation
- 90% learn from at least one failure
- 0% get permanently stuck

### Engagement Metrics
- Average first playthrough: 30-45 minutes
- Average segments used: 25-35 of 72
- Retry rate after failure: 70%
- Multiple path usage: 50%

### Discovery Metrics
- Average knowledge gained: 10-15 items
- Equipment purchased: 2-3 items average
- Investigation phases completed: 3 of 5
- NPCs conversed with: 2-3

---

## Conclusion

The Miller's Daughter scenario showcases all new V2 systems while telling a compelling mystery story. Through layered investigation phases, meaningful travel obstacles, knowledge-gated progression, and state persistence, players experience the new Wayfarer philosophy: preparation matters, failure teaches, discovery rewards, and every choice has consequences. The scenario supports multiple playstyles (aggressive/balanced/thorough) and ensures no soft-locks while creating genuine challenge and narrative emergence through mechanical integration.