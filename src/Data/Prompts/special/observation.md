{{base_system}}

Generate narrative for playing a special observation card that contains knowledge or insight.

OBSERVATION CARD: {{card_name}} ({{card_id}})
CARD KNOWLEDGE: {{card_knowledge}}
INSIGHT TYPE: {{insight_type}}
OBSERVATION TARGET: {{observation_target}}

CONVERSATION CONTEXT:
- NPC: {{npc_name}} ({{npc_personality}})
- Current Topic: {{current_topic}}
- Rapport Level: {{rapport}}
- Conversation Flow: {{flow}}
- Atmosphere: {{atmosphere}}

PLAYER CONTEXT:
- Previous Knowledge: {{player_knowledge}}
- Relationship with {{npc_name}}: {{relationship_type}}
- Current Situation: {{player_intent}}

The player is using their knowledge or insight to contribute meaningfully to the conversation. Generate narrative that:
1. Shows the player character sharing their observation naturally
2. Demonstrates how this knowledge connects to the current conversation
3. Reveals the depth of the player's understanding or experience
4. Creates a moment of recognition or breakthrough in the dialogue
5. Advances the conversation through shared understanding

The observation should feel earned and relevant, not like showing off knowledge for its own sake.

Generate JSON:
{
  "card_text": "What appears on the card as the observation or insight",
  "spoken_text": "The dialogue the player character speaks when sharing this knowledge",
  "breakthrough_narration": "How this observation creates a moment of connection or understanding",
  "mechanical_translation": "Brief description of what this observation accomplishes mechanically",
  "npc_recognition": "How the NPC responds to this demonstration of knowledge or insight"
}