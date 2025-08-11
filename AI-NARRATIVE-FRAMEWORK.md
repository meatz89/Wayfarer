# AI Narrative Framework for Wayfarer

## Overview
Wayfarer uses AI to generate dynamic narrative from mechanical game state. This document defines the framework for AI integration.

## Core Principle
**Mechanics provide context, AI provides narrative.** The game generates mechanical choices based on categorical rules. AI interprets these mechanics into human stories.

## Mechanical Categories

### 0. EXIT (Free)
**Purpose**: Allow graceful conversation exit  
**Mechanical Effect**: Preserve current state  
**AI Context**: Generate farewell based on NPC emotional state  

### 1. EXCHANGE (1 Attention)
**Purpose**: Low-commitment interactions  
**Subtypes**:
- **Information**: Reveal schedule/route/secret → +knowledge
- **Tokens**: Trade token_A for token_B at defined rates
- **Time**: Spend 10-30 minutes for specific benefit

**AI Context**: Type of exchange, what's being traded, NPC personality

### 2. INFLUENCE (2 Attention)
**Purpose**: Change game state significantly  
**Subtypes**:
- **Queue Operations**: Move letter to position X, costs tokens with displaced party
- **Accept Letter**: Take letter into queue at leverage-determined position
- **Refuse Letter**: Remove from queue, burn relationship

**AI Context**: Who's affected, power dynamics (leverage), consequences

### 3. COMMIT (2 Attention)
**Purpose**: Create future obligations  
**Subtypes**:
- **Promise Delivery**: Obligation to deliver by deadline
- **Promise Return**: Must visit NPC again within timeframe
- **Promise Favor**: Owe unspecified future action

**AI Context**: What's being promised, stakes, deadline pressure

### 4. TRANSFORM (3 Attention)
**Purpose**: Major permanent changes  
**Requirements**: Token threshold >= 5 OR secret knowledge OR prior obligation fulfilled  
**Subtypes**:
- **Gain Permit**: Unlock route permanently
- **Form Alliance**: Change relationship type
- **Resolve Crisis**: Clear multiple problems at once

**AI Context**: What's changing, requirements met, permanent consequences

## Context Structure for AI

```typescript
interface ConversationContext {
  // WHO - The NPC in conversation
  npc: {
    id: string                    // "elena"
    name: string                  // "Elena"
    profession: string            // "seamstress"
    emotionalState: string        // "DESPERATE"
    leverage: number              // 2 (power over player)
  }
  
  // WHAT - Letters relevant to this NPC
  relevantLetters: {
    id: string
    sender: string                // "Lord Aldwin"
    recipient: string             // "Elena's cousin"
    deadline: number              // 4 (hours)
    stakes: string                // "REPUTATION"
    position: number              // 3 (queue position)
    contentHook?: string          // "marriage proposal"
  }[]
  
  // WHERE - Current location
  location: {
    type: string                  // "TAVERN"
    name: string                  // "Copper Kettle"
    atmosphere: string            // "warm hearth-light, nervous energy"
  }
  
  // WHEN - Time pressure
  time: {
    hour: number                  // 18 (6 PM)
    timeBlock: string             // "EVENING"
    pressure: {
      npcId: string
      deadline: number
      consequence: string
    }[]
  }
  
  // WHY - Player's current state
  playerState: {
    tokens: Map<string, number>   // elena -> {Trust: 1, Status: 0}
    obligations: {
      npcId: string
      type: string
      deadline: number
    }[]
    attention: number             // 2 (remaining)
    queuePressure: number         // 15 (calculated)
  }
  
  // HISTORY - Past interactions
  history: {
    deliveredCount: number        // Letters delivered for this NPC
    failedCount: number           // Letters failed for this NPC
    refusedCount: number          // Letters refused from this NPC
    lastInteraction: string       // "delivered_urgent_letter"
  }
}
```

## AI Generation Process

### Input
1. **Mechanical Choice**: Category + Subtype + Effects
2. **Game Context**: Full ConversationContext object
3. **Generation Type**: CHOICE_TEXT | NPC_RESPONSE | NARRATIVE_FLAVOR

### Output
```typescript
interface GeneratedNarrative {
  choiceText: string              // "I'll take your letter about the marriage proposal"
  npcResponse: string             // "Her hand trembles as she passes you Lord Aldwin's sealed letter"
  narrativeFlavor: string         // "The wax seal bears the Aldwin crest - a marriage that would save or damn her"
  emotionalTone: string           // "desperate_hopeful"
}
```

## Example: Elena's Marriage Proposal

### Mechanical State
```typescript
{
  category: "INFLUENCE",
  subtype: "ACCEPT_LETTER",
  effects: {
    queuePosition: 3,  // Enters at position 3 due to leverage
    tokenChange: 0,     // No immediate trust (split rewards)
    weight: 2
  }
}
```

### Context Provided
```typescript
{
  npc: {
    name: "Elena",
    profession: "seamstress",
    emotionalState: "DESPERATE",
    leverage: 2
  },
  relevantLetters: [{
    sender: "Lord Aldwin",
    recipient: "Elena's cousin at court",
    deadline: 4,
    stakes: "REPUTATION",
    contentHook: "marriage refusal"
  }],
  location: {
    type: "TAVERN",
    atmosphere: "warm hearth-light, nervous energy"
  }
}
```

### AI Generates
```typescript
{
  choiceText: "I'll take your letter about Lord Aldwin's proposal",
  npcResponse: "Her hand trembles as she passes you the sealed letter. 'If he learns before my cousin can intervene at court, I'll be ruined.'",
  narrativeFlavor: "The letter feels heavier than its weight suggests - a woman's future sealed in wax",
  emotionalTone: "desperate_grateful"
}
```

## Implementation Requirements

### For Current System
1. Mechanical choices must include all context fields
2. Each choice must have clear category and effects
3. Game state must track all context elements

### For AI Integration
1. AI service receives full ConversationContext
2. AI has knowledge of mechanical categories
3. AI understands emotional states and their implications
4. AI can reference past interactions for continuity

## Benefits of This Approach

1. **No Templates Needed**: AI generates fresh narrative each time
2. **Contextual Richness**: Every detail affects the story
3. **Emotional Depth**: AI can add trembling hands, nervous glances
4. **Scalability**: New NPCs/letters just provide new context
5. **Consistency**: Mechanical framework ensures coherent choices

## Testing Without AI

Until AI integration, the system can use placeholder text:
```typescript
function generatePlaceholder(context: ConversationContext): string {
  return `[${context.npc.emotionalState}] ${context.npc.name} discusses ${context.relevantLetters[0]?.stakes || 'matters'}`;
}
```

## Future Enhancements

- **Memory System**: AI remembers previous conversations
- **Personality Profiles**: Deeper NPC characterization
- **Dynamic Relationships**: Evolving based on player actions
- **Contextual Secrets**: Information that changes meaning based on knowledge

## Conclusion

This framework provides the mechanical skeleton for rich narrative. The game ensures every choice matters mechanically. The AI ensures every choice feels human. Together, they create Elena's trembling hand holding Lord Aldwin's proposal - a moment both systematically generated and emotionally real.