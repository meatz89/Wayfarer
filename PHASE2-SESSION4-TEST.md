# Phase 2 Session 4: Contract Generation Testing

## Test Plan

### 1. Test NPC ContractCategories Parsing
- **Goal**: Verify NPCs load their contract categories from JSON
- **Test**: Create NPC with contractCategories = ["Standard", "Rush"]
- **Expected**: NPC.ContractCategories contains ["Standard", "Rush"]

### 2. Test ContractGenerator Creation
- **Goal**: Verify ContractGenerator can create contracts from templates
- **Test**: Generate contract from "rush_delivery" template with "Market Trader" NPC
- **Expected**: New contract with unique ID, NPC-specific description, appropriate deadline

### 3. Test ContractSystem Daily Refresh
- **Goal**: Verify daily contract refresh generates new contracts
- **Test**: Call RefreshDailyContracts() and check available contracts
- **Expected**: New contracts appear from NPCs with contract categories

### 4. Test Contract Categories Mapping
- **Goal**: Verify contract generation matches NPC categories
- **Test**: Workshop Master (["Craft"]) should generate craft contracts
- **Expected**: Generated contracts match NPC's contract categories

## Implementation Test

### Test 1: NPC ContractCategories Parsing
```csharp
// Test data in npcs.json already has:
// "market_trader": { "contractCategories": ["Standard", "Rush"] }
// "workshop_master": { "contractCategories": ["Craft"] }

// Load NPCs and verify parsing
List<NPC> npcs = gameWorldInitializer.LoadNPCs();
NPC marketTrader = npcs.FirstOrDefault(n => n.ID == "market_trader");
Assert.NotNull(marketTrader);
Assert.Contains("Standard", marketTrader.ContractCategories);
Assert.Contains("Rush", marketTrader.ContractCategories);
```

### Test 2: ContractGenerator Basic Generation
```csharp
// Setup
ContractGenerator generator = new ContractGenerator(contractTemplates, contractRepository);
NPC workshopMaster = npcs.FirstOrDefault(n => n.ID == "workshop_master");

// Generate contracts
List<Contract> contracts = generator.GenerateRenewableContracts(workshopMaster, 1);

// Verify
Assert.NotEmpty(contracts);
Assert.All(contracts, c => c.Id.StartsWith("workshop_master_"));
Assert.All(contracts, c => c.Description.Contains("Workshop Master"));
```

### Test 3: ContractSystem Integration
```csharp
// Setup game world with NPCs
GameWorld gameWorld = testGameWorldInitializer.CreateTestWorld();
ContractSystem contractSystem = new ContractSystem(gameWorld, messageSystem, contractRepository, locationRepository);

// Initial state: no contracts
Assert.Empty(contractRepository.GetAvailableContracts(1, TimeBlocks.Morning));

// Daily refresh
contractSystem.RefreshDailyContracts();

// Verify contracts generated
List<Contract> availableContracts = contractRepository.GetAvailableContracts(1, TimeBlocks.Morning);
Assert.NotEmpty(availableContracts);
```

### Test 4: Contract Category Matching
```csharp
// Test each NPC type generates appropriate contracts
Dictionary<string, string[]> expectedCategories = new() {
    { "workshop_master", new[] { "craft" } },
    { "market_trader", new[] { "standard", "rush" } },
    { "trade_captain", new[] { "rush", "exploration" } }
};

foreach (var (npcId, expectedTypes) in expectedCategories)
{
    List<Contract> npcContracts = contractSystem.GetAvailableContractsFromNPC(npcId);
    Assert.All(npcContracts, contract => 
        expectedTypes.Any(type => contract.Id.Contains(type)));
}
```

## Success Criteria

### Technical Validation
- ✅ NPCs parse contractCategories from JSON correctly
- ✅ ContractGenerator creates valid contracts from templates
- ✅ ContractSystem.RefreshDailyContracts() generates new contracts
- ✅ Contract generation matches NPC categories

### Strategic Validation
- ✅ Different NPCs offer distinct contract types
- ✅ Contract variety creates strategic choices
- ✅ Generated contracts maintain POC balance (Rush: 15 coins/1 day, Standard: 8 coins/3 days)

### Integration Validation
- ✅ Generated contracts work with existing acceptance/completion system
- ✅ UI can display generated contracts appropriately
- ✅ Contract requirements are satisfiable with available items/routes

## Manual Testing Steps

1. **Start game**: Load with POC content
2. **Check initial contracts**: Should see 0 contracts initially
3. **Advance day**: Trigger daily refresh
4. **Verify contract generation**: Should see contracts from different NPCs
5. **Test contract acceptance**: Accept a generated contract
6. **Test contract completion**: Complete an accepted contract
7. **Test daily refresh**: Advance day again, see new contracts

## Expected Results

- **9 NPCs** with contractCategories generate **1-2 contracts each** per day
- **Contract variety**: Rush (1 day, 15 coins), Standard (3 days, 8 coins), Craft (2 days, 12 coins), Exploration (5 days, 6 coins)
- **Strategic pressure**: Different contract types create different optimization challenges
- **Renewable system**: New contracts appear daily, expired contracts removed

This test plan validates that the renewable contract generation system works correctly and integrates with the existing game systems.