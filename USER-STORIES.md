# Wayfarer: Letters and Ledgers - Complete User Stories

## Epic 1: Core Letter Queue System

### Story 1.1: Queue Display and Basic Mechanics
**As a** player  
**I want to** see my letter queue as 8 distinct slots numbered 1-8  
**So that** I understand my delivery obligations in priority order  

**Acceptance Criteria:**
- Queue displays 8 slots vertically with position 1 at top
- Empty slots are clearly visible
- Letters show: sender name, description, deadline, payment, token type
- Position 1 is visually emphasized as "must deliver next"
- Queue updates immediately when letters are added/removed

### Story 1.2: Letter Delivery from Position 1
**As a** player  
**I want to** only be able to deliver the letter in position 1  
**So that** I must respect the queue order or pay social costs  

**Acceptance Criteria:**
- Deliver action only available for position 1 letter when at recipient location
- Attempting to deliver other positions shows token cost via ConversationManager
- Successful delivery removes letter and shifts all below up one position
- Uncollected letters cannot be delivered (show "must collect first")
- Delivery triggers conversation with recipient NPC

### Story 1.3: Token Burning for Queue Skipping (Conversation Action)
**As a** player  
**I want to** skip queue order by burning connection tokens  
**So that** I can prioritize urgent deliveries at social cost  

**Acceptance Criteria:**
- Selecting non-position-1 letter triggers ConversationManager
- Conversation shows exact token cost (1 per skipped sender)
- Player chooses: "Skip and deliver" or "Respect queue order"
- Skip choice burns tokens with specific NPCs
- Cannot skip if insufficient tokens (choice disabled)
- Skip action delivers letter immediately after token payment

### Story 1.4: Queue Shifting on Delivery
**As a** player  
**I want** letters to automatically move up when position 1 is delivered  
**So that** the queue naturally progresses  

**Acceptance Criteria:**
- On delivery, positions 2→1, 3→2, etc.
- Animation shows letters sliding up
- No gaps ever exist between positions
- If only 3 letters in queue, they occupy positions 1-3

## Epic 2: Letter Entry and Leverage System

### Story 2.1: Base Letter Entry Positions
**As a** player  
**I want** letters to enter at positions based on sender leverage  
**So that** social hierarchy affects my obligations  

**Acceptance Criteria:**
- Patron letters prefer position 1
- Noble letters prefer position 3
- Trade/Shadow letters prefer position 5
- Common/Trust letters prefer position 7
- Letters enter at lowest available position unless it conflicts with preference
- If preferred position occupied, letter forces entry and pushes others down

### Story 2.2: Token Debt System
**As a** player  
**I want to** go into token debt with NPCs  
**So that** accepting help has mechanical consequences  

**Acceptance Criteria:**
- Tokens can go negative (-1, -2, etc.)
- Negative tokens display as debt in UI
- Request actions available: "Borrow money" (-2 tokens), "Ask for help" (-1 token)
- Can go into debt from positive or zero
- Debt immediately affects letter entry positions

### Story 2.3: Token-Modified Leverage
**As a** player with varying token relationships  
**I want** token balance to affect letter entry positions  
**So that** my relationships change sender leverage  

**Acceptance Criteria:**
- Positive tokens (1-3): Normal position
- High tokens (4+): Position +1 (less leverage)
- Zero tokens: Normal position
- Negative tokens: Position -1 per negative token (more leverage)
- Example: Noble at -2 tokens enters at position 1 instead of 3
- Leverage position cannot go below 1 or above 8

### Story 2.4: Queue Overflow and Discard
**As a** player with a full queue  
**I want** high-leverage letters to force entry  
**So that** powerful senders can't be ignored  

**Acceptance Criteria:**
- When queue is full and letter forces entry above position 8
- Letter at position 8 is automatically discarded
- ConversationManager narrates the loss: "Your obligation to [NPC] falls by the wayside"
- No token penalty for forced discard
- Discarded letter's sender remembers this slight (affects future interactions)

## Epic 3: NPC Letter Offers Through Conversation

### Story 3.1: Letter Discovery Through Conversation
**As a** player  
**I want to** discover letter opportunities through NPC conversations  
**So that** relationships feel natural and contextual  

**Acceptance Criteria:**
- "Converse" action with NPC triggers ConversationManager
- Conversation reveals if NPC has letter need
- NPCs with 0 tokens only offer small talk
- NPCs with 1+ tokens may mention letter needs
- Player can accept or decline within conversation flow

### Story 3.2: Letter Acceptance Conversation
**As a** player offered a letter  
**I want** the conversation to show full details  
**So that** I can make an informed decision  

**Acceptance Criteria:**
- Conversation shows: destination, deadline, payment, size, special properties
- Shows current queue state and where letter would enter
- Warning if acceptance would discard position 8 letter
- Choice: "Accept letter" or "Politely decline"
- Acceptance adds to queue at calculated position

### Story 3.3: Token-Based Letter Categories
**As a** player building relationships  
**I want** better letters from stronger relationships  
**So that** investment in NPCs pays off  

**Acceptance Criteria:**
- 0 tokens: No letters offered
- 1-2 tokens: Basic letters (3-5 coins)
- 3-4 tokens: Quality letters (8-12 coins)
- 5+ tokens: Premium letters (15-20 coins)
- Letter quality shown in conversation
- Higher category letters may have better deadlines

## Epic 4: Physical Letter Management

### Story 4.1: Letter Collection Action
**As a** player with queued letters  
**I want to** collect physical letters from senders  
**So that** accepting and carrying are distinct  

**Acceptance Criteria:**
- Uncollected letters show "Not Collected" status in queue
- "Collect Letter" action available at sender's location
- Collection transfers to inventory if space available
- Shows required slots: Small (1), Medium (2), Large (3)
- Cannot collect if insufficient inventory space

### Story 4.2: Inventory Management Through Conversation
**As a** player with full inventory  
**I want** conversations to handle overflow  
**So that** I make strategic carrying decisions  

**Acceptance Criteria:**
- Collection with full inventory triggers ConversationManager
- Shows current inventory contents
- Choices: Drop items, leave letter uncollected, reorganize
- Visual inventory grid in conversation UI
- Items and letters share same 8 slots

### Story 4.3: Multi-Purpose Item Choices
**As a** player carrying deliverable items  
**I want** conversation choices for their use  
**So that** moral decisions emerge naturally  

**Acceptance Criteria:**
- Items with multiple uses trigger conversations when relevant
- Medicine during illness: "Use it yourself" vs "Save for delivery"
- Trade documents: "Read for information" vs "Deliver sealed"
- Conversation shows consequences of each choice
- Using delivery items damages relationship with sender

## Epic 5: Standing Obligations

### Story 5.1: Obligation Letter Warning
**As a** player  
**I want** clear warnings before accepting obligation letters  
**So that** I understand permanent consequences  

**Acceptance Criteria:**
- Special letters marked with obligation icon
- Accepting triggers ConversationManager warning
- Full explanation of obligation effects
- Explicit choice: "Accept obligation" vs "Refuse (burn tokens)"
- Cannot back out once accepted

### Story 5.2: Patron's Expectation (Starting Obligation)
**As a** player with a patron  
**I want** patron letters to have special priority  
**So that** my employment has mechanical weight  

**Acceptance Criteria:**
- "Patron's Expectation" obligation active from game start
- Patron letters always enter at position 1
- Pushes all other letters down
- Cannot refuse patron letters
- Breaking obligation requires burning all patron tokens

### Story 5.3: Obligation-Modified Queue Rules
**As a** player with obligations  
**I want** them to permanently alter queue behavior  
**So that** my choices reshape gameplay  

**Acceptance Criteria:**
- "Noble's Courtesy": Noble letters enter at 5, cannot refuse nobles
- "Shadow's Burden": Forced shadow letter every 3 days, cannot purge shadow
- "Merchant's Priority": Trade letters pay +10 coins, cannot move trade letters
- Modifications stack and can conflict
- UI shows active obligations and their effects

## Epic 6: Emergency Actions Through Conversation

### Story 6.1: Patron Fund Requests
**As a** player needing resources  
**I want to** request help from my patron  
**So that** desperation has consequences  

**Acceptance Criteria:**
- "Write to Patron" action available at desk/study locations
- ConversationManager handles request narrative
- Choices: Request funds (-1 token), Request equipment (-2 tokens), Cancel
- Resources arrive next morning if approved
- Each request increases patron leverage

### Story 6.2: NPC Debt Actions
**As a** player  
**I want to** borrow from NPCs when desperate  
**So that** local relationships become financial  

**Acceptance Criteria:**
- "Ask for loan" available with 1+ token NPCs
- Conversation negotiates terms
- Borrowing creates -2 token debt
- Repayment options in future conversations
- Debt affects letter entry positions immediately

### Story 6.3: Shadow Dealings
**As a** player  
**I want** illegal work options from shadow NPCs  
**So that** desperation can compromise morals  

**Acceptance Criteria:**
- Shadow NPCs offer illegal work when player is desperate
- Conversation emphasizes risks and moral weight
- Accepting creates -1 Shadow token (they have leverage)
- Illegal work pays triple but adds "Heat" status
- Heat affects future NPC interactions

## Epic 7: Route Access and Token Thresholds

### Story 7.1: Route Discovery Through NPCs
**As a** player building relationships  
**I want** NPCs to share route knowledge  
**So that** connections enable efficient travel  

**Acceptance Criteria:**
- NPCs with route knowledge show in conversations
- At 3+ tokens, can ask about shortcuts
- Learning route through conversation (not automatic)
- Routes show token requirements for access
- Losing tokens below threshold locks route

### Story 7.2: Route Access Warnings
**As a** player planning travel  
**I want** clear warnings about route requirements  
**So that** I can maintain critical relationships  

**Acceptance Criteria:**
- Route selection shows: "Requires 3+ Commerce tokens with Guide"
- Warning if current tokens exactly at threshold
- "Route locked" if below threshold
- Alternative routes always visible
- Time differences shown for comparison

## Epic 8: Delivery Conversations

### Story 8.1: Letter Delivery as Conversation
**As a** player delivering letters  
**I want** each delivery to be a full conversation with choices  
**So that** deliveries are narrative experiences, not transactions  

**Acceptance Criteria:**
- Delivery action triggers ConversationManager with recipient
- Conversation has multiple beats based on context
- Every delivery offers at least one meaningful choice
- Choices emerge from relationship, letter contents, timing, etc.
- ConversationManager determines narrative complexity

### Story 8.2: Delivery Choice Outcomes
**As a** player in delivery conversations  
**I want** choices that affect different rewards  
**So that** I can prioritize what matters to me  

**Acceptance Criteria:**
- Common choice types include but aren't limited to:
  - Accept token reward OR extra payment
  - Deliver privately OR publicly  
  - Share news from sender OR keep discrete
  - Accept return letter OR decline additional work
- All choices show clear mechanical outcomes
- Choices affect relationships, resources, or future opportunities
- No "correct" choice - just different priorities

### Story 8.3: Contextual Delivery Narratives
**As a** player  
**I want** delivery conversations to reflect context  
**So that** each delivery feels unique and grounded  

**Acceptance Criteria:**
- Late deliveries reflected in recipient's reaction
- Fragile items acknowledged if damaged/protected
- Valuable deliveries may prompt trust/suspicion
- Relationship history affects conversation tone
- Letter contents influence available choices
- All context provided to ConversationManager for narrative generation

### Story 8.4: Post-Delivery Opportunities
**As a** player completing deliveries  
**I want** conversations to potentially open new opportunities  
**So that** deliveries can chain into further gameplay  

**Acceptance Criteria:**
- Recipients may offer new letters during delivery conversation
- Successful delivery might unlock new information or routes
- Building relationship through delivery enables future actions
- Some deliveries reveal connections to other NPCs
- All opportunities emerge through conversation choices, not automatic rewards

## Epic 9: Travel Encounters

### Story 9.1: Route-Based Travel Events
**As a** player traveling  
**I want** encounters during travel  
**So that** journeys are active gameplay  

**Acceptance Criteria:**
- Each route has potential encounters
- ConversationManager handles encounter narrative
- Choices based on equipment and skills
- Outcomes affect: time, stamina, items, discoveries
- Equipment enables additional choices

### Story 9.2: Equipment-Dependent Options
**As a** player with equipment  
**I want** special travel choices  
**So that** gear investment pays off  

**Acceptance Criteria:**
- Climbing gear: "Scale cliff to save 2 hours"
- Waterproof satchel: "Ford river directly"
- Cart: "Offer ride to gain information"
- Choices only appear with correct equipment
- Clear risk/reward for each option

## Epic 10: Compound Actions

### Story 10.1: Natural Action Combinations
**As a** player  
**I want** some actions to naturally accomplish multiple goals  
**So that** I can discover efficiencies  

**Acceptance Criteria:**
- Player can carry trade items to a letter delivery destination to sell there for a profit
- ...
- No special bonuses - just logical overlap
- Players discover these through play

### Story 10.2: Location-Based Opportunities
**As a** player at specific locations  
**I want** contextual actions based on spot properties  
**So that** locations feel distinct  

**Acceptance Criteria:**
- Forest spots: "Gather berries" (uses RESOURCES domain tag)
- Market spots: "Browse stalls" (uses COMMERCE tag)
- Tavern spots: "Listen to gossip" (uses SOCIAL tag)
- Actions generated from domain tags, not hardcoded
- Environmental actions only when no relevant NPCs present

