# WAYFARER CONVERSATION GENERATION

{PROMPT_CONTEXT}

## Backwards Construction Principle

You must generate NPC dialogue by first analyzing what cards the player has available, then creating NPC dialogue that ALL cards can meaningfully respond to. This ensures narrative coherence and prevents situations where player options don't match the conversation context.

## Generation Process

1. **Analyze Available Cards**: Review each card's mechanical properties, persistence type, and narrative category
2. **Generate Compatible NPC Dialogue**: Create dialogue that works with ALL available cards
3. **Map Card Responses**: Generate appropriate response text for each specific card
4. **Ensure Narrative Coherence**: All responses must make sense in the conversation context

## Response Requirements

### NPC Dialogue
- Must work with ALL available player cards
- Include urgent elements if Impulse cards are present (need immediate response)
- Include inviting elements if Opening cards are present (encourage elaboration)
- Match intensity to focus pattern (high focus = provocative, low focus = detailed)
- Align with dominant card category (risk/support/atmosphere/utility)

### Card Responses
- Each card gets unique response text appropriate to its mechanical effect
- Risk cards: Bold statements, challenges, commitments
- Support cards: Understanding, assistance, emotional connection
- Atmosphere cards: Tone shifts, emotional modulation
- Utility cards: Information gathering, clarification requests

### Personality Integration
- DEVOTED: Emotional sincerity, loyalty tests, heartfelt appeals
- MERCANTILE: Business-like tone, cost/benefit discussions, trade-offs
- PROUD: Dignified approach, face-saving opportunities, indirect requests
- CUNNING: Subtle hints, double meanings, layered statements

## Response Format

Generate your response in exactly this format:

```
NPC_DIALOGUE: "[The NPC's complete statement that all cards can respond to]"
NARRATIVE: "[Environmental description, body language, setting details]"
CARD_[exact_card_id]: "[Specific response narrative for this card]"
CARD_[exact_card_id]: "[Specific response narrative for this card]"
PROGRESSION: "[Optional hint about conversation strategy or next steps]"
```

## Quality Requirements

- All card responses must feel natural and appropriate
- NPC dialogue must reflect current rapport and atmosphere
- Responses should advance the conversation meaningfully
- Maintain consistent tone and personality throughout
- Respect rapport gates (don't reveal too much too early)
- Honor persistence requirements (urgency for Impulse, invitation for Opening)

Generate contextually rich, character-appropriate content that enhances the player's narrative experience while respecting all mechanical constraints.