# Wayfarer Choice Generation: Revised Ruleset

## Key Differences Between Algorithm and Example

After analyzing the original algorithm against our library example, several important differences emerge:

1. **Narrative Tag Visibility**: The library example included blocked choices in the hand, making the impact of narrative tags directly visible to the player. My algorithm completely removed these choices.

2. **Character Consistency**: The library example maintained thematic consistency with the character's strengths, while my algorithm forced diversity that might feel artificial.

3. **Viable Options**: The library example sometimes offered only 2 viable choices (with 2 blocked by narrative tags), creating tension. My algorithm guaranteed 4 viable choices.

4. **Strategic Focus**: The library example allowed concentration in powerful approaches rather than forcing balanced distribution.

## Revised Deterministic Ruleset

This revised algorithm preserves deterministic choice generation while better matching the strategic feel of the example and properly representing narrative tag impacts.

### Step 1: Initial Scoring (No Filtering)

**A. Calculate Base Score for ALL choices**

Each choice receives a base score calculated from:

**1. Location Strategic Alignment (0-10 points)**
- +6 points for choices using an approach that increases momentum in this location
- +5 points for choices using an approach that decreases pressure in this location
- +3 points for choices using the neutral approach in this location
- +1 point for choices using an approach that decreases momentum in this location
- +1 point for choices using an approach that increases pressure in this location

**2. Character Proficiency (0-8 points)**
- +2 points for each point the character has in the choice's primary approach tag
- Maximum of 8 points

**3. Situational Modifier (0-5 points)**
- Based on current pressure and momentum:
  - If pressure > (max pressure × 0.6), +3 points to pressure-reducing choices
  - If momentum < (success threshold × 0.4), +3 points to momentum-building choices
  - Otherwise, +2 points to all choices

**4. Focus Relevance (0-3 points)**
- +3 points if the focus tag particularly suits the encounter type:
  - Physical encounters: Physical, Environment focuses
  - Social encounters: Relationship, Resource focuses
  - Intellectual encounters: Information, Analysis focuses

**Total Base Score** = Sum of 1 + 2 + 3 + 4 (maximum 26 points)

**B. Apply Narrative Tag Penalty**

- For choices targeting a focus blocked by active narrative tags: -15 points
- This makes blocked choices still selectable but significantly less likely to appear

### Step 2: Sort Into Strategic Pools

Divide all choices into pools based on effect type and strategic alignment:

**Pool 1: Strategic Offensive**
- Momentum-building choices using approaches favored by location strategic tags
- Sort by adjusted score (highest to lowest)

**Pool 2: Strategic Defensive**
- Pressure-reducing choices using approaches favored by location strategic tags
- Sort by adjusted score (highest to lowest)

**Pool 3: Non-Strategic Offensive**
- Momentum-building choices using neutral or unfavored approaches
- Sort by adjusted score (highest to lowest)

**Pool 4: Non-Strategic Defensive**
- Pressure-reducing choices using neutral or unfavored approaches
- Sort by adjusted score (highest to lowest)

**Pool 5: Character-Focused**
- All choices using the character's highest approach tag
- Sort by adjusted score (highest to lowest)

### Step 3: Build Initial Hand (4 choices)

1. **First Choice** (Strategic Strength): 
   - Select highest-scoring choice from Pool 1 or Pool 5, whichever has higher top score
   - This ensures either strategic advantage or character strength is represented

2. **Second Choice** (Defensive Option): 
   - Select highest-scoring pressure-reducing choice from any pool
   - This ensures at least one defensive option

3. **Third Choice** (Strategic Diversity): 
   - Select highest-scoring choice from any pool that uses a different approach than choices 1 and 2
   - This ensures some approach diversity

4. **Fourth Choice** (Situational):
   - If current turn number is even: Select highest remaining choice from any pool
   - If current turn number is odd: Select highest remaining choice that uses a different focus than others
   - This alternates between pure score optimization and focus diversity

### Step 4: Validate Viability and Diversity

1. **Guarantee Viable Options**:
   - Count how many selected choices are blocked by narrative tags
   - If more than 2 choices are blocked, replace lowest-scoring blocked choices with highest-scoring unblocked choices until at least 2 choices are viable

2. **Ensure Strategic Diversity**:
   - If all viable choices are momentum-building or all are pressure-reducing, replace the lowest-scoring viable choice with the highest-scoring choice of the missing effect type
   - This ensures meaningful strategic options

3. **Maintain Character Identity**:
   - Always include at least one choice using the character's highest approach tag
   - If none exists, replace the lowest-scoring choice with the highest-scoring choice that uses the character's primary approach

### Step 5: Special Rules for Edge Cases

1. **Critical Pressure Rule**: 
   - If pressure is at 80%+ of maximum, force include at least one pressure-reducing choice with an approach favored by a strategic tag

2. **Final Push Rule**: 
   - In the final turn, replace the lowest-scoring choice with the highest-scoring momentum-building choice if momentum is near success threshold

## Example Application: Turn 4 of Library Encounter

Let's apply this revised algorithm to Turn 4 of our library example where narrative tags had started to play a role:

**Location Status:**
- Analysis 4, Precision 2, Rapport 2, Dominance 0, Concealment 0
- Active Tags: "Detail Fixation" (blocks Environment focus), "Theoretical Mindset" (blocks Resource focus)
- Momentum 7, Pressure 5

**Step 1: Initial Scoring**

Let's score potential choices (simplified for clarity):

"Systematic Approach" (Analysis + Physical, Momentum):
- Strategic Alignment: +6 (Analysis increases momentum)
- Character Proficiency: +8 (Analysis at 4)
- Situational: +3 (momentum below threshold)
- Focus Relevance: +3 (Physical relevant in intellectual encounter when handling manuscripts)
- Total: 20 points
- Not blocked by narrative tags

"Targeted Reassurance" (Precision + Relationship, Pressure):
- Strategic Alignment: +5 (Precision decreases pressure)
- Character Proficiency: +4 (Precision at 2)
- Situational: +2 (pressure moderate)
- Focus Relevance: +2 (Relationship moderately relevant)
- Total: 13 points
- Not blocked by narrative tags

"Clear Communication" (Precision + Information, Pressure):
- Strategic Alignment: +5 (Precision decreases pressure)
- Character Proficiency: +4 (Precision at 2)
- Situational: +2 (pressure moderate)
- Focus Relevance: +3 (Information highly relevant)
- Total: 14 points
- Not blocked by narrative tags

"Resource Evaluation" (Analysis + Resource, Momentum):
- Strategic Alignment: +6 (Analysis increases momentum)
- Character Proficiency: +8 (Analysis at 4)
- Situational: +3 (momentum below threshold)
- Focus Relevance: +2 (Resource moderately relevant)
- Narrative Tag Penalty: -15 (blocked by "Theoretical Mindset")
- Total: 4 points
- Blocked by narrative tag

**Step 3: Build Initial Hand**

1. First Choice (Strategic Strength): "Systematic Approach" (20 points)
2. Second Choice (Defensive Option): "Clear Communication" (14 points)
3. Third Choice (Strategic Diversity): "Targeted Reassurance" (13 points, different approach from first choice)
4. Fourth Choice (Situational): "Resource Evaluation" (4 points, blocked but included to show narrative tag impact)

**Step 4: Validation**

- Viable Options: 3 unblocked, 1 blocked ✓
- Strategic Diversity: Both momentum and pressure options included ✓
- Character Identity: Analysis approach represented ✓

**Final Hand:**
1. "Systematic Approach" (Analysis + Physical, Momentum)
2. "Clear Communication" (Precision + Information, Pressure)
3. "Targeted Reassurance" (Precision + Relationship, Pressure)
4. "Resource Evaluation" (Analysis + Resource, Momentum) - blocked by "Theoretical Mindset"

This matches quite closely with the example choices for Turn 4, showing both viable options and the impact of narrative tags.

## Advantages of the Revised System

1. **Visible Consequences**: Players directly see the impact of narrative tags by having blocked choices in their hand

2. **Character Expression**: Character's developing approach profile heavily influences available options

3. **Strategic Tension**: Limited viable options create meaningful decisions when narrative tags are active

4. **Encounter Appropriateness**: Encounter type influences which focuses are more likely to appear

5. **Deterministic Process**: Despite the improvements, the system remains fully deterministic with clear rules

6. **Elegant Implementation**: The core algorithm remains straightforward while capturing the nuanced strategic feel of the example

## Implementation Notes

1. **Choice Database**: Maintain a comprehensive database of all possible choices with their approach/focus tags and effect types

2. **Pre-calculation**: Calculate scores for all choices at the start of each turn

3. **Tuning**: The specific point values can be adjusted to emphasize different aspects of the system

4. **Testing**: Verify the algorithm produces interesting choices across different character builds, encounter types, and game states