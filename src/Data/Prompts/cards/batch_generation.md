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
{{#each cards}}
- {{id}}: {{name}} (Focus: {{focus}}, Effect: {{effect}}, Difficulty: {{difficulty}}, Persistence: {{persistence}}, Category: {{narrativeCategory}})
{{/each}}

Generate one-sentence narratives that:
1. All respond sensibly to "{{npc_dialogue}}"
2. Match their mechanical intensity (higher focus = bolder statement)
3. Reflect their risk level (higher risk = more aggressive approach)
4. Offer meaningfully different approaches

Generate JSON:
{
  "card_narratives": {
    {{#each cards}}
    "{{id}}": {
      "card_text": "One sentence that appears on card",
      "approach_type": "Type of approach based on {{narrativeCategory}}"
    }{{#unless @last}},{{/unless}}
    {{/each}}
  },
  "narrative_coherence": "Brief note on how all options work together"
}