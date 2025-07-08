# The Contextual Approach System

## Core Mechanics: Action → Approach Selection

1. **Player selects an action** at a location spot (e.g., "Work at the Forge")
2. **Player commits to the action**, consuming the time window
3. **Multiple approaches appear**, each with:
   - Specific numerical requirements (now visible)
   - Distinct costs and yields (not simply scaled versions)
   - Clear enablement for other actions/approaches
4. **Player selects one available approach** based on requirements they meet

## Strategic Web of Enablement

The key innovation is how each approach specifically enables different actions across locations:

```
FORGE approaches enable GUARD POST approaches:
- Forge layout knowledge → "Suggest Equipment Storage" at Guard Post

GUARD POST approaches enable WILDERNESS approaches:
- Combat training → "Track Dangerous Game" in Forest 
- Patrol routes → "Find Hidden Paths" in Forest

WILDERNESS approaches enable MARKET approaches:
- Rare herbs → "Trade Exotic Goods" at Market
- Game knowledge → "Negotiate with Hunter's Guild" at Market

MARKET approaches enable FORGE approaches:
- Quality materials → "Craft Superior Items" at Forge
- Foreign techniques → "Implement New Methods" at Forge
```

## Cyclical Goal Achievement

This creates a system where:

1. Player identifies a desired outcome (e.g., "I want to craft a superior weapon")
2. Player discovers the requirement chain (Quality materials → Market → Wilderness → Guard Post → Forge)
3. Player works backward to find an entry point matching current capabilities
4. Each success builds toward the ultimate goal while providing immediate benefits
5. Multiple valid paths exist to reach the same goal

## Number-Based Tracking Systems

All requirements use trackable numeric values:

1. **Skills** (0-10 scale)
   - Warfare, Wilderness, Scholarship, Diplomacy, Subterfuge
   - Increased by using related approaches (+1 to +3)

2. **Relationships** (0-10 scale)
   - Each character has relationship value
   - Increased through character interactions (+1 to +3)

3. **Resources & Items** (inventory system)
   - Tracked as specific named items
   - Boolean flags for possession

4. **Knowledge Points**

## Distinct Approach Types

Each action offers fundamentally different approaches, not just scaled versions:

1. **Skill-Based Approaches**
   - Require skill thresholds
   - Yield good coin rewards and skill improvements
   - Enable other skill-based approaches

2. **Relationship-Based Approaches**
   - Require relationship levels
   - Yield special knowledge and relationship improvements
   - Enable social opportunities and information

3. **Resource-Based Approaches**
   - Require specific items
   - Yield valuable new items or substantial coin

4. **Knowledge-Based Approaches**
   - Require learned techniques or information
   - Yield strategic advantages and insights
   - Enable specialized approaches across locations

## Implementation Across Location Types

This system works consistently across all location types:

### At the Market:
```
You select: "Interact with Merchant"

[After committing...]

1. "Negotiate Trade Agreement" 
   Requirements: Diplomacy ≥ 3
   Enables: Wholesale purchases, Supply chain information

2. "Share Regional Information"
   Requirements: Have visited 3+ locations
   Enables: Foreign goods access, Caravan schedules

3. "Display Quality Goods"
   Requirements: Possess Rare Item
   Enables: Exclusive buyer introductions, Market network
```

### In the Wilderness:
```
You select: "Explore Forest Path"

[After committing...]

1. "Track Wildlife" 
   Requirements: Wilderness ≥ 2
   Enables: Efficient hunting

2. "Identify Medicinal Plants"
   Requirements: Scholarship ≥ 2
   Enables: Remedy creation

3. "Search for Hidden Passages"
   Requirements: Knowledge of local landmarks
   Enables: Shortcut travel options
```

## The Key Benefits

This refined system creates exactly what you requested:

1. **Anticipation through hidden requirements** until player commits
2. **Multiple valid approaches** to every situation
3. **Different outcomes** based on approach, not just scaled rewards
4. **Cyclical enablement** where each success opens new possibilities
5. **Goal-oriented progression** driven by player choice
6. **Fast cycles** of challenge and achievement
7. **Integrated systems** without adding complexity

Most importantly, it creates the core motivation loop where players constantly discover new possibilities just beyond their reach, identify clear paths to reach them, and experience the satisfaction of turning limitations into new opportunities.


# The Integrated Context Web: A Complete Action-Approach Framework

## Core Action-Approach Loop

1. **Player selects an action** at a location spot
2. **Player commits resources** (time window, energy, etc.)
3. **Player sees multiple approaches** with specific requirements
4. **Some approaches are unavailable** due to unmet requirements
5. **Player selects available approach** based on desired outcomes
6. **Success develops journal entries** that unlock new approaches elsewhere

## Entry Types and Requirements

All journal entries function as keys to unlock specific approaches:

### Place Entries (Where you've been)
```
"Village Forge"
Tags: [Workshop, Metal, Commerce]
Depth: 1-3 (familiarity)
```

### Bond Entries (Who you know)
```
"Emil the Apprentice"
Tags: [Craftsman, Youth, Village]
Depth: 1-3 (relationship)
```

### Insight Entries (What you've learned)
```
"Metal Properties"
Tags: [Trade]
Depth: 1-3 (understanding)
```

## Tag-Based Requirement System

Requirements are defined through tag combinations:
```
PLACE:[Workshop,Metal]:2
= Any Place entry with Workshop AND Metal tags at Depth 2+

BOND:[Craftsman]:1
= Any Bond entry with Craftsman tag at Depth 1+

INSIGHT:[Material,Trade]:2
= Any Insight entry with Material AND Trade tags at Depth 2+
```

## Context-Sensitive Approaches

Each action presents four different approaches:

```
ACTION: "Work at the Forge"

1. "Apply Material Knowledge" (Knowledge Approach)
   Requires: INSIGHT:[Metal,Crafting]:2
   Costs: 3 Energy, 2 Focus
   Yields: 8 coins, deepen Metal Properties insight

2. "Collaborate with Smith" (Social Approach)
   Requires: BOND:[Craftsman,Village]:1  
   Costs: 2 Energy, 1 Focus
   Yields: 6 coins, deepen relationship with smith

3. "Use Forge Layout" (Experience Approach)
   Requires: PLACE:[Workshop]:1
   Costs: 3 Energy, 1 Focus
   Yields: 5 coins, new insight about techniques

4. "Provide Basic Labor" (Always Available)
   Requires: None
   Costs: 4 Energy
   Yields: 3 coins, create basic workshop familiarity
```

## Cyclical Enablement System

Each successful approach enables future approaches elsewhere:

```
"Apply Material Knowledge" at the forge
↓
Deepens "Metal Properties" insight
↓
Unlocks "Evaluate Merchant Goods" approach at Market
↓
Yields "Trading Techniques" insight
↓
Unlocks "Negotiate Special Prices" approach with Traveling Vendor
```

## Approach Generation Rules

1. **Every action always presents exactly 4 approaches**:
   - One with no special requirements (always available)
   - Three with different types of requirements (skill, journal entry, or combination)
   
2. **Requirements must follow these patterns**:
   - ENTRY_TYPE:[TAG1,TAG2]:DEPTH
   - SKILL_TYPE:VALUE
   - Or combinations of both
   
3. **Requirements must be contextually relevant**:
   - Place requirements must logically connect to the current location
   - Bond requirements must involve socially connected characters
   - Insight requirements must relate to applicable knowledge

4. **Approaches must offer different outcome types**:
   - Resource-focused outcomes (optimal coin/item gains)
   - Relationship-focused outcomes (bond development)
   - Knowledge-focused outcomes (insight development)
   - Travel-focused outcomes (new location discovery)

## Entry Development System

1. **Entry Creation**:
   - First meaningful interaction creates Depth 1 entry
   - Basic entries have 2-3 contextually appropriate tags

2. **Entry Deepening**:
   - Using entries in successful approaches increases depth
   - Higher depth requires multiple successful applications
   - Maximum depth is 3 (representing significant understanding)

3. **Tag Expansion**:
   - Additional tags can be added through specific experiences
   - Maximum 5 tags per entry
   - Tags must remain contextually appropriate

## Tag Categories and Hierarchies

Tags follow organized categories for consistency:

1. **Location Tags**: Physical place types
   - Settlement: [Village, Town, City, Outpost]
   - Natural: [Forest, River, Mountain, Road]
   - Function: [Market, Workshop, Inn, Farm]

2. **Character Tags**: Social roles and relationships
   - Profession: [Merchant, Craftsman, Guard, Farmer]
   - Relationship: [Family, Friend, Rival, Authority]
   - Background: [Local, Traveler, Noble, Common]

3. **Knowledge Tags**: Types of understanding
   - Craft: [Metal, Wood, Cloth, Food]
   - Activity: [Trade, Combat, Navigation, Performance]
   - Subject: [Nature, People, History, Magic]

## Example: The Motivation Loop in Action

```
1. Player visits the Village Forge
   - Selects "Work at the Forge" action
   - Only "Provide Basic Labor" approach is available
   - Success creates "Village Forge" Place entry [Workshop,Metal]:1

2. Player visits the Village Market
   - Selects "Browse Wares" action
   - "Identify Quality Materials" approach requires INSIGHT:[Material]:1
   - This approach is unavailable (player lacks material knowledge)

3. Player returns to Forge with Place entry
   - New approach "Discuss Materials" now available
   - Requires: PLACE:[Workshop]:1 (player has this!)
   - Success creates "Basic Materials" Insight entry [Material,Craft]:1

4. Player returns to Market with new Insight
   - "Identify Quality Materials" approach now available!
   - Success yields better prices and deepens Material knowledge
   - Also creates connection to traveling merchant character

5. Meeting the merchant unlocks new location
   - Specialist forge in neighboring town
   - Offers advanced approaches requiring deeper knowledge
   - Player now has clear motivation to develop material knowledge
```

## Implementation Benefits

1. **Immediate Progression**: Players see results within minutes through journal entries

2. **Clear Motivation**: Unavailable approaches with visible requirements create obvious goals

3. **Multiple Paths**: Different requirement types allow various strategies for progress

4. **Contextual Value**: The same knowledge has different applications in different contexts

5. **Scaling Challenge**: As players develop entries, they encounter approaches with higher requirements

6. **Deterministic Rules**: All requirements and outcomes follow clear, programmable patterns

7. **AI-Compatible**: Tag-based system allows procedural content generation
