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

## Character Archetypes

Character archetypes serve as starting points for players, each specializing in one approach and having a basic set of choices.

- Artisan
- Herbalist
- Scribe
- Shadow
- Merchant

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
- Set secondary resource (Health/Spirit/Focus) to starting value
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

### Library Encounter

**Character**: Scholar (Analysis-focused)
- Starting Approach Tags: Analysis 2, others 0
- Starting Focus Tags: All 0
- Momentum: 0
- Pressure: 0
- Focus: 10

**Location**: Ancient Library (Intellectual Encounter)
- Strategic Tags: Analysis increases momentum, Precision decreases pressure, Dominance increases pressure, Rapport decreases momentum
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