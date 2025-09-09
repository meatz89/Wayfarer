# Conversation Narrative Generation System

## Core Principle: Backwards Narrative Construction

Generate NPC dialogue by analyzing what cards the player COULD play next, ensuring every possible player choice makes narrative sense. This inverts traditional dialogue trees - instead of NPC speaks → player responds, we analyze player options → generate NPC dialogue that all options can address → map card narratives as responses.

## System Architecture

### Phase 1: Mechanical State Analysis

Gather current conversation state:
```
{
  flow: 0-24 (connection distance)
  rapport: -50 to +50 (trust level)
  atmosphere: current atmosphere effect
  focus: available focus points
  patience: remaining patience
  
  activeCards: [
    {
      id: card identifier
      focus: cost to play
      difficulty: success percentage
      effect: mechanical outcome
      persistence: Persistent/Impulse/Opening
      narrativeCategory: risk/support/pressure/probe
    }
  ]
  
  npcProfile: {
    personality: Devoted/Mercantile/Proud/etc
    crisis: core problem/need
    currentTopic: what's being discussed
    revealedInfo: what's been shared
    emotionalState: derived from flow + rapport
  }
}
```

### Phase 2: NPC Dialogue Generation

#### Step 1: Card Compatibility Analysis

Examine all cards in player's hand to find common narrative threads:

**Persistence Requirements**:
- **Impulse present**: NPC must say something requiring immediate response
- **Opening present**: NPC must say something inviting elaboration
- **All Persistent**: NPC can speak normally

**Focus Distribution Patterns**:
- All low focus (1-2): NPC should be verbose, giving player many response angles
- Mixed focus: NPC should present layered statement with multiple engagement levels
- All high focus (3+): NPC should say something provocative warranting strong response

**Effect Category Mapping**:
- Rapport risks present: NPC should expose vulnerability or make claim
- Atmosphere setters present: NPC should be in transitional emotional state
- Utility cards present: NPC should provide information worth gathering

#### Step 2: Dialogue Generation Formula

```
NPC_Dialogue = BaseStatement + EmotionalModifier + TopicLayer + PersistenceHook

Where:
- BaseStatement = derived from current topic and rapport range
- EmotionalModifier = colored by atmosphere and flow
- TopicLayer = depth determined by rapport threshold
- PersistenceHook = specific setup for Impulse/Opening cards
```

### Phase 3: Card Narrative Mapping

Each card's narrative depends on:
1. Its mechanical category
2. Current rapport level
3. The NPC dialogue it's responding to
4. Its risk/reward profile

#### Narrative Categories by Mechanical Effect

**Risk Cards** (rapport with failure consequence):
- Low rapport: Probing/testing statements
- Mid rapport: Supportive but pushing statements  
- High rapport: Bold commitments or challenges

**Atmosphere Cards** (set conversation tone):
- Create narrative transitions
- Represent emotional shifts
- Signal conversation phase changes

**Utility Cards** (draw/focus):
- Information gathering
- Buying time to think
- Redirecting conversation

#### Rapport-Based Narrative Layers

```
Rapport 0-5 (Surface):
- Observations about immediate situation
- Polite deflections
- Testing boundaries

Rapport 6-10 (Gateway):
- Showing understanding
- Sharing relevant experience
- Gentle challenges

Rapport 11-15 (Personal):
- Direct emotional support
- Personal commitments
- Confronting core issues

Rapport 16+ (Intimate):
- Deep vulnerability
- Unconditional support
- Life-changing offers
```

### Phase 4: Topic Progression Management

#### Three-Layer Topic Structure

**Layer 1: Deflection Topics**
- What NPC discusses when avoiding real issue
- Rapport 0-5 range
- Examples: Weather, business, local gossip

**Layer 2: Gateway Topics**
- Related to crisis but not directly addressing it
- Rapport 6-10 range
- Examples: Family, responsibilities, past experiences

**Layer 3: Core Crisis**
- The actual problem/need
- Rapport 11+ range
- Examples: Forced marriage, debt, betrayal

#### Revelation Triggers

Certain conditions cause topic jumps:
- Observation card played → Jump to gateway or core
- Specific rapport threshold → Unlock new topic layer
- Atmosphere change → Emotional break allowing deeper discussion
- Flow transition → Connection state shift changes openness

### Phase 5: Dynamic Response Generation

#### The Generation Loop

1. **Analyze Active Cards**
```
For each card in hand:
  - Identify mechanical category (risk/atmosphere/utility)
  - Check persistence type
  - Note focus cost (intensity indicator)
  - Calculate success probability at current rapport
```

2. **Generate NPC Dialogue**
```
Based on card analysis:
  - Find statement all cards could respond to
  - Include hooks for Impulse/Opening if present
  - Layer in current topic/emotion/atmosphere
  - Ensure progression toward goal if rapport sufficient
```

3. **Map Card Narratives**
```
For each card:
  - Generate response fitting its mechanical effect
  - Scale intensity to focus cost
  - Adjust tone for risk level
  - Ensure it addresses NPC dialogue
```

## Implementation Examples

### Example 1: Low Rapport, Mixed Cards

**Active Cards**:
- "Desperate Gamble" (1 focus, risk, Persistent)
- "Press Hard" (2 focus, risk, Impulse)
- "Draw Information" (2 focus, utility, Opening)

**NPC Dialogue Generated**:
"I can see you're not from around here. People have been talking about the roads getting dangerous." 
(Allows probing response, demanding response, or information gathering)

**Card Narratives**:
- Desperate Gamble: "The roads aren't the only thing that's dangerous here."
- Press Hard: "Stop deflecting. Something's wrong with you specifically."
- Draw Information: "What kind of danger exactly?"

### Example 2: High Rapport, Observation Card Present

**Active Cards**:
- "Safe Passage Knowledge" (0 focus, observation)
- "All In" (3 focus, risk)

**NPC Dialogue Generated**:
"I need to leave the city, but every route is watched or blocked. I'm trapped."

**Card Narratives**:
- Safe Passage Knowledge: "The merchant caravans use hidden routes through the warehouse district."
- All In: "I'll personally guarantee your safe passage."

## Contextual Modifiers

### Personality Modifiers

**Devoted**: NPC dialogue includes emotional appeals, loyalty tests
**Mercantile**: NPC dialogue includes cost/benefit, trade-offs
**Proud**: NPC dialogue includes face-saving, indirect requests
**Cunning**: NPC dialogue includes hints, double meanings

### Atmosphere Coloring

**Volatile**: All dialogue more extreme, emotional
**Pressured**: Dialogue becomes terse, urgent
**Patient**: Dialogue becomes thoughtful, measured
**Focused**: Dialogue becomes direct, clear

### Flow State Influence

**Flow 0-4 (Disconnected)**: Formal, distant, deflecting
**Flow 5-9 (Guarded)**: Cautious sharing, testing
**Flow 10-14 (Neutral)**: Open but professional
**Flow 15-19 (Receptive)**: Warm, inviting deeper connection
**Flow 20-24 (Trusting)**: Completely open, vulnerable

## Continuity Tracking

Track between turns:
```
conversationMemory: {
  topicsCovered: ["deflection_1", "gateway_1", "crisis_mentioned"]
  informationRevealed: ["has_sick_uncle", "needs_letter_delivered"]
  emotionalBeats: ["initial_deflection", "breakthrough", "vulnerability"]
  playerApproach: ["supportive", "pushing", "patient"]
}
```

Use this to:
- Avoid repeating topics
- Build on previous revelations
- Reference earlier statements
- Create narrative callbacks

## Goal Card Integration

When rapport reaches goal threshold:

1. **Check if topic has been revealed**: Has core crisis been discussed?
2. **Generate request setup**: NPC must frame the request naturally
3. **Make request feel earned**: Reference earlier conversation beats
4. **Create urgency**: Why must this happen now?

Example progression:
- Rapport 0-5: "Everything's fine" (deflection)
- Rapport 6-10: "My family situation is complicated" (gateway)
- Rapport 11-14: "I'm being forced into marriage" (crisis revealed)
- Rapport 15+: "Would you deliver my refusal letter?" (request)

## Implementation Algorithm

```
function generateNarrativeContent(mechanicalState, npcProfile) {
  // 1. Analyze what player can do
  cardAnalysis = analyzeActiveCards(mechanicalState.activeCards)
  
  // 2. Determine narrative constraints
  constraints = {
    hasImpulse: cardAnalysis.hasImpulse,
    hasOpening: cardAnalysis.hasOpening,
    focusRange: cardAnalysis.focusRange,
    rapportStage: getRapportStage(mechanicalState.rapport)
  }
  
  // 3. Generate NPC dialogue
  npcDialogue = generateNPCDialogue(
    constraints,
    npcProfile,
    mechanicalState,
    conversationMemory
  )
  
  // 4. Generate card narratives
  cardNarratives = {}
  for (card in mechanicalState.activeCards) {
    cardNarratives[card.id] = generateCardNarrative(
      card,
      npcDialogue,
      mechanicalState.rapport,
      npcProfile.personality
    )
  }
  
  return {
    npcDialogue: npcDialogue,
    cardNarratives: cardNarratives,
    progressionHint: getProgressionHint(mechanicalState, npcProfile)
  }
}
```

## Key Design Principles

1. **Every card in hand must make narrative sense as response to NPC dialogue**
2. **Persistence types create specific narrative requirements**
3. **Focus cost indicates statement intensity/commitment level**
4. **Risk level determines statement boldness**
5. **Rapport gates topic depth, not topic availability**
6. **Same mechanical card has different narrative at different rapport levels**
7. **NPC personality colors expression but not structure**
8. **Atmosphere modifies tone but not content**
9. **Flow represents emotional distance in relationship**
10. **Goal requests feel earned through natural progression**

## Testing Validation

For any generated conversation:
1. Can every card in hand respond sensibly to NPC dialogue?
2. Do Impulse cards feel urgent?
3. Do Opening cards invite follow-up?
4. Does topic progression feel natural?
5. Are rapport gates meaningful?
6. Does the goal request feel earned?
7. Is there variety without repetition?
8. Do card narratives match their mechanical effects?

This system ensures narrative coherence while maintaining mechanical integrity across any conversation context.