# Work Packet Assignments & Validation Strategy

## CRITICAL: Agent Verification Protocol

**ASSUMPTION: Every agent will lie about completion.**
**REQUIREMENT: Concrete proof of work before acceptance.**

## Validation Levels

### Level 1: File Existence
- Files actually created
- Correct location
- Not empty files

### Level 2: Code Completeness  
- All methods implemented
- No TODOs or placeholders
- No throw new NotImplementedException()

### Level 3: Integration
- Compiles without errors
- GameFacade actually uses new code
- Old code actually deleted

### Level 4: Functional Testing
- Run the game
- Test specific features
- Screenshot proof required

---

## WORK PACKET #1: LocationSubsystem Analysis

**Assigned To:** Content Integrator Agent
**Purpose:** Map EVERY location-related method before implementation
**Deadline:** 2 hours

### Deliverables Required:
1. Complete list of ALL location methods in GameFacade.cs
2. Line numbers for each method
3. Complete list of ALL location methods in other files
4. Dependency map showing what calls what
5. List of UI components that use location methods

### Validation Checklist:
- [ ] Agent provides actual line numbers (I will verify)
- [ ] Agent lists at least 15+ location methods (GameFacade has many)
- [ ] Agent identifies LocationRepository, LocationSpotRepository usage
- [ ] Agent finds UI dependencies in LocationContent.razor.cs
- [ ] Agent provides grep/search results as proof

### Validation Commands:
```bash
# Verify agent found all location methods
grep -n "Location" /mnt/c/git/wayfarer/src/Services/GameFacade.cs | wc -l
grep -n "Spot" /mnt/c/git/wayfarer/src/Services/GameFacade.cs | wc -l
grep -n "CurrentLocation" /mnt/c/git/wayfarer/src/Services/GameFacade.cs
```

### Red Flags (Reject if present):
- Vague descriptions without line numbers
- "Approximately X methods" instead of exact count
- No grep output provided
- Claims to have analyzed without showing search results

---

## WORK PACKET #2: LocationSubsystem Implementation

**Assigned To:** Systems Architect Agent
**Purpose:** COMPLETE implementation of LocationSubsystem
**Deadline:** 4 hours
**Dependencies:** WP#1 must be validated first

### Deliverables Required:
```
/src/Subsystems/Location/
├── LocationFacade.cs (200+ lines minimum)
├── LocationManager.cs (150+ lines minimum)
├── LocationSpotManager.cs (150+ lines minimum)
├── MovementValidator.cs (100+ lines minimum)
├── NPCLocationTracker.cs (100+ lines minimum)
└── LocationRepository.cs (moved from old location)
```

### Required Methods in LocationFacade:
```csharp
public Location GetCurrentLocation()
public LocationSpot GetCurrentSpot()
public bool MoveToSpot(string spotId)
public List<LocationSpot> GetAccessibleSpots()
public List<NPC> GetNPCsAtCurrentLocation()
public List<NPC> GetNPCsAtSpot(string spotId)
public bool CanMoveTo(string spotId)
public Route GetRouteToLocation(string locationId)
public List<Location> GetAllLocations()
public Location GetLocationById(string id)
public LocationSpot GetSpotById(string id)
public void UpdatePlayerLocation(Location location, LocationSpot spot)
```

### Validation Checklist:
- [ ] All 6 files exist and have content
- [ ] LocationFacade has ALL 12 required methods
- [ ] Each file compiles (dotnet build)
- [ ] No TODOs in any file
- [ ] No NotImplementedException
- [ ] GameFacade updated to use LocationFacade

### Validation Commands:
```bash
# Check files exist
ls -la /mnt/c/git/wayfarer/src/Subsystems/Location/

# Check file sizes (no empty files)
wc -l /mnt/c/git/wayfarer/src/Subsystems/Location/*.cs

# Check for TODOs
grep -r "TODO" /mnt/c/git/wayfarer/src/Subsystems/Location/

# Check for NotImplementedException
grep -r "NotImplementedException" /mnt/c/git/wayfarer/src/Subsystems/Location/

# Verify compilation
cd /mnt/c/git/wayfarer/src && dotnet build

# Verify GameFacade uses LocationFacade
grep "LocationFacade" /mnt/c/git/wayfarer/src/Services/GameFacade.cs
```

### Red Flags (Reject if present):
- Files with less than 50 lines
- Methods that just return null
- Methods with empty bodies
- TODO comments anywhere
- "Will implement later" comments
- Build errors

---

## WORK PACKET #3: ConversationSubsystem Analysis

**Assigned To:** Content Integrator Agent  
**Purpose:** Map ENTIRE conversation system before implementation
**Deadline:** 2 hours

### Deliverables Required:
1. Complete method list from ConversationManager.cs (833 lines)
2. Complete method list from ConversationSession.cs (1,348 lines)
3. Dependency map of card system
4. List of all conversation-related ViewModels
5. UI components using conversations

### Validation Checklist:
- [ ] Lists ALL methods from ConversationManager (should be 30+)
- [ ] Lists ALL methods from ConversationSession (should be 40+)
- [ ] Identifies CardDeck, HandDeck, SessionCardDeck classes
- [ ] Finds ConversationContext usage
- [ ] Maps emotional state management

### Validation Commands:
```bash
# Count actual methods in files
grep -E "public|private|protected" /mnt/c/git/wayfarer/src/Game/ConversationSystem/Managers/ConversationManager.cs | wc -l
grep -E "public|private|protected" /mnt/c/git/wayfarer/src/Game/ConversationSystem/Models/ConversationSession.cs | wc -l

# Verify agent found deck classes
ls /mnt/c/git/wayfarer/src/Game/ConversationSystem/Core/
```

---

## WORK PACKET #4: ConversationSubsystem Implementation

**Assigned To:** Systems Architect Agent
**Purpose:** COMPLETE implementation of ConversationSubsystem
**Deadline:** 6 hours
**Dependencies:** WP#3 must be validated first

### Deliverables Required:
```
/src/Subsystems/Conversation/
├── ConversationFacade.cs (300+ lines)
├── ConversationOrchestrator.cs (400+ lines)
├── CardDeckManager.cs (300+ lines)
├── EmotionalStateManager.cs (200+ lines)
├── ComfortManager.cs (150+ lines)
├── DialogueGenerator.cs (200+ lines)
├── ConversationContext.cs (moved)
└── ConversationSession.cs (refactored)
```

### Required Functionality:
- Start conversations with NPCs
- Process LISTEN/SPEAK actions
- Handle card draws and plays
- Manage emotional state transitions
- Calculate comfort changes
- Handle goal card urgency
- Generate dialogue

### Validation Checklist:
- [ ] All 8 files exist with substantial content
- [ ] OLD ConversationManager.cs DELETED
- [ ] Can start a conversation in-game
- [ ] Can play cards
- [ ] Emotional states change correctly
- [ ] Comfort system works

### Validation Commands:
```bash
# Verify old file deleted
ls /mnt/c/git/wayfarer/src/Game/ConversationSystem/Managers/ConversationManager.cs
# Should return: No such file or directory

# Test compilation
cd /mnt/c/git/wayfarer/src && dotnet build

# Run game and test conversation
ASPNETCORE_URLS="http://localhost:5099" timeout 30 dotnet run --no-build
# Then use Playwright to test conversation
```

### Functional Test Script:
```javascript
// Playwright test that agent MUST pass
await page.goto('http://localhost:5099');
await page.click('[data-npc-id]'); // Click any NPC
await page.waitForSelector('.conversation-screen');
await page.click('[data-action="listen"]');
await page.waitForSelector('.card-hand');
// Screenshot required showing cards in hand
```

---

## WORK PACKET #5: ObligationSubsystem Analysis

**Assigned To:** Content Integrator Agent
**Purpose:** Map ALL 2,819 lines of ObligationQueueManager
**Deadline:** 3 hours

### Deliverables Required:
1. Method inventory with line numbers (should be 80+ methods)
2. Categorization into 5 new managers
3. Queue manipulation method list
4. Displacement calculation analysis
5. UI dependencies

### Validation Checklist:
- [ ] Provides line-by-line analysis
- [ ] Identifies at least 80 methods
- [ ] Correctly categorizes into DeliveryManager, MeetingManager, etc.
- [ ] Finds QueueDisplacementPlanner usage
- [ ] Maps obligation types

### Validation Commands:
```bash
# Verify size of file
wc -l /mnt/c/git/wayfarer/src/GameState/ObligationQueueManager.cs
# Must show 2819 lines

# Count methods
grep -E "public|private|protected.*\(" /mnt/c/git/wayfarer/src/GameState/ObligationQueueManager.cs | wc -l
# Should be 80+
```

---

## WORK PACKET #6: ObligationSubsystem Implementation

**Assigned To:** Systems Architect Agent
**Purpose:** Break down 2,819 line monolith into 5 managers
**Deadline:** 8 hours
**Dependencies:** WP#5 must be validated first

### Deliverables Required:
```
/src/Subsystems/Obligation/
├── ObligationFacade.cs (250+ lines)
├── DeliveryManager.cs (500+ lines)
├── MeetingManager.cs (400+ lines)
├── QueueManipulator.cs (600+ lines)
├── DisplacementCalculator.cs (500+ lines)
├── DeadlineTracker.cs (400+ lines)
└── ObligationStatistics.cs (200+ lines)
```

### Critical Requirements:
- ALL 2,819 lines must be accounted for
- ObligationQueueManager.cs must be DELETED
- Queue operations must still work
- Letter delivery must work
- Displacement must calculate correctly

### Validation Checklist:
- [ ] Total lines in new files >= 2,800
- [ ] OLD ObligationQueueManager.cs deleted
- [ ] Can accept letters in-game
- [ ] Can deliver letters
- [ ] Queue displacement works
- [ ] Token burning works

### Validation Commands:
```bash
# Count total migrated lines
wc -l /mnt/c/git/wayfarer/src/Subsystems/Obligation/*.cs | tail -1
# Must be >= 2,800 lines

# Verify old file deleted
ls /mnt/c/git/wayfarer/src/GameState/ObligationQueueManager.cs
# Must return: No such file or directory

# Test game
cd /mnt/c/git/wayfarer/src && dotnet build && dotnet run
```

### Functional Test:
- Start game
- Accept a letter from Elena
- Check queue shows letter
- Deliver letter
- Verify payment received

---

## WORK PACKET #7: GameFacade Refactoring

**Assigned To:** Systems Architect Agent
**Purpose:** Reduce GameFacade from 3,448 to <500 lines
**Deadline:** 4 hours
**Dependencies:** WP#2, WP#4, WP#6 must be complete

### Deliverables Required:
1. GameFacade.cs with ONLY delegation code
2. All business logic moved to subsystems
3. Clean constructor with facade injection
4. Every method one-line delegation

### Validation Checklist:
- [ ] GameFacade.cs < 500 lines
- [ ] No business logic in GameFacade
- [ ] All methods delegate to facades
- [ ] Game still runs perfectly

### Validation Commands:
```bash
# Check file size
wc -l /mnt/c/git/wayfarer/src/Services/GameFacade.cs
# Must show < 500

# Check for business logic (should find none)
grep -E "if \(|for \(|while \(|switch \(" /mnt/c/git/wayfarer/src/Services/GameFacade.cs | wc -l
# Should be near 0

# Run full test suite
cd /mnt/c/git/wayfarer/src && dotnet test
```

---

## WORK PACKET #8: Integration Testing

**Assigned To:** Change Validator Agent
**Purpose:** Verify EVERYTHING works after refactoring
**Deadline:** 2 hours
**Dependencies:** All previous work packets

### Required Tests:
1. Start game without errors
2. Move between locations
3. Start conversation with NPC
4. Play cards in conversation
5. Accept letter obligation
6. View obligation queue
7. Deliver letter
8. Receive payment

### Validation Checklist:
- [ ] Provides screenshots of each test
- [ ] Shows console output (no errors)
- [ ] Confirms all 8 tests pass
- [ ] Performance metrics (startup time)

### Required Evidence:
```bash
# Build without errors
cd /mnt/c/git/wayfarer/src && dotnet build

# Run and screenshot each feature
ASPNETCORE_URLS="http://localhost:5099" dotnet run
```

Screenshot requirements:
1. Main game screen
2. Conversation with cards visible
3. Obligation queue with letter
4. Successful delivery message

---

## Assignment Schedule

### Phase 1: Analysis (Parallel)
- **WP#1**: LocationSubsystem Analysis → Content Integrator
- **WP#3**: ConversationSubsystem Analysis → Content Integrator  
- **WP#5**: ObligationSubsystem Analysis → Content Integrator

### Phase 2: Implementation (After Analysis Validated)
- **WP#2**: LocationSubsystem Implementation → Systems Architect
- **WP#4**: ConversationSubsystem Implementation → Systems Architect
- **WP#6**: ObligationSubsystem Implementation → Systems Architect

### Phase 3: Refactoring
- **WP#7**: GameFacade Refactoring → Systems Architect

### Phase 4: Validation
- **WP#8**: Integration Testing → Change Validator

---

## Validation Protocol

### Before Accepting Any Work:
1. Run ALL validation commands
2. Check for red flags
3. Test functionality in-game
4. Require screenshots as proof
5. Verify old code deleted

### If Agent Claims Completion:
1. Ask for specific line numbers
2. Ask for grep output
3. Ask for build output
4. Ask for screenshots
5. Run validation commands yourself

### Common Agent Lies:
- "I've implemented all methods" → Check for empty methods
- "It compiles fine" → Run dotnet build yourself
- "The old code is deleted" → Check if file still exists
- "All tests pass" → Run tests yourself
- "It's fully functional" → Test in-game with Playwright

---

## Escalation Process

If agent fails validation:
1. Point out specific failures
2. Require concrete fixes
3. Re-run ALL validations
4. Do not accept partial work
5. Demand complete implementation

Remember: **NO FALLBACKS, NO COMPATIBILITY LAYERS, NO TODOS**

Every piece of work must be COMPLETE and VERIFIED before acceptance.