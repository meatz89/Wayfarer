# Travel Obstacle System Refactor

## Overview

The travel system transforms from abstract path card selection to concrete obstacle challenges. Instead of choosing cards and paying stamina costs, players face real obstacles (creek crossings, dense forest, steep climbs) that require specific preparation, equipment, or knowledge to overcome. This creates meaningful travel where routes are puzzles to solve, not resources to spend.

---

## Current System Analysis

### What Exists Now

```
Current Travel System:
├── Routes: Connections between locations
├── Path Cards: Options for each route segment
├── Stamina Costs: Abstract resource spending
├── Weight Restrictions: Limits based on carrying capacity
├── Segment Costs: Time consumption
└── Discovery Effects: Finding items along the way
```

### Problems with Current System

1. **Abstract Mechanics**: "Pay 3 stamina" doesn't create narrative
2. **No Learning**: Failed travel just wastes resources
3. **Weight as Primary Gate**: Artificial restriction
4. **No Preparation Value**: Can't prepare for specific challenges
5. **Repetitive**: Same choices every time
6. **No Mastery**: Routes don't improve with experience

### What We Keep

- Route connections between locations
- Time segment costs
- Stamina as physical exertion measure
- Basic discovery mechanics
- Route unlocking through exploration

### What We Replace

- Path card selection → Obstacle approaches
- Weight restrictions → Equipment requirements
- Abstract costs → Concrete challenges
- Binary success → Learning through failure
- Static routes → Improvable paths

---

## New Architecture

### Core Components

#### 1. Travel Route (Connection)

```
TravelRoute
├── Id: Unique identifier
├── Name: Display name ("Creek Path to Farmstead")
├── FromLocationSpotId: Starting location Spot
├── ToLocationSpotId: Destination Spot
├── Obstacles: List of challenges in order
├── BaseSegmentCost: Minimum time if all obstacles cleared
├── DiscoveryState: What player knows about route
├── Improvements: Permanent enhancements made
├── Availability: Weather restrictions
└── AlternateRoutes: Other ways to same destination
```

**Route Types:**
- **Direct Routes**: Shortest but often most challenging
- **Safe Routes**: Longer but fewer/easier obstacles
- **Hidden Routes**: Require discovery through investigation

#### 2. Travel Obstacle (Challenge)

```
TravelObstacle
├── Id: Unique identifier
├── Name: Display name ("Widow's Creek Crossing")
├── Description: What player sees
├── Approaches: Different ways to overcome
├── DefaultDanger: Risk if unprepared
├── Improvements: Can be permanently improved?
├── WeatherEffects: How conditions affect difficulty
├── TimeEffects: How time of day matters
└── OneTime: Cleared permanently once succeeded?
```

**Obstacle Categories:**

**Natural Barriers:**
- Water crossings (creeks, rivers, marshes)
- Terrain challenges (cliffs, ravines, dense forest)
- Weather hazards (storms, fog, extreme temperatures)
- Wildlife territories (dangerous animals)

**Structural Obstacles:**
- Damaged bridges/roads
- Locked gates/checkpoints
- Collapsed passages
- Abandoned buildings to traverse

**Social Barriers:**
- Territory boundaries
- Hostile groups
- Required permissions
- Cultural restrictions

**Environmental Puzzles:**
- Navigation challenges (getting lost)
- Timing requirements (tides, schedules)
- Resource management (long distances)
- Combination challenges

#### 3. Travel Approach (Solution Method)

```
TravelApproach
├── Id: Unique identifier
├── Name: Display name ("Wade Across")
├── Description: What this approach entails
├── Requirements: What's needed to attempt
├── SuccessOutcome: Results if requirements met
├── FailureOutcome: Results if attempted unprepared
├── DiscoveryMethod: How player learns about this
├── ImprovementPotential: Can make easier?
└── AlwaysAvailable: Or must be discovered?
```

**Approach Types:**

**Direct Physical:**
- Brute force (high stamina/health requirements)
- Athletic (climbing, swimming, jumping)
- Endurance (long distances, harsh conditions)

**Equipment-Based:**
- Tools enable (rope for climbing, boat for water)
- Safety gear (warm clothes for cold, light for darkness)
- Specialized equipment (climbing gear, navigation tools)

**Knowledge-Based:**
- Known paths (NPC information, prior discovery)
- Timing (when obstacle is easiest)
- Techniques (how to safely cross)

**Social Solutions:**
- Get help (NPC assistance, hired guide)
- Permission (official passes, bribes)
- Cooperation (travel with others)

**Avoidance:**
- Find alternative (longer but safer)
- Wait for conditions (weather change, schedule)
- Turn back (admit defeat, try different route)

#### 4. Travel Requirements (Gates)

```
TravelRequirements
├── MinHealth: Physical durability needed
├── MinStamina: Energy for exertion
├── RequiredEquipment: Specific items needed
├── RequiredKnowledge: Information required
├── RequiredStats: Skill levels needed
├── RequiredCompanions: NPC assistance
├── RequiredWeather: Condition restrictions
├── RequiredTime: Time of day limits
└── RequiredItems: Consumables needed
```

**Requirement Logic:**
- Requirements are AND conditions (all must be met)
- Some approaches have minimal requirements (always attemptable)
- Requirements can be discovered through failure
- NPCs can reveal requirements in advance

#### 5. Travel Outcome (Results)

```
TravelOutcome
├── Success: Did approach work?
├── ObstaclePassed: Can continue journey?
├── HealthChange: Physical damage/recovery
├── StaminaChange: Exhaustion/rest
├── SegmentCost: Time consumed
├── ItemsConsumed: What was used up
├── ItemsGained: Discoveries made
├── KnowledgeGained: What was learned
├── RouteImproved: Permanent enhancement?
├── InjuriesIncurred: Lasting effects?
└── NarrativeDescription: What happened
```

**Outcome Types:**

**Complete Success:**
- Obstacle overcome efficiently
- Minimal resource cost
- Possible route improvement
- Knowledge gained for future

**Costly Success:**
- Obstacle overcome but expensive
- High resource consumption
- Injuries or exhaustion
- Time delays

**Failure with Learning:**
- Cannot pass but learn requirements
- Discover alternative approaches
- Gain partial progress
- Build knowledge for retry

**Catastrophic Failure:**
- Serious injury or resource loss
- Forced retreat to starting location
- Items lost or damaged
- Significant time waste

---

## Refactoring Plan

### Phase 1: Parallel Systems

Keep existing path card system while building obstacle system:

```
Route (Existing)
├── PathCards[] (current system)
└── Obstacles[] (new system, optional)

if (route.Obstacles.Any())
    UseObstacleSystem();
else
    UseLegacyPathCards();
```

### Phase 2: Content Migration

Gradually convert routes from path cards to obstacles:

**Simple Conversion:**
```
Path Card: "Forest Trail"
- Cost: 2 stamina
- Time: 1 segment
- Weight limit: 8

Becomes:

Obstacle: "Dense Forest"
- Approach 1: "Cut through undergrowth" (requires tools or high stamina)
- Approach 2: "Find game trail" (requires knowledge or Cunning 2+)
- Approach 3: "Go around" (safe but 2 extra segments)
```

**Complex Conversion:**
```
Path Cards: "Creek Crossing" sequence
- Card 1: "Approach Creek" (1 stamina)
- Card 2: "Cross Water" (3 stamina, weight < 6)
- Card 3: "Dry Off" (1 stamina)

Becomes:

Obstacle: "Widow's Creek"
- Approach 1: "Wade across" (40+ stamina, 60% fail chance)
- Approach 2: "Find shallow spot" (knowledge + time)
- Approach 3: "Use rope" (requires rope, safe)
- Approach 4: "Turn back" (admit defeat)
```

### Phase 3: Legacy Cleanup

Once all routes converted:
1. Remove path card references from routes
2. Delete path card classes
3. Remove card selection UI
4. Update save/load serialization
5. Clean up database schema

---

## Obstacle Design Patterns

### Pattern 1: Simple Gate

```
Obstacle: Locked Gate
├── Approach 1: "Pick lock" (requires lockpicks or Cunning 3+)
├── Approach 2: "Climb over" (requires stamina 30+)
├── Approach 3: "Find key" (requires specific knowledge)
└── Approach 4: "Go around" (+2 segments)
```

**Use When:** Teaching single requirement type

### Pattern 2: Risk/Reward Trade-off

```
Obstacle: Rickety Bridge
├── Approach 1: "Cross carefully" (slow but safe, 2 segments)
├── Approach 2: "Run across" (fast but 40% collapse risk)
├── Approach 3: "Reinforce first" (requires rope, then safe)
└── Approach 4: "Find another way" (+3 segments)
```

**Use When:** Creating tension between speed and safety

### Pattern 3: Escalating Challenge

```
Obstacle: Mountain Pass
├── Approach 1: "Direct climb" (requires climbing gear AND stamina 50+)
├── Approach 2: "Switchback path" (no equipment but 3 segments)
├── Approach 3: "Wait for guide" (costs coins, specific schedule)
└── Approach 4: "Discover hidden path" (requires investigation)
```

**Use When:** Rewarding preparation and knowledge

### Pattern 4: Environmental Puzzle

```
Obstacle: Tidal Marsh
├── Approach 1: "Cross at low tide" (requires timing knowledge)
├── Approach 2: "Build raft" (requires materials and time)
├── Approach 3: "Hire boat" (requires coins and NPC present)
└── Approach 4: "Long way around" (+5 segments)
```

**Use When:** Creating scheduling challenges

### Pattern 5: Social Challenge

```
Obstacle: Hostile Territory
├── Approach 1: "Sneak through" (requires Cunning 3+ or darkness)
├── Approach 2: "Negotiate passage" (requires Diplomacy 3+ or trade goods)
├── Approach 3: "Fight through" (requires Authority 4+ or weapons)
└── Approach 4: "Get escort" (requires relationship with locals)
```

**Use When:** Using social skills for travel

---

## Integration Points

### With Investigation System

Investigations can reveal:
- Hidden approaches to obstacles
- Optimal timing for challenges
- Equipment caches near obstacles
- Historical paths now overgrown
- Weaknesses in barriers

Routes can gate investigations:
- Must overcome obstacle to reach investigation site
- Investigation location only accessible via specific route

### With Knowledge System

```
Knowledge Types for Travel:

ROUTE KNOWLEDGE
├── Hidden Paths: Alternative routes
├── Shortcuts: Faster approaches
├── Safe Passages: Reduced danger
├── Timing: When obstacles are easiest
└── Techniques: How to overcome safely

OBSTACLE KNOWLEDGE
├── Weaknesses: Vulnerable points
├── Patterns: Predictable behaviors
├── History: Why obstacle exists
├── Workarounds: Unconventional solutions
└── Improvements: How to permanently fix
```

### With Equipment System

```
Travel Equipment Uses:

CROSSING EQUIPMENT
├── Rope: Climbing, securing, bridging
├── Boat: Water crossings
├── Ladder: Vertical obstacles
└── Planks: Temporary bridges

NAVIGATION EQUIPMENT
├── Map: Avoid getting lost
├── Compass: Direction finding
├── Telescope: Scout ahead
└── Markers: Mark path for return

SAFETY EQUIPMENT
├── Climbing gear: Safe ascent/descent
├── Weather gear: Protection from elements
├── Light sources: Night travel
└── Medical supplies: Treat travel injuries

SPECIAL EQUIPMENT
├── Permits: Legal passage
├── Bribes: Social passage
├── Tools: Clear obstacles
└── Gifts: Appease locals
```

### With NPC System

NPCs provide:
- Route information before travel
- Warnings about dangers
- Equipment recommendations
- Timing advice
- Alternative route suggestions
- Guide services
- Emergency rescue

NPCs affected by:
- Player route discoveries
- Obstacle improvements
- New connections opened
- Dangerous routes cleared
- Trade route establishment

---

## State Persistence

### Route Progress Tracking

```
RouteProgress
├── RouteId: Which route
├── ObstaclesCleared: List of passed obstacles
├── ApproachesUsed: History of methods
├── KnowledgeGained: What was learned
├── ImprovementsMade: Permanent changes
├── FailureCount: Times attempted
└── BestTime: Fastest completion
```

### What Persists

**Per-Route:**
- Which obstacles cleared
- Known approaches
- Improvements made
- Discovery state
- Best completion time

**Per-Obstacle:**
- Attempted approaches
- Successful methods
- Permanent changes
- Knowledge gained

**Global:**
- All route knowledge
- Equipment locations found
- NPC route information

---

## UI/UX Considerations

### Travel Planning Screen

```
ROUTE SELECTION
├── Available Routes (known)
│   ├── Distance/time estimate
│   ├── Known obstacles
│   ├── Equipment recommendations
│   └── Current conditions
├── Unknown Routes (hints only)
│   ├── "There might be another way..."
│   ├── "Ask locals about alternatives"
│   └── "Investigate to discover"
└── Route History
    ├── Previous attempts
    ├── Best times
    └── Methods used
```

### Obstacle Encounter Screen

```
OBSTACLE PRESENTATION
├── Obstacle Description
│   └── Narrative text explaining challenge
├── Available Approaches
│   ├── Always visible (basic options)
│   ├── Equipment-enabled (if equipped)
│   ├── Knowledge-enabled (if known)
│   └── Stat-enabled (if qualified)
├── Requirements Display
│   ├── Met requirements (green)
│   ├── Unmet requirements (red)
│   └── Unknown requirements (???)
└── Risk Assessment
    ├── Success probability (if applicable)
    ├── Failure consequences
    └── Resource costs
```

### Post-Travel Summary

```
JOURNEY COMPLETE
├── Route Taken
├── Obstacles Overcome
│   └── Methods used
├── Resources Consumed
│   ├── Health lost
│   ├── Stamina spent
│   ├── Items used
│   └── Time taken
├── Discoveries Made
│   ├── Knowledge gained
│   ├── Items found
│   └── Routes revealed
└── Improvements
    ├── Personal records
    └── Route enhancements
```

---

## Content Examples

### Example 1: Creek Crossing (Simple)

```json
{
  "id": "widows_creek",
  "name": "Widow's Creek Crossing",
  "description": "The creek runs fast and cold, no bridge in sight.",
  "approaches": [
    {
      "id": "wade",
      "name": "Wade across",
      "requirements": {
        "minStamina": 40
      },
      "outcome": {
        "success": false,
        "healthChange": -30,
        "staminaChange": -30,
        "narrativeDescription": "The current is too strong! You're swept downstream.",
        "knowledgeGained": ["creek_dangerous"]
      }
    },
    {
      "id": "rope_cross",
      "name": "Use rope to secure crossing",
      "requirements": {
        "requiredEquipment": ["rope"],
        "minStamina": 20
      },
      "outcome": {
        "success": true,
        "staminaChange": -20,
        "segmentCost": 1,
        "narrativeDescription": "You secure the rope and carefully cross."
      }
    },
    {
      "id": "find_shallow",
      "name": "Search for shallower spot",
      "requirements": {},
      "outcome": {
        "success": true,
        "segmentCost": 2,
        "staminaChange": -10,
        "knowledgeGained": ["creek_shallow_spot"],
        "narrativeDescription": "After searching upstream, you find a better crossing."
      }
    }
  ]
}
```

### Example 2: Forest Path (Complex)

```json
{
  "id": "dense_forest",
  "name": "Dense Forest",
  "description": "The path is overgrown, barely visible through thick undergrowth.",
  "approaches": [
    {
      "id": "cut_through",
      "name": "Cut through undergrowth",
      "requirements": {
        "requiredEquipment": ["cutting_tools"],
        "minStamina": 30
      },
      "outcome": {
        "success": true,
        "staminaChange": -30,
        "segmentCost": 2,
        "routeImproved": true,
        "narrativeDescription": "You clear a path, making future travel easier."
      }
    },
    {
      "id": "force_through",
      "name": "Force through without tools",
      "requirements": {
        "minStamina": 50,
        "minHealth": 40
      },
      "outcome": {
        "success": true,
        "healthChange": -15,
        "staminaChange": -40,
        "segmentCost": 3,
        "narrativeDescription": "You push through, getting scratched and exhausted."
      }
    },
    {
      "id": "find_game_trail",
      "name": "Search for animal trails",
      "requirements": {
        "requiredStats": {"Cunning": 3},
        "requiredKnowledge": ["forest_navigation"]
      },
      "outcome": {
        "success": true,
        "segmentCost": 1,
        "staminaChange": -10,
        "knowledgeGained": ["hidden_forest_paths"],
        "narrativeDescription": "Your woodcraft reveals a hidden trail."
      }
    }
  ]
}
```

---

## Testing Strategy

### Conversion Testing
1. Create parallel obstacle for existing route
2. Test both systems work
3. Verify same destination reached
4. Compare resource costs
5. Remove legacy path cards

### Obstacle Testing
- [ ] All approaches reachable with requirements
- [ ] Failure conditions trigger correctly
- [ ] Knowledge gained persists
- [ ] Route improvements save
- [ ] Weather/time effects apply

### Integration Testing
- [ ] Equipment enables correct approaches
- [ ] Knowledge gates work
- [ ] NPC information helps
- [ ] Investigation reveals routes
- [ ] State persistence works

### Balance Testing
- [ ] Resource costs reasonable
- [ ] Multiple valid strategies exist
- [ ] Preparation valued but not required
- [ ] Failure recoverable
- [ ] Time investment appropriate

---

## Migration Timeline

### Week 1: Foundation
- Create obstacle domain models
- Build approach evaluation system
- Implement outcome application
- Add state persistence

### Week 2: Integration
- Connect to equipment system
- Connect to knowledge system
- Build UI components
- Create testing framework

### Week 3: Content Conversion
- Convert 2-3 routes to obstacles
- Test parallel systems
- Gather feedback
- Refine approach

### Week 4: Full Migration
- Convert all routes
- Remove legacy system
- Update documentation
- Final testing

### Week 5: Polish
- Balance tuning
- UI improvements
- Bug fixes
- Performance optimization

---

## Success Metrics

### Player Experience Goals
- "Travel feels like adventure, not tax"
- "Obstacles make sense in world"
- "Preparation matters but isn't required"
- "Failure teaches without frustrating"
- "Routes become easier with mastery"

### Mechanical Goals
- 80% obstacles have multiple viable approaches
- 60% failures result in learning
- 90% equipment sees regular use
- 0% soft-lock situations
- 50% routes improvable

### Content Goals
- 15-20 unique obstacles across all routes
- 3-5 approaches per obstacle average
- 10-15 pieces of travel equipment
- 20-30 route knowledge items
- 5-10 improvable routes

---

## Conclusion

The travel obstacle system transforms abstract path card selection into concrete challenges that create narrative through mechanics. By replacing stamina costs with equipment requirements, weight limits with preparation choices, and binary success with learning through failure, travel becomes an adventure system that integrates with investigations, knowledge, and NPC relationships. The refactor can be implemented gradually through parallel systems, ensuring stability while building toward the new vision of travel as meaningful challenge rather than resource tax.