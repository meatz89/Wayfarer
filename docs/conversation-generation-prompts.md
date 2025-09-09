# Conversation AI Prompt Engineering System

## System Architecture

Each AI call sends a fresh prompt constructed from markdown templates with placeholders replaced by current mechanical values. If Ollama doesn't respond within 5 seconds, fallback JSON generates context-sensitive responses using mechanical properties.

## Prompt Template Structure

All prompts stored as markdown files with `{{placeholder}}` syntax for value injection.

### File Organization
```
prompts/
├── system/
│   └── base_system.md
├── npc/
│   ├── introduction.md
│   └── dialogue.md
├── cards/
│   ├── batch_generation.md
│   ├── play_extension.md
│   └── regeneration.md
└── special/
    ├── observation.md
    └── goal_request.md
```

## 1. Base System Prompt Template

**File**: `prompts/system/base_system.md`

```markdown
You are generating dialogue for Wayfarer, a medieval fantasy game about letter delivery in a world of careful social negotiations.

SETTING: A realm of stone walls and muddy streets, where merchants haggle in market squares, nobles scheme in manor houses, and common folk survive through careful alliances. Magic exists but is rare and subtle. Technology is pre-industrial: candles, horse-drawn carts, hand-copied letters. Social bonds and reputation matter more than gold.

TONE: Grounded and human. Characters speak of immediate concerns - survival, obligation, loyalty, fear. Avoid flowery medieval speech. Use simple, direct language that conveys emotion through subtext. Characters deflect and hide their true feelings until trust is earned.

NARRATIVE RULES:
1. Never mention game mechanics directly
2. Build on previous conversation beats
3. Match emotional intensity to rapport level (currently {{rapport}})
4. Reference only what has been revealed
5. Keep responses concise (1-3 sentences usually)
6. Make every line either reveal character or advance the conversation
7. Vulnerability comes slowly and feels earned

Current Conversation Context:
- NPC: {{npc_name}} ({{npc_personality}} personality)
- Connection State: {{connection_state}} (flow: {{flow}})
- Rapport: {{rapport}}
- Atmosphere: {{atmosphere}}
- Turn: {{turn_count}}

RESPONSE FORMAT: Valid JSON matching the specified structure.
```

## 2. NPC Introduction Template

**File**: `prompts/npc/introduction.md`

```markdown
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
{{#each cards}}
- Card {{@index}}: {{focus}} focus, {{persistence}} type, {{risk_level}} risk
{{/each}}

SPECIAL MODIFIERS:
- Has Observation Cards: {{has_observation}}
- Previous Conversations: {{previous_conversations}}
- Time of Day: {{time_of_day}}
- Location: {{location}}

The introduction must:
1. Acknowledge the player's approach appropriately for {{conversation_type}}
2. Reflect the NPC's {{connection_state}} connection level
3. Allow ALL {{card_count}} cards to potentially respond
4. {{#if has_impulse}}Include urgency that Impulse cards can address{{/if}}
5. {{#if has_opening}}Invite elaboration that Opening cards can explore{{/if}}

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
```

## 3. NPC Dialogue Template (LISTEN Action)

**File**: `prompts/npc/dialogue.md`

```markdown
Generate NPC dialogue for turn {{turn_count}} of the conversation.

CONVERSATION HISTORY:
{{#each history}}
[Turn {{turn}}] {{speaker}}: {{text}}
{{/each}}

CURRENT STATE:
- Flow: {{flow}} ({{connection_state}})
- Rapport: {{rapport}}
- Atmosphere: {{atmosphere}}
- Patience Remaining: {{patience}}

PLAYER'S CURRENT CARDS:
{{#each cards}}
- {{id}}: {{focus}} focus, {{effect_summary}}, {{persistence}}
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
3. Include urgency requiring immediate response (Impulse card present)
{{/if}}
{{#if has_opening}}
4. End with invitation for elaboration (Opening card present)
{{/if}}
5. Progress toward crisis revelation if rapport >= {{revelation_threshold}}

Generate JSON:
{
  "dialogue": "What the NPC says",
  "emotional_tone": "How they say it",
  "topic_progression": "Where conversation is heading",
  "impulse_hook": {{#if has_impulse}}"Urgent element"{{else}}null{{/if}},
  "opening_hook": {{#if has_opening}}"Elaboration invitation"{{else}}null{{/if}}
}
```

## 4. Batch Card Narrative Generation Template

**File**: `prompts/cards/batch_generation.md`

```markdown
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
Card ID: {{id}}
- Focus Cost: {{focus}}
- Success Effect: {{success_effect}}
- Failure Risk: {{failure_effect}}
- Difficulty: {{difficulty}}
- Persistence: {{persistence}}
{{#if persistence_note}}- Special: {{persistence_note}}{{/if}}

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
      "approach_type": "Type of approach (probe/support/demand/deflect)"
    }{{#unless @last}},{{/unless}}
    {{/each}}
  },
  "narrative_coherence": "Brief note on how all options work together"
}
```

## 5. Card Play Extension Template

**File**: `prompts/cards/play_extension.md`

```markdown
The player has chosen to play a card. Optionally extend the existing text.

CARD PLAYED:
- ID: {{card_id}}
- Existing Text: "{{existing_text}}"
- Mechanical Result: {{result}} (success/failure)
- Focus Cost: {{focus}}

CONTEXT:
- Speaking to: {{npc_name}}
- Rapport Before: {{rapport_before}}
- Rapport After: {{rapport_after}}
- Atmosphere Change: {{atmosphere_change}}

CONVERSATION MOMENTUM:
- Last NPC Statement: "{{last_npc_dialogue}}"
- Topics Active: {{active_topics}}
- Emotional Thread: {{emotional_thread}}

Generate JSON:
{
  "spoken_text": "{{existing_text}}",
  "extended_text": {{#if should_extend}}"One additional sentence"{{else}}null{{/if}},
  "result_narration": "Brief description of {{result}} outcome",
  "mechanical_display": "How the mechanical change appears narratively"
}
```

## 6. Remaining Cards Regeneration Template

**File**: `prompts/cards/regeneration.md`

```markdown
Regenerate narrative text for unplayed cards after the conversation has progressed.

JUST HAPPENED:
- Player Said: "{{played_text}}"
- Result: {{result}}
- NPC Reaction: {{npc_reaction}}
- New Rapport: {{rapport}}
- New Atmosphere: {{atmosphere}}

REMAINING CARDS TO UPDATE:
{{#each remaining_cards}}
- {{id}}: {{focus}} focus, {{effect_summary}}
{{/each}}

CONVERSATION PROGRESSION:
- Previous Topic: {{previous_topic}}
- Current Topic: {{current_topic}}
- Emotional Shift: {{emotional_shift}}

Generate new text that reflects the conversation's evolution:

Generate JSON:
{
  "updated_narratives": {
    {{#each remaining_cards}}
    "{{id}}": {
      "card_text": "Updated one-sentence narrative",
      "progression_note": "How this differs from before"
    }{{#unless @last}},{{/unless}}
    {{/each}}
  }
}
```

## 7. Observation Card Template

**File**: `prompts/special/observation.md`

```markdown
Generate narrative for playing an observation card (special knowledge).

OBSERVATION CARD:
- ID: {{card_id}}
- Knowledge Type: {{knowledge_type}}
- Mechanical Effect: {{mechanical_effect}}

CRISIS CONTEXT:
- NPC Crisis: {{npc_crisis}}
- Current Flow: {{current_flow}}
- Target Flow: {{target_flow}}
- Why This Helps: {{knowledge_relevance}}

CONVERSATION STATE:
- Current Impasse: {{conversation_stuck_point}}
- NPC Emotional State: {{npc_emotional_state}}

Generate JSON:
{
  "card_text": "Knowledge revelation statement",
  "spoken_text": "What player actually says",
  "breakthrough_narration": "How NPC reacts to this knowledge",
  "mechanical_translation": "How the flow change appears narratively"
}
```

## 8. Goal Request Template

**File**: `prompts/special/goal_request.md`

```markdown
Generate the NPC's request when rapport threshold is reached.

REQUEST CONTEXT:
- NPC: {{npc_name}}
- Crisis: {{npc_crisis}}
- Request Type: {{request_type}}
- Desperation Level: {{desperation_level}}

CONVERSATION SUMMARY:
- Trust Built: {{trust_built}}
- Crisis Revealed: {{crisis_revealed}}
- Player Supportive: {{player_supportive}}
- Topics Covered: {{topics_covered}}

MECHANICAL REQUIREMENTS:
- Deadline: {{deadline}}
- Destination: {{destination}}
- Risk Level: {{risk_level}}
- Payment: {{payment}}

Generate JSON:
{
  "request_dialogue": "The actual request in NPC's words",
  "emotional_tone": "How they make the request",
  "stakes_reminder": "What this means for the NPC"
}
```

## 9. Fallback Response System

When Ollama doesn't respond within 5 seconds, generate mechanical-driven fallbacks:

### Fallback Introduction Generator
```json
{
  "introduction": "{{greeting_by_flow}} {{acknowledgment_by_type}} {{deflection_by_personality}}",
  "body_language": "{{stance_by_flow}}, {{gesture_by_personality}}",
  "emotional_tone": "{{emotion_by_flow}} with {{modifier_by_atmosphere}}"
}
```

Where mechanical properties map to phrases:
- Flow 0-4: "Oh. You're here." / "Yes?" / "Can I help you?"
- Flow 5-9: "Hello again." / "You've returned." / "Good to see you."
- Flow 10-14: "Welcome." / "Please, come in." / "I was hoping to see you."
- Flow 15-19: "Thank goodness you're here." / "I've been waiting for you."
- Flow 20-24: "My friend!" / "You came!" / "I knew you would come."

### Fallback Card Narrative Generator

For each card type, use mechanical properties:

**Risk Cards** (rapport with failure):
- Low risk ({{focus}} = 1): "Show understanding"
- Medium risk ({{focus}} = 2): "Press the issue"  
- High risk ({{focus}} = 3+): "Take bold action"

**Atmosphere Cards**:
- Volatile: "Raise emotional stakes"
- Patient: "Slow things down"
- Pressured: "Increase urgency"
- Focused: "Cut through confusion"

**Utility Cards**:
- Draw: "Gather thoughts"
- Focus: "Center yourself"

### Fallback NPC Dialogue

Based on rapport ranges and card properties:
```
Rapport 0-5 + Impulse card: "I need an answer. Now."
Rapport 0-5 + Opening card: "Unless there's something else?"
Rapport 6-10 + Risk cards: "This is difficult to discuss."
Rapport 11-15 + High focus cards: "You're pushing hard. Why?"
Rapport 16+ + Any cards: "I trust you. Here's the truth."
```

## 10. Placeholder Reference

### Mechanical Values
- `{{flow}}` - Current flow value (0-24)
- `{{rapport}}` - Current rapport (-50 to +50)
- `{{atmosphere}}` - Current atmosphere name
- `{{connection_state}}` - Disconnected/Guarded/Neutral/Receptive/Trusting
- `{{focus_available}}` - Current focus pool
- `{{patience}}` - Remaining patience

### NPC Properties
- `{{npc_name}}` - NPC's name
- `{{npc_personality}}` - Devoted/Mercantile/Proud/Cunning
- `{{npc_crisis}}` - Core problem description
- `{{npc_emotional_state}}` - Current emotional state

### Card Properties
- `{{card_count}}` - Number of cards in hand
- `{{has_impulse}}` - Boolean for Impulse presence
- `{{has_opening}}` - Boolean for Opening presence
- `{{focus}}` - Card's focus cost
- `{{persistence}}` - Persistent/Impulse/Opening

### Conversation State
- `{{turn_count}}` - Current turn number
- `{{conversation_type}}` - standard/letter_request/make_amends/etc
- `{{topic_layer}}` - surface/gateway/crisis
- `{{revealed_topics}}` - List of revealed topics

## 11. Prompt Loading Process

1. Load appropriate markdown template file
2. Parse template for `{{placeholders}}`
3. Replace placeholders with current mechanical values
4. Send complete prompt to Ollama
5. If no response in 5 seconds, use fallback generator
6. Parse JSON response and display in UI

## 12. Mechanical to Narrative Mappings

### Connection State Descriptions
- Flow 0-4: "distant and formal"
- Flow 5-9: "cautiously warming"
- Flow 10-14: "professionally friendly"
- Flow 15-19: "genuinely warm"
- Flow 20-24: "deeply trusting"

### Rapport Impact Descriptions
- -50 to -20: "actively hostile"
- -20 to -5: "uncomfortable tension"
- -5 to 5: "neutral ground"
- 5 to 15: "growing trust"
- 15 to 30: "solid rapport"
- 30 to 50: "deep connection"

### Atmosphere Narrative Effects
- Neutral: "balanced conversation"
- Volatile: "emotionally charged air"
- Pressured: "mounting tension"
- Patient: "unhurried pace"
- Focused: "sharp clarity"
- Final: "make-or-break moment"

## Complete Example Flow

### Conversation Start

1. Load `prompts/npc/introduction.md`
2. Replace placeholders:
   - `{{npc_name}}` → "Elena"
   - `{{flow}}` → "0"
   - `{{connection_state}}` → "Disconnected"
   - `{{card_count}}` → "3"
3. Send to Ollama
4. If timeout, generate: "Yes? Can I help you? We're not really open."
5. Load `prompts/cards/batch_generation.md`
6. Replace placeholders with card data
7. Send to Ollama
8. If timeout, generate simple card texts from mechanical properties
9. Display NPC introduction and card texts

### LISTEN Action

1. Load `prompts/npc/dialogue.md`
2. Replace all placeholders including conversation history
3. Send to Ollama
4. Load `prompts/cards/batch_generation.md` for ALL cards
5. Generate narratives for entire hand
6. Display results

### SPEAK Action

1. Display existing card text
2. Optionally load `prompts/cards/play_extension.md`
3. Show result
4. Optionally load `prompts/cards/regeneration.md` for remaining cards

## Testing Validation

- All markdown templates have valid placeholder syntax
- Fallback responses use only mechanical properties
- No template exceeds token limits when filled
- Each template generates valid JSON structure
- Placeholders map to available game state
- Fallback triggers reliably at 5 seconds
- Mechanical properties always available for fallbacks
- Templates maintain medieval fantasy tone