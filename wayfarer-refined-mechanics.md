# Wayfarer: Refined Game Mechanics

## Core Design Philosophy

### Fundamental Principles

Strategic depth through impossible choices, not mechanical complexity. Atmosphere through grounded danger and meaningful preparation. Slice-of-life pacing with earned discoveries.

- **Elegance Over Complexity**: Every mechanic serves exactly one purpose
- **Verisimilitude Throughout**: All mechanics make narrative sense
- **Active Challenges**: Investigation and travel require choices and preparation
- **State Persistence**: Progress saves, you can retreat and return prepared
- **No Soft-Lock**: Always paths forward, though some require preparation
- **Atmosphere Through Mechanics**: Low fantasy, historical, personal scale

### Design Constraints

**Setting:**
- Low fantasy, historical atmosphere (Frieren, Roadwarden inspiration)
- Small personal scale - villages, farmsteads, nearby countryside
- No modern concepts - no crowbars, no checkpoints, no guards
- Player vs. nature and mystery, not abstract systems
- Text-based with simple decisions, no graphics beyond maps

**Scope:**
- Small timeframes (days, not seasons)
- Small distances (walking range, not mountain passes)
- Small actions (seconds and minutes, like D&D turns)
- No endgame - ongoing slice-of-life structure

**Core Loop:**
- Hear about mystery through obligations and conversations
- Gather information from NPCs
- Prepare resources and equipment
- Attempt challenges actively
- Learn from attempts, return better prepared
- Solve mysteries, gain rewards enabling new challenges

## Three Core Systems

### Conversations (Primary Gameplay)

Builder/spender card dynamics where you generate Initiative through Foundation cards to enable Standard and Decisive cards.

**Five Resources:**
- **Initiative**: Action economy (starts 0, must be built)
- **Momentum**: Progress toward conversation goals (Basic 8, Enhanced 12, Premium 16)
- **Doubt**: Timer (conversation ends at 10)
- **Cadence**: Balance tracker (+1 per SPEAK, -2 per LISTEN, affects Doubt/cards)
- **Statements**: Conversation history (scales effects, costs time)

**Stat Gating:**
Your stat level determines maximum card depth accessible. Higher stats unlock deeper conversational options.

**Personality Rules:**
Each NPC personality transforms tactical approach:
- **Proud**: Must play ascending Initiative order
- **Devoted**: Doubt accumulates faster (+2 instead of +1)
- **Mercantile**: Highest Initiative card gets bonus
- **Cunning**: Repeated Initiative costs penalize
- **Steadfast**: All effects capped at ±2

**Outputs:**
- Stat XP toward depth access
- Token relationship progress
- Information and access
- Observation discoveries

### Investigation (Challenge System)

Locations contain mysteries with multiple investigation steps. Each step presents active choices requiring resources, stats, equipment, or knowledge. Can fail with consequences. State persists between attempts.

**Structure:**
- Mysteries have sequential steps (entry → exploration → discovery)
- Each step offers choices with requirements
- Choices can succeed, fail, or teach what you need
- Retreat possible, progress saved
- Return with better preparation

**Requirements:**
- Adequate health/stamina for physical challenges
- Appropriate equipment (rope, light, tools, warm clothing)
- Knowledge from NPCs revealing easier approaches
- Stat checks for specialized approaches
- Time of day affecting visibility and witnesses

**Dangers:**
- **Physical**: Structural collapse, falls, environmental exposure, exhaustion
- **Social**: Being seen investigating, trespassing, discovering secrets, making enemies
- **Information**: Learning things others want hidden

**Rewards:**
- Observation cards added to specific NPC decks
- Physical items (equipment, permits, trade goods)
- Route access (hidden paths, shortcuts)
- Knowledge enabling other investigations
- NPC relationship depth

### Travel (Obstacle System)

Routes present real obstacles requiring preparation and choices. Not just resource costs - actual challenges to overcome.

**Structure:**
- Routes have obstacles (terrain, water crossings, dense forest)
- Obstacles present choices with requirements
- Can fail and retreat to prepare differently
- Success enables passage, failure teaches needs
- State persists (discovered approaches remain available)

**Requirements:**
- Physical capability (stamina, health)
- Equipment (rope, cutting tools, light, appropriate clothing)
- Knowledge from NPCs (safer paths, hidden routes, timing)
- Weather and time considerations
- Risk assessment based on preparation

**Dangers:**
- Physical injury from inadequate preparation
- Getting lost without proper knowledge
- Environmental exposure (cold, wet, exhaustion)
- Time loss from failed attempts

**Rewards:**
- Route access for future travel
- Discoveries along the way
- Shortcuts with proper knowledge
- Understanding of area geography

## Supporting Systems

### Journal (Obligation Tracking)

The journal replaces queue logistics. Obligations are adventure hooks, not administrative burden.

**Structure:**
- Accept obligations from NPCs
- Each provides: destination, payment, context
- Natural constraints (weather, NPC availability, story logic)
- No position management or weight limits
- Focus on "which adventure to pursue" not "which box to check"

**Function:**
- Obligations give reasons to visit locations
- Create urgency through logical constraints
- Provide income for equipment purchases
- Build relationships through completion
- Connect you to mysteries and discoveries

### Resources

Resources serve challenge preparation, not maintenance.

**Core Resources:**
- **Health**: Injury from dangers, required for physical challenges
- **Stamina**: Exertion capacity for sustained effort
- **Coins**: Purchase equipment and supplies
- **Equipment**: Tools for specific obstacles (rope, light, cutting implements, warm clothing, climbing gear)
- **Supplies**: Consumables for journeys (food, torches, medicine)

**Work:**
- Earn coins for equipment purchases
- Time cost in segments and blocks
- Represents preparation time for challenges
- Not penalized by hunger - just trades time for resources

### Time Structure

**Day Structure:**
- 24 segments total
- 6 blocks of 4 segments each:
  - Early Morning (4 segments)
  - Late Morning (4 segments)
  - Midday (4 segments)
  - Afternoon (4 segments)
  - Evening (4 segments)
  - Night (4 segments)

**Time Costs:**
- Conversations: 1 segment + statements in Spoken
- Investigation steps: Variable by choice (1-3 segments typical)
- Travel: Variable by route and obstacles (1-4 segments typical)
- Work: 4 segments (full block)

**Time Matters:**
- NPCs available at different times
- Investigation visibility changes (daylight vs. darkness)
- Weather affects accessibility
- Natural urgency from world rhythm

### NPC Architecture

**Five Persistent Decks:**
- **Conversation**: Base cards for their personality
- **Request**: Obligations they can offer
- **Observation**: Cards from your investigation discoveries
- **Burden**: Failed obligations and damaged trust
- **Exchange**: Trade opportunities they provide

**Token Relationships:**
- Gradual accumulation through completed obligations
- Unlock signature cards at thresholds
- Represent trust and familiarity
- Enable access to specialized exchanges
- Open dialogue about sensitive topics

**Personality Impact:**
- Determines conversation tactical approach
- Influences available obligation types
- Affects reaction to investigation discoveries
- Shapes how they help with challenges

### Location Architecture

**Investigation Opportunities:**
- Each location has mysteries to uncover
- Mysteries have multiple steps with choices
- State persists between visits
- Partial progress saves
- Dangers appropriate to location type

**Time Properties:**
- NPC availability changes by time block
- Investigation difficulty varies (darkness, crowds)
- Some opportunities only at specific times
- Creates natural rhythm and planning

**Connections:**
- Routes between locations
- Each route has obstacles
- Access enabled through preparation or knowledge
- Shortcuts discoverable through investigation

## Stat System

**Five Stats:**
- **Insight**: Analytical thinking, pattern recognition
- **Rapport**: Emotional connection, empathy
- **Authority**: Force of personality, command
- **Diplomacy**: Negotiation, mercantile thinking
- **Cunning**: Indirect approaches, subtlety

**Progression:**
- Gain XP from conversation cards (1 XP per card × conversation difficulty)
- Level advancement unlocks deeper card access
- Higher levels enable specialized investigation approaches
- Represents developing problem-solving methodologies

**Investigation Approaches by Stat:**
- Insight: Systematic analysis, deduction
- Rapport: Social information networks
- Authority: Demanding access, assertive investigation
- Diplomacy: Purchasing information and access
- Cunning: Covert investigation, avoiding detection

## Integration Principles

### Obligations Drive Exploration

Obligations are reasons to visit locations, not tasks to complete. Elena needs documents delivered to remote farmstead - that's your hook to explore countryside, discover the old mill, uncover mysteries.

Payment covers costs, but real reward is access to new content.

### Investigation Reveals Connections

Discoveries create observation cards for specific NPCs. Your exploration of locations yields knowledge valuable to people you know. Creates natural connection between exploration and relationships.

Tower investigation finds merchant guild records - creates card for Marcus's deck. Courthouse investigation finds corruption evidence - creates card for Elena's deck.

### Preparation Enables Success

Challenges require proper preparation. Failed attempts teach what you need. Return with better equipment, knowledge from NPCs, higher stats enabling different approaches.

First creek crossing fails without rope. Mill investigation fails without light. Tower mechanism requires understanding from journals. Each teaches what preparation is needed.

### NPCs Provide Practical Support

Relationships aren't just sentiment - they provide survival value. NPCs offer:
- Information revealing easier approaches
- Equipment through trade
- Knowledge of dangers and how to avoid them
- Access to restricted areas
- Support during challenges

Marcus knows merchant routes and provides maps. Elena knows legal access rights. Hunter knows safe wilderness paths and crafts equipment.

### Dangers Create Atmosphere

Physical dangers from environment and structure. Social dangers from witnessing or discovery. Information dangers from learning secrets. All grounded in verisimilitude.

No abstract penalties - consequences make sense. Fall and get injured. Witness crime and make enemies. Trespass and face social consequences.

### State Persistence Enables Planning

Investigations save progress. Routes remember discovered approaches. Failed attempts teach without punishment. Can retreat, prepare properly, return to continue.

Creates long-term goals: "Need rope for that crossing." "Need to talk to Marcus about mill history." "Need better stamina for that climb." Work toward objectives across multiple days.

## Content Structure: Three Layers

### Mechanical Layer (Authoritative)

All card effects, investigation choices, travel obstacles, resource requirements. Creates strategic decisions through impossible choices and meaningful constraints.

Deterministic and clear. Never hidden or ambiguous.

### Authored Layer (Structural, Sparse)

Key moments creating memorable peaks:
- NPC introductions establishing personality
- Token threshold scenes (5, 10, 15 tokens)
- Location first-visit vignettes
- Major mystery reveals
- Seasonal or calendar events

Same for all players, no branching. Creates structure in slice-of-life flow.

### AI Flavor Layer (Contextual, Pervasive)

Everything adapting to exact game state:
- Conversation dialogue reflecting relationship depth
- Investigation narration building on history
- NPC reactions to your choices and discoveries
- Travel descriptions showing familiarity
- Work scenes showing community integration

Makes mechanical choices feel personal without inventing consequences.

**Critical Constraint:** AI flavors but never invents. Mechanics remain authoritative. AI describes what mechanics determined, never creates new mechanical effects.

## Design Verification

### Does It Create Atmosphere?

Low fantasy historical setting through:
- Physical dangers (environmental, structural)
- Small personal scale (villages, farmsteads, nearby countryside)
- Grounded equipment (rope, light, clothing, tools)
- Social consequences (reputation, trespassing, enemies)
- Natural world as challenge (weather, terrain, darkness)

### Does It Support Slice-of-Life?

- No endgame or ticking clock
- Daily rhythm through time blocks
- Gradual relationship building
- Discovery as intrinsic reward
- Multiple valid focuses (different NPCs, different locations)
- Routine becomes comfortable
- Small moments accumulate meaning

### Does It Avoid System Conflict?

- Obligations encourage exploration (don't compete with it)
- Resources serve challenges (not maintenance)
- Relationships provide practical value (not just sentiment)
- Investigation is engaging activity (not grind)
- Time creates rhythm (not administrative stress)

### Does It Maintain Verisimilitude?

- Investigation choices make sense
- Dangers are real and grounded
- Equipment serves logical purposes
- NPC help is practical
- Consequences follow naturally
- State persistence is realistic

### Does Preparation Matter?

- Failed attempts teach needs
- Equipment enables success
- Knowledge reveals better approaches
- Stats unlock specialized solutions
- NPCs provide critical information
- Resources accumulated serve purpose

## Conclusion

The refined system achieves integration through:

**Mechanical clarity** - Each system serves one purpose elegantly

**Atmospheric immersion** - Dangers and challenges create low fantasy historical feel

**Slice-of-life pacing** - No urgency beyond natural constraints, ongoing discovery

**Active engagement** - Investigation and travel require choices and problem-solving

**Meaningful preparation** - Resources serve clear purposes toward overcoming challenges

**Relationship value** - NPCs provide practical support and information

**State persistence** - Progress saves, enabling long-term planning and goals

Every choice matters. Every preparation serves purpose. Every discovery connects to broader world. The mechanics create a courier's life exploring mysteries in a dangerous world, where relationships provide survival value and accumulated small moments create meaning.
