# Standing Obligations Effects Implementation Plan

## PHASE 3: FORCED LETTER GENERATION & OBLIGATION EFFECTS

### **OVERVIEW**
This phase transforms standing obligations from passive display elements into active gameplay modifiers that fundamentally change how the letter queue behaves. The goal is to make each obligation create unique strategic situations through forced letters, queue modifications, and permanent behavioral changes.

### **CORE DESIGN PRINCIPLES**
1. **Emergent Conflicts**: Multiple obligations create natural tensions between competing demands
2. **Meaningful Consequences**: Breaking obligations has real relationship costs, not arbitrary penalties
3. **Queue-Centric Design**: All effects focus on queue behavior, entry positions, and manipulation costs
4. **Discovery Through Play**: Players learn obligation interactions through experimentation

---

## **IMPLEMENTATION TASKS**

### **Task 1: Forced Letter Generation System** (High Priority)
**File**: `src/GameState/StandingObligationManager.cs`

**Purpose**: Generate letters automatically based on obligation schedules
- **Shadow Forced**: Every 3 days generates shadow letter (danger, high pay)
- **Patron Monthly**: Every 30 days generates patron resource package letter
- **Trust Obligations**: Generate relationship maintenance letters

**Key Methods**:
```csharp
public List<Letter> ProcessDailyObligations(int currentDay)
public Letter GenerateForcedLetter(StandingObligation obligation)
public bool ShouldGenerateForcedLetter(StandingObligation obligation, int currentDay)
```

**Integration**: Called by TimeManager during daily advancement

---

### **Task 2: Queue Entry Position Effects** (High Priority)
**File**: `src/GameState/LetterQueueManager.cs` 

**Purpose**: Modify where new letters enter the queue based on obligations
- **NoblesPriority**: Noble letters enter at slot 5 instead of 8
- **CommonFolksPriority**: Common letters enter at slot 6 instead of 8
- **PatronJumpToTop**: Patron letters jump to slots 1-3 on arrival

**Key Methods**:
```csharp
public int CalculateEntryPosition(Letter letter)  // Uses obligation effects
public void AddLetterWithObligationEffects(Letter letter)  // Enhanced version
```

**Effect Flow**: New Letter → Check Obligations → Calculate Position → Place in Queue

---

### **Task 3: Payment Modification Effects** (High Priority)
**File**: `src/GameState/LetterQueueManager.cs`

**Purpose**: Modify letter payment amounts based on obligations
- **TradeBonus**: Trade letters get +10 coins
- **ShadowTriplePay**: Shadow letters pay triple amount
- **Patron Benefits**: Monthly resource packages

**Key Methods**:
```csharp
public int CalculateModifiedPayment(Letter letter)
public void ApplyPaymentBonuses(Letter letter)
```

**Integration**: Applied when letter is delivered, not when added to queue

---

### **Task 4: Obligation Restrictions** (High Priority)
**File**: `src/GameState/LetterQueueManager.cs`

**Purpose**: Block certain queue manipulation actions based on obligations
- **NoTradePurge**: Cannot purge trade letters
- **TrustSkipDoubleCost**: Skipping trust letters costs 2x tokens
- **NoNobleRefusal**: Cannot refuse noble letters (forced acceptance)

**Key Methods**:
```csharp
public bool IsActionForbidden(string actionType, Letter letter, out string reason)
public int CalculateActionCost(string actionType, Letter letter)
```

**Integration**: Called before all queue manipulation actions

---

### **Task 5: Daily Obligation Processing** (High Priority) 
**File**: `src/GameState/TimeManager.cs`

**Purpose**: Process obligations during daily time advancement
- Check for forced letter generation
- Update obligation day counters
- Apply daily effects (patron letter advancement)

**Key Methods**:
```csharp
public void ProcessDailyObligations()
```

**Integration**: Called during `AdvanceToNextDay()` after deadline processing

---

### **Task 6: Enhanced Queue Integration** (Medium Priority)
**File**: `src/GameState/LetterQueueManager.cs`

**Purpose**: Fully integrate obligation effects into existing queue methods
- Enhance `AddLetterWithObligationEffects` method
- Update delivery methods to apply payment bonuses
- Integrate restriction checking into all actions

**Enhancement Targets**:
- `AddLetterToFirstEmpty()` → Use obligation position calculation
- `DeliverLetter()` → Apply payment bonuses
- `TryPurgeLetter()` → Check restrictions
- `TrySkipDeliver()` → Check cost modifications

---

### **Task 7: Forced Letter Templates** (Medium Priority)
**File**: `src/Content/Templates/forced_letters.json`

**Purpose**: Create templates for automatically generated letters
- Shadow obligation letters (dangerous assignments)
- Patron monthly packages (resources and demands)
- Trust maintenance letters (relationship upkeep)

**Template Structure**:
```json
{
  "obligation_type": "ShadowForced",
  "letter_templates": [
    {
      "sender_name": "The Fence",
      "recipient_name": "Midnight Contact",
      "base_payment": 25,
      "deadline_range": [1, 3],
      "description": "Dangerous package delivery"
    }
  ]
}
```

---

### **Task 8: Violation Tracking** (Medium Priority)
**File**: `src/GameState/StandingObligation.cs`

**Purpose**: Track when obligations are violated and apply consequences
- Count violations per obligation
- Apply relationship damage for violations
- Display violation warnings in UI

**Key Properties**:
```csharp
public int ViolationCount { get; set; }
public DateTime LastViolationDate { get; set; }
public List<string> ViolationReasons { get; set; }
```

---

### **Task 9: Queue Conflict Detection** (Low Priority)
**File**: `src/GameState/StandingObligationManager.cs`

**Purpose**: Detect when multiple obligations create impossible situations
- Noble priority vs Shadow forced timing
- Trade purge restriction vs queue overflow
- Multiple payment bonuses on same letter

**Conflict Resolution**: Player choice, not automatic resolution

---

### **Task 10: Comprehensive Testing** (Low Priority)
**File**: `Wayfarer.Tests/ObligationEffectsTests.cs`

**Purpose**: Test all obligation effects and interactions
- Forced letter generation timing
- Queue position calculations
- Payment modifications
- Restriction enforcement
- Conflict scenarios

---

## **IMPLEMENTATION SEQUENCE**

### **Day 1: Core Forced Letter System**
1. Implement forced letter generation in StandingObligationManager
2. Add daily processing to TimeManager
3. Create basic forced letter templates

### **Day 2: Queue Position Effects**
1. Implement entry position calculation
2. Enhance AddLetterWithObligationEffects method
3. Update UI to show position changes

### **Day 3: Payment & Restrictions**
1. Add payment modification system
2. Implement queue action restrictions
3. Integrate restriction checking

### **Day 4: Integration & Testing**
1. Full queue integration
2. Violation tracking
3. Comprehensive testing

---

## **SUCCESS CRITERIA**

### **Functional Requirements**
- [ ] Shadow obligations generate letters every 3 days
- [ ] Noble letters enter at slot 5 with Noble's Courtesy
- [ ] Trade letters get +10 coins with Merchant's Priority
- [ ] Cannot purge trade letters with trade restrictions
- [ ] Trust letters cost double to skip with trust obligations

### **Quality Requirements**
- [ ] All effects visible in UI with clear messaging
- [ ] No performance degradation with multiple obligations
- [ ] Proper error handling for impossible situations
- [ ] Full test coverage for all obligation effects

### **Design Validation**
- [ ] Multiple obligations create interesting conflicts
- [ ] Player retains agency despite restrictions
- [ ] Natural consequences emerge from simple rules
- [ ] Queue manipulation stays strategic, not automated

---

## **ARCHITECTURAL NOTES**

### **Dependency Flow**
```
TimeManager → StandingObligationManager → Letter Generation
Letter → LetterQueueManager → Queue Position Calculation
Queue Actions → Obligation Restrictions → Allow/Block
```

### **Data Flow**
```
Daily Advance → Check Obligations → Generate Letters → Apply Effects → Update Queue
```

### **UI Integration**
- Obligation effects visible in queue display
- Forced letter warnings in obligations panel  
- Restriction messages in action buttons
- Payment bonuses shown in letter details

This plan ensures that standing obligations become a central strategic element that reshapes queue gameplay rather than just passive modifiers.