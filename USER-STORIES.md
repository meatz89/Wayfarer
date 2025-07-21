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
- Deliver action only available for position 1 letter
- Attempting to deliver other positions shows token cost
- Successful delivery removes letter and shifts all below up one position
- Uncollected letters cannot be delivered (show "must collect first")

### Story 1.3: Token Burning for Queue Skipping
**As a** player  
**I want to** skip queue order by burning connection tokens  
**So that** I can prioritize urgent deliveries at social cost  

**Acceptance Criteria:**
- Selecting non-position-1 letter shows skip cost
- Cost = 1 token of sender's type for each position skipped
- Example: Deliver position 3 = burn 1 token each with position 1 and 2 senders
- Burns tokens with specific NPCs, not general tokens
- Cannot skip if insufficient tokens (action disabled)
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

### Story 2.2: Token-Modified Leverage
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

### Story 2.3: Queue Overflow and Discard
**As a** player with a full queue  
**I want** high-leverage letters to force entry  
**So that** powerful senders can't be ignored  

**Acceptance Criteria:**
- When queue is full and letter forces entry above position 8
- Letter at position 8 is automatically discarded
- Player receives notification of discarded letter
- No token penalty for forced discard
- Discarded letter's sender remembers this slight

### Story 2.4: Standing Obligation Position Overrides
**As a** player with standing obligations  
**I want** obligations to override normal entry positions  
**So that** my commitments reshape queue behavior  

**Acceptance Criteria:**
- "Patron's Expectation": Patron letters always enter at position 1
- "Noble's Courtesy": Noble letters always enter at position 5
- Override applies regardless of token balance
- Multiple obligations can conflict (design doc rules apply)

## Epic 3: Connection Token Management

### Story 3.1: Per-NPC Token Tracking
**As a** player  
**I want** separate token counts with each NPC  
**So that** each relationship is distinct  

**Acceptance Criteria:**
- Each NPC shows token count by type
- Example: "Marcus: Trade 3, Common 1"
- Tokens can be positive, zero, or negative
- Negative tokens show as debt (e.g., "Trade -2")
- Token changes show visual feedback (+1, -1)

### Story 3.2: Token Generation Through Delivery
**As a** player  
**I want to** earn tokens by completing deliveries  
**So that** I build relationships through work  

**Acceptance Criteria:**
- Successful delivery grants 1 token of recipient's type
- Token goes to recipient, not sender
- Delivery from positions 4-6: No token (mild delay)
- Delivery from positions 7-8: -1 token with sender (major delay)
- Token changes display immediately

### Story 3.3: Token Debt Creation
**As a** player  
**I want to** go into token debt for emergency help  
**So that** desperation has mechanical weight  

**Acceptance Criteria:**
- Request funds from patron: -1 Patron token
- Borrow money from NPC: -2 tokens
- Use service without payment: -1 to -3 tokens based on value
- Can go into debt even from positive (e.g., +2 to -1 is allowed)
- Debt affects leverage immediately

### Story 3.4: Token Spending for Queue Actions
**As a** player  
**I want to** spend tokens for queue manipulation  
**So that** crisis management costs relationships  

**Acceptance Criteria:**
- Purge (remove position 8): 3 tokens of any type
- Emergency Priority (move to position 1): 5 matching tokens
- Extend Deadline: 2 matching tokens adds 2 days
- Tokens must match letter type for specific actions
- Cannot spend tokens you don't have

## Epic 4: Physical Letter States

### Story 4.1: Three-State Letter Existence
**As a** player  
**I want** letters to exist in offered/queued/collected states  
**So that** accepting and collecting are distinct actions  

**Acceptance Criteria:**
- Offered: NPC has letter, player can accept/refuse
- Queued: In queue but not physical
- Collected: In inventory and queue
- Must collect before delivery
- Uncollected letters show "Not Collected" status

### Story 4.2: Physical Collection Requirements
**As a** player  
**I want to** collect letters from sender locations  
**So that** routing includes collection planning  

**Acceptance Criteria:**
- Collection requires visiting sender's location
- Collection transfers letter to inventory
- Letters have size: Small (1), Medium (2), Large (3)
- Inventory has 8 total slots shared with items
- Cannot collect if insufficient inventory space

### Story 4.3: Inventory Management
**As a** player  
**I want** letters and items to compete for space  
**So that** carrying capacity creates hard choices  

**Acceptance Criteria:**
- 8 inventory slots total
- Letters use slots: S=1, M=2, L=3
- Equipment uses slots (climbing gear=1, etc.)
- Trade goods use slots
- Visual inventory grid shows current usage
- Cannot exceed 8 slots

## Epic 5: NPC and Relationship Systems

### Story 5.1: NPC Letter Generation
**As a** player building relationships  
**I want** NPCs to offer letters based on tokens  
**So that** relationships unlock opportunities  

**Acceptance Criteria:**
- 0 tokens: No letters offered
- 1-2 tokens: Basic letters (low pay)
- 3-4 tokens: Quality letters (medium pay)
- 5+ tokens: Premium letters (high pay)
- Letter availability checks happen during conversation
- Can refuse offered letters

### Story 5.2: NPC Memory and Reactions
**As a** player who skips letters  
**I want** NPCs to remember and react  
**So that** actions have social consequences  

**Acceptance Criteria:**
- NPCs remember last 3 skipped letters
- Dialogue reflects relationship state
- At 0 tokens: NPC refuses all interaction
- Negative tokens: NPC may demand repayment
- Positive history: NPC offers better opportunities

### Story 5.3: Letter Categories by NPC Type
**As a** player  
**I want** NPCs to offer appropriate letter types  
**So that** merchants offer trade letters, nobles offer noble letters  

**Acceptance Criteria:**
- Each NPC has 1-2 appropriate token types
- Marcus (merchant): Trade and Common only
- Duke (noble): Noble and Trust only
- Elena (scribe): Trust only
- Letters match NPC's social sphere

## Epic 6: Standing Obligations

### Story 6.1: Obligation Acquisition
**As a** player  
**I want to** gain obligations through special deliveries  
**So that** my choices permanently alter gameplay  

**Acceptance Criteria:**
- Certain letters create obligations when delivered
- Clear warning before accepting obligation letter
- Obligation effects display before acceptance
- Cannot refuse obligation once acquired
- Multiple obligations can be active

### Story 6.2: Queue Rule Modifications
**As a** player with obligations  
**I want** obligations to modify queue behavior  
**So that** my commitments reshape my game  

**Acceptance Criteria:**
- "Noble's Courtesy": Noble letters enter at 5, cannot refuse nobles
- "Shadow's Burden": Forced shadow letter every 3 days, cannot purge shadow
- "Merchant's Priority": Trade letters pay +10 coins, cannot move trade letters
- Modifications apply automatically
- Conflicts between obligations create compound effects

### Story 6.3: Breaking Obligations
**As a** player  
**I want to** break obligations through specific actions  
**So that** I can escape at permanent cost  

**Acceptance Criteria:**
- Each obligation has a breaking condition
- Example: Refuse noble letter breaks "Noble's Courtesy"
- Breaking is permanent and irreversible
- Lose all benefits immediately
- Some NPCs may refuse interaction after breaking

## Epic 7: Route and Travel Systems

### Story 7.1: Token-Gated Route Access
**As a** player  
**I want** routes to require relationship thresholds  
**So that** connections enable efficient travel  

**Acceptance Criteria:**
- Mountain Pass: Requires 3+ Common tokens with Guide
- River Ferry: Requires 3+ Trade tokens with Captain
- Show requirements on route selection
- Routes unavailable if below threshold
- "Lost access" notification when tokens drop

### Story 7.2: Route Properties and Travel Time
**As a** player  
**I want** different routes with varying properties  
**So that** route choice matters strategically  

**Acceptance Criteria:**
- Each route shows: time cost, token requirements, hazards
- Mountain routes: Faster but need equipment
- River routes: Moderate speed, weather dependent
- Main roads: Slow but always available
- Travel time consumes hours from the day

## Epic 8: Deadline and Time Management

### Story 8.1: Independent Deadline System
**As a** player  
**I want** letter deadlines independent of queue position  
**So that** urgency and priority conflict  

**Acceptance Criteria:**
- Each letter shows days remaining
- Deadlines tick down each morning
- Visual urgency indicators (color coding)
- Expired letters vanish at dawn
- Expiration damages relationship (-2 tokens)

### Story 8.2: Time Period Availability
**As a** player  
**I want** NPCs available at specific times  
**So that** timing affects planning  

**Acceptance Criteria:**
- NPCs show availability windows
- Morning (6am-12pm), Afternoon (12pm-6pm), Evening (6pm-10pm)
- Some NPCs have multiple windows
- Travel time affects arrival period
- Cannot interact outside availability

## Epic 9: Multi-Purpose Items

### Story 9.1: Items with Multiple Uses
**As a** player carrying items  
**I want** each item to have delivery/use/trade options  
**So that** moral choices emerge from gameplay  

**Acceptance Criteria:**
- Medicine: Deliver for payment OR use for healing OR sell
- Documents: Deliver sealed OR read for information OR sell
- Trade goods: Deliver OR sell at different prices
- Each use is exclusive (can't use then deliver)
- Using items meant for delivery damages relationships

### Story 9.2: Item Properties and Requirements
**As a** player  
**I want** items to have properties affecting carriage  
**So that** inventory becomes strategic  

**Acceptance Criteria:**
- Fragile items: Require protection or risk breaking
- Heavy items: Require cart or two people
- Perishable items: Lose value over time
- Valuable items: Risk of theft in certain areas
- Properties shown on item inspection

## Epic 10: Resource Management

### Story 10.1: Action Resource Costs
**As a** player  
**I want** actions to cost appropriate resources  
**So that** everything has a price  

**Acceptance Criteria:**
- Travel: 2 stamina per segment
- Work: 2 stamina per period
- Deep conversation: 2 Focus
- Rest: +3 stamina per period
- All costs visible before action

### Story 10.2: Resource Recovery Options
**As a** player  
**I want** multiple ways to recover resources  
**So that** I can manage depletion strategically  

**Acceptance Criteria:**
- Rest action: +3 stamina, costs 1 period
- Full sleep: +6 stamina, ends day
- Meditation: +3 Focus, costs 1 period
- Food items: Variable stamina recovery
- Inn stay: Full recovery, costs coins

This comprehensive set of user stories captures the complete game design, ready for implementation. Each story is specific, testable, and maintains the interconnected systems we've designed.