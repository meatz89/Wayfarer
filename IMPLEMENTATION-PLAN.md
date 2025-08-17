# Wayfarer: Complete Implementation Plan
*Generated from comprehensive specialized agent analysis*

## Executive Summary

This document provides the complete implementation roadmap for Wayfarer's dual-layer medieval letter carrier simulation. Based on analysis from specialized agents (Game Design, Systems Architecture, UI/UX, and Narrative Design), this plan addresses 88 user stories across 15 epics while preserving the human heart of relationship-driven storytelling.

**Critical Finding**: Current implementation has sophisticated backend mechanics but suffers from "spreadsheet simulator" syndrome. The plan prioritizes narrative-first integration where mechanics serve story, not the reverse.

## Current State Assessment

### ✅ COMPLETED SYSTEMS
- **GameWorld**: Basic structure with time/day tracking
- **LetterQueue**: 8-slot, 12-weight system with displacement mechanics
- **Basic Conversation System**: NPCs, cards, and conversation manager
- **Token Framework**: Four types (Trust, Commerce, Status, Shadow) with storage
- **Letter Properties**: Comprehensive letter system with special types
- **Basic UI Components**: ConversationScreen, LetterQueueScreen, LocationScreen

### 🚨 CRITICAL GAPS IDENTIFIED
1. **Missing Two-Layer Integration**: Conversations don't generate letters properly
2. **Broken Token Effects**: No mechanical differences between token types
3. **Invisible Queue Management**: Queue not visible during conversations (blocks core gameplay)
4. **Missing Emotional State Bridge**: Deadlines don't affect conversation mechanics
5. **✅ Standing Obligations Complete**: Core mechanic fully implemented with betrayal recovery system
6. **Narrative Translation Missing**: UI shows mechanical optimization instead of human relationships

## Implementation Strategy: Narrative-First Systems Integration

### Core Principle
> **"Hide all mathematics behind human truth"** - Every mechanical system must feel like authentic medieval relationship management, not spreadsheet optimization.

### Three-Layer Approach
1. **Mechanical Layer**: Sophisticated systems for emergent gameplay
2. **Translation Layer**: Convert mechanical effects to narrative meaning
3. **Presentation Layer**: Medieval UI that reinforces the wayfarer fantasy

---

# PHASE 1: FOUNDATION STABILIZATION
*Week 1 - Critical Architecture Fixes*

## Objective
Complete mechanical integration and surface hidden systems to achieve basic functional completeness.

### P1.1: Critical UI Integration (Day 1-2)
**Priority: BLOCKING** - Core mechanic must be visible

#### Tasks:
1. **✅ Queue Visibility During Conversations** (COMPLETED)
   - **REFACTORED**: Navigation system to allow queue access from conversations
   - **SOLUTION**: Click queue in BottomStatusBar → opens full LetterQueueScreen → "Back" returns to conversation
   - **IMPLEMENTATION**: Added ReturnView parameter to LetterQueueScreen, tracks PreviousView in MainGameplayViewBase
   - **Acceptance Criteria**: ✅ Players can view full queue state while in conversations and return seamlessly

2. **✅ Complete Token Display System** (COMPLETED)
   - **REFACTORED**: Changed `ShowOnlyRelevant="false"` in ConversationScreen.razor
   - **EXISTING SYSTEM**: TokenDisplay component already supported all 4 types (Trust/Commerce/Status/Shadow)
   - **ISSUE FOUND**: Token filtering was hiding Commerce/Shadow tokens during conversations
   - **SOLUTION**: Disabled filtering to show complete relationship state
   - **Acceptance Criteria**: ✅ Players see all 4 token types with narrative descriptions during conversations

3. **✅ Attention Integration Completion** (ALREADY COMPLETE)
   - **EXISTING SYSTEM**: TimeBlockAttentionManager fully integrated into conversation flow
   - **VERIFIED**: GameFacade provides attention via `GetCurrentAttentionState()`
   - **VERIFIED**: ConversationManager processes attention costs via `TrySpend()`
   - **VERIFIED**: UnifiedAttentionBar displays attention during conversations
   - **VERIFIED**: Conversation choices show attention costs with badge display
   - **Acceptance Criteria**: ✅ Attention system completely functional and integrated

4. **✅ Test Build and E2E Verification** (COMPLETED)
   - **BUILD STATUS**: ✅ Clean build with 0 warnings, 0 errors  
   - **E2E VERIFICATION**: ✅ Game loads successfully, queue visible in status bar
   - **FUNCTIONALITY VERIFIED**: Queue visibility, attention costs, navigation working
   - **ACCEPTANCE CRITERIA**: ✅ P1.1 changes verified and functional

---

## ✅ PHASE 1.1 COMPLETE: Critical UI Integration
**All P1.1 tasks completed successfully through refactoring existing systems rather than writing new code.**

---

## ✅ PHASE 1.2 COMPLETE: Card System Enhancement  
**NPC-specific starting decks and personality mapping implemented successfully.**

### P1.2 Implementation Results:

#### P1.2.1: ✅ NPC-Specific Starting Decks (COMPLETED)
- **CREATED**: PersonalityType enum with 5 categorical types (DEVOTED, MERCANTILE, PROUD, CUNNING, STEADFAST)
- **CREATED**: PersonalityMappingService to bridge authentic JSON descriptions to categorical types
- **ENHANCED**: NPC class with PersonalityDescription (authentic) and PersonalityType (categorical) fields
- **UPDATED**: NPCParser to parse and map personality information during NPC creation
- **REFACTORED**: NPCDeck constructor to accept PersonalityType parameter
- **IMPLEMENTED**: GetPersonalityCards() method with 15 personality-specific conversation cards

#### P1.2.2: ✅ Personality-Specific Card Design (COMPLETED)
- **DEVOTED Cards**: "Offer Comfort", "Listen Deeply", "Share Personal Story" - emphasize emotional connection
- **MERCANTILE Cards**: "Discuss Business", "Negotiate Terms", "Assess Worth" - focus on practical matters  
- **PROUD Cards**: "Show Respect", "Acknowledge Status", "Formal Address" - respect hierarchy
- **CUNNING Cards**: "Read Between Lines", "Share Information", "Test Loyalty" - deal with secrets
- **STEADFAST Cards**: "Show Honor", "Speak Plainly", "Respect Duty" - value directness and duty

#### Technical Implementation:
- **PRESERVED**: Authentic personality descriptions from JSON (e.g., "Intelligent and desperate")
- **MAPPED**: Categorical types for mechanical differentiation without losing human truth
- **MAINTAINED**: UI/UX principles - no visual distinction needed, personality emerges through card content
- **ACHIEVED**: Narrative design goals - NPCs feel genuinely different through natural conversation patterns

#### Mapping Validation:
- Elena: "Intelligent and desperate" → DEVOTED (emotional investment)
- Marcus: "Businesslike and hurried" → MERCANTILE (trade focus)  
- Lord Aldwin: "Formal and insistent" → PROUD (status conscious)
- Garrett: "Mysterious and observant" → CUNNING (information focus)
- Viktor: "Stern and dutiful" → STEADFAST (duty-bound)
- Bertram: "Friendly and observant" → STEADFAST (reliable baseline)

**ARCHITECTURE**: Clean separation between authentic content (JSON) and mechanical categorization (enum) enables both human truth and gameplay differentiation.

---

## ✅ PHASE 1.3 COMPLETE: Queue Position Algorithm
**Exact specification implemented with proper backend/frontend separation.**

### P1.3 Implementation Results:

#### P1.3.1: ✅ Relationship-Based Letter Entry Algorithm (COMPLETED)
- **REFACTORED**: `CalculateLeveragePosition()` method to implement exact specification
- **ALGORITHM IMPLEMENTED**: 
  ```csharp
  Position = 8 - (highest positive token) + (worst negative token penalty)
  if (HasActiveObligation(npc)) Position = 1;
  if (CommerceDebt >= 3) Position = 2; // Leverage override
  ```
- **CATEGORICAL LOGIC**: Backend only sets positioning reasons, frontend translates to text
- **OBLIGATION INTEGRATION**: Active standing obligations grant position 1 priority
- **COMMERCE DEBT OVERRIDE**: Debt >= 3 Commerce tokens forces position 2

#### P1.3.2: ✅ Backend/Frontend Separation (COMPLETED)
- **CREATED**: `LetterPositioningReason` enum with 5 categorical types
- **CREATED**: `LetterPositioningMessage` class for UI translation data
- **REFACTORED**: MessageSystem to use categorical data instead of UI text
- **ENHANCED**: Letter class with positioning metadata fields
- **PRINCIPLE ENFORCED**: Backend systems never define UI texts

#### P1.3.3: ✅ Algorithm Helper Methods (COMPLETED)
- **IMPLEMENTED**: `HasActiveObligationWithNPC()` - checks standing obligations
- **IMPLEMENTED**: `GetHighestPositiveToken()` - finds best relationship strength  
- **IMPLEMENTED**: `GetWorstNegativeTokenPenalty()` - calculates debt penalty
- **IMPLEMENTED**: `DeterminePositioningReason()` - categorical reason assignment
- **IMPLEMENTED**: `RecordLetterPositioning()` - stores data for UI translation

#### P1.3.4: ✅ Legacy Code Removal (COMPLETED)
- **DELETED**: `GetBasePositionForTokenType()` method (no longer needed)
- **DELETED**: `ApplyPatternModifiers()` method (replaced by exact algorithm)
- **DELETED**: `ShowSimpleLeverageNarrative()` method (violates backend/frontend separation)
- **CLEANED**: All references to removed methods updated

#### Technical Implementation:
- **SPECIFICATION COMPLIANCE**: Algorithm matches implementation plan exactly
- **ARCHITECTURAL PRINCIPLE**: Backend sets categories, frontend translates
- **CATEGORICAL TYPES**: 5 positioning reasons (Obligation, CommerceDebt, PoorStanding, GoodStanding, Neutral)
- **INTEGRATION**: Seamless with existing StandingObligationManager and TokenMechanicsManager

#### Algorithm Validation:
- **Obligation Letters**: Position 1 (highest priority)
- **Commerce Debt >= 3**: Position 2 (leverage override)  
- **Positive Relationships**: Higher positions (8 - strength)
- **Negative Relationships**: Lower positions (8 + debt penalty)
- **Neutral Relationships**: Position 8 (default)

**ARCHITECTURE**: Clean categorical backend with UI translation maintains both mechanical precision and narrative authenticity.

---

## ✅ PHASE 2.1 COMPLETE: Narrative Wrapper System
**Literary overlays added while preserving mechanical transparency.**

### P2.1 Implementation Results:

#### P2.1.1: ✅ Literary Overlays for Mechanical Displays (COMPLETED)
- **ENHANCED**: QueueManipulationPreview component with medieval position descriptions
- **ADDED**: `GetPositionDescription()` method converting positions to literary terms
- **EXAMPLES**: 
  - Position 1 → "Immediate priority (Position 1)"
  - Position 3 → "Third commitment (Position 3)" 
  - Position 8 → "Final burden (Position 8)"
- **PRESERVED**: Mechanical transparency with position numbers in parentheses

#### P2.1.2: ✅ Success Probability Removal (ALREADY COMPLETE)
- **VERIFIED**: No success probability displays in conversation choices
- **CONFIRMED**: `CalculateSuccessProbability()` method exists but unused in UI
- **STATUS**: UI correctly shows mechanical effects without probability calculations

#### P2.1.3: ✅ Relationship Impact Previews (COMPLETED)
- **ENHANCED**: `ParseMechanicalDescription()` to add relationship context
- **ADDED**: `AddRelationshipImpactPreview()` method for human relationship translation
- **EXAMPLES**:
  - "♥ +2 Trust tokens" → "♥ +2 Trust tokens (Elena will remember this kindness)"
  - "⚠ -1 Status" → "⚠ -1 Status (Loss of respect and standing)"
  - "⛓ Creates Binding Obligation" → "⛓ Creates Binding Obligation (Creates ongoing commitment)"
- **PRESERVED**: Full mechanical display with human context overlay

#### Technical Implementation:
- **NARRATIVE-FIRST APPROACH**: Literary descriptions provide context while keeping mechanics visible
- **FRONTEND TRANSLATION**: UI components translate categorical data to human-readable context
- **MEDIEVAL AUTHENTICITY**: Terms like "commitment", "obligation", "burden" replace mechanical position numbers
- **RELATIONSHIP FOCUS**: Token changes explained in terms of human relationship impacts

#### Translation Examples:
- **BEFORE**: "Position 3 in queue" 
- **AFTER**: "Third commitment (Position 3)"
- **BEFORE**: "+2 Trust tokens"
- **AFTER**: "+2 Trust tokens (Elena will remember this kindness)"
- **BEFORE**: "Commerce debt >= 3 forces position 2"
- **AFTER**: "Urgent matter (Position 2 - Leverage)"

**ARCHITECTURE**: Successfully balances mechanical transparency with narrative authenticity through literary overlay system.

---

## ✅ PHASE 2.2 COMPLETE: Information Discovery Architecture
**Comprehensive two-layer discovery system already implemented with backend/frontend separation enhanced.**

### P2.2 Implementation Results:

#### P2.2.1: ✅ Route Discovery System (ALREADY IMPLEMENTED)
- **EXISTING**: RouteDiscoveryManager with `GetDiscoveriesFromNPC()` method
- **EXISTING**: RouteDiscoveryRepository for data management  
- **EXISTING**: Travel permits delivered to Transport NPCs unlock routes
- **EXISTING**: Route network expansion through social connections
- **EXISTING**: Two-layer discovery: "Learn it exists" → "Gain access" pattern

#### P2.2.2: ✅ NPC Introduction Network (ALREADY IMPLEMENTED)
- **EXISTING**: NetworkUnlockManager with relationship-based unlocking
- **EXISTING**: `GetUnlockableNPCs()` method for conversation integration
- **EXISTING**: Introduction letters unlock new NPCs through SpecialLetterHandler
- **EXISTING**: Network effects create emergent opportunities through relationship building

#### P2.2.3: ✅ Backend/Frontend Separation Enhancement (COMPLETED)
- **REFACTORED**: SpecialLetterHandler to use categorical `SpecialLetterEvent` types
- **CREATED**: `SpecialLetterEventType` enum with 13 categorical event types
- **CREATED**: `NarrativeSeverity` enum for UI translation
- **ENHANCED**: MessageSystem with `AddSpecialLetterEvent()` method
- **REMOVED**: Direct UI string generation from backend systems

#### Technical Implementation:
- **ROUTE DISCOVERY**: NPCs share route knowledge based on relationship levels and requirements
- **NETWORK UNLOCKING**: Introduction letters processed categorically with proper event types
- **ACCESS PERMITS**: Location unlocking through categorical event system
- **TOKEN INTEGRATION**: Relationship thresholds determine discovery availability
- **CATEGORICAL EVENTS**: 13 event types for complete special letter processing

#### Discovery Integration Examples:
- **Route Learning**: NPCs mention routes during conversations based on relationship strength
- **NPC Introductions**: High-trust NPCs can introduce player to new contacts
- **Location Access**: Travel permits grant access to restricted locations
- **Information Sharing**: Network effects enable information discovery
- **Emergency Events**: Backend sends categorical data, UI provides narrative context

**ARCHITECTURE**: Complete information discovery ecosystem with proper categorical backend and narrative frontend translation.

---

# PHASE 3: EMOTIONAL STATE PIPELINE
*Week 3 - Bridging Strategic and Tactical Layers*

## Objective
Complete the emotional state system that bridges conversations and letter management.

## ✅ PHASE 3.1 COMPLETE: Emotional State Effects
**Successfully integrated emotional states with conversation mechanics while preserving human authenticity.**

### P3.1 Implementation Results:

#### P3.1.1: ✅ Card Filtering Integration (COMPLETED)
- **ENHANCED**: ConversationCard with `IsAvailableInState()` method for emotional state filtering
- **ENHANCED**: NPCDeck.DrawCards() to filter by emotional state (DESPERATE/ANXIOUS states enable Crisis cards)
- **IMPLEMENTED**: Crisis cards only appear during DESPERATE/ANXIOUS states (authentic desperate measures)
- **MAINTAINED**: HOSTILE NPCs blocked from conversation with clear resolution path

#### P3.1.2: ✅ Patience Calculation with Emotional Effects (COMPLETED)
- **CREATED**: ConversationPatienceCalculator for clean emotional state integration
- **IMPLEMENTED**: Starting patience = BasePatience + TrustTokens - EmotionalPenalty - DeckPenalty
- **EMOTIONAL PENALTIES**: DESPERATE (-3), ANXIOUS (-1), WITHDRAWN (-2), HOSTILE (blocked)
- **BASE PATIENCE**: Derived from personality (DEVOTED/STEADFAST: 8, CUNNING: 7, MERCANTILE: 6, PROUD: 5)

#### P3.1.3: ✅ HOSTILE State Handling (COMPLETED)
- **ENHANCED**: ConversationChoiceGenerator with HOSTILE conversation blocking
- **IMPLEMENTED**: Clear resolution messaging: "deliver overdue letter to restore communication"
- **PRESERVED**: Relationship continuity - HOSTILE is temporary state, not permanent damage

#### P3.1.4: ✅ Human Authenticity Preservation (COMPLETED)
- **NARRATIVE FOCUS**: Emotional states explained through behavioral descriptions, not formulas
- **HIDDEN MECHANICS**: No "Stakes + Time = State" displays in UI
- **AUTHENTIC RESPONSES**: Patience penalties feel like realistic stress effects
- **CRISIS CARDS**: Emergency options feel like genuine desperate measures, not game abilities

#### Technical Implementation:
- **ARCHITECTURAL PRINCIPLE**: Emotional state calculator remains stateless utility
- **INTEGRATION PATTERN**: ConversationChoiceGenerator orchestrates state calculation and card filtering
- **PERFORMANCE**: Clean calculation with personality-based patience lookup
- **ERROR HANDLING**: HOSTILE state throws clear exceptions with resolution guidance

#### Crisis Card Examples:
- **"Promise Personal Help"**: High comfort gain (6), high patience cost (3) - only in DESPERATE/ANXIOUS
- **"Emergency Arrangement"**: Immediate assistance offer (5 comfort, 2 patience) - crisis situations

#### Emotional State Integration Examples:
- **Elena DESPERATE** (marriage deadline <6h): Patience drops from 8 to 5, Crisis cards available
- **Marcus HOSTILE** (deadline passed): Cannot converse until overdue letter delivered
- **Lord Blackwood CALCULATING** (16h deadline): Normal patience, all cards available

**ARCHITECTURE**: Successfully balances mechanical precision with narrative authenticity. Emotional states enhance core tension without artificial difficulty scaling.

## ✅ PHASE 3.2 COMPLETE: Standing Obligations System
**Core mechanic for queue rule overrides - Implementation complete with betrayal recovery system**

### P3.2 Current Status:

#### ✅ COMPLETED FOUNDATION:
- **StandingObligation class**: Complete with 44 ObligationEffect enums and categorical types
- **Position calculation methods**: CalculateEntryPosition() with type-specific logic  
- **Queue integration**: LetterQueueManager calls obligation effects
- **Crisis card availability**: NPCDeck.cs filters for DESPERATE/ANXIOUS states
- **Mechanical effects transfer**: ConversationChoiceGenerator.cs line 82 fixed

#### ✅ IMPLEMENTATION COMPLETED:
1. **✅ Crisis Card Effects**: Crisis cards verified with +3 token effects and binding obligations
2. **✅ Obligation Limit Enforced**: "Max one obligation per NPC" enforcement implemented
3. **✅ Betrayal System Complete**: Relationship-specific betrayal cards with HOSTILE→recovery state transitions
4. **✅ Architecture Enhanced**: NPCDeckFactory with DI injection, anti-defensive programming applied

#### P3.2 IMPLEMENTATION PHASES:

**✅ Phase 1: Crisis Card Mechanics (COMPLETED)**
- ✅ Elena's DEVOTED deck crisis cards with +3 token effects and binding obligations
- ✅ Max one obligation per NPC enforcement implemented
- ✅ Mechanical effects transfer verified with proper +3 Trust tokens

**✅ Phase 2: Obligation Consequences (COMPLETED)**
- ✅ Betrayal card system with 3 relationship-specific recovery cards
- ✅ NPCRelationship.Betrayed enum with RestoreRelationshipEffect for immediate recovery
- ✅ Obligation breaking penalties using GameRules.OBLIGATION_BREAKING_PENALTY
- ✅ Temporal data corruption eliminated in favor of proper state tracking

**✅ Phase 3: Architecture Enhancement (COMPLETED)**
- ✅ NPCDeckFactory with proper DI injection
- ✅ Removed all optional arguments per CLAUDE.md anti-defensive programming principles
- ✅ Unicode symbols replaced with geometric CSS system
- ✅ Build verification and E2E testing completed

**✅ Phase 4: Implementation Verification (COMPLETED)**
- ✅ End-to-end system builds successfully
- ✅ All specialized agent concerns addressed
- ✅ Clean architecture with proper DI throughout

#### Specialized Agent Analysis Results:
- **Game Design**: Excellent foundation, needs proper implementation to achieve potential
- **Systems Architecture**: Critical gaps between current implementation and specifications
- **UI/UX**: Missing obligation display and warning systems
- **Narrative**: System needs emotional authenticity over mechanical optimization

---

# PHASE 4: DEEP SYSTEMS INTEGRATION
*Week 4 - Complete Two-Layer Functionality*

## Objective
Connect all mechanical systems through conversations to achieve full design vision.

### P4.1: Travel System Implementation (Day 16-18)
**Priority: MEDIUM** - Route progression system

#### Implementation:
1. **Route Network with Unlocking**
   - Base routes available from start
   - Permit letters unlock restricted routes
   - Relationship gates for noble/shadow routes
   - Transport NPCs at departure locations

2. **Travel Time Integration**
   - Clear time cost display for each route
   - Transport options (walking/cart/carriage) with trade-offs
   - Deadline pressure affects transport choices

### P4.2: Letter Generation Pipeline (Day 19-20)
**Priority: HIGH** - Core strategic system

#### Implementation:
1. **Comfort Threshold System**
   ```csharp
   if (Comfort >= Patience) UnlockLetterCards();
   if (Comfort >= Patience * 1.5) PerfectConversationBonus();
   ```

2. **Letter Card Integration**
   - Letter cards added to deck when threshold reached
   - Playing letter card successfully generates actual letter
   - Letter properties based on relationship context and NPC state

---

# PHASE 5: MODAL UI ARCHITECTURE
*Week 5 - Information Hierarchy System*

## Objective
Implement four-modal focus system to manage cognitive load of complex dual-layer gameplay.

### P5.1: Modal State Management (Day 21-23)
**Priority: MEDIUM** - UI architecture enhancement

#### Four Modal States:
1. **Map Mode** (Default) - City overview with NPC locations
2. **Conversation Mode** - Current implementation enhanced
3. **Queue Mode** - Letter management interface
4. **Route Planning Mode** - Travel decision interface

#### Implementation:
1. **Navigation Service Enhancement**
   - Modal switching with context preservation
   - Keyboard shortcuts (Q for queue, M for map, ESC for map)
   - Smooth transitions between states

### P5.2: Information Hierarchy (Day 24-25)
**Priority: MEDIUM** - Cognitive load management

#### Always Visible (Critical Info):
- Current attention points
- Time and time block
- Coins
- Position 1 deadline countdown

#### Context Sensitive:
- In conversation: patience, comfort, available cards
- In map: NPC locations, emotional states
- In queue: all letters, weights, deadlines
- In route: connections, times, costs

---

# PHASE 6: NARRATIVE GENERATION ENHANCEMENT
*Week 6 - AI Integration for Authentic Storytelling*

## Objective
Make AI narrative generation feel authentic and connected to mechanical state.

### P6.1: Context-Aware Generation (Day 26-28)
**Priority: LOW** - Polish and immersion

#### Implementation:
1. **Relationship History Integration**
   - AI receives full token history and recent events
   - References previous conversations and shared experiences
   - Shows how NPCs talk to each other about player

2. **Stakes-Appropriate Narrative**
   - Letter contents match emotional weight
   - Conversation tone reflects relationship depth
   - Environmental descriptions support emotional state

### P6.2: Memory Continuity (Day 29-30)
**Priority: LOW** - Deep immersion

#### Implementation:
1. **Persistent Narrative Memory**
   - NPCs remember specific conversation outcomes
   - Relationship changes acknowledged in dialogue
   - Network effects show information spreading

---

# TESTING STRATEGY

## Automated Testing
- **Unit Tests**: All 17 new algorithmic classes
- **Integration Tests**: Full conversation → letter → delivery → deck modification flow
- **E2E Tests**: Complete gameplay loops using Playwright

## Narrative Integrity Testing
- **Playtest Question**: "Do you feel like a medieval letter carrier or a spreadsheet optimizer?"
- **Immersion Metrics**: Players reference NPCs by name and relationship
- **Emotional Connection**: Players feel consequences as relationship impacts

## Performance Testing
- **Build Time**: Under 15 seconds
- **Response Time**: UI interactions under 100ms
- **Memory Usage**: Stable over 30-day campaign

---

# RISK MANAGEMENT

## Technical Risks
- **Circular Dependencies**: Use dependency injection and clear interfaces
- **Performance Degradation**: Implement caching for relationship calculations
- **State Inconsistency**: Single source of truth with validation

## Narrative Risks
- **Spreadsheet Syndrome**: Every change requires narrative review
- **Mechanical Exposure**: Hide all mathematics behind human context
- **Complexity Overwhelming**: Progressive disclosure and smart defaults

## Scope Risks
- **Feature Creep**: Strict adherence to 15 epics, 88 user stories
- **Over-Engineering**: Complete existing systems, don't add new ones
- **Analysis Paralysis**: Time-boxed implementation with clear milestones

---

# SUCCESS METRICS

## Technical Success
- ✅ All 88 user stories pass acceptance criteria
- ✅ Zero compilation errors and warnings
- ✅ 90%+ test coverage on core systems
- ✅ Build and test time under 2 minutes

## Narrative Success
- ✅ Playtesters identify as "letter carrier" not "optimizer"
- ✅ Conversations feel personal and meaningful
- ✅ Queue management feels like managing commitments
- ✅ Mechanical effects feel like relationship consequences

## Gameplay Success
- ✅ Two-layer system creates emergent strategic depth
- ✅ Time pressure creates meaningful tension
- ✅ Relationship building enables tactical advantages
- ✅ Failed deliveries create recovery challenges

---

# IMPLEMENTATION TRACKING

## Session Progress
- **Session 1**: [Date] - Analysis and planning complete
- **Session 2**: [Date] - TBD
- **Session 3**: [Date] - TBD

## Current Status: PLANNING COMPLETE
**Next Session Goal**: P1.1 - Critical UI Integration (Queue visibility, token display, attention integration)

## Dependencies and Blockers
- **Critical Path**: Queue visibility blocks all conversation testing
- **Narrative Integration**: Translation layer blocks UI authenticity
- **System Integration**: Token effects block relationship progression

---

*This implementation plan addresses all specialized agent concerns while maintaining the human heart of Wayfarer's medieval letter carrier fantasy. Each phase builds on previous work while preserving narrative authenticity and mechanical depth.*