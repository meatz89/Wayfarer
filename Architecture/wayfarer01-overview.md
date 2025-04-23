# Wayfarer Encounter System: Complete Design Document

## Introduction

Wayfarer is a medieval life simulation game designed to create immersive, narratively rich encounters across a wide range of scenarios. Unlike traditional RPGs that use different systems for combat, social interaction, and exploration, Wayfarer employs a unified encounter system that handles all forms of interaction through the same core mechanics.

This document details the complete encounter system, explaining the motivation behind design decisions and providing concrete rules for implementation. The system aims to achieve the following goals:

1. **Mechanical Consistency**: Use the same underlying systems for all encounter types
2. **Strategic Depth**: Create meaningful choices that reward different character builds
3. **Narrative Integration**: Seamlessly blend mechanics with engaging storytelling
4. **Elegant Simplicity**: Focus on depth through interaction rather than complex rules
5. **Verisimilitude**: Create a system that feels natural and appropriate to the medieval setting

The core innovation of the Wayfarer system is its tag-based approach, where characters develop certain approaches to problems (HOW they tackle challenges) and focuses (WHAT they concentrate on). Different locations naturally favor certain approaches through strategic tags, while narrative tags create evolving constraints as player approaches intensify.

## Core System Architecture

The Wayfarer encounter system consists of three distinct but interconnected layers:

### 1. Base Layer
- Tracks fundamental state variables (approach and focus tags)
- Tags are modified by player choices
- Tag values determine interaction with the encounter layer

### 2. Encounter Layer
- Provides the actual gameplay mechanics through two types of tags:
  - **Narrative Tags**: Control which focus choices can appear based on approach tag thresholds
  - **Strategic Tags**: Define momentum and pressure effects based on approach tag values

### 3. Presentation Layer
- Transforms mechanical choices into appropriate narrative text
- Changes presentation based on encounter type while mechanics remain consistent

This layered architecture creates a clean separation of concerns while allowing rich interactions between systems.

## Base Tag System

The foundation of the Wayfarer system is its tag-based approach to **temporary encounter state tracking**. Unlike traditional character attribute systems, these tags exist only during encounters and reset afterward.

### Approach Tags (HOW) - Temporary Encounter State

Approach tags represent how the character is currently tackling the specific challenge within this encounter only. Each tag ranges from 0 to 10 within a single encounter, with higher values indicating greater emphasis on that approach in the current situation.

| Approach Tag  | Description                               |
|---------------|-------------------------------------------|
| Dominance     | Force, authority, intimidation            |
| Rapport       | Social connections, charm, persuasion     |
| Analysis      | Intelligence, observation, problem-solving|
| Precision     | Careful execution, finesse, accuracy      |
| Evasion   | Stealth, hiding, subterfuge               |

### Focus Tags (WHAT) - Temporary Encounter State

Focus tags represent what the character is concentrating on during the current encounter only. Each tag ranges from 0 to 10 within a single encounter.

| Focus Tag     | Description                               |
|---------------|-------------------------------------------|
| Relationship  | Connections with others, social dynamics  |
| Information   | Knowledge, facts, understanding           |
| Physical      | Bodies, movement, physical objects        |
| Environment   | Surroundings, spaces, terrain             |
| Resource      | Items, money, supplies, valuables         |

### Tag Interaction During Encounters

Every choice in an encounter affects the encounter's tag values in three ways:
1. Every choice increases one focus tag by 1
2. Every choice increases one primary approach tag by 1-2
3. Some choices may modify a secondary approach tag by ±1-2

These tag modifications are temporary and only relevant during the current encounter. They determine how effective different approaches are in the current location and how narrative tags activate. When the encounter ends, these tag values are discarded.

### Design Rationale

The temporary approach and focus tag system was chosen for several key reasons:
- It provides a vocabulary for describing character actions in the current encounter
- It allows the same underlying system to handle all types of encounters
- It creates a foundation for strategic and narrative tags to interact with
- It supports diverse approaches to different encounters
- It keeps persistence simple by not tracking these values between encounters

## Encounter Resources and States

### Core Resources

Each encounter tracks two primary resources:

**Momentum**: Represents progress toward the character's goal in the encounter. Players must accumulate sufficient momentum to succeed at the encounter. Momentum is primarily built through offensive choices.

**Pressure**: Represents complications, stress, and risk. When pressure builds too high, it causes negative effects and can lead to encounter failure. Pressure is reduced through defensive choices, but the location's difficulty adds pressure each turn regardless of choices.

### Secondary Resources

Depending on the encounter type, pressure damages one of three secondary resources:

**Health** (Physical Encounters): Represents physical wellbeing and stamina.  
**Confidence** (Social Encounters): Represents social standing and composure.  
**Concentration** (Intellectual Encounters): Represents mental focus and clarity.

When these resources reach zero, the encounter automatically fails.

### Encounter State

The encounter state consists of:
- Current momentum and pressure values
- Current approach and focus tag values
- Active narrative tags
- Strategic tag effects
- Current turn number and max turns
- Success thresholds for momentum

## Encounter Tag System

The encounter tag system provides the actual gameplay mechanics through two types of tags.

### Narrative Tags

Narrative tags shift the requiremenents to make it easier to harder to unlock high level cards with acucmulated focus tag value. Each location has 2-3 narrative tags out of 25 possible combinations (5 approaches × 5 focuses).

Example would be that high level Analysis choices require less built up Analysis tag value to play in Locations with the Quiet property

**Focus Tag**: Targets choices with this specific Focus Tag
**Effect**: Number of cost or discount for high level choices with a specific focus tag

### Strategic Tags

Strategic tags are always active and define how approach tag values affect momentum and pressure. Each location defines 4 strategic tags (one per approach), with the fifth approach remaining neutral.

**Effect**: Modifies momentum or pressure based on approach tag values  
**Scaling**: Effects typically scale at a rate of 1 effect point per 2 approach points  

The four strategic tag types are:
1. **Momentum Increase**: One approach that increases momentum (beneficial)
2. **Pressure Decrease**: One approach that decreases pressure (beneficial)
3. **Momentum Decrease**: One approach that decreases momentum (detrimental)
4. **Pressure Increase**: One approach that increases pressure (detrimental)

### Design Rationale

The encounter tag system creates several important gameplay dynamics:

1. **Evolving Challenges**: As players build approach tags, narrative tags activate, creating evolving constraints that force adaptation
2. **Location Personality**: Each location's unique combination of strategic tags creates a distinct strategic landscape, making different approaches naturally more effective
3. **Meaningful Specialization**: Character builds naturally excel in situations that favor their approaches, while still having options in less ideal environments
4. **Visible Consequences**: Narrative tags provide visible, logical consequences for extreme specialization
5. **Strategic Gameplay**: Players must carefully consider which approaches to build based on the current location's strategic tags

## Choice System

Choices are the primary way players interact with the game world. Each choice represents a specific approach to the current challenge.

### Choice Structure

Each choice consists of:
- **Approach**: Main approach tag increased by 1-2
- **Focus**: One focus tag increased by 1
- **Effect Type**: Momentum increase or pressure decrease
- **Base Effect**: Standard values (i.e. +2 momentum or -1 pressure)
- **Narrative Description**: Context-sensitive description of the action

### Sample Choices

Here is a selection of sample choices showing how the system works:

#### Dominance-Based Choices

**"Display of Force"** (Dominance + Physical, Momentum)
- Tag Effects: +2 Dominance, +1 Physical
- Mechanical Effect: +2 momentum
- Social Description: *"You raise your voice and speak with unwavering authority, making it clear you won't back down."*
- Intellectual Description: *"You forcefully assert your interpretation, dismissing alternative theories with conviction."*
- Physical Description: *"You flex your muscles and adopt an intimidating stance, demonstrating your physical power."*

**"Stand Ground"** (Dominance + Physical, Pressure)
- Tag Effects: +1 Dominance, +1 Physical
- Mechanical Effect: -1 pressure
- Social Description: *"You refuse to be intimidated, standing tall and maintaining your position despite challenges."*
- Intellectual Description: *"You hold firmly to your conclusion despite criticism, confident in your analysis."*
- Physical Description: *"You plant your feet and stand firm, refusing to yield ground to your opponent."*

#### Rapport-Based Choices

**"Charming Words"** (Rapport + Relationship, Momentum)
- Tag Effects: +2 Rapport, +1 Relationship
- Mechanical Effect: +2 momentum
- Social Description: *"You engage with warmth and genuine interest, making the other person feel valued and understood."*
- Intellectual Description: *"You frame your ideas in a way that resonates emotionally with your audience."*
- Physical Description: *"Your friendly demeanor puts others at ease, creating natural cooperation in physical tasks."*

**"Smooth Over"** (Rapport + Relationship, Pressure)
- Tag Effects: +1 Rapport, +1 Relationship
- Mechanical Effect: -1 pressure
- Social Description: *"You defuse tension with well-chosen words and genuine empathy, easing strained relationships."*
- Intellectual Description: *"You incorporate others' perspectives respectfully, reducing defensive reactions."*
- Physical Description: *"You use humor and camaraderie to release tension during physically challenging moments."*

#### Analysis-Based Choices

**"Analytical Insight"** (Analysis + Information, Momentum)
- Tag Effects: +2 Analysis, +1 Information
- Mechanical Effect: +2 momentum
- Social Description: *"You observe subtle social cues and patterns, gaining crucial insights into motivations and relationships."*
- Intellectual Description: *"You identify the critical connection between seemingly unrelated pieces of information."*
- Physical Description: *"You analyze patterns of movement and position, discovering an optimal approach to the physical challenge."*

**"Careful Consideration"** (Analysis + Information, Pressure)
- Tag Effects: +1 Analysis, +1 Information
- Mechanical Effect: -1 pressure
- Social Description: *"You consider all available information before speaking, avoiding potential social missteps."*
- Intellectual Description: *"You methodically rule out incorrect interpretations, preventing wasted effort."*
- Physical Description: *"You gather crucial information before acting, avoiding unnecessary risks and dangers."*

#### Precision-Based Choices

**"Precise Strike"** (Precision + Physical, Momentum)
- Tag Effects: +2 Precision, +1 Physical
- Mechanical Effect: +2 momentum
- Social Description: *"You deliver exactly the right words at exactly the right moment for maximum impact."*
- Intellectual Description: *"You focus on the exactly correct detail that unlocks the entire problem."*
- Physical Description: *"You execute a perfectly timed movement with flawless technique."*

**"Measured Response"** (Precision + Physical, Pressure)
- Tag Effects: +1 Precision, +1 Physical
- Mechanical Effect: -1 pressure
- Social Description: *"You calibrate your response perfectly to defuse tension without showing weakness."*
- Intellectual Description: *"You make careful, measured adjustments to your approach, avoiding overreactions."*
- Physical Description: *"You move with deliberate control, minimizing strain and risk of injury."*

#### Evasion-Based Choices

**"Hidden Advantage"** (Evasion + Physical, Momentum)
- Tag Effects: +2 Evasion, +1 Physical
- Mechanical Effect: +2 momentum
- Social Description: *"You hide your true capabilities until the perfect moment to reveal them for maximum effect."*
- Intellectual Description: *"You work behind the scenes, developing insights others haven't considered."*
- Physical Description: *"You move stealthily, positioning yourself for an advantageous approach."*

**"Fade Away"** (Evasion + Physical, Pressure)
- Tag Effects: +1 Evasion, +1 Physical
- Mechanical Effect: -1 pressure
- Social Description: *"You make yourself socially invisible when attention would create complications."*
- Intellectual Description: *"You withdraw your more controversial ideas temporarily, reducing resistance."*
- Physical Description: *"You slip into the shadows, removing yourself from immediate danger."*

### Design Rationale

The choice system serves several important purposes:
1. **Player Agency**: Gives players meaningful control over both immediate outcomes (momentum/pressure) and character development (approach/focus tags)
2. **Strategic Depth**: Creates interesting decisions between immediate progress and long-term approach building
3. **Character Expression**: Allows players to develop distinct play styles based on their preferred approaches
4. **Narrative Flexibility**: The same mechanical choices can be represented through different narrative lenses based on encounter type

## Choice Generation Algorithm

The choice generation algorithm deterministically selects which 4 choices appear in the player's hand each turn, based on the current encounter state.

### Step 1: Calculate Choice Scores

For **every possible choice** in the database, calculate its score using a formula based on the distance of the approach and focus tag values to the card's unique optimal approach and focus value combination.

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

### Design Rationale

The choice generation algorithm is designed to achieve several important goals:

1. **Deterministic Selection**: The algorithm produces consistent, predictable results based solely on the current game state
2. **Strategic Relevance**: Choices reflect location strategic tags, encouraging appropriate approaches
3. **Character Expression**: Character's strengths influence available options
4. **Situational Adaptation**: Hand composition responds to pressure/momentum state
5. **Visible Consequences**: Blocked choices appear in the hand to show the effects of narrative tags
6. **Balanced Options**: System ensures both offensive and defensive options are available
7. **No Narrative Dependency**: Selection process is purely mechanical, with narrative applied afterward

## Location System

Locations define the strategic and narrative context for encounters. Each location has a distinct personality expressed through its tag combinations.

### Encounter Definition Components

- **Location Name and Type**: Thematic identity and physical characteristics
- **Encounter Type**: Social, Intellectual, or Physical (for narrative presentation)
- **Strategic Tags**: Four approach-related effects (two beneficial, two detrimental)
- **Narrative Tags**: 2-3 tags that block specific focus choices at approach thresholds
- **Momentum Thresholds**: Success breakpoints (Failure, Partial, Standard, Exceptional)
- **Difficulty Level**: Determines automatic pressure increase per turn
- **Duration**: How many turns the encounter lasts
- **Resource Type**: Which secondary resource is affected by pressure (Health, Confidence, or Concentration)

### Design Rationale

The location system creates distinct strategic landscapes that encourage different play styles while maintaining consistent underlying mechanics:

1. **Contextual Strategy**: Each location naturally favors different approaches through strategic tags
2. **Balanced Challenges**: Each location has both beneficial and detrimental effects
3. **Character Development**: Players can develop characters that excel in specific location types
4. **Thematic Consistency**: Strategic and narrative tags reflect the thematic identity of each location
5. **Replayability**: The same location can present different challenges to different character builds

## Narrative AI System

The narrative system translates mechanical choices and outcomes into engaging, contextually appropriate text. While mechanics remain consistent across all encounter types, the narrative presentation changes dramatically.

### Encounter Presentation Modes

The narrative system adapts its output to three distinct presentation modes:

#### Social Encounters - Direct Speech
All choices and outcomes presented as dialogue between characters.
- Example: *"Good evening! I'm new to these parts. What can you tell me about the road conditions ahead?"*

#### Intellectual Encounters - Internal Monologue/Observations
All choices and outcomes presented as the character's thoughts and analytical observations.
- Example: *"I need to connect these disparate facts. The ledger entries from winter don't match the storehouse inventory..."*

#### Physical Encounters - Action Descriptions
All choices and outcomes presented as physical movements and environmental interactions.
- Example: *"You carefully place each foot between fallen leaves, testing the ground before shifting your weight to avoid making any sound."*

### Narrative Prompts

The narrative system uses three distinct prompts to generate appropriate text.

### Design Rationale

The narrative system serves several important purposes:

1. **Immersion**: Translates abstract mechanics into engaging narrative
2. **Contextual Presentation**: Adapts the same mechanics to different encounter types
3. **Mechanical Transparency**: Clearly represents mechanical effects through narrative
4. **Character Consistency**: Maintains consistent character voice and approach
5. **Narrative Continuity**: Creates a seamless flow between turns and encounters

## Character Archetypes

Character archetypes serve as starting points for players, each specializing in one approach and having a basic set of choices.

### Knight (Dominance)
- **Offensive Choice**: "Display of Force" (+2 momentum, +2 Dominance, +1 chosen focus)
- **Defensive Choice**: "Stand Ground" (-1 pressure, +1 Dominance, +1 chosen focus)
- Naturally excels in combat encounters
- Struggles in intellectual and diplomatic encounters

### Courtier (Rapport)
- **Offensive Choice**: "Charming Words" (+2 momentum, +2 Rapport, +1 chosen focus)
- **Defensive Choice**: "Smooth Over" (-1 pressure, +1 Rapport, +1 chosen focus)
- Naturally excels in social and negotiation encounters
- Struggles in combat and stealthy encounters

### Sage (Analysis)
- **Offensive Choice**: "Analytical Insight" (+2 momentum, +2 Analysis, +1 chosen focus)
- **Defensive Choice**: "Careful Consideration" (-1 pressure, +1 Analysis, +1 chosen focus)
- Naturally excels in intellectual and investigation encounters
- Struggles in fast-paced combat encounters

### Forester (Precision)
- **Offensive Choice**: "Precise Strike" (+2 momentum, +2 Precision, +1 chosen focus)
- **Defensive Choice**: "Measured Response" (-1 pressure, +1 Precision, +1 chosen focus)
- Naturally excels in exploration and hunting encounters
- Balances well across most encounter types

### Shadow (Evasion)
- **Offensive Choice**: "Hidden Advantage" (+2 momentum, +2 Evasion, +1 chosen focus)
- **Defensive Choice**: "Fade Away" (-1 pressure, +1 Evasion, +1 chosen focus)
- Naturally excels in stealth and theft encounters
- Struggles in direct social encounters

### Design Rationale

The character archetype system provides several benefits:

1. **Accessible Starting Points**: Clear archetypes make it easy for players to get started
2. **Natural Specializations**: Each archetype naturally excels in certain encounter types
3. **Development Paths**: Archetypes provide a foundation for more complex character builds
4. **Strategic Variety**: Different archetypes approach the same challenges in different ways
5. **Replay Value**: Players can try different archetypes for a fresh experience

## Turn Structure and Encounter Flow

### Turn Sequence

Each encounter turn follows this sequence:

1. **Generate Choices**: Apply the choice generation algorithm to create the player's hand
2. **Present Choices**: Display the narrative descriptions for each choice
3. **Player Selection**: Player selects one of the available choices
4. **Apply Effects**: Update momentum, pressure, and tag values
5. **Generate Outcome**: Create narrative outcome for the selected choice
6. **Check End Conditions**: Determine if the encounter has ended
7. **Set New Situation**: If the encounter continues, present the new challenge

### Encounter Initialization

At the start of an encounter:
- Set momentum to 0
- Set pressure to 0
- Initialize approach and focus tags from character's current values
- Set secondary resource (Health/Confidence/Concentration) to starting value
- Apply the encounter setup prompt to generate initial narrative

### End Conditions

An encounter ends when one of the following occurs:
- **Success**: Momentum reaches or exceeds the success threshold
- **Failure (Pressure)**: Pressure reaches or exceeds the maximum pressure threshold
- **Failure (Resource)**: Secondary resource reaches 0
- **Failure (Time)**: Maximum turn count is reached without sufficient momentum

### Encounter Resolution

When an encounter ends, the outcome is determined by the momentum level reached:
- **Failure**: Momentum below the partial success threshold
- **Partial Success**: Momentum between partial and standard thresholds
- **Standard Success**: Momentum between standard and exceptional thresholds
- **Exceptional Success**: Momentum reaches or exceeds the exceptional threshold

## Example Encounter

The following example demonstrates how all these systems work together to create a complete encounter experience.

### Ancient Library Encounter

**Character**: Scholar (Analysis-focused)
- Starting Approach Tags: Analysis 2, others 0
- Starting Focus Tags: All 0
- Momentum: 0
- Pressure: 0
- Concentration: 10

**Location**: Ancient Library (Intellectual Encounter)
- Strategic Tags: Analysis increases momentum, Precision decreases pressure, Dominance increases pressure, Rapport decreases momentum
- Narrative Tags: "Detail Fixation" (Analysis 3+ blocks Environment focus), "Theoretical Mindset" (Analysis 4+ blocks Resource focus)
- Difficulty: 2 (adds +2 pressure per turn)
- Goal: Find a specific ancient text before a rival scholar

**Turn 1**:

*Initial Situation*:
*You enter the dimly lit ancient library, dust motes dancing in shafts of light from high windows. The smell of old parchment and leather bindings fills the air. Towering bookshelves stretch in all directions, creating a labyrinth of knowledge. Near the entrance, an elderly caretaker watches you with suspicion from behind a worn desk. In the distance, you glimpse your academic rival already consulting a catalog.*

*Player Choices*:
1. **Analytical Insight** (Analysis + Information, Momentum)
2. **Assess Relationships** (Analysis + Relationship, Momentum)
3. **Create Ambiance** (Rapport + Environment, Momentum)
4. **Calculate Risks** (Analysis + Physical, Pressure)

*Player Selects*: "Analytical Insight"

*Outcome*:
*Your trained scholar's eye quickly discerns the library uses a modified Alexandrian classification system. The mathematical and geographical texts—most likely to contain the artifact's location—would be housed in the eastern wing, significantly narrowing your search area. As you make these connections, you become deeply absorbed in the theoretical framework of the cataloging system, its elegantly nested categories capturing your full attention.*

*However, as your mind races through the possibilities, you barely notice the physical deterioration of the surrounding shelves or the caretaker's growing interest in your methodical analysis. Your focus has narrowed to the intellectual puzzle, making the broader environment fade into the background.*

*New Situation*:
*With your understanding of the library's organization, you now need access to the eastern wing. The elderly caretaker holds a large ring of keys at his belt, suggesting the most valuable collections are locked. Your rival appears to be struggling with the general collection, not yet having deduced the correct section to search.*

**Turn 2**:

*Player Choices*:
1. **Targeted Question** (Precision + Information, Momentum)
2. **Command Attention** (Dominance + Relationship, Momentum)
3. **Gather Secrets** (Evasion + Information, Momentum)
4. **Assess Relationships** (Analysis + Relationship, Momentum)

*Player Selects*: "Targeted Question"

*Outcome*:
*You approach the caretaker and ask, "I'm particularly interested in examining Ptolemy's geographical codices with the marginal annotations by Abbot Severus. Would those be found in the locked eastern collection?"*

*The caretaker's eyes widen with impressed surprise. "You know of Severus's annotations? Most visitors aren't aware of such specifics." He studies you with newfound respect. "The eastern collection does indeed house those works, along with several related geographical treatises from the same period."*

*Your precisely targeted question not only yields valuable information about the collection's organization but also establishes your credibility. The caretaker relaxes his guarded posture slightly. Meanwhile, your careful, methodical approach helps you maintain focus and manage the growing pressure of the challenge.*

*However, as you converse, you notice your academic rival has recruited additional assistants who are now systematically checking catalog entries.*

And so the encounter would continue through each turn, with the player making choices that affect their approach and focus tags, momentum and pressure, and ultimately determine whether they succeed in finding the text before their rival.

## Conclusion

The Wayfarer encounter system creates a universal framework for all interactions in the game, replacing traditional specialized systems with an elegant tag-based approach. Key strengths include:

1. **Unified Mechanics**: The same core systems handle social, intellectual, and physical encounters
2. **Strategic Depth**: Location strategic tags create meaningful choices about approach selection
3. **Character Archetypes**: Natural advantages for different character types in different situations
4. **Narrative Integration**: Mechanical systems are presented through appropriate narrative context
5. **Elegant Tag System**: Simple approach and focus tags interact to create complex gameplay
6. **Deterministic Design**: Clear, precise rules ensure consistent and predictable behavior

The system's design comes from its clear separation of responsibilities:
- **Base Layer**: Choices modify approach and focus tags
- **Encounter Layer**: Strategic and narrative tags determine mechanical effects
- **Presentation Layer**: The narrative adapts to encounter type

This architecture creates meaningful player agency and varied encounter experiences while maintaining consistent underlying mechanics.

The design prioritizes elegant simplicity over unnecessary complexity, creating depth through the interaction of simple systems rather than through complex individual mechanisms. This supports the game's goal of creating immersive, narratively rich encounters that allow players to develop distinct approaches to challenges in a medieval world.