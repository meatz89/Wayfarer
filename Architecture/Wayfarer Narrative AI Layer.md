# Wayfarer Narrative AI Layer: Design Document

## Overview

The Narrative AI Layer in Wayfarer serves as the interpretive bridge between the game's mechanical systems and the player experience. This layer transforms abstract mechanics into contextually appropriate narrative text without altering the underlying rules or decision-making processes of the game.

## Core Purpose

The Narrative AI Layer exists to create narrative coherence and immersion by translating mechanical elements (tags, choices, effects) into natural language that reflects the current encounter context. It does not make mechanical decisions but presents those decisions in narratively engaging ways.

## System Integration

The Narrative AI Layer receives information from multiple game systems:

- **Base Tag System**: Current values of all approach and focus tags
- **Encounter Tag System**: Active strategic and narrative tags
- **Choice System**: The 4 available choices with their mechanical effects
- **Location System**: Context about the current location
- **Encounter History**: Previous narrative developments and choices made

## Core Responsibilities

### 1. Generate Choice Descriptions

When presented with the 4 mechanically-defined choices for a turn, the Narrative AI Layer:

- Creates contextual narrative descriptions for each choice
- Adapts the text to match the appropriate encounter mode (Social, Intellectual, or Physical)
- Ensures descriptions reflect the mechanical effects
- Incorporates active strategic and narrative tags into the descriptions
- Maintains continuity with previous narrative developments
- Differentiates momentum choices from pressure choices narratively

### 2. Create Encounter Reaction Narratives

After a player selects a choice, the Narrative AI Layer:

- Generates a narrative description of the outcome
- Reflects the mechanical changes in narrative form
- Acknowledges newly activated or deactivated tags within the narrative
- Creates a smooth narrative transition to the next set of choices
- Maintains continuity and builds upon the existing narrative foundation

## Encounter Presentation Modes

The Narrative AI Layer adapts its output to three distinct presentation modes while maintaining the same underlying mechanics:

### Social Encounters - Direct Speech

All choices and outcomes are presented as dialogue between characters.

Example: A "Friendly Inquiry" choice (Charm+Information, Momentum) would appear as:
*"Good evening! I'm new to these parts. What can you tell me about the road conditions ahead?"*

### Intellectual Encounters - Internal Monologue/Observations

All choices and outcomes are presented as the character's thoughts and analytical observations.

Example: A "Process Evidence" choice (Wit+Information, Momentum) would appear as:
*I need to connect these disparate facts. The ledger entries from winter don't match the storehouse inventory...*

### Physical Encounters - Action Descriptions

All choices and outcomes are presented as physical movements and environmental interactions.

Example: A "Silent Movement" choice (Stealth+Physical, Momentum) would appear as:
*You carefully place each foot between fallen leaves, testing the ground before shifting your weight to avoid making any sound.*

## Processing Flow

1. **Input**: The game system determines the encounter state, active tags, and available choices
2. **Choice Description Generation**: The Narrative AI creates appropriate descriptions for each choice
3. **Player Selection**: The player selects a choice
4. **Mechanical Update**: The game system updates all mechanical elements (tags, momentum, pressure)
5. **Reaction Narrative Generation**: The Narrative AI creates a reaction narrative describing what happened
6. **Next Choices**: The game system determines the next set of available choices
7. **Repeat**: The Narrative AI creates new descriptions for these choices, and the cycle continues

## Mechanical Relationships

### Choice Components and Narrative Representation

Each choice in Wayfarer contains:
- One approach tag (automatically +1 when chosen)
- One focus tag (automatically +1 when chosen)
- Additional encounter state tag modifications (can be one or two tags, positive or negative)
- Effect type: Momentum (base +1) or Pressure (base +1)
- Location difficulty pressure increase (varies based on location hostility)

The Narrative AI must represent all these elements contextually in its descriptions.

### Strategic and Narrative Tags

- **Strategic tags**: Always active when their conditions are met. The Narrative AI incorporates their effects into descriptions without explicitly mentioning mechanical effects.
- **Narrative tags**: Deactivate choices of specific approach or focus. The Narrative AI must explain these restrictions narratively when they become active.

## What the Narrative AI Layer Does NOT Do

The Narrative AI Layer explicitly does NOT:
- Make mechanical decisions about which choices are available
- Alter the effects of choices
- Change the activation conditions of tags
- Determine encounter difficulty or success criteria
- Handle the card selection algorithm

## Example Narrative Flow

### Initial State
- **Location**: Village Market
- **Tags**: Rapport 1, Analysis 1, Resource 0
- **Active Strategic Tags**: None yet
- **Available Choices**: 4 mechanically defined options

### Narrative AI Layer Processing

1. **Generate Choice Descriptions**:
   - Transforms "Negotiate Terms" (Charm+Resource, Momentum) into: *"Perhaps we could come to an arrangement that benefits us both? I have some goods that might interest you."*
   - Adapts all 4 choices to the social context of a village market

2. **Player Selects "Negotiate Terms"**

3. **System Updates**:
   - Rapport +1 (now 2)
   - Analysis +1 (now 2)
   - Momentum +1
   - New Strategic Tag activated: "Merchant's Respect"

4. **Generate Reaction Narrative**:
   - Creates a narrative that shows the merchant warming to the player
   - Subtly indicates the activation of "Merchant's Respect" without explicitly mentioning the tag
   - Transitions to the next set of choices

5. **Generate New Choice Descriptions**:
   - Creates 4 new descriptions that build upon this new relationship
   - Incorporates the effect of "Merchant's Respect" into relevant choices

## Implementation Considerations

The Narrative AI Layer requires:
- Access to complete encounter state information
- History tracking of previous narrative developments
- Context sensitivity to location and encounter type
- Ability to represent mechanical changes narratively
- Consistent voice and tone appropriate to the game world
