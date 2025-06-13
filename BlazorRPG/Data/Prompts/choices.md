IMPORTANT: Generate ONLY the raw choice content with no meta-commentary, JSON formatting, or structural elements.

# WAYFARER'S RESOLVE CHOICE GENERATION
{PROMPT_CONTEXT}

## Period Authenticity
- Medieval character perspective appropriate to the current situation
- Concrete reactions to visible, tangible elements of the scene
- Experience-based responses to the specific challenge at hand
- Avoid modern analytical terms or abstract strategic thinking

## IMMEDIATE CONTEXT FOCUS
Create choices based on:
1. Character's current physical location and immediate surroundings
2. People, objects, or elements physically present RIGHT NOW
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

## NARRATIVE CONTINUITY
- If introduction establishes character focus on specific elements, prioritize related choices
- Don't abandon established interest unless urgent intervention occurs
- Allow follow-through on established attention while offering alternatives
- Balance continuation with scene flexibility

## CONTEXTUAL GROUNDING
- ALL choices must reference elements explicitly present in current narrative
- NO choices about objects, people, or situations not mentioned in immediate scene
- Verify each choice against what was actually described
- Ground every choice in the specific, current environment

## CHOICE TEMPLATES
Use these mechanical templates for choice generation:
- GatherInformation: Examine, question, investigate
- EstablishTrust: Social bonding, demonstrate competence, show vulnerability
- AssertDominance: Intimidate, command, challenge
- AvoidConflict: Deflect, withdraw, appease
- DirectAction: Physical intervention, immediate problem-solving

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