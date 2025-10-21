## Obstacle Templates

Templates provide interesting decisions while working across situations. Most obstacles use one template, varying which properties and consequences apply.

### Template 1: The Gauntlet

**Pattern:** Single approach type, multiple preparations reduce cost

**Structure:**
- One approach goal available immediately at high property level
- Multiple preparation goals reduce same property
- Each preparation unlocks marginally better resolution
- Final state can reach property 0 (free resolution)

**Example: Flooded River Crossing**
```
Obstacle: Flooded River
PhysicalDanger: 3

Approach Goals:
- "Wade Across" (Physical ≤3, always available, ~60 Stamina cost risk)

Preparation Goals:
- "Find Shallow Point" (Mental) → Danger 3→2
- "Construct Rope Bridge" (Physical) → Danger 2→1  
- "Wait for Waters to Recede" (costs 4 time segments) → Danger 1→0

Resolution: All approaches Remove obstacle (cross successfully)
```

**Decision space:** How much time/effort to invest vs stamina cost? Can I afford to wait for free crossing or need to pay cost now?

**Use cases:** Physical barriers, natural obstacles, terrain challenges

### Template 2: The Fork

**Pattern:** Two approaches immediately available, no preparation

**Structure:**
- Two approach goals available from start
- Different property requirements (both ≤2 or ≤3)
- No preparation goals offered
- Forces immediate choice based on current resources

**Example: Suspicious Gatekeeper**
```
Obstacle: Guarded Gate
PhysicalDanger: 2, SocialDifficulty: 1

Approach Goals:
- "Force Past Guard" (Physical ≤2, ~30 Health risk, Remove)
- "Pay Entry Fee" (Social ≤1, 10-15 Coins cost in cards, Remove)

Preparation Goals: None

Resolution: Both Remove obstacle (gain entry)
```

**Decision space:** Spend Health or Coins? Choose based on current resource availability and stat strengths.

**Use cases:** Simple barriers, resource checks, binary choices

### Template 3: The Lock

**Pattern:** One terrible option available, one good option locked

**Structure:**
- One desperate approach (property ≤3) always available
- One much better approach (property ≤2 or ≤1) locked
- Single preparation unlocks better option
- Clear "prepare or suffer" choice

**Example: Sealed Vault Door**
```
Obstacle: Reinforced Vault
PhysicalDanger: 3, MentalComplexity: 3

Approach Goals:
- "Smash Door" (Physical ≤3, ~50 Health, loud/consequences, Remove)
- "Pick Lock" (Mental ≤2, ~15 Focus, clean, Remove)

Preparation Goals:
- "Study Lock Mechanism" (Mental) → Complexity 3→2

Resolution: Both Remove obstacle (door open)
```

**Decision space:** Accept brutal consequence now or invest time for clean approach?

**Use cases:** Locked doors, secured areas, technical barriers

### Template 4: The Puzzle

**Pattern:** Single approach requires multiple properties reduced

**Structure:**
- One approach goal requires two property types ≤ threshold
- Multiple preparation paths (different properties)
- Player must prepare both aspects
- Choice in which preparations to do

**Example: Ancient Mechanism**
```
Obstacle: Complex Machine
PhysicalDanger: 2, MentalComplexity: 2

Approach Goals:
- "Activate Mechanism" (requires Physical ≤1 AND Mental ≤1, Remove)

Preparation Goals:
- "Clear Debris" (Physical) → Danger 2→1
- "Decode Instructions" (Mental) → Complexity 2→1
- "Find Power Source" (Mental, alternate) → Complexity 2→1

Resolution: Remove obstacle (mechanism activated)
```

**Decision space:** Must prepare both Physical and Mental aspects. Choose which Mental prep to do. Order matters for resource management.

**Use cases:** Complex mechanical obstacles, multi-aspect barriers, investigation gates

### Template 5: The Escalation

**Pattern:** Multiple approaches at graduated property levels, better preparation yields better consequences

**Structure:**
- Several approach goals at different property thresholds
- Lower properties unlock better outcomes (not just easier challenges)
- Deep investment yields superior long-term results
- Consequence types improve with preparation

**Example: Hostile Territory**
```
Obstacle: Enemy Territory
PhysicalDanger: 3, SocialDifficulty: 2

Approach Goals:
- "Fight Through" (Physical ≤3, ~50 Health, Remove bandits)
- "Show Force" (Physical ≤2, ~25 Health, Transform to tribute relationship)
- "Diplomatic Entry" (Social ≤2, ~10 Coins, Transform to neutral)
- "Honored Guest" (Social ≤1, free, Transform to allies)

Preparation Goals:
- "Demonstrate Strength" (Physical) → Danger 3→2
- "Build Reputation" (Social) → Difficulty 2→1
- "Complete Favor" (Physical or Social) → properties reduced

Resolution: Consequence type improves with preparation depth
```

**Decision space:** How much to invest in better long-term consequences? Quick violence vs patient diplomacy vs deep relationship building?

**Use cases:** Faction territories, major social barriers, relationship gates

### Template 6: The Discovery

**Pattern:** Obvious approach + hidden bypass

**Structure:**
- One clear approach available (property ≤2)
- Hidden bypass approach (property ≤2) requires discovery
- Discovery comes from unrelated exploration/conversation
- Bypass typically cheaper/cleaner than obvious approach

**Example: Guarded Checkpoint**
```
Obstacle: Military Checkpoint
SocialDifficulty: 2

Approach Goals:
- "Show Papers" (Social ≤2, ~10 Coins, Remove)
- "Servant's Entrance" (Mental ≤2, ~5 Focus, Bypass) [HIDDEN]

Preparation Goals:
- "Bribe Official" (Social) → Difficulty 2→1
- "Observe Building" (Mental, at different location) → Reveals "Servant's Entrance" approach

Resolution: Show Papers removes checkpoint interaction, Servant's Entrance bypasses entirely
```

**Decision space:** Pay obvious cost or explore elsewhere for alternative? Thoroughness rewarded with discovery.

**Use cases:** Guarded locations, official barriers, locations with secrets

### Template 7: The Transformation

**Pattern:** Force approach vs relationship approach with divergent consequences

**Structure:**
- Two approaches at similar difficulty
- One aggressive (Remove consequence)
- One social (Transform consequence)
- Same immediate result, different long-term outcomes

**Example: Unfriendly Innkeeper**
```
Obstacle: Hostile Proprietor
PhysicalDanger: 2, SocialDifficulty: 2

Approach Goals:
- "Intimidate" (Physical ≤2, Remove, get room but create enemy)
- "Build Rapport" (Social ≤2, Transform to friendly contact, future benefits)

Preparation Goals:
- "Ask Around" (Social) → Difficulty 2→1
- "Demonstrate Capability" (Physical) → Danger 2→1

Resolution: Both grant access, but Intimidate creates enemy, Rapport creates ally
```

**Decision space:** Immediate access vs long-term relationship? Force vs friendship? Impact on future interactions?

**Use cases:** NPC obstacles, social barriers, relationship gates

### Template 8: The Expertise Gate

**Pattern:** Requires specific stat level or alternative preparation

**Structure:**
- One approach requires high stat level (shortcut for specialists)
- Alternative approach available with preparation (accessible to all)
- Preparation takes time but bypasses stat requirement
- Rewards specialization but doesn't lock out generalists

**Example: Scholar's Riddle**
```
Obstacle: Ancient Puzzle
MentalComplexity: 2

Approach Goals:
- "Solve Riddle" (Mental ≤2, requires Insight 4+, free if qualified)
- "Research Answer" (Mental ≤1, ~15 Focus, accessible to all)

Preparation Goals:
- "Study Ancient Texts" (Mental) → Complexity 2→1

Resolution: Both Remove obstacle (puzzle solved)
```

**Decision space:** Use high stats as shortcut or prepare to bypass requirement? Specialists rewarded but others not blocked.

**Use cases:** Knowledge gates, skill checks, specialist shortcuts

### Template 9: The Urgent Choice

**Pattern:** Fast dangerous option vs slow safe option

**Structure:**
- Two approaches with time/safety trade-off
- Fast approach costs more resources, no time
- Slow approach costs fewer resources, significant time
- Deadline pressure drives decision

**Example: Collapsing Structure**
```
Obstacle: Unstable Building
PhysicalDanger: 3, MentalComplexity: 2

Approach Goals:
- "Sprint Through" (Physical ≤3, ~40 Health risk, 0 time cost, Remove)
- "Navigate Safely" (Mental ≤2, ~20 Focus, 2 time segments, Remove)

Preparation Goals: None (time pressure prevents preparation)

Resolution: Both Remove obstacle (traversal complete)
```

**Decision space:** Speed vs safety based on deadline urgency and current Health/Focus levels.

**Use cases:** Time-sensitive obstacles, emergency situations, deadline pressure

### Template 10: The Staged Reveal

**Pattern:** Completing approaches unlocks new preparations with deeper outcomes

**Structure:**
- Initial approach available
- Completing first approach or preparation reveals new options
- Each stage unlocks deeper investment opportunities
- Final stage yields superior outcome but requires significant investment

**Example: Criminal Network**
```
Obstacle: Underground Organization
SocialDifficulty: 3

Approach Goals:
- "Prove Yourself" (Physical ≤3, violent entry, Remove)
- "Negotiate Entry" (Social ≤2, paid entry, Transform to contact) [Locked initially]
- "Invited Guest" (Social ≤1, friendly entry, Transform to allies) [Locked initially]

Preparation Goals:
- "Find Contact" (Social) → Difficulty 3→2, reveals "Negotiate Entry"
- "Complete Favor" (Physical, unlocked after Find Contact) → Difficulty 2→1, reveals "Invited Guest"

Resolution: Remove vs Transform to contact vs Transform to allies (each better than previous)
```

**Decision space:** How deep into relationship investment? Quick violence vs progressive social escalation? Time investment vs outcome quality?

**Use cases:** Faction progression, complex social structures, relationship depth systems

---