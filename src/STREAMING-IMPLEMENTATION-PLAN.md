# Streaming Implementation Plan for DeterministicNarrativeProvider

## Problem Statement

The game has two narrative providers:
1. **AIGameMaster** - Uses AI to generate narrative, implements streaming via IResponseStreamWatcher
2. **DeterministicNarrativeProvider** - Returns fixed text immediately with Task.FromResult()

This creates an inconsistent UI experience. The AI provider streams text character by character, while the deterministic provider shows text instantly. The UI should not know or care which provider is active.

## Current Architecture Analysis

### Streaming Flow for AI Provider
```
AIGameMaster.GenerateIntroduction()
  → Creates StreamingContentStateWatcher
  → Passes to AIClient.CreateAndQueueCommand()
  → AIGenerationQueue.ProcessQueue()
  → OllamaProvider.GetCompletionAsync()
  → OllamaProvider.StreamCompletionAsync()
    → Reads HTTP stream line by line
    → Calls watcher.OnStreamUpdate(chunk) for each chunk
    → Updates GameWorld.StreamingContentState
  → MainGameplayView polls GameWorldSnapshot every 50ms
  → ConversationView displays streaming text with cursor
```

### Non-Streaming Flow for Deterministic Provider
```
DeterministicNarrativeProvider.GenerateIntroduction()
  → Returns Task.FromResult("Fixed text")
  → No streaming
  → Text appears instantly in UI
```

### Key Components

#### StreamingContentState (in GameWorld)
```csharp
public class StreamingContentState
{
    public string CurrentText { get; private set; }
    public bool IsStreaming { get; private set; }
    public float StreamProgress { get; private set; }
    
    public void BeginStreaming()
    public void UpdateStreamingText(string partialText)
    public void CompleteStreaming(string completeText)
}
```

#### IResponseStreamWatcher Interface
```csharp
public interface IResponseStreamWatcher
{
    void OnStreamUpdate(string chunk);
    void OnStreamComplete(string completeResponse);
    void OnError(Exception ex);
}
```

#### StreamingContentStateWatcher Implementation
```csharp
public class StreamingContentStateWatcher : IResponseStreamWatcher
{
    private readonly StreamingContentState _streamingContentState;
    private readonly StringBuilder _buffer = new StringBuilder();
    
    public void BeginStreaming() { ... }
    public void OnStreamUpdate(string chunk) { ... }
    public void OnStreamComplete(string completeResponse) { ... }
}
```

## Implementation Plan

### Step 1: Create DeterministicStreamingService

Create a new service that simulates streaming for deterministic text:

```csharp
public class DeterministicStreamingService
{
    private readonly GameWorld _gameWorld;
    
    public DeterministicStreamingService(GameWorld gameWorld)
    {
        _gameWorld = gameWorld;
    }
    
    public async Task StreamTextAsync(string text, List<IResponseStreamWatcher> watchers)
    {
        // Begin streaming
        foreach (var watcher in watchers)
        {
            if (watcher is StreamingContentStateWatcher sw)
            {
                sw.BeginStreaming();
            }
        }
        
        // Stream word by word (or character by character)
        var words = text.Split(' ');
        var buffer = new StringBuilder();
        
        foreach (var word in words)
        {
            buffer.Append(word).Append(' ');
            
            // Update watchers
            foreach (var watcher in watchers)
            {
                watcher.OnStreamUpdate(word + " ");
            }
            
            // Simulate typing speed
            await Task.Delay(50); // Adjust for desired speed
        }
        
        // Complete streaming
        var finalText = buffer.ToString().TrimEnd();
        foreach (var watcher in watchers)
        {
            watcher.OnStreamComplete(finalText);
        }
    }
}
```

### Step 2: Update DeterministicNarrativeProvider

Modify the provider to use streaming instead of immediate returns:

```csharp
public class DeterministicNarrativeProvider : INarrativeProvider
{
    private readonly DeterministicStreamingService _streamingService;
    private readonly GameWorld _gameWorld;
    // ... other dependencies
    
    public DeterministicNarrativeProvider(
        DeterministicStreamingService streamingService,
        GameWorld gameWorld,
        // ... other dependencies
    )
    {
        _streamingService = streamingService;
        _gameWorld = gameWorld;
        // ...
    }
    
    public async Task<string> GenerateIntroduction(ConversationContext context, ConversationState state)
    {
        // Generate the text as before
        string narrativeText = GetNarrativeText(context);
        
        // Create watchers like AIGameMaster does
        var watchers = new List<IResponseStreamWatcher>
        {
            new StreamingContentStateWatcher(_gameWorld.StreamingContentState)
        };
        
        // Stream the text
        await _streamingService.StreamTextAsync(narrativeText, watchers);
        
        return narrativeText;
    }
    
    // Similar updates for GenerateReaction and GenerateConclusion
}
```

### Step 3: Register DeterministicStreamingService

In ServiceConfiguration.cs:
```csharp
services.AddSingleton<DeterministicStreamingService>();
```

### Step 4: Clean Up ConversationView

Remove all mode-specific code from ConversationView.razor and ConversationView.razor.cs:

1. Remove `IsDeterministic` property
2. Remove `GroupedChoices` and grouping logic
3. Remove all conditional rendering based on provider type
4. Keep only the streaming display logic that works for both providers

## Expected Behavior After Implementation

1. **Consistent UI Experience**: Both AI and deterministic modes will show text streaming in
2. **No Mode Detection**: UI has no knowledge of which provider is active
3. **Clean Architecture**: Follows the principle of using interfaces, not mode flags
4. **Reusable Infrastructure**: Leverages existing StreamingContentState and watchers

## Testing Plan

1. Test with `UseDeterministicNarrative: true` - Should see text streaming
2. Test with `UseDeterministicNarrative: false` - Should see AI text streaming
3. Verify no UI differences between modes
4. Ensure streaming speed feels natural for deterministic text

## Configuration Considerations

Add configuration for deterministic streaming speed:
```json
{
  "DeterministicStreaming": {
    "DelayPerWordMs": 50,
    "DelayPerCharacterMs": 10,
    "UseWordMode": true
  }
}
```

## Risks and Mitigations

1. **Risk**: Deterministic text might stream too slowly
   - **Mitigation**: Make streaming speed configurable
   
2. **Risk**: Streaming might feel unnatural for fixed text
   - **Mitigation**: Use word-by-word streaming instead of character-by-character

3. **Risk**: Performance impact from artificial delays
   - **Mitigation**: Use efficient async/await patterns, avoid blocking threads

## Alternative Approaches Considered

1. **Instant display with fade-in**: Rejected - Creates different UI behavior
2. **Mode flag in UI**: Rejected - Violates clean architecture principles
3. **Different UI components**: Rejected - Unnecessary complexity

## Next Session Tasks

1. Implement DeterministicStreamingService
2. Update DeterministicNarrativeProvider to use streaming
3. Clean up ConversationView to remove mode-specific code
4. Test both providers to ensure consistent behavior
5. Fine-tune streaming speed for best user experience