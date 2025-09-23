# Card System Balance Implementation Plan

## PROBLEM ANALYSIS
The current Elena conversation deck suffers from fundamental design flaws that eliminate strategic decision-making:

### Current Deck Issues
1. **Duplicate Saturation**: 4x soft_agreement + 3x emotional_outburst = 35% of deck
2. **Power Curve Flatness**: Most cards cost 1-3 focus with similar power levels
3. **Resource Homogenization**: 70% of cards just generate momentum variants
4. **No Resource Tension**: Everything generates, nothing consumes resources
5. **Focus Generosity**: Can play 2-4 cards per turn, no meaningful constraints

### Strategic Problems
- "Do I play +2 momentum or +3 momentum?" = no real choice
- Can afford entire hand most turns = no planning required
- No resource tradeoffs = hoarding always optimal
- No card synergies = individual cards don't interact meaningfully

## SOLUTION ARCHITECTURE

### New Card Ecosystem Design
Transform from "momentum generator variants" to "strategic decision framework" with 4 distinct card roles:

1. **Generators** (6 cards): Build momentum at different efficiency/power levels
2. **Converters** (6 cards): Trade momentum for other resources (doubt reduction, flow, cards)
3. **Investments** (4 cards): Scaling effects that reward different game states
4. **Utility** (4 cards): Focus manipulation and support effects

### Power Tier Structure
**TIER 1 (1-2 Focus) - Efficient but Weak:**
- High efficiency (1 momentum per focus)
- Always affordable
- Foundation for every turn

**TIER 2 (3-4 Focus) - Standard Power:**
- Balanced efficiency (0.75-1 momentum per focus)
- Core strategic options
- Require some planning

**TIER 3 (5-6 Focus) - High Impact:**
- Lower efficiency but high absolute power
- Require significant planning or setup
- Game-changing potential

## DETAILED CARD REDESIGN

### TIER 1 - EFFICIENT (1-2 Focus)
```json
{
  "id": "gentle_agreement",
  "focus": 1,
  "successType": "Strike",
  "effect": "+1 momentum",
  "description": "A soft nod of understanding."
}

{
  "id": "quick_insight",
  "focus": 1,
  "successType": "Threading",
  "effect": "Draw 1 card",
  "description": "A flash of understanding opens new possibilities."
}

{
  "id": "pause_reflect",
  "focus": 1,
  "successType": "Soothe",
  "effect": "-1 doubt",
  "description": "Take a moment to center yourself."
}

{
  "id": "build_rapport",
  "focus": 2,
  "successType": "Strike",
  "effect": "+2 momentum",
  "description": "Establish a genuine connection."
}
```

### TIER 2 - STANDARD (3-4 Focus)
```json
{
  "id": "passionate_plea",
  "focus": 3,
  "successType": "Strike",
  "effect": "+3 momentum",
  "description": "Speak with heartfelt conviction."
}

{
  "id": "clear_confusion",
  "focus": 3,
  "successType": "Soothe",
  "effect": "Spend 2 momentum → -3 doubt",
  "description": "Clarify misunderstandings directly."
}

{
  "id": "establish_trust",
  "focus": 3,
  "successType": "Advancing",
  "effect": "Spend 3 momentum → +1 flow",
  "description": "Invest in the relationship's future."
}

{
  "id": "racing_thoughts",
  "focus": 3,
  "successType": "Threading",
  "effect": "Draw 2 cards",
  "description": "Multiple ideas flood your mind."
}
```

### TIER 3 - POWERFUL (5-6 Focus)
```json
{
  "id": "burning_conviction",
  "focus": 5,
  "successType": "Strike",
  "effect": "+5 momentum",
  "description": "Pour your entire being into this moment."
}

{
  "id": "moment_of_truth",
  "focus": 5,
  "successType": "Advancing",
  "effect": "Spend 4 momentum → +2 flow",
  "description": "Make a profound connection breakthrough."
}

{
  "id": "deep_understanding",
  "focus": 6,
  "successType": "Strike",
  "effect": "Momentum = cards in hand",
  "description": "Everything clicks into perfect clarity."
}
```

### SCALING/UTILITY CARDS
```json
{
  "id": "show_understanding",
  "focus": 3,
  "successType": "Strike",
  "momentumScaling": "CardsInHandDivided",
  "description": "The more I listen, the more I understand."
}

{
  "id": "build_pressure",
  "focus": 4,
  "successType": "Strike",
  "momentumScaling": "DoubtReduction",
  "description": "Act before doubt undermines everything."
}

{
  "id": "mental_reset",
  "focus": 0,
  "successType": "Focusing",
  "effect": "+2 focus this turn only",
  "description": "Clear your mind and refocus."
}

{
  "id": "desperate_gambit",
  "focus": 2,
  "successType": "Strike",
  "momentumScaling": "DoubtMultiplier",
  "description": "Channel desperation into determination."
}
```

## ECONOMY REBALANCING

### Momentum Generation Reduction
- **Current**: 2-4 momentum per card average
- **New**: 1-3 momentum per card average
- **Impact**: Slower buildup requires more strategic planning

### Goal Threshold Adjustment
- **Current**: Basic 10, Enhanced 15, Premium 20
- **New**: Basic 8, Enhanced 12, Premium 16
- **Impact**: Matches reduced generation while maintaining turn count

### Resource Conversion Introduction
- **New Mechanic**: Cards that spend momentum for other benefits
- **Purpose**: Creates spend vs save decisions every turn
- **Examples**: Spend momentum for doubt reduction, flow advancement, card draw

## STRATEGIC DECISION FRAMEWORK

### Turn 1 Example (4 focus available):
**Option A**: burning_conviction (5 focus)
- Requires: mental_reset first for extra focus
- Result: +5 momentum but no other effects

**Option B**: passionate_plea + pause_reflect (3+1 focus)
- Result: +3 momentum, -1 doubt (balanced approach)

**Option C**: build_rapport + quick_insight (2+1 focus)
- Result: +2 momentum, +1 card (card advantage)

### Mid-Game Example (8 momentum, 3 doubt, 5 focus):
**Option A**: Accept basic goal (8 momentum)
- Safe but misses enhanced rewards

**Option B**: clear_confusion (spend 2 momentum, -3 doubt)
- Safer position but delays goal achievement

**Option C**: moment_of_truth (spend 4 momentum, +2 flow)
- Long-term investment in better connection state

## IMPLEMENTATION PHASES

### Phase 1: JSON Card Definitions
- Replace all 20 cards in 02_cards.json
- Implement new focus costs and effect types
- Add momentum spending mechanics

### Phase 2: Parser Updates
- Add support for momentum spending effects
- Implement new scaling formulas
- Update effect resolver for conversions

### Phase 3: Balance Testing
- Verify turn length (5-8 turns target)
- Confirm strategic tension exists
- Validate no "play everything" exploit

### Phase 4: Elena Integration
- Test with desperate_request conversation type
- Verify doubtPerListen: 1 balance works
- Confirm momentum erosion creates pressure

## SUCCESS METRICS

### Strategic Depth
- ✅ Clear power curve forces planning decisions
- ✅ Resource conversion creates meaningful tradeoffs
- ✅ Multiple viable strategies each turn
- ✅ Focus constraints prevent "play everything"

### Card Diversity
- ✅ Every card has unique strategic purpose
- ✅ No duplicate effects or redundancy
- ✅ Cards interact and synergize meaningfully
- ✅ Scaling effects reward different game states

### Economic Balance
- ✅ Conversation length 5-8 turns
- ✅ Goals achievable but require planning
- ✅ Momentum erosion creates time pressure
- ✅ Elena conversation shows all effect types

This implementation transforms the conversation system from a momentum-generation simulator into a strategic resource management puzzle with meaningful decisions every turn.