# Wayfarer Session Handoff - Literary UI Implementation
## Session Date: 2025-01-27

## 🎯 CRITICAL: Literary UI Transformation In Progress

We are transforming Wayfarer from a traditional RPG interface to an immersive literary experience where **everything is felt, not displayed**.

### Current Status

#### ✅ Phase 1 COMPLETE: Backend Systems
- **AttentionManager** - 3-point attention system implemented
- **SceneContext** - Renamed from ConversationContext, expanded with tags
- **Context Tags** - Pressure, Relationship, Discovery, Resource, Feeling tags
- **ContextTagCalculator** - Bridges GameWorld to narrative presentation  
- **Rumor System** - Complete with confidence levels and trading
- **Attention Integration** - Conversation choices now use AttentionCost
- **NO BACKWARDS COMPATIBILITY** - All FocusCost references removed

#### 📋 GitHub Kanban Board Status
Check the project board: https://github.com/users/meatz89/projects/2

**Updated Issues:**
- #27 ✅ Attention system - Backend complete
- #30 ✅ Rumor system - Backend complete  
- #33 ✅ Feeling tags - Backend complete
- #28-29, #31-32, #34-36 - Awaiting UI implementation

#### 🚧 Phase 2 IN PROGRESS: UI Components

**NEXT IMMEDIATE TASK**: Create LiteraryConversationScreen components

---

## 🏗️ Architecture Reminders

### CRITICAL: GameFacade Pattern
- **UI components MUST only useIGameFacade** - Never inject services directly
- **GameWorld has NO dependencies** - Single source of truth
- **NO @code blocks in .razor files** - Use code-behind (.razor.cs)
- **Delete legacy code entirely** - No compatibility layers

### SceneContext Integration
The conversation system now uses `SceneContext` (not ConversationContext):
- Contains AttentionManager instance
- Populated with context tags by ContextTagCalculator
- Passed to all narrative generation methods

---

## 📂 Key Files Created/Modified

### New Files
- `/src/GameState/AttentionManager.cs` - Attention point system
- `/src/GameState/SceneTags.cs` - All tag enums
- `/src/GameState/ContextTagCalculator.cs` - Tag generation
- `/src/GameState/Rumor.cs` - Rumor data model
- `/src/GameState/RumorManager.cs` - Rumor tracking
- `/src/Game/AiNarrativeSystem/AttentionCost.cs` - Renamed from FocusCost
- `/LITERARY-UI-IMPLEMENTATION.md` - Complete documentation

### Modified Files  
- `/src/Game/ConversationSystem/SceneContext.cs` - Renamed from ConversationContext
- `/src/Game/ConversationSystem/ConversationManager.cs` - Uses attention
- `/src/Game/ConversationSystem/DeterministicNarrativeProvider.cs` - Generates attention costs
- `/src/Game/AiNarrativeSystem/InputMechanics.cs` - Uses AttentionCost

---

## 🎨 Phase 2: UI Components to Build

### LiteraryConversationScreen Components

1. **LiteraryConversationScreen.razor + .razor.cs**
   - Replace ConversationView entirely
   - Inject onlyIGameFacade
   - Get ConversationViewModel from facade
   - Display narrative text without streaming effect

2. **AttentionDisplay.razor + .razor.cs**
   ```csharp
   @injectIGameFacade GameFacade
   
   // In code-behind:
   private int CurrentAttention => ConversationVM?.AttentionRemaining ?? 3;
   ```

3. **PeripheralAwareness.razor + .razor.cs**
   - Show deadline pressure from SceneContext.MinutesUntilDeadline
   - Display binding obligations if OBLIGATION_ACTIVE tag present
   - Environmental hints based on FeelingTags

4. **InternalThoughtChoice.razor + .razor.cs**
   - Choices as italicized text
   - Show AttentionCost as symbols (◆)
   - Disable if not affordable

5. **BodyLanguageDisplay.razor + .razor.cs**
   - Convert RelationshipTags to descriptions
   - TRUST_HIGH → "Deep trust flows between you"
   - No numeric displays

---

## 🔧 Implementation Pattern

### Example Component Structure

```csharp
// LiteraryConversationScreen.razor
@page "/conversation"
@inherits LiteraryConversationScreenBase

<div class="literary-conversation">
    <AttentionDisplay />
    <PeripheralAwareness Context="@SceneContext" />
    
    <div class="narrative-content">
        @CurrentNarrative
    </div>
    
    <div class="choices">
        @foreach(var choice in Choices)
        {
            <InternalThoughtChoice Choice="@choice" OnSelected="@HandleChoice" />
        }
    </div>
</div>

// LiteraryConversationScreen.razor.cs
public partial class LiteraryConversationScreenBase : ComponentBase
{
    [Inject] privateIGameFacade GameFacade { get; set; }
    
    private ConversationViewModel ConversationVM { get; set; }
    private SceneContext SceneContext { get; set; }
    
    protected override async Task OnInitializedAsync()
    {
        ConversationVM = await GameFacade.GetCurrentConversation();
        // SceneContext is part of ConversationVM
    }
}
```

---

## ⚠️ Common Pitfalls to Avoid

1. **DON'T inject services directly** - Only useIGameFacade
2. **DON'T use @code blocks** - Always use code-behind
3. **DON'T show numbers** - Everything must be narrative
4. **DON'T keep FocusCost** - It's been completely removed
5. **DON'T create RenderFragments** - Use proper components

---

## 📝 Testing Checklist

Before marking any component complete:
- [ ] Uses onlyIGameFacade
- [ ] Has proper code-behind file
- [ ] No numeric displays
- [ ] Integrates with SceneContext
- [ ] Respects attention costs
- [ ] Follows mockup design

---

## 🚀 Quick Commands

```bash
# Build the project
cd /mnt/c/git/wayfarer/src
dotnet build

# Run the game
dotnet run
# Navigate to http://localhost:5011

# Check GitHub issues
gh issue list --repo meatz89/Wayfarer --state open

# Update issue progress
gh issue comment [number] --repo meatz89/Wayfarer --body "Progress update"
```

---

## 📖 Reference Documents

- **LITERARY-UI-IMPLEMENTATION.md** - Complete technical documentation
- **UI-MOCKUPS/conversation-elena.html** - Target conversation UI
- **UI-MOCKUPS/location-screens.html** - Location screen examples
- **CLAUDE.md** - Core architectural principles

---

## Next Session Focus

Continue Phase 2: Build the literary UI components starting with LiteraryConversationScreen. The backend is ready - now we need the frontend to match our vision of an immersive, literary interface where everything is felt, not displayed.