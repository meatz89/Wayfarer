# Conversation System Refactor Plan

## Core System Changes

### 1. Momentum as Card Depth Determinant

**Context**: The original system used stats as hard gates for card access, which created a rigid progression where players either had access or didn't. This binary gating conflicted with the dynamic nature of conversations where competence should ebb and flow based on conversational state, not just character stats.

**Problem Solved**: Static stat gates made conversations feel the same regardless of current conversational dynamics. A player with Authority 5 always had the same cards available whether dominating or struggling in the conversation.

**Why This Change**: Momentum-based access creates dynamic conversations where your available options change based on conversational flow. When you're building momentum (doing well), more sophisticated responses become available. When forced to reset (listening after accumulated doubt), you return to basics and must rebuild.

**Verisimilitude**: In real conversations, our ability to make sophisticated arguments depends on the conversational foundation we've built. You can't jump straight to complex points - you build toward them. When a conversation "resets" after tension, we return to simpler exchanges before building again.

**OLD SYSTEM**: Stats directly gate card depth access
**NEW SYSTEM**: Current momentum determines available card depths in draw pool

**Implementation**:
- Current momentum value = maximum card depth that can be drawn
- Cards already in hand remain playable regardless of current momentum
- When momentum drops, only new draws are affected
- Foundation cards (depth 1-2) always remain in draw pool

**Formula**:
```
Available Draw Pool = All cards with depth ≤ current momentum
```

**Design Principle**: This creates impossible choices - push for advanced cards but risk doubt accumulation, or play safe with limited options. The punishment for greed emerges naturally from the system.

### 2. Stat Specialization Bonuses

**Context**: With momentum as the primary gate, stats risked becoming irrelevant for conversation mechanics. We needed stats to matter without returning to hard gates that ignore conversational state.

**Problem Solved**: Pure momentum-based access would make all characters play identically. A merchant and a warrior would have the same conversational options at the same momentum, breaking character identity.

**Why This Change**: Stat bonuses create specialization advantages without exclusion. A social specialist can access their specialized tools even when struggling (low momentum), while maintaining the universal foundation that everyone shares. This preserves character identity while respecting conversational dynamics.

**Verisimilitude**: People maintain their core competencies even in difficult situations. An experienced negotiator can still employ advanced negotiation tactics even when on the back foot, though their overall options are limited.

**OLD SYSTEM**: Stats are hard gates for card access
**NEW SYSTEM**: Stats provide depth bonuses for their associated cards

**Stat Bonus Progression**:
- Levels 1-3: +0 depth bonus
- Levels 4-6: +1 depth bonus
- Levels 7-9: +2 depth bonus
- Level 10: +3 depth bonus

**Formula**:
```
Card Accessible = (Card Depth ≤ Momentum) OR 
                  (Card has Stat Type AND Card Depth ≤ Momentum + Stat Bonus)
```

**Example**: 
- Momentum 5, Rapport 7 (+2 bonus)
- Can draw ANY card depth 1-5
- Can draw RAPPORT cards depth 1-7

**Design Principle**: Specialization provides advantage without creating exclusive content. Every player can theoretically access any card through momentum, but specialists get easier access to their tools.

### 3. LISTEN Action Refactor

**Context**: The original system had doubt persisting through LISTEN actions with only incremental relief. This created a death spiral where high doubt became increasingly difficult to manage, often leading to conversation failure through accumulation rather than player choice.

**Problem Solved**: Persistent doubt created a punishment spiral where one bad exchange could doom the entire conversation. Players needed multiple LISTEN actions to recover from high doubt, making conversations feel punishing rather than strategic.

**Why This Change**: Complete doubt reset creates a clear, immediate trade-off. Players can always save a conversation by sacrificing progress. This transforms LISTEN from a delaying tactic to a strategic reset button with meaningful cost. The choice becomes "Do I risk continuing with high doubt for greater reward, or reset safely but lose progress?"

**Verisimilitude**: In real conversations, taking a moment to truly listen and reset does clear tension, but it also means stepping back from your argumentative position. You can't maintain aggressive momentum while also defusing tension.

**OLD SYSTEM**: Doubt persists, applies momentum tax
**NEW SYSTEM**: Doubt resets to 0, momentum reduced by doubt cleared

**New LISTEN Mechanics**:
```
On LISTEN:
1. Calculate doubt_cleared = current_doubt
2. Set doubt = 0
3. Reduce momentum by doubt_cleared (minimum 0)
4. Draw cards = 3 + abs(min(cadence, 0))
5. Reduce cadence by 3
```

**Example**:
- Current state: Momentum 8, Doubt 4, Cadence 2
- LISTEN action: Doubt → 0, Momentum → 4, Draw 3 cards, Cadence → -1
- Next LISTEN would draw 4 cards due to -1 cadence

**Design Principle**: Every mechanic must offer clear trade-offs with perfect information. Players always know exactly what LISTEN will cost and can make informed decisions.

### 4. Cadence Linear Scaling

**Context**: The original -3 threshold for bonus cards was an arbitrary breakpoint that created optimal play patterns around hitting specific values. This type of threshold-based design adds complexity without strategic depth.

**Problem Solved**: Threshold mechanics create "correct" play patterns where players optimize for hitting magic numbers. This reduces strategic diversity and makes conversations feel mechanical rather than natural.

**Why This Change**: Linear scaling makes every point of cadence meaningful. Each step into negative cadence directly improves your LISTEN value, creating smooth progression rather than sudden jumps. This eliminates optimal threshold targeting in favor of continuous tactical consideration.

**Verisimilitude**: The more someone has been dominating a conversation, the more they learn when they finally stop to listen. There's no magical point where listening suddenly becomes more valuable - it scales naturally with how much you've been talking.

**OLD SYSTEM**: Threshold at -3 for bonus card
**NEW SYSTEM**: Linear scaling for negative cadence

**Formula**:
```
Cards drawn on LISTEN = 3 + abs(min(cadence, 0))
```

**Examples**:
- Cadence 0: Draw 3 cards
- Cadence -2: Draw 5 cards
- Cadence -5: Draw 8 cards

**Design Principle**: Avoid arbitrary thresholds. Every mechanical progression should be smooth and predictable, allowing players to calculate exact outcomes.

### 5. Starting Momentum

**Context**: Starting all conversations at the same momentum made every opening identical, reducing variety. However, starting momentum couldn't be too high or players would skip the foundation-building phase entirely.

**Problem Solved**: Identical openings made conversations feel repetitive. Experienced characters should have slightly smoother conversation starts without bypassing the fundamental build-up phase.

**Why This Change**: Tying starting momentum to stats provides minor progression reward without breaking the core loop. Even masters start with basics, just slightly more options. The formula ensures even maximum stats only provide modest starting advantage.

**Verisimilitude**: Experienced conversationalists can establish rapport slightly faster, but everyone still needs to warm up a conversation. You don't start negotiations with your strongest arguments.

**Formula**:
```
Starting Momentum = 2 + floor(highest_stat / 3)
```

**Progression**:
- Highest stat 1-2: Start at momentum 2
- Highest stat 3-5: Start at momentum 3
- Highest stat 6-8: Start at momentum 4
- Highest stat 9-10: Start at momentum 5

**Design Principle**: Progression rewards should be meaningful but not game-breaking. Starting with 5 momentum vs 2 provides advantage without bypassing core mechanics.

## Card Architecture Refactor

### Foundation Card Distribution

**Context**: The original design didn't distinguish between repeatable techniques and one-time statements for Foundation cards. This created the death spiral where playing Statement Foundations early would deplete Initiative generation, making advanced cards unplayable even when available.

**Problem Solved**: If Initiative-generating cards were Statements, they'd go to Spoken pile and become unavailable. Players would literally run out of fuel mid-conversation, creating unwinnable states through normal play rather than poor decisions.

**Why This Change**: Making Initiative-generating cards Echoes ensures fuel remains available throughout the conversation. This prevents mechanical deadlock while maintaining the strategic tension of resource management. Players can always generate Initiative, but must balance it with progress-generating Statements.

**Verisimilitude**: Conversational techniques like active listening, encouraging nods, and thoughtful pauses are naturally repeatable. You don't "use up" the ability to listen actively. Meanwhile, specific points and opening statements are made once.

**Depth 1-2 Cards** (Always available):
- 70% should be Echo type (repeatable techniques)
- 30% can be Statement type (specific opening points)
- ALL Initiative-generating cards must be Echo type

**Examples of Foundation Echoes**:
```
"Active Listening" - Echo, 0 Initiative, +2 Initiative
"Encouraging Nod" - Echo, 0 Initiative, +1 Initiative, +1 Momentum
"Clarifying Question" - Echo, 1 Initiative, +2 Initiative, Draw 1
"Thoughtful Pause" - Echo, 0 Initiative, +1 Initiative, -1 Cadence
```

**Examples of Foundation Statements**:
```
"Introduce Myself" - Statement, 1 Initiative, +2 Momentum
"State Purpose" - Statement, 2 Initiative, +3 Momentum
"Opening Offer" - Statement, 1 Initiative, +1 Momentum, Draw 1
```

**Design Principle**: Prevent mechanical deadlock through system design, not safety mechanisms. The solution emerges naturally from card types rather than special rules.

### Card Depth Distribution by Type

**Context**: Different conversation types should feel mechanically distinct, not just thematically different. The ratio of Foundation to advanced cards fundamentally changes how a conversation plays.

**Problem Solved**: Uniform card distributions would make all conversation types feel identical mechanically, just with different flavor text. This breaks verisimilitude where different social situations require different approaches.

**Why This Change**: Varying the depth distribution creates distinct mechanical personalities for each conversation type. Support conversations have many Foundations for sustained emotional exchange. Authority conversations have fewer Foundations, forcing careful resource management. The mechanics match the theme.

**Verisimilitude**: Emotional support requires lots of listening and encouragement (Foundations). Business negotiations build toward agreement (progressive depth). Authority confrontations are tense with fewer "safe" options.

**Support Conversations**:
- Depth 1-2: 40% (heavy Foundation presence)
- Depth 3-4: 30%
- Depth 5-6: 20%
- Depth 7-8: 10%

**Request Conversations**:
- Depth 1-2: 35%
- Depth 3-4: 30%
- Depth 5-6: 25%
- Depth 7-8: 10%

**Investigation Conversations**:
- Depth 1-2: 30%
- Depth 3-4: 35% (more mid-range tools)
- Depth 5-6: 25%
- Depth 7-8: 10%

**Authority Conversations**:
- Depth 1-2: 25% (fewer Foundations)
- Depth 3-4: 30%
- Depth 5-6: 30%
- Depth 7-8: 15%

**Design Principle**: Mechanical variety through systematic differences, not random variation. Each conversation type has a clear mechanical identity.

### Statement/Echo Decision Framework

**Context**: The Echo/Statement distinction isn't arbitrary - it represents whether something can be meaningfully repeated in a conversation. This decision fundamentally affects resource flow and conversation sustainability.

**Problem Solved**: Without clear guidelines, card types would be assigned randomly, creating inconsistent resource patterns and potential deadlocks. The framework ensures mechanical function aligns with thematic sense.

**Why This Change**: Clear rules for card type assignment ensure consistent design and prevent accidental deadlocks. The framework makes future card creation straightforward while maintaining system integrity.

**Verisimilitude**: You can ask multiple clarifying questions but only reveal a secret once. You can pause thoughtfully repeatedly but make a specific promise once. The mechanics match intuitive understanding.

**Make it an Echo if**:
- It generates Initiative (mandatory)
- It represents a repeatable technique
- It's a conversational tool rather than content
- Thematically can be done multiple times

**Make it a Statement if**:
- It represents specific information or position
- It builds conversation substance
- It's a one-time reveal or commitment
- Other cards reference Statement count for bonuses

**Design Principle**: Mechanical requirements should align with thematic sense. When mechanics and theme align, players intuitively understand the system.

## Critical Formula Updates

### Initiative System

**Context**: Initiative represents conversational energy and attention - your ability to make substantive contributions. Unlike typical builder/spender systems where the resource resets each turn, Initiative persists as accumulated social capital within the conversation.

**Problem Solved**: Turn-based resource reset would eliminate multi-turn planning and setup/payoff moments. Persistent Initiative enables strategic reserves and complex multi-card combinations.

**Why This Change**: Persistent Initiative creates strategic depth where players must plan multiple turns ahead. Do you spend Initiative immediately for small gains or save for powerful plays? This decision tree creates the impossible choices core to the design philosophy.

```
Base Initiative = 3 + floor(highest_stat / 3)
Initiative never resets during conversation
Can accumulate without limit
```

### Momentum System

**Context**: Momentum represents conversational progress and complexity. As momentum builds, the conversation deepens and more sophisticated exchanges become possible. The system allows reduction but maintains forward possibility.

**Problem Solved**: Hard momentum caps would create ceiling effects where optimal play becomes trivial. Unlimited momentum with exponentially harder maintenance creates natural soft caps through system pressure.

**Why This Change**: No hard maximum allows skilled players to push for exceptional results while system pressure (doubt accumulation) provides natural limiting factors. The minimum of 0 ensures forward progress is always possible.

```
Starting Momentum = 2 + floor(highest_stat / 3)
Minimum Momentum = 0
Maximum Momentum = No hard limit (goals at 8/12/16)
Momentum reduction from LISTEN = min(current_doubt, current_momentum)
```

### Doubt System

**Context**: Doubt represents conversational tension and disengagement. It's the timer pressure forcing action and the consequence of aggressive play. The refactor transforms it from persistent burden to clearable-but-costly tension.

**Problem Solved**: Persistent doubt created death spirals. The new system makes doubt manageable through strategic sacrifice, ensuring conversations remain salvageable through player choice rather than luck.

**Why This Change**: Binary threat (doubt at 10 ends conversation) with complete reset option (LISTEN clears all doubt) creates clear decision points. Players always know the exact cost of safety versus the risk of continuing.

```
Doubt starts at 0
Doubt increases from card effects and cadence
Doubt maximum = 10 (conversation ends)
LISTEN action: Doubt → 0, Momentum -= doubt_cleared
```

### Draw Pool Calculation

**Context**: This formula determines which cards enter the available pool each draw. It's the core mechanism linking momentum progression to card access while preserving stat specialization.

**Problem Solved**: Simple momentum gating would eliminate character differentiation. Pure stat gating would ignore conversational dynamics. This hybrid approach preserves both systems' strengths.

**Why This Change**: The OR condition allows specialization to shine even during struggle. A master negotiator (Commerce 10) can access advanced commerce cards even at low momentum, maintaining character identity through mechanical advantage.

```
For each card in conversation type deck:
  if (card.depth <= current_momentum) OR 
     (card.stat_type == player.stat AND 
      card.depth <= current_momentum + stat_bonus):
    Add to available draw pool
```

## Migration Checklist

### Phase 1: Core System Updates

**Context**: These changes must happen first as they fundamentally alter how conversations flow. Other systems depend on these core mechanics functioning correctly.

- [ ] Update LISTEN action to reset doubt and reduce momentum
- [ ] Change cadence to linear scaling for card draws
- [ ] Implement momentum-based draw pool filtering
- [ ] Add stat specialization bonuses

### Phase 2: Card Refactoring

**Context**: Once core systems work, cards must be updated to function within the new framework. This phase ensures content aligns with mechanical changes.

- [ ] Audit all Foundation cards for Echo/Statement classification
- [ ] Convert ALL Initiative-generating cards to Echo type
- [ ] Verify card depth distributions per conversation type
- [ ] Validate no Initiative generation exists above depth 2

### Phase 3: Balance Validation

**Context**: With systems and content updated, extensive testing ensures the intended play patterns emerge naturally from the system rather than requiring artificial constraints.

- [ ] Test conversation flow with new momentum gates
- [ ] Verify Foundation card availability prevents deadlock
- [ ] Confirm signature cards (depth 3-5) accessible mid-conversation
- [ ] Validate greedy play creates appropriate punishment

### Phase 4: NPC Integration

**Context**: NPC systems must be validated against the new conversation flow to ensure personality rules and special decks integrate properly without breaking core mechanics.

- [ ] Update personality rules for new system
- [ ] Verify burden cards don't break momentum flow
- [ ] Test observation deck integration
- [ ] Validate stranger encounters with simplified mechanics

## Design Validation Tests

### Test 1: Greedy Player Trap

**Purpose**: Verify that rushing momentum without building Initiative foundation creates self-inflicted difficulty requiring careful recovery. This ensures player blame for suboptimal situations.

1. Start conversation (momentum 2-3)
2. Rush momentum to 8 without building Initiative
3. Verify hand clogs with expensive unplayable cards
4. Confirm forced LISTEN drops player back
5. Validate slow recovery required

**Success Criteria**: Player recognizes their greed created the situation, not random chance or system unfairness.

### Test 2: Specialist Advantage

**Purpose**: Confirm that stat specialization provides meaningful advantage without excluding non-specialists from basic competence. Character builds should feel different.

1. Create Rapport 10 character
2. Start conversation at momentum 3
3. Verify access to depth 6 Rapport cards (3 + 3 bonus)
4. Confirm other cards limited to depth 3
5. Test meaningful specialization advantage

**Success Criteria**: Specialist can execute their style even when struggling, but cannot access all advanced cards without momentum.

### Test 3: Foundation Sustainability

**Purpose**: Ensure conversations cannot mechanically deadlock through normal play. Initiative generation must remain available throughout extended conversations.

1. Play 20+ turn conversation
2. Verify Initiative generation never fully depletes
3. Confirm Echo Foundations remain drawable
4. Test Statement accumulation in Spoken
5. Validate no mechanical deadlock

**Success Criteria**: Long conversations remain playable with careful resource management, only ending through doubt or goal achievement.

### Test 4: Conversation Flow Arc

**Purpose**: Verify that conversations naturally create dramatic arcs through mechanical pressure rather than scripted events.

1. Track momentum curve over full conversation
2. Verify natural wave pattern from doubt pressure
3. Confirm Foundation → Standard → Advanced progression
4. Test LISTEN creates meaningful reset points
5. Validate climax possibility at high momentum

**Success Criteria**: Momentum curves show 3-5 natural peaks and valleys creating satisfying conversation rhythm.

## Risk Mitigation

### Potential Issue: Hand Clogging
**Why This Could Happen**: Drawing expensive cards during high momentum that become unplayable after momentum drops could create permanent hand pollution.

**Solution**: Hand size limit of 7, must discard down at LISTEN

**Why This Solution**: Forces players to regularly refresh their tactical options while preventing permanent disadvantage from temporary setbacks.

### Potential Issue: Foundation Depletion
**Why This Could Happen**: Too many Statement Foundations could deplete Initiative generation over long conversations.

**Solution**: Minimum 70% of Foundations as Echoes

**Why This Solution**: Ensures sustainable fuel generation while allowing some Statements for variety and progression.

### Potential Issue: Momentum Stagnation
**Why This Could Happen**: If all Foundations only generate Initiative, momentum might never grow without advanced cards.

**Solution**: Some Foundations generate small momentum

**Why This Solution**: Enables slow but steady progress even with limited card access, preventing complete stagnation.

### Potential Issue: Stat Irrelevance
**Why This Could Happen**: If momentum completely determines access, stats become meaningless for conversations.

**Solution**: Depth bonuses create meaningful specialization

**Why This Solution**: Preserves character identity through mechanical advantage without returning to hard gates.

### Potential Issue: Conversation Length
**Why This Could Happen**: Unlimited turn counts could create endless conversations without resolution.

**Solution**: Time cost = 1 + Statements in Spoken (not total turns)

**Why This Solution**: Links conversation duration to substance rather than mechanical cycling, creating natural endpoints.

## Implementation Priority

**Context**: Priority based on system dependencies and impact on player experience. Critical changes enable everything else, while low priority changes are polish that can be iteratively refined.

1. **Critical**: LISTEN refactor (doubt reset, momentum reduction)
   - *Why*: Fundamental to new conversation flow
2. **Critical**: Momentum-based draw pool
   - *Why*: Core progression mechanic
3. **Critical**: Foundation cards as Echoes
   - *Why*: Prevents mechanical deadlock
4. **High**: Stat specialization bonuses
   - *Why*: Preserves character differentiation
5. **High**: Linear cadence scaling
   - *Why*: Removes arbitrary complexity
6. **Medium**: Starting momentum formula
   - *Why*: Provides progression reward
7. **Medium**: Card depth rebalancing
   - *Why*: Creates conversation variety
8. **Low**: Personality rule adjustments
   - *Why*: Fine-tuning after core works
9. **Low**: Visual indicator updates
   - *Why*: Polish for clarity

## Success Metrics

**Context**: Measurable outcomes that indicate the refactor achieves its design goals. These metrics validate that the system creates intended play patterns through emergence rather than enforcement.

- Conversations create 3-5 natural momentum waves
  - *Why*: Shows dynamic flow rather than linear progression
- Players can reach Premium goals (16 momentum) with skill
  - *Why*: Confirms system allows excellence through mastery
- Greedy play creates recoverable but punishing situations
  - *Why*: Validates player blame for suboptimal states
- Specialists feel meaningfully different without exclusion
  - *Why*: Proves stat system provides identity without gating
- No mechanical deadlocks possible
  - *Why*: Ensures system sustainability
- Average conversation length 12-20 turns
  - *Why*: Shows appropriate pacing without exhaustion
- Foundation cards played throughout entire conversation
  - *Why*: Confirms Echo/Statement balance works

## Design Philosophy Validation

This refactor achieves the core design philosophy through emergent mechanics rather than rules:

**Elegance Over Complexity**: Each system serves exactly one purpose. Momentum determines access. Stats provide specialization. Initiative enables action. No overlap or confusion.

**Verisimilitude Throughout**: Every mechanic makes narrative sense. Building conversation momentum to access deeper exchanges. Listening resets tension but loses progress. Techniques can be repeated, statements cannot.

**Perfect Information**: All calculations visible. Players know exact LISTEN cost, precise card access, specific stat bonuses. No hidden probabilities or success rates.

**No Soft-Lock Architecture**: Foundation Echoes ensure Initiative generation. Minimum momentum of 0 ensures progress possibility. LISTEN always available as reset option.

**Deterministic Systems**: If you meet requirements and pay costs, cards always work. No failure chance or random modifiers. Strategic planning over probability management.