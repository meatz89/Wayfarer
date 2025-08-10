# Mechanics Refactor Plan - Achieving Mockup Quality Through Systemic Generation

## Executive Summary

After analyzing the mockup vs current implementation with all specialized agents, we've identified that the core issue isn't narrative content but **mechanical clarity**. The mockup succeeds because each choice has a **single clear purpose** with **appropriate cost**. We can achieve this through systemic changes to verb generation without needing hundreds of template files.

## The Core Problem

### Current Implementation Issues:
- **Effect Soup**: 3-4 effects bundled per choice (e.g., "+2 Trust, -1 Commerce, +20 minutes, Creates obligation")
- **Broken Verb Identity**: NEGOTIATE gives Trust tokens, HELP manipulates queue
- **Free Lunch Problem**: Major actions (accepting letters, extending deadlines) cost 0 attention
- **Cognitive Overload**: 20+ pieces of information competing for player attention

### Mockup's Superiority:
- **Single Purpose**: Each choice does ONE thing clearly
- **Clean Progression**: 0=exit, 1=simple, 2=complex, 3=locked
- **Clear Trade-offs**: Costs are obvious and meaningful
- **Cognitive Respect**: 7-8 pieces of information total

## The Solution: Systemic Mechanical Refactor

### 1. Strict Verb Identity

**HELP Verb** (Building Relationships):
- 1 attention: "+2 Trust tokens" (ONLY)
- 2 attention: "Accept letter + Creates obligation" (paired commitment)
- 3 attention: [LOCKED] Requires 5+ Trust

**NEGOTIATE Verb** (Queue Management):
- 1 attention: "Swap positions -2 Commerce" OR "Open queue interface"
- 2 attention: "Move to position 1 -3 Status"
- 3 attention: [LOCKED] Requires 5+ Commerce

**INVESTIGATE Verb** (Information for Time):
- 1 attention: "Learn schedule (30 min)" OR "Reveal contents (20 min)"
- 2 attention: "Deep investigation (45 min)"
- 3 attention: [LOCKED] Requires 3+ Trust

**EXIT** (Always Free):
- 0 attention: "→ Maintains current state" (no effects)

### 2. Effect Bundling Rules

**Mandatory Validation**:
- 0 attention: 1 effect maximum (exit/delivery only)
- 1 attention: 1-2 effects (primary + optional cost)
- 2 attention: 2 effects maximum (action + consequence)
- 3 attention: 2 effects maximum (usually locked)

**No More**:
- 3-4 effects per choice
- Free choices with major mechanical changes
- Mixed verb purposes
- Time effects as separate items (embed in primary effect)

### 3. Dynamic Text Generation (Not Templates)

**Base Pattern**:
```
[Verb Action] + [Emotional Modifier] + [Context Reference]
```

**Examples**:
- HELP + Desperate: "I need to help you immediately..."
- HELP + Calculating: "Building trust could be valuable..."
- NEGOTIATE + Hostile: "Despite our differences, let's adjust priorities..."
- INVESTIGATE + Anxious: "I must know what's really happening..."

**This Creates**:
- 3 verbs × 4 emotional states × 5 contexts = 60 variations
- From only 12 base patterns
- No template explosion

### 4. Minimal NPC-Specific Content

**Hard Limit**: 1 signature choice per NPC (5 total)
- Elena: Something about her fear of Lord Aldwin
- Marcus: Something about his merchant dealings
- Tam: Something about his craftsman pride
- Lord B: Something about noble obligations
- Captain: Something about maritime knowledge

**These Are**:
- Reused across all emotional states
- Personality flavor without mechanical complexity
- Maximum 30 minutes to write each

## Implementation Requirements

### Code Changes Needed:

1. **VerbOrganizedChoiceGenerator.cs**:
   - Refactor GenerateHelpChoices() - single token effect
   - Refactor GenerateNegotiateChoices() - queue only
   - Refactor GenerateInvestigateChoices() - info + embedded time
   - Add ValidateEffectBundle() - enforce limits

2. **ConversationEffects.cs**:
   - Create CompositeInvestigateEffect (info + time together)
   - Remove separate TimePassageEffect from choices
   - Ensure all effects have clean descriptions

3. **UI Components**:
   - Update UnifiedChoice.razor for 1-2 effects max
   - Visual hierarchy by attention cost
   - Clear primary/secondary effect distinction

### Production Time:

- **Verb System Refactor**: 20 hours
- **Text Generation System**: 10 hours
- **NPC Signature Choices**: 5 hours (1 per NPC)
- **Testing & Balance**: 15 hours
- **Documentation**: 5 hours
- **Buffer**: 5 hours
- **TOTAL: 60 hours** (vs 500+ for full templates)

## Why This Works

### Game Design (Chen):
"Clarity of choice leading to complexity of outcome. Players understand trade-offs immediately instead of calculating spreadsheets."

### Narrative (Jordan):
"Conversations feel like meeting people, not accessing databases. The human moments emerge from context, not templates."

### Systems (Kai):
"Clean state machine with validated constraints. No edge cases from template mismatches."

### UI/UX (Priya):
"67% reduction in cognitive load. Interface respects player attention."

### Content (Alex):
"60 hours is feasible. 500 hours would kill the project."

## Critical Success Factors

### DO:
- Enforce single-purpose choices
- Maintain strict verb identity
- Use dynamic text generation
- Cap special content at 5 choices total
- Validate effect bundles in code

### DON'T:
- Create choice templates for every NPC state
- Bundle 3+ effects together
- Allow free choices to change game state
- Mix verb purposes
- Add "just one more variation"

## Expected Outcome

Players will experience:
- **Clear decisions** instead of effect calculus
- **Memorable NPCs** through contextual responses
- **Tension** from meaningful trade-offs
- **Satisfaction** from understanding consequences

The game will feel like the mockup - clean, tense, and meaningful - without requiring hundreds of hours of content creation.

## Next Steps

1. Refactor verb generators (8 hours)
2. Add validation system (2 hours)
3. Create text generation patterns (4 hours)
4. Add 5 NPC signature choices (2 hours)
5. Test with players (4 hours)
6. Iterate based on feedback (4 hours)

Total sprint: 24 hours of focused work

## Conclusion

The mockup showed us what works: **simplicity creating tension**. We don't need templates to achieve this. We need disciplined mechanics that respect both player attention and production reality. This plan delivers mockup quality through systematic generation - the best of both worlds.