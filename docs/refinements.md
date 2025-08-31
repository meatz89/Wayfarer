# Wayfarer: Conversation System Refinements

## Core Transformation: Contextual Magnitude

### The Fundamental Change
**Original**: Cards had fixed effects. A +2 comfort card always added exactly 2 comfort.

**Refined**: Every card does exactly ONE thing, but the magnitude of that effect scales based on transparent game state. Cards now have scaling formulas instead of fixed numbers.

**Reasoning**: Slay the Spire's depth comes from simple cards interacting with contextual modifiers (relics, powers, stances). This creates constant tactical reevaluation without adding complexity to individual cards.

### Card Scaling Examples
Every card effect scales based on transparent resources. The card still does ONE thing but context determines power:

- **"Building Trust"**: +1 comfort, +1 for each Trust token you have (max +4)
- **"Desperate Plea"**: +X comfort where X = (6 minus current comfort)
- **"Final Argument"**: +X comfort where X = patience remaining ÷ 3
- **"Status Play"**: +X comfort where X = your Status tokens with them

This creates the tactical question: when is the optimal moment to play each card? Early when patience is high? Late when tokens accumulated? After building success chains?

### Card Property Structure
Each card now displays:
- **Base Effect**: The minimum it always does
- **Scaling Property**: What resource it scales with
- **Scaling Rate**: How much per point of resource
- **Maximum**: Ceiling to prevent runaway

Example card display:
```
"Final Argument"
Base: +1 comfort
Scales with: Patience remaining
Rate: +1 per 3 patience
Maximum: +5 comfort total
Current value: +3 comfort (at 7 patience)
```

## Emotional State Multipliers

### Weight Capacity by State (Unchanged from Previous)
- Hostile: 0 (conversation ends)
- Desperate: 2
- Tense: 3
- Neutral: 4
- Open: 4
- Connected: 5

## NPC Intent System

### Visible Future Consequences
**New Mechanic**: The NPC displays their intent for next turn, visible to player before choosing action.

Intent examples:
- **"Growing suspicious"**: Shadow cards will automatically fail next turn
- **"Opening up"**: Next Trust card draws an additional card if successful
- **"Losing patience"**: Next failure costs 2 patience instead of 1
- **"Testing sincerity"**: Must play weight 3+ card next SPEAK or shift left
- **"Defensive stance"**: Maximum weight reduced by 1 next turn
- **"Moment of clarity"**: Next comfort card has double effect

**Reasoning**: Creates planning without hidden information. You see what's coming and must decide whether to meet their expectation or pivot. It's the conversational equivalent of enemy intent in Slay the Spire.

### Intent Generation Rules
Intent is determined by:
- Current emotional state (each state has pool of possible intents)
- Comfort level (extreme comfort values trigger specific intents)
- Token relationships (high tokens enable positive intents)

## Weight as Persistent Resource Pool

### Revolutionary Weight System
**Original**: Weight limit refreshed each turn.

**Refined**: Weight is a persistent pool that depletes with use:
- Start each conversation with full weight pool based on emotional state
- Each card played spends its weight from your pool
- Pool doesn't refresh automatically between SPEAK actions
- LISTEN action refreshes weight pool to maximum for current state
- Can SPEAK multiple turns in a row until weight depleted

Example:
- In Tense state (3 weight capacity)
- Turn 1: SPEAK - play weight-2 card (1 weight remaining)
- Turn 2: SPEAK - can only play weight-1 card
- Turn 3: Must LISTEN to refresh weight pool

**Reasoning**: Creates decision between chaining multiple small cards or one big card. Makes LISTEN not just about drawing but about refreshing capacity to speak. Simulates conversational exhaustion.

## Observation Cards as Contextual Exploits

### Observation Deck Overhaul
**Original**: Observations provided state changes.

**Refined**: Observation cards exploit specific NPC contexts based on transparent game state:

- **"Mention Their Rival"**: +X comfort where X = Commerce tokens
- **"Bring Up Old Times"**: Draw X cards where X = trust tokens with this NPC
- **"Professional Courtesy"**: +X weight capacity where X = Commerce tokens

**Reasoning**: These aren't magical effects - they're conversational tactics that exploit what you've observed about the NPC. The more you know about them (their token counts, state), the more powerful your observations become. Maintains verisimilitude.

## Interrupt Mechanics

### Conditional Auto-Draw Cards
**New Mechanic**: Certain cards have conditional draw triggers that don't require player action. They automatically enter your hand when conditions are met.

Examples:
- **"Awkward Silence"**: Auto-draws when no cards played for 2 turns
- **"Defensive Response"**: Auto-draws when comfort goes negative
- **"Crisis Moment"**: Auto-draws when patience below 3
- **"Perfect Opening"**: Auto-draws when reach Connected state
- **"Last Chance"**: Auto-draws at exactly 1 patience remaining

Properties of interrupt cards:
- Always weight 1 (playable in any state except Hostile)
- Cannot be discarded until played
- Usually have high variance effects
- Represent involuntary conversational moments

**Reasoning**: Creates tactical moments that break standard play patterns. That "Awkward Silence" might be exactly what you need to break a deadlock, or it might clog your hand at the worst moment.

## Success Chain System

## Burden Resolution Conversations

### Special Conversation Type
**New Mechanic**: When choosing "Make Amends" conversation type (only available if NPC has burden cards from past failures):

Special rules:
- Start with ALL burden cards in hand immediately
- Burden cards have weight 0 but negative effects when played
- Must play every burden card before resolution goal becomes playable
- Cannot draw new cards until all burdens played
- The puzzle is sequencing burdens to minimize total damage

Example burden cards:
- **"Acknowledge Betrayal"**: -2 comfort, -1 Trust token
- **"Accept Responsibility"**: -3 patience
- **"Make Amends"**: Costs 10 coins
- **"Explain Failure"**: Reveals 2 cards from deck, discard any comfort cards revealed
- **"Face Consequences"**: Emotional state shifts left
- **"Broken Promise"**: Next 2 cards have -20% success rate

**Reasoning**: Failed obligations create permanent consequences that must be resolved through special conversations. The tactical challenge is finding the order that leaves you in position to still play the resolution goal card.

## Comfort Battery System (Refined from Previous)

### Transparent State Transitions
**Unchanged Core**: Comfort operates as -3 to +3 battery. Reaching +3 triggers rightward state transition. Reaching -3 triggers leftward state transition. All comfort consumed on transition.

**New Addition**: Some cards can "bank" comfort beyond the trigger threshold:
- **"Steady Progress"**: +2 comfort, can exceed +3 (excess carries to next state)
- **"Dramatic Gesture"**: +5 comfort (guarantees state shift with +2 in new state)

## Linear Emotional State Track (Unchanged from Previous)
States form a linear track:
```
Hostile → Desperate → Tense → Neutral → Open → Connected
```

## Goal Card Visibility

### Starting Hand Presence
**Refined**: Goal card enters hand at conversation start based on chosen conversation type. Only playable in specific emotional states:

- **Trust Letters**: Playable in Open or Connected
- **Commerce Letters**: Playable in Neutral, Open, or Connected  
- **Status Letters**: Playable in Tense or Neutral
- **Shadow Letters**: Playable in Desperate or Tense
- **Crisis Letters**: Playable in Desperate only
- **Resolution Goals**: Playable in any state except Hostile

Goal cards show their current playability status and required states clearly.

## Draw Rules by Emotional State

### State-Specific Card Pools
Each state has distinct draw tendencies that work with the scaling system:

- **Desperate**: Crisis cards (scale with low resources), interrupt triggers
- **Tense**: Defensive cards, low-weight high-reliability cards
- **Neutral**: Balanced mix of all types
- **Open**: Token cards with scaling, positive comfort cards
- **Connected**: Heavy cards with maximum scaling potential

## Removed Mechanics

### Completely Eliminated
- Depth system (cards no longer have depth requirements)
- Patience cards (patience is fixed resource)
- State cards (comfort battery is only state transition method)
- Momentum as separate -3 to +3 range (replaced by success chains)

### Core Principles Maintained
- One card played per SPEAK action (no exceptions)
- No hidden state tracking without visible resources
- All effects are deterministic and transparent
- Cards do exactly ONE thing (but magnitude scales)
- Perfect information at all times

## Design Philosophy

These refinements achieve depth through contextual evaluation rather than complex individual cards. Every card's value changes based on:
- Current emotional state multipliers
- Available weight pool
- Success chain status
- Token relationships
- NPC intent for next turn
- Resources like patience and comfort
- Other cards in hand

The conversation becomes a dynamic puzzle where the same hand plays completely differently based on game state. This mirrors Slay the Spire where the same card can be worthless or game-winning depending on your relics, powers, and current fight state.

Verisimilitude is maintained - emotional volatility in desperate states amplifies everything, building on established trust makes conversation easier, weight depletion represents conversational exhaustion, and observations are rhetorical tactics that exploit what you know about your conversational partner.