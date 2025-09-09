{{base_system}}

Generate the NPC's opening speech for a {{conversation_type}} conversation.

NPC PROFILE:
- Name: {{npc_name}}
- Personality: {{npc_personality}}
- Current Activity: {{npc_activity}}
- Core Crisis: {{npc_crisis}}
- Emotional State: {{npc_emotional_state}}

MECHANICAL CONTEXT:
- Flow: {{flow}} (0-24 scale, currently {{connection_state}})
- Starting Rapport: {{rapport}}
- Player has {{card_count}} cards drawn
- Focus Available: {{focus_available}}

PLAYER'S CARDS:
{{card_summary}}

SPECIAL MODIFIERS:
- Has Observation Cards: {{has_observation}}
- Previous Conversations: {{previous_conversations}}
- Time of Day: {{time_of_day}}
- Location: {{location}}

The introduction must:
1. Acknowledge the player's approach appropriately for {{conversation_type}}
2. Reflect the NPC's {{connection_state}} connection level
3. Allow ALL {{card_count}} cards to potentially respond
{{impulse_requirement}}
{{opening_requirement}}

Generate JSON:
{
  "introduction": "NPC's opening statement",
  "body_language": "Physical cues and demeanor",
  "emotional_tone": "Underlying emotional state",
  "conversation_hooks": {
    "surface": "Safe topic being discussed",
    "gateway": "Deeper topic hinted at",
    "hidden": "Core crisis not yet revealed"
  }
}