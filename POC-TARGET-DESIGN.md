# WAYFARER - Letters and Ledgers Minimal POC Target Design

**⚠️ IMPORTANT: This is the TARGET DESIGN specification, not current implementation status.**

**🔄 TRANSFORMATION PLAN**: See **`LETTER-QUEUE-TRANSFORMATION-ANALYSIS.md`** for the complete analysis of how to transform from current implementation to this target design.

## CORE DESIGN TARGET

Create **impossible queue management dilemmas** where the iron law of delivery order conflicts with deadline pressure. Players must constantly choose between following queue order (preserving relationships) or spending precious connection tokens to manipulate their obligations. The 8-slot queue creates genuine strategic puzzles where mathematical perfection is impossible.

## ENTITY LIMITS (5-8 Each)

### LOCATIONS (3)
1. **Millbrook** (Starting Hub) - [Market], [Tavern], [Manor_Court] - **Token Access**: None/Trust/Noble
2. **Thornwood** (Resource Hub) - [Logging_Camp], [Tavern] - **Token Access**: None/Common
3. **Crossbridge** (Trade Hub) - [Market], [Trading_House], [Dock] - **Token Access**: None/Trade/Noble

### ROUTES (8)
1. **Main Road** (Millbrook ↔ Crossbridge) - [Cart] compatible, 2 periods
2. **Mountain Pass** (Millbrook ↔ Crossbridge) - Requires [Climbing], 1 period
3. **Forest Trail** (Millbrook ↔ Thornwood) - Requires [Navigation], 1 period
4. **River Route** (Thornwood ↔ Crossbridge) - Requires [Navigation], 1 period
5. **Logging Road** (Millbrook ↔ Thornwood) - [Cart] compatible, 2 periods
6. **Trade Highway** (Crossbridge ↔ Thornwood) - [Cart] compatible, 2 periods
7. **Rapids Route** (Thornwood ↔ Crossbridge) - Requires [Climbing] + [Navigation], 1 period
8. **Direct Path** (Thornwood ↔ Millbrook) - Requires [Climbing], 1 period

### EQUIPMENT CATEGORIES (5) - ROUTE & ACCESS ENABLERS
1. **[Climbing]** - Enables mountain routes for urgent deliveries, takes 1 slot, costs 5 coins
2. **[Navigation]** - Enables forest/river shortcuts, takes 1 slot, costs 5 coins
3. **[Letter_Satchel]** - Base equipment, holds 8 letters in queue, provided at start
4. **[Court_Attire]** - Required for Noble letter deliveries, takes 1 slot, costs 8 coins
5. **[Guild_Credentials]** - Required for Trade guild deliveries, takes 1 slot, costs 6 coins

### LETTER QUEUE SYSTEM (Core Mechanic)
- **8 Slots Total**: Letters occupy positions 1-8 in priority order
- **Queue Order Rule**: Must deliver from position 1 or spend tokens
- **New Letters**: Enter at slot 8 (or higher with connection gravity)
- **Delivery**: Completing delivery removes letter, all below move up
- **Deadlines**: Each letter has 3-10 day deadline, tick down daily

### STAMINA SYSTEM
- **Base Stamina**: 10 points
- **Travel Cost**: 1 stamina per period traveled
- **Work Cost**: 2 stamina per period worked (letter delivery contracts, equipment commissioning)
- **Recovery**: 3 stamina per rest period, 6 stamina per night's sleep

### ROUTE DISCOVERY SYSTEM
- **All routes visible**: Players can see all 8 routes from the start
- **Equipment requirements shown**: Route interface displays required equipment categories
- **Blocked routes**: Attempting routes without required equipment shows "Cannot travel - requires [Climbing]"
- **Discovery through failure**: Players learn equipment needs by attempting blocked routes

### CONNECTION TOKEN SYSTEM
**Five Token Types (Earned through deliveries):**
- **Trust (Green)**: Personal letters, friendships, romance
- **Trade (Blue)**: Merchant deliveries, commercial dealings
- **Noble (Purple)**: Court correspondence, aristocratic favors
- **Common (Brown)**: Everyday folk, local deliveries
- **Shadow (Black)**: Illicit letters, underground networks

**Token Uses:**
- **Purge (3 tokens)**: Remove bottom letter from queue
- **Priority (5 matching tokens)**: Move letter to position 1
- **Extend (2 matching tokens)**: Add 2 days to deadline
- **Skip (1 matching token)**: Deliver out of order without penalty

### NPCS AS LETTER SENDERS (9 total)
**Millbrook**:
- **[Elena_Messenger]** - Trust tokens, sends personal letters, remembers skipped romance notes
- **[Market_Trader]** - Trade tokens, sends merchant deliveries, offers bulk trade letters
- **[Manor_Steward]** - Noble tokens, sends court correspondence, requires Court Attire

**Thornwood**:
- **[Logger]** - Common tokens, sends local deliveries, simple honest folk
- **[Herb_Gatherer]** - Trust tokens, sends medicinal deliveries, builds friendships
- **[Camp_Boss]** - Trade tokens, sends resource shipments, time-sensitive orders

**Crossbridge**:
- **[Dock_Master]** - Trade tokens, sends shipping manifests, strict deadlines
- **[Shadow_Contact]** - Shadow tokens, sends illicit packages, high risk/reward
- **[Port_Official]** - Noble tokens, sends customs documents, formal procedures

### YOUR MYSTERIOUS PATRON
**Special Letters that disrupt everything:**
- **Patron Letters**: Jump to slots 1-3 when they arrive, pushing everything down
- **High Payment**: 20-30 coins vs normal 3-8 coin letters
- **Unknown Purpose**: Deliver to seemingly random people for unclear reasons
- **Monthly Resources**: Your patron sends coins and equipment if you serve well
- **The Mystery**: You never learn who they are or what they want

### STANDING OBLIGATIONS (Permanent modifiers from special letters)
1. **[Noble's Courtesy]** - Noble letters enter at slot 5 instead of 8, but you CANNOT refuse noble letters
2. **[Merchant's Priority]** - Trade letters pay +10 coins bonus, but Trade letters cannot be purged
3. **[Shadow's Burden]** - Shadow letters pay triple rate, but you receive forced Shadow letter every 3 days
4. **[Patron's Expectation]** - Monthly resource package, but patron letters jump to slots 1-3
5. **[Elena's Promise]** - Trust letters can extend deadlines by 1 day free, but skipping Trust letters costs double tokens
6. **[Common Folk's Friend]** - Common letters enter at slot 6, but refusing Common letters loses 2 tokens

## STRATEGIC TENSION DESIGN: THE QUEUE CREATES IMPOSSIBLE CHOICES

### Core Impossibility: Queue Order vs Deadlines
**Mathematical Conflicts That Force Token Spending**
- Letter at position 1: Noble court summons (low pay, 8 days left)
- Letter at position 4: Elena's love letter (Trust building, expires tomorrow!)
- Letter at position 5: Shadow delivery (triple pay, 2 days left)
- **The Dilemma**: Follow order and lose Elena + Shadow opportunity, or burn tokens?
- **Token Scarcity**: You only have 3 Trust tokens - spend them to save romance or keep for crisis?

### Connection Gravity Effects
**Token Accumulation Creates Queue Entry Benefits**
- 0-2 tokens: Letters enter at slot 8 (bottom priority)
- 3-4 tokens: Letters enter at slot 7 (slight priority boost)
- 5+ tokens: Letters enter at slot 6 (significant priority boost)
- **Strategic Choice**: Spread tokens for flexibility or specialize for gravity benefits?
- **The Trade-off**: Tokens spent on crisis management reduce gravity effects

### Standing Obligations Reshape Everything
**Permanent Modifiers Create Unique Playthroughs**
- Accept "Noble's Courtesy": Noble letters flood slots 5-6, harder to help commoners
- Accept "Shadow's Burden": Forced shadow letters every 3 days eat queue space
- Accept "Patron's Expectation": Patron controls slots 1-3, personal life suffers
- **Strategic Pressure**: Each obligation provides benefits but permanently constrains freedom
- **Result**: By day 14, your queue behavior is completely unique based on obligations accepted

## POC CHALLENGE DESIGN: THE LETTER QUEUE CRISIS

### Day 1: Your Life in 8 Slots
**Starting Conditions**: 
- Location: Millbrook
- Equipment: [Letter_Satchel] only
- Money: 12 coins
- Tokens: 2 Trust, 1 Trade, 1 Common
- Queue: 3 letters already waiting

**Initial Queue State**:
1. **Patron Letter** (Unknown sender → Crossbridge merchant, 5 days, pays 20 coins)
2. **Trade Letter** (Market_Trader → Thornwood, 3 days, pays 5 coins) 
3. **Trust Letter** (Elena → Her friend in Millbrook, 4 days, pays 3 coins + Trust token)

**Morning Choices**: Letter board shows 3 new letters - which do you accept into slots 4-8?

**Natural Discovery**: Player learns queue order rule when trying to deliver Elena's easy local letter first

### Open Sandbox Challenge: "Master the Queue - Survive 14 Days"
**Victory Condition**: Maintain positive token balance with at least 3 NPCs and deliver your patron's final letter

**Strategic Dimensions**:
1. **Queue Management**: Balance order requirements vs deadline pressure
2. **Token Economy**: Build reserves vs spend for crisis management
3. **Route Optimization**: Equipment investment for shortcuts vs letter capacity
4. **Gravity Building**: Specialize in token types vs maintain flexibility
5. **Obligation Choices**: Accept permanent modifiers for short-term gains?

**Discovery Through Queue Pressure**:
- When to follow order vs when to burn tokens (learned through deadline failures)
- Which token types to stockpile (discovered through crisis patterns)
- How gravity affects queue management (seen as letters enter higher)
- Which obligations help vs hinder (experienced through permanent constraints)
- The true cost of skipping letters (NPCs remember and relationships cool)

### Failure States That Teach Queue Management
**Queue Paralysis**: Player refuses to spend tokens, follows strict order, watches urgent high-value letters expire

**Token Bankruptcy**: Player burns all tokens early for minor crises, has nothing left when patron letter arrives at bad position

**Obligation Trap**: Player accepts too many standing obligations, queue becomes unmanageable with forced letters

**Relationship Death Spiral**: Player repeatedly skips same NPC's letters, they stop offering letters entirely

**The Elena Heartbreak**: Player prioritizes money over Trust letters, Elena stops writing, romance path closes permanently

## SUCCESS METRICS FOR POC

### Queue Management Mastery
1. **Daily Queue Crises**: Every day presents 2+ impossible delivery order dilemmas
2. **Token Spending Decisions**: Players agonize over each token expenditure
3. **Deadline Juggling**: Players do queue math to optimize delivery routes
4. **Gravity Strategy**: Players intentionally build token types for queue benefits

### Emotional Investment Indicators
1. **Elena Moments**: Players genuinely care about maintaining Trust relationships
2. **Patron Mystery**: Players speculate about patron's identity and motives
3. **Token Hoarding**: Players create "emergency token reserves" for crisis
4. **Obligation Regret**: Players realize too late how obligations constrain them

### Strategic Depth Validation
1. **No Perfect Run**: Mathematical impossibility of delivering all letters on time
2. **Multiple Approaches**: Token specialist vs generalist both viable
3. **Recovery Options**: Failed relationships can be rebuilt with effort
4. **Meaningful Progression**: Each day's choices affect future queue state

## VALIDATION SCENARIOS

### Test 1: Queue Order Enforcement
- Does requiring delivery in order create genuine strategic tension?
- Do players feel the weight of skipping letters to deliver others?
- Is the queue position visible and understandable?

### Test 2: Token Economy Balance
- Are token costs meaningful (3 for purge, 5 for priority, etc.)?
- Do players build token reserves vs spend immediately?
- Does token scarcity create difficult decisions?

### Test 3: Deadline Pressure
- Do 3-10 day deadlines create mathematical impossibilities?
- Can players plan routes considering queue order + deadlines?
- Do expired letters feel like meaningful losses?

### Test 4: Standing Obligations
- Do obligations reshape gameplay in interesting ways?
- Are the benefits worth the permanent constraints?
- Do different obligation combinations create unique experiences?

## IMPLEMENTATION REQUIREMENTS

### JSON Content Structure
All entities must be defined in JSON files with proper categorical relationships:
- **locations.json**: 3 locations with token access requirements
- **routes.json**: 8 routes with equipment requirements
- **letters.json**: Letter templates with token types, deadlines, payments
- **npcs.json**: 9 NPCs with token types and letter sending behavior
- **obligations.json**: Standing obligation definitions with benefits/constraints

### Queue System Implementation
- **LetterQueue**: 8-slot array with position enforcement
- **Letter**: TokenType, Deadline, Payment, Sender, Recipient, Size
- **ConnectionToken**: Type (Trust/Trade/Noble/Common/Shadow), Count
- **StandingObligation**: Benefit effect, Constraint effect, Permanent flag

### Core Mechanics Validation
- Queue must enforce delivery order (position 1 first)
- Tokens must be spendable resources with meaningful costs
- Deadlines must create real pressure through expiration
- Gravity must affect letter entry position based on token count
- Obligations must permanently modify queue behavior

## DESIGN VALIDATION CHECKLIST

- [ ] **Queue Order Creates Drama**: Position 1-8 enforcement creates constant dilemmas
- [ ] **Token Costs Feel Meaningful**: Spending tokens represents burning real relationships
- [ ] **Deadlines Force Impossible Choices**: Mathematical conflicts between order and expiration
- [ ] **Gravity Rewards Specialization**: 3+ tokens providing queue benefits encourages focus
- [ ] **Obligations Reshape Gameplay**: Permanent modifiers create unique strategic landscapes
- [ ] **Elena Creates Emotional Stakes**: Players care about Trust letters beyond mere mechanics
- [ ] **Patron Mystery Intrigues**: Unknown benefactor creates narrative speculation
- [ ] **No Perfect Solution Exists**: Queue management remains challenging throughout

The POC should demonstrate that the letter queue system creates genuine emotional investment where players agonize over every token spent, every letter skipped, and every relationship balanced. Success requires not just strategic planning but acceptance that some letters - and some relationships - must be sacrificed.

## Related Implementation Documents

**For implementing this POC design**:
1. **`LETTER-QUEUE-TRANSFORMATION-ANALYSIS.md`** - Comprehensive transformation plan from current state
2. **`POC-IMPLEMENTATION-ROADMAP.md`** - Step-by-step development phases with timelines
3. **`LETTER-QUEUE-UI-SPECIFICATION.md`** - Detailed UI requirements for the POC screens
4. **`LETTER-QUEUE-INTEGRATION-PLAN.md`** - How to transform existing systems for the POC

**Development Priority**: Follow Phase 1 of the transformation plan to establish core queue mechanics first