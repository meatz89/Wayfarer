# Letter Queue Transformation: Master Todo List

**Document Purpose**: This is the comprehensive high-level todo list for transforming Wayfarer from a contract-based trading RPG into a letter queue management game.

**📚 Essential Reading Before Starting**:
- **`LETTER-QUEUE-TRANSFORMATION-ANALYSIS.md`** - Complete analysis and detailed implementation plan
- **`LETTER-QUEUE-UI-SPECIFICATION.md`** - UI requirements for all screens
- **`LETTER-QUEUE-INTEGRATION-PLAN.md`** - System transformation approach

---

## **PHASE 1: FOUNDATION (Weeks 1-2)** 
*Goal: Create core letter queue infrastructure alongside existing systems*

### Week 1: Core Data Structures
- [ ] **Create Letter Entity**
  - Properties: Id, SenderId, RecipientId, TokenType, Deadline, Payment, Size, QueuePosition, IsFromPatron
  - Implement Letter class with all validations
  - Create unit tests for Letter entity

- [ ] **Implement LetterQueue Class**
  - 8-slot array with position enforcement
  - Methods: AddLetter(), RemoveLetter(), GetLetterAtPosition(), ShiftQueue()
  - Enforce sacred delivery order (position 1 first)
  - Create comprehensive unit tests

- [ ] **Build ConnectionToken System**
  - Create ConnectionType enum (Trust, Trade, Noble, Common, Shadow)
  - Implement player token storage (Dictionary<ConnectionType, int>)
  - Add per-NPC token tracking (Dictionary<string, Dictionary<ConnectionType, int>>)
  - Create token manipulation methods

- [ ] **Create Core Repositories**
  - LetterRepository with queue management methods
  - ConnectionTokenRepository with token operations
  - Follow existing repository patterns (stateless, GameWorld access only)
  - Add repository unit tests

### Week 2: Basic UI and Time Integration
- [ ] **Add Basic Queue UI Component**
  - Create LetterQueueDisplay.razor component
  - Visual 8-slot display with position numbers
  - Letter cards showing essential information
  - Empty slot placeholders

- [ ] **Create Token Display UI**
  - TokenBalance.razor component
  - Show all 5 token types with icons
  - Visual indicators for token spending

- [ ] **Integrate with Time System**
  - Add daily deadline countdown in GameWorldManager
  - Implement morning queue update routine
  - Create letter expiration mechanics
  - Hook into existing time progression

---

## **PHASE 2: CONTENT TRANSFORMATION (Weeks 3-4)**
*Goal: Transform existing content to letter-based system*

### Week 3: NPC and Letter Content
- [ ] **Transform NPCs to Letter Senders**
  - Add tokenType property to all NPCs
  - Add letterGeneration configuration (frequency, types, deadlines)
  - Implement relationship tracking properties
  - Create NPC location assignments

- [ ] **Create Letter Templates**
  - Design 50+ letter templates across all token types
  - Implement procedural generation variations
  - Create sender/recipient mapping rules
  - Build deadline generation logic

- [ ] **Design Letter Categories**
  - Personal (Trust): love letters, family news, friendship notes
  - Commercial (Trade): goods delivery, payment collection, guild business
  - Aristocratic (Noble): formal invitations, court summons, diplomatic pouches
  - Everyday (Common): local news, help requests, market gossip
  - Underground (Shadow): secret packages, coded messages, illicit goods

### Week 4: Standing Obligations
- [ ] **Design Core Obligations**
  - Noble's Courtesy: Noble letters enter at slot 5, cannot refuse
  - Merchant's Priority: Trade letters +10 coins, cannot purge
  - Shadow's Burden: Triple pay shadow letters, forced every 3 days
  - Patron's Eye: Patron letters advance 1 slot/day
  - Heart's Bond: Trust letters extend deadline free, double skip cost

- [ ] **Create Obligation System**
  - StandingObligation class with effects and constraints
  - Acquisition triggers and requirements
  - Conflict detection between obligations
  - Queue behavior modification logic

---

## **PHASE 3: SYSTEM INTEGRATION (Weeks 5-6)**
*Goal: Make existing systems serve the queue*

### Week 5: Travel and Equipment Integration
- [ ] **Transform Travel System**
  - Make routes serve queue delivery optimization
  - Add route selection based on queue order
  - Implement equipment requirements for efficient paths
  - Create delivery batching opportunities

- [ ] **Update Equipment System**
  - Transform equipment to route enablers
  - Climbing Gear: mountain shortcuts
  - Navigation Tools: forest paths
  - Court Attire: noble letter access
  - Guild Credentials: merchant areas

### Week 6: Queue Manipulation
- [ ] **Implement Core Queue Actions**
  - Purge: 3 any tokens to remove bottom letter
  - Priority: 5 matching tokens to move to slot 1
  - Extend: 2 matching tokens to add 2 days
  - Skip: 1 matching token per position skipped

- [ ] **Create Connection Gravity**
  - Calculate entry position based on NPC tokens
  - 0-2 tokens: slot 8, 3-4 tokens: slot 7, 5+ tokens: slot 6
  - Implement patron letter override (slots 1-3)

- [ ] **Build Manipulation UI**
  - Action buttons with token costs
  - Validation and confirmation dialogs
  - Visual feedback for actions
  - Token spending animations

---

## **PHASE 4: UI TRANSFORMATION (Weeks 7-8)**
*Goal: Replace existing UI with queue-centric design*

### Week 7: Core Screens
- [ ] **Build Letter Queue Screen**
  - Primary gameplay interface
  - 8-slot queue with manipulation actions
  - Standing obligations panel
  - Token balance display
  - Morning actions (swap, accept letters)

- [ ] **Create Character Relationship Screen**
  - NPC list organized by location
  - Per-NPC token display
  - Relationship status indicators
  - Interaction availability (location-based)
  - Letter history tracking

### Week 8: Integration and Polish
- [ ] **Implement Standing Obligations Screen**
  - Active obligations with descriptions
  - Queue effects visualization
  - Conflict warnings
  - Acquisition progress

- [ ] **Add Cross-Screen Navigation**
  - Main navigation menu
  - Context-sensitive links
  - Keyboard shortcuts
  - Mobile responsive design

- [ ] **Create Notification System**
  - Queue crisis alerts
  - Deadline warnings
  - Relationship status changes
  - Patron letter arrivals

---

## **PHASE 5: MIGRATION AND CLEANUP (Weeks 9-10)**
*Goal: Remove legacy systems and migrate data*

### Week 9: Legacy System Removal
- [ ] **Delete Contract System**
  - Remove ContractManager, ContractRepository
  - Delete contract-related UI components
  - Clean up contract JSON files
  - Update all contract references

- [ ] **Remove Reputation/Favor Systems**
  - Delete reputation tracking code
  - Remove favor calculations
  - Transform to token counts
  - Update NPC interactions

- [ ] **Clean Obsolete UI**
  - Remove old quest screens
  - Delete contract displays
  - Update navigation flows
  - Simplify menu structure

### Week 10: Balance and Migration
- [ ] **Implement Save Migration**
  - Create versioned save system
  - Build migration routines
  - Add rollback capability
  - Test with various save files

- [ ] **Balance Token Economy**
  - Tune earning rates
  - Adjust spending costs
  - Validate scarcity
  - Test crisis frequency

- [ ] **Comprehensive Testing**
  - Queue pressure scenarios
  - Token economy balance
  - Obligation conflicts
  - Save migration integrity

---

## **PHASE 6: NARRATIVE POLISH (Weeks 11-12)**
*Goal: Add depth and special mechanics*

### Week 11: Dynamic Events
- [ ] **Implement Letter Chains**
  - Follow-up letter generation
  - Narrative sequences
  - Relationship story arcs
  - Completion rewards

- [ ] **Create Crisis Events**
  - Denna-style interruptions
  - Rival letter conflicts
  - Emergency deliveries
  - Relationship crises

- [ ] **Add Seasonal Events**
  - Holiday letter surges
  - Weather-based urgency
  - Festival obligations
  - Patron special requests

### Week 12: Final Polish
- [ ] **Implement Patron Mystery**
  - Monthly letter generation
  - Pattern revelation system
  - Resource provision
  - Mystery resolution hints

- [ ] **Create Tutorial Flow**
  - Queue mechanics introduction
  - Token system explanation
  - Obligation warnings
  - Crisis management tips

- [ ] **Add Achievement System**
  - Queue mastery achievements
  - Relationship milestones
  - Obligation collections
  - Perfect day challenges

---

## **VALIDATION CHECKPOINTS**

### Technical Validation
- [ ] Queue order enforcement works correctly
- [ ] Token spending validates properly
- [ ] Deadlines countdown accurately
- [ ] Save migration completes without data loss
- [ ] Performance remains smooth with complex state

### Design Validation
- [ ] Queue creates genuine strategic tension
- [ ] Tokens feel precious and meaningful
- [ ] Standing obligations reshape gameplay
- [ ] Mathematical impossibility exists
- [ ] Player agency preserved

### Emotional Validation
- [ ] Players care about specific NPCs
- [ ] Token spending creates difficult decisions
- [ ] Queue crises feel urgent
- [ ] Patron mystery intrigues
- [ ] Relationships feel authentic

---

## **RISK MITIGATION TASKS**

### Throughout Development
- [ ] Maintain parallel systems during transition
- [ ] Create rollback points at each phase
- [ ] Document all breaking changes
- [ ] Test continuously with real players
- [ ] Keep existing game playable

### Critical Safeguards
- [ ] Implement comprehensive logging
- [ ] Create debugging tools for queue state
- [ ] Build admin commands for testing
- [ ] Prepare hotfix procedures
- [ ] Plan post-launch support

---

## **SUCCESS CRITERIA**

### Core Experience
- [ ] ✅ Players feel like Kvothe juggling obligations
- [ ] ✅ Queue management creates authentic stress
- [ ] ✅ Every decision has relationship consequences
- [ ] ✅ Simple rules create complex situations
- [ ] ✅ Replayability through different obligation paths

### Technical Excellence
- [ ] ✅ Smooth transition from old to new systems
- [ ] ✅ No data loss during migration
- [ ] ✅ Performance meets or exceeds current
- [ ] ✅ Code follows established patterns
- [ ] ✅ Comprehensive test coverage

---

## **NEXT IMMEDIATE STEPS**

1. **Read**: `LETTER-QUEUE-TRANSFORMATION-ANALYSIS.md` completely
2. **Setup**: Create feature branch for transformation
3. **Begin**: Start with Letter entity creation (Phase 1, Week 1, Day 1)
4. **Track**: Update this todo list daily
5. **Communicate**: Document discoveries and blockers

**Estimated Timeline**: 12 weeks from start to complete transformation
**Critical Path**: Letter Queue → Tokens → NPCs → UI → Migration → Polish

This transformation will revolutionize Wayfarer from a traditional RPG into an emotional obligation management experience that captures the authentic feeling of being overwhelmed by social commitments while maintaining player agency and strategic depth.