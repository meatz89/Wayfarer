# Conversation Architecture Refactor Plan

## Goal
Create a robust, extensible conversation architecture that:
- Eliminates ALL Dictionary usage (97 files affected)
- Supports multiple conversation types with type-specific data
- Ensures parsers fully resolve data at parse time
- Provides strongly-typed models throughout

## Core Principles
1. **NO DICTIONARIES** - Only List<T> of strongly-typed objects
2. **PARSERS MUST PARSE** - Convert JSON to complete domain objects, no string IDs
3. **TYPE-SPECIFIC CONTEXTS** - Each conversation type has its own context class
4. **SINGLE SOURCE OF TRUTH** - One model per concept, no duplicates

## Work Packets

### Packet 1: Core Resource System (COMPLETE)
- [x] Create ResourceAmount class to replace Dictionary<ResourceType, int>
- [x] Refactor ExchangeData to use List<ResourceAmount>
- [x] Delete duplicate ResourceExchange class
- [x] Update all resource-related methods

### Packet 2: Fix Exchange Data Population (COMPLETE)
- [x] Update ConversationSession.StartExchange to use List<ResourceAmount>
- [x] Fix ConversationOrchestrator routing for Commerce conversations
- [x] Ensure ConversationFacade calls CreateExchangeSession for Commerce
- [x] Populate ExchangeData at session creation with proper resource lists

### Packet 3: Update UI Layer (COMPLETE)
- [x] Update ConversationContent.razor.cs to use List<ResourceAmount>
- [x] Fix GetExchangeCostDisplay and GetExchangeRewardDisplay
- [x] Remove FormatResourceDict helper
- [x] Test display shows "10 coins → 1 food"

### Packet 4: Type-Specific Conversation Contexts
- [ ] Create base ConversationContext class
- [ ] Create CommerceContext with exchange-specific data
- [ ] Create PromiseContext with letter-specific data
- [ ] Create DeliveryContext with obligation-specific data
- [ ] Replace god-object CardContext

### Packet 5: Systematic Dictionary Elimination
- [ ] Replace all Dictionary<ConnectionType, int> with List<TokenAmount>
- [ ] Replace all Dictionary<string, object> with typed properties
- [ ] Update all 97 affected files
- [ ] Ensure no new dictionaries are introduced

### Packet 6: Parser Architecture Overhaul
- [ ] Create ParserContext with GameWorld access
- [ ] Update all parsers to take ParserContext
- [ ] Ensure complete object resolution at parse time
- [ ] Remove all runtime string lookups

## Implementation Order
1. Fix immediate exchange display issue (Packets 1-3)
2. Create extensible architecture (Packet 4)
3. Systematic cleanup (Packets 5-6)

## Success Criteria
- Exchange cards display correct costs/rewards
- No Dictionary<> usage anywhere in codebase
- All conversation types have specific context classes
- Parser fully resolves all data at parse time
- Clean separation between parsing and runtime