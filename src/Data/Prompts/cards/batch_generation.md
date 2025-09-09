{{base_system}}

Generate narrative text for ALL cards simultaneously. Each must respond to the NPC's dialogue.

NPC JUST SAID: "{{npc_dialogue}}"

EMOTIONAL CONTEXT:
- NPC: {{npc_name}} ({{npc_personality}})
- Rapport: {{rapport}}
- Flow: {{flow}}
- Atmosphere: {{atmosphere}}
- Conversation Depth: {{topic_layer}}

CARDS TO GENERATE:
{{cards_detail}}

Generate one-sentence narratives that:
1. All respond sensibly to "{{npc_dialogue}}"
2. Match their mechanical intensity (higher focus = bolder statement)
3. Reflect their risk level (higher risk = more aggressive approach)
4. Offer meaningfully different approaches

Generate JSON:
{
  "card_narratives": {
    {{card_narrative_template}}
  },
  "narrative_coherence": "Brief note on how all options work together"
}