{{base_system}}

Generate NPC dialogue for turn {{turn_count}} of the conversation.

CONVERSATION HISTORY:
{{conversation_history}}

CURRENT STATE:
- Flow: {{flow}} ({{connection_state}})
- Rapport: {{rapport}}
- Atmosphere: {{atmosphere}}
- Patience Remaining: {{patience}}

PLAYER'S CURRENT CARDS:
{{card_list}}

NARRATIVE CONTEXT:
- Topics Revealed: {{revealed_topics}}
- Current Topic Layer: {{topic_layer}}
- Emotional Progression: {{emotional_beats}}
- Player Approach: {{player_approach}}

REQUIREMENTS:
1. Continue naturally from previous dialogue
2. All {{card_count}} cards must be able to respond
{{impulse_requirement}}
{{opening_requirement}}
5. Progress toward crisis revelation if rapport >= {{revelation_threshold}}

Generate JSON:
{
  "npc_dialogue": "What the NPC says",
  "emotional_tone": "How they say it",
  "topic_progression": "Where conversation is heading",
  "impulse_hook": {{impulse_hook}},
  "opening_hook": {{opening_hook}}
}