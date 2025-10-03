# Modular Investigation Content System

## Executive Summary

The Modular Content System solves Wayfarer's content velocity crisis by enabling rapid creation of investigations through combinatorial assembly. Instead of crafting each investigation from scratch (20-30 hours), creators select and combine pre-built components (2-5 hours), generating thousands of unique experiences from a small component library.

---

## The Content Velocity Problem

### Current Reality
- Bespoke investigation: 30 hours to create, 45 minutes to play
- Creation-to-play ratio: 40:1
- 20 investigations = 600 hours of development
- Unsustainable for small team or single developer

### Modular Solution
- Component selection: 2 hours to assemble, 45 minutes to play
- Creation-to-play ratio: 3:1
- 20 investigations = 40 hours of development
- 93% reduction in content creation time

---

## Component Architecture

### Layer 1: Base Patterns (5 types)

#### 1. Lost & Found
**Structure**: Search → Clues → Deduction → Discovery
**Phases**:
- Initial Search (3-4 approaches)
- Following Clues (2-3 paths)
- Final Deduction (revelation or dead end)

**Example Flow**:
```
Player searches for missing item/person
    ↓
Discovers clues pointing to locations
    ↓
Follows trail through obstacles
    ↓
Confronts truth about disappearance
```

#### 2. Hidden Truth
**Structure**: Rumor → Investigation → Evidence → Confrontation
**Phases**:
- Hearing Rumors (multiple sources)
- Gathering Evidence (3-4 pieces needed)
- Confrontation (accusation or coverup)

**Example Flow**:
```
NPCs hint at secret/conspiracy
    ↓
Player gathers evidence from multiple sources
    ↓
Builds case against suspect
    ↓
Confronts with proof or gets silenced
```

#### 3. Structural Danger
**Structure**: Discovery → Exploration → Crisis → Resolution
**Phases**:
- Discovering Danger (warning signs)
- Exploring Extent (how bad is it?)
- Crisis Point (immediate action required)
- Resolution (fix or evacuate)

**Example Flow**:
```
Find dangerous structure/situation
    ↓
Assess severity and scope
    ↓
Crisis forces immediate decision
    ↓
Save or abandon based on preparation
```

#### 4. Social Conflict
**Structure**: Dispute → Sides → Mediation → Resolution
**Phases**:
- Learning of Dispute
- Understanding Both Sides
- Mediation Attempts
- Forced Resolution

**Example Flow**:
```
Two parties in conflict seek help
    ↓
Learn each side's position
    ↓
Attempt diplomatic solution
    ↓
Choose side or forge compromise
```

#### 5. Mystery Object
**Structure**: Discovery → Analysis → Purpose → Decision
**Phases**:
- Finding Object
- Analyzing Properties
- Understanding Purpose
- Deciding Fate

**Example Flow**:
```
Discover unusual object/artifact
    ↓
Research and experiment to understand
    ↓
Learn true purpose and power
    ↓
Keep, destroy, or deliver
```

### Layer 2: Location Templates (10 types)

#### 1. Abandoned Building
**Features**: Multiple floors, hidden rooms, structural dangers
**Obstacles**: Locked doors, collapsed sections, darkness
**Discoveries**: Documents, personal effects, hidden caches

#### 2. Cave System
**Features**: Multiple chambers, water features, verticality
**Obstacles**: Darkness, narrow passages, flooding
**Discoveries**: Minerals, bones, ancient markings

#### 3. Dense Forest
**Features**: Trails, clearings, streams, hidden groves
**Obstacles**: Getting lost, wildlife, thorns
**Discoveries**: Plants, tracks, hidden structures

#### 4. Riverside/Lake
**Features**: Shores, depths, islands, currents
**Obstacles**: Water crossing, swimming, currents
**Discoveries**: Washed-up items, underwater secrets

#### 5. Mountain Path
**Features**: Cliffs, valleys, peaks, caves
**Obstacles**: Climbing, weather, avalanches
**Discoveries**: Views revealing secrets, hidden valleys

#### 6. Underground Tunnel
**Features**: Networks, chambers, vertical shafts
**Obstacles**: Navigation, cave-ins, flooding
**Discoveries**: Infrastructure, smuggling routes

#### 7. Ruined Settlement
**Features**: Buildings, streets, central plaza
**Obstacles**: Rubble, unstable structures, overgrowth
**Discoveries**: History, survivors, resources

#### 8. Swamp/Marsh
**Features**: Waterways, islands, quicksand
**Obstacles**: Navigation, sinking, disease
**Discoveries**: Preserved items, hidden paths

#### 9. Ancient Monument
**Features**: Structures, carvings, chambers
**Obstacles**: Puzzles, traps, degradation
**Discoveries**: History, artifacts, knowledge

#### 10. Occupied Building
**Features**: Residents, private areas, public spaces
**Obstacles**: Social barriers, locked areas, witnesses
**Discoveries**: Secrets, documents, overheard information

### Layer 3: Complications (10 types)

#### 1. Rival Investigator
Someone else seeks the same goal, creating competition and time pressure.

#### 2. Weather Crisis
Storm, flood, or other weather creates additional dangers and time limits.

#### 3. Hostile Wildlife
Dangerous animals complicate exploration and add combat/avoidance.

#### 4. Local Opposition
Residents don't want investigation, create social obstacles.

#### 5. Time Pressure
External deadline forces quick, risky decisions.

#### 6. Resource Scarcity
Limited supplies make each attempt costly.

#### 7. Misinformation
False clues and lies misdirect investigation.

#### 8. Trap/Ambush
Someone has prepared defenses against investigation.

#### 9. Witness Protection
Key information source needs help before talking.

#### 10. Jurisdiction Dispute
Authorities want to take over or shut down investigation.

### Layer 4: Revelations (10 types)

#### 1. Criminal Conspiracy
Uncovering illegal activity affects multiple NPCs.

#### 2. Lost History
Discovery rewrites understanding of past events.

#### 3. Hidden Relationship
Secret connection between NPCs changes everything.

#### 4. Treasure Cache
Valuable discovery creates new opportunities/problems.

#### 5. Supernatural Truth
World is stranger than expected, opens new mysteries.

#### 6. Betrayal Exposed
Trusted NPC revealed as traitor/spy.

#### 7. Innocent Accused
Wrong person blamed, real culprit hidden.

#### 8. Noble Sacrifice
Someone's death/loss saved others.

#### 9. Power Struggle
Discovery shifts political/economic balance.

#### 10. Ancient Warning
Discovery reveals impending danger to region.

---

## Assembly Process

### Step 1: Select Base Pattern
Choose one of five patterns based on desired narrative arc.

**Example**: Hidden Truth pattern for corruption investigation

### Step 2: Choose Location
Select location that fits the narrative and provides appropriate challenges.

**Example**: Occupied Building (town hall) for investigating mayor

### Step 3: Add Complication
Select complication to increase difficulty and interest.

**Example**: Rival Investigator (journalist) competing for story

### Step 4: Determine Revelation
Choose what truth the investigation reveals.

**Example**: Criminal Conspiracy (mayor embezzling with merchant guild)

### Step 5: Customize Details
Add specific names, NPCs, and local flavor.

**Example**: Mayor Aldrich, Guild Master Reeves, journalist Sara Chen

### Total Time: 2-5 hours

---

## Component Interactions

### Synergy Examples

**Lost & Found + Cave + Weather Crisis + Noble Sacrifice**
= Missing person trapped by storm flood in caves, sacrificed themselves to save others

**Hidden Truth + Occupied Building + Local Opposition + Betrayal**
= Town hall investigation opposed by citizens, reveals beloved leader is traitor

**Structural Danger + Mountain Path + Time Pressure + Ancient Warning**
= Collapsing mountain pass must be traversed quickly, reveals prophecy

**Social Conflict + Forest + Hostile Wildlife + Hidden Relationship**
= Mediate hunter vs druid dispute while avoiding wolves, discover they're siblings

**Mystery Object + Monument + Trap + Power Struggle**
= Ancient artifact in booby-trapped temple shifts regional power balance

### Anti-Patterns to Avoid

**Don't Combine**:
- Weather Crisis + Underground Tunnel (weather doesn't affect underground)
- Rival Investigator + Hostile Wildlife (animals don't compete intellectually)
- Jurisdiction Dispute + Abandoned Building (no authority in abandoned places)

---

## Content Generation Tools

### Investigation Template (JSON)

```json
{
  "id": "modular_investigation_001",
  "name": "[LOCATION] [PATTERN]",
  "description": "[CUSTOMIZED DESCRIPTION]",
  "pattern": "[PATTERN_TYPE]",
  "location": "[LOCATION_TYPE]",
  "complication": "[COMPLICATION_TYPE]",
  "revelation": "[REVELATION_TYPE]",
  "phases": [
    {
      "id": "phase_1",
      "template": "[PATTERN]_PHASE_1",
      "locationFeatures": ["[LOCATION_FEATURE_1]", "[LOCATION_FEATURE_2]"],
      "complicationActive": false,
      "choices": [...]
    },
    {
      "id": "phase_2",
      "template": "[PATTERN]_PHASE_2",
      "locationFeatures": ["[LOCATION_FEATURE_3]"],
      "complicationActive": true,
      "choices": [...]
    },
    {
      "id": "phase_3",
      "template": "[PATTERN]_PHASE_3",
      "revelationType": "[REVELATION_TYPE]",
      "choices": [...]
    }
  ]
}
```

### Quick Assembly Worksheet

```
INVESTIGATION NAME: _______________________

BASE PATTERN: □ Lost&Found □ Hidden Truth □ Structural □ Social □ Mystery
LOCATION: □ Building □ Cave □ Forest □ River □ Mountain □ Tunnel □ Ruins □ Swamp □ Monument □ Occupied
COMPLICATION: □ Rival □ Weather □ Wildlife □ Opposition □ Time □ Scarcity □ Misinfo □ Trap □ Witness □ Dispute
REVELATION: □ Crime □ History □ Relationship □ Treasure □ Supernatural □ Betrayal □ Innocent □ Sacrifice □ Power □ Warning

PHASE 1 FOCUS: _______________________
PHASE 2 FOCUS: _______________________
PHASE 3 FOCUS: _______________________

KEY NPCS:
1. _____________ (role: _________)
2. _____________ (role: _________)
3. _____________ (role: _________)

REQUIRED KNOWLEDGE:
- From NPC: _______________: "_______________"
- From Location: _______________: "_______________"
- From Investigation: _______________: "_______________"

REQUIRED EQUIPMENT:
- For Approach A: _______________
- For Approach B: _______________
- For Approach C: _______________

REWARDS:
- Knowledge: "_______________"
- Secret: "_______________"
- Item: _______________
- Discovery: "_______________"
```

---

## Scaling Strategy

### Phase 1: Core Components (Launch)
- 5 patterns × 3 locations × 3 complications × 3 revelations = 135 combinations
- Development time: 120 hours for component library
- Result: 3-6 months of content

### Phase 2: Expansion (Month 3)
- 5 patterns × 5 locations × 5 complications × 5 revelations = 625 combinations
- Development time: 80 additional hours
- Result: 12-18 months of content

### Phase 3: Full Library (Month 6)
- 5 patterns × 10 locations × 10 complications × 10 revelations = 5,000 combinations
- Development time: 160 additional hours
- Result: Effectively infinite content

---

## Quality Assurance

### Coherence Testing
Each component combination must:
- Make narrative sense
- Have appropriate difficulty
- Provide meaningful choices
- Connect to other content

### Balance Requirements
- Every investigation needs 3+ viable approaches
- Equipment approaches can't dominate
- Knowledge approaches reward conversation
- Stat approaches reward character building

### Narrative Standards
- Clear problem statement
- Logical phase progression
- Satisfying revelation
- Meaningful consequences

---

## Advanced Techniques

### Component Variants
Each component can have variants:
- Cave (Dry) vs Cave (Flooded)
- Forest (Day) vs Forest (Night)
- Rival (Aggressive) vs Rival (Subtle)

### Conditional Components
Complications that only activate based on player actions:
- Rival appears if player is too obvious
- Weather strikes at phase 2
- Wildlife emerges if player makes noise

### Cascading Investigations
Revelations that unlock new investigations:
- Criminal Conspiracy → Track the Money
- Lost History → Find the Ruins
- Hidden Relationship → Blackmail Plot

### Procedural Adjustments
Modify components based on:
- Player level (stat requirements)
- Time of year (weather complications)
- Previous investigations (NPC reactions)
- World events (political complications)

---

## Content Creator Guidelines

### DO:
- Test all three approaches through investigation
- Ensure revelation affects multiple NPCs
- Provide equipment and knowledge alternatives
- Create interesting failure states
- Add discovered secrets that matter

### DON'T:
- Create single-solution investigations
- Make stat requirements mandatory
- Lock essential progress behind investigations
- Forget to connect to existing NPCs
- Leave investigations without consequences

### REMEMBER:
- Investigations are about discovery, not combat
- Preparation should matter but not gate-keep
- Multiple attempts are expected and encouraged
- Revelations should ripple through the world
- Player agency must be preserved

---

## Implementation Priority

### Must Have First:
1. All 5 base patterns
2. 3 core locations (Building, Forest, Cave)
3. 3 core complications (Rival, Weather, Time)
4. 3 core revelations (Crime, History, Betrayal)

### Should Have Soon:
1. 5 additional locations
2. 5 additional complications
3. 5 additional revelations
4. Component variant system

### Nice To Have:
1. Full 10x10x10x10 library
2. Procedural adjustment system
3. Community creation tools
4. AI-assisted assembly

---

## Success Metrics

### Quantitative:
- Assembly time: <2 hours average
- Play time: 30-60 minutes per investigation
- Replayability: 3+ different paths per investigation
- Coherence: 95% of combinations make sense

### Qualitative:
- "Each investigation feels unique"
- "I couldn't predict the revelation"
- "My preparation mattered"
- "I want to investigate more"

---

## Example: The Lighthouse Keeper's Secret

**Assembly Process** (Total time: 2.5 hours)

1. **Pattern**: Hidden Truth (investigating suspicious death)
2. **Location**: Coastal Lighthouse (variant of Monument)
3. **Complication**: Weather Crisis (storm approaching)
4. **Revelation**: Noble Sacrifice (keeper died protecting town from raiders)

**Customization**:
- Keeper: Old Thomas
- NPCs: Widow Martha, Fisherman Erik, Mayor Blackwood
- Knowledge needed: "Thomas was seen with strangers" (from Erik)
- Equipment needed: Rope (to climb lighthouse), Lantern (to search in storm)

**Phase 1**: Hear rumors about keeper's suspicious death
- Choice A: Question widow (learns about strange behavior)
- Choice B: Examine lighthouse (finds evidence of struggle)
- Choice C: Check records (discovers missing log pages)

**Phase 2**: Storm approaches, investigation urgent
- Choice A: Climb lighthouse in storm (requires rope)
- Choice B: Search keeper's cottage (requires persuading widow)
- Choice C: Confront mayor about cover-up (requires evidence)

**Phase 3**: Truth revealed during storm
- Success: Find keeper's hidden journal, learn about sacrifice
- Partial: Piece together truth from clues
- Failure: Storm destroys evidence, truth remains hidden

**Result**: Complete investigation in 2.5 hours, provides 45-minute play experience with three distinct paths.

---

## Conclusion

The Modular Content System transforms Wayfarer's content creation from an unsustainable hand-crafted process to a scalable, sustainable system. By investing 360 hours in a complete component library, we enable thousands of hours of unique gameplay experiences. The system maintains narrative quality while drastically reducing creation time, solving the content velocity crisis and enabling Wayfarer to grow beyond the limitations of bespoke content creation.