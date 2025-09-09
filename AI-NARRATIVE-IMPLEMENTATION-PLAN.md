# AI Narrative Generation - Implementation Plan

## Date: 2025-01-09

## Executive Summary

The current AI narrative generation system only generates NPC dialogue, while card narratives use simple fallback text. This implementation plan details how to fix the system to generate contextual card narratives using AI, following the provided solution architecture.

## Critical Discovery: Two AI Calls Required

The solution uses **TWO separate AI calls per LISTEN action**, not one unified call:
1. **First AI Call**: Generate NPC dialogue using `prompts/npc/dialogue.md`
2. **Second AI Call**: Generate card narratives using `prompts/cards/batch_generation.md` with the NPC dialogue as context

This is the **KEY ARCHITECTURAL DIFFERENCE** from the current implementation.

## Current State Analysis

### What's Working ✅
- AI provider infrastructure (OllamaClient, AIConversationNarrativeProvider)
- Basic prompt template system with placeholder replacement
- NPC dialogue generation on LISTEN actions
- JSON fallback system with mechanical generation
- Conversation history tracking
- 5-second timeout with fallback

### What's Broken ❌
- **No AI generation of card narratives** - only using fallback text like "Carefully respond thoughtfully"
- **Only ONE AI call made** - should be TWO calls per LISTEN
- **Card narratives not contextual** - don't respond to specific NPC dialogue
- **Missing templates** - play_extension.md, regeneration.md, observation.md, goal_request.md
- **Template inconsistencies** - field names and structure don't match solution

## Implementation Tasks

### Task 1: Fix AIConversationNarrativeProvider.cs

**File**: `/src/Subsystems/Conversation/NarrativeGeneration/Providers/AIConversationNarrativeProvider.cs`

**Current Code (Line 48-86):**
```csharp
public NarrativeOutput GenerateNarrativeContent(...)
{
    // Only makes ONE AI call for NPC dialogue
    string prompt = DetermineAndBuildPrompt(...);
    string aiResponse = GenerateAIResponseAsync(prompt);
    NarrativeOutput output = ParseAIResponse(aiResponse, ...);
    return ValidateAndEnhanceOutput(output, ...); // Uses fallback for cards
}
```

**Required Changes:**
```csharp
public NarrativeOutput GenerateNarrativeContent(...)
{
    // Step 1: Generate NPC dialogue (existing)
    string npcPrompt = DetermineAndBuildPrompt(state, npcData, activeCards, analysis);
    string npcResponse = await GenerateAIResponseAsync(npcPrompt);
    NarrativeOutput output = ParseAIResponse(npcResponse, activeCards, state);
    
    // Step 2: NEW - Generate card narratives using NPC dialogue as context
    if (!string.IsNullOrEmpty(output.NPCDialogue))
    {
        string cardPrompt = promptBuilder.BuildBatchCardGenerationPrompt(
            state, npcData, activeCards, output.NPCDialogue);
        string cardResponse = await GenerateAIResponseAsync(cardPrompt);
        ParseCardNarratives(cardResponse, output);
    }
    
    // Step 3: Validate and fill any gaps
    return ValidateAndEnhanceOutput(output, state, npcData, activeCards, analysis);
}
```

### Task 2: Add ParseCardNarratives Method

**Add to AIConversationNarrativeProvider.cs:**
```csharp
private void ParseCardNarratives(string jsonResponse, NarrativeOutput output)
{
    if (string.IsNullOrEmpty(jsonResponse)) return;
    
    try
    {
        var jsonDoc = JsonDocument.Parse(jsonResponse);
        var root = jsonDoc.RootElement;
        
        if (root.TryGetProperty("card_narratives", out JsonElement cardNarratives))
        {
            foreach (var cardProp in cardNarratives.EnumerateObject())
            {
                string cardId = cardProp.Name;
                var cardData = cardProp.Value;
                
                if (cardData.TryGetProperty("card_text", out JsonElement cardText))
                {
                    output.CardNarratives[cardId] = cardText.GetString();
                }
            }
        }
    }
    catch
    {
        // AI parsing failed - fallback will handle it
    }
}
```

### Task 3: Fix dialogue.md Template

**File**: `/src/Data/Prompts/npc/dialogue.md`

**Current Issues:**
- Uses `"npc_dialogue"` instead of `"dialogue"` in JSON
- Simple placeholders instead of {{#each}} loops
- Missing conditional blocks for impulse/opening

**Required Changes:**
```markdown
{{base_system}}

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

### Task 4: Fix batch_generation.md Template

**File**: `/src/Data/Prompts/cards/batch_generation.md`

**Current Issues:**
- Uses generic placeholders like {{cards_detail}} and {{card_narrative_template}}
- Doesn't iterate cards properly

**Required Changes:**
```markdown
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

### Task 5: Update PromptBuilder.cs

**File**: `/src/Subsystems/Conversation/NarrativeGeneration/Providers/PromptBuilder.cs`

**Add Support for {{#each}} Loops:**
```csharp
private string ProcessTemplate(string template, Dictionary<string, object> placeholders)
{
    // Handle {{#each}} loops
    template = ProcessEachLoops(template, placeholders);
    
    // Handle conditionals
    template = ProcessConditionals(template, placeholders);
    
    // Handle simple placeholders
    template = ProcessPlaceholders(template, placeholders);
    
    return template;
}

private string ProcessEachLoops(string template, Dictionary<string, object> placeholders)
{
    var eachRegex = new Regex(@"\{\{#each\s+(\w+)\}\}(.*?)\{\{/each\}\}", 
                               RegexOptions.Singleline);
    
    return eachRegex.Replace(template, match =>
    {
        var collectionName = match.Groups[1].Value;
        var loopTemplate = match.Groups[2].Value;
        
        if (placeholders.TryGetValue(collectionName, out object collection) && 
            collection is IEnumerable items)
        {
            var result = new StringBuilder();
            int index = 0;
            bool isLast = false;
            
            foreach (var item in items)
            {
                // Build loop context
                var loopContext = BuildLoopContext(item, index, isLast);
                var processed = ProcessPlaceholders(loopTemplate, loopContext);
                result.Append(processed);
                index++;
            }
            
            return result.ToString();
        }
        
        return string.Empty;
    });
}
```

### Task 6: Create Missing Templates

**Create**: `/src/Data/Prompts/cards/play_extension.md`
```markdown
{{base_system}}

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

**Create**: `/src/Data/Prompts/cards/regeneration.md`
```markdown
{{base_system}}

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

**Create**: `/src/Data/Prompts/special/observation.md`
```markdown
{{base_system}}

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

**Create**: `/src/Data/Prompts/special/goal_request.md`
```markdown
{{base_system}}

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

## Testing Checklist

### Functional Tests
- [ ] Two AI calls happen per LISTEN action
- [ ] NPC dialogue generates correctly (first call)
- [ ] Card narratives generate using NPC dialogue as context (second call)
- [ ] Each card gets unique, contextual narrative
- [ ] Fallback works if AI times out
- [ ] Card text displays AI-generated narrative, not static Description

### Integration Tests
- [ ] Build succeeds without errors
- [ ] Application starts successfully
- [ ] Conversation UI shows generated narratives
- [ ] LISTEN action triggers both AI calls
- [ ] SPEAK action uses pre-generated card narrative

### Quality Tests
- [ ] Card narratives respond sensibly to NPC dialogue
- [ ] Higher focus cards have bolder narratives
- [ ] Impulse cards reflect urgency
- [ ] Opening cards invite elaboration
- [ ] Narratives match medieval fantasy tone

## Success Criteria

The implementation is successful when:
1. Every LISTEN action makes TWO AI calls (NPC dialogue + card narratives)
2. Cards display contextual AI-generated text that responds to NPC dialogue
3. Fallback system still works when AI is unavailable
4. No compilation errors or runtime exceptions
5. UI displays dynamic card narratives instead of static text

## Risk Mitigation

- **Risk**: Breaking existing functionality
- **Mitigation**: Make changes incrementally, test after each step

- **Risk**: AI timeout causes poor UX
- **Mitigation**: Keep 5-second timeout, ensure fallback generates quality text

- **Risk**: Template syntax errors
- **Mitigation**: Test template processing thoroughly, add error handling

## Timeline

1. **Phase 1** (Core Changes): 2-3 hours
   - Fix AIConversationNarrativeProvider
   - Add ParseCardNarratives method
   - Update PromptBuilder

2. **Phase 2** (Templates): 1-2 hours
   - Fix existing templates
   - Create missing templates

3. **Phase 3** (Testing): 1 hour
   - Build and runtime testing
   - Manual conversation testing
   - Fix any issues found

Total estimated time: 4-6 hours

## Notes

- The key insight is that the solution uses TWO separate AI calls, not one unified call
- Card narratives must be generated AFTER NPC dialogue to ensure contextual responses
- Template structure must match exactly for proper JSON parsing
- Fallback system remains critical for when AI is unavailable