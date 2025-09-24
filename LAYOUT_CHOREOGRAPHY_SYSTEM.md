# Layout Choreography System - Implementation Plan

## Overview
The Layout Choreography System transforms card animations from individual effects into coordinated "dance sequences" that help players visually track changes in their hand. This system ensures predictable, trackable, and visually coherent card movements.

## Core Principles

### 1. Stack Choreography
Cards behave like a **physical stack** with animated transitions:
- **Stack discipline**: Strict order maintained, new cards added to bottom, removals cause coordinated stack collapse
- **Coordinated movement**: When stack changes, ALL affected cards move in harmony
- **Visual storytelling**: Every transition tells a clear story of cause and effect

### 2. Two-Phase Pattern
Every card action follows the same pattern:
- **Planning Phase** (Instant): Game state changes immediately, choreography calculated
- **Execution Phase** (Animated): Coordinated visual sequence plays out

## Architecture Components

### Choreography Engine
**Location**: `src/Services/Conversation/LayoutChoreographyEngine.cs`

**Responsibilities**:
- **Movement Planning**: Calculate all position changes before animation starts
- **Timing Coordination**: Manage staggered starts and overlapping sequences
- **Spatial Management**: Ensure no conflicts between simultaneous movements
- **Recovery Handling**: Manage interruptions and edge cases

```csharp
public class LayoutChoreographyEngine
{
    // Core planning methods
    public ChoreographySequence PlanListenChoreography(List<CardInstance> newCards)
    public ChoreographySequence PlanSpeakChoreography(CardInstance playedCard, List<CardInstance> effectCards)

    // Execution methods
    public async Task ExecuteChoreography(ChoreographySequence sequence)
}
```

### Slot-Based Positioning
**Location**: `src/Services/Conversation/CardSlotManager.cs`

**Responsibilities**:
- **Virtual Slots**: Each card position has stable "address"
- **Slot Transitions**: Cards smoothly move between addresses
- **Stack Expansion**: New slots appear at bottom as needed
- **Stack Contraction**: Empty slots collapse with coordinated movement

```csharp
public class CardSlotManager
{
    // Slot management
    public List<CardSlot> CalculateSlots(List<CardInstance> cards)
    public SlotTransition CalculateTransitions(List<CardSlot> before, List<CardSlot> after)
}
```

### Movement Models
**Location**: `src/Models/Choreography/`

```csharp
public class ChoreographySequence
{
    public List<CardMovement> Movements { get; set; }
    public ChoreographyType Type { get; set; }
    public int TotalDuration { get; set; }
}

public class CardMovement
{
    public CardInstance Card { get; set; }
    public Vector2 FromPosition { get; set; }
    public Vector2 ToPosition { get; set; }
    public int StartDelay { get; set; }
    public int Duration { get; set; }
    public MovementType Type { get; set; } // Enter, Exit, Reposition
}
```

## Choreography Types

### LISTEN Choreography: "New Cards Joining the Stack"
**Pattern**: EXPANSION FROM BOTTOM

**Sequence**:
1. **Immediate**: Cards added to game state
2. **Visual**: New card slots appear at bottom
3. **Animation**: Cards slide in from "deck area" to slots, staggered timing
4. **Result**: Existing cards stable, new cards smoothly join stack

**Timing**:
```
Card 1: Delay 0ms,    Duration 500ms
Card 2: Delay 150ms,  Duration 500ms
Card 3: Delay 300ms,  Duration 500ms
Total: 800ms (300ms + 500ms)
```

### SPEAK Choreography: "Stack Collapse and Rebuild"
**Pattern**: COORDINATED COLLAPSE + REBUILD

**Sequence**:
1. **Phase 1** (0-1250ms): Played card exit animation
2. **Phase 2** (800ms-1100ms): Cards below slide up to fill gap (overlapping)
3. **Phase 3** (1200ms+): New effect cards slide in at bottom (if any)

**Timing**:
```
Played Card Exit:    0ms-1250ms (success/failure flash + exit)
Stack Collapse:      800ms-1100ms (overlaps with card exit)
New Cards Enter:     1200ms+ (staggered, 150ms apart)
```

## User Experience Design

### Visual Tracking Support
- **Clear cause and effect**: User action produces predictable movement pattern
- **Movement telegraphing**: Slight preparation before major changes
- **Coherent motion**: All movements feel physically plausible
- **Attention guidance**: Most important movement (played card) happens prominently

### Cognitive Load Management
- **Familiar patterns**: Same action types always animate the same way
- **Reasonable complexity**: Never more movements than user can track simultaneously
- **Recovery time**: Brief pause between complex sequences
- **Input protection**: Block new actions during active choreography

## CSS Animation Classes

### Core Movement Classes
```css
.card-slot {
  position: relative;
  transition: transform 0.3s ease-out;
}

.card.choreography-enter {
  animation: card-enter-from-bottom 0.5s ease-out;
}

.card.choreography-exit {
  animation: card-exit-with-flash 1.25s ease-out;
}

.card.choreography-reposition {
  transition: transform 0.3s ease-out;
}
```

### Staggered Animations
```css
.card.stagger-1 { animation-delay: 0.0s; }
.card.stagger-2 { animation-delay: 0.15s; }
.card.stagger-3 { animation-delay: 0.3s; }
```

## Integration Points

### With UIAnimationOrchestrator
- **Extend orchestrator** to use LayoutChoreographyEngine
- **Replace simple timing** with choreographed sequences
- **Maintain input blocking** during choreography execution

### With CardDisplayManager
- **Enhance display logic** to use slot-based positioning
- **Add choreography state** to CardDisplayInfo
- **Coordinate with movement calculations**

### With ConversationContent
- **Minimal changes** to existing LISTEN/SPEAK methods
- **Add choreography triggers** after game state changes
- **Subscribe to choreography completion** for UI updates

## Implementation Phases

### Phase 1: Core Engine
1. Create LayoutChoreographyEngine service
2. Implement basic movement planning
3. Create choreography models and types

### Phase 2: Slot System
1. Create CardSlotManager
2. Implement slot calculation logic
3. Add slot transition math

### Phase 3: Movement Coordination
1. Build movement sequencing
2. Add timing coordination
3. Implement staggered execution

### Phase 4: Choreography Sequences
1. Implement LISTEN choreography
2. Implement SPEAK choreography
3. Add CSS animation classes

### Phase 5: Integration
1. Update UIAnimationOrchestrator
2. Enhance CardDisplayManager
3. Integrate with ConversationContent

### Phase 6: Testing & Polish
1. Test all choreography sequences
2. Tune timing and easing
3. Add error handling and edge cases

## Success Criteria
- ✅ **No layout jumps**: Cards never teleport or leave empty gaps
- ✅ **Trackable movements**: Users can follow every card's journey
- ✅ **Predictable patterns**: Same actions always animate consistently
- ✅ **Professional polish**: Smooth, coordinated movements like premium card games
- ✅ **Performance**: Choreography doesn't impact game responsiveness
- ✅ **Accessibility**: Can be disabled for users who prefer reduced motion

## Technical Notes
- **FLIP Technique**: Use First, Last, Invert, Play pattern for smooth transitions
- **CSS Transform-based**: Avoid layout thrashing by using transforms for positioning
- **Coordinate System**: Establish stable reference frame for position calculations
- **Memory Management**: Clean up choreography data after sequences complete
- **Fallback Handling**: Graceful degradation when animations are disabled or interrupted