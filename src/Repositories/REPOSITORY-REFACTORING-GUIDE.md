# Repository Pattern Refactoring Guide

This guide demonstrates how to migrate from the old repository pattern to the new interface-based pattern with separated business logic.

## Overview of Changes

### 1. Repository Interfaces Created
- `IRepository<T>` - Base interface for CRUD operations
- `IItemRepository` - Specific interface for Item entities
- `INPCRepository` - Specific interface for NPC entities  
- `ILetterTemplateRepository` - Specific interface for LetterTemplate entities

### 2. Base Repository Implementation
- `BaseRepository<T>` - Abstract base class with common CRUD operations
- Includes logging and validation
- Enforces consistent error handling

### 3. Business Logic Extraction
- `NPCService` - Contains NPC-related business logic (time block planning, service availability)
- `LetterGenerationService` - Contains letter generation logic

### 4. Dependency Abstraction
- `IWorldStateAccessor` - Abstracts GameWorld dependency
- `ITimeManager` - Abstracts TimeManager dependency

## Migration Steps

### Step 1: Update Service Registration

In `ServiceConfiguration.cs` or wherever services are registered:

```csharp
// Add the new repository pattern services
services.AddRepositories();

// Or manually register:
services.AddScoped<IWorldStateAccessor, GameWorldStateAccessor>();
services.AddScoped<IItemRepository, ItemRepositoryImpl>();
services.AddScoped<INPCRepository, NPCRepositoryImpl>();
services.AddScoped<ILetterTemplateRepository, LetterTemplateRepositoryImpl>();
services.AddScoped<NPCService>();
services.AddScoped<LetterGenerationService>();
```

### Step 2: Update Existing Code

#### Before (Direct Repository Usage):
```csharp
public class SomeManager
{
    private readonly ItemRepository _itemRepository;
    
    public SomeManager(ItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }
    
    public void DoSomething()
    {
        var item = _itemRepository.GetItemById("123");
        // ...
    }
}
```

#### After (Interface-Based):
```csharp
public class SomeManager
{
    private readonly IItemRepository _itemRepository;
    
    public SomeManager(IItemRepository itemRepository)
    {
        _itemRepository = itemRepository;
    }
    
    public void DoSomething()
    {
        var item = _itemRepository.GetById("123");
        // ...
    }
}
```

### Step 3: Extract Business Logic

#### Before (Business Logic in Repository):
```csharp
public class NPCRepository
{
    public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(string locationId)
    {
        // Complex business logic mixed with data access
        var timeBlockPlan = new List<TimeBlockServiceInfo>();
        var allTimeBlocks = Enum.GetValues<TimeBlocks>();
        var locationNPCs = GetNPCsForLocation(locationId);
        // ... more logic ...
        return timeBlockPlan;
    }
}
```

#### After (Business Logic in Service):
```csharp
public class NPCService
{
    private readonly INPCRepository _repository;
    
    public List<TimeBlockServiceInfo> GetTimeBlockServicePlan(string locationId)
    {
        // Business logic separated from data access
        var locationNPCs = _repository.GetNPCsForLocation(locationId);
        // ... business logic using repository data ...
        return timeBlockPlan;
    }
}
```

### Step 4: Use Facades for Gradual Migration

For a gradual migration, use the provided facades:

```csharp
// In ServiceConfiguration.cs
services.AddScoped<ItemRepository>(provider => 
    RepositoryMigrationHelper.CreateItemRepositoryFacade(provider));
    
services.AddScoped<NPCRepository>(provider => 
    RepositoryMigrationHelper.CreateNPCRepositoryFacade(provider));
```

This allows existing code to continue working while you migrate piece by piece.

## Benefits of the New Pattern

1. **Testability** - Interfaces allow easy mocking in unit tests
2. **Separation of Concerns** - Business logic is separated from data access
3. **Consistency** - All repositories follow the same pattern
4. **Maintainability** - Clear boundaries between layers
5. **Flexibility** - Easy to swap implementations (e.g., for testing or different data sources)


## Next Steps

1. Gradually replace direct repository usage with interface usage
2. Move any remaining business logic from repositories to services
3. Add unit tests for services using mocked repositories
4. Consider adding caching or other cross-cutting concerns to base repository
5. Remove old repository implementations once migration is complete