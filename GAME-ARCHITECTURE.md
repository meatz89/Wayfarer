# Wayfarer Game Architecture

## Critical Architecture Principles

### 1. GameWorld Initialization (NEVER CHANGE THIS)

**CRITICAL**: GameWorld MUST be initialized through a static GameWorldInitializer class. This is the foundation of the entire game startup process.

#### Why This Pattern Exists

1. **Prevents Circular Dependencies**: During ServerPrerendered mode, Blazor components are rendered on the server before the SignalR connection is established. If GameWorld creation requires dependency injection, it creates circular dependencies that cause the application to hang.

2. **Clean Startup**: GameWorld is the root aggregate of the entire game state. It must be created cleanly without dependencies.

3. **Singleton Guarantee**: By creating GameWorld through a static method and registering it as a singleton, we ensure only one instance exists throughout the application lifetime.

#### The Pattern

```csharp
// In GameWorldInitializer.cs
public static class GameWorldInitializer
{
    public static GameWorld CreateGameWorld()
    {
        // Create GameWorld with default content directory
        // No dependency injection needed
    }
}

// In ServiceConfiguration.cs
services.AddSingleton<GameWorld>(_ =>
{
    // Call static initializer - no DI dependencies
    return GameWorldInitializer.CreateGameWorld();
});
```

#### What Will Break If You Change This

1. **ServerPrerendered Mode Will Hang**: The application will freeze when trying to load the page
2. **Circular Dependencies**: Services that depend on GameWorld won't be able to resolve
3. **Startup Failures**: The entire application startup sequence will fail

#### Tests That Enforce This

- `GameWorldInitializationTests.cs` - Verifies GameWorldInitializer remains static
- `ArchitectureTests.cs` - Ensures no service locator patterns
- `StartupValidationTests.cs` - Validates startup doesn't hang

### 2. Dependency Flow Direction

**Rule**: All dependencies flow INWARD towards GameWorld, never outward from it.

```
UI Components
    ↓
Managers/Services
    ↓
Repositories
    ↓
GameWorld (No Dependencies)
```

GameWorld is the single source of truth and has NO dependencies on any services, managers, or external components.

### 3. Navigation Architecture

**Rule**: GameUIBase is the ONLY navigation handler in the application.

- GameUIBase (at @page "/") controls all navigation
- MainGameplayView is a regular component, NOT a navigation handler
- No NavigationService with events or delegates
- Simple component-based navigation using CurrentView property

### 4. No Service Locator Pattern

**Rule**: Never use GetRequiredService outside of ServiceConfiguration.

Bad:
```csharp
public class SomeService
{
    public void DoSomething(IServiceProvider provider)
    {
        var gameWorld = provider.GetRequiredService<GameWorld>(); // NO!
    }
}
```

Good:
```csharp
public class SomeService
{
    private readonly GameWorld _gameWorld;
    
    public SomeService(GameWorld gameWorld) // Inject through constructor
    {
        _gameWorld = gameWorld;
    }
}
```

## Testing Strategy

### 1. Unit Tests
- Test individual components in isolation
- Verify GameWorld can be created without dependencies
- Ensure static initialization pattern

### 2. Architecture Tests
- Enforce architectural rules through reflection
- Prevent accidental breaking changes
- Validate dependency directions

### 3. Integration Tests
- Test the full startup sequence
- Verify no hangs during prerendering
- Ensure all services can be resolved

### 4. E2E Tests
- Validate the entire game can start
- Check critical services are available
- Ensure UI renders properly

## Common Pitfalls to Avoid

1. **Making GameWorldInitializer Non-Static**: This will break ServerPrerendered mode
2. **Adding Dependencies to GameWorld**: This violates the core architecture
3. **Using Events in Navigation**: This creates circular dependencies
4. **Service Locator Anti-Pattern**: Always use constructor injection

## Monitoring Architecture Health

Run these commands regularly:

```bash
# Run architecture tests
dotnet test --filter "FullyQualifiedName~ArchitectureTests"

# Run initialization tests
dotnet test --filter "FullyQualifiedName~GameWorldInitializationTests"

# Run startup validation
dotnet test --filter "FullyQualifiedName~StartupValidationTests"
```

## When to Revisit This Architecture

This architecture should ONLY be changed if:

1. Moving away from Blazor Server to a different framework
2. Fundamentally changing how game state is managed
3. Complete rewrite of the game engine

Even then, the principle of static initialization for the root aggregate should be maintained.