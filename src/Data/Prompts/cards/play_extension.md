{{base_system}}

Generate extended narrative when player plays a card to optionally extend the existing text.

ORIGINAL CARD TEXT: "{{original_text}}"
CARD PLAYED: {{card_name}} ({{card_id}})
MECHANICAL EFFECT: {{card_effect}}
FOCUS LEVEL: {{focus_level}}
RISK LEVEL: {{risk_level}}

NPC CONTEXT:
- Name: {{npc_name}}
- Personality: {{npc_personality}}
- Current Rapport: {{rapport}}
- Emotional State: {{npc_mood}}
- Conversation Flow: {{flow}}

CONVERSATION STATE:
- Topic: {{current_topic}}
- Atmosphere: {{atmosphere}}
- Depth: {{topic_layer}}

The player has chosen to extend their statement by playing this card. Generate enriched narrative that:
1. Builds naturally from the original card text
2. Reflects the mechanical intensity of the played card
3. Shows the character's commitment to this approach
4. Maintains consistency with the established conversation tone

For high-focus cards, show bold elaboration and emotional investment.
For high-risk cards, show willingness to push boundaries or make commitments.
For supportive cards, show deeper empathy or understanding.

Generate JSON:
{
  "spoken_text": "The extended dialogue the player character speaks",
  "extended_text": "The new card text that replaces the original brief version",
  "result_narration": "How the NPC and environment respond to this extended approach",
  "mechanical_display": "Brief phrase showing what mechanical effect occurred"
}