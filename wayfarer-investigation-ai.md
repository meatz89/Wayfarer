## High-Level Investigation Architecture

### The Core Revelation

The game is a **mechanical seed that grows through AI generation**. The authored POC content provides the starting point and template structure that AI will replicate procedurally. This fundamentally changes the design constraints.

### Investigation Lifecycle States

**1. Potential (AI-Detected)**
Game state allows investigation. AI can generate it when triggered.

**2. Discovered (Intro Available)**
Investigation exists with intro action spawned. Not yet in journal. Player sees action but doesn't know it's an investigation until completing it.

**3. Active (Intro Completed)**
Investigation revealed in journal. Goals spawn based on prerequisites. Player pursues resolution.

**4. Complete (All Goals Done)**
Investigation resolved. Rewards granted. World state updated. Moves to journal archive.

### Investigation Structure

```
Investigation
├─ Metadata
│  ├─ Title
│  ├─ Narrative Context (the premise/situation)
│  └─ Color Code (for UI grouping)
│
├─ Intro (Discovery Action)
│  ├─ Trigger Type (categorical enum)
│  ├─ Prerequisites (mechanical checks)
│  ├─ Action Text
│  ├─ Engagement Type
│  ├─ Location Spot
│  ├─ Intro Narrative (reveals investigation premise)
│  └─ Spawns First Goal(s)
│
├─ Goals (Ordered or Parallel)
│  ├─ Prerequisites
│  ├─ Action Text
│  ├─ Engagement Type
│  ├─ Location Spot
│  ├─ Outcome Narratives
│  └─ Spawned Goals (next steps)
│
├─ Completion
│  ├─ Resolution Narrative
│  ├─ Rewards (coins, XP, reputation, observations)
│  └─ World State Changes (NPC attitudes, location access, new knowledge)
│
└─ Generation Rules (for AI)
   ├─ Trigger Conditions (when to generate)
   ├─ Narrative Constraints (setting, tone, scale)
   └─ Mechanical Constraints (goal count, complexity, rewards)
```

### Five Intro Trigger Types

**1. Immediate Visibility**
Prerequisites: `at_location_spot(X)`
Spawns when: Player enters location
Use: Obvious environmental problems

**2. Environmental Observation**
Prerequisites: `location_familiarity(X) >= N`
Spawns when: Player explores location sufficiently
Use: Subtle details revealed through investigation

**3. Conversational Discovery**
Prerequisites: `has_knowledge(key)`
Spawns when: NPC conversation grants specific knowledge
Use: Rumors, mentions, information sharing

**4. Item Discovery**
Prerequisites: `has_item(X)`
Spawns when: Player acquires specific item
Use: Found objects requiring investigation

**5. Obligation-Triggered**
Prerequisites: `accepted_obligation(X)`
Spawns when: Player accepts NPC request
Use: Explicit quests driving investigation

These are categorical—the system (and later, AI) selects which trigger type fits the game state context.

### POC Tutorial Flow

**Game Start State:**
```
Location: Old Mill → Courtyard
Time: Morning, Day 1
Resources: Full health/stamina, 20 coins
Journal: Empty (no active investigations yet)

Available Action:
→ Notice the silent waterwheel [Mental]
  (This is Investigation Intro, but player doesn't know yet)
```

**Player Completes Intro:**
```
1. Click "Notice the silent waterwheel"
2. Mental challenge resolves (simplified for tutorial)
3. Intro narrative appears:
   "You notice the waterwheel stands eerily still. The mechanism 
   shows signs of deliberate damage—fresh scoring on the gears, 
   metal filings that don't match the mill's iron. Someone 
   sabotaged this, and recently."
   
4. Pop-up notification:
   "Investigation Discovered: The Waterwheel Mystery"
   
5. Journal auto-opens showing:
   The Waterwheel Mystery [1/5 completed]
   ✓ Discovered the sabotage
   Goals remaining at:
   → Courtyard (2 actions)
   → Storage Room (1 action)
   
6. First goal actions spawn:
   → Examine the gear damage closely [Mental]
   → Ask Aldric about recent visitors [Conversation]
```

This teaches:
- Actions can reveal investigations
- Investigations track progress in journal
- New goals spawn as you progress
- Cross-location investigation structure

### Authored POC Content

**Single Investigation: "The Waterwheel Mystery"**

Structure demonstrates all mechanics:
- Intro: Immediate visibility trigger
- 5 Goals: Mix of Mental/Conversation/Physical
- 3 Location Spots: Courtyard, Storage, Workshop
- 2 NPCs: Aldric (miller), Petra (apprentice)
- Multiple discovery paths within investigation
- Satisfying resolution with rewards

This authored content serves as:
1. **Tutorial** for players learning investigation flow
2. **Template** for AI to analyze and replicate
3. **Test Case** for mechanical validation

### AI Generation Hooks (Future)

**When to Generate:**
- Player completes investigation (generate related follow-up)
- Player reaches new location (generate location-appropriate mysteries)
- NPC relationship threshold (generate relationship-driven content)
- Time passes (generate time-sensitive events)
- World state changes (generate consequence investigations)

**How to Generate:**
1. Analyze game state (location, NPCs, player history, world events)
2. Select appropriate trigger type based on context
3. Generate intro action with valid prerequisites
4. Generate 3-7 goals following investigation structure
5. Ensure goals use available locations/NPCs/engagement types
6. Create rewards matching investigation complexity
7. Validate no soft-locks (all prerequisites achievable)

**Mechanical Constraints AI Must Follow:**
- All prerequisites must be achievable with current game state
- Goals must use valid engagement types (Conversation/Mental/Physical)
- NPCs referenced must exist or be generatable
- Locations referenced must exist or be generatable
- Rewards must follow economic balance rules
- Observation cards must be conversationally useful
- No OR conditions in prerequisites
- No soft-locks (always path forward)

### Knowledge System Integration

Knowledge is the connective tissue between investigations:

**Knowledge Sources:**
- Conversation outcomes
- Investigation goal outcomes
- Environmental observations
- Item inspections
- NPC testimonies

**Knowledge Uses:**
- Unlock investigation intros
- Unlock investigation goals
- Enable conversation options
- Inform AI generation (what player knows shapes what generates)

Knowledge creates investigation chains naturally:
```
Complete Investigation A 
→ Gain Knowledge X
→ Triggers Intro for Investigation B
→ Investigation B references Investigation A events
→ World feels continuous and reactive
```

### POC Implementation Scope

**Authored Content:**
- 1 Starting Location (Old Mill with 3 spots)
- 2 NPCs (Aldric, Petra)
- 1 Complete Investigation (5 goals)
- 3 Standalone Actions (rest, work, observe)
- 2 Travel Routes (to adjacent locations)

**Generated Content Stubs:**
- Location generation rules (not implemented yet)
- NPC generation rules (not implemented yet)
- Investigation generation rules (not implemented yet)
- But structure supports it from day one

**Success Criteria:**
- Player completes waterwheel investigation
- Journal tracks progress correctly
- Goals spawn appropriately
- Intro → Active → Complete flow works
- Notifications appear at right moments
- World state updates after completion
- No soft-locks encountered

This POC proves the mechanical foundation. AI generation is layered on later using the same structure, just generating content dynamically instead of loading authored data.
