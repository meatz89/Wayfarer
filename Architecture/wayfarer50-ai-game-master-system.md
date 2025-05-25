# Wayfarer's Resolve: AI Game Master System

## I. Introduction & Design Philosophy

### Vision Statement

Wayfarer's Resolve reimagines encounter design as a dynamic collaboration between deterministic game systems and AI narration. By replacing static encounter structures with an adaptive AI Game Master working within carefully defined mechanical boundaries, we create encounters that respond naturally to player choices while maintaining strategic depth and mechanical integrity.

### Core Problems Addressed

Traditional encounter systems face several inherent limitations:

1. **Narrative-Mechanical Disconnect**: Scripted encounter systems often separate narrative (what's happening in the story) from mechanics (what's happening in the game systems).

2. **Limited Adaptability**: Pre-defined encounter flows cannot meaningfully react to the full range of player approaches or circumstances.

3. **Repetitive Patterns**: Players quickly identify optimal strategies once they understand the underlying mechanics.

4. **Development Scalability**: Creating rich, varied encounters requires extensive hand-crafting of content.

### Design Principles

Our approach is guided by five fundamental principles:

1. **Bounded Narrative Freedom**: The AI has significant narrative control within clearly defined mechanical boundaries.

2. **Strategic Transparency**: All mechanical aspects of choices (costs, requirements, potential outcomes) are fully visible to players before decisions.

3. **Skill Expression**: Character capabilities directly influence available options and their chances of success.

4. **Resource Tension**: Finite Focus Points create meaningful strategic pressure throughout encounters.

5. **Universal Adaptability**: The same core mechanics work across all encounter types (social, physical, intellectual) while feeling contextually appropriate.

## II. System Overview

### Core Components

The AI Game Master system integrates five primary components:

1. **Focus Point Economy**: A finite pool of 6-8 points per encounter representing available mental and physical effort.

2. **Skill Card System**: Cards representing specific abilities that players select during daily planning and exhaust when used in encounters.

3. **Universal State Flags (UESTs)**: A standardized set of binary flags representing the encounter's narrative state, serving as the primary way for the AI to track progress.

4. **Payload System**: A library of predefined mechanical effects that the AI can select from, ensuring all narrative developments have appropriate mechanical consequences.

5. **AI Prompt Engineering**: Carefully designed prompts that guide the AI in creating contextually appropriate choices, setting fair difficulties, and narrating outcomes.

### Player Experience Flow

From the player's perspective, encounters follow this pattern:

1. **Morning Planning**: Select skill cards to prepare for anticipated challenges.

2. **Encounter Initiation**: System provides context to AI; AI generates initial situation.

3. **Beat Cycle**:
   - AI presents 3-5 contextual choices with clear mechanical parameters
   - Player selects choice and appropriate skill card
   - System resolves skill check and applies payload
   - AI narrates outcome and updates state
   - Repeat until conclusion

4. **Encounter Conclusion**: AI determines success/failure based on UEST state, proposes persistent changes, and provides narrative resolution.

5. **Return to World Map**: System applies validated persistent changes to game state.

## III. Mechanical Framework

### Focus Point Economy

Focus Points represent a character's available mental and physical capacity during an encounter.

**Core Rules:**
- Each encounter begins with 6-8 Focus Points (based on encounter significance)
- Standard choices cost 1 Focus Point
- Powerful choices cost 2 Focus Points
- Recovery options cost 0 Focus Points
- Focus cannot be carried between encounters
- When Focus is depleted, only 0-cost recovery options remain available
- Encounter concludes when goal is achieved or Focus is exhausted with goal unreached

**Strategic Depth:**
- Focus creates natural tension between immediate action and conservation
- The finite pool forces prioritization of approaches
- Recovery options provide strategic decisions about when to rebuild resources

**Anti-Stalling Mechanisms:**
- Consecutive recovery attempts face increasing difficulty
- Diminishing returns for repeated recovery
- Each recovery advances the duration counter
- Hard cap of 8 duration units forces encounter conclusion

### Skill Card System

Skill Cards represent specific abilities a character has developed, replacing the abstract "approach" system from earlier designs.

**Card Categories:**
1. **Physical Skills**: Brute Force, Acrobatics, Lockpicking, etc.
2. **Intellectual Skills**: Investigation, Perception, Strategy, etc.
3. **Social Skills**: Etiquette, Negotiation, Acting, Threatening, etc.

**Card Levels & Costs:**
- Each card has a level (1-5) providing corresponding bonus (+1 to +5)
- Physical cards cost Energy equal to level for daily preparation
- Intellectual cards cost Concentration equal to level for daily preparation
- Social cards have no preparation cost
- Players select a limited hand of cards each morning based on available Energy/Concentration

**Card Usage in Encounters:**
- Each choice may require a specific Skill Card
- Using a card exhausts it for the duration of the encounter
- Multiple skill options may be available for the same narrative choice
- Cards provide direct bonuses to skill checks
- Exhausted cards must be refreshed outside encounters through specific activities

**Untrained Attempts:**
- When a player lacks the ideal Skill Card, two options exist:
  1. AI offers alternative choices using available cards
  2. For critical paths, player may attempt "Untrained" at severe disadvantage:
     - Base skill level 0
     - +2 SCD difficulty modifier
     - More severe failure consequences
     - Clear UI indication of disadvantage

### Universal State Flag System (UESTs)

Universal State Flags provide a standardized vocabulary for the AI to track narrative state across all encounter types.

**Flag Categories:**

1. **Positional Flags** (spatial relationship to situation)
   - ADVANTAGEOUS_POSITION
   - DISADVANTAGEOUS_POSITION
   - HIDDEN_POSITION
   - EXPOSED_POSITION

2. **Relational Flags** (social/emotional connections)
   - TRUST_ESTABLISHED
   - DISTRUST_TRIGGERED
   - RESPECT_EARNED
   - HOSTILITY_PROVOKED

3. **Informational Flags** (knowledge state)
   - INSIGHT_GAINED
   - SECRET_REVEALED
   - DECEPTION_DETECTED
   - CONFUSION_CREATED

4. **Tactical Flags** (action opportunities)
   - SURPRISE_ACHIEVED
   - PREPARATION_COMPLETED
   - PATH_CLEARED
   - RESOURCE_SECURED

5. **Environmental Flags** (surroundings)
   - AREA_SECURED
   - DISTRACTION_CREATED
   - HAZARD_NEUTRALIZED
   - OBSTACLE_PRESENT

6. **Emotional Flags** (mood/tone)
   - TENSION_INCREASED
   - CONFIDENCE_BUILT
   - FEAR_INSTILLED
   - URGENCY_CREATED

**Flag Management Rules:**
- Setting a flag automatically clears its opposite (e.g., TRUST_ESTABLISHED clears DISTRUST_TRIGGERED)
- Maximum 1-2 active flags per category to maintain clarity
- Most recently set flag takes precedence in conflicts
- The AI internally defines which combination of flags constitutes goal achievement

### Skill Check Resolution

Skill checks determine whether negative consequences trigger when attempting actions.

**Resolution Formula:**
- Success occurs when: Skill Level + Card Bonus ≥ SCD
- Failure occurs when: Skill Level + Card Bonus < SCD

**Difficulty Tiers:**
- Easy (SCD 2): Most people can accomplish this
- Standard (SCD 3): Requires some competence
- Hard (SCD 4): Demands significant skill
- Exceptional (SCD 5): Challenges even masters

**Key Principles:**
- Positive effects always occur regardless of check outcome
- Skill checks only determine if negative consequences trigger
- Check difficulty should match narrative context
- Multiple skill options may be available for the same narrative choice
- Each skill option may have its own difficulty level and consequences

### Encounter Flow Management

The system manages encounter progression through a series of beats until conclusion.

**Initialization:**
1. System provides context (location, character stats, goal)
2. AI determines success conditions as flag combinations
3. System allocates Focus Points (6-8 based on encounter type)
4. AI generates initial situation and 3-4 choices

**Beat Cycle:**
1. Player selects choice and applicable skill card
2. System resolves check and applies mechanical effects
3. AI narrates outcome and updates state
4. AI determines if goal conditions are met
5. If not, AI generates next beat choices

**Conclusion Triggers:**
- Goal achievement (required UESTs are active)
- Focus depletion with goal unreached
- Duration counter reaching maximum limit

**Conclusion Processing:**
1. AI generates appropriate narrative conclusion
2. AI proposes persistent changes (relationships, items, knowledge)
3. System validates changes against encounter type
4. System applies approved changes to game state

## IV. AI Integration Framework

### Permission Tiers System

To maintain balance and narrative cohesion, the AI operates within clearly defined boundaries:

**System-Controlled (AI Cannot Modify)**
- Current player location (overworld movement)
- Core skill progression/levels
- Quest completion states
- Major inventory changes
- Time advancement

**System-Validated (AI Can Suggest)**
- Minor item acquisitions (consumables only)
- Currency changes (±10 maximum per encounter)
- Relationship adjustments (tiered by significance)
- New NPCs and locations (through proposal system)

**AI-Controlled (Freely Managed)**
- Dialogue content
- Encounter-specific narrative
- Environmental descriptions
- Challenge framing and difficulty

### Structured Communication Protocols

The AI communicates with the system through precisely structured formats:

**1. Choice Presentation Format**

```json
{
  "beatNarration": "The merchant eyes you suspiciously.",
  "availableChoices": [
    {
      "choiceID": "BEAT1_CHOICE_A",
      "narrativeText": "Engage in polite conversation",
      "focusCost": 1,
      "skillOptions": [
        {
          "skillName": "Etiquette",
          "difficulty": "STANDARD",
          "sCD": 3,
          "successPayload": {
            "narrativeEffect": "The merchant warms to your approach",
            "mechanicalEffectID": "SET_FLAG_TRUST_ESTABLISHED"
          },
          "failurePayload": {
            "narrativeEffect": "Your attempt falls flat",
            "mechanicalEffectID": "SET_FLAG_AWKWARD_SITUATION"
          }
        },
        {
          "skillName": "Negotiation",
          "difficulty": "HARD",
          "sCD": 4,
          "successPayload": {
            "narrativeEffect": "The merchant respects your directness",
            "mechanicalEffectID": "SET_FLAG_RESPECT_EARNED"
          },
          "failurePayload": {
            "narrativeEffect": "The merchant finds you presumptuous",
            "mechanicalEffectID": "SET_FLAG_DISTRUST_TRIGGERED"
          }
        }
      ]
    },
    // Additional choices...
  ]
}
```

**2. New Element Proposal Format**

```json
{
  "proposedNewElements": [
    {
      "elementType": "CHARACTER",
      "elementName": "Merchant Silas",
      "elementDescription": "A weathered merchant with suspicious eyes",
      "suggestedPersistence": true,
      "suggestedLocation": "MARKET_DISTRICT",
      "relevance": "Potential information source about stolen goods"
    }
  ]
}
```

**3. Encounter Conclusion Format**

```json
{
  "encounterOutcome": "SUCCESS",
  "narrativeSummary": "You've convinced the merchant to reveal information.",
  "activeStateFlags": ["TRUST_ESTABLISHED", "SECRET_REVEALED"],
  "proposedPersistentChanges": [
    {
      "changeType": "RELATIONSHIP",
      "target": "MERCHANT_SILAS",
      "magnitude": "MODERATE",
      "direction": "POSITIVE"
    },
    {
      "changeType": "KNOWLEDGE",
      "information": "Bandit hideout location",
      "significance": "MAJOR"
    }
  ]
}
```

### AI Prompt Engineering

The AI requires carefully designed prompts to guide its behavior:

**Base Prompt Template**

```
You are the AI Game Master for the game Wayfarer.

ROLE: You create narrative encounters with strategic choices that test the player's skills.

ENCOUNTER CONTEXT:
- Location: {{location}}
- Goal: {{goal}}
- Player Skills Available: {{available_skill_cards}}
- Player Focus Points: {{focus_points}}
- Active State Flags: {{active_flags}}

INSTRUCTIONS:
1. Generate a narrative beat description (2-3 sentences)
2. Create 3-4 distinct choices that advance toward the goal
3. For each choice:
   - Set Focus cost (0-2)
   - Define which skill cards can be used
   - Set appropriate difficulty (Easy=2, Standard=3, Hard=4, Exceptional=5)
   - Select appropriate success and failure payloads from the available library
   - Ensure narrative descriptions match mechanical effects

SPECIAL GUIDELINES:
- Always include one 0-Focus "recovery" option
- Prioritize choices that use the player's available skill cards
- Distribute difficulties according to: {{difficulty_distribution}}
- Select payloads of appropriate tier: {{payload_tier_guidance}}
- If the player has multiple paths to the goal, offer choices for each

FORMAT YOUR RESPONSE EXACTLY AS SHOWN IN THE EXAMPLE BELOW:
{{example_json_structure}}
```

**Difficulty Distribution Guidance**

```
For early encounter (under 50% progress):
- 60% Easy (SCD 2), 30% Standard (SCD 3), 10% Hard (SCD 4)
- Choose difficulty relative to player's skill levels
- No Exceptional (SCD 5) challenges unless narratively essential

For later encounter (over 50% progress):
- 30% Easy, 40% Standard, 25% Hard, 5% Exceptional
- Consider active flags when setting difficulties
- Climactic moments warrant Hard/Exceptional challenges
```

**Payload Selection Guidance**

```
For choices that:
- Directly advance main goal: Primary payloads from moderate tier
- Provide strategic advantage: Secondary advantages from minor tier
- Involve high risk/reward: Major tier payloads with significant impact
- Cost 0 Focus: Minor positive effects with standard negative risks

Balance positive/negative effects based on:
- Early encounter: Minor negatives, recoverable setbacks
- Mid encounter: Moderate negatives, meaningful consequences
- Late encounter: Major negatives for failures, greater rewards for success
```

### Validation and Fallbacks

To ensure AI responses meet system requirements:

**Validation Pipeline**
1. Check structure completeness (all required fields present)
2. Verify narrative consistency with context
3. Validate each choice:
   - Confirm payload IDs exist in library
   - Verify skill categories are valid
   - Check difficulty distribution against guidelines
   - Ensure narrative-mechanical consistency
4. Confirm 0-Focus recovery option exists

**Fallback System**
If AI response fails validation, the system generates default choices:
1. Standard narrative description based on context
2. Basic choice set with appropriate distribution
3. Standard recovery option

## V. Payload System

### Payload Categories

Payloads are predefined mechanical effects that the AI can select from:

**1. State Flag Payloads**
- SET_FLAG_[FLAG_NAME]
- CLEAR_FLAG_[FLAG_NAME]

**2. Resource Payloads**
- GAIN_FOCUS_[AMOUNT]
- LOSE_FOCUS_[AMOUNT]
- GAIN_CURRENCY_[TIER]
- LOSE_CURRENCY_[TIER]

**3. Mechanical Effect Payloads**
- BUFF_NEXT_CHECK_[AMOUNT]
- DEBUFF_NEXT_CHECK_[AMOUNT]
- ADVANCE_DURATION_[AMOUNT]

**4. Relationship Payloads**
- MODIFY_RELATIONSHIP_[TARGET]_[DIRECTION]_[MAGNITUDE]

**5. Item Payloads**
- ACQUIRE_ITEM_[RARITY]_[TYPE]
- LOSE_ITEM_[SPECIFICITY]

**6. Compound Payloads**
- EXECUTE_SEQUENCE_[PAYLOAD1]_[PAYLOAD2]
- CONDITIONAL_[CONDITION]_[TRUE_PAYLOAD]_[FALSE_PAYLOAD]

### Payload Tiers

Payloads are organized into tiers for appropriate scaling:

**Minor Tier**
- Small resource changes (±1-3 currency)
- Temporary advantages/disadvantages
- Minimal relationship impact

**Moderate Tier**
- Medium resource changes (±4-7 currency)
- Significant state flag changes
- Notable relationship impact

**Major Tier**
- Large resource changes (±8-10 currency)
- Critical state flag combinations
- Substantial relationship impact
- Rare item acquisition

### Implementation Architecture

The payload system is implemented through a flexible registry:

```csharp
// Core interface for all mechanical effects
public interface IMechanicalEffect
{
    void Apply(EncounterState state);
    string GetDescriptionForPlayer();
}

// Payload registry maps string IDs to concrete implementations
public class PayloadRegistry
{
    private Dictionary<string, IMechanicalEffect> registeredEffects;
    
    public void RegisterEffect(string effectID, IMechanicalEffect effect) {...}
    public IMechanicalEffect GetEffect(string effectID) {...}
}

// Sample implementation of a state flag effect
public class SetStateFlagEffect : IMechanicalEffect
{
    private string flagToSet;
    private string opposingFlag; // Will be cleared automatically
    
    public void Apply(EncounterState state)
    {
        state.SetFlag(flagToSet);
        if (!string.IsNullOrEmpty(opposingFlag))
            state.ClearFlag(opposingFlag);
    }
    
    public string GetDescriptionForPlayer()
    {
        return $"Gains {flagToSet.ToHumanReadable()}";
    }
}
```

## VI. Player Interface

### Choice Presentation

Choices must be presented with complete information for strategic decision-making:

**Hierarchical Choice Organization**
1. Primary narrative choices (3-4 per beat)
2. For each choice, skill options (1-3 per choice)

**Example UI Layout**
```
┌─ "Get past the guard" ──────────────────────────────┐
│  Focus Cost: 1                                      │
│                                                     │
│  [Etiquette Lvl 2] Standard Difficulty              │
│  Success: Guard grants access, gains respect         │
│  Failure: Guard becomes suspicious                  │
│                                                     │
│  [Threatening Lvl 3] Easy Difficulty                │
│  Success: Guard allows passage, becomes fearful     │
│  Failure: Guard becomes hostile, alerts others      │
└─────────────────────────────────────────────────────┘
```

**Selection Process**
1. Player selects narrative choice
2. Player selects which skill card to use (if multiple options)
3. System confirms selection, showing cost and card exhaustion
4. System resolves check, displaying formula and outcome
5. System applies payloads and updates state
6. AI narrates outcome

### State Visualization

Active Universal State Flags must be clearly communicated to the player:

**UI Components**
- Display 3-5 most significant flags as icons with distinctive designs
- Group by category (Positional, Relational, etc.)
- Use animation to highlight newly set/cleared flags
- Provide tooltips with clear explanations of each flag's meaning
- Show focus points remaining prominently

**Flag Prioritization**
- Goal-related flags receive highest priority
- Recently set flags get higher priority
- Flags with major mechanical impact are emphasized
- Maintain at least one flag from each relevant category

### Card Management

The UI must clearly show skill card status during encounters:

**Card Display Elements**
- Available cards shown with level and bonus value
- Exhausted cards visually distinct (grayed out, marked)
- Cards applicable to current choice highlighted
- Card bonuses incorporated into difficulty assessment
- Clear indication of which card will be exhausted upon selection

**Untrained Attempt Indicators**
- Visual warning when attempting without appropriate card
- Clear display of penalty (e.g., "+2 Difficulty, Base Level 0")
- Explicit explanation of increased risk

## VII. Implementation Guide

### Development Phases

Implement the system in four distinct phases:

**Phase 1: Core Mechanical Systems**
- Payload system implementation and testing
- Skill card system development
- Universal state flag system
- Focus point economy mechanics

**Phase 2: UI & Player Experience**
- Choice presentation system
- State visualization components
- Card management interface
- Skill check resolution display

**Phase 3: AI Integration**
- Prompt engineering and testing
- Response parsing and validation
- Fallback system implementation
- New element proposal handling

**Phase 4: Testing & Refinement**
- Encounter testing framework
- Balance parameter tuning
- Narrative coherence assessment
- Performance optimization

### Core Components

The system requires these essential components:

**EncounterState Class**
- Tracks all state during an encounter
- Manages active flags, Focus Points, duration counter
- Applies and validates payloads
- Checks goal achievement conditions

**SkillCard System**
- Defines all available skill cards with levels
- Manages card selection and exhaustion
- Calculates effective skill values for checks
- Handles untrained attempt logic

**PayloadRegistry**
- Maintains library of all available payloads
- Maps payload IDs to concrete implementations
- Validates AI payload selections
- Applies payload effects to encounter state

**AIResponseHandler**
- Sends prompts to AI with appropriate context
- Parses and validates AI responses
- Implements fallback generation if needed
- Translates AI choices into UI elements

### Testing Approach

Comprehensive testing should cover:

**Functionality Testing**
- Validate all payload types function correctly
- Verify skill checks resolve according to formula
- Confirm Focus economy operates as designed
- Test flag setting/clearing mechanics

**AI Response Testing**
- Evaluate prompt effectiveness across scenarios
- Test validation system with varied AI outputs
- Verify fallback systems maintain playability
- Assess narrative coherence across multiple beats

**Balance Testing**
- Measure success rates across difficulty levels
- Analyze Focus expenditure patterns
- Evaluate encounter length and pacing
- Test different skill card distributions

**Player Experience Testing**
- Assess UI clarity and information accessibility
- Measure decision-making time and confidence
- Evaluate narrative-mechanical integration
- Test accessibility for different player types

### Balance Tuning

The system includes parameters for balance adjustment:

**Tunable Parameters**
- Starting Focus Points (6-8)
- Maximum Duration Cap (8-10)
- Recovery Success Amount (1-2)
- Difficulty Distribution Percentages
- Payload Impact Values

**Data Collection**
- Track choice distribution by skill category
- Record success/failure rates by difficulty
- Measure average encounter length
- Analyze Focus economy flow
- Monitor flag activation patterns

## VIII. Reference Materials

### Skill Card Catalog

The core specialized skill cards include:

**Physical Skills**
- **Brute Force** (Strength): Breaking, pushing, lifting
- **Acrobatics** (Agility): Jumping, tumbling, balancing
- **Lockpicking** (Precision): Opening locks and mechanisms
- **Climbing** (Strength): Scaling walls and obstacles
- **Stealth** (Agility): Moving quietly, hiding
- **Resistance** (Endurance): Withstanding pain, toxins

**Intellectual Skills**
- **Investigation** (Analysis): Finding clues, connecting evidence
- **Perception** (Observation): Noticing details, spotting anomalies
- **Strategy** (Planning): Devising plans, anticipating outcomes
- **Knowledge** (Memory): Recalling information, identifying items
- **Tracking** (Observation): Following trails, reading signs
- **Medicine** (Analysis): Treating injuries, identifying ailments

**Social Skills**
- **Etiquette** (Charm): Social graces, proper behavior
- **Negotiation** (Persuasion): Bargaining, deal-making
- **Acting** (Deception): Presenting false personas
- **Threatening** (Intimidation): Applying pressure, coercion
- **Leadership** (Persuasion): Inspiring others, taking charge
- **Empathy** (Charm): Reading emotions, building trust

### Payload Library Examples

**State Flag Payloads**
- SET_FLAG_TRUST_ESTABLISHED
- SET_FLAG_ADVANTAGEOUS_POSITION
- SET_FLAG_SECRET_REVEALED
- CLEAR_FLAG_DISTRUST_TRIGGERED

**Resource Payloads**
- GAIN_FOCUS_1
- LOSE_FOCUS_1
- GAIN_CURRENCY_MINOR
- LOSE_CURRENCY_MODERATE

**Mechanical Effect Payloads**
- BUFF_NEXT_CHECK_1
- DEBUFF_NEXT_CHECK_1
- ADVANCE_DURATION_1
- ADVANCE_DURATION_2

**Relationship Payloads**
- MODIFY_RELATIONSHIP_POSITIVE_MINOR
- MODIFY_RELATIONSHIP_NEGATIVE_MODERATE
- MODIFY_RELATIONSHIP_POSITIVE_MAJOR

**Item Payloads**
- ACQUIRE_ITEM_COMMON_CONSUMABLE
- ACQUIRE_ITEM_UNCOMMON_TOOL
- LOSE_ITEM_GENERIC

### UEST Definitions

**Positional Flags**
- ADVANTAGEOUS_POSITION: Character has spatial/tactical advantage
- DISADVANTAGEOUS_POSITION: Character is at spatial/tactical disadvantage
- HIDDEN_POSITION: Character is concealed from others
- EXPOSED_POSITION: Character is unusually visible/vulnerable

**Relational Flags**
- TRUST_ESTABLISHED: Subject has developed trust toward character
- DISTRUST_TRIGGERED: Subject is suspicious of character
- RESPECT_EARNED: Subject acknowledges character's competence/status
- HOSTILITY_PROVOKED: Subject has become antagonistic

**Informational Flags**
- INSIGHT_GAINED: Character has obtained useful understanding
- SECRET_REVEALED: Hidden information has been disclosed
- DECEPTION_DETECTED: Character has recognized falsehood
- CONFUSION_CREATED: Uncertainty has been introduced

**Tactical Flags**
- SURPRISE_ACHIEVED: Character has gained element of surprise
- PREPARATION_COMPLETED: Character is ready for upcoming challenge
- PATH_CLEARED: Obstacle to progress has been removed
- RESOURCE_SECURED: Valuable asset has been obtained

### Prompt Templates

**Initial Prompt Template**
```
You are the AI Game Master for the game Wayfarer.

ROLE: You create narrative encounters with strategic choices that test the player's skills.

ENCOUNTER CONTEXT:
- Location: {{location}}
- Goal: {{goal}}
- Player Skills Available: {{available_skill_cards}}
- Player Focus Points: {{focus_points}}
- Active State Flags: {{active_flags}}

INSTRUCTIONS:
1. Generate a narrative beat description (2-3 sentences)
2. Create 3-4 distinct choices that advance toward the goal
3. For each choice:
   - Set Focus cost (0-2)
   - Define which skill cards can be used
   - Set appropriate difficulty (Easy=2, Standard=3, Hard=4, Exceptional=5)
   - Select appropriate success and failure payloads from the available library
   - Ensure narrative descriptions match mechanical effects

SPECIAL GUIDELINES:
- Always include one 0-Focus "recovery" option
- Prioritize choices that use the player's available skill cards
- Distribute difficulties according to: {{difficulty_distribution}}
- Select payloads of appropriate tier: {{payload_tier_guidance}}
- If the player has multiple paths to the goal, offer choices for each

FORMAT YOUR RESPONSE EXACTLY AS SHOWN IN THE EXAMPLE BELOW:
{{example_json_structure}}
```

**Outcome Narration Prompt**
```
Based on the player's choice and the outcome of the skill check:

CHOICE CONTEXT:
- Player selected: "{{choice_narrative_text}}"
- Skill used: {{skill_name}} (Level {{skill_level}})
- Difficulty: {{difficulty}} (SCD {{scd}})
- Result: {{success_or_failure}}
- Active Flags: {{active_flags}}

TASK:
Narrate the outcome of this action in 2-3 sentences. The narration should:
1. Reflect the success or failure of the skill check
2. Incorporate relevant active flags into the description
3. Set up the next beat of the encounter
4. For success, describe: {{success_payload_narrative}}
5. For failure, describe: {{failure_payload_narrative}}

Your narration should maintain the tone and context of the encounter while highlighting the consequences of the player's choice.
```

**Conclusion Prompt**
```
The encounter is now concluding:

CONCLUSION CONTEXT:
- Encounter goal: {{encounter_goal}}
- Active flags: {{active_flags}}
- Overall outcome: {{success_or_failure}}
- Key events: {{significant_choices_summary}}

TASK:
Generate a conclusion for this encounter that:
1. Summarizes the outcome and its significance (2-3 sentences)
2. Describes any lasting impacts on characters or the environment (1-2 sentences)
3. Provides narrative closure while potentially hinting at future developments
4. For success, emphasizes achievement and rewards
5. For failure, explains consequences while maintaining forward momentum

Also provide a structured list of proposed persistent changes to the game world:
1. Relationship changes with specific characters
2. Knowledge or information gained
3. Items acquired or lost
4. New opportunities or challenges created

FORMAT YOUR RESPONSE EXACTLY AS SHOWN IN THE EXAMPLE BELOW:
{{example_conclusion_format}}
```

## IX. Conclusion

The Wayfarer's Resolve AI Game Master System represents a significant innovation in encounter design. By combining bounded AI narrative control with deterministic mechanical systems, we create encounters that feel dynamic and responsive while maintaining strategic depth and player agency.

The system successfully addresses the challenges of traditional encounter design through:

1. **Integrated Narrative-Mechanical Experience**: AI-driven narration and choice generation directly connects to mechanical systems through the payload and UEST frameworks.

2. **Adaptive Challenge Design**: The AI tailors challenges to player capabilities while maintaining appropriate difficulty progression.

3. **Emergent Strategic Depth**: The combination of Focus economy, Skill Card management, and AI-driven challenges creates rich strategic options.

4. **Development Efficiency**: Once implemented, the system generates diverse, contextually appropriate encounters without requiring extensive hand-crafting.

This framework provides a comprehensive blueprint for implementation while allowing flexibility for refinement through testing and iteration. The resulting player experience will be both strategically engaging and narratively immersive, setting a new standard for encounter design in RPGs.