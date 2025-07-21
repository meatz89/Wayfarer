# LOGICAL SYSTEM INTERACTIONS

**CRITICAL DESIGN GUIDELINES - MANDATORY FOR ALL IMPLEMENTATIONS**

This document defines how the letter queue system creates emergent gameplay through logical interactions. The queue order rule combined with connection tokens and deadlines generates all strategic complexity without arbitrary modifiers.

**🔄 TRANSFORMATION CONTEXT**: For analysis of how these interactions transform the entire game, see **`LETTER-QUEUE-TRANSFORMATION-ANALYSIS.md`** - Section: "Game Design Ramifications"

## FUNDAMENTAL DESIGN PRINCIPLE

### **Queue Order Creates Everything**

**CORE RULE: The requirement to deliver letters in position order (1→2→3...) creates all strategic tension**

✅ **CORRECT: Queue Position Enforcement**
```csharp
// Queue position determines delivery order absolutely
if (letterQueue.GetPosition(letter) != 1 && !spendingConnections)
    return DeliveryResult.MustDeliverInOrder("Cannot deliver letter at position " + position);
```

❌ **WRONG: Arbitrary Delivery Bonuses**
```csharp
// Arbitrary modifiers that don't emerge from queue mechanics
deliverySpeed *= playerLevel > 5 ? 1.5f : 1.0f;
payment = basePayment * reputationMultiplier;
```

## CORE QUEUE SYSTEM INTERACTIONS

### **Letter Queue Priority System**

**IMPLEMENTED**: 8-slot priority queue with leverage-based positioning.

**Queue Position Rules**:
- **Position 1**: Must deliver next unless spending connections to skip
- **Positions 2-8**: Wait their turn as letters above are delivered
- **Leverage Entry**: Letters enter based on sender's social status + token debt
- **Displacement**: High-leverage letters push existing letters down
- **Delivery**: Removes letter, all below move up one position

**Leverage-Based Entry Positions**:
- **Base Positions**: Patron (1), Noble (3), Trade/Shadow (5), Common/Trust (7)
- **Token Debt Effect**: Each negative token moves position -1 (earlier)
- **High Respect Effect**: 4+ positive tokens moves position +1 (later)
- **Leverage Inversion**: Debt can make commoners enter at noble positions

**Strategic Pressure Created**:
- Token debt creates queue disruption through leverage
- Social hierarchies invert when you owe money
- Accepting help reshapes your entire priority structure
- Forced discards when leverage pushes letters past position 8

### **Connection Token Economy System**

**IMPLEMENTED**: Five token types representing spendable social capital.

**Token Types & Sources**:
- **Trust (Heart)**: Earned from personal deliveries, romance actions
- **Trade (Merchant)**: Earned from commercial deliveries, trade deals
- **Noble (Court)**: Earned from aristocratic deliveries, court events
- **Common (Folk)**: Earned from everyday deliveries, tavern socializing
- **Shadow (Black)**: Earned from illicit deliveries, underground contacts

**Token Spending Rules**:
- **Purge (3 any tokens)**: Remove bottom letter from queue
- **Priority (5 matching)**: Move letter to position 1
- **Extend (2 matching)**: Add 2 days to deadline
- **Skip (1 matching)**: Deliver one letter out of order
- **Route Discovery (1-3 tokens)**: Learn secret paths from knowledgeable NPCs

**Strategic Decisions Created**:
- Save tokens for emergencies vs spend for immediate relief
- Specialize in token types vs maintain balanced reserves
- Burn relationships for queue management vs preserve for future
- Token type matching requirements create collection pressure

### **Leverage Through Token Debt System**

**IMPLEMENTED**: Negative tokens create power dynamics that reshape the queue.

**Debt Creation Actions**:
- **Request Patron Funds**: -1 Patron token, gain 30 coins
- **Borrow from NPC**: -2 tokens, gain 20 coins
- **Emergency Route Access**: -1 token to maintain access below threshold
- **Accept Illegal Work**: -1 Shadow token (they have dirt on you)
- **Use Service Without Payment**: -1 to -3 tokens based on value

**Leverage Effects on Queue Position**:
```
Token Balance → Queue Position Modifier
+4 or more   → +1 (less leverage, mutual respect)
+1 to +3     → +0 (normal relationship)
0            → +0 (neutral)
-1           → -1 (some leverage)
-2           → -2 (significant leverage)
-3 or less   → -3 (extreme leverage)
```

**Example Leverage Inversions**:
- **Noble at -2 tokens**: Letters enter at position 1 (not 3)
- **Common at -3 tokens**: Letters enter at position 4 (not 7)
- **Merchant at +5 tokens**: Letters enter at position 6 (not 5)

**Strategic Debt Usage**:
- Deliberately go into debt with reliable NPCs for queue control
- Use high-leverage positions to block unreliable senders
- Strategic bankruptcy: accept all help to fill queue with chosen debts
- Independence path: never go negative, maintain queue autonomy

### **Deadline Pressure System**

**IMPLEMENTED**: Every letter has a deadline creating mathematical impossibilities.

**Deadline Mechanics**:
- **Letter Deadlines**: 3-10 days typical, tick down daily
- **Morning Countdown**: All deadlines reduce by 1 each dawn
- **Expiration**: Deadline 0 = letter vanishes, sender relationship damaged
- **Queue Position Irrelevant**: Deadlines tick regardless of position

**Mathematical Conflicts Created**:
- **Queue Order vs Urgency**: Position 1 has 8 days, position 5 has 2 days
- **Route Distance vs Time**: Optimal delivery path conflicts with deadlines
- **Multiple Expirations**: Several letters expiring same day, can't save all
- **Rest vs Delivery**: Need rest but letters expire during sleep

**Example Crisis**:
```
Queue State:
1. Noble: Court summons (8 days) - Low priority but blocking
2. Merchant: Trade goods (3 days) - Good pay, moderate urgency  
3. Trust: Elena's letter (1 day!) - Personal, expires tomorrow
4. Shadow: Illegal package (2 days) - High pay, high risk

Problem: Must deliver noble first or spend tokens. Elena expires if you do.
```

### **Standing Obligations System**

**IMPLEMENTED**: Permanent modifiers that reshape queue behavior forever.

#### **Obligation Types**
- **Noble's Courtesy**: Noble letters enter at slot 5, cannot refuse nobles
- **Merchant's Priority**: Trade letters pay +10 coins, cannot purge trade letters  
- **Shadow's Burden**: Shadow letters pay triple, forced shadow letter every 3 days
- **Patron's Eye**: Patron letters advance 1 slot per day automatically
- **Heart's Bond**: Trust letters can extend deadline free, double skip cost

#### **Obligation Mechanics**
```csharp
// Obligations permanently modify queue behavior
if (player.HasObligation("Noble's Courtesy")) {
    if (letter.Type == ConnectionType.Noble) {
        queuePosition = Math.Min(5, emptySlot); // Enter at 5 or first empty
        canRefuse = false; // Must accept all noble letters
    }
}

// Some obligations conflict with each other
if (player.HasObligation("Shadow's Burden") && player.HasObligation("Noble's Courtesy")) {
    // Shadow letters compete with noble letters for mid-queue space
    // Creates permanent queue pressure
}
```

**Strategic Impact**:
- Each obligation provides power but constrains freedom
- Multiple obligations can create unmanageable queue states
- Breaking obligations has permanent consequences
- Your obligations tell the story of your choices

### **Letter Category System**

**NOT YET IMPLEMENTED**: Token thresholds unlock better letter categories.

#### **Category Thresholds**
```csharp
// Token thresholds unlock better paying letter categories
public LetterCategory GetAvailableCategory(string npcId, ConnectionType tokenType) {
    var tokenCount = GetTokensWithNPC(npcId, tokenType);
    if (tokenCount >= 8) return LetterCategory.Premium;  // 15-20+ coins
    if (tokenCount >= 5) return LetterCategory.Quality;  // 8-12 coins  
    if (tokenCount >= 3) return LetterCategory.Basic;    // 3-5 coins
    return LetterCategory.None;                          // No letters offered
}

// All letters enter at slot 8 regardless of tokens
public int GetQueueEntryPosition(Letter letter) {
    if (letter.IsFromPatron) {
        return Random.Range(1, 3); // Patron letters still jump to 1-3
    }
    return 8; // Everything else enters at bottom
}
```

#### **Strategic Implications**
- Building tokens with specific NPCs unlocks better paying work
- Spending tokens loses access to higher categories temporarily
- Creates tension between spending for crises vs saving for categories
- Different NPCs offer different token types (most have 1-2 types only)

## QUEUE-BASED ENTITY CATEGORIES

### **Letters (Core Game Objects)**
- **ConnectionType**: [Trust, Trade, Noble, Common, Shadow] - determines token rewards
- **Size**: [Small, Medium, Large] - affects inventory if implemented
- **Deadline**: [1-10 days] - creates time pressure
- **Payment**: [3-30 coins] - monetary reward
- **QueuePosition**: [1-8] - current position in queue
- **Sender**: NPC reference - affects relationship on skip/expire
- **Recipient**: NPC reference - destination for delivery

### **NPCs (Letter Ecosystem Participants)**
- **ConnectionType**: [Trust, Trade, Noble, Common, Shadow] - tokens they give
- **LetterFrequency**: How often they generate letters
- **DeadlinePreference**: Tight (1-3), Normal (4-6), Relaxed (7-10)
- **SkipMemory**: Remembers last N skipped letters
- **RelationshipStatus**: [Warm, Neutral, Cold, Frozen]

### **Standing Obligations (Character Development)**
- **BenefitType**: Payment bonus, deadline extension, free actions
- **ConstraintType**: Cannot refuse type, cannot purge type, forced letters
- **Frequency**: One-time, daily, every N days
- **BreakCost**: What happens if you violate the obligation

## QUEUE-BASED LOGICAL INTERACTION RULES

### **Queue Position × Deadline Interactions**
1. **Position > 1** + **Deadline = 1 day** = Must spend tokens or letter expires
2. **Multiple deadlines same day** + **Queue order** = Mathematical impossibility
3. **Patron letter arrival** + **Full queue** = Something must be purged
4. **Skip delivery** + **No matching tokens** = Cannot deliver, relationship damage
5. **Queue full** + **New urgent letter** = Must refuse or purge existing

### **Token × Letter Category Interactions**
1. **0-2 tokens with NPC** + **No letters offered** = Build relationship first
2. **3-4 tokens with NPC** + **Basic letters unlocked** = 3-5 coin letters
3. **5-7 tokens with NPC** + **Quality letters unlocked** = 8-12 coin letters
4. **8+ tokens with NPC** + **Premium letters unlocked** = 15-20+ coin letters
5. **Spend tokens** + **Drop below threshold** = Lose access to category

### **Obligation × Queue Behavior Interactions**
1. **Noble's Courtesy** + **Noble letter** = Enters at 5, cannot refuse
2. **Shadow's Burden** + **Day % 3 = 0** = Forced shadow letter appears
3. **Multiple obligations** + **Conflicting rules** = Queue becomes unmanageable
4. **Break obligation** + **Permanent consequence** = Letter source eliminated
5. **Patron's Eye** + **Each dawn** = Patron letters advance 1 position

### **Relationship × Letter Generation Interactions**
1. **Skip letter 3+ times** + **Same NPC** = Stop receiving their letters
2. **Deliver consistently** + **Build tokens** = More/better letters offered
3. **Let letter expire** + **Sender memory** = Relationship cools, fewer letters
4. **Help in crisis** + **Personal letter** = Deepens bond, special letters
5. **Rivalry active** + **Conflicting letters** = Must choose sides

## IMPLEMENTATION REQUIREMENTS

### **1. Queue Order Is Sacred**
The requirement to deliver in position order drives everything:
- Cannot deliver position 3 before positions 1-2 (without token cost)
- Queue position must be clearly visible at all times
- Skipping requires explicit token spending with clear cost display
- NPCs must remember and react to skipped letters

### **2. Tokens Are Spendable Relationships**
Connection tokens represent actual social capital:
- Earning tokens strengthens specific relationships
- Spending tokens weakens those same relationships
- Token costs must be meaningful (not trivial to earn back)
- Different token types cannot substitute for each other

### **3. Deadlines Create Real Pressure**
Every letter deadline must matter:
- Deadlines tick down regardless of queue position
- Expired letters damage sender relationships permanently
- Multiple expiring letters create unsolvable dilemmas
- No way to pause or extend time without token cost

### **4. Obligations Reshape Gameplay**
Standing obligations permanently alter the game:
- Benefits and constraints are both meaningful
- Multiple obligations can conflict with each other
- Breaking obligations has permanent consequences
- Obligations tell the story of player choices

### **5. Everything Must Be Visible**
Players need complete information to make hard choices:
- Queue positions 1-8 clearly displayed
- Deadlines shown on each letter
- Token costs for actions explicit
- Relationship status with each NPC visible
- Obligation effects clearly explained

## VALIDATION CHECKLIST

Before implementing any queue mechanic, verify:

1. ✅ **Queue Order Enforcement**: Does it respect the sacred delivery order?
2. ✅ **Token Cost Meaningful**: Are tokens valuable enough that spending hurts?
3. ✅ **Deadline Creates Pressure**: Do expiring letters force hard choices?
4. ✅ **Obligations Have Weight**: Do they meaningfully reshape gameplay?
5. ✅ **Relationships Matter**: Do NPCs remember and react to player choices?
6. ✅ **Visible Information**: Can players see everything needed to decide?

## CORE DESIGN RULES

- **NEVER** allow free queue reordering without token cost
- **ALWAYS** make deadlines tick regardless of queue position
- **REQUIRE** matching token types for type-specific actions
- **ENSURE** standing obligations have both benefits and constraints
- **VALIDATE** that mathematical impossibilities exist (can't deliver all letters)

## PLAYER EXPERIENCE TARGET - LIFE IN THE QUEUE

**Morning Crisis**: "Elena's birthday letter expires today but it's in position 4. Do I spend 3 Trust tokens to skip ahead, or let it expire and damage our friendship?"

**Patron Disruption**: "My patron's letter just arrived and jumped to slot 1, pushing everything down. The merchant letter that was about to be delivered is now position 4 with 1 day left."

**Token Dilemma**: "I have 5 Shadow tokens. I could move this lucrative shadow delivery to position 1, but what if tomorrow brings a worse crisis?"

**Obligation Conflict**: "Noble's Courtesy means I must accept this noble letter, but my queue is full. Which letter do I purge? The one from my friend or the one that pays my rent?"

**Relationship Death**: "Marcus stopped sending letters after I skipped his last three for more profitable deliveries. Now I need to learn the merchant's route but have no Trade connections to ask."

**The Daily Puzzle**: "Queue: Noble summons (8 days), Patron intel (5 days), Elena urgent (1 day!), Shadow package (2 days). I can deliver 2 today if I take the mountain route, but I sold my climbing gear for tokens..."

Every choice emerges from **queue position mechanics** and **token economy** creating authentic relationship management through simple, clear rules. The queue isn't just a task list - it's your entire social life visualized as a puzzle that's always breaking.

## QUEUE SYSTEM VALIDATION CHECKLIST

Before implementing any queue mechanic, verify:

1. ✅ **Queue Order Sacred**: Must deliver from position 1 or pay token cost
2. ✅ **Deadlines Tick Always**: Every morning reduces all deadlines by 1
3. ✅ **Tokens Have Weight**: Spending tokens damages actual relationships
4. ✅ **Obligations Constrain**: Benefits come with permanent restrictions
5. ✅ **Memory Persists**: NPCs remember every skip, delay, and failure
6. ✅ **Choices Matter**: Some letters must expire - perfection impossible

## QUEUE-SPECIFIC DESIGN RULES

- **ALWAYS** enforce position order - no free reordering
- **NEVER** pause deadlines - time pressure is constant
- **REQUIRE** exact token type matches for specific actions
- **ENSURE** obligations conflict with each other eventually
- **VALIDATE** mathematical impossibilities exist regularly
- **MAINTAIN** patron mystery while letters reveal patterns