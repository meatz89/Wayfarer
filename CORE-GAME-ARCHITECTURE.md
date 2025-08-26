# WAYFARER: Core Game Architecture

## Design Philosophy
**Each mechanic does ONE thing intentionally. No dual-purpose mechanics.**

## Three Core Loops

### 1. CONVERSATION SYSTEM (Challenge & Growth)
**Purpose**: Why player needs to grow stronger

#### Card Types & Effect Pools (NO DUPLICATES!)
- **COMFORT**: Build comfort → unlock deeper cards
- **STATE**: Change emotional state → modify rules
- **CRISIS**: Emergency actions → desperate measures
- **LETTER**: Create obligations → ALWAYS creates obligation (success = better terms, failure = worse terms)
- **BURDEN**: Clog deck → punishment for mistakes
- **OBSERVATION**: Temporary knowledge → vanish on Listen

#### Progression Through Tokens (PERMANENT)
- Start: 0 tokens → Basic cards only
- Deliver letters → Gain tokens
- Tokens unlock → New cards added to NPC deck
- Tokens provide → +5% success per token (linear)

#### Comfort (TEMPORARY per conversation)
- Starts at 5 each conversation
- Gates card depth (0-20)
- Resets between conversations
- NOT progression, just tactical resource

### 2. OBLIGATION QUEUE (Travel & Relationships)
**Purpose**: Why player travels and builds relationships

#### Queue Mechanics
- Position 1 MUST be done first
- New obligations enter at lowest available position
- Crisis/urgent can negotiate for position 1

#### Displacement System
- Want to do position 2 before position 1?
- BURN tokens with NPC whose obligation you're displacing
- Burning tokens → Adds BURDEN cards to their deck
- Each obligation specifies which token type burns

#### Letter Types Map to Tokens
- Trust letter → Requires Trust tokens to unlock
- Commerce letter → Requires Commerce tokens
- Status letter → Requires Status tokens  
- Shadow letter → Requires Shadow tokens

### 3. LOCATION/TRAVEL SYSTEM (World & Progression)
**Purpose**: How progression manifests, world grows

#### Observation Cards (Knowledge Abstraction)
- Cost 1 attention to observe
- Create temporary cards for conversations
- Decay over time (fresh→stale→expired)
- NOT hard-coded flags, but mechanical cards

#### Route Access
- Routes require ACCESS PERMITS (special letters)
- Access permits:
  - Take satchel space
  - NO obligation created
  - Obtained via high-token cards (e.g., 5+ Shadow tokens)

#### Travel Challenges
- Conversations to pass checkpoints
- Observation cards help (checkpoint info)
- Time pressure from obligations

## Resource Interconnections

### Attention
- **Uses**: Conversations, Observations, Work
- **Sources**: Rest (coins→attention), Morning refresh
- **Affects**: What actions available

### Coins
- **Uses**: Rest, Travel, Bribes, Letter payments
- **Sources**: Work, Letter delivery
- **Affects**: Attention recovery, route options

### Hunger
- **Uses**: None (pure drain)
- **Sources**: Time passage (+20 per period)
- **Affects**: Patience in conversations (-1 per 20)

### Tokens (Trust/Commerce/Status/Shadow)
- **Uses**: Letter eligibility, Queue displacement, Access permits
- **Sources**: Successful conversations only
- **Affects**: Card success rates, Available cards

### Time
- **Uses**: None (measurement)
- **Sources**: Actions consume time
- **Affects**: Deadlines, Hunger, Observation decay

## Card Success/Failure Effects

### COMFORT Cards
- **Success**: +X comfort
- **Failure**: +0 comfort (wasted action)

### STATE Cards
- **Success**: Change to target state
- **Failure**: No change (or worse state)

### LETTER Cards
- **Success**: Good terms (long deadline, flexible position, standard pay)
- **Failure**: Bad terms (tight deadline, position 1, high pay)
- **BOTH**: Create obligation + give letter

### CRISIS Cards
- **Success**: Resolve crisis, gain major comfort
- **Failure**: Add burden cards, conversation ends

### BURDEN Cards
- **Success**: Remove burden
- **Failure**: Burden remains, lose token

## Strategic Depth

### Early Game (0-5 tokens)
- Low success rates
- Basic cards only
- Poor letter terms
- Must accept bad obligations

### Mid Game (5-15 tokens)
- Better success rates
- State change cards appear
- Can negotiate terms
- Some queue manipulation

### Late Game (15+ tokens)
- High success rates
- Powerful cards available
- Access permits obtainable
- Full queue control

## Key Design Rules

1. **Tokens are progression** - Permanent growth
2. **Comfort is tactical** - Temporary per conversation
3. **Every card has risk** - Success/failure both meaningful
4. **Letters always given** - Negotiation is about terms
5. **Queue forces choices** - Position 1 or burn tokens
6. **Observation is temporary** - Knowledge decays
7. **Access needs permission** - Routes require permits
8. **Each mechanic does ONE thing** - No dual purpose

## Example: Elena's Desperate Letter

1. **Token Check**: Player has 1 Trust token (minimum met)
2. **State Check**: Elena in DESPERATE (crisis situation)
3. **Letter Eligible**: "Urgent Refusal to Lord Blackwood" available
4. **Listen in OPEN**: Letter card added to hand
5. **Play Letter Card**:
   - Success (55%): 4h deadline, position 2, 10 coins
   - Failure (45%): 1h deadline, FORCES position 1, 15 coins
6. **Either way**: Letter added, obligation created
7. **Strategic choice**: Risk negotiation or take crisis card?

This creates a risk/reward decision, not a simple "accept quest" button.