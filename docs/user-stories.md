# Wayfarer User Stories - Complete Implementation Requirements

## Epic 1: Core Game Loop and Time Management

### US-1.1: Game Timeline
**As a** player  
**I want to** play through a 30-day narrative arc  
**So that** my choices create a complete story with meaningful ending

**Acceptance Criteria:**
- Game tracks current day (1-30) and time of day
- Each day divided into four time blocks: Morning (6AM-12PM), Afternoon (12PM-6PM), Evening (6PM-10PM), Night (10PM-12AM)
- Time advances only through player actions (conversations, travel, delivery)
- After day 30, ending is generated based on relationship configuration
- Player can continue in "endless mode" after ending but main story concludes

### US-1.2: Time Advancement
**As a** player  
**I want** time to advance predictably through my actions  
**So that** I can plan routes and manage deadlines strategically

**Acceptance Criteria:**
- Starting conversation: +15 minutes
- Travel between adjacent districts: +30 minutes (walking)
- Travel across city: +60 minutes (walking)
- Delivering letter: +15 minutes
- Premium transport reduces travel time (cart: 50%, carriage: to 10 minutes flat)
- All letter deadlines count down when time advances
- NPCs transition availability at hour marks with 15-minute grace periods

### US-1.3: Daily Attention Budget
**As a** player  
**I want** limited attention per time block  
**So that** I must prioritize which actions to take

**Acceptance Criteria:**
- Morning: 5 attention points
- Afternoon: 5 attention points
- Evening: 3 attention points
- Night: 2 attention points
- Starting conversation costs 1 attention
- Observing location costs 1 attention
- Special actions cost 2-3 attention
- Can refresh attention with coins (once per time block)
- Attention resets at time block transitions

## Epic 2: NPC and Relationship Systems

### US-2.1: NPC Personalities
**As a** player  
**I want** NPCs with distinct personalities  
**So that** each relationship feels unique

**Acceptance Criteria:**
- Five personality types: DEVOTED, MERCANTILE, PROUD, CUNNING, STEADFAST
- Personality determines base patience (3-10)
- Personality affects availability schedule
- Personality influences which letter types NPC typically offers
- Personality never changes during game

### US-2.2: Four-Dimensional Relationships
**As a** player  
**I want to** build four types of relationships with each NPC  
**So that** I can develop complex, multifaceted connections

**Acceptance Criteria:**
- Each NPC tracks Trust, Commerce, Status, and Shadow tokens (0-10 scale)
- Tokens are permanent memory cards representing shared experiences
- Cannot trade or spend tokens (they're relationship history)
- Each token type provides distinct mechanical benefits
- All four dimensions exist simultaneously for every NPC

### US-2.3: Relationship Effects on Conversations
**As a** player  
**I want** relationships to affect conversations mechanically  
**So that** deeper relationships enable deeper interactions

**Acceptance Criteria:**
- Trust adds to starting patience (more conversation depth)
- Commerce reduces all card patience costs by 1 (minimum 1)
- Status reduces all card difficulties by 1
- Shadow 3+ allows playing cards without meeting requirements
- Effects stack when multiple relationships exist

### US-2.4: Emotional States
**As a** player  
**I want** NPCs to have emotional states based on letter deadlines  
**So that** urgency affects all interactions

**Acceptance Criteria:**
- DESPERATE: Letter has <6 hours remaining (patience -3)
- ANXIOUS: Letter has 6-12 hours remaining (patience -1)
- CALCULATING: Letter has >12 hours remaining (no modifier)
- HOSTILE: Letter expired (cannot converse until resolved)
- NEUTRAL: No active letter
- States change instantly when deadline thresholds crossed

## Epic 3: Conversation Deck-Building System

### US-3.1: NPC Conversation Decks
**As a** player  
**I want** each NPC to have their own deck of conversation cards  
**So that** relationships evolve through shared experiences

**Acceptance Criteria:**
- Each NPC has unique deck starting with 10 cards
- Deck can grow to maximum 20 cards
- Cards cannot be transferred between NPCs
- Cards cannot be purchased (only earned through experiences)
- Each card represents specific shared history

### US-3.2: Card Properties
**As a** player  
**I want** cards with clear mechanical properties  
**So that** I can make strategic decisions

**Acceptance Criteria:**
- Each card has: Name, Requirements, Difficulty (0-10), Patience Cost, Outcomes
- Three outcomes: Success (66%+ roll), Neutral (33-65%), Failure (0-32%)
- Success chance = (Patience - Difficulty + 5) × 12%
- Negative cards exist and create conversation challenges
- Some cards have requirements (relationship levels, previous cards played)

### US-3.3: Conversation Flow
**As a** player  
**I want to** play cards strategically during conversations  
**So that** I can achieve comfort thresholds

**Acceptance Criteria:**
- Draw exactly 5 cards each round
- Check requirements, display only qualifying cards
- "Exit" always available as option
- Pay patience cost to play card
- Roll for outcome based on current patience
- Comfort starts at 0, build through card play
- Conversation ends when patience reaches 0 or player exits

### US-3.4: Comfort Thresholds
**As a** player  
**I want** clear comfort goals in conversations  
**So that** I know what I'm trying to achieve

**Acceptance Criteria:**
- Comfort ≥ Patience/2: Maintain relationship
- Comfort ≥ Patience: Letter becomes available
- Comfort ≥ Patience × 1.5: Perfect conversation bonus
- Perfect conversations neutralize one negative card
- Thresholds scale with starting patience

### US-3.5: Adding Cards Through Letter Delivery
**As a** player  
**I want to** improve NPC decks by delivering letters  
**So that** future conversations become richer

**Acceptance Criteria:**
- Successful delivery: Choose to add card, upgrade card, or neutralize negative
- Card type matches letter type (Trust letter → Trust card)
- Failed delivery forces negative card into deck
- Late delivery adds different negative than failure
- Cards represent relationship memories, not generic abilities

## Epic 4: Letter Queue Management System

### US-4.1: Queue Structure
**As a** player  
**I want** a limited queue for letters  
**So that** I must prioritize which obligations to accept

**Acceptance Criteria:**
- Queue has 8 positions (slots)
- Queue has 12 total weight capacity
- Each letter has weight 1-3 based on complexity
- Only position 1 can be delivered
- Cannot exceed weight capacity
- Must manage both position and weight constraints

### US-4.2: Queue Entry Position
**As a** player  
**I want** letter position determined by relationships  
**So that** my history with NPCs affects urgency

**Acceptance Criteria:**
- Base position when accepting letter: position 4
- Any negative relationship (-3 or worse): position 2 (leverage)
- Positive relationship 3-4: position 5
- Positive relationship 5+: position 6
- Active obligation: position 1 (absolute priority)
- Negative relationships override positive for position

### US-4.3: Letter Displacement
**As a** player  
**I want to** reorder letters at a cost  
**So that** queue management requires strategic decisions

**Acceptance Criteria:**
- Moving letter up X positions costs X tokens with displaced NPC
- Token type burned matches displaced letter's type
- Cannot move if insufficient tokens
- Shows preview of token cost before confirmation
- Displaced letter moves down one position
- Can cascade multiple displacements

### US-4.4: Queue Overflow
**As a** player  
**I want** clear choices when queue is full  
**So that** accepting new letters requires sacrifice

**Acceptance Criteria:**
- When accepting would exceed weight capacity, choose:
- Option 1: Abandon existing letter (with full penalties)
- Option 2: Refuse new letter (with refusal penalties)
- Abandoned letters are lost forever
- Shows consequences before confirmation

## Epic 5: Letter Generation and Properties

### US-5.1: Letter Generation Through Conversations
**As a** player  
**I want** letters to emerge from successful conversations  
**So that** relationship building creates obligations

**Acceptance Criteria:**
- Reach comfort ≥ patience threshold to unlock letter cards
- Letter cards added to deck after first success
- Playing letter card successfully generates actual letter
- Letter type matches highest relationship (Trust/Commerce/Status/Shadow)
- Failed letter card play means try again next conversation

### US-5.2: Letter Properties
**As a** player  
**I want** letters with clear properties  
**So that** I can plan delivery strategies

**Acceptance Criteria:**
- Sender: NPC who offered letter
- Recipient: Destination NPC
- Type: Matches relationship type
- Deadline: Hours until expiration
- Weight: 1-3 based on tier
- Tier: Based on relationship depth (0-2: T1, 3-5: T2, 6+: T3)
- Stakes: Narrative description of importance

### US-5.3: Letter Acceptance
**As a** player  
**I want to** accept letters through conversation  
**So that** helping NPCs feels personal

**Acceptance Criteria:**
- "How Are Things?" card reveals letter need
- Letter request card shows all properties before acceptance
- Accepting enters letter at calculated queue position
- Can refuse with relationship penalties based on emotional state
- DESPERATE refusal: -3 tokens
- ANXIOUS refusal: -1 token

### US-5.4: Letter Delivery
**As a** player  
**I want to** deliver letters through conversations  
**So that** completion feels meaningful

**Acceptance Criteria:**
- Travel to recipient location
- Start conversation with recipient (1 attention)
- Letter in position 1 automatically creates delivery option
- Choose reward: Add card, upgrade card, or neutralize negative
- Gain coins based on tier and Commerce multiplier
- Emotional state resets for sender

## Epic 6: Travel and Route System

### US-6.1: Location Network
**As a** player  
**I want** interconnected locations  
**So that** routing becomes strategic

**Acceptance Criteria:**
- Five main districts: Common Quarter, Merchant Quarter, Noble District, Castle, Shadow Alleys
- Each district has multiple specific locations
- Districts connected by defined routes
- Each route has distance and available transport types
- Some routes locked initially, requiring permits or relationships

### US-6.2: Transport Types
**As a** player  
**I want** different transport options  
**So that** I can trade coins for time

**Acceptance Criteria:**
- Walking: Always available, free, base time cost
- Cart: Costs 2 coins, reduces time by 50%
- Carriage: Costs 5 coins, reduces to 10 minutes flat
- Special routes may have unique transport requirements
- Transport availability can depend on time of day
- Some transports unlocked through relationships or permits

### US-6.3: Route Unlocking
**As a** player  
**I want to** unlock new routes through gameplay  
**So that** the city opens gradually

**Acceptance Criteria:**
- Base routes available from start
- Permit letters permanently unlock specific routes when delivered
- High Status relationships unlock noble routes
- Shadow relationships unlock hidden passages
- Some routes only available at certain times
- Unlocked routes remain permanently available

### US-6.4: NPC Availability at Locations
**As a** player  
**I want** NPCs to have logical schedules  
**So that** finding them requires planning

**Acceptance Criteria:**
- NPCs present at specific locations based on personality
- DEVOTED: Morning and evening at various locations
- MERCANTILE: All day except night at markets
- PROUD: Afternoon at noble venues
- CUNNING: Evening and night at hidden locations
- Can extend availability with coins (5 coins = 1 hour)

## Epic 7: Special Letters and Unlocking

### US-7.1: Permit Letters
**As a** player  
**I want** special letters that unlock routes  
**So that** delivering certain letters expands possibilities

**Acceptance Criteria:**
- Permit letters occupy queue slots like normal letters
- No deadline but take up weight
- Delivering to specific official unlocks associated route
- Routes unlocked permanently
- Shows which route will unlock before acceptance
- Can choose to use for access or sell for coins

### US-7.2: Introduction Letters
**As a** player  
**I want** letters that introduce new NPCs  
**So that** my social network expands through connections

**Acceptance Criteria:**
- Introduction letters unlock conversation with new NPCs
- Must deliver to introduce two NPCs to each other
- Unlocked NPC becomes permanently available
- New NPC starts with neutral relationship
- Shows which NPC will be unlocked
- Some NPCs only accessible through introductions

### US-7.3: Information Letters
**As a** player  
**I want** letters containing valuable information  
**So that** I can learn secrets and opportunities

**Acceptance Criteria:**
- Information letters reveal hidden mechanics or opportunities
- Can choose to deliver for reward or read for knowledge
- Reading destroys letter but provides strategic advantage
- Information might reveal NPC schedules, hidden routes, or context
- Creates strategic choice between immediate knowledge and delivery reward

## Epic 8: Obligations System

### US-8.1: Creating Obligations
**As a** player  
**I want to** make binding promises during crises  
**So that** I can help desperate NPCs at a cost

**Acceptance Criteria:**
- Crisis cards appear only when NPC is DESPERATE
- Each obligation type has specific card and requirements
- Success creates obligation and gives +3 tokens immediately
- Failure damages relationship (-2 tokens)
- Each NPC can have maximum one active obligation
- Obligation persists until fulfilled or broken

### US-8.2: Obligation Effects
**As a** player  
**I want** obligations to modify game rules  
**So that** promises have mechanical weight

**Acceptance Criteria:**
- Trust obligation: Their letters enter at position 1
- Commerce obligation: Their letters enter at position 2
- Status obligation: Must deliver on time or lose -5 Status
- Shadow obligation: Cannot refuse their letters
- Obligations override normal queue position rules
- Shows active obligations in UI at all times

### US-8.3: Breaking Obligations
**As a** player  
**I want** serious consequences for breaking promises  
**So that** obligations feel meaningful

**Acceptance Criteria:**
- Breaking obligation costs -5 tokens of obligation type
- Positive card replaced with "Betrayal" in their deck
- NPC becomes HOSTILE until resolved
- Future letters enter at worst positions
- Must complete perfect conversation to restore
- Shows warning before breaking obligation

## Epic 9: Resource Management

### US-9.1: Coin Economy
**As a** player  
**I want to** earn and spend coins strategically  
**So that** money provides tactical flexibility

**Acceptance Criteria:**
- Earn 2-20 coins per letter based on tier
- Commerce relationship multiplies rewards from that NPC only
- Commerce 5: 200% coins, Commerce 3: 150%, Commerce -3: 50%
- Can buy: Food/drink (attention refresh), Transport, Emergency courier
- Cannot buy: Cards, relationships, or conversation advantages
- Coins provide services, not relationship improvements

### US-9.2: Attention Refresh
**As a** player  
**I want to** refresh attention with coins  
**So that** I can extend productive time blocks

**Acceptance Criteria:**
- Quick drink: 1 coin = +1 attention
- Full meal: 3 coins = +2 attention
- Maximum once per time block
- Can exceed normal maximum (e.g., 7/5 possible)
- Must be at appropriate location (tavern, inn)
- Shows current and maximum attention

### US-9.3: Token Tracking
**As a** player  
**I want to** see all relationship tokens clearly  
**So that** I understand my connections

**Acceptance Criteria:**
- Display all four token types for current NPC
- Show range 0-10 for each type
- Indicate mechanical effects of current levels
- Tokens are permanent, cannot be traded or spent
- Negative tokens shown clearly with consequences
- Total Status across all NPCs shown for access requirements

## Epic 10: Observation System

### US-10.1: Location Observation
**As a** player  
**I want to** observe locations before acting  
**So that** I can make informed decisions

**Acceptance Criteria:**
- Costs 1 attention per observation
- Reveals all NPCs present at location
- Shows emotional states of present NPCs
- Indicates current patience levels
- Shows if letter needs available
- Information remains valid until time advances

### US-10.2: Travel Observation
**As a** player  
**I want to** observe during travel  
**So that** I can find opportunities

**Acceptance Criteria:**
- Can observe while traveling for additional attention
- Might reveal shortcuts reducing travel time
- Can spot NPCs in transit
- May discover temporary events
- Creates risk/reward for attention spending

### US-10.3: Peripheral Awareness
**As a** player  
**I want** awareness during conversations  
**So that** I notice environmental changes

**Acceptance Criteria:**
- While conversing, peripheral information visible
- Shows other NPCs arriving/leaving
- Indicates urgent deadlines in corner
- Can spend attention to investigate periphery
- Creates tension between focus and awareness
- Important events highlighted but require attention to pursue

## Epic 11: Game Progression and Endings

### US-11.1: Relationship Progression
**As a** player  
**I want** relationships to deepen through play  
**So that** I see character growth

**Acceptance Criteria:**
- Tier 1 letters available at relationship 0-2
- Tier 2 letters available at relationship 3-5
- Tier 3 letters available at relationship 6+
- Higher tiers have better rewards but harder requirements
- Each delivery can improve relationship by 1-3 tokens
- Perfect conversations can give bonus tokens

### US-11.2: Story Endings
**As a** player  
**I want** different endings based on relationships  
**So that** my choices create unique narratives

**Acceptance Criteria:**
- After 30 days, generate ending based on final relationships
- Multiple ending types: Community, Economic, Noble, Shadow, Balanced, Specialist
- Ending determined by pattern of relationships, not just totals
- Shows relationship summary before ending
- Can continue playing after ending in endless mode
- Each ending has unique narrative generation

### US-11.3: Daily Progression
**As a** player  
**I want** each day to feel different  
**So that** the 30-day arc has variety

**Acceptance Criteria:**
- Different NPCs available based on day of week
- Letter complexity increases over time
- Crises become more frequent in later days
- Relationships affect what letters appear
- Special events on certain days
- Deadline pressure increases toward end

## Epic 12: AI Narrative Generation

### US-12.1: Dynamic Dialogue
**As a** player  
**I want** unique dialogue for each interaction  
**So that** conversations feel natural

**Acceptance Criteria:**
- AI receives full mechanical state for each interaction
- Generates appropriate dialogue based on emotional state
- Reflects all four relationship dimensions in speech
- Varies based on location and time
- Remembers recent interactions for continuity
- Never repeats exact dialogue

### US-12.2: Letter Descriptions
**As a** player  
**I want** meaningful letter narratives  
**So that** deliveries feel important

**Acceptance Criteria:**
- Each letter has generated description based on type and stakes
- Urgent letters reflect desperation in description
- Commerce letters describe business details
- Trust letters convey personal matters
- Narrative matches sender personality
- Description hints at consequences

### US-12.3: Environmental Narrative
**As a** player  
**I want** rich location descriptions  
**So that** the world feels alive

**Acceptance Criteria:**
- Locations described based on time of day
- Crowd and atmosphere vary by context
- Weather and seasons affect descriptions
- NPCs have ambient activities when not conversing
- Travel descriptions show city life
- Peripheral events described naturally

### US-12.4: Card Narrative
**As a** player  
**I want** cards to show our history  
**So that** relationships feel real

**Acceptance Criteria:**
- Each card has narrative description of the memory
- Negative cards show what went wrong
- Upgraded cards reflect relationship growth
- Card descriptions reference specific past events
- Obligation cards show the promise made
- Perfect conversation neutralization shows resolution

## Epic 13: Tutorial and Onboarding

### US-13.1: Opening Scenario
**As a** player  
**I want** the opening to teach through play  
**So that** I learn naturally

**Acceptance Criteria:**
- Start with one letter already in queue (Lord Blackwood)
- Elena has urgent need demonstrating emotional states
- Commerce debt with Elena shows leverage mechanics
- Three NPCs available showing different personalities
- Clear immediate dilemma requiring queue decision
- All core mechanics demonstrated in first morning

### US-13.2: Progressive Complexity
**As a** player  
**I want** mechanics introduced gradually  
**So that** I'm not overwhelmed

**Acceptance Criteria:**
- Day 1-3: Basic conversations and simple letters
- Day 4-7: Multiple letters and queue management
- Day 8-12: Obligations and complex routes
- Day 13-20: Full complexity with all systems
- Day 21-30: Endgame with storyline culmination
- Each phase introduces new mechanics naturally

## Epic 14: Save System and Progression

### US-14.1: Save Game State
**As a** player  
**I want to** save and resume my game  
**So that** I can play across multiple sessions

**Acceptance Criteria:**
- Save current day and time
- Save all NPC relationships and emotional states
- Save complete deck composition for each NPC
- Save queue state and all letter properties
- Save unlocked routes and NPCs
- Save active obligations
- Multiple save slots available

### US-14.2: New Game Plus
**As a** player  
**I want to** start fresh with some benefits  
**So that** I can explore different strategies

**Acceptance Criteria:**
- After completing ending, unlock new game plus
- Can start with different initial relationships
- Some routes remain unlocked
- Can select different starting district
- Previous playthrough statistics visible
- Achievement progress carries over

## Epic 15: UI and Feedback Systems

### US-15.1: Conversation Interface
**As a** player  
**I want** clear conversation UI  
**So that** I understand my options

**Acceptance Criteria:**
- Current patience displayed prominently
- Comfort progress bar visible
- Card requirements clearly shown
- Success percentage displayed before playing
- Exit always available
- Token changes preview on hover

### US-15.2: Queue Visualization
**As a** player  
**I want to** see my queue clearly  
**So that** I can plan deliveries

**Acceptance Criteria:**
- All 8 positions visible
- Weight capacity shown (current/max)
- Deadlines display with urgency colors
- Displacement cost preview on drag
- Letter details on hover
- Sender emotional state indicated

### US-15.3: City Map
**As a** player  
**I want** an interactive city map  
**So that** I can plan routes

**Acceptance Criteria:**
- All districts visible with connections
- Current location highlighted
- NPC locations marked (if known)
- Transport options shown on routes
- Travel time displayed
- Locked routes shown differently

### US-15.4: Relationship Summary
**As a** player  
**I want to** track all relationships  
**So that** I can see my progress

**Acceptance Criteria:**
- Grid showing all NPCs and four token types
- Sort by any token type
- Filter by personality or location
- Show active letters and obligations
- Indicate emotional states
- Display last interaction time