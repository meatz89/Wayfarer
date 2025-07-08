# Wayfarer: Affliction and Resource Systems

## 1. Design Philosophy and Inspirations

Wayfarer's core systems draw inspiration from two masterful games with distinct mechanical approaches:

**From dotAGE:**
- Resource management with constant pressure and escalating needs
- Threat-based afflictions requiring preventative maintenance
- Building relationships to unlock resource production
- Strategic tradeoffs between immediate needs and long-term sustainability

**From Sir Brante:**
- Stat-gated choices that shape character development
- Willpower as a limited resource for significant decisions
- Character specialization affecting available options
- Choices with persistent consequences

The design principles guiding these systems are:
- Natural escalation tied directly to player progression
- Indirect influence between afflictions through shared resources
- Immediate tension from Day 1 forcing meaningful decisions
- Character archetype integration through mechanical advantages
- Elegant simplicity with minimal tracking variables

## 2. Foundational Resources

### Action Points (AP)

- **Starting Amount**: 4 AP per day
- **Function**: Primary action currency, required for all activities
- **Restoration**: Full reset at the beginning of each day
- **Progression Impact**: Endurance skill increases max AP (+1 per level) but raises daily Energy cost
- **Archetype Integration**: No direct AP bonuses by archetype, but efficiency varies by approach alignment

### Energy

- **Starting Amount**: 12 Energy
- **Daily Cost**: Equal to maximum AP (4 initially)
- **Function**: Core survival resource creating 3-day buffer before consequences
- **Restoration**: Only through Rest actions with varying efficiency
  - Basic rest (outdoors/floor): +2 Energy
  - Inn rest (Level 1 relationship): +4 Energy
  - Quality accommodations (Level 2): +6 Energy
  - Luxury accommodations (Level 3): +8 Energy
- **Progression Impact**: Conservation skill reduces daily Energy cost by 1 per level
- **Archetype Integration**: No direct Energy bonuses by archetype

### Vigor

- **Function**: Approach flexibility resource inspired by Sir Brante's Willpower
- **Character Specialization**: Each archetype has:
  - Natural approaches (2): 0 Vigor cost
  - Neutral approaches (2): 1 Vigor cost
  - Weak approach (1): 2 Vigor cost
- **Impact**: When Vigor is depleted, character is limited to natural approaches only
- **Restoration**: Specific actions through developed relationships, often costing coins
- **Archetype Integration**: Direct advantage through natural approaches aligning with character strengths

## 3. The Four Affliction Systems

### Exhaustion Affliction

**Core Mechanics:**
- When day ends with Energy at 0:
  * Gain Exhaustion points equal to unpaid AP cost
  * Example: Ending with -2 Energy = 2 Exhaustion points

**Effect Thresholds:**
- Level 3+: Rest restores 1 less Energy
- Level 6+: Lose 1 random AP daily to "dozing off"
- Level 9+: Character collapses, loses a full day recovering

**Escalation System:**
- When Exhaustion affliction has points:
  * Each AP spent adds +1 to Exhaustion
  * Creates accelerating spiral where more actions lead to more Exhaustion

**Recovery:**
- Rest actions with varying efficiency:
  * Basic rest (outdoors): -2 Exhaustion
  * Inn rest (Level 1): -4 Exhaustion
  * Quality accommodations (Level 2): -6 Exhaustion

**Archetype Integration:**
- Physical archetypes naturally spend less Energy on strenuous tasks
- Mental archetypes require more restorative rest as mental skills increase

### Hunger Affliction

**Core Mechanics:**
- Generate Hunger points through:
  * +1 per day automatically
  * +2 per physical action
  * +1 per intellectual action
  * +0 per social action
- End of day: remaining Hunger points add to Hunger affliction

**Effect Thresholds:**
- Level 3+: -1 to all physical actions
- Level 6+: -2 to physical actions, physical skill usage costs +1 Vigor
- Level 9+: Character weakens, loses 1 Health per day until reduced

**Escalation System:**
- When Hunger affliction has points:
  * Each action adds +1 additional Hunger
  * Creates spiral where activity accelerates hunger

**Recovery:**
- Eat food action (1 AP) reduces Hunger points
  * Basic food: -3 Hunger
  * Quality meal: -6 Hunger
  * Feast: -10 Hunger
- Excess food creates "satiation" preventing Hunger generation temporarily

**Progression-Linked Escalation:**
- Each level in Strength skill reduces basic food effectiveness by 1
- Higher strength requires better quality food

**Archetype Integration:**
- Physical archetypes excel at hunting/gathering
- Can access better food sources with less AP investment
- Generate more Hunger but have more efficient means to address it

### Mental Strain Affliction

**Core Mechanics:**
- Generate Mental Load points through:
  * +2 for complex intellectual tasks
  * +1 for standard intellectual tasks
  * +1 for failed actions of any type
- End of day: remaining Mental Load adds to Mental Strain affliction

**Effect Thresholds:**
- Level 3+: Max Focus reduced by 2
- Level 6+: Analysis/Precision effectiveness -1
- Level 9+: Mental burnout, cannot learn new skills for 3 days

**Escalation System:**
- When Mental Strain affliction has points:
  * Focus costs increase by 1 for all actions
  * At Mental Strain 3+: All actions generate +1 Mental Load
  * At Mental Strain 6+: Failed actions generate +2 Mental Load

**Recovery:**
- Relaxation actions (1 AP) reduce Mental Load
  * Basic relaxation: -3 Mental Load
  * Quality relaxation: -6 Mental Load
  * Scholarly techniques: -10 Mental Load

**Progression-Linked Escalation:**
- Higher Focus enables more complex tasks that generate more Mental Load
- Higher mental skills make basic relaxation less effective

**Archetype Integration:**
- Analytical archetypes generate less Mental Load from intellectual tasks
- Can sustain mental effort longer before strain accumulates
- Have access to more efficient relaxation techniques

### Isolation Affliction

**Core Mechanics:**
- Generate Disconnection points through:
  * +1 per day automatically
  * +1 for each day without social interaction
- End of day: remaining Disconnection adds to Isolation affliction

**Effect Thresholds:**
- Level 3+: -1 to all Rapport actions
- Level 6+: Max Confidence reduced by 3
- Level 9+: Paranoia, social actions cost double AP

**Escalation System:**
- When Isolation affliction has points:
  * Social actions cost +1 Vigor (harder to connect)
  * At Isolation 3+: Gain +1 Disconnection per day (now +2 total)
  * At Isolation 6+: Failure rate increases for all social actions

**Recovery:**
- Social actions (1 AP) reduce Disconnection
  * Basic conversation: -2 Disconnection
  * Meaningful interaction: -5 Disconnection
  * Deep connection: -8 Disconnection

**Progression-Linked Escalation:**
- Each Rapport level reduces basic social interaction effectiveness by 1
- Higher social skill makes casual conversation less satisfying

**Archetype Integration:**
- Social archetypes gain bonus Disconnection reduction from interactions
- Can build deeper relationships more efficiently
- May discover unique interaction options that provide multiple benefits

## 4. Progression-Driven Escalation

The elegance of Wayfarer's difficulty curve lies in how challenge escalates naturally through player progression:

### The Cost of Capability
- Increasing AP through Endurance directly increases Energy cost
- This creates a natural metabolic escalation - more capability requires more fuel
- Players who become more powerful must invest in better Energy restoration

### Sophistication Breeds Need
- As core skills increase, basic maintenance options become less effective:
  * Stronger characters need better food (Strength reduces basic food effectiveness)
  * Mentally developed characters need better rest (Mental skills reduce basic rest effectiveness)
  * Socially adept characters need deeper connections (Rapport reduces basic social effectiveness)
- This isn't artificial difficulty - it reflects how growth naturally creates more refined needs

### The Burden of Complexity
- Progression unlocks more complex, rewarding actions that inherently:
  * Generate more Mental Load due to complexity
  * Carry higher social stakes and risks
  * Require more physical exertion
- The pressure increases because the player chooses to engage with more demanding activities

### Affliction Acceleration
- Once afflictions have points, they begin accelerating:
  * Exhaustion makes each AP spent add to Exhaustion
  * Hunger makes each action generate additional Hunger
  * Mental Strain makes all actions generate Mental Load
  * Isolation makes social actions harder (costing Vigor)
- This creates the classic dotAGE-style death spiral that rewards prevention over cure

## 5. System Triangles: Indirect Affliction Influence

Afflictions influence each other indirectly through resource competition and shared consequences:

### AP Competition Triangle
- All affliction management requires AP
- AP spent on Rest (Exhaustion) can't be used for eating (Hunger)
- AP spent on social interaction (Isolation) can't be used for relaxation (Mental Strain)
- Affliction penalties that reduce AP (like Exhaustion) affect all other affliction management

### Vigor Flexibility Triangle
- Affliction management often requires specific approaches
- Using non-natural approaches costs Vigor
- Vigor spent addressing one affliction limits flexibility for others
- When Vigor is depleted, only natural approaches remain available

### Relationship Investment Triangle
- Better affliction management requires developing relationships
- Relationship development consumes AP and resources
- Investing in one relationship (Innkeeper for Rest) delays others (Food Vendor for Hunger)
- Forces strategic prioritization of which afflictions to manage efficiently

### Penalty Cascade Triangle
- Affliction penalties affect core resources needed by all systems
- Hunger penalties reduce physical effectiveness, making work more costly
- Mental Strain penalties increase failure rates, generating more Mental Load
- Isolation penalties double AP costs for social actions, limiting affliction management

## 6. Early Game Dynamics (Days 1-3)

From day one, players face immediate tension and difficult choices:

### The Energy Countdown
- Starting Energy (12) creates a 3-day buffer before Exhaustion
- Player must establish sustainable Energy restoration before Day 4
- Creates urgent medium-term goal with strict deadline

### The AP Scarcity & Opportunity Cost
- Only 4 AP per day forces brutal prioritization
- Can't simultaneously:
  * Explore to find key locations
  * Build necessary relationships
  * Address immediate affliction needs
  * Generate resources for services

### The Gated Solutions Paradox
- Effective affliction management requires Level 1+ relationships
- Building relationships requires AP investment
- AP needed for survival competes with AP needed for relationship development
- Creates classic "invest or manage" dilemma

### Early Affliction Creep
- Multiple afflictions begin rising by Day 2-3
- Forces decisions about which afflictions to address first
- Creates natural tension between short-term survival and long-term sustainability

## 7. Character Archetype Integration

Different archetypes experience afflictions differently through mechanical advantages:

### Physical Archetypes
- Natural at Dominance/Strength approaches (0 Vigor cost)
- More efficient at food procurement and physical labor
- Generate more Hunger but handle physical effects better
- May struggle with Mental Strain and social interactions

### Mental Archetypes
- Natural at Analysis/Precision approaches (0 Vigor cost)
- Generate less Mental Load from intellectual tasks
- Find more efficient solutions through insight
- May struggle with physical tasks and Hunger management

### Social Archetypes
- Natural at Rapport approaches (0 Vigor cost)
- Build relationships more efficiently
- Manage Isolation with minimal effort
- May struggle with physical tasks and Mental Strain

### Balanced Archetypes
- Moderate advantages across multiple approaches
- No extreme strengths or weaknesses
- More flexible but less specialized
- Vigor management becomes more critical

## 8. Location Spot Relationship System

Each location spot functions like a building in dotAGE, with levels that unlock better options:

### Relationship Progression
- Level 0→1: Requires AP investment only (2-3 AP typically)
- Level 1→2: Requires AP + specific resource (coins, confidence, etc.)
- Level 2→3: Requires AP + increased resources + unique requirements

### Relationship Types and Benefits
- **Innkeeper**: Energy restoration and Exhaustion management
  * Level 1: Basic room (+4 Energy, -4 Exhaustion)
  * Level 2: Quality room (+6 Energy, -6 Exhaustion)
  * Level 3: Luxury suite (+8 Energy, -8 Exhaustion)

- **Food Vendor**: Hunger management and satiation
  * Level 1: Daily meals (+3 Hunger reduction)
  * Level 2: Quality meals (+6 Hunger reduction)
  * Level 3: Feasts (+10 Hunger reduction)

- **Scholar/Artist**: Mental Strain management
  * Level 1: Basic techniques (-3 Mental Load)
  * Level 2: Advanced methods (-6 Mental Load)
  * Level 3: Mastery techniques (-10 Mental Load)

- **Social Connection**: Isolation management
  * Level 1: Regular interaction (-2 Disconnection)
  * Level 2: Meaningful connection (-5 Disconnection)
  * Level 3: Deep relationship (-8 Disconnection)

---

This system creates the elegant pressure of dotAGE with the meaningful character specialization of Sir Brante, all within an authentic medieval setting. Players face constant choices between immediate needs and long-term sustainability, with challenges that escalate naturally as a direct consequence of their progression.