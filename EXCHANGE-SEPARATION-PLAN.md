# Exchange System Separation Implementation Plan

## Executive Summary
Separate the Exchange system from the Conversation system to enable clean integration with the Travel system. Exchanges will become a standalone subsystem with dedicated data models, UI components, and processing logic.

## Current State Analysis

### Problem
- Exchanges are currently embedded within the Conversation system using ConversationCard
- ConversationType.Commerce is handled as a special case within conversation logic
- Exchange cards share the same data structure as conversation cards (focus, rapport, etc.)
- This coupling prevents clean integration of the Travel system which needs similar card mechanics

### Solution
Complete architectural separation of Exchange and Conversation systems while maintaining identical UI appearance.

## Work Packet Distribution

### Packet 1: Data Models & Core Types
**Agent**: systems-architect
**Scope**: Design and implement core exchange data structures

Files to create:
- `/src/GameState/ExchangeCard.cs` - Exchange card model
- `/src/GameState/ExchangeSession.cs` - Exchange session state
- `/src/GameState/ExchangeContext.cs` - Exchange context for UI
- `/src/GameState/ExchangeType.cs` - Exchange type enum
- `/src/GameState/ExchangeCost.cs` - Cost structure
- `/src/GameState/ExchangeReward.cs` - Reward structure

Key decisions:
- No inheritance from ConversationCard
- Direct resource mechanics (no focus/flow/rapport)
- Token requirements as prerequisites
- Support for multiple costs and rewards

### Packet 2: Exchange Subsystem Architecture
**Agent**: systems-architect
**Scope**: Create Exchange subsystem following facade pattern

Files to create:
- `/src/Subsystems/Exchange/ExchangeFacade.cs` - Public interface
- `/src/Subsystems/Exchange/ExchangeOrchestrator.cs` - Core logic
- `/src/Subsystems/Exchange/ExchangeValidator.cs` - Validation logic
- `/src/Subsystems/Exchange/ExchangeProcessor.cs` - Execute trades
- `/src/Subsystems/Exchange/ExchangeInventory.cs` - Track available exchanges

Key architecture:
- Facade pattern matching other subsystems
- Clear separation from Conversation subsystem
- Integration points with Resource subsystem
- Token validation through Token subsystem

### Packet 3: UI Components & Styling
**Agent**: content-integrator
**Scope**: Create Exchange UI maintaining visual consistency

Files to create:
- `/src/Pages/Components/ExchangeContent.razor` - Main exchange screen
- `/src/Pages/Components/ExchangeContent.razor.cs` - Code-behind
- `/src/wwwroot/css/exchange-screen.css` - Exchange-specific styles
- `/src/Pages/Components/ExchangeCardDisplay.razor` - Card rendering

UI Requirements:
- Copy visual structure from exchange-conversation.html mockup
- Remove conversation mechanics (flow bar, rapport, patience)
- Keep card-based selection pattern
- Display costs and rewards clearly
- Maintain SPEAK/LEAVE action buttons

### Packet 4: Content Parsing & Loading
**Agent**: wayfarer-design-auditor
**Scope**: Implement exchange content pipeline

Files to create:
- `/src/Content/ExchangeCardParser.cs` - Parse exchange definitions
- `/src/Content/DTOs/ExchangeCardDTO.cs` - Data transfer object
- `/src/Content/Validation/Validators/ExchangeValidator.cs` - Validation

Updates needed:
- Modify PackageLoader to handle exchanges separately
- Update core_game_package.json structure
- Separate "exchanges" array from "cards" array
- Validate exchange data independently

### Packet 5: NPC & GameWorld Updates
**Agent**: game-mechanics-designer
**Scope**: Update NPC structure for separate exchange system

Files to modify:
- `/src/GameState/NPC.cs` - Change ExchangeDeck to ExchangeCards list
- `/src/GameState/GameWorld.cs` - Add NPCExchangeCards dictionary
- `/src/Content/SkeletonGenerator.cs` - Generate exchange skeletons

Key changes:
- Remove CardType.Exchange from ConversationCard enum
- NPCs store List<ExchangeCard> instead of exchange deck
- GameWorld tracks exchange cards separately
- Skeleton generation for missing exchanges

### Packet 6: Navigation & Integration
**Agent**: content-integrator
**Scope**: Wire Exchange system into game navigation

Files to modify:
- `/src/Pages/GameScreen.razor(.cs)` - Add Exchange screen mode
- `/src/Pages/Components/LocationContent.razor(.cs)` - Add exchange selection
- `/src/Services/GameFacade.cs` - Add exchange methods
- `/src/ServiceConfiguration.cs` - Register exchange services

Integration points:
- Add ScreenMode.Exchange enum value
- GameScreen.StartExchange(npcId) method
- CurrentExchangeContext property
- Update screen switching logic
- Quick Exchange option in location UI

### Packet 7: Migration & Cleanup
**Agent**: change-validator
**Scope**: Remove exchange logic from Conversation system

Files to modify:
- `/src/Subsystems/Conversation/ConversationOrchestrator.cs` - Remove exchange handling
- `/src/Subsystems/Conversation/ExchangeHandler.cs` - Delete file
- `/src/GameState/Contexts/CommerceContext.cs` - Delete file
- `/src/GameState/ConversationType.cs` - Remove Commerce value

Cleanup tasks:
- Remove all ConversationType.Commerce references
- Delete ExchangeHandler from Conversation subsystem
- Remove exchange-specific code from ConversationSession
- Update all switch statements handling Commerce

### Packet 8: Testing & Validation
**Agent**: change-validator
**Scope**: Comprehensive testing of separated systems

Test scenarios:
- Exchange card parsing and validation
- Resource cost validation
- Token requirement checking
- UI navigation to exchange screen
- Exchange execution and resource updates
- NPC exchange availability
- Integration with existing systems

Files to create:
- `/src/Tests/ExchangeCardParserTest.cs`
- `/src/Tests/ExchangeOrchestratorTest.cs`
- `/src/Tests/ExchangeValidatorTest.cs`

## Implementation Sequence

### Phase 1: Foundation (Packets 1-2)
1. Create data models
2. Build Exchange subsystem
3. No breaking changes yet

### Phase 2: Content Pipeline (Packets 3-4)
1. Create parsing infrastructure
2. Update content loading
3. Maintain backward compatibility

### Phase 3: UI Layer (Packet 5)
1. Create ExchangeContent component
2. Copy styling from mockup
3. Test standalone UI

### Phase 4: Integration (Packet 6)
1. Wire into navigation
2. Update GameFacade
3. Connect all systems

### Phase 5: Migration (Packet 7)
1. Remove old exchange code
2. Clean up Conversation system
3. Update all references

### Phase 6: Validation (Packet 8)
1. Run all tests
2. Manual testing
3. Fix any issues

## Success Criteria

1. **Complete Separation**: No shared code between Exchange and Conversation
2. **Visual Consistency**: Exchange UI identical to mockup
3. **Clean Architecture**: Exchange follows facade pattern
4. **Type Safety**: Separate types prevent mixing systems
5. **Backward Compatible**: Existing content continues working
6. **Travel Ready**: Clean integration point for Travel system

## Risk Mitigation

1. **Data Migration**: Keep old system working during transition
2. **UI Consistency**: Use exact mockup as reference
3. **Testing Coverage**: Test each phase before proceeding
4. **Rollback Plan**: Git branches for each phase

## Future Considerations

1. **Bartering System**: Exchange can support negotiation
2. **Reputation Effects**: Exchanges affected by standing
3. **Dynamic Pricing**: Market conditions affect costs
4. **Travel Integration**: Path cards use similar mechanics
5. **Quest Rewards**: Exchanges as quest completion

This plan ensures clean separation while maintaining game functionality throughout the implementation process.