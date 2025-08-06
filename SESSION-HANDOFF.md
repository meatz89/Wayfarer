# Wayfarer Session Handoff - Literary UI Implementation
## Session Date: 2025-01-27 (Updated)

## 🎯 CRITICAL DESIGN CLARIFICATION: Mechanics-First Literary UI

We are building a **mechanics-first** game where board game-like systems generate narrative content through their operation, NOT a narrative game with hidden mechanics.

### 🔥 KEY UNDERSTANDING (User Clarification)

The user wants:
1. **UI exactly as shown in mockups** - Literary presentation with attention points, body language, etc.
2. **Board game mechanics underneath** - NOT tag matching but actual SYSTEMS (like Wingspan, Spirit Island)
3. **Content emerges from mechanics** - No hardcoded narrative, just systematic generation
4. **Like the letter queue** - Physical slots, weight, deadlines, token burning are MECHANICS that generate narrative

### Current Implementation Status

#### ✅ What's Working
- **GameFacade** updated with CreateConversationViewModel populating literary properties
- **UI Components** created (AttentionDisplay, PeripheralAwareness, etc.)
- **ContextTagCalculator** and **RumorManager** integrated
- Build succeeds with only warnings

#### ❌ Previous Compilation Errors (NOW FIXED)
- ContextTagCalculator methods updated to use correct APIs
- ConversationChoice properties mapped correctly
- GameFacade dependencies added (ContextTagCalculator, RumorManager)

---

## 🎲 PROPOSED BOARD GAME MECHANICS

### 1. CONVERSATION SYSTEM: Pressure Differentials
**Core Mechanics:**
- **Pressure Pools** (0-15) for both Player and NPC
- **3 Attention Points** allocated each turn to:
  - **Press**: Apply pressure to NPC
  - **Guard**: Defend against NPC pressure
  - **Observe**: Reveal hidden information
- **Momentum** (-5 to +5) carries between conversations

**How It Generates Content:**
- Pressure levels → body language descriptions
- Press vs Guard differential → conversation flow
- Observe success → reveals tiered information
- 15 pressure = conversation breakdown

**Formula:**
```
PressureGain = MAX(0, Attacker.Press - Defender.Guard) + FloorBonus + MomentumBonus
```

### 2. LOCATION SYSTEM: Activity Tokens
**Core Mechanics:**
- **Activity Tokens** (0-12) generated at dawn per location type
- **3 Observation Dice** to allocate: Passive (d4), Active (d6), Deep (d8)
- **Flux Cards** reveal location state changes at thresholds

**How It Generates Content:**
- High activity (9-12) → crowded, chaotic descriptions
- Low activity (1-4) → intimate, quiet descriptions
- Captured tokens → information discovery tiers
- Token types → Trust/Commerce/Status/Shadow opportunities

### 3. TRAVEL SYSTEM: Stamina & Segments
**Core Mechanics:**
- **Route Segments** with terrain-based costs (2-4 hours)
- **Stamina Pool** (10 points) depletes per segment
- **Weather Die** (d6) modifies travel speed
- **Letter Weight** affects stamina drain

**How It Generates Content:**
- Terrain + Weather + Time → encounter templates
- Stamina level → exhaustion descriptions
- Weight burden → struggle narrative
- Route knowledge → efficiency decisions

---

## 📐 CRITICAL DESIGN PRINCIPLES

### Mechanics Generate Narrative (Not Vice Versa)
- **NO** hardcoded story content
- **NO** unique NPC dialogue per situation
- **YES** systematic generation from mechanical state
- **YES** template combinations based on mechanics

### Everything Is Deterministic
- Same mechanical state → same narrative output
- Player can learn and predict systems
- Randomness only in dice/cards, not content selection

### Board Game Feel
- Clear resource management (attention, stamina, tokens)
- Visible state tracking (pressure, momentum, activity)
- Strategic decisions with trade-offs
- No hidden narrative branches

---

## 🛠️ IMPLEMENTATION APPROACH

### Phase 1: Core Mechanical Systems
1. **PressureDifferentialSystem** class for conversations
2. **ActivityTokenSystem** class for locations
3. **SegmentedTravelSystem** class for journeys

### Phase 2: Template Generators
1. **BodyLanguageGenerator** from pressure levels
2. **AtmosphereGenerator** from activity tokens
3. **TravelNarrativeGenerator** from segment state

### Phase 3: UI Integration
1. Map mechanical values to UI displays
2. Show mechanics transparently (pressure bars, token counts)
3. Keep literary descriptions as OUTPUT of mechanics

---

## 📊 CONTENT REQUIREMENTS

### Template-Based System (67 hours total)
- **6 NPC Archetypes** (not 30 unique personalities)
- **168 Description Snippets** for combinations
- **28 Travel Components** for template mixing
- **NO unique dialogue** per NPC per situation

### Scaling Strategy
- New NPC = assign archetype (5 minutes)
- New location = set activity pattern (10 minutes)
- New route = define segments (5 minutes)
- Content emerges from mechanical operation

---

## ⚠️ WHAT WE'RE NOT BUILDING

- ❌ Tag matching systems that generate prose
- ❌ Narrative trees with mechanical effects
- ❌ Unique content for every situation
- ❌ Hidden story branches
- ❌ Special case narrative events

## ✅ WHAT WE ARE BUILDING

- ✅ Board game mechanics that happen to generate text
- ✅ Resource management with narrative skin
- ✅ Deterministic systems players can master
- ✅ Template combinations from mechanical state
- ✅ Literary UI as visualization of mechanics

---

## 🎯 Next Critical Tasks

1. **Implement PressureDifferentialSystem**
   - Create pressure tracking
   - Build attention allocation
   - Generate body language from pressure

2. **Implement ActivityTokenSystem**
   - Token generation per location
   - Observation dice mechanics
   - Information discovery tiers

3. **Create Template Generators**
   - Map mechanical states to descriptions
   - Build combination rules
   - Test output variety

4. **Update UI Components**
   - Display mechanical state clearly
   - Show literary descriptions as output
   - Maintain transparency of systems

---

## 📋 GitHub Kanban Board Status
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