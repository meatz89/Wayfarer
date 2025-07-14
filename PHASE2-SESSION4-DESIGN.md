# Phase 2 Session 4: Renewable Contract Generation Design

## Current System Analysis

### NPCs (npcs.json)
- **9 NPCs** with different roles: Workshop Master, Market Trader, etc.
- **contractCategories field**: ["Standard", "Rush"], ["Craft"], etc.
- **Currently not parsed** by NPCParser.cs

### Contract Templates (contracts.json)
- **4 contract types**: Rush (15 coins, 1 day), Standard (8 coins, 3 days), Craft (12 coins, 2 days), Exploration (6 coins, 5 days)
- **Detailed requirements**: Equipment categories, destinations, transactions
- **Strategic variety**: High risk/reward vs steady income

### Current ContractSystem.cs
- **Contract completion logic**: Time validation, reputation tracking
- **Simple contract generation**: GenerateContract() creates basic template
- **No NPC-based generation**: Contracts not linked to NPC specialties

## Implementation Plan

### 1. Enhance NPC Class and Parser
**Goal**: NPCs store and use their contract categories

**Changes**:
- Add `List<string> ContractCategories` to NPC.cs
- Update NPCParser.cs to parse contractCategories from JSON
- Map string categories to ContractCategory enum values

### 2. Create ContractGenerator Class
**Goal**: Generate contracts from templates based on NPC categories

**Architecture**:
```csharp
public class ContractGenerator
{
    private readonly List<Contract> _contractTemplates;
    private readonly List<NPC> _npcs;
    private readonly GameWorld _gameWorld;
    
    public List<Contract> GenerateRenewableContracts(NPC npc, int currentDay)
    public Contract CreateContractFromTemplate(Contract template, NPC npc, int currentDay)
}
```

**Logic**:
- Find contract templates matching NPC's contractCategories
- Generate unique contract instances with appropriate deadlines
- Randomize requirements within template constraints
- Set payment based on difficulty and NPC reputation

### 3. Enhance ContractSystem
**Goal**: Daily contract refresh from NPCs

**New Methods**:
```csharp
public void RefreshDailyContracts()
public List<Contract> GetAvailableContractsFromNPC(string npcId)
public bool AddGeneratedContract(Contract contract)
```

**Daily Refresh Logic**:
1. Get all NPCs with contract categories
2. Generate 1-2 contracts per NPC based on their categories
3. Add contracts to available pool with appropriate scheduling
4. Remove expired contracts that haven't been accepted

### 4. NPC-Contract Mapping Strategy
**Goal**: Link contract categories to strategic gameplay

**Mapping**:
- **Workshop Master** → Craft contracts (requires Trade Tools, workshop access)
- **Market Trader** → Standard/Rush contracts (reliable income vs high pressure)
- **Logger, Herb Gatherer** → Standard/Exploration contracts (resource-based)
- **Trade Captain** → Rush/Exploration contracts (time-critical, long-distance)

### 5. Contract Template Enhancement
**Goal**: Templates create meaningful strategic variety

**Rush Contracts** (1 day, 15 coins):
- Require climbing equipment (mountain routes)
- High profit but equipment investment vs time pressure
- Create scheduling conflicts with equipment commissioning

**Standard Contracts** (3 days, 8 coins):
- Use cart-compatible routes (reliable but lower profit)
- Steady income for basic progression
- Lower risk, consistent availability

**Craft Contracts** (2 days, 12 coins):
- Require Trade Tools and workshop access
- Medium profit with inventory slot pressure (large items)
- Link to equipment commissioning system

**Exploration Contracts** (5 days, 6 coins):
- Require Navigation Tools (wilderness routes)
- Lower immediate profit but discovery potential
- Long-term investment strategy

## Success Criteria

### Technical Validation
- ✅ NPCs parse contractCategories from JSON correctly
- ✅ ContractGenerator creates valid contracts from templates
- ✅ Daily refresh adds 1-2 contracts per NPC per day
- ✅ Contract requirements are satisfiable with available items/routes

### Strategic Validation
- ✅ Equipment investment vs immediate income creates optimization tension
- ✅ Different NPCs offer distinct strategic pathways
- ✅ Contract variety supports multiple viable player strategies
- ✅ Mathematical constraints (slots/stamina/time) affect contract selection

### Integration Validation
- ✅ Contract generation works with existing repository patterns
- ✅ UI can display generated contracts appropriately
- ✅ Contract completion logic works with generated contracts
- ✅ Player progression creates access to better contract opportunities

## Implementation Sequence

1. **NPCParser Enhancement** - Parse contractCategories field
2. **NPC Class Update** - Add ContractCategories property
3. **ContractGenerator Creation** - Template-based contract generation
4. **ContractSystem Enhancement** - Daily refresh and NPC-based queries
5. **Integration Testing** - Verify end-to-end contract flow
6. **Strategic Validation** - Confirm optimization pressure exists

This design maintains the POC principle that mathematical constraints drive strategic decisions while providing renewable content that supports different playstyles.