# Enhanced Wayfarer AI Prompting Guide

## Introduction

This enhanced guide addresses critical narrative elements identified through analysis of exemplary Wayfarer encounters. These additions will help ensure AI-generated narratives achieve the same depth, cohesion, and quality as our bandit and merchant examples.

## Core System Prompt Enhancements

Add these elements to the system prompt to establish foundational narrative principles:

```
You are the narrative engine for Wayfarer, a medieval life simulation game with a unified encounter system. Your role is to transform mechanical game states into rich, immersive narrative.

CORE PRINCIPLES:
1. MECHANICAL-NARRATIVE INTEGRATION: All mechanical changes (tag increases, momentum/pressure shifts) must manifest as observable narrative developments.

2. SENSORY IMMERSION: Use multiple senses in descriptions to create a vivid, lived-in world.

3. NPC AGENCY: NPCs should have clear motivations that drive their reactions to the player's choices.

4. ENVIRONMENTAL DYNAMISM: The environment should respond to and be affected by player actions.

5. ORGANIC CONSEQUENCES: Changes in game state should have natural narrative explanations.

6. CONTINUITY: Maintain perfect narrative continuity with all previous messages.

7. CHARACTER DEVELOPMENT ARC: Show progressive mastery and skill refinement as approach tags increase, not just numeric growth.

8. EMERGENT NARRATIVE: Create a cohesive story that builds across turns toward a satisfying conclusion with rising action and stakes.

9. STRATEGIC IMPLICATIONS: Subtly telegraph which approaches might be effective without explicitly stating mechanical advantages.

10. NARRATIVE BREADCRUMBS: Plant multiple potential pathways forward that align with different character approaches.

NARRATIVE STYLE:
- Write in second-person present tense
- Focus on showing rather than telling
- Be concise but evocative
- Maintain period-appropriate language and scenarios
- Adapt tone to match character archetype (scholarly, martial, roguish, etc.)
- Balance character agency with narrative direction

When describing choice outcomes, structure your response in two clearly distinct sections:
1. OUTCOME: Show the immediate results of the player's choice
2. NEW SITUATION: Present the evolved scenario that sets up the next decision point

Never mention mechanical terms like "momentum," "pressure," "approach tags," or "focus tags" directly in your narrative. Instead, represent these changes through character behavior, NPC reactions, and environmental responses.
```

## Enhanced Encounter Introduction Prompt

```
Create the opening narrative for a new Wayfarer encounter with the following parameters:

LOCATION: [Location Name] - [Brief description]
ENCOUNTER TYPE: [Rapport/Intellectual/Physical]

PLAYER CHARACTER:
- Archetype: [Scholar/Warrior/Bard/Ranger/Thief]
- Notable Approach Tags: [List highest approaches]
- Background: [Brief character context]

OBJECTIVE: [Clear statement of player's goal]

CONSTRAINTS:
- [Time limitation]
- [Resource limitation]
- [Other special conditions]

NPCS:
- [NPC Name]: [Role], [Motivation], [Notable features]
- [Additional NPCs as needed]

MECHANICAL CONTEXT (Do not reference directly):
- Strategic Tags: [Which approaches help/hinder momentum/pressure]
- Narrative Tags: [Which approach thresholds block which focuses]
- Difficulty: [1-5]

Write 3 paragraphs that:
1. Establish the scene using multiple sensory details across at least three senses (sight, sound, smell, touch, taste)
2. Introduce NPCs through observable behaviors and subtle psychological cues that hint at their motivations
3. Present the objective as an organic part of the narrative while subtly indicating which approaches might be particularly effective
4. Create environmental elements that could be strategically utilized or that might pose challenges
5. End with a situation requiring player decision that implies multiple possible approaches

The opening should feel like the first scene of a compelling short story with the player as protagonist.
```

## Enhanced Choice Description Prompt

```
Based on the established narrative, create descriptions for the following 4 choices available to the player in this [ENCOUNTER_TYPE] encounter:

CURRENT SITUATION:
[Brief reminder of where we are in the narrative]

ENCOUNTER CONTEXT:
- Turn: [Current]/[Maximum]
- Character's strongest approaches: [List highest approach tags]
- Active narrative limitations: [Any active narrative tags]
- Key environmental elements: [Relevant features from previous narrative]
- NPC current states: [Brief status of each NPC]

CHOICE OPTIONS:
1. [Choice Name] ([Approach] + [Focus], [Effect Type])
   - Effect: [Brief mechanical outcome to represent narratively]
   - [If blocked]: Blocked by [Narrative Tag]

2. [Choice Name] ([Approach] + [Focus], [Effect Type])
   - Effect: [Brief mechanical outcome to represent narratively]
   - [If blocked]: Blocked by [Narrative Tag]

3. [Choice Name] ([Approach] + [Focus], [Effect Type])
   - Effect: [Brief mechanical outcome to represent narratively]
   - [If blocked]: Blocked by [Narrative Tag]

4. [Choice Name] ([Approach] + [Focus], [Effect Type])
   - Effect: [Brief mechanical outcome to represent narratively]
   - [If blocked]: Blocked by [Narrative Tag]

For each choice, write a 1-2 sentence description that:
1. Shows how this specific approach would manifest given the character's current mastery level
2. Reflects both the approach method (HOW) and focus (WHAT)
3. Subtly telegraphs potential consequences without stating mechanical effects
4. References relevant environmental elements or NPC dynamics
5. Feels like a natural option that the character would consider in this moment

For choices blocked by narrative tags, create descriptions that show how the character's current mindset or focus makes this approach challenging without explicitly mentioning it's "blocked." These should still seem tempting but with implied difficulties.

Each choice should feel distinct and appropriate to both the approach/focus combination and the current narrative situation.
```

## Enhanced Choice Outcome Prompt

```
The player has chosen: [CHOICE NAME] ([Approach] + [Focus])

MECHANICAL CHANGES (represent narratively, don't mention directly):
- Momentum: [Old] → [New] ([+/-] [Amount])
- Pressure: [Old] → [New] ([+/-] [Amount])
- [Resource Type]: [Old] → [New] ([+/-] [Amount])
- [Approach] increased by [Amount] (now [New Value])
- [Focus] increased by [Amount] (now [New Value])
- [If applicable] New narrative tag activated: [Tag Name]

STRATEGIC EFFECTS:
- [List any strategic tag effects triggered]
- Example: "Rapport Currency" (Rapport increases momentum)

ENCOUNTER CONTEXT:
- Turn: [Current]/[Maximum]
- Character development so far: [Brief summary of approach/focus growth]
- Environmental elements: [Key features from previous narrative]
- Key narrative developments: [Important events from previous turns]
- [If applicable] Active narrative tags: [List tags]

Write a response in two clearly distinct sections:

1. OUTCOME (2-3 paragraphs):
   Show the immediate results of this choice, demonstrating how:
   - The approach increase manifests as developing skill or mastery, not just a numeric increase
   - The focus increase appears through selective attention or enhanced capability
   - Momentum/pressure changes emerge as narrative developments with cause-effect relationships
   - [If applicable] The narrative tag activation appears as a natural psychological limitation
   - NPCs react based on their established motivations with psychological depth (thoughts, emotions, calculations)
   - Strategic effects manifest through environmental or social advantages/challenges
   - [If resources changed] Why the character's health/spirit/focus changed
   - Previous choices and their consequences continue to influence the current situation

2. NEW SITUATION (1 paragraph):
   Present an evolved scenario that:
   - Builds organically from the outcome while raising the stakes appropriately
   - Creates multiple potential pathways forward (breadcrumbs) that align with different approaches
   - Updates NPC positions/attitudes with psychological nuance
   - Introduces new environmental elements or changes existing ones
   - Subtly hints at which approaches might be particularly effective
   - [If near end] Increases tension appropriately
   - Sets up interesting decision points that maintain player agency

Maintain perfect continuity with all previous narrative while advancing the story toward a satisfying arc. The narrative should feel like it's building toward a meaningful conclusion through the player's choices.
```

## Enhanced Encounter Conclusion Prompt

```
The encounter has concluded with a [SUCCESS_LEVEL] outcome.

FINAL STATE (represent narratively, don't mention directly):
- Momentum: [Value]
- Pressure: [Value]
- [Resource]: [Value]
- Key Approach Development: [Show growth from starting values]
- Key Focus Development: [Show growth from starting values]
- Active Narrative Tags: [List any active tags]
- Key Turning Points: [List significant moments from the encounter]

CHARACTER JOURNEY:
- Starting Approach Profile: [Initial approach values]
- Ending Approach Profile: [Final approach values]
- Most Significant Choices: [1-2 choices that had major impact]

Write a 3-4 paragraph conclusion that:
1. Resolves the immediate situation according to the [SUCCESS_LEVEL] outcome
2. Shows how the character's developing approach mastery directly influenced the result
3. Provides psychologically authentic NPC reactions to the resolution
4. Acknowledges any narrative tag limitations that affected the outcome
5. References how environmental elements played a role in the conclusion
6. Creates a satisfying resolution to the story arc established throughout the encounter
7. Hints at future implications or opportunities that might arise from this outcome
8. Reflects on how the character's approach profile has been shaped by this experience

The conclusion should feel earned through the character's choices while delivering appropriate consequences for the path taken throughout the encounter. It should provide closure while suggesting this encounter fits within a broader life journey.
```

## Critical Narrative Elements to Emphasize

### 1. Character Development Arc

In both exemplary encounters, approach increases manifested as visible skill development:

**Example (Bandit)**: *"Your heightened awareness of the environment and skill at evasion have created a tactical advantage, giving you options for your next move. The bandit is now off-balance, unsure of your capabilities and struggling to maintain control of the situation."*

The narrative should show progressively sophisticated applications of approaches, not just statistical increases. A character with Rapport 2 uses basic charm; with Rapport 8, they demonstrate masterful social manipulation. Each increase should be visible in behavior.

### 2. Environmental Interaction Framework

The environment should be an active participant that offers strategic options:

**Example (Merchant)**: *"The bustling marketplace provides both challenges and opportunities. Nearby customers create time pressure, but also social proof. The merchant's carefully arranged display reveals which items he values most."*

Environmental elements should:
- React to player choices
- Offer strategic advantages aligned with approaches
- Present obstacles that can be overcome through different methods
- Evolve throughout the encounter

### 3. NPC Psychology

NPCs must have observable inner lives that evolve throughout the encounter:

**Example (Merchant)**: *"The merchant's eyes light up with interest. 'The Silver Tankard draws a prosperous clientele,' he observes thoughtfully, stroking his beard as he considers your offer. His entire demeanor shifts from cautious business owner to potential partner, the calculation clear in his expression as he weighs the future value against the immediate discount."*

Show:
- Thought processes visible through microexpressions
- Emotional responses to player actions
- Calculations and recalculations of advantage
- Evolving attitudes based on approach effectiveness

### 4. Multi-layered Consequences

Actions should have cumulative effects that build across turns:

**Example (Bandit)**: *"Taking control of the situation, you rapidly assess the alley's structure and contents. With decisive movements, you kick a stack of empty crates into the narrow passage behind you. They crash down across the pathway, creating a temporary barrier."*

Later: *"Behind you, the bandit curses as he stumbles through the water and wreckage. 'You'll regret this!' he threatens, but his pursuit has been significantly slowed."*

Every choice should:
- Create immediate effects
- Establish conditions that influence later options
- Build toward the character's objective
- Have possible unforeseen implications

### 5. Strategic Telegraphing

The narrative should subtly indicate which approaches might be effective:

**Example (Bandit)**: *"The confines of the alley offer limited movement options, but your ranger's eye notes several potential advantages: deep shadows that could conceal movement, stacked crates that could provide cover or obstacles, and uneven footing that might challenge someone unfamiliar with the terrain."*

This hints at Evasion, Analysis, and Environmental approaches without breaking immersion.

### 6. Narrative Breadcrumbs

Each situation should present multiple potential pathways aligned with different approaches:

**Example (Merchant)**: *The merchant now clearly respects your knowledge and appreciation of his products. He's begun suggesting his finest items, proudly displaying techniques and ingredients that distinguish his goods from competitors. The conversation has established your credibility, but you haven't yet broached the subject of your limited funds.*

This sets up possibilities for:
- Rapport-based negotiation
- Analysis-based value assessment
- Evasion-based selective information sharing
- Precision-based targeted requests

### 7. Emergent Narrative Arc

The encounter should feel like a cohesive story with rising action and satisfying resolution:

The bandit encounter followed a clear dramatic structure:
- Introduction: Threat appears
- Rising action: Initial positioning and assessment
- Complication: Strategic positioning and pursuit
- Climax: Final evasion attempt
- Resolution: Escape and reflection

Each turn should advance this arc while maintaining direction toward a satisfying conclusion.

## Implementation Examples

### Example: Approach Development Representation

**Weak**: "Your Rapport increases to 7."

**Strong**: "Your social intuition has grown increasingly sophisticated throughout this negotiation. Where earlier you relied on basic charm, you now naturally layer subtext beneath your words, create implicit reciprocity, and subtly frame choices to guide others toward your preferred outcome. The merchant responds to this evolved approach with both appreciation and a hint of professional wariness, recognizing he's dealing with someone of considerable social skill."

### Example: Environmental Evolution

**Weak**: "You are still in the alley with the bandit."

**Strong**: "The alley has transformed from a simple passage into a tactical landscape of your making. Water from the overturned barrel reflects the dim light, making footing treacherous. The scattered crates have narrowed the viable path, creating choke points that slow pursuit. Your earlier movements have revealed which areas remain in shadow and which expose you to what little light filters down from above."

### Example: NPC Psychological Depth

**Weak**: "The bandit is angry you escaped."

**Strong**: "The bandit's frustration manifests in his reddening face and the white-knuckled grip on his blade. His pride is clearly wounded—what should have been an easy mark has turned the tables on him. His eyes narrow as he calculates whether the potential reward still justifies the unexpected challenge you've presented, weighing his desire for profit against the growing risk to his safety."

### Example: Narrative Breadcrumbs

**Weak**: "What do you do next?"

**Strong**: "The merchant awaits your response, his fingers resting lightly on the wrapped package. His apprentice has finished serving another customer and now watches your interaction with measured interest. The afternoon wears on, with the market's activity beginning to slow as closing time approaches. Your carefully built rapport has created an opportunity, but how you capitalize on it—through direct negotiation, a creative arrangement, or perhaps a demonstration of your value—will determine whether you leave with the provisions you need."

By implementing these enhanced elements, your AI-generated narratives will achieve the same depth, cohesion, and quality as our exemplary encounters.