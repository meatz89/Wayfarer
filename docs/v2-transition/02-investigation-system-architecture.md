# Investigation System Architecture

## Core Concept

Investigations are multi-phase mysteries that players actively explore through choices, preparation, and careful decision-making. Unlike passive familiarity grinding or simple skill checks, investigations require gathering information, acquiring equipment, managing resources, and learning from failure. State persistence allows players to retreat when unprepared and return when ready, creating a satisfying loop of attempt → learn → prepare → succeed.

---

## System Components

### 1. Investigation Mystery (Container)

The top-level container for an entire investigation experience.

```
InvestigationMystery
├── Id: Unique identifier (e.g., "mill_mystery")
├── Name: Display name ("The Abandoned Mill")
├── LocationId: Where investigation occurs ("old_mill")
├── InitialPhaseId: Starting phase ("approach")
├── Phases: List of all phases in order
├── Prerequisites: Optional requirements to unlock
└── Rewards: What player gains on completion
```

**Design Principles:**
- Each mystery exists at specific location
- Mysteries unlock through prerequisites (knowledge, relationships, prior investigations)
- Completion provides meaningful rewards (discoveries, items, reputation)
- State persists across play sessions

### 2. Investigation Phase (Stage)

Individual stages within an investigation, each presenting unique challenges.

```
InvestigationPhase
├── Id: Phase identifier ("interior")
├── Name: Display name ("Mill Interior")
├── Description: Narrative setup for player
├── Choices: Available approaches (2-4 typically)
├── AmbientDanger: Optional passive hazards
├── TimeOfDayModifiers: How time affects phase
└── WeatherModifiers: How weather affects phase
```

**Phase Types:**
- **Entry**: How to access location (front door, window, hidden entrance)
- **Exploration**: Initial investigation (examine area, search for clues)
- **Puzzle**: Specific challenge (mechanism, locked door, coded message)
- **Discovery**: Finding key evidence or items
- **Resolution**: Final phase (confrontation, escape, revelation)

**Design Principles:**
- Each phase offers 2-4 meaningful choices
- Phases can be completed in different ways
- Some phases optional but rewarding
- Environmental factors affect difficulty

### 3. Investigation Choice (Approach)

Specific actions player can attempt within a phase.

```
InvestigationChoice
├── Id: Choice identifier ("examine_mechanism")
├── Text: Display text ("Examine the grinding mechanism")
├── Requirements: What's needed to attempt
├── SuccessOutcome: Results if successful
├── FailureOutcome: Results if requirements not met
├── PartialOutcome: Optional middle ground
└── OneTime: Can only be attempted once?
```

**Choice Categories:**
- **Direct**: Straightforward approach (force door, climb wall)
- **Clever**: Indirect method (find alternative, use environment)
- **Cautious**: Safe but slow (observe first, prepare extensively)
- **Social**: Involve others (get help, create distraction)
- **Equipment**: Use specific tool (rope for climbing, lantern for darkness)

### 4. Investigation Requirements

Gates that determine if player can attempt specific choice.

```
InvestigationRequirements
├── MinHealth: Minimum health needed
├── MinStamina: Minimum stamina needed
├── RequiredEquipment: List of equipment ids
├── RequiredKnowledge: List of knowledge ids
├── RequiredStats: Dictionary of stat requirements
├── RequiredTokens: Minimum NPC relationships
├── TimeRestriction: Only certain time blocks
└── WeatherRestriction: Only certain weather
```

**Requirement Types:**

**Resource Requirements:**
- Health: Physical durability for dangerous actions
- Stamina: Energy for sustained efforts
- Coins: Bribe guards, purchase access

**Equipment Requirements:**
- Specific items enable specific actions
- Equipment not consumed (except consumables)
- Multiple equipment can enable same action

**Knowledge Requirements:**
- Information from NPCs or prior investigations
- Creates information economy
- Encourages thorough exploration

**Stat Requirements:**
- Higher stats unlock sophisticated approaches
- Not just modifiers but different options
- Represents character competency

**Social Requirements:**
- Token thresholds with specific NPCs
- Reputation level
- Prior quest completion

### 5. Investigation Outcome

Results of attempting a choice.

```
InvestigationOutcome
├── Success: Did approach work?
├── HealthChange: Damage or healing
├── StaminaChange: Exhaustion or rest
├── SegmentCost: Time consumed
├── KnowledgeGained: Information learned
├── ItemsGained: Physical discoveries
├── ObservationCardsCreated: For NPC decks
├── NextPhaseId: Progress to next phase
├── StateChanges: Permanent world changes
├── DangerRoll: Probabilistic danger
└── NarrativeText: What happened
```

**Outcome Types:**

**Success Outcomes:**
- Progress to next phase
- Gain valuable knowledge
- Discover items or equipment
- Create observation cards
- Unlock new routes or locations

**Failure Outcomes:**
- Take damage but learn requirements
- Waste time but gain knowledge
- Alert enemies creating urgency
- Block approach permanently
- Force retreat but save progress

**Partial Outcomes:**
- Some success with complications
- Progress but take damage
- Learn partial information
- Create future difficulty

### 6. Investigation State (Persistence)

Tracks player's progress through investigation.

```
InvestigationState
├── MysteryId: Which investigation
├── CurrentPhaseId: Where player is
├── CompletedPhases: List of finished phases
├── ChoicesMade: History of attempts
├── DiscoveredKnowledge: Information found
├── PermanentChanges: World state alterations
├── AttemptCount: How many tries
└── CustomData: Mystery-specific tracking
```

**Persistence Principles:**
- All progress saves automatically
- Can leave and return anytime
- Failed attempts recorded
- Knowledge accumulates
- World changes permanent

---

## Investigation Flow

### Standard Flow Pattern

```
1. DISCOVERY
   Player discovers investigation exists
   - NPC mentions location
   - Obligation leads there
   - Exploration reveals it
   ↓
2. PREPARATION
   Gather requirements for attempt
   - Talk to NPCs for information
   - Acquire needed equipment
   - Build stats/relationships
   - Wait for right conditions
   ↓
3. INITIAL ATTEMPT
   Try first approach
   - Often fails without preparation
   - Teaches what's needed
   - Partial progress saves
   ↓
4. LEARNING
   Understand requirements
   - Failed approach reveals needs
   - NPCs provide hints
   - Knowledge accumulates
   ↓
5. PREPARATION REFINEMENT
   Acquire specific needs
   - Buy/find equipment
   - Get specific knowledge
   - Build relationships
   ↓
6. SUCCESSFUL ATTEMPT
   Complete investigation
   - Use right approach
   - Multiple phases cleared
   - Major discovery made
   ↓
7. CONSEQUENCES
   Discoveries affect world
   - NPCs react to findings
   - New investigations unlock
   - Reputation changes
   - World state updates
```

### Multi-Phase Progression

Investigations typically have 3-5 phases:

**Phase 1: Approach**
How do you access the location?
- Direct entry (force/pick lock)
- Alternative entrance (window/roof)
- Social engineering (disguise/distraction)
- Wait for opportunity (schedule/event)

**Phase 2: Initial Exploration**
What do you investigate first?
- Systematic search (thorough but slow)
- Targeted examination (need knowledge)
- Quick scan (fast but miss details)
- Follow clues (requires prior info)

**Phase 3: Deep Investigation**
Pursue specific leads
- Solve puzzle/mechanism
- Access restricted area
- Decode information
- Confront danger

**Phase 4: Critical Discovery**
Find key evidence/item
- Requires preparation
- Often dangerous
- Major revelation
- Changes everything

**Phase 5: Resolution**
Deal with consequences
- Escape with discovery
- Confront antagonist
- Make moral choice
- Trigger world change

---

## Danger System Integration

### Danger Types in Investigations

**Physical Dangers:**
```
StructuralCollapse
├── Trigger: Careless actions, wrong approach
├── Effect: 20-40 health damage
├── Avoidance: Proper equipment, careful approach
└── Learning: Structural weakness visible

Environmental Hazard
├── Trigger: Weather, time of day
├── Effect: 10-20 health, stamina drain
├── Avoidance: Proper clothing, timing
└── Learning: Environmental patterns

Trap or Mechanism
├── Trigger: Triggered by exploration
├── Effect: 15-30 damage, possible retreat
├── Avoidance: Knowledge, careful observation
└── Learning: Trap indicators
```

**Social Dangers:**
```
Discovery by Hostile
├── Trigger: Noisy approach, daylight entry
├── Effect: Confrontation, reputation loss
├── Avoidance: Stealth, timing, disguise
└── Learning: Patrol patterns, schedules

Trespassing Consequences
├── Trigger: Caught in restricted area
├── Effect: Legal troubles, fines, enemies
├── Avoidance: Permission, stealth, bribes
└── Learning: Legal boundaries
```

**Information Dangers:**
```
Dangerous Knowledge
├── Trigger: Discover sensitive information
├── Effect: Become target, make enemies
├── Avoidance: Cannot avoid if investigating
└── Learning: Who wants this hidden

False Information
├── Trigger: Misinterpret clues
├── Effect: Wrong conclusions, wasted time
├── Avoidance: Multiple sources, verification
└── Learning: Check multiple angles
```

### Danger Probability System

```csharp
DangerOutcome
├── BaseProbability: 0.0 to 1.0
├── Modifiers:
│   ├── Equipment: -0.1 to -0.3 (reduces risk)
│   ├── Knowledge: -0.1 to -0.2 (know dangers)
│   ├── Stats: -0.05 per level (competency)
│   ├── Conditions: +0.1 to +0.3 (bad weather/time)
│   └── Injuries: +0.1 to +0.2 (already hurt)
├── MinimumRisk: 0.05 (always some danger)
└── MaximumRisk: 0.95 (never guaranteed failure)
```

---

## Knowledge System Integration

### Knowledge Flow in Investigations

```
SOURCES → KNOWLEDGE → APPLICATIONS

Sources:
├── NPC Conversations
│   └── "Martha tells you about the mill's dangerous mechanism"
├── Investigation Discoveries
│   └── "You notice the upper floor boards are rotting"
├── Failed Attempts
│   └── "The creek is too strong without proper preparation"
├── Observation
│   └── "You see fresh footprints in the dust"
└── Documents
    └── "The ledger reveals the elder's involvement"

Knowledge Types:
├── Location Knowledge
│   ├── Entry points ("mill_side_entrance")
│   ├── Dangers ("upper_floor_unsafe")
│   └── Opportunities ("hidden_compartment")
├── Method Knowledge
│   ├── Approaches ("rope_stabilization")
│   ├── Timing ("guard_patrol_schedule")
│   └── Requirements ("needs_50_stamina")
├── Social Knowledge
│   ├── Relationships ("elder_criminal")
│   ├── Motivations ("martha_seeks_closure")
│   └── Secrets ("daughter_murdered")
└── Historical Knowledge
    ├── Events ("smuggling_operation")
    ├── Connections ("mill_was_smuggling_hub")
    └── Evidence ("ledgers_prove_guilt")

Applications:
├── Unlock Choices
│   └── Knowledge enables new investigation approaches
├── Reduce Risk
│   └── Know dangers, prepare properly
├── Skip Phases
│   └── Knowledge of secret entrance skips puzzle
├── Conversation Advantages
│   └── Confront NPCs with evidence
└── Investigation Chains
    └── One investigation reveals another
```

### Knowledge Persistence

All knowledge persists permanently once gained:
- Never lost on failure
- Carries between attempts
- Available for all future investigations
- Can be shared with NPCs
- Creates player progression beyond stats

---

## Equipment System Integration

### Equipment Roles in Investigations

```
Equipment Categories:

ACCESS TOOLS
├── Rope: Climbing, securing, crossing
├── Lockpicks: Locked doors, chests
├── Prybar: Forcing entries, moving debris
└── Keys: Specific locks

VISION AIDS
├── Lantern: Dark spaces, examining details
├── Spyglass: Distant observation
├── Magnifier: Close examination
└── Mirror: Around corners

SAFETY GEAR
├── Gloves: Handling dangerous items
├── Boots: Difficult terrain
├── Cloak: Weather protection
└── Mask: Dust, identification

SPECIALIZED TOOLS
├── Notebook: Recording clues
├── Measuring: Precise work
├── Chemical: Testing substances
└── Medical: Treating injuries
```

### Equipment Enabling System

Equipment doesn't provide bonuses - it enables actions:

```
WITHOUT Equipment:
- Cannot attempt certain choices
- Higher risk on dangerous choices
- Missing information opportunities
- Limited approach options

WITH Equipment:
- New choices available
- Reduced danger probability
- Better information gathering
- Multiple approach options
```

Example:
```
Mill Interior Investigation:
├── Without Lantern:
│   ├── "Fumble in darkness" (high injury risk)
│   └── "Leave immediately" (no progress)
├── With Lantern:
│   ├── "Examine mechanism carefully" (safe, informative)
│   ├── "Search for clues" (find evidence)
│   └── "Read documents" (gain knowledge)
```

---

## State Persistence Architecture

### What Persists

```
Per Investigation:
├── Current Phase: Exactly where player stopped
├── Completed Phases: All finished sections
├── Choices Made: Complete history
│   ├── Which approaches tried
│   ├── Which succeeded/failed
│   └── What was learned
├── Knowledge Gained: All discovered information
├── World Changes: Permanent alterations
│   ├── Doors opened/broken
│   ├── Mechanisms activated
│   ├── Items removed
│   └── NPCs alerted
└── Time Investment: How long spent investigating
```

### Save/Load Behavior

**On Investigation Exit:**
1. Current phase saved
2. All progress recorded
3. Knowledge added to player
4. World state updated
5. Can exit anytime safely

**On Investigation Return:**
1. Load saved phase
2. Restore world changes
3. Show investigation state
4. Continue from exact position
5. Previous choices remembered

### Replay Considerations

**First Attempt:**
- All choices available (gated by requirements)
- No prior knowledge assumed
- Full discovery experience
- Maximum danger risk

**Subsequent Attempts:**
- Knowledge from failures retained
- Some choices may be exhausted
- World changes permanent
- Reduced danger (know risks)
- Can try different approaches

---

## Content Creation Guide

### Investigation Complexity Levels

**Simple Investigation (2-3 phases, 6-8 choices)**
- Tutorial complexity
- Single location
- Clear requirements
- Low danger
- 2-3 hours to author
- Example: Finding lost item

**Standard Investigation (3-4 phases, 10-15 choices)**
- Normal complexity
- Multiple areas
- Moderate requirements
- Some danger
- 5-8 hours to author
- Example: Exploring abandoned building

**Complex Investigation (4-5 phases, 15-20 choices)**
- High complexity
- Multiple locations
- Complex requirements
- Significant danger
- 15-20 hours to author
- Example: Miller's Daughter

**Epic Investigation (5+ phases, 20+ choices)**
- Maximum complexity
- Multiple locations/NPCs
- Extensive requirements
- Major dangers
- 25-30 hours to author
- Example: Courthouse conspiracy

### Phase Design Patterns

**Pattern 1: Linear Progression**
```
Phase A → Phase B → Phase C → Complete
- Simple, clear progression
- Each phase enables next
- Good for tutorials
```

**Pattern 2: Branching Paths**
```
        → Phase B →
Phase A            → Phase D → Complete
        → Phase C →
- Multiple valid approaches
- Player choice matters
- Increases replayability
```

**Pattern 3: Hub and Spokes**
```
        Phase B
           ↑
Phase A → Hub → Phase D
           ↓
        Phase C
- Central area with sub-investigations
- Can complete in any order
- Good for complex locations
```

**Pattern 4: Recursive Deepening**
```
Phase A → Phase B → Phase C
   ↑          ↓
   ←  (retry with knowledge)
- Failure teaches requirements
- Return with preparation
- Natural learning loop
```
## Testing Checklist

### Phase Progression Testing
- [ ] Can enter investigation from location
- [ ] Can progress through phases sequentially
- [ ] Can skip optional phases
- [ ] Can complete investigation
- [ ] Completion rewards granted correctly

### Requirements Testing
- [ ] Health requirements block when insufficient
- [ ] Stamina requirements block when insufficient
- [ ] Equipment requirements checked correctly
- [ ] Knowledge requirements checked correctly
- [ ] Stat requirements checked correctly
- [ ] Multiple requirements work together

### State Persistence Testing
- [ ] Can exit investigation at any phase
- [ ] Progress saves correctly
- [ ] Can return and continue
- [ ] Knowledge persists
- [ ] World changes persist
- [ ] Cannot repeat one-time choices

### Danger Testing
- [ ] Probabilistic dangers roll correctly
- [ ] Deterministic dangers always trigger
- [ ] Danger modifiers apply
- [ ] Health loss applied correctly
- [ ] Can die from dangers (health reaches 0)

### Integration Testing
- [ ] Knowledge from NPCs enables choices
- [ ] Equipment from shops enables choices
- [ ] Discoveries create observation cards
- [ ] Completion affects NPC relationships
- [ ] Unlocks new investigations
- [ ] World state changes correctly

---

## Performance Considerations

### Memory Management
- Load only active investigation
- Unload completed phase data
- Cache frequently accessed knowledge
- Minimize state tracking overhead

### Save File Size
- Compress investigation states
- Store only deltas from default
- Limit choice history length
- Periodic cleanup of old data

### UI Responsiveness
- Async loading of phase data
- Preload next likely phase
- Stream narrative text
- Cache requirement checks

---

## Future Enhancements

### Planned Features
1. **Partial Success States**: Gradient outcomes instead of binary
2. **Cooperative Investigations**: Require NPC assistance
3. **Timed Investigations**: Urgency from external pressure
4. **Branching Narratives**: Choices affect story direction
5. **Investigation Mastery**: Bonuses for repeated completions
6. **Procedural Investigations**: AI-generated content

---

## Conclusion

The investigation system transforms Wayfarer from passive delivery to active exploration. Through multi-phase mysteries with meaningful choices, requirement-based gating, state persistence, and world-affecting discoveries, investigations create emergent narratives where player preparation and decision-making matter. The system integrates seamlessly with conversations (for information gathering), equipment (for enabling approaches), and travel (for reaching locations), creating a cohesive gameplay loop that rewards curiosity, planning, and persistence.