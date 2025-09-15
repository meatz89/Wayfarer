# Card Immutability Refactoring Plan

## Problem Statement
The conversation system violates the instance/template pattern. CardInstance duplicates 15 properties from ConversationCard instead of holding a reference to its immutable template. Additionally, ConversationCard templates are fully mutable, allowing dangerous runtime modifications.

## Architectural Goals
1. **ConversationCard becomes immutable** - Templates are blueprints that never change
2. **CardInstance holds template reference** - Instances reference their template, storing only instance-specific data (XP, InstanceId, Context)
3. **Clear separation of concerns** - Templates define static behavior, instances track runtime state

## Implementation Phases

### Phase 1: Make ConversationCard Immutable
- Change all properties from `{ get; set; }` to `{ get; init; }`
- Convert `List<T>` collections to `IReadOnlyList<T>`
- Ensure no property can be modified after construction

### Phase 2: Refactor CardInstance Architecture
- Add `public ConversationCard Template { get; init; }` property
- Remove 15 duplicate properties (Id, Description, Focus, Difficulty, etc.)
- Keep only instance-specific properties: InstanceId, XP, Context, IsPlayable, SourceContext
- Add delegating properties for API compatibility: `public string Id => Template.Id;`

### Phase 3: Update Parsers and Factories
- Refactor ConversationCardParser to use object initializer syntax
- Update PackageLoader to create immutable cards in one step
- Fix SessionCardDeck.CreateFromInstances() to preserve XP without duplicating properties

### Phase 4: Fix Downstream Dependencies
- Update all code that accesses CardInstance properties
- Ensure computed properties (Persistence, Level) still work correctly
- Fix NPCDeckViewer to work with new architecture

## Benefits
- **Memory efficiency**: 90% reduction in CardInstance memory footprint
- **Thread safety**: Immutable templates can be safely shared
- **Single source of truth**: No duplicate data to keep synchronized
- **Architectural clarity**: Clear template/instance separation

## Technical Details

### Properties to Remove from CardInstance
1. Id
2. Description
3. SuccessType
4. FailureType
5. ExhaustType
6. CardType
7. TokenType
8. Focus
9. Difficulty
10. RapportThreshold
11. RequestId
12. MinimumTokensRequired
13. DialogueFragment
14. VerbPhrase
15. _basePersistence (keep private for computation)

### Properties to Keep in CardInstance
- InstanceId (unique identifier)
- XP (progression data)
- Context (runtime state)
- IsPlayable (runtime flag)
- SourceContext (creation context)
- Template (reference to immutable template)

### Computed Properties
- Persistence (combines Template.Persistence + level effects)
- Level (computed from XP via GameRules)
- IgnoresFailureListen (computed from level effects)

## Files Affected
- `/src/GameState/ConversationCard.cs` - Make immutable
- `/src/GameState/CardInstance.cs` - Refactor to reference template
- `/src/Content/ConversationCardParser.cs` - Use object initializers
- `/src/Content/PackageLoader.cs` - Create immutable cards
- `/src/GameState/SessionCardDeck.cs` - Fix CreateFromInstances()
- `/src/Pages/Components/NPCDeckViewer.razor` - Update for new architecture
- 30+ other files that access CardInstance properties

## Testing Strategy
1. Verify cards cannot be modified after creation
2. Confirm CardInstance properly delegates to template
3. Test XP persistence across conversations
4. Validate computed properties still work
5. Run full conversation integration tests

## Success Criteria
- Zero property duplication between CardInstance and ConversationCard
- ConversationCard properties cannot be modified after construction
- All existing functionality preserved
- Memory usage reduced for CardInstance objects
- Code is cleaner and more maintainable