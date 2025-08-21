# Wayfarer Content Contracts - Mechanical JSON Specification

## Core Design Principles

This document defines the complete mechanical contracts for Wayfarer's content system. Every element is purely mechanical with narrative tags for AI interpretation. The game mechanics are deterministic and complete without any narrative content. The AI translates mechanical relationships into contextual story.

### Fundamental Rules
1. **No narrative in mechanics** - Mechanics define what happens, tags guide how it's described
2. **Complete determinism** - Every mechanical outcome is fully specified
3. **Strict typing** - Each element has exactly one type and purpose
4. **ID-based relationships** - All connections use IDs, never content
5. **Mechanical tags** - Semantic markers for AI narrative generation

## 1. Emotional States Definition

Emotional states are complete rulesets that define how conversations function. Each state specifies exact mechanical rules for Listen and Speak actions.

## 2. NPC Mechanical Definition

NPCs are mechanical entities defined by their deck archetype, relationship network, and token preferences.

## 3. Card Mechanical Definition

Cards are purely mechanical with clear type separation and deterministic effects.

## 4. Observation Mechanical Definition

Observations are information packets that convert to opportunity cards in conversations.

## 5. Letter Contract Definition

Letters are mechanical contracts between NPCs that create obligations and modify relationships.

## 6. Location Mechanical Definition

Locations are interaction spaces with available NPCs and observations.

## 7. Route Encounter Definition

Routes generate encounters based on familiarity level.

## 8. Campaign Configuration

Defines starting conditions and victory parameters.

## Implementation Architecture

### Content Loading Order
1. Load emotional states (defines all conversation rules)
2. Load card templates (defines all possible cards)
3. Load NPCs (references cards and states)
4. Load observations (references NPCs for relevance)
5. Load locations (references NPCs and observations)
6. Load route encounters (references requirements)
7. Load campaign (references everything)

### Deck Generation Process
1. NPC personality archetype determines base distribution
2. Token preferences weight specific card types
3. Relationship states add specific cards
4. Letter deliveries permanently modify deck
5. Failed obligations add burden cards

### Conversation Flow
1. Meeting obligation deadline determines starting emotional state
2. Starting hand draws from deck plus observation cards
3. Each turn follows state-specific Listen/Speak rules
4. State transitions occur through listening or state cards
5. Comfort accumulation triggers letter generation at thresholds
6. Conversation ends when patience depleted or crisis triggered

### AI Narrative Generation

The AI receives mechanical events with narrative tags and generates appropriate descriptions:

The AI generates contextual narrative without affecting mechanics:
- "Elena clutches the sealed letter with trembling hands..."
- "The merchant's eyes dart nervously to the door..."
- "Your old friend grips the parchment desperately..."

### Mechanical Integrity Rules

1. **State Changes**: Only occur through explicit mechanics (listening or state cards)
2. **Card Effects**: Execute exactly as specified, no interpretation
3. **Weight Limits**: Absolute constraints based on emotional state
4. **Opportunity Vanishing**: ALL vanish on Listen (except special states)
5. **Token Effects**: Apply mathematically without rounding
6. **Deadline Tracking**: Precise minute-based calculation
7. **Success Calculation**: Deterministic formula with no hidden modifiers

### Content Validation

Every JSON element must pass validation:
- All IDs are unique within their type
- All referenced IDs exist in appropriate collections
- All numerical values are integers (no floats)
- All state transitions reference valid states
- All card effects are mechanically complete
- No narrative content in mechanical definitions

## Design Rationale

### Why Separate Mechanics from Narrative

The mechanical system is completely deterministic and can run without any narrative layer. This separation ensures:
- Mechanics can be tested independently
- Narrative can be regenerated for different settings
- No ambiguity in rule interpretation
- AI can freely generate story without breaking mechanics

### Why Emotional States as Rulesets

Rather than modifiers, each emotional state is a complete ruleset because:
- Clear binary rules instead of percentage modifications
- Each state creates genuinely different gameplay
- No mental math or modifier stacking
- Authentic representation of how emotions change interactions

### Why Card Type Separation

Strict separation between Comfort, State, and Crisis cards ensures:
- No ambiguity about what can combine
- Clear strategic choices between building comfort or changing state
- Crisis situations feel mechanically distinct
- No edge cases or conflicts in card interactions

### Why Observation Integration

Observations converting to Opportunity cards creates:
- Reward for world exploration
- Natural integration between location and conversation gameplay
- Time pressure through Opportunity vanishing
- Mechanical representation of bringing outside knowledge into conversations

### Why Meeting Obligations Determine State

NPC emotional states deriving from player punctuality means:
- Single source of truth for time pressure
- Natural emergence of crisis situations
- Player agency in managing NPC states
- No need for complex NPC state tracking

This system creates a mechanically complete game where narrative emerges from the interaction of simple, deterministic rules. Every story is unique, but every mechanical outcome is predictable and fair.