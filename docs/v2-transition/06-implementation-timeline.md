# Wayfarer V2 Implementation Timeline: 5-Week POC Sprint

## Executive Summary

This document provides a detailed week-by-week implementation plan for transitioning Wayfarer from V1 to V2, culminating in a playable Miller's Daughter POC. The plan follows a layered approach: foundation systems first, then core mechanics, then content, then polish.

---

## Week 1: Foundation Systems (Days 1-5)

### Goal
Establish core data models and state persistence infrastructure without breaking existing game.

### Monday: Knowledge System Foundation
**Morning (4 hours)**
- Create PlayerKnowledge domain model
  - AcquiredKnowledge HashSet<string> for facts
  - KnownSecrets HashSet<string> for discovered secrets
  - Methods: HasKnowledge(string), AddKnowledge(string), GetAllKnowledge()
- Add Knowledge property to Player model
- Create KnowledgeService for managing knowledge state
- Write unit tests for knowledge acquisition and checking

**Afternoon (4 hours)**
- Create KnowledgeRequirement domain model
  - RequiredKnowledge List<string>
  - RequiredSecrets List<string>
  - MeetsRequirements(PlayerKnowledge) method
- Integrate with existing conversation system
  - Add KnowledgeGained to CardEffect
  - Update conversation resolution to grant knowledge
- Test knowledge flow through conversations

### Tuesday: Equipment Enables System
**Morning (4 hours)**
- Create Equipment domain model extending Item
  - EnabledActions List<string> property
  - Weight, Value properties (reuse existing)
  - IsEquipment flag for differentiation
- Create EquipmentService
  - GetEnabledActions(Player) method
  - HasEnabledAction(Player, string) method
  - Equipment inventory management

**Afternoon (4 hours)**
- Create EquipmentRequirement domain model
  - RequiredEquipment List<string>
  - RequiredActions List<string>
  - MeetsRequirements(Player) method
- Add equipment checks to existing systems
- Create test equipment items in JSON
- Unit test equipment enables

### Wednesday: Investigation Domain Models
**Morning (4 hours)**
- Create Investigation domain model
  - Id, Name, Description, LocationId
  - Phases List<InvestigationPhase>
  - InitialPhaseId string
  - CompletionRewards (knowledge, items, discoveries)
- Create InvestigationPhase model
  - Id, Name, Description
  - Choices List<InvestigationChoice>
  - IsTerminal bool

**Afternoon (4 hours)**
- Create InvestigationChoice model
  - Id, Name, Description
  - Requirements (knowledge, equipment, stats)
  - Outcomes List<InvestigationOutcome>
  - NextPhaseId string
- Create InvestigationOutcome model
  - Description, Probability
  - Effects (resource changes, knowledge gained)
  - Discoveries List<string>

### Thursday: State Persistence Layer
**Morning (4 hours)**
- Create InvestigationProgress domain model
  - InvestigationId string
  - CurrentPhaseId string
  - CompletedPhases HashSet<string>
  - DiscoveredInformation HashSet<string>
  - LastAttemptTime DateTime
- Add to GameWorld
  - InvestigationProgress Dictionary<string, InvestigationProgress>
  - RouteImprovements Dictionary<string, List<string>>

**Afternoon (4 hours)**
- Create StateService for persistence
  - SaveInvestigationProgress method
  - LoadInvestigationProgress method
  - Integration with existing save/load system
- Test state persistence across sessions

### Friday: Travel Obstacle System Foundation
**Morning (4 hours)**
- Create TravelObstacle domain model
  - Id, Name, Description
  - ObstacleType enum (Physical, Environmental, Social)
  - Approaches List<ObstacleApproach>
- Create ObstacleApproach model
  - Name, Description
  - Requirements (equipment, knowledge, stats)
  - SuccessOutcome, FailureOutcome

**Afternoon (4 hours)**
- Create Route refactor (parallel to existing)
  - Add Obstacles List<TravelObstacle> to Route
  - Keep existing PathCards for backward compatibility
  - Add UseObstacleSystem bool flag
- Create TravelService updates
  - Support both path cards and obstacles
  - Route selection logic
- Integration testing

### Deliverables
- Complete domain model layer
- State persistence working
- Unit tests for all new models
- Existing game still fully functional

---

## Week 2: Core Mechanics Implementation (Days 6-10)

### Goal
Implement the investigation and travel mechanics, integrate with existing systems.

### Monday: Investigation Service Layer
**Morning (4 hours)**
- Create InvestigationService
  - StartInvestigation(Player, Investigation)
  - GetAvailableChoices(Investigation, Phase, Player)
  - ProcessChoice(Player, Investigation, Choice)
  - CompleteInvestigation logic

**Afternoon (4 hours)**
- Create InvestigationManager (UI orchestration)
  - Load investigation from GameWorld
  - Track current phase
  - Handle choice selection
  - Update UI state
- Wire up to existing location system

### Tuesday: Investigation UI Components
**Morning (4 hours)**
- Create InvestigationScreen.razor
  - Display current phase description
  - Show available choices (filtered by requirements)
  - Display requirements not met (grayed out)
  - Navigation between phases

**Afternoon (4 hours)**
- Create InvestigationChoiceCard.razor component
  - Display choice with requirements
  - Show success/failure probabilities
  - Handle selection and processing
- Style with existing card CSS patterns
- Test UI flow

### Wednesday: Travel Obstacle Processing
**Morning (4 hours)**
- Update TravelService
  - ProcessObstacle method
  - CheckApproachRequirements
  - ApplyObstacleOutcomes
  - Route improvement logic

**Afternoon (4 hours)**
- Create TravelObstacleScreen.razor
  - Display obstacle description
  - Show available approaches
  - Display requirements
  - Handle approach selection
- Integration with travel flow

### Thursday: Danger System Implementation
**Morning (4 hours)**
- Create DangerOutcome domain model
  - DangerType enum (Health, Stamina, Social, Information)
  - Severity (Minor, Major, Critical)
  - Effects and mitigation
- Create DangerService
  - ProcessDanger method
  - ApplyDangerEffects
  - CheckMitigation

**Afternoon (4 hours)**
- Integrate dangers with investigations
  - Add Dangers to InvestigationOutcome
  - Process during choice resolution
  - Display danger warnings in UI
- Test danger processing

### Friday: System Integration
**Morning (4 hours)**
- Connect investigation discoveries to NPCs
  - Create ObservationCard system
  - Update NPC conversations with discoveries
  - Test discovery flow

**Afternoon (4 hours)**
- Connect knowledge from conversations to investigations
  - Update conversation rewards
  - Test knowledge gating
  - Verify circular dependencies work
- Full integration testing

### Deliverables
- Investigation system fully functional
- Travel obstacles working in parallel
- Danger system integrated
- All systems talking to each other

---

## Week 3: Content Creation - Miller's Daughter (Days 11-15)

### Goal
Implement the complete Miller's Daughter scenario as proof of concept.

### Monday: Location and NPC Setup
**Morning (4 hours)**
- Create Millhaven location JSON
  - Town square, Mill, Elena's cottage
  - Investigation markers
- Create Miller NPC
  - Full conversation deck
  - Investigation quest trigger

**Afternoon (4 hours)**
- Create supporting NPCs
  - Woodcutter (Magnus)
  - Innkeeper
  - Town Elder
- Create their conversation decks
- Add knowledge dispensing

### Tuesday: Investigation Content - Part 1
**Morning (4 hours)**
- Create Mill investigation JSON
  - Phase 1: Initial exploration
  - Phase 2: Scene examination
  - Phase 3: Miller confrontation
  - All choices and requirements

**Afternoon (4 hours)**
- Create Forest Well investigation
  - Phase 1: Approach
  - Phase 2: Descent choices
  - Phase 3: Discovery
  - Phase 4: Elena's fate

### Wednesday: Investigation Content - Part 2
**Morning (4 hours)**
- Create Investigation outcomes
  - Success paths and rewards
  - Failure consequences
  - Knowledge and secret discoveries
  - Item rewards (Elena's locket, etc.)

**Afternoon (4 hours)**
- Create danger scenarios
  - Well descent dangers
  - Miller confrontation dangers
  - Forest travel dangers
- Test all paths through investigations

### Thursday: Travel Routes and Obstacles
**Morning (4 hours)**
- Create Ironford-Millhaven route
  - Bridge obstacle
  - Forest path obstacle
  - Weather complications
- Create Millhaven-Forest route
  - Trail finding obstacle
  - Wildlife obstacle

**Afternoon (4 hours)**
- Create obstacle approaches
  - Equipment-based solutions
  - Knowledge-based solutions
  - Stat-based solutions
- Test all travel paths

### Friday: Content Integration and Testing
**Morning (4 hours)**
- Create discovery propagation
  - Miller's confession affects NPCs
  - Elena's fate affects NPCs
  - Resolution creates new quests
- Test NPC reaction changes

**Afternoon (4 hours)**
- Full playthrough testing
  - Test all three main paths
  - Verify no soft-locks
  - Check completion times (45-60 min target)
- Document any issues

### Deliverables
- Complete Miller's Daughter content
- All NPCs and locations configured
- All investigations playable
- All routes traversable

---

## Week 4: Polish and Secondary Systems (Days 16-20)

### Goal
Add polish, UI improvements, and secondary systems to enhance player experience.

### Monday: UI Polish
**Morning (4 hours)**
- Create Investigation Journal UI
  - Track active investigations
  - Show discovered information
  - Display investigation progress
  - Organize by location

**Afternoon (4 hours)**
- Create Knowledge Codex UI
  - Display all acquired knowledge
  - Organize by source and topic
  - Show knowledge applications
  - Search and filter

### Tuesday: Tutorial System
**Morning (4 hours)**
- Create tutorial investigation
  - Simple 3-phase structure
  - Teaches all mechanics
  - No fail states
  - Clear guidance

**Afternoon (4 hours)**
- Create tutorial UI overlays
  - Highlight new mechanics
  - Explain requirements
  - Guide first investigation
- Test with fresh profile

### Wednesday: Economic Integration
**Morning (4 hours)**
- Create equipment shop updates
  - Add investigation equipment
  - Price balancing
  - Availability conditions
- Create service economy
  - Information brokers
  - Guides for travel
  - Investigation supplies

**Afternoon (4 hours)**
- Balance economic loop
  - Work job payments
  - Investigation rewards
  - Equipment costs
  - Service prices
- Test economic progression

### Thursday: Save/Load Robustness
**Morning (4 hours)**
- Enhance save system
  - Save mid-investigation
  - Save mid-travel
  - Investigation progress
  - Knowledge state
- Test save/load cycles

**Afternoon (4 hours)**
- Create migration system
  - V1 saves to V2 conversion
  - Preserve what's possible
  - Graceful degradation
- Test with old saves

### Friday: Performance and Bug Fixes
**Morning (4 hours)**
- Performance profiling
  - Optimize investigation loading
  - Reduce UI re-renders
  - Memory usage check
- Apply optimizations

**Afternoon (4 hours)**
- Bug fix sprint
  - Address all logged issues
  - Edge case handling
  - Error recovery
- Regression testing

### Deliverables
- Polished UI for all new systems
- Tutorial for new players
- Robust save/load
- Performance optimized

---

## Week 5: Testing, Balancing, and Launch Prep (Days 21-25)

### Goal
Extensive testing, balance tuning, and preparation for POC release.

### Monday: Comprehensive Testing
**Morning (4 hours)**
- Create test scenarios
  - Speed run path (minimum time)
  - Completionist path (all content)
  - Failure recovery path
  - Edge case path

**Afternoon (4 hours)**
- Execute all test scenarios
  - Document completion times
  - Note difficulty spikes
  - Find soft-locks
  - Check narrative coherence

### Tuesday: Balance Tuning
**Morning (4 hours)**
- Tune investigation difficulty
  - Requirement thresholds
  - Danger probabilities
  - Reward values
- Ensure multiple viable paths

**Afternoon (4 hours)**
- Tune economic balance
  - Ensure equipment accessible
  - Prevent infinite money
  - Balance time investment
- Test full economic loop

### Wednesday: Content Polish
**Morning (4 hours)**
- Writing pass on all text
  - Investigation descriptions
  - Choice descriptions
  - Outcome narratives
- Ensure consistent tone

**Afternoon (4 hours)**
- Add narrative flourishes
  - Ambient descriptions
  - Character voices
  - World building details
- Test narrative flow

### Thursday: Documentation and Packaging
**Morning (4 hours)**
- Create player documentation
  - New mechanics guide
  - Investigation tips
  - Change summary from V1
- Update help system

**Afternoon (4 hours)**
- Create technical documentation
  - Content creation guide
  - System architecture docs
  - Modding guidelines
- Package for release

### Friday: Final Testing and Release
**Morning (4 hours)**
- Final acceptance testing
  - Fresh install test
  - Migration test
  - Full playthrough
- Sign-off checklist

**Afternoon (4 hours)**
- Release preparation
  - Build release version
  - Prepare patch notes
  - Update version numbers
  - Deploy POC

### Deliverables
- Fully tested and balanced POC
- Complete documentation
- Release-ready build
- Miller's Daughter playable

---

## Risk Mitigation Schedule

### Daily Practices
- Morning standup (even if solo): Review plan, identify blockers
- End-of-day commit: Always have working version
- Test after each major change: Prevent regression accumulation

### Weekly Checkpoints
- Monday: Week plan review and adjustment
- Wednesday: Mid-week progress assessment
- Friday: Week retrospective and next week prep

### Contingency Buffers
- Each week has 20% time buffer built in
- Critical features front-loaded
- Nice-to-haves clearly marked
- Can cut: Tutorial polish, economic secondary systems, some UI polish

---

## Success Metrics Tracking

### Daily Metrics
- Features completed vs planned
- Tests written and passing
- Bugs discovered and fixed
- Hours worked vs estimated

### Weekly Metrics
- Milestone completion percentage
- Code coverage percentage
- Performance benchmarks
- Playtest feedback scores

### Final Metrics
- Complete Miller's Daughter playable: YES/NO
- No soft-locks possible: YES/NO
- 45-60 minute completion: YES/NO
- All three paths work: YES/NO
- Save/load reliable: YES/NO

---

## Post-POC Roadmap (Weeks 6+)

### Week 6-7: Feedback Integration
- Gather playtest feedback
- Priority bug fixes
- Balance adjustments
- UI improvements from feedback

### Week 8-10: Content Expansion
- 3 additional investigations
- 2 new locations
- 5 new NPCs
- Modular content system implementation

### Week 11-12: Systems Enhancement
- AI flavor layer integration
- Advanced equipment system
- Reputation system basics
- Discovery cascade system

---

## Critical Path Dependencies

```
Knowledge System → Investigation Requirements
Equipment System → Approach Requirements
State Persistence → Investigation Progress
Investigation Domain → Investigation Service → Investigation UI
Travel Obstacles → Travel Service Updates → Travel UI
Danger System → Investigation Outcomes
All Core Systems → Miller's Daughter Content
Miller's Daughter → Testing and Balance
Testing → Documentation → Release
```

## Minimum Viable POC

If time becomes critical, these are the absolute minimums:

1. Knowledge system (conversations grant knowledge)
2. Investigation system (one investigation works)
3. Miller's Daughter content (main path only)
4. Basic UI (functional, not pretty)
5. Save/load (investigations persist)

Everything else can be post-POC.