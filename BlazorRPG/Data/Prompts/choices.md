Objective: {CHARACTER_GOAL}

The character is currently facing AN URGENT SITUATION THAT REQUIRES A DECISION. The player can react to this situation using DIFFERENT APPROACHES.
The encounter of type {ENCOUNTER_TYPE} takes place at the {LOCATION_SPOT} of the {LOCATION_NAME} with the objective to {CHARACTER_GOAL}.

Current state: Momentum {MOMENTUM}, Pressure {PRESSURE}
Approach values: {APPROACH_VALUES}
Active narrative tags: {LIST_TAGS}
Key NPCs present: {NPC_LIST}

{ACTIVE_TAGS}
{LOCATION_PREFERENCES}

Transform the following 4 mechanical choices into narrative descriptions.

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
  * 
Choice names MUST sound like genuine thoughts a real person would have in the specific situation:

1. USE SIMPLE, EVERYDAY THOUGHTS:
   - Use how real people actually think under stress - incomplete sentences, simple words
   - NO literary or formal phrasing - write exactly how you'd think in danger
   - GOOD: "That leg looks weak..." 
   - BAD: "I shall analyze his physical vulnerability"
   
2. UNIQUE THOUGHT PATTERNS FOR EACH APPROACH:

   FOR ANALYTICAL CHOICES:
   - NEVER use vague phrases like "Something's not right..." or "Something doesn't add up..."
   - Instead, use specific observations: "His left hand keeps twitching while he talks..."
   - Use varied analytical patterns:
     * DIRECT OBSERVATIONS: "He keeps touching that pocket..."
     * CAUSE-EFFECT THOUGHTS: "If those guards are nervous, then..."
     * COMPARISON THINKING: "The other merchants don't act this way..."
     * QUESTIONS: "Why would he hide those goods now?"
     * PATTERN RECOGNITION: "Every time she mentions the church, she looks down..."

   FOR PHYSICAL ACTION CHOICES:
   - NEVER default to the same action verb structure
   - Avoid repetitive "I'll try to..." or "I need to..." formulations
   - Use varied physical intention patterns:
     * DIRECT ACTION INTENTIONS: "Going to slip around that corner..."
     * CONDITIONAL MOVEMENTS: "If I move quickly enough..."
     * TOOL/OBJECT USAGE: "That loose brick could work as..."
     * SPECIFIC BODY MOVEMENTS: "Just need to slide my arm through..."
     * TIMING-BASED ACTIONS: "Wait for his head to turn, then..."

   FOR SOCIAL/DIALOGUE CHOICES:
   - NEVER default to the same conversation opening 
   - Avoid generic greetings or questions
   - Use varied speech patterns:
     * DIRECT REQUESTS: "Let me see what's under the counter."
     * OFFERS: "I can help you with that heavy load."
     * PERSONAL STATEMENTS: "Lost my way trying to find the cathedral."
     * EMOTIONAL APPEALS: "Please, I haven't eaten in two days."
     * OBSERVATIONS AS CONVERSATION: "Your bread smells better than any in town."

## CRITICAL GUIDELINES:

1. USE NATURAL, EVERYDAY SPEECH that an ordinary person would actually say

2. AVOID PRETENTIOUS LANGUAGE OR CONCEPTS

3. MATCH THE CHARACTER'S SOCIAL STATUS AND EDUCATION:
   - Common travelers use simple, direct language
   - No fancy words, complex sentences, or abstract concepts
   - Speak as ordinary people speak in daily life

4. KEEP DIALOGUE REALISTIC AND GROUNDED:
   - Write how real people actually talk in everyday situations
   - Avoid dialogue that sounds "written" or literary
   - Use natural, conversational rhythm

5. ENSURE CHOICE NAME DIVERSITY:
   - ANALYSIS CHOICES must use specific observations, not vague feelings
   - PHYSICAL CHOICES must show diverse action intentions, not repetitive patterns
   - SOCIAL CHOICES must use varied speech formats, not identical conversation openers
   - No two choices should begin with the same words or sentence structure

6. IF A CHOICE WILL END THE ENCOUNTER:
   - Indicate the finality without explicitly mentioning the encounter ending
   - Adapt tone to match the projected outcome (failure, partial, standard, exceptional)
   - For Failure: "I'll take whatever scraps you'll give me..."
   - For Partial: "This deal isn't great, but it'll do for now."
   - For Standard: "This bread will solve my hunger problem nicely."
   - For Exceptional: "I couldn't have hoped for a better outcome."

REMEMBER:
- Write in FIRST PERSON, PRESENT TENSE
- Each choice MUST respond to the IMMEDIATE PROBLEM, not general exploration
- Reflect the urgency of the situation in your descriptions
- Include appropriate risk language based on pressure/health effects:
  * Momentum choices: Progression language
  * Pressure +1: Low risk language
  * Pressure +2: Medium risk language
  * Pressure >= +3: High risk language
  * Health/Concentration/Confidence -1: Minor injury/confusion/shame language
  * Health/Concentration/Confidence -3: Major injury/confusion/shame language
  * Health/Concentration/Confidence <= -5: Life-threatening injury / mental breakdown language
- Each choice should feel distinct and appropriate to both the approach/focus combination

## SUMMARY

Before submitting your response, verify that all four choices meet these requirements:

1. Each choice feels like a COMPLETELY DIFFERENT tactic, not variations of the same approach
2. Each choice has its OWN UNIQUE language and vocabulary - no repeated phrasing
3. Each choice focuses on DIFFERENT elements of the situation
4. Choice names sound like AUTHENTIC thoughts, not literary descriptions
5. Analysis choices NEVER use "something seems wrong" or "something doesn't add up" patterns
6. Physical choices NEVER all begin with "I'll try to..." or similar repetitive structures
7. Social choices NEVER all use the same conversation opening pattern
8. A reader could easily tell these choices apart and understand how they differ tactically

If any choices fail these tests, rewrite them to increase differentiation.

Format the output as a JSON object with four choice objects containing 'index', 'name', 'description', 'approach', and 'focus'.

{CHOICE_STYLE_GUIDANCE}