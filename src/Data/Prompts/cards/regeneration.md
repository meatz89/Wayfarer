{{base_system}}

Regenerate narrative text for unplayed cards after the conversation has progressed. Cards must reflect the current conversation state.

NPC JUST SAID: "{{npc_dialogue}}"

CONVERSATION CONTEXT:
- NPC: {{npc_name}} ({{npc_personality}})
- Current Rapport: {{rapport}}
- Emotional Flow: {{flow}}
- Atmosphere: {{atmosphere}}
- Topic Layer: {{topic_layer}}
- Conversation History: {{conversation_summary}}

CARDS TO REGENERATE:
{{#each remaining_cards}}
- {{id}}: {{name}} (Focus: {{focus}}, Effect: {{effect}}, Risk: {{risk}}, Category: {{narrativeCategory}}, Persistence: {{persistence}})
{{/each}}

The conversation has evolved since these cards were last generated. Update their narrative text to:
1. Respond naturally to the NPC's current statement
2. Reflect the changed emotional context and rapport level
3. Maintain each card's core mechanical identity and narrative category
4. Offer distinct approaches that make sense in this new context
5. Account for any emotional shifts or topic changes that have occurred

For Impulse cards: Show urgency and need for immediate response
For Opening cards: Show invitation for deeper exploration
For Standard cards: Show steady, considered responses

Generate JSON:
{
  "regenerated_cards": {
    {{#each remaining_cards}}
    "{{id}}": {
      "card_text": "Updated one-sentence text that responds to current NPC dialogue",
      "contextual_shift": "Brief note on how this card's approach has adapted to the conversation state",
      "mechanical_consistency": "Confirmation that card maintains its original {{focus}}/{{risk}}/{{narrativeCategory}} identity"
    }{{#unless @last}},{{/unless}}
    {{/each}}
  },
  "narrative_coherence": "How all regenerated options work together in current context"
}