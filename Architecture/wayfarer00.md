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

Narrative tags activate at specific approach tag thresholds and block specific focus tags from appearing in choices. Each location has 2-3 narrative tags out of 25 possible combinations (5 approaches × 5 focuses).

**Activation**: When an approach tag reaches the specified threshold (typically 3+ or 4+)  
**Effect**: Blocks choices with a specific focus from appearing in the player's hand  
**Deactivation**: If the approach value drops below the threshold  

**Complete List of Narrative Tags:**

#### Dominance-Based Narrative Tags

1. **"Intimidating Presence"** (Dominance → Blocks Relationship)
   - **Activation**: Dominance 3+
   - **Description**: Your forceful demeanor causes others to keep their distance, making it impossible to form genuine connections. Others respond with caution rather than openness.

2. **"Battle Rage"** (Dominance → Blocks Information)
   - **Activation**: Dominance 3+
   - **Description**: Your aggressive focus narrows your attention to immediate threats, making it difficult to process complex information or notice subtle details.

3. **"Brute Force Fixation"** (Dominance → Blocks Physical)
   - **Activation**: Dominance 4+
   - **Description**: Your reliance on overwhelming force makes precise physical manipulation impossible. You're too focused on power to execute delicate physical tasks.

4. **"Tunnel Vision"** (Dominance → Blocks Environment)
   - **Activation**: Dominance 3+
   - **Description**: Your dominant approach narrows your focus to immediate targets, blinding you to environmental factors and surroundings.

5. **"Destructive Impulse"** (Dominance → Blocks Resource)
   - **Activation**: Dominance 3+
   - **Description**: Your forceful approach threatens to damage or waste valuable resources. Others are reluctant to let you handle or negotiate for important items.

#### Rapport-Based Narrative Tags

6. **"Superficial Charm"** (Rapport → Blocks Relationship)
   - **Activation**: Rapport 4+
   - **Description**: Your social approach has become too polished and shallow, preventing the formation of deeper, meaningful connections. You're all charm, no substance.

7. **"Social Distraction"** (Rapport → Blocks Information)
   - **Activation**: Rapport 3+
   - **Description**: Your focus on pleasantries and social niceties distracts from gathering practical information. The conversation remains enjoyable but unproductive.

8. **"Hesitant Politeness"** (Rapport → Blocks Physical)
   - **Activation**: Rapport 3+
   - **Description**: Your concern with social propriety makes you reluctant to take direct physical action, especially anything that might appear ungraceful or impolite.

9. **"Public Awareness"** (Rapport → Blocks Environment)
   - **Activation**: Rapport 3+
   - **Description**: Your focus on maintaining social connections makes you overlook your surroundings. The people have your complete attention, not the place.

10. **"Generous Spirit"** (Rapport → Blocks Resource)
    - **Activation**: Rapport 4+
    - **Description**: Your sociable approach makes material concerns seem petty. You're reluctant to focus on resources when relationships are at stake.

#### Analysis-Based Narrative Tags

11. **"Cold Calculation"** (Analysis → Blocks Relationship)
    - **Activation**: Analysis 3+
    - **Description**: Your analytical approach makes emotional connections difficult. Others perceive you as cold and calculating rather than warm and approachable.

12. **"Analysis Paralysis"** (Analysis → Blocks Information)
    - **Activation**: Analysis 4+
    - **Description**: You're overthinking everything, making it impossible to process new information clearly. You're lost in your own thoughts.

13. **"Overthinking"** (Analysis → Blocks Physical)
    - **Activation**: Analysis 3+
    - **Description**: Your analytical focus prevents instinctive physical action. You're too busy calculating trajectories and outcomes to act decisively.

14. **"Detail Fixation"** (Analysis → Blocks Environment)
    - **Activation**: Analysis 3+
    - **Description**: Your intense focus on specific analytical details blinds you to the broader environment and context.

15. **"Theoretical Mindset"** (Analysis → Blocks Resource)
    - **Activation**: Analysis 4+
    - **Description**: You're lost in abstract theory, ignoring practical resource concerns. The theoretical solution interests you more than material practicalities.

#### Precision-Based Narrative Tags

16. **"Mechanical Interaction"** (Precision → Blocks Relationship)
    - **Activation**: Precision 3+
    - **Description**: Your precise, technical approach comes across as mechanical and inhuman. Others feel like they're talking to an automaton rather than a person.

17. **"Narrow Focus"** (Precision → Blocks Information)
    - **Activation**: Precision 3+
    - **Description**: Your precision makes you focus too narrowly, missing broader informational patterns and contexts that would be valuable.

18. **"Perfectionist Paralysis"** (Precision → Blocks Physical)
    - **Activation**: Precision 4+
    - **Description**: Your obsession with perfect execution prevents fluid physical movement. You're so concerned with getting it exactly right that you can't act naturally.

19. **"Detail Obsession"** (Precision → Blocks Environment)
    - **Activation**: Precision 3+
    - **Description**: Your precise focus on specific details blinds you to the wider environment. You can't see the forest for the trees.

20. **"Inefficient Perfectionism"** (Precision → Blocks Resource)
    - **Activation**: Precision 4+
    - **Description**: Your precise approach wastes resources in pursuit of perfect outcomes. You're unwilling to settle for "good enough" when resources are concerned.

#### Evasion-Based Narrative Tags

21. **"Shadow Veil"** (Evasion → Blocks Relationship)
    - **Activation**: Evasion 3+
    - **Description**: Your hidden approach prevents genuine relationship building. You're too guarded to form meaningful connections with others.

22. **"Paranoid Mindset"** (Evasion → Blocks Information)
    - **Activation**: Evasion 3+
    - **Description**: Your secretive approach makes you question all information you receive. You can't process new data when you're constantly looking for traps and lies.

23. **"Cautious Restraint"** (Evasion → Blocks Physical)
    - **Activation**: Evasion 3+
    - **Description**: Your hidden approach prevents direct physical action. You're too concerned with maintaining cover to act decisively.

24. **"Hiding Place Fixation"** (Evasion → Blocks Environment)
    - **Activation**: Evasion 4+
    - **Description**: Your focus on remaining concealed limits your environmental awareness. You only see the environment in terms of places to hide.

25. **"Hoarding Instinct"** (Evasion → Blocks Resource)
    - **Activation**: Evasion 3+
    - **Description**: Your secretive approach makes you hide rather than use resources effectively. You'd rather keep resources hidden than risk revealing yourself by using them.

### Strategic Tags

Strategic tags are always active and define how approach tag values affect momentum and pressure. Each location defines 4 strategic tags (one per approach), with the fifth approach remaining neutral.

**Activation**: Always active  
**Effect**: Modifies momentum or pressure based on approach tag values  
**Scaling**: Effects typically scale at a rate of 1 effect point per 2 approach points  

The four strategic tag types are:
1. **Momentum Increase**: One approach that increases momentum (beneficial)
2. **Pressure Decrease**: One approach that decreases pressure (beneficial)
3. **Momentum Decrease**: One approach that decreases momentum (detrimental)
4. **Pressure Increase**: One approach that increases pressure (detrimental)

**Complete List of Strategic Tags:**

#### Dominance-Based Strategic Tags

1. **"Overwhelming Force"** (Dominance → Increases Momentum)
   - **Effect**: Adds momentum proportional to Dominance value
   - **Scaling**: +1 momentum per 2 points in Dominance
   - **Description**: Your forceful approach intimidates opponents and clears obstacles, creating opportunities for progress.

2. **"Commanding Presence"** (Dominance → Decreases Pressure)
   - **Effect**: Reduces pressure proportional to Dominance value
   - **Scaling**: -1 pressure per 2 points in Dominance
   - **Description**: Your authoritative demeanor keeps threats at bay, reducing complications and risks.

3. **"Brute Force Backfire"** (Dominance → Decreases Momentum)
   - **Effect**: Reduces momentum proportional to Dominance value
   - **Scaling**: -1 momentum per 2 points in Dominance
   - **Description**: Your forceful approach alienates others or damages delicate objects, hindering progress.

4. **"Escalating Tension"** (Dominance → Increases Pressure)
   - **Effect**: Adds pressure proportional to Dominance value
   - **Scaling**: +1 pressure per 2 points in Dominance
   - **Description**: Your aggressive approach heightens tensions and creates additional complications.

#### Rapport-Based Strategic Tags

5. **"Social Currency"** (Rapport → Increases Momentum)
   - **Effect**: Adds momentum proportional to Rapport value
   - **Scaling**: +1 momentum per 2 points in Rapport
   - **Description**: Your charming approach opens doors and creates opportunities. People naturally want to help you succeed.

6. **"Calming Influence"** (Rapport → Decreases Pressure)
   - **Effect**: Reduces pressure proportional to Rapport value
   - **Scaling**: -1 pressure per 2 points in Rapport
   - **Description**: Your amiable approach defuses tensions and prevents complications from escalating.

7. **"Social Distraction"** (Rapport → Decreases Momentum)
   - **Effect**: Reduces momentum proportional to Rapport value
   - **Scaling**: -1 momentum per 2 points in Rapport
   - **Description**: Your focus on social niceties wastes valuable time. Pleasant conversation replaces meaningful progress.

8. **"Social Awkwardness"** (Rapport → Increases Pressure)
   - **Effect**: Adds pressure proportional to Rapport value
   - **Scaling**: +1 pressure per 2 points in Rapport
   - **Description**: Your attempts at charm come across as inappropriate or suspicious in this context.

#### Analysis-Based Strategic Tags

9. **"Insightful Approach"** (Analysis → Increases Momentum)
   - **Effect**: Adds momentum proportional to Analysis value
   - **Scaling**: +1 momentum per 2 points in Analysis
   - **Description**: Your analytical approach reveals optimal solutions and efficient paths forward.

10. **"Calculated Response"** (Analysis → Decreases Pressure)
    - **Effect**: Reduces pressure proportional to Analysis value
    - **Scaling**: -1 pressure per 2 points in Analysis
    - **Description**: Your thoughtful approach anticipates and prevents complications before they arise.

11. **"Overthinking"** (Analysis → Decreases Momentum)
    - **Effect**: Reduces momentum proportional to Analysis value
    - **Scaling**: -1 momentum per 2 points in Analysis
    - **Description**: Your analytical approach leads to paralysis by analysis. You're caught up in examining possibilities.

12. **"Analytical Anxiety"** (Analysis → Increases Pressure)
    - **Effect**: Adds pressure proportional to Analysis value
    - **Scaling**: +1 pressure per 2 points in Analysis
    - **Description**: Your overanalysis reveals all possible failure points, increasing your stress and pressure.

#### Precision-Based Strategic Tags

13. **"Masterful Execution"** (Precision → Increases Momentum)
    - **Effect**: Adds momentum proportional to Precision value
    - **Scaling**: +1 momentum per 2 points in Precision
    - **Description**: Your precise approach eliminates wasted effort and maximizes effectiveness.

14. **"Careful Positioning"** (Precision → Decreases Pressure)
    - **Effect**: Reduces pressure proportional to Precision value
    - **Scaling**: -1 pressure per 2 points in Precision
    - **Description**: Your precise movements and careful approach reduce the risk of errors and complications.

15. **"Rigid Methodology"** (Precision → Decreases Momentum)
    - **Effect**: Reduces momentum proportional to Precision value
    - **Scaling**: -1 momentum per 2 points in Precision
    - **Description**: Your precise approach is too methodical for this situation. The need for exactness slows your progress.

16. **"Perfectionist Pressure"** (Precision → Increases Pressure)
    - **Effect**: Adds pressure proportional to Precision value
    - **Scaling**: +1 pressure per 2 points in Precision
    - **Description**: Your precise approach creates additional pressure as you strive for perfection in a situation that doesn't require it.

#### Evasion-Based Strategic Tags

17. **"Tactical Advantage"** (Evasion → Increases Momentum)
    - **Effect**: Adds momentum proportional to Evasion value
    - **Scaling**: +1 momentum per 2 points in Evasion
    - **Description**: Your hidden approach provides the element of surprise, creating opportunities for progress.

18. **"Invisible Presence"** (Evasion → Decreases Pressure)
    - **Effect**: Reduces pressure proportional to Evasion value
    - **Scaling**: -1 pressure per 2 points in Evasion
    - **Description**: Your stealthy approach prevents complications from arising. By avoiding detection, you sidestep many potential problems.

19. **"Overcautious Approach"** (Evasion → Decreases Momentum)
    - **Effect**: Reduces momentum proportional to Evasion value
    - **Scaling**: -1 momentum per 2 points in Evasion
    - **Description**: Your hidden approach is unnecessarily cautious for this situation. You waste time hiding when direct action would be more effective.

20. **"Suspicious Behavior"** (Evasion → Increases Pressure)
    - **Effect**: Adds pressure proportional to Evasion value
    - **Scaling**: +1 pressure per 2 points in Evasion
    - **Description**: Your secretive approach raises suspicions and creates complications. The more you try to hide, the more attention you draw to yourself.

### Design Rationale

The encounter tag system creates several important gameplay dynamics:

1. **Evolving Challenges**: As players build approach tags, narrative tags activate that block certain focuses, creating evolving constraints that force adaptation
2. **Location Personality**: Each location's unique combination of strategic tags creates a distinct strategic landscape, making different approaches naturally more effective
3. **Meaningful Specialization**: Character builds naturally excel in situations that favor their approaches, while still having options in less ideal environments
4. **Visible Consequences**: Narrative tags provide visible, logical consequences for extreme specialization
5. **Strategic Gameplay**: Players must carefully consider which approaches to build based on the current location's strategic tags

## Choice System

Choices are the primary way players interact with the game world. Each choice represents a specific approach to the current challenge.

### Choice Structure

Each choice consists of:
- **Primary Approach**: Main approach tag increased by 1-2
- **Secondary Approach**: Optional second approach tag modified by ±1-2
- **Focus**: One focus tag increased by 1
- **Effect Type**: Momentum increase or pressure decrease
- **Base Effect**: Standard values (+2 momentum or -1 pressure)
- **Narrative Description**: Context-sensitive description of the action

### Core Choice Types

In the basic version, each approach has two primary choice types:

#### Offensive Choices (Momentum Building)
- Base effect: +2 momentum
- Primary approach increase: +2
- Focus tag increase: +1 to chosen focus

#### Defensive Choices (Pressure Management)
- Base effect: -1 pressure
- Primary approach increase: +1
- Focus tag increase: +1 to chosen focus

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

### Location Definition Components

- **Location Name and Type**: Thematic identity and physical characteristics
- **Encounter Type**: Social, Intellectual, or Physical (for narrative presentation)
- **Strategic Tags**: Four approach-related effects (two beneficial, two detrimental)
- **Narrative Tags**: 2-3 tags that block specific focus choices at approach thresholds
- **Momentum Thresholds**: Success breakpoints (Failure, Partial, Standard, Exceptional)
- **Difficulty Level**: Determines automatic pressure increase per turn
- **Duration**: How many turns the encounter lasts
- **Resource Type**: Which secondary resource is affected by pressure (Health, Confidence, or Concentration)

### Strategic Tag Implementation

Each location defines strategic tags for 4 out of 5 approaches:
1. **Momentum Increase** (beneficial): One approach that increases momentum
2. **Pressure Decrease** (beneficial): One approach that decreases pressure
3. **Momentum Decrease** (detrimental): One approach that decreases momentum
4. **Pressure Increase** (detrimental): One approach that increases pressure

The fifth approach has no strategic tag effects (neutral).

### Narrative Tag Implementation

- Each location defines 2-3 narrative tags
- Each tag activates at a specific approach threshold
- When active, each tag blocks one focus type from appearing in choices
- Tags deactivate if approach value drops below threshold

### Sample Location: Ancient Library (Intellectual Encounter)

```
Location: Ancient Library

Core Properties:
- Encounter Type: Intellectual
- Difficulty: 2 (adds +2 pressure per turn)
- Duration: 6 turns
- Resource Type: Concentration
- Momentum Thresholds: 0-11 (Failure), 12-15 (Partial), 16-19 (Standard), 20+ (Exceptional)

Strategic Tags:
- "Insightful Approach" (Analysis → Increases Momentum): +1 momentum per 2 points in Analysis
- "Careful Positioning" (Precision → Decreases Pressure): -1 pressure per 2 points in Precision
- "Escalating Tension" (Dominance → Increases Pressure): +1 pressure per 2 points in Dominance
- "Social Distraction" (Rapport → Decreases Momentum): -1 momentum per 2 points in Rapport
- Evasion: Neutral (no strategic effect)

Narrative Tags:
- "Detail Fixation" (Analysis 3+): Blocks Environment focus choices
- "Theoretical Mindset" (Analysis 4+): Blocks Resource focus choices
- "Paranoid Mindset" (Evasion 3+): Blocks Information focus choices
```

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

#### 1. Encounter Setup Prompt

This prompt generates the initial narrative that sets the scene at the beginning of an encounter.

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

#### 2. Choice Description Prompt

This prompt transforms mechanical choices into narratively appropriate descriptions for player selection.

```
Transform the following 4 mechanical choices into narrative descriptions for a [ENCOUNTER_TYPE] encounter in a [LOCATION_TYPE]. The character is currently [CURRENT_SITUATION] with the objective to [CHARACTER_GOAL].

Current state: Momentum [M], Pressure [P]
Approach values: Analysis [A], Precision [P], Rapport [R], Dominance [D], Evasion [C]
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

#### 3. Turn Resolution Prompt

This prompt generates the outcome of the player's choice and introduces the new situation/challenge for the next turn.

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

### Design Rationale

The narrative system serves several important purposes:

1. **Immersion**: Translates abstract mechanics into engaging narrative
2. **Contextual Presentation**: Adapts the same mechanics to different encounter types
3. **Mechanical Transparency**: Clearly represents mechanical effects through narrative
4. **Character Consistency**: Maintains consistent character voice and approach
5. **Narrative Continuity**: Creates a seamless flow between turns and encounters

## Character Archetypes

Character archetypes serve as starting points for players, each specializing in one approach and having a basic set of choices.

### Warrior (Dominance)
- **Offensive Choice**: "Display of Force" (+2 momentum, +2 Dominance, +1 chosen focus)
- **Defensive Choice**: "Stand Ground" (-1 pressure, +1 Dominance, +1 chosen focus)
- Naturally excels in combat encounters
- Struggles in intellectual and diplomatic encounters

### Bard (Rapport)
- **Offensive Choice**: "Charming Words" (+2 momentum, +2 Rapport, +1 chosen focus)
- **Defensive Choice**: "Smooth Over" (-1 pressure, +1 Rapport, +1 chosen focus)
- Naturally excels in social and negotiation encounters
- Struggles in combat and stealthy encounters

### Scholar (Analysis)
- **Offensive Choice**: "Analytical Insight" (+2 momentum, +2 Analysis, +1 chosen focus)
- **Defensive Choice**: "Careful Consideration" (-1 pressure, +1 Analysis, +1 chosen focus)
- Naturally excels in intellectual and investigation encounters
- Struggles in fast-paced combat encounters

### Ranger (Precision)
- **Offensive Choice**: "Precise Strike" (+2 momentum, +2 Precision, +1 chosen focus)
- **Defensive Choice**: "Measured Response" (-1 pressure, +1 Precision, +1 chosen focus)
- Naturally excels in exploration and hunting encounters
- Balances well across most encounter types

### Thief (Evasion)
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