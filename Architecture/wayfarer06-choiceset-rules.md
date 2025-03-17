# Wayfarer Choice Generation: Final Ruleset

## Choice Generation Algorithm

This ruleset defines the exact, deterministic process for generating the 4 choices presented to players each turn in Wayfarer encounters.

### Step 1: Calculate Choice Scores

For **every possible choice** in the database, calculate its score using the following formula:

```
Choice Score = Strategic Alignment Score + Character Proficiency Score + 
               Situational Score + Focus Relevance Score + Narrative Tag Modifier
```

Where:

**1. Strategic Alignment Score**
- If choice's primary approach increases momentum in location: Score = 6
- If choice's primary approach decreases pressure in location: Score = 5
- If choice's primary approach is neutral in location: Score = 3
- If choice's primary approach decreases momentum in location: Score = 1
- If choice's primary approach increases pressure in location: Score = 1

**2. Character Proficiency Score**
- Score = Character's value in choice's primary approach × 2
- Maximum value: 8

**3. Situational Score**
- If (current pressure ≥ max pressure × 0.6) AND choice reduces pressure: Score = 3
- If (current momentum ≤ success threshold × 0.4) AND choice builds momentum: Score = 3
- Otherwise: Score = 2

**4. Focus Relevance Score**
- For Physical encounters:
  - If choice has Physical focus: Score = 3
  - If choice has Environment focus: Score = 2
  - Otherwise: Score = 1
- For Social encounters:
  - If choice has Relationship focus: Score = 3
  - If choice has Information focus: Score = 2
  - Otherwise: Score = 1
- For Intellectual encounters:
  - If choice has Information focus: Score = 3
  - If choice has Relationship focus: Score = 2
  - Otherwise: Score = 1

**5. Narrative Tag Modifier**
- If choice's focus is blocked by any active narrative tag: Score = -15
- Otherwise: Score = 0

### Step 2: Categorize All Choices

Divide all choices into categorized lists:

**Pool A: By Effect Type and Strategic Alignment**
- A1: Momentum-building choices using approaches that increase momentum
- A2: Pressure-reducing choices using approaches that decrease pressure 
- A3: Momentum-building choices using neutral approaches
- A4: Pressure-reducing choices using neutral approaches
- A5: Momentum-building choices using approaches that decrease momentum or increase pressure
- A6: Pressure-reducing choices using approaches that decrease momentum or increase pressure

**Pool B: By Approach**
- B1: Choices using character's highest approach tag
- B2: Choices using character's second highest approach tag
- B3: Choices using character's third highest approach tag
- B4: Choices using character's fourth highest approach tag
- B5: Choices using character's fifth highest approach tag

**Pool C: By Narrative Tag Status**
- C1: Choices not blocked by narrative tags
- C2: Choices blocked by narrative tags

Within each list, sort choices by their total score in descending order.

### Step 3: Select Initial Choices

1. **First Choice: Character Strength**
   - Select the highest-scoring choice from list B1
   - If B1 is empty, select the highest-scoring choice from B2

2. **Second Choice: Strategic Advantage**
   - If First Choice builds momentum:
     - Select the highest-scoring choice from list A2
   - If First Choice reduces pressure:
     - Select the highest-scoring choice from list A1
   - If list is empty, select from next available list in sequence (A3→A4→A5→A6)

3. **Third Choice: Approach Diversity**
   - Create a temporary list of all choices that use a different approach than First and Second Choice
   - Select the highest-scoring choice from this list
   - If list is empty, select the highest-scoring remaining choice from A1 through A6 in sequence

4. **Fourth Choice: Focus Diversity or Narrative Tag Impact**
   - If Turn Number is odd OR fewer than 2 choices in hand are blocked by narrative tags:
     - If any active narrative tags, select highest-scoring choice from C2
     - Otherwise, select highest-scoring choice not yet selected
   - If Turn Number is even AND 2 choices in hand are already blocked:
     - Select highest-scoring choice from C1 not yet selected

### Step 4: Validate Hand Composition

1. **Ensure Viable Choices Rule**
   - Count choices blocked by narrative tags in the current hand
   - If more than 2 choices are blocked:
     - Remove the lowest-scoring blocked choice
     - Add the highest-scoring unblocked choice not already in hand

2. **Guarantee Strategic Options Rule**
   - If all unblocked choices build momentum:
     - Remove the lowest-scoring momentum choice
     - Add the highest-scoring pressure-reducing choice not already in hand
   - If all unblocked choices reduce pressure:
     - Remove the lowest-scoring pressure choice
     - Add the highest-scoring momentum-building choice not already in hand

3. **Character Identity Rule**
   - If no choice in hand uses the character's highest approach tag:
     - Remove the lowest-scoring choice
     - Add the highest-scoring choice using character's highest approach tag

### Step 5: Handle Edge Cases

1. **Critical Pressure Rule**
   - If pressure ≥ (max pressure × 0.8):
     - If no choice in hand from A2 (pressure-reducing using favorable approach):
       - Remove the lowest-scoring choice
       - Add the highest-scoring choice from A2

2. **Success Within Reach Rule**
   - If (momentum + 6) ≥ success threshold AND turn_number ≥ (max_turns - 2):
     - If fewer than 2 momentum-building choices in hand:
       - Remove the lowest-scoring pressure-reducing choice
       - Add the highest-scoring momentum-building choice not in hand

3. **Empty List Handling**
   - If any selection step encounters an empty list, proceed to the next list in sequence
   - If all lists are empty, return default choice for that approach type

### Step 6: Output Finalized Hand

Present the final 4 choices to the player, sorted by choice type:
1. Unblocked momentum-building choices first
2. Unblocked pressure-reducing choices second
3. Blocked choices last (if any)

Within each group, sort by total score (highest to lowest).

## Example: Applying the Algorithm to Turn 4 of Library Encounter

### Current State:
- Turn: 4 (of 6)
- Character: Scholar with Analysis 4, Precision 2, Rapport 2, others 0
- Active Tags: "Detail Fixation" (blocks Environment focus), "Theoretical Mindset" (blocks Resource focus)
- Momentum: 7, Pressure: 5, Success Threshold: 16

### Step 1: Calculate Scores

Selected choices with calculated scores:

1. "Systematic Approach" (Analysis + Physical, Momentum)
   - Strategic Alignment: 6 (Analysis increases momentum)
   - Character Proficiency: 8 (Analysis = 4)
   - Situational: 3 (momentum below threshold)
   - Focus Relevance: 2 (Physical in intellectual encounter)
   - Narrative Tag: 0 (not blocked)
   - Total: 19 points

2. "Targeted Reassurance" (Precision + Relationship, Pressure)
   - Strategic Alignment: 5 (Precision decreases pressure)
   - Character Proficiency: 4 (Precision = 2)
   - Situational: 2 (pressure moderate)
   - Focus Relevance: 2 (Relationship in intellectual encounter)
   - Narrative Tag: 0 (not blocked)
   - Total: 13 points

3. "Clear Communication" (Precision + Information, Pressure)
   - Strategic Alignment: 5 (Precision decreases pressure)
   - Character Proficiency: 4 (Precision = 2)
   - Situational: 2 (pressure moderate)
   - Focus Relevance: 3 (Information in intellectual encounter)
   - Narrative Tag: 0 (not blocked)
   - Total: 14 points

4. "Resource Evaluation" (Analysis + Resource, Momentum)
   - Strategic Alignment: 6 (Analysis increases momentum)
   - Character Proficiency: 8 (Analysis = 4)
   - Situational: 3 (momentum below threshold)
   - Focus Relevance: 1 (Resource in intellectual encounter)
   - Narrative Tag: -15 (blocked by "Theoretical Mindset")
   - Total: 3 points

### Step 2: Categorize

**Pool A (Strategic)**
- A1: "Systematic Approach" (19 points)
- A2: "Clear Communication" (14 points), "Targeted Reassurance" (13 points)
- A5: "Resource Evaluation" (3 points - but would be 18 without tag penalty)

**Pool B (Approach)**
- B1: "Systematic Approach" (19 points), "Resource Evaluation" (3 points)
- B2: "Clear Communication" (14 points), "Targeted Reassurance" (13 points)

**Pool C (Narrative Status)**
- C1: "Systematic Approach" (19 points), "Clear Communication" (14 points), "Targeted Reassurance" (13 points)
- C2: "Resource Evaluation" (3 points)

### Step 3: Select Initial Choices

1. First Choice: "Systematic Approach" (highest from B1)
2. Second Choice: "Clear Communication" (highest from A2)
3. Third Choice: "Targeted Reassurance" (different approach from first choice)
4. Fourth Choice: "Resource Evaluation" (from C2 to show narrative tag impact)

### Step 4: Validate

- Viable Options: 3 unblocked, 1 blocked ✓
- Strategic Options: Both momentum and pressure choices included ✓
- Character Identity: Analysis approach represented ✓

### Final Hand:
1. "Systematic Approach" (Analysis + Physical, Momentum)
2. "Clear Communication" (Precision + Information, Pressure)
3. "Targeted Reassurance" (Precision + Relationship, Pressure)
4. "Resource Evaluation" (Analysis + Resource, Momentum) - blocked by "Theoretical Mindset"

This matches closely with our example encounter, providing both strategic options and showing the impact of narrative tags.

## Key Features of This Ruleset

1. **Fully Deterministic**: Each step has precise rules with no subjective interpretation
2. **Visible Consequences**: Players see blocked choices in their hand, making narrative tags impactful
3. **Strategic Balance**: Ensures both offensive and defensive options are available
4. **Character Expression**: Heavily weights character's approach strengths
5. **Encounter Appropriate**: Adjusts focus tag relevance based on encounter type
6. **Easily Translatable**: Clear formulas and decision trees can be directly coded