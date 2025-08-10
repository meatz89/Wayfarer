# Token Display: Narrative-Focused Design

## Jordan's Narrative Perspective: Preserving Human Moments

### The Problem We Solved
The original token display treated relationships like a resource management game:
- Showed all 4 token types regardless of context (Trust/Commerce/Status/Shadow)
- Displayed explicit numbers: "Trust: ●●●○○ (debt: -2)"
- Felt like managing a spreadsheet, not navigating medieval social obligations

### The Narrative Solution

#### 1. Context-Sensitive Display
Tokens now appear based on narrative relevance:
- **During HELP conversations**: Shows Trust tokens (personal bonds matter)
- **During NEGOTIATE conversations**: Shows Commerce and Status tokens
- **During INVESTIGATE conversations**: Shows Shadow and Trust tokens
- **Debt always visible**: Creates narrative tension regardless of context

#### 2. Descriptive Language Over Numbers
Instead of mechanical indicators, we use emotional descriptions:

**Trust Examples:**
- Debt: "Old debts strain your friendship with Elena"
- Positive: "Elena considers you reliable"
- Strong: "Years of friendship bind you"

**Commerce Examples:**
- Debt: "You're in their ledger - in red ink"
- Positive: "Reliable trading partner"
- Strong: "Most trusted business partner"

**Status Examples:**
- Debt: "Your reputation with them lies in tatters"
- Positive: "Respected in their circles"
- Strong: "Held in highest esteem"

**Shadow Examples:**
- Debt: "They know you've betrayed their secrets"
- Positive: "They share sensitive information"
- Strong: "Keeper of their darker secrets"

#### 3. Visual Design Without Gamification
- Subtle color-coded borders (warm for Trust, practical green for Commerce)
- Gradient backgrounds that suggest rather than shout
- Warning symbols (⚠) for debt without showing numbers
- Italicized text for narrative flavor

### Implementation Details

#### Component Structure
```razor
<TokenDisplay 
    Tokens="@NpcTokens" 
    NpcName="@Model.NpcName" 
    ConversationContext="@GetCurrentConversationContext()"
    NpcRole="@GetNpcRole()"
    ShowOnlyRelevant="true"
    UseNarrativeDescriptions="true" />
```

#### Context Detection
The system determines which tokens to show based on:
1. Current conversation verb (HELP/NEGOTIATE/INVESTIGATE)
2. NPC role (merchant → Commerce, noble → Status, friend → Trust)
3. Debt status (always shown to create tension)
4. Non-zero values (hide irrelevant zero relationships)

### Design Philosophy

**What We Preserved:**
- The feeling of owing someone or being owed
- The weight of social obligations
- The complexity of medieval relationships
- The narrative consequences of player actions

**What We Removed:**
- Numerical thinking ("I need 2 more tokens")
- Universal stat displays (all 4 types always visible)
- Gamified progress bars
- Mechanical optimization mindset

### The Human Test
Every display element passes these checks:
- Does this make me feel the relationship, not just see it?
- Am I thinking about the person, not the numbers?
- Does this enhance the fantasy of medieval social navigation?
- Would a real letter carrier think this way?

### Technical Architecture

**Files Modified:**
- `/Pages/Components/TokenDisplay.razor` - Complete rewrite for narrative focus
- `/Pages/Components/TokenDisplay.razor.css` - New visual design
- `/Pages/ConversationScreen.razor` - Added context parameters
- `/Pages/ConversationScreen.razor.cs` - Context detection logic

**Key Features:**
- Dynamic filtering based on conversation context
- Narrative descriptions replace numerical displays
- Contextual empty states ("No business dealings yet" vs "A stranger")
- Graceful degradation for debugging (can show numbers if needed)

### Result
Players now experience relationships as human connections with weight and meaning, not as numbers to optimize. The medieval letter carrier fantasy is preserved - you're navigating a web of social obligations, not managing a database of reputation points.

### Future Considerations
- Could add subtle animations for relationship changes
- Might introduce seasonal variations in descriptions
- Could tie descriptions to recent player actions
- Possible to add NPC-specific relationship flavors

The token system now serves the story rather than dominating it. Players think "I need to rebuild Elena's trust" instead of "I need 2 more Trust tokens." This is narrative design done right.