# Wayfarer Narrative AI Prompts

After analyzing the narrative elements in the library example, I've identified three distinct prompts needed to generate the narrative layer for Wayfarer encounters. Each prompt serves a specific purpose and requires different input information to create cohesive, contextually appropriate narrative.

## 1. Encounter Setup Prompt

This prompt generates the initial narrative that sets the scene at the beginning of an encounter.

### Required Input:
- Location name and type
- Encounter type (Social, Intellectual, Physical)
- Character archetype and approach stats
- Key NPCs present
- Character's goal/objective
- Environmental characteristics
- Time constraints or pressure factors
- Any existing narrative context

### Prompt:

```
Create an immersive introductory scene for a [ENCOUNTER_TYPE] encounter in a [LOCATION_NAME]. The character is a [CHARACTER_ARCHETYPE] with [APPROACH_STATS] seeking to [CHARACTER_GOAL].

The location contains [ENVIRONMENT_DETAILS] and key NPCs include [NPC_LIST]. The character faces [TIME_CONSTRAINTS] and [ADDITIONAL_CHALLENGES].

Write 2-3 paragraphs that:
1. Describe the physical environment using multiple senses
2. Establish the presence and initial attitude of key NPCs
3. Hint at the central challenge/objective
4. Create a sense of atmosphere appropriate to the encounter type
5. End with a situation that requires the character to make a choice

The tone should match a [ENCOUNTER_TYPE] encounter - [for Social: conversational and interpersonal; for Intellectual: analytical and observational; for Physical: action-oriented and environmental].
```

### Example Output (Library Encounter):

*You enter the dimly lit ancient library, dust motes dancing in shafts of light from high windows. The smell of old parchment and leather bindings fills the air. Towering bookshelves stretch in all directions, creating a labyrinth of knowledge. Near the entrance, an elderly caretaker watches you with suspicion from behind a worn desk. In the distance, you glimpse your academic rival already consulting a catalog.*

## 2. Choice Description Prompt

This prompt transforms mechanical choices into narratively appropriate descriptions for player selection.

### Required Input:
- List of 4 mechanical choices (approach, focus, effect type)
- Current encounter state (momentum, pressure, tags)
- Active narrative tags and effects
- Location details and NPCs
- Encounter type
- Current situation/challenge
- Character's current approach stats

### Prompt:

```
Transform the following 4 mechanical choices into narrative descriptions for a [ENCOUNTER_TYPE] encounter in a [LOCATION_TYPE]. The character is currently [CURRENT_SITUATION] with the objective to [CHARACTER_GOAL].

Current state: Momentum [M], Pressure [P]
Approach values: Analysis [A], Precision [P], Rapport [R], Dominance [D], Concealment [C]
Active narrative tags: [LIST_TAGS]
Key NPCs present: [NPC_LIST]

For each choice, write a 1-2 sentence description that:
1. Reflects the mechanical approach and focus (how and what)
2. Is appropriate for the encounter type (Social/Intellectual/Physical)
3. Relates to the current situation and goal
4. Shows how the character would execute this choice in this specific context
5. For blocked choices (by narrative tags), still create a description but indicate why it might be difficult

CHOICES:
1. [CHOICE_1_NAME] ([CHOICE_1_APPROACH] + [CHOICE_1_FOCUS], [EFFECT_TYPE])
2. [CHOICE_2_NAME] ([CHOICE_2_APPROACH] + [CHOICE_2_FOCUS], [EFFECT_TYPE])
3. [CHOICE_3_NAME] ([CHOICE_3_APPROACH] + [CHOICE_3_FOCUS], [EFFECT_TYPE])
4. [CHOICE_4_NAME] ([CHOICE_4_APPROACH] + [CHOICE_4_FOCUS], [EFFECT_TYPE])

Format each as a numbered list with the choice name as a heading and the description below.
```

### Example Output (Library Turn 1):

*1. **Analytical Insight** (Analysis + Information, Momentum)*
   - *You immediately begin evaluating the library's organizational system, looking for patterns in how the sections are arranged. Academic libraries often follow specific classification methods that might reveal shortcuts to the relevant texts.*

*2. **Assess Relationships** (Analysis + Relationship, Momentum)*
   - *You carefully observe the interaction between the caretaker and your rival, noting the caretaker's body language and level of assistance. Understanding their relationship could provide valuable insights into how to approach the caretaker yourself.*

*3. **Create Ambiance** (Rapport + Environment, Momentum)*
   - *You approach with a warm smile and appreciative comments about the magnificent library, attempting to establish a positive atmosphere conducive to cooperation with the caretaker.*

*4. **Calculate Risks** (Analysis + Physical, Pressure)*
   - *You take time to assess the physical condition of the nearest ancient texts, noting signs of deterioration and proper handling techniques that will prevent damage during your search.*

## 3. Turn Resolution Prompt

This prompt generates the outcome of the player's choice and introduces the new situation/challenge for the next turn.

### Required Input:
- Selected choice (approach, focus, effect type)
- Previous encounter state
- New encounter state after choice
- Tag changes (approach and focus tags)
- Newly activated narrative tags
- Strategic tag effects triggered
- Progress toward goal
- Turn number and max turns
- NPC reactions
- Location details

### Prompt:

```
Generate a narrative outcome for a character in a [ENCOUNTER_TYPE] encounter who chose [SELECTED_CHOICE] ([APPROACH] + [FOCUS], [EFFECT_TYPE]). The encounter takes place in [LOCATION] with the objective to [CHARACTER_GOAL].

Previous state: Momentum [M_OLD], Pressure [P_OLD]
New state: Momentum [M_NEW], Pressure [P_NEW]
Approach changes: [APPROACH_CHANGES]
Focus changes: [FOCUS_CHANGES]
Narrative tags activated: [NEW_NARRATIVE_TAGS]
Strategic effects triggered: [STRATEGIC_EFFECTS]
Turn: [CURRENT_TURN] of [MAX_TURNS]

Write a response in two parts:
1. OUTCOME (2-3 paragraphs):
   - Describe the immediate result of the chosen action
   - Show how approach/focus tags increase narratively
   - Illustrate strategic tag effects (momentum/pressure changes)
   - If narrative tags activated, describe their effects subtly
   - Include NPC reactions if applicable
   - Indicate progress toward the overall goal

2. NEW SITUATION (1 paragraph):
   - Present a new challenge or development for the next turn
   - Update NPC status/positions if applicable
   - Reference the character's current progress
   - Provide context for the next set of choices
   - If turn count is high, increase sense of urgency

Maintain the tone appropriate for a [ENCOUNTER_TYPE] encounter. If narrative tags are now blocking certain focuses, subtly hint at this limitation in the character's perspective.
```

### Example Output (Library Turn 1 Resolution):

*OUTCOME:*
*Your trained scholar's eye quickly discerns the library uses a modified Alexandrian classification system. The mathematical and geographical texts—most likely to contain the artifact's location—would be housed in the eastern wing, significantly narrowing your search area. As you make these connections, you become deeply absorbed in the theoretical framework of the cataloging system, its elegantly nested categories capturing your full attention.*

*However, as your mind races through the possibilities, you barely notice the physical deterioration of the surrounding shelves or the caretaker's growing interest in your methodical analysis. Your focus has narrowed to the intellectual puzzle, making the broader environment fade into the background.*

*NEW SITUATION:*
*With your understanding of the library's organization, you now need access to the eastern wing. The elderly caretaker holds a large ring of keys at his belt, suggesting the most valuable collections are locked. Your rival appears to be struggling with the general collection, not yet having deduced the correct section to search.*

## Additional Considerations

### Combining Prompts for Efficiency

Depending on the API implementation, you might want to combine the Choice Description and Turn Resolution prompts to reduce the number of API calls. This would require waiting until after the player makes a selection, then generating both the outcome and the next set of choices in one call.

### Enhancing Narrative Continuity

To enhance narrative continuity, each prompt should include a brief history of previous turns and choices. This helps maintain consistent character development and situation progression.

### Adapting to Different Encounter Types

The prompts should adjust their language and focus based on the encounter type:
- **Social Encounters**: Emphasize dialogue, relationships, social dynamics
- **Intellectual Encounters**: Focus on observations, deductions, analysis
- **Physical Encounters**: Highlight movement, environment, physical actions

### Handling Edge Cases

Additional parameters might be needed for:
- First turn vs. middle turns vs. final turn
- Critical pressure situations
- Near success/failure states
- Special location features
- Unique narrative tag combinations

## Implementation Workflow

The recommended workflow for implementing these prompts is:

1. **Encounter Start**:
   - Run Encounter Setup Prompt
   - Display initial narrative

2. **Each Turn**:
   - Run Choice Description Prompt with current game state
   - Display 4 narrative choices to player
   - Player selects a choice
   - Run Turn Resolution Prompt with selected choice and updated state
   - Display outcome and new situation
   - Repeat until encounter ends

3. **Encounter End**:
   - Run a specialized Encounter Conclusion Prompt (not detailed here)
   - Display final outcome based on success level