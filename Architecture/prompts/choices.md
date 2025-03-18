# WAYFARER CHOICE DESCRIPTIONS

Transform the following 4 mechanical choices into narrative descriptions for a {ENCOUNTER_TYPE} encounter in {LOCATION}. 

{ENCOUNTER_HISTORY}

The character is currently facing this URGENT SITUATION:
{CURRENT_SITUATION}

Objective: {CHARACTER_GOAL}

Current state: Momentum {MOMENTUM}, Pressure {PRESSURE}
Approach values: {APPROACH_VALUES}
Active narrative tags: {LIST_TAGS}
Key NPCs present: {NPC_LIST}

{ACTIVE_TAGS}

For each choice, write a 1-2 sentence description that:
1. Shows how this specific approach would manifest given the character's current mastery level
2. DIRECTLY ADDRESSES the immediate urgent problem in the final paragraph of the narrative
3. Reflects both the approach method (HOW) and focus (WHAT)
4. Subtly telegraphs potential consequences without stating mechanical effects
5. References relevant environmental elements or NPC dynamics
6. Is appropriate for the encounter type (Social/Intellectual/Physical)
7. Shows how the character would execute this choice under the current time pressure

Each choice MUST provide a different approach to solving the CURRENT URGENT PROBLEM.

CHOICES:
{CHOICES_INFO}

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
- Adapt choice descriptions to reflect the character's growing skill with approaches
- Each choice should feel distinct and appropriate to both the approach/focus combination

Format the output as a JSON object with four choice objects containing 'index', 'name', 'description', 'approach', and 'focus'.
{CHOICE_STYLE_GUIDANCE}