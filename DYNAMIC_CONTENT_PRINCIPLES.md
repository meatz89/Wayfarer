# Dynamic Content Creation Architecture

## Fundamental Paradigm Shift

**From: Content Unlocking**
- Pre-authored library of scenes at locations
- Check conditions/flags/requirements
- Filter available scenes
- Present options

**To: Content Creation**
- Actions create new scenes at target locations
- Scenes spawn with resource costs
- No checking, only spawning
- Scenes consumed or expire

**Core Insight:** Content doesn't wait to be unlocked. Content is created by player actions.

---

## Principle 1: Actions Create Content, Resources Gate Participation

**Actions have outputs, not just rewards:**

```
Complete delivery to Elena
→ Creates scene "Elena's Gratitude" at Brass Bell Inn
→ Scene appears immediately
→ Entering costs 0 (created as consequence)
→ Expires after 1 interaction

Help during fire event
→ Creates scene "Elena's Trust" at Brass Bell Inn
→ Creates card "Elena Remembers" (goes into player's card collection)
→ Scene expires in 3 days
→ Card persists permanently
```

**Resources gate entry, not existence:**

```
Scene "Guild Meeting" created at Guild Hall every Tuesday evening
→ Entry cost: 5 coins (attendance fee)
→ Without coins: See scene but cannot enter
→ Perfect information: Player knows cost before committing

Scene "Thomas Conversation" always exists at Mill
→ Entry cost: 2 Initiative (start conversation)
→ If you own card "Thomas Owes Favor": Entry cost 0
→ Resources modify economics
```

**No boolean gates. Only resource costs.**

---

## Principle 2: Locations Have Persistent Features + Active Scenes

**Location Structure:**

```
Brass Bell Inn
├─ Persistent Features (always present)
│  ├─ Buy room (10 coins → rest)
│  ├─ Work for coins (1 segment + 1 Stamina → 5 coins)
│  └─ Order food (3 coins → 2 Health)
└─ Active Scenes (ephemeral, created by actions)
   ├─ "Elena's Gratitude" (spawned by delivery, one-time)
   ├─ "Merchant Seeks Help" (spawned by investigation, expires in 2 days)
   └─ "Fire Aftermath" (spawned by event, expires after interaction)
```

**Persistent features:**
- Always available
- Represent location's core function
- Resource conversion (coins for goods, time for coins, etc.)
- No creation needed, part of location definition

**Active scenes:**
- Created by completing actions
- Temporary (expire by time or interaction)
- Narrative moments
- Advance investigations, relationships, story

---

## Principle 3: Scene Lifecycle is Creation → Availability → Consumption/Expiration

**Creation:**
- Action completion specifies: target location, scene parameters
- Scene added to location's active list
- Scene becomes immediately available

**Availability:**
- Scene appears at location
- Shows entry cost (resources needed)
- Player can assess cost before committing

**Consumption:**
- Player enters scene (pays cost)
- Scene plays out (exchanges, challenges, choices)
- One-time scenes remove after interaction
- Repeatable features persist

**Expiration:**
- Time-based: Scene expires after N days/segments
- Event-based: Other action makes scene obsolete
- Expired scenes remove automatically
- Player may miss content

---

## Principle 4: Equipment and Knowledge are Owned Resources That Modify Costs

**Equipment as cost reducers:**

```
Location feature "Bell Tower Observation" at Church
→ Base cost: 4 Stamina + Physical challenge threshold 8
→ Own Climbing Gear: Cost becomes 1 Stamina + threshold 4
→ Same scene, different economics

Location feature "Ancient Text Translation" at Library
→ Base cost: 6 Focus + Mental challenge threshold 10
→ Own Scholar's Glasses: Cost becomes 2 Focus + threshold 5
→ Equipment makes expensive thing affordable
```

**Knowledge as cards you own:**

```
Complete Mental challenge "Examine Waterwheel"
→ Gain card "Evidence of Sabotage"
→ Card goes into player's collection
→ Card appears in hand during Thomas conversation
→ Playing card: +4 Momentum effect
→ Without card: Same conversation available, just harder
```

**Pattern:** Equipment/knowledge don't unlock content. They make content cheaper or easier.

---

## Principle 5: NPCs Track Relationship Through Cards and Scenes

**No memory flags. Cards represent history.**

```
Help Elena during fire
→ Creates scene "Elena's Gratitude"
→ Grants card "Elena Remembers Fire"

Later, in any Elena conversation:
→ Card appears in your hand (you own it)
→ Can play: "Remind her what you did" (+4 Momentum)
→ She responds positively (mechanical effect)
```

**Relationship progression through scene chains:**

```
Complete delivery (basic action)
→ Creates scene "Elena Thanks You" (immediate)
→ Scene grants +2 Elena cubes

Accumulate 5 Elena cubes
→ Spend them in scene "Share Personal Story" (costs 3 cubes to enter)
→ Creates scene "Elena's Trust" (spawned after you paid cubes)
→ Scene grants card "Elena's Favor"

Later use "Elena's Favor" card
→ In any social situation, play card for bonus
→ Card consumed after use
→ Elena remembers helping you
```

**Cubes are currency spent to create deeper content.**

---

## Principle 6: Investigations Are Scene Chains, Not Phase Gates

**Wrong pattern:**
```
Investigation has Phase 1, 2, 3
Check: Is Phase 1 complete?
If yes, unlock Phase 2
```

**Correct pattern:**
```
Accept investigation "Silent Mill"
→ Creates scene "Examine Waterwheel" at Mill
→ Cost: 1 Focus

Complete "Examine Waterwheel"
→ Grants card "Evidence of Sabotage"
→ Creates scene "Question Mill Owner" at Mill Owner's House
→ Cost: 2 Initiative

Complete "Question Mill Owner"
→ Creates TWO scenes (player chooses which):
  • "Search Shed" at Mill Storage (Physical)
  • "Ask Neighbors" at Market Square (Social)
→ Cost: 1 Stamina or 1 Focus respectively

Complete either
→ Creates scene "Confront Culprit" (final)
→ Investigation completes
```

**Each scene creates next scene. No phase checking.**

---

## Principle 7: Failure Creates Different Content, Not Repeated Content

**Success and failure spawn different scenes:**

```
Scene "Question Suspect" (Social challenge)

Success outcome:
→ Creates scene "Suspect Confesses" at Guard House
→ Clean resolution path

Failure outcome:
→ Creates scene "Suspect Flees Town" at Town Gate
→ Now Physical challenge to catch them
→ Different mechanics, same investigation completion
```

**Failure redirects, doesn't block.**

---

## Principle 8: Time Creates and Expires Scenes

**Schedule-based creation:**

```
Tuesday evening arrives (segment 13)
→ System creates scene "Guild Meeting" at Guild Hall
→ Scene expires at segment 16 (meeting ends)
→ Cost to enter: 5 coins

Player lacks coins:
→ Sees scene but cannot afford entry
→ Meeting expires, opportunity lost
→ No flag "missed meeting", just no content created
```

**Time-based expiration:**

```
Complete action Monday segment 8
→ Creates scene "Elena's Request" at Inn
→ Scene expires Thursday segment 16
→ If not visited by then, removed automatically
→ No penalty beyond lost opportunity
```

**Time is resource that creates/expires content, not condition that unlocks it.**

---

## Principle 9: World State is Owned Resources, Not Boolean Flags

**Player owns:**
- Coins (numeric)
- Health/Stamina/Focus (numeric)
- Cubes per NPC (numeric)
- Cards (collection of typed objects)
- Equipment (collection of typed objects)
- Current time (numeric segment/day)

**Player does NOT have:**
- Boolean flags
- Memory states
- Quest progress integers
- Relationship status enums

**All state is resources with values that can be spent or used.**

---

## Principle 10: Content Scarcity Through Natural Limits

**Locations have limited scene capacity:**

```
Brass Bell Inn supports 3 active scenes maximum
Current: 2 scenes active

New scene created:
→ Adds to location (now 3 active)

Another new scene created:
→ Location at capacity
→ Oldest scene expires
→ New scene takes its place
```

**Simple rule: Oldest expires when location full.**

No priority systems. No complex sorting. Just: first in, first out when capacity reached.

---

## How Existing Systems Adapt

### Obstacles at Locations

**Current: Obstacle with properties blocks goals**

**New: Obstacle spawns multiple approach scenes**

```
Obstacle "Locked Gate" at Town Border

Creates three persistent features at location:
1. "Force Gate" (Physical challenge, costs 3 Stamina)
2. "Pick Lock" (consumes Lockpicks item, costs 1 Focus)
3. "Bribe Guard" (costs 10 coins)

All three always available. Player chooses which resources to spend.

**CRITICAL DISTINCTION:**
- ❌ "Requires Lockpicks" = Boolean gate (have it or don't)
- ✅ "Consumes Lockpicks" = Resource cost (visible, predictable)

Player sees ALL options. Resources determine what they can AFFORD, not what they can SEE.
```

**Completing any approach:**
→ Creates scene "Beyond Gate" at destination
→ Removes gate features from Town Border
→ Travel now possible

### Goals at Locations/NPCs

**Current: Goal is action you can take**

**New: Goal completion creates follow-up scenes**

```
Location has persistent feature "Deliver Package to Thomas"
→ Cost: 1 segment (find him)

Complete delivery:
→ Grants: 15 coins, +2 Thomas cubes
→ Creates scene "Thomas Mentions Problem" at Mill
→ Investigation chain begins
```

**Goals are persistent features. Completing them spawns narrative content.**

### Challenge System Integration

**Challenges remain unchanged mechanically.**

**What changes: When/why challenges appear**

```
Scene "Negotiate Trade Agreement" at Merchant Guild

Scene entry: Costs 2 Initiative (start negotiation)
Scene content: Social challenge (standard thresholds)
Scene completion: Grants coins, creates follow-up scene

Same challenge mechanics. Different framing (scene, not abstract goal).
```

### Routes and Travel

**Current: Routes have obstacles**

**New: Travel creates scenes at destination**

```
Travel Creek Route (costs 2 segments + 2 Stamina)

Arrival at Mill:
→ Check for obstacles on route
→ If obstacle present: Creates scene "Deal With Obstacle"
→ Must resolve before accessing Mill features

No obstacle:
→ Arrive directly at Mill
→ Access all Mill features immediately
```

**Random events during travel:**

```
Travel Creek Route
→ 20% chance: Creates scene "Injured Traveler"
→ Scene appears at Creek Route location (temporary)
→ Options: Help (costs resources), Ignore (continue)
→ Different consequences based on choice
```

---

## What This Architecture Eliminates

**No condition checking:**
- No "if has flag then show scene"
- No "if relationship ≥5 then unlock"
- No "if completed phase then enable"

**No boolean state:**
- No flags
- No quest completion booleans
- No "talked_to_elena" tracking

**No unlock systems:**
- No gating
- No requirements
- No prerequisites

**No scene libraries:**
- No pre-authored collection awaiting conditions
- No filtering available scenes
- Only creating scenes when earned

---

## AI Content Generation Fit

**AI receives action completion event:**

```
Event: Player completed "Help During Fire"
Context: Location = Brass Bell Inn, NPC = Elena, Action type = Heroic

AI generates:
→ Scene "Elena's Gratitude"
→ Location: Brass Bell Inn
→ Entry cost: 0 (reward for action)
→ Narration: Elena's specific thanks
→ Response options: Humble/Proud/Request favor
→ Outputs: +3 Elena cubes, card "Elena Remembers"
→ Expiration: 3 days
```

**Template-driven with parameters:**
- What happened (action completed)
- Who involved (NPC)
- Where (location)
- Player personality from past choices

**AI doesn't need fixed scene library. Generates contextual scenes on demand.**

---

## Design Benefits

**Verisimilitude:**
- Content appears when earned, not checked
- World reacts to actions through new content
- NPCs remember through cards you own
- Time flows naturally creating/expiring opportunities

**Elegance:**
- One pattern: Actions create scenes
- One resource system: Everything is numeric or typed object
- One flow: Creation → Availability → Consumption
- No special cases

**Scalability:**
- Add actions by defining what they create
- AI generates scene content
- No exponential condition checking
- Linear complexity

**Board Game Feel:**
- Clear costs before committing
- Resource management drives decisions
- Action consequences visible (scenes spawn)
- No hidden state

**Impossible Choices:**
- Limited time before expiration
- Multiple scenes competing for resources
- Must choose which opportunities to pursue
- Scarcity through natural limits

---

## Implementation Questions for Refactoring

**Location data structure:**
- Persistent features list
- Active scenes list  
- Scene capacity limit

**Scene data structure:**
- Location reference
- Entry cost (resources)
- Expiration condition (time or interaction)
- Content (exchanges, challenges)
- Outputs (what it creates when complete)

**Action completion:**
- Specify scenes to create
- Specify location targets
- Specify resources granted

**Card system:**
- Player owns collection
- Cards appear in relevant challenges
- Cards consumed or permanent

**Cube system:**
- Per-NPC currency
- Spent to create intimate scenes
- Granted by completing relationship actions

---

## Migration Path from Current Architecture

**Phase 1: Dual system**
- Keep current goal/obstacle system
- Add scene spawning layer
- Goals can spawn scenes as outputs

**Phase 2: Convert investigations**
- Reframe phases as scene chains
- Each phase creates next phase
- Test investigation flow

**Phase 3: Convert NPCs**
- Conversations become scenes
- Relationship milestones spawn scenes
- Test NPC interaction flow

**Phase 4: Remove condition checking**
- Replace all "if X then Y" with cost gates
- Convert flags to cards/resources
- Clean up boolean state

**Phase 5: Full dynamic content**
- All content spawned by actions
- No pre-existing scene libraries
- AI generation integrated

---

## Core Principle Summary

**Content creation over content unlocking.**

Actions don't check conditions. Actions create content.

Scenes don't wait for requirements. Scenes spawn from actions.

Resources don't unlock features. Resources let you afford features.

Equipment doesn't gate content. Equipment reduces costs.

Knowledge doesn't enable options. Knowledge improves outcomes.

Failure doesn't block progress. Failure spawns different content.

Time doesn't lock scenes. Time creates and expires scenes.

**The world doesn't check what you've done. The world reacts by creating new opportunities.**
