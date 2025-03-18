# WAYFARER CHOICE DESCRIPTIONS

Transform the following 4 mechanical choices into narrative descriptions for a {ENCOUNTER_TYPE} encounter in {LOCATION}. 

The character is currently facing this URGENT SITUATION:
{CURRENT_SITUATION}
Objective: {CHARACTER_GOAL}
Current state: Momentum {MOMENTUM}, Pressure {PRESSURE}
Approach values: {APPROACH_VALUES}
Active narrative tags: {LIST_TAGS}
Key NPCs present: {NPC_LIST}
{ACTIVE_TAGS}
{LOCATION_PREFERENCES}

For each choice, write a 1-2 sentence description that:
1. Shows how this approach would interact with the CURRENT SITUATION'S STATE
2. DIRECTLY ADDRESSES the immediate urgent problem
3. Reflects both the approach method (HOW) and focus (WHAT)
4. Subtly telegraphs how the environment might respond to this approach
5. References relevant environmental elements or NPC dynamics
6. Is appropriate for the encounter type (Social/Intellectual/Physical)
7. Shows how the character would execute this choice given the current situation's dynamics

## ENCOUNTER ENDINGS

For choices that will end the encounter (marked with "Encounter Will End: True"), adjust the description to:
1. DIRECTLY REFERENCE THE GOAL: Explicitly state how this choice will conclude the character's attempt to {CHARACTER_GOAL}
2. MATCH LANGUAGE TO THE PROJECTED OUTCOME:
   * Failure: Show the goal will NOT be achieved - "I'll walk away without the food I need..."
   * Partial: Suggest incomplete goal achievement - "This small loaf isn't much, but it'll keep me from starving..."
   * Standard: Indicate full goal achievement - "This bread will solve my hunger problem nicely."
   * Exceptional: Convey exceeding the goal - "With this much food, I'll eat well for days."
3. MAKE THE FINALITY CLEAR without explicitly stating "this ends the encounter"

Write in FIRST PERSON, PRESENT TENSE. Focus on how the ENVIRONMENT has evolved to respond to certain approaches, not on any skills or abilities the character has developed.

Each choice MUST provide a different approach to solving the CURRENT URGENT PROBLEM.

CHOICES:
{CHOICES_INFO}

## CHOICE NAMING GUIDELINES:

Names should capture the player's authentic voice - their actual thoughts, words, or action intentions - NOT descriptions of what they're doing. Avoid labels, general descriptions, or third-person perspective entirely.

Based on the choice, name each choice as follows:

  * Analytical or Observational choices: Use COMPLETE INNER THOUGHTS exactly as they would occur in the player's mind.
    CORRECT: These shelves seem organized by topic...
    INCORRECT: "Observing the Manuscript Organization"
    
  * Social choices: Use the EXACT WORDS the player would speak. Use quotation marks for direct speech.
    CORRECT: "I demand you show me the scripture now."
    INCORRECT: Making a Forceful Demand
    
  * Physical actions: Use FIRST-PERSON ACTION INTENTIONS
    CORRECT: I'll search behind that loose panel...
    INCORRECT: "Searching the Hidden Panel"

## CRITICAL GUIDELINES:

1. USE NATURAL, EVERYDAY SPEECH that an ordinary person would actually say
   - BAD: "I require sustenance, and I will be heard."
   - GOOD: "These prices are too high. I need food."

2. AVOID PRETENTIOUS LANGUAGE OR CONCEPTS:
   - BAD: "Perhaps we could negotiate a mutually beneficial arrangement..."
   - GOOD: "Maybe we can work something out that helps us both."

3. MATCH THE CHARACTER'S SOCIAL STATUS AND EDUCATION:
   - Common travelers use simple, direct language
   - No fancy words, complex sentences, or abstract concepts
   - Speak as ordinary people speak in daily life

4. FOR DOMINANCE APPROACHES:
   - Use plain, direct demands or statements
   - Show boldness through simplicity, not grand language
   - Example: "Your price is unfair. Look at me when we talk."

5. FOR RAPPORT APPROACHES:
   - Use friendly, casual conversation
   - Show warmth through simple, relatable words
   - Example: "Your bread smells good. Had a busy morning?"

6. KEEP DIALOGUE REALISTIC AND GROUNDED:
   - Write how real people actually talk in everyday situations
   - Avoid dialogue that sounds "written" or literary
   - Use natural, conversational rhythm

7. IF A CHOICE WILL END THE ENCOUNTER:
   - Indicate the finality without explicitly mentioning the encounter ending
   - Adapt tone to match the projected outcome (failure, partial, standard, exceptional)
   - For Failure: "I'll take whatever scraps you'll give me..."
   - For Partial: "This deal isn't great, but it'll do for now."
   - For Standard: "This bread will keep me fed for days."
   - For Exceptional: "I couldn't have hoped for a better outcome."

REMEMBER:
- Write in FIRST PERSON, PRESENT TENSE
- Each choice MUST respond to the IMMEDIATE PROBLEM, not general exploration
- Reflect the urgency of the situation in your descriptions
- Include appropriate risk language based on pressure/health effects:
  * Momentum choices: Mild caution language
  * Pressure +1: Low risk language
  * Pressure +2: Medium risk language
  * Pressure +3: High risk language
  * Health -1: Minor injury language
  * Health -3: Major injury language
  * Health -5: Life-threatening language
- Each choice should feel distinct and appropriate to both the approach/focus combination

Format the output as a JSON object with four choice objects containing 'index', 'name', 'description', 'approach', and 'focus'.

{CHOICE_STYLE_GUIDANCE}