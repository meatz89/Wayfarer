# Session Handoff - Unified Action System Design

## Latest Update: Tier System for Conversation Choices (2025-08-10)

### What Was Done
Added tier checking to the ConversationChoiceGenerator system, matching the pattern already implemented in ActionGenerator. Conversation choices are now gated by the player's tier level (T1: Stranger, T2: Associate, T3: Confidant).

### Files Modified
- `/src/Game/ConversationSystem/VerbOrganizedChoiceGenerator.cs` - Added tier checking for all conversation verbs

### Tier Requirements Implemented

#### HELP Verb
- **T1 (Always Available)**: Accept letters, basic trust building
- **T2 (Associate)**: Accept urgent letters, deeper commitments (+4 trust)
- **T3 (Confidant)**: Deep bonds (+6 trust, also requires 5 existing trust)

#### NEGOTIATE Verb  
- **T1 (Always Available)**: Simple swaps, queue interface
- **T2 (Associate)**: Refuse letters, prioritize to position 1
- **T3 (Confidant)**: Major token trades (5 Commerce → 3 Status)

#### INVESTIGATE Verb
- **T2 (Associate)**: ALL basic investigation (schedules, letters, networks)
- **T3 (Confidant)**: Expose conspiracies (also requires 3 trust)

### Narrative Design Preserved
- Lock messages feel like social barriers: "I'm not familiar enough..." instead of "Level too low"
- Maintains the wayfarer fantasy - a stranger gradually earning trust and standing
- Dual requirements (tier + tokens) create layered progression

### Test File Created
- `/src/test-tier-conversation-choices.cs` - Documents and validates the tier system design

### Known Issues
- Multiple pre-existing compilation errors unrelated to tier system
- These appear to be from incomplete refactoring of other systems

### Next Steps
1. Fix compilation errors in EmergentMechanicalEffects.cs and other files
2. Test the tier system in-game with Playwright
3. Verify that tier progression feels natural during play
4. Consider adding visual indicators for locked choices in the UI

---

## Previous Session Summary (Date: Earlier)

### Context
The user requested implementation of a unified action system where location actions and NPC conversation actions share the same attention resource pool. The system must use binary availability (actions are either available or not) based on tags and state, with a tier progression system (T1-T3) for letters and routes.

### Key Constraints
- NO HashSet, Dictionary, or untyped data structures
- NO Func<> or delegate types  
- Must REFACTOR existing code, not create new
- Binary availability only (no variable costs)
- AI will generate narrative content on-the-fly

### What Was Discussed

#### 1. Initial Proposal
User proposed fully unified action system where location and conversation actions appear in same list, sharing attention pool, with tier-based progression.

#### 2. Agent Analysis

**Chen (Game Design)**: 
- Strongly opposed full unification, calling it "menu soup"
- Warned it would destroy the tension between exploration and social navigation
- Supported tier system for progression but emphasized it must feel earned, not gated
- Advocated for keeping contexts separate but making both meaningful

**Kai (Systems Architect)**:
- Designed technical implementation using strongly-typed structures
- Proposed binary availability checker without HashSet/Dictionary
- Created TierAccessList and ActionAvailability structures
- Could implement either unified or separate technically

**Jordan (Narrative Designer)**:
- Initially supported unification with strong narrative framing
- Proposed attention as "emotional bandwidth" metaphor
- Suggested tier names reflect social standing (Stranger→Acquaintance→Associate→Confidant)
- Ultimately agreed narrative goals achievable without mechanical unification

**Alex (Content Production)**:
- Calculated 39 hours for enhanced separation vs 126 hours for full unification
- Warned against throwing away 687 lines of working code
- Showed content explosion risk with unified system (210 tier variations)
- Strongly recommended keeping systems separate

**Priya (UI/UX Designer)**:
- Identified "categorical confusion" problem with mixed action lists
- Warned of cognitive overload and context pollution
- Noted existing UnifiedChoice.razor already provides visual consistency
- Recommended maintaining separation with shared resources

### Final Decision: Enhanced Separation with Shared Resources

**Consensus**: Keep ActionGenerator and ConversationChoiceGenerator separate, but share attention pool and add tier system.

### Implementation Plan

#### Phase 1: Link Attention Pools (2 hours)
```csharp
// Both systems use same TimeBlockAttentionManager
var attention = _timeBlockAttention.GetAttentionState();
conversationChoices.FilterByAttention(attention);
locationActions.FilterByAttention(attention);
```

#### Phase 2: Add Tier System (4 hours)
```csharp
public class TierAccessList
{
    private readonly bool[] _tiers = new bool[3]; // T1, T2, T3
    public void SetAccess(int tier, bool hasAccess) => _tiers[tier-1] = hasAccess;
    public bool HasAccess(int tier) => tier > 0 && tier <= 3 && _tiers[tier-1];
}
```

#### Phase 3: Binary Availability (2 hours)
```csharp
public class ActionAvailability 
{
    public string ActionId { get; set; }
    public bool IsAvailable { get; set; }
    public string RequiredTag { get; set; }
    public int RequiredTier { get; set; }
    public string UnavailableReason { get; set; }
}
```

#### Phase 4: Content Creation (31 hours)
- Tier-appropriate descriptions for actions
- Testing all combinations
- AI prompt templates for dynamic generation

### What Exists in Codebase

#### Working Systems to Keep:
- `ActionGenerator.cs` (377 lines) - Generates location actions
- `ConversationChoiceGenerator.cs` (310 lines) - Generates NPC choices  
- `VerbOrganizedChoiceGenerator.cs` - HELP/NEGOTIATE/INVESTIGATE verbs
- `TimeBlockAttentionManager.cs` - Already manages attention per time block
- `UnifiedChoice.razor` - UI component for consistent display
- `LocationTags.cs` - Tag-based action enabling (needs HashSet removal)

#### Systems Needing Refactor:
- Remove HashSet usage in LocationTags.cs (use List<string>)
- Add tier checks to both generators
- Create binary availability checker
- Link attention pools between systems

### Key Design Principles Established

1. **Separation Creates Focus**: Location exploration and NPC conversations are distinct mental modes
2. **Shared Resources, Not Shared Lists**: Same attention pool, different contexts
3. **Binary Availability**: Actions either available or not, no variable costs
4. **Tier Progression**: T1-T3 represents social standing and trust, not arbitrary gates
5. **AI Generation**: Reduces content burden from 126 to 39 hours

### Risks and Mitigations

**Risk**: Players confused by attention spanning both systems
**Mitigation**: Clear UI indicators showing attention is universal resource

**Risk**: Tier system feels like arbitrary gating
**Mitigation**: Frame as social standing (Stranger/Associate/Confidant) not numbers

**Risk**: Binary availability too restrictive
**Mitigation**: Always ensure 3-5 meaningful choices available

### Next Steps

1. Refactor LocationTags.cs to remove HashSet usage
2. Add tier checking to ActionGenerator
3. Link attention pools in both generators
4. Test binary availability with actual gameplay
5. Create AI prompt templates for tier-based narrative generation

### Total Estimated Time: 39 hours
- Technical implementation: 8 hours
- Content creation: 31 hours
- Risk: LOW (using existing, working systems)

### Session Notes
- User emphasized tier systems are required, not optional
- AI generation will handle narrative variety, reducing content burden
- Must refactor existing code rather than creating new systems
- Strong consensus against full unification despite initial proposal

---
*End of Session Handoff*