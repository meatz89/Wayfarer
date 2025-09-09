# Conversation Narrative Generation System - Implementation Plan

## Overview
Implement a modular narrative generation system that supports both AI-powered generation (via Ollama) and JSON fallback content. The system follows the backwards narrative construction principle from conversation-generation.md.

## Architecture Principles
1. **Modular Design**: Interface-based with swappable providers
2. **Blazor-Safe**: Uses IAsyncEnumerable instead of EventHandler pattern
3. **Streaming Support**: Progressive narrative display with typewriter effect
4. **Fallback Safety**: Always has JSON content when AI unavailable
5. **Minimal Integration**: Single touch point in ConversationOrchestrator

## System Components

### Core Infrastructure
```
/src/Infrastructure/AI/
├── OllamaClient.cs           - HTTP client for Ollama API
├── OllamaConfiguration.cs    - Config model for Ollama settings
└── OllamaModels.cs           - Request/response models
```

### Narrative Generation Subsystem
```
/src/Subsystems/Conversation/NarrativeGeneration/
├── INarrativeProvider.cs              - Core interface
├── NarrativeProviderFactory.cs        - Provider selection
├── ConversationNarrativeService.cs    - Integration service
├── NarrativeStreamingService.cs       - Streaming support
├── Models/
│   ├── ConversationState.cs          - Input state wrapper
│   ├── NPCData.cs                    - NPC info for generation
│   ├── CardCollection.cs             - Active cards wrapper
│   ├── NarrativeOutput.cs            - Generated output
│   ├── NarrativeChunk.cs             - Streaming chunk
│   └── ConversationNarrativeState.cs - UI state management
└── Providers/
    ├── AIConversationNarrativeProvider.cs    - AI implementation
    ├── ConversationNarrativeGenerator.cs     - Core algorithm
    ├── PromptBuilder.cs                      - Prompt construction
    ├── JsonNarrativeProvider.cs              - JSON fallback
    └── JsonNarrativeRepository.cs            - JSON content loader
```

### Content
```
/src/Content/Narratives/
└── conversation_narratives.json    - Pre-authored narratives
```

## Work Packets

### Packet 1: Core Infrastructure (OllamaClient)
**Files to create**:
- `/src/Infrastructure/AI/OllamaClient.cs`
- `/src/Infrastructure/AI/OllamaConfiguration.cs`
- `/src/Infrastructure/AI/OllamaModels.cs`

**Requirements**:
- HTTP client wrapper for Ollama API
- Support streaming via IAsyncEnumerable
- Configuration from appsettings.json
- Proper cancellation token support
- No exception handling (let failures bubble)

### Packet 2: Narrative Provider Interface & Models
**Files to create**:
- `/src/Subsystems/Conversation/NarrativeGeneration/INarrativeProvider.cs`
- `/src/Subsystems/Conversation/NarrativeGeneration/Models/ConversationState.cs`
- `/src/Subsystems/Conversation/NarrativeGeneration/Models/NPCData.cs`
- `/src/Subsystems/Conversation/NarrativeGeneration/Models/CardCollection.cs`
- `/src/Subsystems/Conversation/NarrativeGeneration/Models/NarrativeOutput.cs`
- `/src/Subsystems/Conversation/NarrativeGeneration/Models/NarrativeChunk.cs`
- `/src/Subsystems/Conversation/NarrativeGeneration/Models/ConversationNarrativeState.cs`

**Requirements**:
- Simple data models, no logic
- Strongly typed (no Dictionary, object, var)
- Match conversation-generation.md structure

### Packet 3: Streaming Service
**Files to create**:
- `/src/Subsystems/Conversation/NarrativeGeneration/NarrativeStreamingService.cs`

**Requirements**:
- IAsyncEnumerable pattern for streaming
- Configurable chunk size and delay
- Support cancellation
- Word-based chunking for natural flow

### Packet 4: AI Provider Implementation
**Files to create**:
- `/src/Subsystems/Conversation/NarrativeGeneration/Providers/AIConversationNarrativeProvider.cs`
- `/src/Subsystems/Conversation/NarrativeGeneration/Providers/ConversationNarrativeGenerator.cs`
- `/src/Subsystems/Conversation/NarrativeGeneration/Providers/PromptBuilder.cs`

**Requirements**:
- Implement backwards narrative construction from conversation-generation.md
- Analyze cards first, then generate NPC dialogue all cards can respond to
- Build prompts from templates in /src/Data/Prompts/
- Parse Ollama responses into NarrativeOutput

### Packet 5: JSON Fallback Provider
**Files to create**:
- `/src/Subsystems/Conversation/NarrativeGeneration/Providers/JsonNarrativeProvider.cs`
- `/src/Subsystems/Conversation/NarrativeGeneration/Providers/JsonNarrativeRepository.cs`
- `/src/Content/Narratives/conversation_narratives.json`

**Requirements**:
- Load pre-authored content from JSON
- Match by flow/rapport ranges
- Support variable substitution
- Always available as fallback

### Packet 6: Factory & Integration Service
**Files to create**:
- `/src/Subsystems/Conversation/NarrativeGeneration/NarrativeProviderFactory.cs`
- `/src/Subsystems/Conversation/NarrativeGeneration/ConversationNarrativeService.cs`

**Requirements**:
- Factory selects provider based on configuration
- Service wraps provider calls with fallback
- Convert game models to narrative models
- Minimal coupling with existing code

### Packet 7: Service Registration
**Files to update**:
- `/src/ServiceConfiguration.cs`

**Requirements**:
- Register all new services as Singleton
- Configure HttpClient for OllamaClient
- Load configuration from IConfiguration

### Packet 8: ConversationOrchestrator Integration
**Files to update**:
- `/src/Subsystems/Conversation/ConversationOrchestrator.cs`

**Requirements**:
- Inject ConversationNarrativeService
- Replace DialogueGenerator calls
- Maintain fallback to existing behavior
- Single integration point only

## Implementation Order
1. Core Infrastructure (Packet 1)
2. Models & Interface (Packet 2)
3. Streaming Service (Packet 3)
4. JSON Provider (Packet 5) - simpler, test fallback first
5. Factory & Integration (Packet 6)
6. Service Registration (Packet 7)
7. Test JSON flow end-to-end
8. AI Provider (Packet 4)
9. ConversationOrchestrator Integration (Packet 8)
10. UI Integration (separate plan)

## Success Criteria
1. JSON fallback works without AI
2. AI provider works when Ollama available
3. Automatic fallback when AI fails
4. No breaking changes to existing code
5. Streaming works smoothly in UI
6. Follows backwards narrative construction
7. All cards can respond to generated dialogue

## Key Algorithms from conversation-generation.md

### Backwards Construction
1. Analyze active cards for:
   - Persistence requirements (Impulse/Opening)
   - Focus distribution patterns
   - Effect categories (risk/atmosphere/utility)
2. Generate NPC dialogue that:
   - All cards can respond to
   - Includes hooks for special persistence
   - Matches rapport/flow state
3. Map each card to appropriate response:
   - Based on mechanical effect
   - Scaled to focus cost
   - Adjusted for rapport level

### Rapport Stages
- 0-5: Surface (observations, deflections)
- 6-10: Gateway (understanding, sharing)
- 11-15: Personal (emotional support, commitments)
- 16+: Intimate (vulnerability, life-changing)

### Topic Layers
- Deflection: What NPC discusses when avoiding
- Gateway: Related but not direct
- Core Crisis: The actual problem

## Testing Strategy
1. Unit tests for each provider
2. Integration test with mock Ollama
3. Fallback test with AI disabled
4. Streaming cancellation test
5. Memory leak test (disposal)
6. Thread safety test (Blazor)