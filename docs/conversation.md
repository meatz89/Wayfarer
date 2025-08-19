# Wayfarer Conversation System Design

## Core Concept
Each NPC owns a conversation deck representing their personality, experiences, and relationship with the player. Players navigate these decks through strategic card play, building comfort and depth to unlock letters and deepen relationships.

## Conversation Flow

### Starting a Conversation
1. Costs 1 attention point
2. Calculate starting patience: Base (3-10) + Trust - Emotional modifiers
3. Set depth to 0, state to Neutral
4. Draw 5 cards from NPC's deck at current depth or lower

### Each Turn
1. Player must play one card (cannot pass)
2. Pay patience cost
3. Roll for success: (Patience - Difficulty + 5) × 12%
4. Apply success or failure effects
5. Check for state changes and depth shifts
6. Draw back to 5 cards from available depth

### Ending Conditions
- Patience reaches 0 (exhaustion)
- Player plays Exit card (always available)
- Letter generated (natural conclusion)
- Obligation created (emotional weight)
- Crisis card forces end

## Card Anatomy

```
Card Name [Rarity: Common/Uncommon/Rare/Legendary]
Type: Trust/Commerce/Status/Shadow
Persistence: Persistent/Opportunity/One-shot/Burden/Crisis
Depth Required: 0/1/2/3
Cost: X patience (+ Y coins optional)
Difficulty: X (used for success calculation)

Success (66%+): +X comfort, [tokens], [special]
Failure (0-65%): -X comfort or no effect, [negative]

State:
- Creates: [State name] or None
- Resonates: [State type] → enables depth increase
```

## Card Persistence Types

### Persistent
- Always reshuffles if not played
- Basic conversation options (Small Talk, Listen)
- Reliable fallbacks

### Opportunity  
- Doesn't reshuffle if not played
- Time-sensitive topics (current events, emotional moments)
- Creates "use it or lose it" decisions

### One-shot
- Permanently removed after playing
- Major confessions, life-changing promises
- Cannot be repeated even in future conversations

### Burden
- Remains in deck until resolved
- Negative cards from failures or broken promises
- Must be addressed to clear

### Crisis
- Only appears when NPC is desperate
- High risk, high reward
- Often ignores normal requirements

## Conversation States

Only ONE state active at a time. States create the emotional climate and govern what's possible.

### State Categories

**Positive States** (enable depth through resonance):
- **Open**: Trust and personal cards can increase depth
- **Warm**: All comfort-building cards can increase depth
- **Vulnerable**: Deep Trust/Shadow cards can increase depth
- **Professional**: Commerce/Status cards can increase depth

**Negative States** (block ALL depth increases):
- **Guarded**: No depth increases possible, some cards unplayable
- **Tense**: No depth increases possible, comfort gains reduced by 1

**Neutral State**:
- **Neutral**: Starting state, no special effects or depth changes

### State Properties
- Cards can create states when played
- States last until replaced or conversation ends
- Negative states must be cleared before depth can increase
- Some cards specifically clear negative states

## Depth System

Depth represents conversation intimacy (0-3, with 4 for legendary moments).

### Depth Progression

**Depth INCREASES when**:
- Playing a resonant card while in a positive state
- Playing type-appropriate card in Professional state
- Special card effects explicitly increase depth
- Perfect success (90%+ roll) on high-difficulty cards

**Depth DECREASES when**:
- Critical failure (10% or less roll) on any depth 2+ card
- Playing severely mismatched cards (Trust in Professional, Commerce in Vulnerable)
- Certain burden cards when played

**Depth CANNOT increase when**:
- In Guarded or Tense state (must clear first)
- No resonant cards available
- Already at maximum depth for current relationship

### Depth Gating
- Depth 0: Surface level, always available
- Depth 1: Personal, requires reaching depth 1
- Depth 2: Intimate, requires reaching depth 2
- Depth 3: Soul-deep, requires reaching depth 3
- Depth 4: Transcendent (legendary cards only)

## Token Generation

Tokens represent relationship milestones, not just successful cards.

### Token Triggers
- Reaching comfort thresholds:
  - Comfort 5: +1 token (maintain)
  - Comfort 10: +1 token (progress)
  - Comfort 15: +2 tokens (perfect)
- Below comfort 5: -1 token (relationship decay)
- Letter delivery: +1-3 tokens based on tier
- Special card effects

### Token Types
Each NPC relationship has four token types:
- **Trust**: Adds to starting patience
- **Commerce**: Reduces card costs (minimum 1)
- **Status**: Reduces card difficulty
- **Shadow**: Unlocks special options

## Letter Generation

Letters emerge from successful conversations, not random offers.

### Letter Card Requirements
- Added to deck at relationship milestones (3, 5, 7)
- Must be drawn into hand
- Must meet comfort threshold (usually 10+)
- Cannot be played in negative states
- One letter maximum per conversation

### Letter Tiers
- Tier 1: Relationship 0-2, basic rewards
- Tier 2: Relationship 3-5, moderate rewards  
- Tier 3: Relationship 6+, powerful rewards

## Special Card Types

### State Clearer Cards
Cards that specifically remove negative states:
```
Sincere Apology [Uncommon]
Type: Trust
Cost: 3 patience
Difficulty: 5
Success: Clear any negative state, +2 comfort
Failure: -1 comfort
Creates: Open
```

### Observation Cards
Added to NPC decks when player observes relevant events:
- Context-dependent effectiveness
- May create unexpected states
- Permanent additions to deck

### Burden Resolution
Burden cards must be addressed:
- Can be played to clear (accepting the negative)
- Can be transformed by special cards
- Accumulate until forcing confrontation

### Crisis Cards
Appear only during desperate states:
- Cost all remaining patience
- Ignore most requirements
- Create obligations or major changes
- Can break through negative states

## Deck Evolution

### Starting Deck (10 cards)
- 5 universal basics (Small Talk, Listen, etc.)
- 3 personality-specific cards
- 2 mild burdens (everyone has baggage)

### Deck Growth (max 20 cards)
- Letter delivery adds powerful cards
- Observations add contextual cards
- Failures add burden cards
- Perfect conversations transform negatives

### Deck Curation
When at 20 cards, adding new requires removing old - forcing players to shape relationships deliberately.

## NPC Personality Influence

### Personality Types
- **Devoted**: High patience (8-10), Trust-focused
- **Mercantile**: Medium patience (5-7), Commerce-focused
- **Proud**: Low patience (3-5), Status-focused
- **Cunning**: Variable patience (4-6), Shadow-focused
- **Steadfast**: Balanced patience (6-8), mixed focus

### Emotional States
NPC emotional states modify conversations:
- **Desperate** (<6 hours on letter): -3 patience, crisis cards appear
- **Anxious** (6-12 hours): -1 patience, starts in Tense state
- **Hostile** (failed letter): Cannot converse normally
- **Neutral**: Normal patience and state

## Success Calculation

```
Success Chance = (Current Patience - Card Difficulty + 5) × 12%
```

- Minimum 0%, Maximum 100%
- Status relationship reduces all difficulties by 1
- Shadow 5+ can ignore one requirement per conversation
- Player sees percentage before playing

## Example Cards

### Basic Card
```
Small Talk [Common]
Type: Trust
Persistence: Persistent
Depth: 0
Cost: 1 patience
Difficulty: 2

Success: +2 comfort
Failure: +1 comfort

State:
- Creates: Warm
- Resonates: Open
```

### State Clearer
```
Break the Ice [Common]
Type: Trust
Persistence: Persistent
Depth: 0
Cost: 2 patience
Difficulty: 3

Success: +2 comfort, clear negative state
Failure: No effect

State:
- Creates: Open
- Resonates: None
```

### Letter Card
```
Request Personal Letter [Uncommon]
Type: Trust  
Persistence: Opportunity
Depth: 2
Cost: 2 patience
Difficulty: 4

Success: +2 comfort, Generate Trust Letter
Failure: Add "Rejected Request" burden

State:
- Creates: None
- Resonates: Open, Vulnerable
```

### Crisis Card
```
Desperate Plea [Crisis]
Type: Trust
Persistence: Crisis (this conversation only)
Depth: Any (ignores requirement)
Cost: All remaining patience
Difficulty: 6

Success: Generate letter, Create obligation, clear negative states
Failure: -5 comfort, NPC becomes hostile

State:
- Creates: Vulnerable
- Resonates: Any (desperation transcends states)
```

### Burden Card
```
Unresolved Issue [Burden]
Type: None
Persistence: Burden
Depth: 0
Cost: 2 patience
Difficulty: 3

Success: Remove this card, +1 comfort
Failure: -2 comfort, creates Tense state

State:
- Creates: Tense (on failure)
- Resonates: None
```

## Design Principles

### Mechanical Elegance
- Binary success/failure (no partial outcomes)
- States have clear mechanical effects
- No multi-turn tracking needed
- Depth emerges from state management
- All information visible on cards

### Verisimilitude
- Negative states block intimacy (can't deepen when guarded)
- Must repair before progressing
- Crisis breaks through barriers
- Some moments only happen once
- Trust builds gradually

### Player Experience
- Clear obstacles (negative states) to overcome
- Discovery through depth exploration
- Risk/reward in state management
- Crisis moments for dramatic saves
- Every conversation tells a story

## AI Narrative Generation

The AI receives mechanical cards and generates contextual narrative:

### Input to AI
- Current NPC (personality, emotional state, location)
- 5 drawn cards with their mechanical properties
- Current conversation state and depth
- Previous cards played this conversation
- Current comfort and patience levels

### AI Generation Process
1. Analyzes emotional state and active conversation state
2. Identifies conversation theme from card types
3. Generates NPC's opening based on state (guarded, open, etc.)
4. Creates contextual description for each card option
5. Maps mechanical effects to narrative outcomes

### Example
Mechanical: "Clear negative state, +2 comfort"
Generated in Guarded state: "Your gentle persistence finally breaks through her defenses..."
Generated in Tense state: "The laughter breaks the tension that's been building..."

The same mechanical effect generates different narrative based on which negative state is being cleared, creating emergent storytelling from systematic mechanics.