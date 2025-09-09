{{base_system}}

Generate NPC dialogue for turn {{turn_count}} of the conversation.

CONVERSATION HISTORY:
{{#each history}}
{{player_action}} → {{npc_response}}
{{/each}}

CURRENT STATE:
- Flow: {{flow}} ({{connection_state}})
- Rapport: {{rapport}}
- Atmosphere: {{atmosphere}}
- Patience Remaining: {{patience}}

PLAYER'S CURRENT CARDS:
{{#each cards}}
- {{name}} (Focus: {{focus}}, Effect: {{effect}})
{{/each}}

NARRATIVE CONTEXT:
- Topics Revealed: {{revealed_topics}}
- Current Topic Layer: {{topic_layer}}
- Emotional Progression: {{emotional_beats}}
- Player Approach: {{player_approach}}

REQUIREMENTS:
1. Continue naturally from previous dialogue
2. All {{card_count}} cards must be able to respond
{{#if has_impulse}}
3. Include an impulse hook for dramatic moment
{{/if}}
{{#if has_opening}}
4. Include an opening hook for new conversation thread
{{/if}}
5. Progress toward crisis revelation if rapport >= {{revelation_threshold}}

Generate JSON:
{
  "dialogue": "What the NPC says",
  "emotional_tone": "How they say it",
  "topic_progression": "Where conversation is heading"{{#if has_impulse}},
  "impulse_hook": "Brief phrase that triggers dramatic response"{{/if}}{{#if has_opening}},
  "opening_hook": "Brief phrase that opens new conversation thread"{{/if}}
}