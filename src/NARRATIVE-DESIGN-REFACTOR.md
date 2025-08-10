# Narrative Design Refactor: Verb Separation & Emotional Authenticity

## Core Design Change: Strict Mechanical Separation

### The Problem
The original verb system mixed mechanical effects across verbs, diluting their identity:
- HELP choices manipulated queues (accepting letters)
- NEGOTIATE choices built relationships 
- INVESTIGATE choices changed tokens
- Generic text that felt like database management

### The Solution: Pure Mechanical Verbs

#### HELP (Relationship Building)
**Mechanical Rule**: ONLY gives Trust tokens, NEVER manipulates queue
- 1 Attention: +2 Trust - Building connection
- 2 Attention: +4 Trust - Deeper commitment  
- 3 Attention: +6 Trust (locked) - Profound bond

**Narrative Pattern**: Focuses on human connection and support
- Desperate: "I understand your urgency. Let me see what I can do."
- Hostile: "I know things have been difficult. Let me make it right."
- Calculating: "I'd like to build something meaningful with you."
- Withdrawn: "I'd like to help, if you'll let me."

#### NEGOTIATE (Queue & Token Management)
**Mechanical Rule**: ONLY affects queue/tokens, NEVER relationships
- 1 Attention Options:
  - Swap positions (costs Commerce)
  - Open queue interface (free)
  - Accept letter to queue (free)
- 2 Attention: Move to position 1 (costs Status)
- 3 Attention: Trade tokens (Commerce → Status)

**Narrative Pattern**: Focuses on priorities and logistics
- Accept: "I can add your letter to my deliveries."
- Interface: "Let's review my delivery priorities."
- Priority: "I need to prioritize this delivery."
- Trade: "Let's make a trade - my connections for your influence."

#### INVESTIGATE (Information Discovery)
**Mechanical Rule**: ONLY reveals info at time cost, NO token changes
- 1 Attention Options:
  - Learn schedules (30 min)
  - Reveal letter properties (20 min)
- 2 Attention: Deep investigation (45 min)
- 3 Attention: Discover networks (60 min, locked)

**Narrative Pattern**: Focuses on understanding and discovery
- Schedule: "Tell me about everyone's schedules."
- Letter: "I need to understand what's really at stake here."
- Deep: "Help me understand the deeper connections."
- Network: "Who's really behind all of this?"

## Emotional Authenticity Features

### 1. State-Responsive Text Generation
Text now responds to NPC emotional states while maintaining mechanical clarity:
- **Desperate NPCs** get crisis-focused language
- **Hostile NPCs** get reconciliation language
- **Calculating NPCs** get balanced, strategic language
- **Withdrawn NPCs** get gentle, trust-building language

### 2. Free Flavor Choices
Added 1-2 free choices per conversation that:
- Cost 0 attention
- Maintain current state (no mechanical changes)
- Add personality based on:
  - NPC profession (Merchant, Scholar, Craftsman, Noble)
  - Emotional state (empathy choices for Desperate/Hostile)

Examples:
- "How's business treating you?" (Merchant)
- "Any interesting discoveries lately?" (Scholar)
- "Is there anything else on your mind?" (Desperate state)

### 3. Dynamic Narrative Text
Instead of static templates, choices now generate contextual text:
- Same mechanics, different framing
- Responds to urgency without changing effects
- Maintains the medieval wayfarer fantasy

## Design Benefits

### For Players
- **Clear Mental Model**: Each verb has ONE purpose
- **Emotional Connection**: Text feels human, not transactional
- **Strategic Clarity**: Know exactly what each choice does
- **Narrative Variety**: Same mechanics feel different per NPC

### For the System
- **Clean Architecture**: No mixed responsibilities
- **Easy to Balance**: Each verb has isolated effects
- **Extensible**: Can add new text patterns without changing mechanics
- **Testable**: Clear separation of concerns

## Implementation Notes

### Key Files Changed
- `VerbOrganizedChoiceGenerator.cs` - Complete refactor of choice generation
- Added narrative text generators for each verb
- Added free flavor choice generator
- Removed queue manipulation from HELP
- Moved letter acceptance to NEGOTIATE
- Cleaned up mechanical descriptions

### Mechanical Patterns (Unchanged)
- Attention costs remain the same
- Token amounts remain balanced
- Time costs for investigation unchanged
- Lock requirements preserved

### What This Preserves
- The human moment in every interaction
- The fantasy of being a medieval letter carrier
- The weight of social obligations
- The emergent storytelling through mechanics

## Future Considerations

### Potential Additions (Without Breaking Core Design)
1. **More Free Choices**: Profession-specific questions that reveal lore
2. **Contextual Reactions**: NPCs comment on your token balance
3. **Emotional Memory**: NPCs remember how you've treated them
4. **Seasonal Variations**: Different text in different seasons

### What to Avoid
- ❌ Adding mechanical effects to free choices
- ❌ Mixing verb purposes again
- ❌ Creating special rules for specific NPCs
- ❌ Reducing relationships to progress bars

## Summary

This refactor maintains the systematic, mockup-aligned mechanics while adding the emotional authenticity that makes conversations feel human. Players interact with people, not content nodes. The mechanics serve the story, not the other way around.

The medieval wayfarer navigates a web of human obligations, not a spreadsheet of tasks.