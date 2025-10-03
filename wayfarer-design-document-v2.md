# Wayfarer: Game Design Document

## Core Design Philosophy

### Fundamental Principles

Wayfarer creates strategic depth through impossible choices, not mechanical complexity. Every decision forces players to choose between multiple suboptimal paths, revealing character through constraint.

- **Elegance Over Complexity**: Every mechanic serves exactly one purpose
- **Verisimilitude Throughout**: All mechanics make narrative sense in a grounded historical-fantasy world
- **Perfect Information**: All calculations visible to players
- **No Soft-Lock Architecture**: Always a path forward, even if suboptimal
- **Atmosphere Through Mechanics**: Danger, preparation, and discovery create immersion

### Genre Definition

Low-fantasy historical CRPG with visual novel dialogue and resource management. Text-based with simple decisions. Atmosphere like Frieren or Roadwarden - player versus nature, relationships matter, small personal scale.

**Not included:** Modern concepts, checkpoints/guards, mountain passes, epic scope. **Focus:** Small timeframes (days), short distances (walking), actions spanning seconds/minutes like D&D sessions.

## Three Core Loops

### Conversations (Primary Gameplay)

Card-based dialogue system using builder/spender dynamics. Five resources (Initiative, Momentum, Doubt, Cadence, Statements) create tactical decisions. Personality rules fundamentally alter approach.

**Purpose:**
- Information gathering for challenges
- Relationship building for practical support
- Stat progression through experience
- Primary expression of player methodology

**Integration:** NPCs provide route knowledge, equipment sources, investigation clues, danger warnings. Conversations serve adventure goals.

### Obligation Journal (Adventure Hooks)

Simple quest log tracking active requests from NPCs. No complex management - obligations are reasons to explore dangerous places, not logistics puzzles.

**Structure:**
- Accept delivery/task from NPC
- Destination becomes adventure location
- Weather/time creates natural urgency
- Completion provides payment, relationships, access

**Purpose:** Drive player toward interesting content (dangerous routes, mysterious locations, challenging investigations).

### Investigation & Travel (Active Challenges)

Locations present mysteries requiring preparation, equipment, knowledge, and careful choices. Travel routes have real obstacles demanding resources and planning.

**Structure:**
- Mysteries with multiple investigation steps
- Each step presents choices with requirements
- State persists - retreat and return prepared
- Success unlocks deeper layers
- Dangers are physical, environmental, and social

**Purpose:** Core adventure gameplay. Preparation matters. Failure teaches. Discovery is earned through risk and effort.

## NPC System

### Five Persistent Decks

Each NPC maintains:
1. **Conversation Deck**: Base cards for their conversation types
2. **Request Deck**: Available tasks and deliveries
3. **Observation Deck**: Cards gained from your discoveries
4. **Burden Deck**: Consequences of failures and conflicts
5. **Exchange Deck**: Trade opportunities and special purchases

### Relationship Progression

Relationships deepen gradually through:
- Completed deliveries (earn trust)
- Information shared (prove resourcefulness)
- Challenges overcome together (demonstrate competence)
- Time spent in conversation (build familiarity)

**Mechanical Benefits:**
- Signature cards appear in conversations
- Access to specialized equipment/items
- Route knowledge and safer paths revealed
- Investigation clues and context provided
- Support during dangerous challenges

### Personality Rules

Each NPC has conversational personality affecting tactical approach:
- **Proud**: Must play cards in ascending Initiative order
- **Devoted**: Doubt accumulates faster (+2 instead of +1)
- **Mercantile**: Highest Initiative card gets bonus effect
- **Cunning**: Repeated Initiative costs penalty
- **Steadfast**: All effects capped at ±2

These transform the conversation puzzle while maintaining core mechanics.

## Investigation System

### Core Concept

Investigation is active problem-solving with choices, requirements, costs, and consequences. Not passive familiarity grinding.

### Structure

**Mysteries Have Layers:**
Locations contain multi-step investigations. Each layer presents new challenges requiring different preparation. Early steps might be accessible, deeper steps demand equipment, knowledge, or physical capability.

**State Persistence:**
Progress saves between visits. Discovered information remains. Partial exploration remembered. Can retreat, prepare, return to exactly where you left off.

**Active Choices:**
Each investigation step presents options:
- Different approaches (stealth vs. direct, careful vs. quick)
- Resource trade-offs (time vs. stamina, safety vs. information)
- Risk assessment (guaranteed safe vs. potential danger)
- Preparation checks (do you have needed equipment/knowledge?)

### Investigation Phases

**Phase 1 - Approach:**
How do you access the location? Choices might include:
- Direct entry (visible, straightforward, might alert others)
- Hidden entrance (requires knowledge from NPC)
- Careful observation first (costs time, reveals information)
- Wait for specific conditions (time of day, weather, NPC absence)

**Phase 2 - Initial Exploration:**
What do you examine first? Each choice reveals different information:
- Structural assessment (physical dangers, stability)
- Environmental reading (lighting, temperature, hazards)
- Social evidence (signs of recent activity, ownership)
- Historical context (age, original purpose, changes over time)

**Phase 3 - Deep Investigation:**
Pursue specific leads based on initial findings. Requires:
- Appropriate equipment (light sources, tools, safety gear)
- Sufficient physical capability (health, stamina for exertion)
- Relevant knowledge (from conversations, previous discoveries)
- Careful decision-making (wrong choices trigger dangers)

**Phase 4 - Resolution:**
Extract discovered knowledge or items. Might involve:
- Careful retrieval (avoid damage, maintain secrecy)
- Confrontation with interested parties (if discovered)
- Connecting clues to broader mysteries
- Deciding what to do with knowledge gained

### Investigation Dangers

**Physical Hazards:**
- Structural collapse in old buildings
- Falls from heights without proper equipment
- Environmental exposure (cold, darkness, getting lost)
- Exhaustion from insufficient stamina reserves
- Injury from navigating dangerous terrain

**Social Risks:**
- Being observed investigating (creates suspicion)
- Trespassing consequences (legal or social)
- Discovering secrets others want hidden (makes enemies)
- Being followed by interested parties
- Alerting wrong people to valuable information

**Resource Costs:**
- Time segments consumed by investigation
- Equipment degraded or consumed (light sources, tools)
- Stamina depleted requiring rest
- Health lost to hazards requiring recovery
- Opportunities missed while investigating

### Investigation Requirements

**Knowledge:**
Information from NPCs critical for success. Examples:
- "The mill has a hidden entrance on the east side"
- "Avoid the upper floor, the boards are rotten"
- "The mechanism is dangerous unless you stabilize it first"
- "Someone else has been investigating there recently"

**Equipment:**
Tools and supplies for specific challenges:
- Rope for climbing or securing unstable areas
- Light sources for darkness
- Tools for clearing obstacles or accessing mechanisms
- Warm clothing for cold/wet environments
- Lockpicks or prying tools for sealed areas

**Physical Capability:**
Health and stamina thresholds for exertion:
- Sufficient stamina for climbing, moving debris, sustained effort
- Adequate health to withstand minor injuries
- Physical strength for forcing open stuck doors/panels

**Timing:**
When you investigate matters:
- Time of day affects visibility and witnesses
- Weather conditions change accessibility
- NPC schedules determine who might observe

### Investigation Rewards

**Knowledge:**
Information valuable for:
- Understanding location history and significance
- Connecting to NPC interests and concerns
- Revealing related mysteries or locations
- Providing conversation advantages (observation cards)
- Opening new investigation opportunities

**Physical Items:**
Discovered objects with practical use:
- Equipment useful for future challenges
- Trade goods with value to specific NPCs
- Documents proving claims or revealing secrets
- Keys or permits enabling access elsewhere
- Tools that enable new investigation approaches

**Access:**
Successful investigation might unlock:
- Safe routes through dangerous locations
- Hidden paths connecting areas
- Alternative approaches to challenges
- Trust with NPCs impressed by discoveries
- Deeper investigation layers previously hidden

## Travel System

### Core Concept

Travel presents real obstacles requiring preparation, equipment, knowledge, and careful choices. Not just stamina/time costs.

### Route Structure

**Visible Paths:**
When approaching travel between locations, visible options show:
- Basic description of path type
- General difficulty indication
- Obvious requirements (if any)

**Hidden Paths:**
Additional routes revealed through:
- NPC conversations providing knowledge
- Investigation discoveries finding hidden ways
- Relationship trust sharing local secrets
- Previous travel experience noting alternatives

### Travel Obstacles

**Terrain Challenges:**
Natural barriers requiring preparation:
- Dense forest (navigation difficulty, stamina cost)
- Water crossings (risk of exposure, equipment loss)
- Steep inclines (high stamina requirement, injury risk)
- Poor visibility (getting lost, missing safer paths)
- Distance (extended exposure to elements)

**Environmental Hazards:**
Weather and conditions affecting travel:
- Rain making paths slippery or impassable
- Cold requiring warm clothing
- Darkness limiting visibility and safety
- Time of day affecting safety and witnesses

**Equipment Requirements:**
Some paths require specific gear:
- Rope for difficult climbs
- Proper footwear for rough terrain
- Weather protection for exposure
- Light sources for darkness
- Tools for clearing obstacles

**Knowledge Gates:**
Routes fully navigable only with information:
- Hidden shortcuts requiring directions
- Dangerous sections avoidable with warnings
- Optimal timing known from local knowledge
- Safe camping spots for long journeys
- Alternate routes when primary blocked

### Travel Choices

Each obstacle presents options with different trade-offs:

**Force Through:**
- High stamina/health cost
- Risk of injury or getting lost
- Faster if successful
- Learn what preparation needed for next time

**Careful Approach:**
- Lower risk but more time
- Better chance of success
- Might discover information for future trips
- Safer with limited resources

**Alternate Route:**
- Avoid obstacle entirely
- Usually longer distance
- Might have own challenges
- Sometimes requires different preparation

**Turn Back:**
- Admit current preparation insufficient
- Preserve resources for better attempt
- Learn what's needed for success
- Can return when properly equipped

### Travel Preparation

**Before attempting challenging routes:**

Gather information:
- Talk to NPCs who know the area
- Learn about hazards and requirements
- Discover hidden paths or shortcuts
- Understand weather factors

Acquire equipment:
- Purchase or craft needed tools
- Ensure adequate supplies
- Prepare for specific challenges
- Consider backup equipment

Build capability:
- Ensure sufficient health/stamina
- Rest if depleted from previous activities
- Time departure for optimal conditions
- Plan for recovery after arrival

## Time & Resource Economy

### Time Structure

**Blocks and Segments:**
- Day contains 6 blocks (Morning, Midday, Afternoon, Evening, Night, Late Night)
- Each block contains 4 segments
- Total: 24 segments per day

**Block Properties:**
- Morning: Fresh start, NPCs in regular locations, quiet investigations
- Midday: Full activity, busy locations, normal operations
- Afternoon: Winding down, some locations closing, changing availability
- Evening: Social time, taverns busy, offices closed, different NPC locations
- Night: Most locations closed, investigation opportunities, travel risks
- Late Night: Sleep recommended, exhaustion penalties, emergency only

**Activity Costs:**
- Conversations: 1 segment + accumulated doubt
- Work: 4 segments (entire block)
- Investigation: 1-4 segments depending on depth
- Travel: Variable by route and obstacles
- Rest: Typically full block for recovery

### Resource Management

**Health:**
- Maximum 100
- Lost to: Physical hazards, environmental exposure, injuries from failures
- Recovered through: Rest (blocks spent sleeping/recuperating), medical treatment, food with restorative properties
- Critical threshold: Below 30 increases danger of further harm

**Stamina:**
- Maximum 100
- Lost to: Physical exertion (travel, investigation, labor)
- Recovered through: Rest blocks, food, reduced activity
- Depletion effects: Cannot attempt stamina-requiring activities when insufficient

**Hunger:**
- Increases 20 per block
- Maximum 100
- Effects: At 75+ movement slowed, work efficiency reduced, stamina recovery impaired
- Management: Food costs coins, weighs capacity, must be carried or purchased at locations

**Coins:**
- Earned through: Completed deliveries, work, trade, discoveries
- Spent on: Food, equipment, route fees (bribes, transport), supplies, information
- Balance: Must work for income vs. spend time on investigation/relationships

### Weight Capacity

**Satchel Maximum: 10**

Must balance:
- Obligations accepted (letters 1, packages 1-3, goods 3-6)
- Equipment carried (tools 1-3, supplies 1-2 each)
- Food reserves (1 per meal)
- Discovered items (variable 1-4)

Trade-offs:
- Accept profitable heavy obligation, can't carry much equipment
- Carry full investigation gear, limited obligation capacity
- Stock food for travel, less room for discoveries
- Travel light for exploration, vulnerable without supplies

## Conversation System

### Five Core Resources

**Initiative** (Action Economy):
- Starts at 0 each conversation
- Must be generated through Foundation cards
- Spent to play higher-cost cards
- Persists between LISTEN actions
- Creates builder/spender dynamic

**Momentum** (Progress Track):
- Starts at 0
- Built toward goal thresholds (8, 12, 16)
- Can be consumed by card effects
- Primary victory condition
- Represents conversation progress

**Doubt** (Timer):
- Starts at 0
- Increases through effects and Cadence
- Conversation ends at 10
- Creates urgency
- Forces efficiency and listening

**Cadence** (Balance):
- Starts at 0, range -5 to +5
- SPEAK action: +1
- LISTEN action: -2
- High Cadence: +1 Doubt per point on LISTEN
- Low Cadence: +1 card draw per point on LISTEN
- Rewards strategic listening

**Statements in Spoken** (History):
- Count of Statement cards played
- Scales card effects
- Enables requirements
- Determines time cost (1 segment + statements)
- Conversation has memory

### Card Structure

Every card has:
- **Initiative Cost**: 0 for Foundation (depth 1-2), scales with depth
- **Either** requirement OR cost, never both
- **One deterministic effect**: No branching or randomness
- **Stat binding**: Which stat gains XP when played
- **Persistence**: Statement (stays in Spoken) or Echo (returns to deck)

### Stat-Gated Depth Access

Stats determine card depth access:
- Stat Level 1: Access depths 1-2 (Foundation only)
- Stat Level 2: Access depths 1-3
- Stat Level 3: Access depths 1-4 (Standard cards)
- Stat Level 5: Access depths 1-6 (Advanced)
- Stat Level 7: Access depths 1-8 (Powerful)
- Stat Level 9+: Access depths 1-10 (Master)

Progression represents growing conversational competence and expanded repertoire.

### Conversation Types

Each conversation type has fixed deck appropriate to context:

**Friendly Chat**: Casual relationship building
**Desperate Request**: Empathy and support (no authority cards)
**Trade Negotiation**: Business and deals (no rapport cards)
**Information Gathering**: Questions and investigation
**Authority Challenge**: Confrontation and power

Same NPC uses different decks based on situation. Marcus at shop: Trade. Marcus desperate: Request. This maintains verisimilitude.

## Three-Layer Content Architecture

### Mechanical Layer (Authoritative)

All deterministic systems:
- Card costs, effects, requirements
- Resource calculations and formulas
- Investigation obstacle requirements
- Travel danger thresholds
- Equipment functionality
- Time segment costs
- Weight constraints

**Authority:** Mechanics determine what's possible. AI cannot override.

### Authored Layer (Structural, Sparse)

Memorable moments identical for all players:
- NPC introduction scenes (first meeting)
- Location first-visit vignettes (atmospheric establishment)
- Relationship milestone scenes (at relationship thresholds)
- High-difficulty investigation discoveries (lore payoffs)

**Purpose:** Create identity, establish tone, provide structural peaks in slice-of-life flow.

**Constraint:** No branching. Same content for everyone. Happens based on triggers (first visit, relationship level, calendar date).

### AI Flavor Layer (Contextual, Pervasive)

Dynamic narration adapting to exact game state:
- All dialogue reflecting relationship history
- Card text showing conversation context
- Investigation descriptions building on knowledge
- Travel narration noting preparation and conditions
- Work scenes showing world continuity
- NPC reactions to player reputation and past actions
- Location descriptions deepening with familiarity

**Constraint:** AI flavors mechanics, never invents consequences. Provides context and atmosphere for deterministic outcomes.

## Integration Principles

### Mechanics Express Narrative

System states have narrative meaning:
- High Cadence = dominating conversation
- Burden cards = damaged relationship
- Investigation progress = location expertise
- Equipment owned = preparedness
- NPC knowledge = trusted advisor

Players read story through mechanical state.

### Systems Create Impossible Choices

Every decision has meaningful trade-offs:
- Accept lucrative heavy obligation OR maintain equipment capacity
- Investigate thoroughly OR meet deadline
- Spend coins on equipment OR save for emergency
- Help desperate NPC OR preserve other relationships
- Optimize efficiency OR explore thoroughly
- Work for income OR develop relationships/investigations

No obvious correct choice. Character revealed through priorities.

### Resources Serve Challenges

Not abstract optimization:
- Coins buy equipment for specific investigation
- Stamina enables difficult travel route
- Knowledge reveals safer path
- Relationships provide critical information
- Equipment allows accessing investigation layers
- Time permits thorough preparation

Every resource has concrete purpose toward adventure goals.

### Preparation Matters

Success comes from planning:
- Gather information before attempting challenge
- Acquire appropriate equipment
- Build sufficient capability (health/stamina)
- Choose optimal timing
- Understand risks and backup plans

Grinding serves preparation, not arbitrary gates.

### Failure Teaches

Unsuccessful attempts provide learning:
- Discover what equipment needed
- Learn hazard locations and types
- Understand timing requirements
- Identify knowledge gaps
- Reveal hidden paths or approaches

State persistence means failed attempts aren't wasted - you return better prepared.

## Slice-of-Life Structure

### Frieren Model

**Core Elements:**
- No endgame or climax
- Relationships deepen through accumulated moments
- World exists independent of player
- Discovery is intrinsic reward
- Small personal scale
- Time creates meaning through persistence

**Mechanical Expression:**
- Open-ended progression (no level cap or story end)
- Gradual relationship building (not binary quest completion)
- NPCs have ongoing lives (requests reflect their activities)
- Investigations reward curiosity (not just mechanical benefit)
- Daily routine creates comfort (familiar rhythms)
- Authored milestones mark passage (but not conclusion)

### Daily Rhythm

**Morning:**
- Fresh start, full resources
- Quiet investigations (better conditions)
- Regular NPC locations
- Planning for day ahead

**Midday/Afternoon:**
- Active challenges (travel, difficult investigations)
- Business and trade
- Deliveries and obligations
- Resource expenditure

**Evening:**
- Social time (different conversation opportunities)
- Rest and recovery
- Reflection on progress
- Preparation for tomorrow

**Night:**
- Special investigations (when locations empty)
- Emergency travel if needed
- Sleep and full recovery
- Day boundary

This creates natural pacing without artificial urgency.

### Relationship Depth

Bonds form through time spent together:
- Multiple conversations over days/weeks
- Helping with varied requests
- Sharing discovered knowledge
- Overcoming challenges cooperatively
- Witnessing NPC ongoing lives

Authored milestones mark progress but bulk of relationship is emergent interaction.

### World Independence

NPCs have lives beyond player:
- Requests reflect their actual needs
- Conversations reference their activities
- World events proceed regardless
- Player is participant, not center

This maintains immersion and verisimilitude.

## Content Scalability

### Package-Based Loading

Content structured as modular packages:
- Core game foundation
- Expansion locations and NPCs
- AI-generated contextual content

### Skeleton System

Missing references create functional placeholders:
- Mechanically complete
- Narratively generic
- Replaced when real content loads
- Enables AI procedural generation

### AI Content Generation

AI creates packages filling gaps:
- New NPCs with proper deck structure
- Additional locations with investigations
- Route connections and obstacles
- Observation rewards and discoveries
- Always follows mechanical templates

## Critical Design Constraints

### Verisimilitude Requirements

**World Building:**
- Low fantasy, historical setting
- Small personal scale (village/town area)
- Walking distances (hours not days)
- Actions span seconds/minutes
- No modern concepts
- Grounded dangers (nature, exposure, injury, social)

**Not Included:**
- Epic scope or destiny
- Modern equipment (crowbars, etc.)
- Large-scale travel (mountain passes)
- Abstract checkpoints/guards
- Gamey mechanics without world logic

### Mechanical Clarity

**Always Visible:**
- All calculations shown to player
- Resource costs before commitment
- Success/failure conditions clear
- No hidden randomness
- Perfect information gameplay

**Always Deterministic:**
- Card effects work exactly as stated
- Requirements either met or not
- No probability beyond stated percentages
- Player skill determines outcomes

### No Soft-Lock

**Always Possible:**
- Earn coins through work
- Progress relationships with any NPC
- Access locations through alternatives
- Recover from resource depletion
- Continue after failures

**Consequences Not Blocks:**
- Mistakes create friction not walls
- Failures add difficulty not impossibility
- Bad choices have costs not game-overs
- Learning enables better future attempts

## Verification Questions

For any new mechanic, ask:

1. **Does it serve exactly one purpose?** (No multi-function systems)
2. **Does it make narrative sense?** (Verisimilitude check)
3. **Is it fully visible to player?** (Perfect information)
4. **Is it deterministic?** (No hidden randomness)
5. **Does it create impossible choices?** (Meaningful trade-offs)
6. **Can it soft-lock the player?** (If yes, redesign)
7. **Does it pull toward adventure or away?** (Integration check)
8. **Is it grounded in world logic?** (No gamey abstractions)
9. **Does AI need mechanical authority?** (If yes, redesign)
10. **Would it work in a D&D session?** (Scale and pacing check)

## Conclusion

Wayfarer achieves integration of visual novel and simulation through:

**Mechanical Foundation:** Elegant systems creating strategic depth through impossible choices and resource constraints.

**Authored Structure:** Sparse memorable moments establishing identity and marking progress without branching complexity.

**AI Contextualization:** Pervasive responsive narration making mechanical choices feel personal and meaningful.

The result: Slice-of-life adventure where character emerges through decisions under constraint, relationships deepen through time and shared challenges, and world reveals itself to curious preparation and persistent effort.

Player versus nature, small personal scale, grounded in verisimilitude, expressed through text and choice.
