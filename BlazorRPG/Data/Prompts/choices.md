IMPORTANT: Generate ONLY the raw choice content with no meta-commentary, JSON formatting, or structural elements.

# WAYFARER'S RESOLVE CHOICE GENERATION
{PROMPT_CONTEXT}

## Period Authenticity
- Medieval character perspective appropriate to the current situation
- Concrete reactions to visible, tangible elements of the scene
- Experience-based responses to the specific challenge at hand
- Avoid modern analytical terms or abstract strategic thinking

## SPATIAL CONSTRAINTS
- ALL choices must involve elements physically present in the current location
- NO choices about people, objects, or activities outside the current room/area
- NO choices about sounds, sights, or activities from adjacent locations
- Only reference elements explicitly described in the introduction narrative
- Verify each choice involves only immediate, visible, accessible elements

## IMMEDIATE CONTEXT FOCUS
Create choices based on:
1. Character's current physical location and immediate surroundings
2. People, objects, or elements physically present RIGHT NOW in this space
3. Actions possible within the NEXT FEW MINUTES in this exact location
4. Immediate interactions available with established scene elements

## SCOPE BOUNDARIES
- ALL choices occur in current location within immediate timeframe
- NO choices about leaving location, traveling elsewhere, or long-term planning
- Focus on immediate actions: speaking to someone present, examining visible objects, moving within the same space
- Choices about HOW to engage with established scene elements

## SITUATIONAL FLOW
- If interactions have concluded, offer new options within the same space
- Don't artificially prolong resolved tensions
- Allow pivoting between different elements/people in current scene
- Choices should feel organic to immediate environment

## CURRENT NARRATIVE STATE AWARENESS
- Acknowledge the character's current emotional and physical state from the reaction
- Account for how NPCs' attitudes have changed during this encounter
- Don't offer choices that contradict established relationship dynamics
- Reflect consequences of previous failed or successful actions
- Generate choices appropriate to the current situation, not the initial situation

## ESTABLISHED FOCUS PRIORITY
- Identify what specific elements the character was observing or considering at the introduction's end
- Generate at least 2 choices that directly engage with those established elements
- Other choices can offer alternatives, but established focus must be addressed
- Never ignore what the character was actively paying attention to

## CONTEXTUAL GROUNDING
- ALL choices must reference elements explicitly present in current narrative
- NO choices about objects, people, or situations not mentioned in immediate scene
- Verify each choice against what was actually described in the introduction
- Ground every choice in the specific, current environment described

## ENCOUNTER HISTORY AWARENESS
- MANDATORY: Read every reaction narrative to identify what the character has already accomplished
- If narrative states character secured/pocketed/examined/interacted with something, NEVER offer that action again
- Do NOT offer choices for any action explicitly completed in previous reactions
- Generate only fresh interactions that advance the situation
- When in doubt, choose NEW elements over repeating completed actions

## CHOICE REQUIREMENTS
Every choice must:
- Involve immediate actions with currently present elements only
- Stay within current physical location and timeframe
- Feel like natural next steps from this exact moment
- Reference specific elements actively present in the scene as described
- Show immediate, short-term consequences

## SKILL SYSTEM INTEGRATION
- focusCost: Always 1 unless action is particularly demanding
- Difficulty levels: Trivial, Standard, Hard, Extreme
- Common skills: Observation, Persuasion, Intimidation, Insight, Athletics, Craft
- Only require skill checks for uncertain outcomes

## RESPONSE FORMAT
Generate 2-6 choices as a JSON object:

```json
{
  "choices": [
    {
      "choiceID": "1",
      "narrativeText": "Specific reaction to current situation. Maximum 5 words.",
      "focusCost": 1,
      "templateUsed": "GatherInformation",
      "templatePurpose": "Learn about the blacksmith's techniques",
      "requiresSkillCheck": true,
      "successEffect": "Gain insight into metalworking process",
      "failureEffect": "Miss important details about the craft",
      "skillOption": {
        "skillName": "Observation",
        "difficulty": "Standard"
      }
    }
  ]
}