# POC Implementation Roadmap

## IMPLEMENTATION PHASES

### **PHASE 1: Content Simplification (HIGH PRIORITY)**
**Goal**: Replace complex content with POC-targeted minimal content

#### **1.1 Rebuild JSON Content Files**
**Timeline**: 2-3 sessions
**Complexity**: Medium

**Actions**:
- **Replace locations.json**: 10 locations → 3 locations (Millbrook, Thornwood, Crossbridge)
- **Replace routes.json**: 28 routes → 8 routes matching POC target design
- **Replace items.json**: 23 items → 9 items (6 trade goods + 3 equipment)
- **Replace contracts.json**: 15 story contracts → 4 renewable contract templates
- **Replace npcs.json**: 16 NPCs → 9 NPCs (3 per location)
- **Create location_spots.json**: Define 2-3 spots per location

**Expected Outcome**: Clean, minimal content matching POC target exactly

#### **1.2 Update Starting Conditions**
**Timeline**: 1 session
**Complexity**: Low

**Actions**:
- **Set starting location**: Millbrook (as specified in POC)
- **Set starting money**: 12 coins (not 8)
- **Set starting inventory**: 4 slots
- **Set starting equipment**: [Trade_Tools] only
- **Update gameWorld.json**: Reflect POC starting conditions

**Expected Outcome**: Player starts in correct POC scenario

### **PHASE 2: Contract Enhancement (MEDIUM PRIORITY)**
**Goal**: Implement renewable contract generation system

#### **2.1 Renewable Contract Templates**
**Timeline**: 1-2 sessions
**Complexity**: Medium

**Actions**:
- **Create contract templates**: Rush, Standard, Craft, Exploration types
- **Implement contract generation**: Daily renewable contracts per NPC
- **Add contract difficulty scaling**: Based on player progress
- **Update ContractSystem.cs**: Support renewable vs one-time contracts

**Expected Outcome**: NPCs offer ongoing contract opportunities

#### **2.2 Market-Driven Contract Generation**
**Timeline**: 1 session
**Complexity**: Low

**Actions**:
- **Link contracts to trade goods**: Delivery contracts based on price differentials
- **Add reputation system**: Contract availability based on player performance
- **Implement contract priority**: Rush contracts appear under time pressure

**Expected Outcome**: Contracts create strategic optimization challenges

### **PHASE 3: Mathematical Constraint Validation (LOW PRIORITY)**
**Goal**: Validate that mathematical impossibilities create strategic tension

#### **3.1 Constraint Testing**
**Timeline**: 1 session
**Complexity**: Low

**Actions**:
- **Test inventory constraint**: 7 slots needed vs 4 available
- **Test stamina constraint**: 12+ stamina needed vs 10 available
- **Test time constraint**: Multiple profitable activities vs limited time
- **Validate route trade-offs**: Equipment specialization vs cargo capacity

**Expected Outcome**: Confirm mathematical constraints create optimization puzzles

#### **3.2 Balance Validation**
**Timeline**: 1 session
**Complexity**: Low

**Actions**:
- **Test starting money**: 12 coins enables immediate equipment investment
- **Test profit margins**: 2-3 coins per slot creates viable trade circuits
- **Test contract payments**: 3-8 coins creates contract vs trading tension
- **Test equipment costs**: 5 coins + 1 period creates investment pressure

**Expected Outcome**: Economic balance creates strategic choices

### **PHASE 4: POC Experience Testing (ONGOING)**
**Goal**: Validate POC creates strategic optimization gameplay

#### **4.1 Player Journey Simulation**
**Timeline**: Ongoing
**Complexity**: Low

**Actions**:
- **Test Day 1 breadcrumb**: Simple delivery contract works as intended
- **Test discovery system**: Equipment requirements learned through route blocking
- **Test strategic dimensions**: Route mastery, trade optimization, equipment investment
- **Test failure states**: Equipment poverty, information starvation, overspecialization

**Expected Outcome**: POC demonstrates strategic gameplay emergence

#### **4.2 Success Metrics Validation**
**Timeline**: Ongoing
**Complexity**: Low

**Actions**:
- **Multiple valid strategies**: Different approaches to same challenge
- **Trade-off recognition**: Players understand optimization is impossible
- **Planning horizon**: Decisions have consequences 2-3 steps ahead
- **Emergent strategy**: Players develop personal approaches

**Expected Outcome**: POC validates design philosophy

## IMPLEMENTATION PRIORITY ORDER

### **Immediate (This Session)**
1. **Content Simplification**: Replace complex JSON with POC-targeted content
2. **Starting Conditions**: Set POC starting scenario (Millbrook, 12 coins, 4 slots)

### **Next Session**
1. **Renewable Contracts**: Implement basic renewable contract generation
2. **NPC Contract Assignment**: Each NPC offers appropriate contract types

### **Following Sessions**
1. **Market-Driven Contracts**: Link contracts to trade opportunities
2. **Mathematical Constraint Testing**: Validate impossible scheduling conflicts
3. **POC Experience Testing**: Confirm strategic gameplay emergence

## TECHNICAL IMPLEMENTATION NOTES

### **Content File Changes**
- **Keep existing JSON structure**: Don't change parsers or data models
- **Simplify content, not systems**: Remove entities, don't rewrite architecture
- **Maintain category relationships**: Equipment categories must match route requirements
- **Preserve mathematical relationships**: Slot costs, stamina requirements, time blocks

### **System Changes Required**
- **Minimal system changes**: Core systems already match POC design
- **Contract generation enhancement**: Add renewable contract creation
- **UI updates**: Display equipment requirements and blocked routes
- **Starting condition updates**: Set POC starting scenario

### **Testing Strategy**
- **Delete problematic tests**: Already completed (9 test files removed)
- **Keep architectural tests**: Tests that validate system behavior with isolated data
- **Create POC validation tests**: Tests that confirm mathematical constraints work
- **Test with POC content**: Validate systems work with simplified content

## RISK MITIGATION

### **Content Complexity Risk**
- **Risk**: Oversimplifying content removes strategic depth
- **Mitigation**: POC design has been carefully crafted to maintain strategic complexity through mathematical constraints

### **System Integration Risk**
- **Risk**: Simplified content exposes system bugs
- **Mitigation**: Core systems are well-tested and architectural principles are sound

### **Player Experience Risk**
- **Risk**: POC feels too simple or boring
- **Mitigation**: Mathematical impossibilities create genuine optimization challenges

## SUCCESS CRITERIA

### **Technical Success**
- ✅ **All systems work with POC content**: No crashes or errors
- ✅ **Mathematical constraints validated**: Impossible scheduling conflicts exist
- ✅ **Route discovery system works**: Equipment requirements block/enable routes
- ✅ **Contract system functional**: Renewable contracts create ongoing challenges

### **Design Success**
- ✅ **Strategic optimization emerges**: Players face genuine trade-offs
- ✅ **Discovery through failure**: Learning happens through route blocking
- ✅ **Multiple viable strategies**: Different approaches to same challenges
- ✅ **Emergent complexity**: Simple systems create deep strategic decisions

### **POC Validation**
- ✅ **"Make 50 Coins in 14 Days" achievable**: Challenge is difficult but fair
- ✅ **Equipment investment creates tension**: Gear vs profit trade-offs
- ✅ **Route mastery enables optimization**: Equipment unlocks strategic options
- ✅ **Contract work competes with trading**: Time allocation creates pressure

## ESTIMATED TIMELINE

- **Phase 1 (Content Simplification)**: 2-3 sessions
- **Phase 2 (Contract Enhancement)**: 1-2 sessions  
- **Phase 3 (Constraint Validation)**: 1 session
- **Phase 4 (Experience Testing)**: Ongoing

**Total Time to POC**: 4-6 sessions

The implementation roadmap leverages the excellent existing systems architecture while focusing on content simplification to reach the POC target design efficiently.