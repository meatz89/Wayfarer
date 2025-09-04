# Blazor ServerPrerendered Mode - Critical Architecture Considerations

## The Double Rendering Issue

When using `render-mode="ServerPrerendered"` (configured in _Host.cshtml), Blazor components render **TWICE**:

1. **First Render**: Server-side prerendering (generates static HTML)
2. **Second Render**: After establishing interactive SignalR connection

This means ALL component lifecycle methods run twice:
- `OnInitializedAsync()`
- `OnParametersSetAsync()`
- `OnAfterRenderAsync()`

## Impact on Game Systems

### What Gets Executed Twice
- Component initialization code
- Any method calls in OnInitializedAsync
- State refresh operations
- Console.WriteLine statements (appear twice in logs)

### Critical Requirements for Idempotence

All initialization code MUST be idempotent. This means:

1. **Check Before Mutating State**
   ```csharp
   public async Task StartGameAsync()
   {
       if (_gameWorld.IsGameStarted)
       {
           return; // Already initialized, skip
       }
       // ... initialization code
       _gameWorld.IsGameStarted = true;
   }
   ```

2. **Never Add Duplicate Messages**
   - System messages should only be added once
   - Use flags to track if messages were already shown

3. **Resource Initialization**
   - Player resources (coins, health, etc.) must not be doubled
   - Use flags to prevent duplicate initialization

4. **Event Subscriptions**
   - Be careful with event handlers - they might subscribe twice
   - Consider unsubscribing first or checking if already subscribed

## Current Implementation

### Protected Systems
- ✅ `GameFacade.StartGameAsync()` - Uses `IsGameStarted` flag
- ✅ Resource display refreshes - Read-only operations
- ✅ Time display updates - Read-only operations
- ✅ Location data refresh - Read-only operations

### Safe Patterns
- All services are Singletons (persist across renders)
- GameWorld maintains state across both render phases
- Read-only operations can safely run multiple times
- User actions (ProcessIntent) only happen after interactive phase

## Architecture Decisions

### Why We Keep ServerPrerendered
- Faster initial page load (user sees content immediately)
- Better SEO (if ever needed)
- Reduced perceived latency

### Alternative: Server Mode
If we changed to `render-mode="Server"`:
- Components would only render once
- Slightly slower initial load (wait for SignalR)
- Simpler mental model but worse UX

## Testing Considerations

When testing, be aware that:
- Console logs will appear twice for initialization
- Breakpoints in OnInitializedAsync will hit twice
- Network requests might happen twice if not protected

## Checklist for New Features

When adding new initialization code:

- [ ] Is the operation idempotent?
- [ ] Are there guards against duplicate execution?
- [ ] Are system messages protected from duplication?
- [ ] Are resource modifications protected?
- [ ] Are event subscriptions managed properly?

## Example Log Output

```
[GameFacade.StartGameAsync] Player initialized at Market Square - The Fountain   // First render
[GameFacade.StartGameAsync] Game already started, skipping initialization        // Second render
```

This is EXPECTED behavior and shows the idempotence protection working correctly.