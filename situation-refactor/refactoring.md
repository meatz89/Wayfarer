# Scene Spawning: Conditions, Filters, and Triggers

## The Three-Part System

Every scene spawn requires three elements:

**SPAWN CONDITIONS** = When is this scene eligible?
**PLACEMENT FILTERS** = Where can this scene appear?
**TRIGGER EVENTS** = What causes the system to check?

---

## Part 1: Spawn Conditions (Eligibility)

**Spawn conditions determine if a template CAN spawn right now.**

### Condition Types

**Player State Conditions:**
- Has completed specific scene
- Has made specific choice  
- Has stat above threshold
- Has item in inventory
- Has visited location X times

**World State Conditions:**
- Current weather (rain, snow, clear)
- Current time (morning, evening, night)
- Day of week
- Locations discovered/undiscovered

**Entity State Conditions:**
- NPC bond level reached
- Location reputation level
- Route traveled X times
- Location property (discovered, hostile, etc.)

**Combinations:**
Multiple conditions with AND/OR logic

### Example: Rain Scene

```
SceneTemplate: "WildernessRainStorm"

SpawnConditions:
  - Weather = Rain
  - Player.TravelMode = OnFoot
  - Route.Type = Wilderness
```

This template only becomes eligible when ALL three true.

### Example: Promise Scene

```
SceneTemplate: "NPCNeedsHelp"

SpawnConditions:
  - Player.CompletedScenes contains "InitialMeeting"
  - Player.ChoiceHistory contains "PromisedToHelp"
  - NPC.BondLevel >= 2
```

Only eligible after specific prior scene, specific choice made, and bond level reached.

---

## Part 2: Placement Filters (Location Selection)

**Placement filters determine WHICH entity the scene spawns on.**

### Filter Types

**Categorical Entity Filters:**
- LocationType = Tavern
- NPCProfession = Merchant
- RouteType = Wilderness
- RouteTerrain = Mountain

**Relationship Filters:**
- NPC = [reference from prior scene]
- Location.ConnectedTo = [specific location]
- Route.Destination = [specific venue]

**Property Filters:**
- Location.HasProperty = "Discovered"
- NPC.Personality = Aggressive
- Route.Difficulty >= 3

**Combinations:**
Multiple filters narrow the set of valid entities

### Example: Rain Scene Placement

```
PlacementFilter:
  - EntityType = Route
  - RouteType = Wilderness
  - RouteTerrain contains "Forest" OR "Mountain"
```

**Matches any wilderness route with forest/mountain terrain.**

When player travels wilderness route meeting conditions, scene spawns on THAT route.

### Example: Promise Scene Placement

```
PlacementFilter:
  - EntityType = NPC
  - NPC.Id = [stored from PromiseScene context]
```

**Matches specific NPC from original promise.**

Scene spawns at THAT NPC's location.

---

## Part 3: Trigger Events (When To Check)

**Trigger events determine WHEN the system evaluates templates.**

### Trigger Types

**Location Entry Trigger:**
- Player enters location
- System evaluates all templates with LocationPlacement
- Eligible templates spawn at location

**Route Segment Trigger:**
- Player begins route segment
- System evaluates all templates with RoutePlacement
- Eligible templates spawn on route

**Action Completion Trigger:**
- Player completes situation/choice
- Choice has SpawnReward specifying template
- System evaluates that specific template
- Spawns if eligible

**NPC Interaction Trigger:**
- Player talks to NPC
- System evaluates all templates with NPCPlacement for this NPC
- Eligible templates spawn

**Time Advancement Trigger:**
- Day advances
- System evaluates all templates with time-based conditions
- Eligible templates spawn at appropriate entities

### Example: Rain Scene Trigger

**Trigger:** Route Segment Begin

```
Player starts route segment from Tavern to Merchant Quarter
  ↓
System: "Route segment starting, check route-placement templates"
  ↓
Template "WildernessRainStorm" found
  ↓
Check spawn conditions:
  - Weather = Rain? YES
  - TravelMode = OnFoot? YES  
  - RouteType = Wilderness? YES
  ↓
Check placement filter:
  - Current route is wilderness? YES
  ↓
SPAWN: Create scene on this route segment
```

### Example: Promise Scene Trigger

**Trigger:** Action Completion (with spawn reward)

```
Player completes choice "Promise to help Marcus"
  ↓
Choice has SpawnReward: SceneTemplate "NPCNeedsHelp"
  ↓
System: "Check if template eligible"
  ↓
Check spawn conditions:
  - CompletedScene "InitialMeeting"? YES
  - ChoiceHistory contains "PromisedToHelp"? YES (just made it)
  - NPC.BondLevel >= 2? YES
  ↓
Check placement filter:
  - NPC.Id = Marcus (stored from choice context)
  ↓
SPAWN: Create scene at Marcus's current location
```

---

## The Unified Flow

**Step 1: Trigger Event Occurs**
- Player does something (enter location, complete action, etc.)

**Step 2: Identify Relevant Templates**
- Based on trigger type, get subset of templates to check
- Location entry → check location-placement templates
- Route begin → check route-placement templates
- Action complete with spawn reward → check that specific template

**Step 3: Evaluate Spawn Conditions**
- For each template, check if conditions met
- Filter to only eligible templates

**Step 4: Apply Placement Filters**
- For each eligible template, find matching entities
- Location templates → which locations match filters?
- NPC templates → which NPCs match filters?
- Route templates → does current route match filters?

**Step 5: Select Specific Entity**
- If multiple matches, selection strategy:
  - Current entity (if on route, spawn here)
  - Closest entity (if at location, spawn at nearest matching)
  - Random from matches (if multiple equally valid)
  - Priority scoring (prefer certain entity types)

**Step 6: Spawn Scene**
- Create Scene instance at selected entity
- Bind scene to entity (Location/NPC/Route)

**Step 7: Placeholder Replacement**
- Scene template has placeholders: {NPC_NAME}, {LOCATION_TYPE}
- AI generation prompt includes entity properties
- AI generates narrative using actual entity details

---

## Placeholder Replacement System

**Templates have placeholder slots:**

```
SituationTemplate narrative hint:
  "Conflict with {NPC_PROFESSION} at {LOCATION_NAME} over {DISPUTED_RESOURCE}"
```

**When scene instantiates:**

Scene binds to: Marcus (Merchant) at Tavern

**AI prompt receives:**
```
Generate situation:
- Archetype: Confrontation
- NPC_NAME: Marcus
- NPC_PROFESSION: Merchant  
- LOCATION_NAME: Copper Kettle Tavern
- LOCATION_TYPE: Tavern
- DISPUTED_RESOURCE: Trade prices
```

**AI generates:**
```
"Marcus the merchant confronts you at the Copper Kettle Tavern.
'Your prices undercut my business. We need to settle this.'"
```

**Verisimilitude achieved through:**
- Real entity names used
- Real entity properties referenced
- Context makes narrative sense
- Mechanics match narrative (Merchant = Economic domain = Negotiation archetype)

---

## Domain Determination from Entity Properties

**Entity categorical properties determine scene domain:**

**LocationType → Domain:**
- Tavern → Economic + Social
- GuardBarracks → Authority + Physical
- Library → Mental + Social
- NobleDistrict → Authority + Social
- Docks → Physical + Economic

**NPCProfession → Domain:**
- Merchant → Economic + Social
- Guard → Authority + Physical
- Scholar → Mental + Social
- Noble → Authority + Social
- Criminal → Cunning + Physical

**RouteType → Domain:**
- Wilderness → Physical + Mental
- CityStreets → Social + Physical
- NobleQuarter → Authority + Social

**This ensures mechanical consistency:**
- Scenes at Tavern always test Economic/Social stats
- Scenes with Guards always test Authority/Physical stats
- Scenes on Wilderness routes always test Physical/Mental stats

**Player learns categorical patterns, not specific scenes.**

---

## Example 1: Environmental Route Scene

### Template Definition

```
SceneTemplate: "MountainStorm"

SpawnConditions:
  - Weather = Storm OR Rain
  - Route.Terrain contains "Mountain"
  - Player.HasItem "WarmCloak" = false

PlacementFilter:
  - EntityType = Route
  - RouteTerrain contains "Mountain"

TriggerEvent: RouteSegmentBegin

Domain: Physical (from RouteType)
ArchetypeSequence: [Confrontation, Crisis]
```

### Spawn Flow

**Player action:** Begin route segment (Mountain Pass)

**Trigger fires:** RouteSegmentBegin event

**System evaluates "MountainStorm":**
- Weather = Storm? YES
- Route.Terrain = Mountain? YES
- Player.HasItem "WarmCloak" = false? YES
- All conditions met: ELIGIBLE

**Placement check:**
- Current route has Mountain terrain? YES
- Spawn scene on THIS route segment

**Scene instantiation:**
- Situation 1 (Confrontation): "Storm intensifies" (Physical archetype)
- Situation 2 (Crisis): "Dangerous conditions" (Physical archetype)

**AI generation context:**
- ROUTE_NAME: Mountain Pass
- TERRAIN: Mountain
- WEATHER: Storm
- PLAYER_EQUIPMENT: [no warm cloak]

**AI generates Situation 1:**
```
"Icy wind cuts through your clothes as you climb the Mountain Pass.
The storm intensifies. You're dangerously exposed."

Choices:
- [LOCKED] Power through with endurance (Physical 3+)
- Purchase shelter from hermit (10 coins)
- Risk hypothermia to save resources (Physical challenge)
- Turn back (lose 1 time segment)
```

### Why This Works

**Conditions ensure appropriate timing:** Only spawns in storm on mountain without protection

**Filters ensure appropriate placement:** Only on mountain routes

**Domain ensures appropriate mechanics:** Physical domain = Physical stats tested

**Verisimilitude:** Storm on mountain requiring Physical endurance makes narrative sense

**Learnable:** Player learns "Mountain routes test Physical stats, prepare accordingly"

---

## Example 2: Consequence-Triggered NPC Scene

### Template Definition

```
SceneTemplate: "MarcusInTrouble"

SpawnConditions:
  - Player.CompletedScenes contains "MarcusFirstMeeting"
  - Player.ChoiceHistory contains "PromisedToHelpMarcus"
  - DaysSince("PromisedToHelpMarcus") >= 2

PlacementFilter:
  - EntityType = NPC
  - NPC.Id = [reference: OriginalPromiseNPC]
  - NPC.CurrentLocation = [any valid location]

TriggerEvent: LocationEntry (check whenever player enters location with Marcus)

Domain: Social (from NPC.Profession = Merchant)
ArchetypeSequence: [Social, Negotiation, Crisis]
```

### Spawn Flow

**Setup:** Player promised to help Marcus 3 days ago in scene "MarcusFirstMeeting"

**Player action:** Enter Merchant Quarter (Marcus's current location)

**Trigger fires:** LocationEntry event

**System evaluates "MarcusInTrouble":**
- CompletedScenes contains "MarcusFirstMeeting"? YES
- ChoiceHistory contains "PromisedToHelpMarcus"? YES
- DaysSince >= 2? YES (3 days passed)
- All conditions met: ELIGIBLE

**Placement check:**
- EntityType = NPC matching Marcus? YES
- Marcus at Merchant Quarter? YES
- Spawn scene at Marcus

**Scene instantiation:**
- Situation 1 (Social): "Marcus in distress"
- Situation 2 (Negotiation): "Find solution"
- Situation 3 (Crisis): "Resolve situation"

**AI generation context:**
- NPC_NAME: Marcus
- NPC_PROFESSION: Merchant
- LOCATION_NAME: Merchant Quarter
- PROMISE_CONTEXT: Player promised to help
- DAYS_PASSED: 3

**AI generates Situation 1:**
```
"Marcus pulls you aside urgently at the Merchant Quarter.
'Thank the gods you're here. Remember you promised to help?
The Guild is moving against me tomorrow.'"

Choices:
- [LOCKED] Leverage your reputation (Rapport 3+)
- Hire protection for Marcus (20 coins)
- Challenge the Guild directly (Social challenge)
- Break your promise (Marcus bond lost permanently)
```

### Why This Works

**Conditions ensure narrative continuity:** Only spawns after promise made and time passed

**Filters ensure correct NPC:** Spawns at specific NPC from promise

**Domain ensures appropriate mechanics:** Merchant = Social domain = Social stats tested

**Verisimilitude:** Helping merchant friend requires social maneuvering, makes sense

**Learnable:** Player learns "Promises create future obligations, NPCs remember"

---

## Selection Strategy When Multiple Matches

**Sometimes multiple entities match placement filter:**

### Strategy 1: Priority Scoring

```
PlacementFilter matches 3 NPCs:
- Marcus (Merchant, bond 3)
- Elena (Merchant, bond 1)  
- Thomas (Merchant, bond 0)

Selection priority:
1. Highest bond level
2. Most recent interaction
3. Closest to player location

Result: Spawn at Marcus (bond 3)
```

### Strategy 2: Contextual Relevance

```
PlacementFilter matches 2 routes:
- Forest Path (recently traveled)
- Mountain Pass (never traveled)

Selection priority:
1. Currently traveling (if on route)
2. Never traveled (discovery priority)
3. Least recently traveled

Result: Spawn at Mountain Pass (discovery)
```

### Strategy 3: Story Coherence

```
PlacementFilter matches 2 locations:
- Tavern (where promise made)
- Market (unrelated)

Selection priority:
1. Location referenced in spawn conditions
2. Location of highest activity
3. Random

Result: Spawn at Tavern (narrative coherence)
```

---

## The Unified Concept

**Every scene spawn follows same pattern:**

1. **Trigger Event** → Something happens (player action, time passage, etc.)
2. **Condition Check** → Template eligible right now? (spawn conditions)
3. **Entity Match** → Which entities valid? (placement filters)
4. **Entity Select** → Pick specific entity (selection strategy)
5. **Scene Create** → Instantiate scene at entity
6. **Context Fill** → Replace placeholders with entity properties
7. **AI Generate** → Create narrative around mechanical skeleton

**This works for all scene types:**
- Location scenes
- NPC scenes
- Route scenes
- Consequence scenes
- Time-triggered scenes
- State-triggered scenes

**Same system. Different triggers, conditions, filters.**

**Verisimilitude emerges from:**
- Domain matching entity type (Merchant → Economic)
- Archetype matching situation (Storm → Physical)
- Conditions ensuring appropriate timing
- Filters ensuring appropriate placement
- AI generation using real entity properties

**This is fully procedural scene generation with mechanical consistency and narrative coherence.**