# Wayfarer POC: The Miller's Daughter

## Scenario Overview

### The Story

Elena, a town scribe, needs urgent legal documents delivered to an outlying farmstead. The route is difficult - crossing either a dangerous creek or passing through an abandoned mill with dark history. Heavy rains come in three days, making the creek impassable for a week.

This POC demonstrates all three challenge systems (Mental, Physical, Social) through both standalone challenges and a multi-phase investigation. Success requires information gathering, preparation, equipment acquisition, and careful decision-making.

### Core Systems Demonstrated

**Three Challenge Types**:
- **Mental**: Investigation-based observation (pauseable, uses Progress/Attention/Exposure/Leads)
- **Physical**: Obstacle-based action (one-shot, uses Breakthrough/Exertion/Danger/Aggression)
- **Social**: Conversation-based interaction (session-bounded, uses Momentum/Initiative/Doubt/Cadence)

**Integration**:
- Obligations drive exploration
- Conversations provide information for challenges
- Investigation requires all three challenge types in sequence
- Resources enable preparation and risk management

### Success Criteria

- Successfully deliver Elena's documents before weather makes route impassable (3-day window)
- Complete at least one standalone challenge per system type
- Navigate The Mill Mystery investigation (all 3 phases)
- Manage resources (Health, Stamina, Focus, coins, time, equipment)
- Build relationships through completed obligation and shared discoveries

## Starting Conditions

### Player State

**Time**: Morning, Day 1, Segment 1 (23 segments remaining before deadline)
**Location**: Town Square
**Resources**:
- Health: 100/100 (Physical permanent resource)
- Stamina: 100/100 (Physical permanent resource)
- Focus: 100/100 (Mental permanent resource)
- Hunger: 20/100
- Coins: 15
- Weight: 2/10 (basic supplies)
- Understanding: 0 (universal tier unlock)

**Equipment**:
- Basic travel pack (weight 1)
- Waterskin (weight 1)
- No specialized gear

**Stats** (All Level 1, unified across all challenge types):
- Insight: 1 (0/10 XP) - Pattern recognition, analysis, structural understanding
- Rapport: 1 (0/10 XP) - Empathy, connection, emotional intelligence
- Authority: 1 (0/10 XP) - Command, decisiveness, power
- Diplomacy: 1 (0/10 XP) - Balance, patience, measured approach
- Cunning: 1 (0/10 XP) - Subtlety, strategy, risk management

### Known Venues and Locations

**Town Square Venue**:
- Town Square (location): Central hub, memorial stone
- Market (location): Equipment vendors
- Elena's Office (location): Scribe's workplace

**Tavern Venue**:
- Common Room (location): Martha works here

**Mill Venue**:
- Mill Approach (location): Overgrown path to mill
- Mill Exterior (location): Outside structure, waterwheel visible
- Mill Interior (location): Dangerous abandoned building

**Farmstead Venue** (destination):
- Aldric's Home (location): Delivery destination

### Active Obligation

**None yet** - Elena's request will be first obligation when accepted.

## Challenge Decks

### Mental Challenge Decks

**mental_observation** (Town Memorial, Mill Investigation):
- Initial Hand Size: 5
- Max Hand Size: 7
- Card IDs: Foundation depth 1-2 (no cost), Standard depth 3-4 (2-3 Attention), Advanced depth 5-6 (4-5 Attention)

Example cards from unified stat system:
- "Notice Details" (depth 2, Insight-bound, ACT): +1 Progress, +1 Lead, costs 0 Attention (Foundation)
- "Careful Examination" (depth 4, Insight-bound, ACT): +2 Progress, +2 Leads, costs 3 Attention
- "Empathetic Reading" (depth 4, Rapport-bound, ACT): +2 Progress, +2 Leads, +1 to next Social, costs 3 Attention
- "Cover Tracks" (depth 4, Cunning-bound, ACT): +2 Progress, +2 Leads, -1 Exposure, costs 3 Attention
- "Patient Observation" (depth 4, Diplomacy-bound, ACT): +2 Progress, +2 Leads, -1 Exposure, costs 2 Attention
- "Draw Conclusions" (depth 2, no binding, OBSERVE): Draw Details equal to Leads, reset Leads to 0, costs 0 Attention (Foundation)

### Physical Challenge Decks

**physical_athletics** (Blocked Path, Mill Navigation):
- Initial Hand Size: 5
- Max Hand Size: 7
- Card IDs: Foundation depth 1-2 (no cost), Standard depth 3-4 (2-3 Exertion), Advanced depth 5-6 (4-5 Exertion)

Example cards from unified stat system:
- "Steady Yourself" (depth 2, no binding, EXECUTE): +1 Breakthrough, +0 Aggression, costs 0 Exertion (Foundation)
- "Power Move" (depth 6, Authority-bound, EXECUTE): +4 Breakthrough when combo triggers, +1 Aggression, +1 Danger, costs 4 Exertion
- "Calculated Risk" (depth 5, Cunning-bound, EXECUTE): +3 Breakthrough, -1 Danger per locked card in combo, +1 Aggression, costs 3 Exertion
- "Structural Analysis" (depth 4, Insight-bound, EXECUTE): +2 Breakthrough, reduces Danger of next card in combo, +1 Aggression, costs 2 Exertion
- "Flow State" (depth 6, Rapport-bound, EXECUTE): +3 Breakthrough, costs 1 less Exertion if 2+ cards in combo, +1 Aggression, costs 3 Exertion base
- "Assess Situation" (depth 2, no binding, ASSESS): Trigger all locked Options as combo, -2 Aggression, exhaust all Options back to Situation, draw fresh Options, costs 0 Exertion (Foundation)

### Social Challenge Decks

**social_request** (Elena conversation):
- Initial Hand Size: 5
- Max Hand Size: 7
- Card IDs: Foundation depth 1-2 (no cost), Standard depth 3-4 (2-3 Initiative), Advanced depth 5-6 (4-5 Initiative)

Example cards from unified stat system:
- "Polite Greeting" (depth 2, no binding, Statement, SPEAK): +1 Initiative, +1 Cadence, costs 0 Initiative (Foundation)
- "Express Empathy" (depth 4, Rapport-bound, Statement, SPEAK): +2 Momentum, +1 Cadence, costs 3 Initiative
- "Assert Competence" (depth 4, Authority-bound, Statement, SPEAK): +2 Momentum, +1 Cadence, +1 Doubt, costs 3 Initiative
- "Ask Questions" (depth 4, Insight-bound, Statement, SPEAK): +1 Momentum, +1 Cadence, +1 Understanding, costs 2 Initiative
- "Suggest Compromise" (depth 4, Diplomacy-bound, Statement, SPEAK): +2 Momentum, +1 Cadence, -1 Cadence next LISTEN, costs 3 Initiative
- "Pause to Listen" (depth 2, no binding, LISTEN): Draw Topics to Mind (base 3 + negative Cadence bonus), -2 Cadence, +Doubt per positive Cadence, costs 0 Initiative (Foundation)

**social_information** (Martha conversation):
- Same structure, different narrative context

**social_confrontation** (Investigation Phase 3):
- Same structure, higher tension narrative

## NPCs

### Elena - The Town Scribe

**Location**: Elena's Office at Town Square Venue (Morning through Afternoon)
**Personality**: Devoted (Doubt accumulates +2 instead of +1 - requires efficiency)
**Relationship**: Stranger (first meeting)
**Connection Level**: Stranger (0 interactions)

**Introduction** (Authored Scene):
"The scribe's office is neat despite the volume of documents. Elena looks up from her work with tired eyes. 'You're the courier?' She assesses you carefully. 'I need documents delivered to Farmer Aldric's stead. It's... not an easy journey. The direct path crosses Widow's Creek - dangerous even in good weather. The mill path is longer but has a crossing, though locals avoid the mill itself. Heavy rains are coming in three days.' She pauses. 'I wouldn't ask if it wasn't important. Aldric is elderly, these documents are time-sensitive legal matters. Will you help?'"

**Active Goals**:
1. **"Request Delivery"** (standalone Social challenge)

**Request Details** (granted on goal completion):
- Destination: Farmer Aldric's Stead
- Weight: 1 (legal documents in weatherproof case)
- Payment: 8 coins on successful delivery
- Deadline: Before heavy rains (Day 4, Morning)

### Martha - The Tavern Cook

**Location**: Common Room at Tavern Venue (all day)
**Personality**: Steadfast (All effects capped at ±2 - rewards patient grinding)
**Relationship**: Acquaintance (local familiarity)
**Connection Level**: Stranger (but friendly, from local history)

**First Conversation**:
Friendly local who knows the countryside. Information Gathering conversation available immediately.

**Information Provided** (through Social challenge):
- About the creek: "Widow's Creek is dangerous. Current's strong even now. One slip and you're swept away. After rains? Impassable for a week."
- About the mill: "Old grain mill, abandoned for years. My family farmed nearby before moving to town. The mill... locals avoid it. Superstitious mostly."
- About her daughter (requires 6+ Momentum): "My daughter disappeared near there a decade ago. Anna. She was curious, always exploring. Went to investigate the mill one day, never came back. No body found. The mill's wheel mechanism was dangerous even when operational."
- Route advice: "The mill path is longer but has safe crossing upstream of the wheel. You'd have to go through the mill building though - the path continues on the far side. Bring light, it'll be dark inside."

**Active Goals**:
1. **"Gather Information"** (standalone Social challenge)
2. **"Confront About Findings"** (Investigation Phase 3 - spawns when Phase 2 complete)

**Mechanical Benefits**:
- Route knowledge (mill path exists, safe crossing location)
- Investigation context (daughter's disappearance, mechanism danger)
- Equipment advice (need light source)
- Historical background (unlocks Investigation Phase 1)

### Thomas - The Equipment Vendor

**Location**: Market at Town Square Venue (Morning through Afternoon)
**Personality**: Mercantile (Highest Initiative card gets bonus effect)
**Relationship**: None (business transaction only)

**Available Items** (Exchange, not Social challenge):
- Rope (2 coins, weight 2): "Good hemp rope. Useful for climbing or securing unstable things."
- Lantern + Oil (2 coins, weight 1): "Two hours of light. Careful with the flame."
- Warm Cloak (3 coins, weight 1): "Weather's turning. Stay dry, stay alive."
- Basic Tools (2 coins, weight 2): "Prying bar, small hammer. Never know when you need to force something."
- Rations (1 coin, weight 1): "Trail food. Keeps you going."

## Standalone Challenges (One Per System)

### Mental Challenge: "Examine Town Memorial"

**Location**: Town Square (standalone challenge, not investigation)
**System Type**: Mental
**Deck**: mental_observation
**Session Model**: Pauseable (can leave and return, state persists)

**Goal**: "Examine Town Memorial"
- Description: "The memorial stone has stood in the square for decades. Something about the inscriptions catches your eye."
- System Type: Mental
- Deck ID: mental_observation
- Location ID: town_square

**Goal Cards** (tiered victory conditions):
1. **Surface Reading** (8 Progress threshold):
   - Rewards: +1 Understanding, knowledge "memorial_history"
   - "You've deciphered the basic inscriptions - names and dates of town founders."

2. **Deep Analysis** (14 Progress threshold):
   - Rewards: +2 Understanding, knowledge "memorial_secrets", +1 Insight XP
   - "Hidden within the formal inscriptions, you notice a pattern - a coded message about the mill."

**Core Resources**:
- **Progress** (builder, persists): 0/14 (accumulates across visits toward victory)
- **Attention** (session budget, resets): Starts at 30 (derived from Focus 100 at challenge start, costs 10 Focus to attempt)
- **Exposure** (persistent penalty): Starts at 0 (no penalty initially, persists when leaving)
- **Leads** (observation flow, persists): Starts at 0 (generated by ACT, spent by OBSERVE)
- **Understanding** (global persistent): Universal tier unlock

**Card Flow Mechanics**:
- **ACT** on Methods in hand: Spend Attention, generate Leads (depth 1-2 = +1 Lead, depth 3-4 = +2 Leads), build Progress, increase Exposure, move completed methods to Applied pile. **Does NOT draw cards.**
- **OBSERVE** Details: Draw Details equal to Leads count (zero Leads = zero draw), costs 0 Attention. **Only action that draws cards in Mental challenges.**
- Methods in hand persist between OBSERVE actions because investigation knowledge doesn't vanish
- Leads persist when you leave because investigative threads remain open
- Only completing investigation resets Leads to zero

**Example Card Play**:
1. Start: 5 Methods in hand (from initial draw), 0 Leads, 30 Attention, 0 Progress
2. Play "Notice Details" (ACT, depth 2, Insight-bound): -0 Attention (Foundation), +1 Progress, +1 Lead, hand now 4 Methods
3. Play "Careful Examination" (ACT, depth 4, Insight-bound): -3 Attention, +2 Progress, +2 Leads (now 3 total), hand now 3 Methods
4. Play "Draw Conclusions" (OBSERVE, Foundation): Draw 3 Details to Methods pile, reset Leads to 0, hand now 6 Methods
5. Continue cycle: ACT generates Leads → OBSERVE draws equal to Leads

**Location Properties**: None (standard investigation, no tactical modifiers)

**Victory**: Reach Progress threshold (8 or 14) in one or more visits. Can pause anytime, state persists.

**Time Cost**: 1 segment per session, costs 10 Focus to attempt session

---

### Physical Challenge: "Clear Blocked Path"

**Location**: Mill Approach (standalone challenge, not investigation)
**System Type**: Physical
**Deck**: physical_athletics
**Session Model**: One-shot (must complete or fail in single attempt)

**Goal**: "Clear Blocked Path"
- Description: "Fallen trees and overgrown brush block the path to the mill. You'll need to force your way through."
- System Type: Physical
- Deck ID: physical_athletics
- Location ID: mill_approach

**Goal Cards** (tiered victory conditions):
1. **Force Through** (8 Breakthrough threshold):
   - Rewards: Path cleared, can access Mill Exterior
   - "You've forced a passage through the debris. The path is clear enough to continue."

2. **Thorough Clearance** (12 Breakthrough threshold):
   - Rewards: Path fully cleared, +1 Authority XP, future passage requires 0 time
   - "You've completely cleared the path. Passage here is now safe and quick."

**Core Resources**:
- **Breakthrough** (builder): 0/12 (toward victory in single session)
- **Exertion** (session budget): Starts at 40 (derived from Stamina 100 at challenge start, costs 20 Stamina to attempt)
- **Danger** (threshold): Starts at 0, maximum 10 before injury (reaching max = 15 Health loss, challenge fails)
- **Aggression** (balance): Starts at 0, range -10 to +10
  - High Aggression (reckless, +3 or above): +1 Danger per action, injury risk increases
  - Low Aggression (overcautious, -3 or below): -1 Breakthrough per action, wasting opportunities
  - Sweet spot: -2 to +2 range (balanced, controlled)
- **Understanding** (global persistent): Universal tier unlock

**Card Flow Mechanics**:
- **EXECUTE** Option from hand: Lock Option as preparation (displayed above hand with "LOCKED" badge), spend Exertion using projection (cost includes modifiers), increase Aggression (+1 base + card approach modifier from projection). Can EXECUTE multiple Options, building prepared sequence and aggressive momentum. **Uses projection to determine affordability.**
- **ASSESS** Situation (Foundation): Trigger all locked Options as combo (effects resolve together using full projection per card including Breakthrough, Danger, Aggression modifiers), decrease Aggression (-2 base + each card's approach modifier from projection), then exhaust all Options back to Situation and draw fresh Options. **Applies full projection per locked card.**
- Unplayed Options return to Situation because they were considerations for a context that no longer exists
- All Options exhaust back to Situation after combo execution - challenge resets with new possibilities

**Example Card Play**:
1. Start: 5 Options in hand, 0 locked, 40 Exertion, 0 Breakthrough, 0 Aggression, 0 Danger
2. Play "Steady Yourself" (EXECUTE, depth 2, Foundation): Lock card, -0 Exertion, +0 Aggression projection, 1 card locked
3. Play "Structural Analysis" (EXECUTE, depth 4, Insight-bound): Lock card, -2 Exertion, +1 Aggression projection, 2 cards locked
4. Play "Power Move" (EXECUTE, depth 6, Authority-bound): Lock card, -4 Exertion, +1 Aggression projection, 3 cards locked
5. Play "Assess Situation" (ASSESS, Foundation): Trigger combo:
   - "Steady Yourself": +1 Breakthrough, +0 Aggression
   - "Structural Analysis": +2 Breakthrough, reduces Danger of next card
   - "Power Move": +4 Breakthrough, +1 Danger (reduced by previous card), +1 Aggression
   - Total: +7 Breakthrough, +1 Danger, combo Aggression modifiers applied, -2 base Aggression = net -1 Aggression
   - Exhaust all 3 locked cards + remaining 2 hand cards back to Situation (5 total)
   - Draw 5 fresh Options from Situation
6. Continue: EXECUTE to build combo → ASSESS to execute and reset

**Challenge Type**: Athletics (climbing/running/jumping, Insight/Rapport/Cunning-focused, first card in combo reduces risk for subsequent cards)

**Victory**: Reach Breakthrough threshold (8 or 12) in single attempt. Reaching Danger maximum (10) causes injury (15 Health loss) and failure.

**Time Cost**: 1 segment, costs 20 Stamina to attempt

---

### Social Challenge: "Request Delivery"

**NPC**: Elena at Town Square Venue
**System Type**: Social
**Deck**: social_request
**Session Model**: Session-bounded (must complete in single conversation, Doubt=10 ends conversation)

**Goal**: "Request Delivery"
- Description: "Elena needs someone to deliver urgent documents. She's assessing whether you're capable."
- System Type: Social
- NPC ID: elena
- Deck ID: social_request

**Goal Cards** (tiered victory conditions):
1. **Accept Basic Terms** (6 Momentum threshold):
   - Rewards: Obligation "Deliver Documents" added, +5 coins upfront
   - "Elena entrusts you with the documents. 'Be careful. The route is dangerous.'"

2. **Negotiate Better Terms** (10 Momentum threshold):
   - Rewards: Obligation "Deliver Documents" added, +5 coins upfront, +10 coins on delivery (instead of +8), +1 Rapport token with Elena
   - "Elena appreciates your questions and confidence. 'You seem capable. I'll pay extra if you complete this quickly and safely.'"

**Core Resources**:
- **Momentum** (builder): 0/10 (progress toward goal in single session)
- **Initiative** (session builder): Starts at 0, accumulated via Foundation cards, persists through LISTEN, max 10
- **Doubt** (threshold): Starts at 0, maximum 10 ends conversation (Elena's personality: Devoted, accumulates +2 per action instead of +1)
- **Cadence** (balance): Starts at 0, range -5 to +5
  - High Cadence (dominating, positive): +1 Doubt per point on LISTEN (dangerous with Devoted personality)
  - Low Cadence (deferential, negative): +1 card draw per point on LISTEN (strategic advantage)
- **Statements** (history): Count of Statement cards played, determines time cost (1 segment + Statements count)
- **Understanding** (global persistent): Universal tier unlock

**Card Flow Mechanics**:
- **SPEAK** thought from Mind: Statement thoughts move from Mind to Spoken pile (said aloud), Echo thoughts return to Topics (fleeting thoughts recirculate), increment Cadence (+1 per SPEAK)
- **LISTEN** to Topics: Draw Topics to Mind (base 3 + negative Cadence bonus), all existing thoughts in Mind persist (your mind accumulates), decrement Cadence (-2), apply Doubt penalty from positive Cadence
- Mind accumulates understanding - thoughts don't vanish when you pause to listen
- Statements in Spoken pile stay said (conversation history)

**Example Card Play**:
1. Start: 5 thoughts in Mind, 0 Initiative, 0 Momentum, 0 Doubt, 0 Cadence, 0 Statements
2. Play "Polite Greeting" (SPEAK, depth 2, Statement, Foundation): -0 Initiative, +1 Initiative, +1 Cadence, +1 Statements (now 1), move to Spoken
3. Play "Express Empathy" (SPEAK, depth 4, Rapport-bound, Statement): -3 Initiative (have 1, CAN'T AFFORD YET)
4. Play "Pause to Listen" (LISTEN, Foundation): Draw 3 Topics to Mind, -2 Cadence (now -1), +0 Doubt (no positive Cadence penalty), hand now 7 thoughts
5. Play "Polite Greeting" again (Echo'd back to Topics): +1 Initiative (now 2), +1 Cadence (now 0), +1 Statements (now 2), returns to Topics (Echo)
6. Play "Express Empathy" (now affordable): -3 Initiative, +2 Momentum, +1 Cadence, +1 Statements (now 3), move to Spoken
7. Continue: Build Initiative via Foundation → Spend on meaningful statements → LISTEN to reset Cadence and draw more

**Personality Rule**: Devoted (Elena) - Doubt accumulates +2 instead of +1, requires efficiency, minimize actions

**Victory**: Reach Momentum threshold (6 or 10) before Doubt reaches 10.

**Time Cost**: 1 segment + Statements count (2-4 segments typical)

---

## Investigation: The Mill Mystery

### Investigation Overview

**Discovery Trigger**: Conversational Discovery - Martha mentions her daughter's disappearance during "Gather Information" conversation (requires 6+ Momentum to unlock personal story)

**Structure**: Three-phase sequential investigation demonstrating all three challenge systems
- Phase 1: Mental challenge at Mill Exterior
- Phase 2: Physical challenge at Mill Interior
- Phase 3: Social challenge with Martha

**Investigation State**: Each phase spawns goal dynamically when requirements met

**Total Time**: 6-12 segments (2-3 segments per phase, depending on thoroughness)

---

### Phase 1: Mental Challenge - "Investigate Mill Exterior"

**Location**: Mill Exterior at Mill Venue
**System Type**: Mental
**Deck**: mental_observation
**Session Model**: Pauseable (can leave and return, state persists at location)

**Requirements**:
- Knowledge "martha_daughter_story" (from Martha conversation at 6+ Momentum)
- No equipment required (but lantern recommended for Phase 2)

**Goal**: "Investigate Mill Exterior"
- Description: "Martha's daughter disappeared here a decade ago. The mill holds secrets."
- System Type: Mental
- Location ID: mill_exterior
- Deck ID: mental_observation

**Goal Cards**:
1. **Surface Investigation** (8 Progress threshold):
   - Rewards: Knowledge "mill_recently_visited", +1 Understanding
   - "Fresh footprints in the mud. Someone has been here recently, and recently enough to matter."

2. **Thorough Investigation** (15 Progress threshold):
   - Rewards: Knowledge "mill_mechanism_unstable", knowledge "hidden_entry_found", +2 Understanding, +1 Insight XP, unlocks Phase 2
   - "The main mechanism is dangerously unstable. But you've found a side entry - safer than the collapsed front door. The wheel's gear housing has an access panel, hidden but visible to careful eyes."

**Core Resources** (Mental challenge):
- **Progress** (builder, persists): 0/15
- **Attention** (session budget, resets): 30 per session (costs 10 Focus per attempt)
- **Exposure** (persistent penalty): 0 initially (persists at location)
- **Leads** (observation flow, persists): 0 initially (persists when leaving)

**Location Properties**: Delicate (fragile evidence, Exposure +2 per ACT - requires Cunning-focused approach to minimize footprint)

**Narrative Description**:
"The abandoned mill looms ahead, wheel frozen mid-rotation, moss-covered and decaying. The path is overgrown - clearly no one uses this route anymore. The building shows structural damage, gaps in walls, sagging roofline. The creek flows beyond it, louder here near the wheel mechanism.

But something's wrong with the 'abandoned' narrative. Fresh disturbance in the undergrowth. Recent footprints, maybe a week old. Someone has been here, and not casually passing through."

**ACT Card Effects** (examples):
- "Notice Details" (depth 2, Insight-bound, ACT): +1 Progress, +1 Lead, +2 Exposure (Delicate penalty), costs 0 Attention
- "Cover Tracks" (depth 4, Cunning-bound, ACT): +2 Progress, +2 Leads, -1 Exposure (counters Delicate penalty), costs 3 Attention
- "Empathetic Reading" (depth 4, Rapport-bound, ACT): +2 Progress, +2 Leads, +1 to next Social challenge, +2 Exposure, costs 3 Attention

**Victory Condition**: Reach 15 Progress for Thorough Investigation (unlocks Phase 2). Can reach 8 Progress for Surface Investigation but investigation doesn't progress to Phase 2.

**Time Cost**: 1-3 segments (depending on how many sessions needed), costs 10 Focus per session

---

### Phase 2: Physical Challenge - "Navigate Dangerous Interior"

**Location**: Mill Interior at Mill Venue
**System Type**: Physical
**Deck**: physical_athletics
**Session Model**: One-shot (must complete or fail in single attempt)

**Requirements**:
- Phase 1 complete (knowledge "mill_mechanism_unstable" and "hidden_entry_found")
- Recommended: Rope (reduces Danger), Lantern (enables better Options), Tools (enables stabilization)

**Goal**: "Navigate Dangerous Interior"
- Description: "The mill's interior is dangerous - rotting floors, unstable mechanism, structural collapse risk. But the hidden compartment is inside."
- System Type: Physical
- Location ID: mill_interior
- Deck ID: physical_athletics

**Goal Cards**:
1. **Quick Passage** (8 Breakthrough threshold):
   - Rewards: Knowledge "compartment_accessed", safe passage to far side
   - "You've forced through the structure without triggering collapse. The compartment is accessible but you didn't examine its contents."

2. **Careful Navigation** (14 Breakthrough threshold):
   - Rewards: Knowledge "found_ledgers", knowledge "daughter_token_found", +2 Understanding, +1 Cunning XP, unlocks Phase 3
   - "You've navigated the structure carefully, stabilizing as you went. Inside the gear housing compartment: old ledgers documenting a smuggling operation from 10 years ago. Names in the ledgers include the current town elder. And a token - belongs to Martha's daughter. She found this. She discovered the smuggling. She was silenced."

**Core Resources** (Physical challenge):
- **Breakthrough** (builder): 0/14
- **Exertion** (session budget): 35 (derived from Stamina 100, costs 25 Stamina to attempt - higher cost for dangerous challenge)
- **Danger** (threshold): 0, maximum 12 before injury (reaching max = 20 Health loss, challenge fails)
- **Aggression** (balance): 0, range -10 to +10

**Challenge Type**: Athletics (climbing through dangerous structure, Insight/Rapport/Cunning-focused, first card in combo reduces risk for subsequent cards)

**Narrative Description**:
"The side entry leads into darkness. Your lantern reveals decades of abandonment - rotted grain bags, rusted tools, structural decay. The massive grinding mechanism dominates the space, wheel shaft running through the wall to the waterwheel outside.

The loft above has questionable flooring - boards sag visibly. The gear housing access panel is corroded but visible, just as you observed from outside. But the structure is unstable. Wrong approach triggers partial collapse."

**Equipment Modifiers**:
- **With Rope**: Can use "Secure Position" Option (reduces Danger when in combo)
- **With Lantern**: Better Options available (can see structural weaknesses)
- **With Tools**: Can use "Stabilize Structure" Option (guarantees safe access)
- **Without Equipment**: Reduced Options, higher baseline Danger

**EXECUTE Card Effects** (examples with equipment):
- "Steady Yourself" (depth 2, Foundation, EXECUTE): +1 Breakthrough, +0 Aggression, costs 0 Exertion
- "Structural Analysis" (depth 4, Insight-bound, EXECUTE, requires lantern): +2 Breakthrough, reduces Danger of next card in combo, +1 Aggression, costs 2 Exertion
- "Secure Position" (depth 4, Cunning-bound, EXECUTE, requires rope): +2 Breakthrough, -1 Danger per locked card, +1 Aggression, costs 3 Exertion
- "Stabilize Structure" (depth 6, Diplomacy-bound, EXECUTE, requires tools): +3 Breakthrough, guarantees -2 Danger from combo, +1 Aggression, costs 4 Exertion

**Victory Condition**: Reach 14 Breakthrough for Careful Navigation (unlocks Phase 3). Can reach 8 Breakthrough for Quick Passage but doesn't unlock Phase 3 (missing evidence).

**Failure Condition**: Reaching Danger maximum (12) causes 20 Health loss and forces exit. Can retry after recovery but costs another 25 Stamina.

**Time Cost**: 2 segments (setup + attempt), costs 25 Stamina, risks 20 Health on failure

---

### Phase 3: Social Challenge - "Confront Martha About Findings"

**NPC**: Martha at Tavern Venue
**System Type**: Social
**Deck**: social_confrontation
**Session Model**: Session-bounded (must complete in single conversation)

**Requirements**:
- Phase 2 complete (knowledge "found_ledgers" and "daughter_token_found")
- Must have Martha's daughter token as evidence

**Goal**: "Confront Martha About Findings"
- Description: "You've discovered what happened to Martha's daughter. She deserves to know the truth, but this conversation will be difficult."
- System Type: Social
- NPC ID: martha
- Deck ID: social_confrontation

**Goal Cards**:
1. **Reveal Basic Truth** (8 Momentum threshold):
   - Rewards: Knowledge "daughter_mystery_solved", +1 Rapport token with Martha, +1 Understanding
   - "Martha receives the news with quiet grief. 'I always knew something happened to her. Thank you for finding the truth.'"

2. **Full Closure** (12 Momentum threshold):
   - Rewards: Investigation complete, +2 Rapport tokens with Martha, +2 Understanding, +1 Rapport XP, knowledge "elder_exposed", Martha becomes Close Friend
   - "You provide not just the truth but the token - proof her daughter existed, proof she found something important, proof her disappearance mattered. Martha weeps, but there's relief in the tears. 'She was brave. She found the truth. And now everyone will know.' The town elder's crimes will be exposed. Anna's memory honored."

**Core Resources** (Social challenge):
- **Momentum** (builder): 0/12
- **Initiative** (session builder): 0, accumulated via Foundation, max 10
- **Doubt** (threshold): 0, maximum 10 ends conversation (Martha's personality: Steadfast, all effects capped at ±2)
- **Cadence** (balance): 0, range -5 to +5
- **Statements** (history): 0 initially

**Personality Rule**: Steadfast (Martha) - All effects capped at ±2, rewards patient grinding, no explosive gains or losses

**Narrative Description**:
"Martha looks up as you approach, sensing something in your expression. 'You found something at the mill.' It's not a question. Her hands still on the table, waiting.

This conversation will be difficult. You're about to tell a mother what happened to her daughter a decade ago. But you're also giving her closure - proof that Anna mattered, that she discovered something important, that her disappearance wasn't random tragedy but consequence of bravery."

**SPEAK Card Effects** (examples, all capped at ±2 by Steadfast):
- "Express Empathy" (depth 4, Rapport-bound, Statement): +2 Momentum (capped), +1 Cadence, costs 3 Initiative
- "Present Evidence" (depth 4, Insight-bound, Statement): +2 Momentum (capped), +1 Cadence, +1 Understanding, costs 3 Initiative
- "Offer Comfort" (depth 5, Diplomacy-bound, Statement): +2 Momentum (capped), +1 Cadence, -1 Doubt (capped at -2), costs 4 Initiative
- "Promise Justice" (depth 6, Authority-bound, Statement): +2 Momentum (capped), +1 Cadence, +2 Doubt (capped), costs 5 Initiative

**Victory Condition**: Reach 12 Momentum for Full Closure (completes investigation). Can reach 8 Momentum for Reveal Basic Truth but doesn't maximize relationship benefit.

**Time Cost**: 2-3 segments (1 base + Statements count, emotional conversation takes time)

---

### Investigation Rewards Summary

**Completing All Three Phases**:
- Knowledge entries: "martha_daughter_story", "mill_mechanism_unstable", "found_ledgers", "daughter_mystery_solved", "elder_exposed"
- Understanding: +5 total (tier unlocking toward better cards)
- Stat XP: +1 Insight, +1 Cunning, +1 Rapport (toward stat leveling)
- Relationship: Martha becomes Close Friend (+3 Rapport tokens, deep trust)
- Narrative: Town elder exposed, Anna's memory honored, Martha gains closure
- Mechanical: Mill path now safe (mechanism knowledge prevents danger)

**Time Investment**: 6-9 segments total (Mental 1-3, Physical 2, Social 2-3)
**Resource Costs**: 20-30 Focus, 25 Stamina, potential Health loss if careless
**Equipment Required**: Rope, Lantern, Tools recommended but not mandatory

---

## Travel Routes

### Route 1: Widow's Creek Direct

**Distance**: Town Outskirts → Farmstead (4 segments if successful)
**Danger**: Water crossing without bridge
**No Challenge Structure** (pure obstacle, not challenge system)

**First Arrival** (after 2 segments travel):
"Widow's Creek runs fast and cold. The ford here is maybe fifteen feet across, but the current is strong. Water's thigh-deep at the shallowest point. Your footing will be uncertain, and one slip means you're swept downstream."

**Choices**:

**Option 1: Wade Across Directly**
- Requires: Stamina 40+
- Risk: 60% chance of failure (deterministic check based on current Stamina)
- Failure: Swept downstream, lose 30 Health, lose 20 Stamina, documents get wet (delivery fails), washed up 3 segments away
- Success: Cross safely, continue to farmstead (2 more segments)

**Option 2: Turn Back to Try Mill Route**
- No loss except time spent (2 segments to return)
- Mill route becomes necessary
- Learn: Need better preparation or different route

**Weather Factor**: If attempted after Day 4 Morning: Impossible, must use mill route or wait a week (fails deadline)

---

### Route 2: Mill Path

**Distance**: Town Outskirts → Mill Venue → Farmstead (6 segments base + challenge time)
**Benefits**: Safe crossing upstream of wheel, avoids creek danger
**Challenges**: Must complete "Clear Blocked Path" Physical challenge, optionally investigate mill

**Segment 1: Travel to Mill Approach**
- Stamina Cost: 10
- No special dangers

**Segment 2: "Clear Blocked Path" Physical Challenge**
- Required to proceed
- 1 segment (challenge time)
- Costs 20 Stamina to attempt

**Segment 3-?: Mill Investigation (Optional)**
- Can skip for quick passage (1 segment to pass through)
- Or investigate (6-9 segments for all phases)
- State persists if you leave and return

**Segment ?: Mill to Farmstead**
- Safe crossing via mill's upstream access
- Clear maintained path beyond mill
- 2 segments to reach farmstead
- No additional obstacles

**Total Time**:
- Quick passage: 6 segments (1 to approach, 1 clear path, 1 quick pass, 2 to farmstead, 1 delivery)
- Full investigation: 12-15 segments (same + 6-9 investigation)

---

## Timeline and Resource Management

### Day 1 (Morning Start, 23 Segments Remaining)

**Morning Block** (Segments 1-4):
- Segment 1: "Request Delivery" Social challenge with Elena (accept obligation)
- Segment 2-3: "Gather Information" Social challenge with Martha (learn about mill, daughter story, routes)
- Segment 4: Purchase equipment at Market (rope, lantern, tools = 6 coins total)

**Midday Block** (Segments 5-8):
- Option A: Work for coins (4 segments, earn 5 coins, delays adventure)
- Option B: Attempt route (if brave/unprepared, likely fail and learn)
- Option C: "Examine Town Memorial" Mental challenge (test system, 1-2 segments, costs 10 Focus)

**Afternoon Block** (Segments 9-12):
- Option A: Begin mill route if prepared (clear path, start investigation)
- Option B: Attempt creek route if confident (risky without preparation)
- Option C: Rest, final preparation, more equipment

**Evening Block** (Segments 13-16):
- Continue investigation or complete delivery if route started

**Resources After Day 1**:
- Coins: 15 start - 6 equipment = 9 remaining (or +5 if worked)
- Stamina: 100 - 10-30 (travel/challenges) = 70-90 remaining
- Focus: 100 - 10-20 (Mental challenges) = 80-90 remaining
- Health: 100 (unless failed challenge)
- Time: 12-16 segments spent of 23 available

### Day 2-3 (Completing Investigation and Delivery)

**Deadline Pressure**:
- Heavy rains Day 4 Morning (Segment 24)
- Must deliver before deadline or fail obligation
- Creek route becomes impossible after rain starts

**Possible Patterns**:

**Aggressive Pattern** (High Risk):
- Day 1: Accept obligation, minimal info, attempt creek → FAIL (swept away, injured)
- Day 2: Recover (rest blocks), gather information properly, purchase equipment
- Day 3: Mill route with preparation, complete delivery
- Result: Learn through failure, tight timeline, succeed on retry

**Thorough Pattern** (Low Risk):
- Day 1: Accept obligation, gather information, purchase equipment, "Examine Memorial" test
- Day 2: Mill route, "Clear Path", complete full investigation (all 3 phases), reach farmstead
- Day 3: Deliver documents, return to town, celebrate with Martha
- Result: Maximum narrative payoff, comfortable timeline, all content experienced

**Efficient Pattern** (Medium Risk):
- Day 1: Accept obligation, minimal info, purchase rope + lantern only, mill route quick passage
- Day 1 Evening: Deliver documents successfully
- Day 2-3: Return to investigate mill thoroughly with proper preparation
- Result: Obligation complete early, investigation at leisure, no deadline pressure

### Resource Balance

**Coins**:
- Start: 15
- Equipment recommended: 6-8 (rope 2, lantern 2, tools 2, rations 2)
- Work income: 5 per block worked
- Delivery payment: 8 on success (or 10 with negotiation)
- Balance: Can afford basic equipment OR work 1 block for full preparation

**Stamina**:
- Start: 100
- Travel costs: 10 per segment walking
- Physical challenges: 20-25 per attempt
- Recovery: 40 per rest block, 20 per meal (rations)
- Critical: Below 30 → max Exertion reduced in Physical challenges
- Balance: Can afford 2-3 Physical challenges or extensive travel without rest

**Focus**:
- Start: 100
- Mental challenges: 10 per attempt
- Recovery: 30 per rest block, light activity
- Critical: Below 30 → Exposure accumulates faster in Mental challenges (+1 per action)
- Balance: Can afford 5-10 Mental challenge sessions before needing rest

**Health**:
- Start: 100
- Danger from: Failed challenges, creek crossing failure, structural collapse
- Recovery: Slow (rest blocks), expensive (medicine)
- Critical: Below 30 → Danger accumulates faster in Physical challenges (+1 per action)
- Balance: High health buffer allows risk-taking, injuries teach caution

**Time**:
- Available: 23 segments before deadline
- Delivery minimum: 6 segments (efficient mill route quick passage)
- Investigation full: 12-15 segments (mill route + full investigation)
- Generous buffer: 8-17 segments remaining for preparation, failures, rest
- Balance: Plenty of time for thorough approach or recovery from mistakes

---

## Learning Outcomes

### Three Challenge Systems (Parallel Architecture)

**Mental Challenges** teach:
- Pauseable investigation (can leave and return, state persists)
- ACT generates Leads → OBSERVE draws Details equal to Leads
- Methods persist in hand (investigation knowledge doesn't vanish)
- Leads persist when leaving (investigative threads remain open)
- Progress accumulates toward victory across multiple visits
- Exposure persists as investigative footprint (penalty increases)
- Focus cost to attempt (concentration resource)
- Location properties alter tactics (Delicate = requires Cunning approach)

**Physical Challenges** teach:
- One-shot attempts (can't pause mid-challenge, must complete or fail)
- EXECUTE locks Options → ASSESS triggers combo and resets
- Aggression balance critical (both extremes penalized)
- Combo execution uses projection (Breakthrough, Danger, Aggression applied together)
- Equipment enables better Options (rope, lantern, tools create tactical choices)
- Stamina cost to attempt + Health risk on failure (exertion + injury)
- Challenge types alter tactics (Athletics = first card reduces risk for sequence)
- Preparation dramatically improves success chance

**Social Challenges** teach:
- Session-bounded (can't pause mid-conversation, NPC has patience limit)
- SPEAK moves thoughts → LISTEN draws while Mind persists
- Cadence management (deferential = card draw bonus, dominating = Doubt penalty)
- Initiative building (Foundation cards generate, spend on meaningful statements)
- Personality rules alter tactics (Devoted = efficiency required, Steadfast = patient grinding)
- No permanent resource cost (conversation is "free" but takes time)
- Statements count determines time cost (1 segment + Statements)
- Relationship building through repeated successful interactions

### Integration Understanding

**Conversations Serve Adventure**:
- Martha conversation unlocks investigation (daughter story triggers Phase 1)
- Elena conversation provides obligation (drives exploration)
- Information gathering provides route knowledge (safe crossing, equipment needs)
- Not separate mini-game - practical tool for adventure preparation

**Investigation Demonstrates All Systems**:
- Mental Phase 1: Gather evidence through observation
- Physical Phase 2: Navigate danger through action
- Social Phase 3: Share discovery through conversation
- Sequential progression shows how systems work together

**Resources Enable Challenges**:
- Coins buy equipment (rope enables Physical tactics)
- Equipment unlocks Options (lantern provides better Physical cards)
- Focus budget limits Mental attempts (must manage concentration)
- Stamina budget limits Physical attempts (must manage exertion)
- Time budget creates urgency (deadline forces efficiency vs. thoroughness)

**Preparation Matters**:
- Aggressive route (unprepared creek attempt) likely fails, teaches importance of information
- Thorough route (equipment, knowledge, planning) succeeds comfortably
- Equipment dramatically improves challenge success (rope, lantern, tools unlock better Options)
- Knowledge gates better choices (Martha's info reveals safe mill route)
- Resource management prevents depletion (rest blocks, rations, pacing)

### Mechanical Depth Through Verisimilitude

**Why Mental Pauseable**:
- Real investigations take multiple sessions with breaks
- Evidence doesn't vanish when investigator leaves
- Returning with fresh perspective is normal investigative practice

**Why Physical One-Shot**:
- Can't pause mid-climb and return tomorrow
- Physical challenges test current capability in the moment
- Your body state persists but challenge state doesn't

**Why Social Session-Bounded**:
- Conversations happen in real-time with dynamic entities
- NPCs have patience limits (Doubt accumulates)
- Can't pause mid-conversation and return hours later

**Result**: Three systems with equivalent tactical depth achieved through parallel architecture that respects what you're actually doing. Parity is in depth and complexity, not mechanical sameness.

---

## Conclusion

This POC demonstrates the complete Wayfarer experience across all three challenge systems:

**Mental Challenge** ("Examine Town Memorial", Investigation Phase 1): Pauseable investigation using ACT to generate Leads and OBSERVE to draw Details. Progress persists across visits. Exposure accumulates as investigative footprint. Focus cost creates resource pressure.

**Physical Challenge** ("Clear Blocked Path", Investigation Phase 2): One-shot attempt using EXECUTE to lock Options and ASSESS to trigger combo. Aggression balance critical - both extremes penalized. Equipment unlocks better Options. Stamina cost + Health risk create preparation pressure.

**Social Challenge** ("Request Delivery", "Gather Information", Investigation Phase 3): Session-bounded conversation using SPEAK to move thoughts and LISTEN to draw while Mind persists. Cadence management creates tactical choice. Personality rules alter approach. Time cost from Statements count.

**The Mill Mystery Investigation** layers all three systems in sequence:
1. Mental investigation gathers evidence (discover mechanism danger, find entry)
2. Physical navigation overcomes obstacle (access compartment, find ledgers)
3. Social confrontation provides closure (reveal truth, honor daughter's memory)

**Resource management** serves adventure goals: coins buy equipment enabling Physical tactics, Focus budget limits Mental attempts, Stamina budget limits Physical attempts, time budget creates urgency forcing efficiency vs. thoroughness trade-offs.

**Preparation importance** demonstrated through risk: aggressive unprepared attempts likely fail (creek crossing without knowledge), thorough prepared attempts succeed (mill route with equipment and information), failure teaches what's needed for retry (resource persistence allows learning).

**Integration** shows systems serving adventure: conversations provide information unlocking investigations, investigations require all three challenge types, obligations drive exploration toward interesting content, resources enable meaningful preparation.

**Verisimilitude** justifies asymmetries: Mental pauseable (investigations take time), Physical one-shot (can't pause mid-climb), Social session-bounded (NPCs have patience), different permanent costs (Mental = Focus, Physical = Stamina + Health, Social = time only).

**Time**: 30-45 minutes first playthrough teaches investigation structure, challenge mechanics, resource management, conversation integration, preparation importance, how systems serve adventure rather than competing for attention.

**Character emerges through priorities**: thorough investigation vs. efficient delivery, safe preparation vs. aggressive attempts, helping NPCs vs. practical efficiency, character revealed through choices under constraint.

Frieren-style slice-of-life adventure: small personal scale, grounded dangers, relationships through shared challenges, discovery as reward, no endgame just ongoing life as courier in world that doesn't revolve around you.
