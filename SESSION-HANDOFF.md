# Wayfarer Session Handoff - Literary UI Implementation
## Session Date: 2025-01-27

## 🎯 CRITICAL: Literary UI Transformation In Progress

We are transforming Wayfarer from a traditional RPG interface to an immersive literary experience where **everything is felt, not displayed**.

### Current Status as of 2025-01-27

#### ✅ Phase 1 COMPLETE: Backend Systems
- **AttentionManager** - 3-point attention system implemented
- **SceneContext** - Renamed from ConversationContext, expanded with tags
- **Context Tags** - Pressure, Relationship, Discovery, Resource, Feeling tags
- **ContextTagCalculator** - Created but has compilation errors (see below)
- **Rumor System** - Complete with confidence levels and trading
- **Attention Integration** - Conversation choices now use AttentionCost
- **NO BACKWARDS COMPATIBILITY** - All FocusCost references removed

#### 🚧 Phase 2 PARTIAL: UI Components Created
- **LiteraryConversationScreen** - Created but has compilation errors
- **AttentionDisplay** - Created and working
- **PeripheralAwareness** - Created and working
- **InternalThoughtChoice** - Created and working
- **BodyLanguageDisplay** - Created and working
- **literary-ui.css** - Created and linked in _Layout.cshtml
- **MainGameplayView** - Updated to use LiteraryConversationScreen

#### ❌ BLOCKING ISSUES - Must Fix First

**ContextTagCalculator.cs compilation errors:**
- Lines 57, 77: `GetQueueSize()` doesn't exist - need to use actual LetterQueueManager methods
- Lines 119-122: `GetTokenCount()` only takes 1 param - need to use `GetTokensWithNPC()`
- Lines 206, 208: Inventory doesn't have `Items` - need proper inventory access
- Line 217: `GetActiveLetters()` doesn't exist
- Lines 245, 248, 251: WeatherCondition enum missing values

**LiteraryConversationScreen.cs error:**
- Line 35: GameFacade doesn't have `ProcessConversationChoice()` - use correct method

**Architecture violations:**
- Using GameFacade directly instead of GameFacade interface
- ConversationViewModel missing literary UI properties

#### 📋 GitHub Kanban Board Status
Check the project board: https://github.com/users/meatz89/projects/2

**User Stories #27-36 Status:**
- #27 ✅ Attention system - Backend complete, UI partial
- #28 🚧 Partial information - Rumor backend complete, UI needed
- #29 ❌ Physical queue - Not started
- #30 ✅ Rumor system - Backend complete
- #31 🚧 Binding obligations - High attention costs implemented
- #32 ✅ Peripheral awareness - Component created
- #33 ✅ Feeling tags - Backend complete
- #34 ✅ Body language - Component created
- #35 ✅ Internal thoughts - Component created
- #36 🚧 Narrative costs - Partially implemented

#### 🚨 NEXT IMMEDIATE TASKS

1. **Fix ContextTagCalculator.cs** - Use correct methods from managers
2. **Fix LiteraryConversationScreen.cs** - Use correct GameFacade method
3. **Create/verify GameFacade interface** - Fix architecture violation
4. **Update ConversationViewModel** - Add literary UI properties
5. **Run build and E2E test** - Verify everything works

---

## 🏗️ Architecture Reminders

### CRITICAL: GameFacade Pattern
- **UI components MUST only use GameFacade** - Never inject services directly
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
   - Inject only GameFacade
   - Get ConversationViewModel from facade
   - Display narrative text without streaming effect

2. **AttentionDisplay.razor + .razor.cs**
   ```csharp
   @inject GameFacade GameFacade
   
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
    [Inject] private GameFacade GameFacade { get; set; }
    
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

1. **DON'T inject services directly** - Only use GameFacade
2. **DON'T use @code blocks** - Always use code-behind
3. **DON'T show numbers** - Everything must be narrative
4. **DON'T keep FocusCost** - It's been completely removed
5. **DON'T create RenderFragments** - Use proper components

---

## 📝 Testing Checklist

Before marking any component complete:
- [ ] Uses only GameFacade
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