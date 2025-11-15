# A1-A3 Tutorial Sequence: Implementation Specification (Corrected Architecture)

## Core Architectural Principles

**Dependency Inversion:** Content spawns via rewards (forward), never via state checking (backward)

**No Boolean Gates:** No SpawnConditions on tutorial scenes (except A1: AlwaysEligible)

**Direct Spawning:** SceneSpawnReward in choice rewards creates new scenes immediately

**No Soft-Lock:** Every final situation choice includes SceneSpawnReward

**Single Mechanism:** All content uses SceneSpawnReward (no special cases for A-story vs B-stories)

---

## A1: Secure Lodging Tutorial

### Scene Metadata
- **Template ID:** `a1_secure_lodging`
- **Category:** MainStory
- **Sequence:** 1
- **SpawnConditions:** AlwaysEligible (game start scene)
- **Base Placement:** VenueType: Inn

### Situations: 3

---

### Situation 1: Negotiate Lodging

**Placement:**
- LocationFilter: { LocationTypes: ["CommonRoom"], VenueType: "Inn" }
- NpcFilter: { Role: "Innkeeper", Demeanor: "Friendly" }
- GrantsLocationAccess: false

**Narrative:**
Elena, the friendly innkeeper, greets you at the common room. Evening approaches and you need shelter. She offers a private room.

**Choices:**

**1. Rapport-Gated (Optimal)**
- Requirements: Rapport ≥ 2
- Cost: None
- Action: "Charm Elena with traveler's tales"
- Rewards:
  - Elena bond +1
  - Advance to Situation 2

**2. Money-Gated (Reliable)**
- Requirements: None
- Cost: 10 coins
- Action: "Pay standard rate"
- Rewards:
  - Advance to Situation 2

**3. Social Challenge (Risky)**
- Requirements: None
- Cost: 1 time block, 10 Resolve
- Action: "Negotiate better terms (Social Challenge)"
- Challenge: Social, Easy (Momentum 8)
- Success Rewards:
  - Cost 5 coins (half price)
  - Elena bond +1
  - Understanding +1
  - Advance to Situation 2
- Failure Rewards:
  - Cost 10 coins (standard)
  - Elena bond -1
  - Advance to Situation 2

**4. Fallback (Guaranteed)**
- Requirements: None
- Cost: None
- Action: "Ask to sleep in common room"
- Rewards:
  - Skip to Situation 3 (bypasses private room)
  - Restore: Health +5, Stamina +5 (poor rest)

**Perfect Information Display:**
- Exact coin costs visible
- Rapport requirement vs player's current Rapport
- Challenge parameters (Momentum threshold, resource costs)
- Projection: "Advances to: Private Room (currently locked)" for choices 1-3
- Projection: "Advances to: Common Room Departure" for choice 4

**Routing:** ExitToWorld (Situation 2 at different location)

---

### Situation 2: Rest in Private Room

**Placement:**
- LocationFilter: { LocationTypes: ["GuestRoom"], PrivacyLevels: ["Private"], VenueType: "Inn" }
- NpcFilter: None
- GrantsLocationAccess: true

**Narrative:**
The private room is modest but clean. A comfortable bed awaits.

**Choices:**

**1. Full Rest**
- Requirements: None
- Cost: None
- Action: "Sleep through the night"
- Rewards:
  - Restore: Health +20, Stamina +20, Focus +20
  - Advance day counter
  - Advance to Situation 3

**2. Organized Rest**
- Requirements: None
- Cost: 1 time block
- Action: "Organize gear before sleeping"
- Rewards:
  - Restore: Health +20, Stamina +20, Focus +20
  - Advance day counter
  - Item: "organized_pack" (minor benefit)
  - Advance to Situation 3

**Routing:** ContinueInScene (same location)

---

### Situation 3: Morning Departure (FINAL)

**Placement:**
- LocationFilter: Inherits from Situation 2 (private room if arrived there) OR { LocationTypes: ["CommonRoom"] } if fallback path
- NpcFilter: None
- GrantsLocationAccess: true (if private room), false (if common room)

**Narrative:**
Morning sunlight. Time to begin the day.

**Choices (ALL SPAWN A2):**

**1. Standard Departure**
- Requirements: None
- Cost: None
- Action: "Head downstairs, ready for opportunities"
- Rewards:
  - **SceneSpawnReward:** {
      Template: "a2_first_delivery",
      LocationFilter: { LocationTypes: ["CommonRoom"], VenueType: "Inn" }
    }

**2. Thorough Preparation**
- Requirements: None
- Cost: 1 time block
- Action: "Double-check belongings before departing"
- Rewards:
  - Item: "well_prepared" (flavor benefit)
  - **SceneSpawnReward:** {
      Template: "a2_first_delivery",
      LocationFilter: { LocationTypes: ["CommonRoom"], VenueType: "Inn" }
    }

**Note:** Both choices spawn A2. Different preparation states affect flavor only, not mechanics.

**Routing:** SceneComplete

---

## A2: First Delivery Opportunity

### Scene Metadata
- **Template ID:** `a2_first_delivery`
- **Category:** MainStory
- **Sequence:** 2
- **SpawnConditions:** NONE (spawned by A1 choice reward)
- **Base Placement:** VenueType: Inn, LocationType: CommonRoom

### Situations: 3

---

### Situation 1: Morning in Room/Hall

**Placement:**
- LocationFilter: Inherits from A1 (private room if player had one) OR CommonRoom
- NpcFilter: None
- GrantsLocationAccess: true (if private room), false (if common room)

**Narrative:**
Morning. Sounds of common room activity below.

**Choices:**

**1. Head Downstairs**
- Requirements: None
- Cost: None
- Action: "Go to common room"
- Rewards:
  - Advance to Situation 2

**Routing:** ExitToWorld (if from private room) or ContinueInScene (if from common room)

---

### Situation 2: Opportunity Presents

**Placement:**
- LocationFilter: { LocationTypes: ["CommonRoom"], VenueType: "Inn" }
- NpcFilter: { Role: "Innkeeper" } (finds Elena from A1)
- GrantsLocationAccess: false

**Narrative:**
Elena waves you over. "Good timing! A merchant needs a courier. Package to Port District—20 coins, 4 time blocks. Interested?"

**Route Preview Displayed:**
- Destination: Port District
- Segments: 4
- Danger: Moderate
- Time: ~4 time blocks
- Payment: 20 coins
- Estimated Profit: 20 - 15 (food/lodging) = 5 coins

**Choices:**

**1. Accept Opportunity**
- Requirements: None
- Cost: None
- Action: "Yes, I'll take the job"
- Rewards:
  - Advance to Situation 3

**2. Decline**
- Requirements: None
- Cost: None
- Action: "Not yet, maybe later"
- Rewards:
  - Remains at Situation 2 (repeatable, player can reconsider)

**Routing:** ContinueInScene

---

### Situation 3: Contract Negotiation (FINAL)

**Placement:**
- LocationFilter: { LocationTypes: ["CommonRoom"], VenueType: "Inn" }
- NpcFilter: { Role: "Merchant" } (System 4 generates if needed)
- GrantsLocationAccess: false

**Narrative:**
The merchant presents the sealed package. "Port District warehouse, by evening. Standard terms: advance payment plus 10 coin completion bonus."

**Choices (ALL SPAWN A3, SPLIT PAYMENT MODEL):**

**1. Rapport-Gated (Optimal)**
- Requirements: Rapport ≥ 3
- Cost: None
- Action: "Leverage reputation for 15 coin advance"
- Rewards:
  - Coins: +15 (upfront payment)
  - Merchant bond +1
  - **SceneSpawnReward:** {
      Template: "a3_route_travel",
      RouteFilter: { From: "CurrentLocation", To: "PortDistrict", Difficulty: "Moderate" }
    }

**2. Money-Gated (Insurance)**
- Requirements: None
- Cost: 3 coins (deposit)
- Action: "Offer 3 coin deposit for 12 coin advance"
- Rewards:
  - Coins: +12 (upfront payment, net +9 after deposit)
  - Merchant bond +1
  - **SceneSpawnReward:** {
      Template: "a3_route_travel",
      RouteFilter: { From: "CurrentLocation", To: "PortDistrict", Difficulty: "Moderate" }
    }

**3. Social Challenge (Risky)**
- Requirements: None
- Cost: 1 time block, 10 Resolve
- Action: "Negotiate aggressively for maximum advance"
- Challenge: Social, Medium (Momentum 12)
- Success Rewards:
  - Coins: +17 (premium upfront payment)
  - Merchant bond +2
  - **SceneSpawnReward:** {
      Template: "a3_route_travel",
      RouteFilter: { From: "CurrentLocation", To: "PortDistrict", Difficulty: "Moderate" }
    }
- Failure Rewards:
  - Coins: +8 (minimal upfront payment)
  - Merchant bond -1
  - **SceneSpawnReward:** {
      Template: "a3_route_travel",
      RouteFilter: { From: "CurrentLocation", To: "PortDistrict", Difficulty: "Moderate" }
    }

**4. Fallback (Standard Accept)**
- Requirements: None
- Cost: None
- Action: "Accept standard 10 coin advance terms"
- Rewards:
  - Coins: +10 (standard upfront payment)
  - **SceneSpawnReward:** {
      Template: "a3_route_travel",
      RouteFilter: { From: "CurrentLocation", To: "PortDistrict", Difficulty: "Moderate" }
    }

**Perfect Information Display:**
- All four upfront payments visible: 17/15/12/10 coins (plus 10 completion)
- Total payouts: 27/25/22/20 coins (upfront + completion)
- Net profit calculations shown (total - 15 survival = net)
- Rapport requirement vs player's Rapport
- Challenge difficulty parameters
- Projection: "Advances to: Route Travel (4 segments, Moderate danger)"

**Routing:** SceneComplete

---

## A3: First Delivery Route Travel

### Scene Metadata
- **Template ID:** `a3_route_travel`
- **Category:** MainStory
- **Sequence:** 3
- **SpawnConditions:** NONE (spawned by A2 choice reward with parameters)
- **Parameters:** { ContractPayment: 18-27 } (from A2 negotiation)
- **Base Placement:** Route (market to port)

### Situations: 5 (4 segments + arrival)

---

### Situation 1: Forest Obstacle (Physical)

**Placement:**
- RouteFilter: { From: "CurrentLocation", To: "PortDistrict", SegmentIndex: 0 }
- GrantsLocationAccess: N/A (route sequential)

**Narrative:**
Massive fallen tree blocks the path. Locals work nearby. Detour would add time.

**Choices:**

**1. Authority-Gated (Optimal)**
- Requirements: Authority ≥ 3
- Cost: None
- Action: "Direct locals to clear path"
- Rewards:
  - Advance to Situation 2

**2. Money-Gated (Reliable)**
- Requirements: None
- Cost: 5 coins
- Action: "Pay locals to clear it"
- Rewards:
  - Advance to Situation 2

**3. Physical Challenge**
- Requirements: None
- Cost: 2 time blocks, 15 Stamina
- Action: "Clear obstacle yourself (Physical Challenge)"
- Challenge: Physical, Easy (Breakthrough 8)
- Success Rewards:
  - Understanding +1
  - Advance to Situation 2
- Failure Rewards:
  - Health -10 (strain injury)
  - Advance to Situation 2

**4. Fallback (Detour)**
- Requirements: None
- Cost: 1 time block
- Action: "Take longer detour"
- Rewards:
  - Advance to Situation 2

**Routing:** ContinueInScene

---

### Situation 2: River Crossing (Mental)

**Placement:**
- RouteFilter: { From: "CurrentLocation", To: "PortDistrict", SegmentIndex: 1 }
- GrantsLocationAccess: N/A

**Narrative:**
Swift river, no obvious crossing. Ferryman downstream charges steep rates.

**Choices:**

**1. Insight-Gated (Optimal)**
- Requirements: Insight ≥ 3
- Cost: None
- Action: "Spot safe shallows through analysis"
- Rewards:
  - Advance to Situation 3

**2. Money-Gated (Reliable)**
- Requirements: None
- Cost: 8 coins
- Action: "Pay ferryman"
- Rewards:
  - Advance to Situation 3

**3. Mental Challenge**
- Requirements: None
- Cost: 2 time blocks, 12 Focus
- Action: "Study crossing carefully (Mental Challenge)"
- Challenge: Mental, Easy (Progress 10)
- Success Rewards:
  - Understanding +1
  - Advance to Situation 3
- Failure Rewards:
  - Health -5 (poor route choice)
  - Advance to Situation 3

**4. Fallback (Risky Wade)**
- Requirements: None
- Cost: None
- Action: "Wade across carefully"
- Rewards:
  - Health -15 (guaranteed injury)
  - Advance to Situation 3

**Routing:** ContinueInScene

---

### Situation 3: Checkpoint Guard (Social)

**Placement:**
- RouteFilter: { From: "CurrentLocation", To: "PortDistrict", SegmentIndex: 2 }
- GrantsLocationAccess: N/A

**Narrative:**
Guard at checkpoint. "Toll road. Need to verify cargo—smugglers been active."

**Choices:**

**1. Rapport-Gated (Optimal)**
- Requirements: Rapport ≥ 3
- Cost: None
- Action: "Friendly conversation about the road"
- Rewards:
  - Guard bond +1
  - Advance to Situation 4

**2. Money-Gated (Reliable)**
- Requirements: None
- Cost: 10 coins
- Action: "Pay toll and inspection fee"
- Rewards:
  - Advance to Situation 4

**3. Social Challenge**
- Requirements: None
- Cost: 2 time blocks, 10 Resolve
- Action: "Persuade to waive toll (Social Challenge)"
- Challenge: Social, Medium (Momentum 12)
- Success Rewards:
  - Understanding +1
  - Guard bond +2
  - Advance to Situation 4
- Failure Rewards:
  - Must pay 12 coins (increased after failed persuasion)
  - Guard bond -1
  - Advance to Situation 4

**4. Fallback (Patient Wait)**
- Requirements: None
- Cost: 2 time blocks
- Action: "Wait patiently through thorough inspection"
- Rewards:
  - Advance to Situation 4

**Routing:** ContinueInScene

---

### Situation 4: Final Approach

**Placement:**
- RouteFilter: { From: "CurrentLocation", To: "PortDistrict", SegmentIndex: 3 }
- GrantsLocationAccess: N/A

**Narrative:**
Road opens to Port District. Ship masts visible. Warehouse district ahead.

**Choices:**

**1. Continue**
- Requirements: None
- Cost: None
- Action: "Head to warehouse district"
- Rewards:
  - Advance to Situation 5

**2. Brief Rest**
- Requirements: None
- Cost: 1 time block
- Action: "Catch breath before arrival"
- Rewards:
  - Restore: Stamina +5, Focus +5
  - Advance to Situation 5

**Routing:** ExitToWorld (next situation at Location, not Route)

---

### Situation 5: Delivery Completion (FINAL)

**Placement:**
- LocationFilter: { LocationTypes: ["Warehouse"], DistrictType: "Port" }
- NpcFilter: { Role: "WarehouseMaster" }
- GrantsLocationAccess: false

**Narrative:**
Warehouse master accepts package, inspects seal. "Elena sent you? Good enough." Counts out completion bonus.

**Choices (SPAWNS A4):**

**1. Complete Delivery**
- Requirements: None
- Cost: None
- Action: "Accept completion bonus and conclude business"
- Rewards:
  - Coins: +10 (completion bonus, paid to all paths)
  - WarehouseMaster bond +1
  - RouteLearn: "market_to_port" (all segments now face-up)
  - **SceneSpawnReward:** {
      Template: "a4_next_tutorial",
      LocationFilter: { DistrictType: "Port" }
    }
  - **Optional B-Story Spawns:** {
      Template: "b1_warehouse_investigation" (side content example),
      LocationFilter: { LocationTypes: ["Warehouse"], DistrictType: "Port" }
    }

**Perfect Information Display:**
- Completion bonus: +10 coins
- Total earnings: Upfront (8-17) + Completion (10) = 18-27 coins
- Route costs accumulated: (varies by route choices)
- Net profit: Total earnings - Route costs - Inn survival (15)
- Route learning shown: "Future travels on this route will show segment details"
- Projection: "A4 available at Port District. Optional investigation also available."

**Routing:** SceneComplete

---

## Tutorial Sequence Integration

### Spawning Chain

```
Game Start
  ↓
A1 spawns (AlwaysEligible)
  ↓
Player completes A1 Situation 3
  ↓
Choice reward includes SceneSpawnReward → A2 spawns at common room
  ↓
Player completes A2 Situation 3
  ↓
Choice reward includes SceneSpawnReward + Parameters → A3 spawns on route
  ↓
Player completes A3 Situation 5
  ↓
Choice reward includes SceneSpawnReward → A4 spawns at port
  ↓
∞ (continues infinitely)
```

### No Soft-Lock Architecture

**Every final situation has SceneSpawnReward in every choice:**
- A1 Situation 3: Both choices spawn A2
- A2 Situation 3: All four choices spawn A3 (with different parameters)
- A3 Situation 5: Choice spawns A4 (and optional B-story)

**Player cannot reach dead end. Always forward progress.**

### Optional Pacing

**Spawning ≠ Forcing:**
- A2 spawns → appears as scene card at common room
- Player can engage immediately OR delay days/weeks
- A2 remains active indefinitely (no expiration unless designed)
- Player controls tempo (can pursue other content first)

**Infinite Content:**
- A-story scenes spawn sequentially forever
- Player always has next A-story option available
- Cannot "run out" of A-story content
- Optional engagement (pursue when ready)

### Multiple Scene Spawning

**Single choice can spawn multiple scenes:**
```
A3 Situation 5 choice spawns:
  - A4 (mandatory next A-story)
  - B1 (optional investigation)
  - C1 (optional bathhouse service)
```

Player sees three scene cards at various locations. Chooses which to pursue first. All remain active until engaged.

---

## Parametric Spawning

### Contract Payment Flow

**A2 negotiation determines A3 payment:**

Choice 1 (Rapport 3): Parameters { ContractPayment: 25 }
Choice 2 (Insurance): Parameters { ContractPayment: 23 }
Choice 3 Success: Parameters { ContractPayment: 27 }
Choice 3 Failure: Parameters { ContractPayment: 18 }
Choice 4 (Standard): Parameters { ContractPayment: 20 }

**A3 final situation uses parameter:**
```
Reward: Coins: {ContractPayment}
```

**Result:** Payment amount determined by A2 choice, applied in A3.

### Template Reusability with Parameters

**Same A3 template used for:**
- Tutorial (specific route, varied payment)
- Ongoing deliveries (different routes, different merchants, varied payment)
- Procedural content (categorical route selection, AI-determined payment)

Parameters enable variety from single template.

---

## Entity Resolution (System 4)

### Eager Resolution at Spawn

**When scene spawns from SceneSpawnReward:**

For each SituationTemplate in scene:
1. Read LocationFilter → FindOrCreateLocation()
2. Read NpcFilter → FindOrCreateNPC()
3. Read RouteFilter → FindOrCreateRoute()
4. Assign resolved objects to Situation.Location/Npc/Route

**All situations have resolved entity references before scene becomes Active.**

### A1 Example

**Scene spawns:**
- Situation 1: LocationFilter (CommonRoom, Inn) → finds common_room → situation1.Location = commonRoomObject
- Situation 1: NpcFilter (Innkeeper, Friendly) → finds Elena → situation1.Npc = elenaObject
- Situation 2: LocationFilter (GuestRoom, Private, Inn) → NOT FOUND → generates private_room → situation2.Location = privateRoomObject
- Situation 3: LocationFilter inherits from Situation 2 → situation3.Location = privateRoomObject

**Before scene becomes Active:**
- Common room exists (found)
- Elena exists (found)
- Private room exists (generated)
- All situations have Location/NPC object references

**Player sees:**
- Common Room (accessible via primary location)
- Private Room (visible but locked - Situation 2 not active yet)

---

## Perfect Information Throughout

### A1 Projections

**Situation 1 choice display:**
- "Advances to: Private Room (currently locked)"
- Query: Situations[1].Location.Name = "Private Room"
- Derive: Situations[1].Template.GrantsLocationAccess = true → shows "(unlocks access)"

### A2 Projections

**Situation 2 route preview:**
- Shows: 4 segments, Moderate danger, 4 time blocks, 20 coins
- Query: RouteFilter parameters
- System displays known route info (if route exists) or estimates (if route generates)

**Situation 3 negotiation display:**
- All four payment outcomes visible: 18/20/23/25/27 coins
- Net profit calculations: Payment - 15 (survival) = Net
- Shows: "Advances to: Route Travel with {Payment} coins payment"

### A3 Projections

**Every situation shows:**
- Exact costs (coins, time, resources)
- Stat requirements vs player's current stats
- Challenge parameters (thresholds, resource costs)
- Success/failure both advance (no soft-lock risk)

**Situation 5 displays:**
- Total payment: {ContractPayment} from parameters
- Route costs accumulated across situations
- Net profit: Payment - Costs
- Next scene: "A4 available at Port District"

---

## Economic Pressure Demonstration

### Tight Margins

**Baseline playthrough:**
- A2: Accept 20 coins payment
- A3: Pay all tolls: 5 + 8 + 10 = 23 coins spent
- Net: 20 - 23 = -3 coins (lost money!)

**Teaches:** Must optimize OR negotiate better OR take challenges.

### Optimization Paths

**Stat-heavy:**
- A2: Negotiate to 25 coins (Rapport 3)
- A3: Authority 3, Insight 3, Rapport 3 → all free
- Net: +25 coins, 0 costs

**Challenge-heavy:**
- A2: Standard 20 coins
- A3: All challenges (if succeed) → 0 coin costs
- Net: +20 coins, but 37 resource units depleted (15 Stamina + 12 Focus + 10 Resolve)

**Trade-off visible:** Save coins, spend tactical resources OR spend coins, save tactical resources. Resource competition demonstrated.

---

## Teaching Summary

### A1 Teaches

- Multi-situation scene progression
- GrantsLocationAccess pattern (temporary spatial access)
- Service transaction flow (negotiate → use → depart)
- Four-choice pattern (stat/money/challenge/fallback)
- Perfect information projection
- Bond formation with NPCs
- Resource restoration mechanics

### A2 Teaches

- Delivery contract structure
- Route preview system
- Economic calculation (profit margins)
- Negotiation variations
- Parametric scene spawning (choice affects next scene)
- Multiple valid approaches (different costs, same outcome)

### A3 Teaches

- Route segment structure
- All three challenge types (Physical, Mental, Social)
- Challenge resource costs (Stamina, Focus, Resolve)
- Cumulative pressure (costs accumulate across situations)
- Understanding progression (cross-system advancement)
- Route learning (segments face-up after first travel)
- Risk assessment (fallback available but costly)
- Economic feedback (payment vs costs = profit)

### Complete Arc Teaches

- Core gameplay loop (lodging → find work → execute delivery → payment)
- Resource competition (coins vs stats vs tactical resources vs time)
- Specialization value (high stats consistently unlock optimal paths)
- Impossible choices (cannot afford all optimal paths simultaneously)
- No soft-locks (fallback always available, challenges always advance)
- Perfect information (calculate before committing)
- Tight economic margins (optimization matters)

---

## Implementation Checklist

### Scene Templates

**Create JSON:**
- `a1_secure_lodging.json` (3 situations)
- `a2_first_delivery.json` (3 situations)
- `a3_route_travel.json` (5 situations)

### Entity Requirements

**Authored entities (categorical matches):**
- Location: common_room (CommonRoom, Inn)
- Location: port_warehouse (Warehouse, Port)
- NPC: Elena (Innkeeper, Friendly)
- NPC: merchant_tutorial (Merchant)
- NPC: warehouse_master (WarehouseMaster, at port_warehouse)
- Route: market_to_port (4 segments, Moderate)

**Generated entities (System 4 creates):**
- Location: private_room (GuestRoom, Private, Inn) - generated during A1 spawn
- NPC: guard_checkpoint (Guard, Neutral) - generated during A3 spawn if not found

### SceneSpawnReward Structure

**Required properties:**
- Template: string (template ID)
- PlacementFilter: { LocationFilter, NpcFilter, RouteFilter }
- Parameters: dictionary (ContractPayment, etc.)

**Multiple rewards per choice:**
- A3 Situation 5 spawns A4 + B1 simultaneously
- Single choice, multiple SceneSpawnRewards in reward list

### Challenge Definitions

**Physical (A3 Situation 1):**
- Type: Physical, Difficulty: Easy
- VictoryThreshold: Breakthrough 8
- FailureThreshold: Danger 8
- SessionResource: Exertion (from Stamina)

**Mental (A3 Situation 2):**
- Type: Mental, Difficulty: Easy
- VictoryThreshold: Progress 10
- FailureThreshold: Exposure 10
- SessionResource: Attention (from Focus)

**Social (A2 Situation 3, A3 Situation 3):**
- Type: Social, Difficulty: Medium
- VictoryThreshold: Momentum 12
- FailureThreshold: Doubt 10
- SessionResource: Initiative

### Verification Tests

**Spawning chain:**
- A1 completes → A2 appears at common room
- A2 completes → A3 appears on route (with payment parameter)
- A3 completes → A4 appears at port

**No soft-locks:**
- Every final situation choice spawns scene
- Cannot reach dead end
- Fallback paths always available

**Perfect information:**
- All costs visible before selection
- All projections accurate (next situation location shown)
- Economic calculations correct (profit = payment - costs)

**Parameters flow:**
- A2 negotiation choice determines payment
- A3 receives correct payment parameter
- A3 final situation applies correct payment amount

---

**End of A1-A3 Corrected Specification**

Tutorial sequence demonstrates complete game loop with architectural correctness: direct spawning via rewards, no boolean gates, guaranteed forward progress, perfect information throughout, and tight economic margins creating impossible choices.