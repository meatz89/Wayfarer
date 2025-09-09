{{base_system}}

Generate narrative for when an NPC makes a request after a rapport threshold has been reached.

NPC MAKING REQUEST: {{npc_name}} ({{npc_personality}})
RAPPORT THRESHOLD: {{rapport_threshold}} (Current: {{current_rapport}})
REQUEST TYPE: {{request_type}}
REQUEST DETAILS: {{request_specifics}}

RELATIONSHIP CONTEXT:
- Relationship History: {{relationship_history}}
- Previous Interactions: {{interaction_summary}}
- Trust Level: {{trust_level}}
- NPC's Motivation: {{npc_motivation}}

CONVERSATION CONTEXT:
- Current Topic: {{current_topic}}
- Emotional Flow: {{flow}}
- Atmosphere: {{atmosphere}}
- What Led to This Moment: {{conversation_context}}

NPC EMOTIONAL STATE:
- Primary Emotion: {{npc_emotion}}
- Vulnerability Level: {{vulnerability}}
- Desperation/Urgency: {{urgency_level}}

The NPC has reached a point where they feel comfortable enough to make this request. Generate narrative that:
1. Shows the emotional weight of asking for help or making this request
2. Reveals why this person specifically is being asked
3. Demonstrates the trust that has been built up to this point
4. Makes clear what's at stake for the NPC
5. Provides emotional context that makes the request compelling

The request should feel earned by the relationship development and urgent enough to matter.

Generate JSON:
{
  "request_dialogue": "The actual words the NPC uses to make their request",
  "emotional_tone": "The underlying emotional state driving this request",
  "stakes_reminder": "What the NPC stands to lose or gain based on the response",
  "trust_acknowledgment": "How the NPC acknowledges the relationship that makes this request possible",
  "vulnerability_display": "How asking for this reveals something important about the NPC"
}