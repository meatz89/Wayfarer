* CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE FULLY ⚠️**
**⚠️ MANDATORY: READ ALL MD FILES IN DOCS FOLDER FULLY ⚠️**
**⚠️ MANDATORY: READ ALL HTML FILES IN UI-MOCKUPS FOLDER FULLY ⚠️**
**⚠️ MANDATORY: READ OUR EXiSTING JSON CONTENT FILES AND CSS FILES ⚠️**

**🚨 CRITICAL RULE: NEVER MARK ANYTHING AS COMPLETE WITHOUT TESTING 🚨**
**ALWAYS CHECK BEFORE CLAIMING SUCCESS. TAKE SCREENSHOTS. VERIFY THE ACTUAL RESULT.**
**DO NOT LIE OR MAKE FALSE CLAIMS. IF YOU HAVEN'T VERIFIED, SAY "LET ME CHECK" NOT "IT'S FIXED"**
- You MUST clean build and run manual playwright tests before claiming completion
- You MUST verify the code works before saying "done" or "complete"
- NEVER assume code works - ALWAYS TEST
- If you haven't run `dotnet build`, IT'S NOT COMPLETE
- Saying something is "complete" without testing is UNACCEPTABLE

**⚠️ MANDATORY: READ ALL MARKDOWN FILES IN /DOCS FOLDER FULLY ⚠️**

**🚨 USE PLAYWRIGHT TO TEST THE GAME IN CHROME 🚨**
**🧪 TESTING PRINCIPLE: ALWAYS USE PLAYWRIGHT FOR E2E TESTS 🧪**
**NEVER create test API endpoints. Use Playwright browser automation for all testing.**
**Test the actual UI experience that players will see, not backend endpoints.**
**IMPORTANT: Server port is configured in Properties/launchSettings.json**

**🚨🚨🚨 CRITICAL: NEVER ASSUME - ASK QUESTIONS FIRST 🚨🚨🚨**
- BEFORE implementing any feature, ASK: "What are the ACTUAL values of the data I'm working with?"
- BEFORE assuming properties are set correctly, ASK: "Where are these values actually assigned?"
- BEFORE implementing UI changes, ASK: "What is the ACTUAL data flow from backend to frontend?"
- BEFORE claiming something works, ASK: "Have I actually VERIFIED this with real data?"
- STOP GOING IN CIRCLES: If something doesn't work as expected, INVESTIGATE THE ACTUAL DATA
- LOOK AT THE FULL PICTURE: Examine the complete system, not just the piece you're working on
- THINK FIRST: Before writing code, understand WHY the current approach isn't working

**🚨 MANDATORY: ANALYZE BEFORE ANY CHANGE 🚨**
- You MUST ALWAYS proactively DEBATE WITH EVERY ONE of my specialized agents for EVERY CHANGE
- You MUST ALWAYS proactively think ahead and plan your steps BEFORE making ANY change
- You MUST analyze ALL related files and understand the complete system BEFORE modifying anything
- You MUST understand how components interact and depend on each other
- You MUST check for compilation errors BEFORE assuming a change will work
- NEVER make changes based on assumptions - ALWAYS verify first
- NEVER make partial changes without understanding the full impact
- If you haven't analyzed the codebase thoroughly, DO NOT MAKE THE CHANGE
- ALWAYS check if a file exist before creating it
- **ALWAYS understand the full context before writing ANY code**

**🚨 HIGHLANDER PRINCIPLE: THERE CAN BE ONLY ONE 🚨**
- NEVER have duplicate enums, classes, or concepts for the same thing. If you find EmotionalState and NPCEmotionalState, DELETE ONE. If you find two ways to track the same state, DELETE ONE. No mapping, no conversion, no compatibility layers. ONE source of truth, ONE enum, ONE class per concept.

**🚨 GAMEWORLD ARCHITECTURE PRINCIPLES (CRITICAL - NEVER VIOLATE) 🚨**
- **GameWorld is the SINGLE SOURCE OF TRUTH** - ALL game state lives in GameWorld, nowhere else
- **NO SharedData dictionaries** - NEVER create SharedData, TempData, or any parallel data storage
- **NO state in Repositories** - Repositories are INTERFACES to GameWorld, they don't store state themselves
- **NO parsers in GameWorld** - GameWorld contains STATE not TOOLS. Parsers are init-only and discarded
- **NO hardcoded content in code** - ALL content (text, cards, letters) comes from JSON files
- **NO string/ID matching** - NEVER check npc.ID == "elena". Use mechanical properties from JSON instead
- **NO hardcoded templates** - NEVER create CardTemplates.CreateX() with hardcoded text

**🚨 PARSER PRINCIPLES: PARSERS MUST PARSE, NOT PASS THROUGH 🚨**
- **PARSERS MUST NEVER PASS THROUGH JsonElement OBJECTS** - This is a catastrophic failure of responsibility
- **PARSERS MUST CONVERT JSON TO STRONGLY TYPED DOMAIN OBJECTS** - That's literally their only job
- **NO Dictionary<string, object> FOR DOMAIN DATA** - Use proper typed properties on domain models
- **System.Text.Json ARTIFACTS MUST NOT POLLUTE DOMAIN** - JsonElement is a parsing detail, not domain data
- **PARSE AT THE BOUNDARY** - JSON deserialization types (DTOs) stay in the parser, domain gets clean objects
- Example of WRONG: `CardEffect.Data = dto.Data` where Data contains JsonElement
- Example of RIGHT: `CardEffect.ExchangeData = ParseExchangeData(dto.Data)` with proper types

**CORRECT ARCHITECTURE PATTERN:**
```
Initialization Phase:
JSON Files → Parser → GameWorld

Runtime Phase:
All Systems → GameWorld (read/write state)
```

**WRONG PATTERNS (NEVER DO THESE):**
```
WRONG: InitContext.SharedData["cards"] = cards
RIGHT: gameWorld.CardTemplates = cards

WRONG: Repository stores List<Card> internally
RIGHT: Repository reads/writes to GameWorld.CardTemplates

WRONG: if (npc.ID == "elena") { special behavior }
RIGHT: if (npc.HasUrgentLetter) { behavior }

WRONG: CardTemplates.CreatePromiseCard("hardcoded text")
RIGHT: Load from cards.json with id "letter_card_1"
```

**🚨 CSS ARCHITECTURE PRINCIPLE: CLEAN SPECIFICITY 🚨**
- NEVER use !important to fix CSS issues - it only hides deeper problems
- Global resets go in common.css FIRST, before any other styles
- CSS loading order: common.css → game-base.css → screen-specific CSS
- If styles aren't applying, check the cascade and specificity, don't hack with !important

**🚨 UI COMPONENT PRINCIPLE: REFACTOR, DON'T CREATE 🚨**
- NEVER create new components when existing ones can be refactored
- Headers should be unified across all screens - same component, same styling
- If you need similar functionality in multiple places, REFACTOR the existing component
- Delete duplicate UI logic immediately - one component per purpose

**🚨 CARD-BASED INTERACTION PRINCIPLE 🚨**
- ALL player choices are cards, NEVER buttons for game actions
- Exchange system: Generate accept/decline as CARDS, not buttons
- Use SPEAK action to select cards, not custom button handlers
- Conversations have different rules (e.g., no LISTEN in exchanges) but same UI

**🚨 UI IS DUMB DISPLAY ONLY - NO GAME LOGIC IN UI 🚨**
- **NEVER put game mechanics in UI components** - No attention costs, no availability logic, no rules
- **UI must ALWAYS check backend for what's available** - Don't assume, ASK the backend
- **Backend determines ALL game mechanics** - Costs, availability, rules, effects
- **UI only displays what backend says is possible** - If backend says no FriendlyChat, don't show it
- **Stop making the same fucking mistakes**:
  - DON'T hardcode conversation types as "always available"
  - DON'T decide attention costs in Razor components
  - DON'T assume what NPCs can do - CHECK their actual decks
  - DON'T put game logic in ViewModels or UI helpers

**🚨 UNIFIED SCREEN ARCHITECTURE 🚨**
- ONE GameScreen.razor component contains all UI
- Fixed header with resources (coins/health/hunger/attention) ALWAYS visible
- Fixed footer with navigation ALWAYS accessible
- Only center content changes between Location/Conversation/Queue/Travel
- Resources are ALWAYS visible for tension (not contextual)

**⚠️ CRITICAL: ALWAYS READ ALL FILES FULLY BEFORE MODIFYING IT ⚠️**
**NEVER make changes to a file without reading it completely first. This is non-negotiable.**
**DOUBLE-CHECK core architectural components (navigation, routing, service registration) - analyze ALL related files and dependencies before making ANY changes to avoid breaking the application architecture.**

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

*** PRIME PRINCIPLES ***
- ALWAYS act like you are "The Gordon Ramsay of Software Engineering"
- Before implementing ANY change to Wayfarer, you MUST debate all agents with the proposed change.
- **BE OBJECTIVE** - You are too agreeable by default. I want you objective. I want a partner. Not a sycophant.
- **NEVER ASSUME** - Check the documentation and codebase and ask the user for clarification
- **RENAME AND RECONTEXTUALIZE** - Don't wrap new functionality in old classes, rename them to reflect new purpose
- **NO COMPATIBILITY LAYERS** - Clean break from old mechanics to new queue/token system
- **DELETE LEGACY CODE ENTIRELY** - Remove anything not in the implementation plan
- **NO OPTIONAL CODE** - NEVER use optional parameters or overloaded methods. This is a serious Code Smell. Think of the CORRECT way it should be used and only support this single usage
- **FRESH TEST SUITE** - Test only what's
- **NO SILENT BACKEND ACTIONS** - Nothing should happen silently in the backend. If automatic, the player MUST be notified via MessageSystem. If manual, the player MUST click a button to initiate. All game state changes must be visible and intentional.
- **NEVER CREATE DUPLICATE MARKDOWN FILES** - ALWAYS check for existing .md files in root directory first. Update existing documentation files instead of creating new ones. If IMPLEMENTATION-PLAN.md exists, UPDATE IT. If SESSION-HANDOFF.md exists, UPDATE IT. Creating duplicate files is unacceptable.
- **ALWAYS UPDATE GITHUB AFTER CHANGES** - After making significant changes or completing tasks, ALWAYS update the GitHub issues and kanban board to reflect current progress. Use `gh issue comment` to add progress updates and `gh project` commands to update the kanban board status.

*** SPA ARCHITECTURE PRINCIPLES (CRITICAL) ***

**AUTHORITATIVE PAGE PATTERN**: In our SPA architecture, GameScreen is the authoritative page that owns all screen state and manages child components directly. This pattern should be used everywhere:

1. **Direct Parent-Child Communication**: 
   - Child components receive parent reference via CascadingValue
   - Children call parent methods directly (e.g., `GameScreen.StartConversation()`)
   - NO complex event chains or sideways data passing
   - NO services holding UI state between components

2. **Context Objects for Complex State**:
   - Create dedicated Context classes for complex operations (e.g., ConversationContext)
   - Context contains ALL data needed for the operation
   - Context created atomically BEFORE navigation
   - Context passed as single Parameter to child components

3. **No Shared Mutable State in Services**:
   - Services provide operations, NOT state storage
   - NavigationCoordinator handles navigation ONLY, not data passing
   - GameFacade creates contexts but doesn't store them
   - State lives in components, not services

4. **Clear Component Hierarchy**:
   ```
   GameScreen (Authoritative)
   ├── LocationContent (calls parent.StartConversation)
   ├── ConversationContent (receives ConversationContext)
   ├── LetterQueueContent (calls parent methods)
   └── TravelContent (calls parent methods)
   ```

5. **Method Patterns**:
   - Parent exposes public methods for state changes
   - Children call parent methods with required data
   - Parent creates contexts, switches screens, manages state
   - NO callbacks with complex signatures like `EventCallback<(string, object)>`

**Example Pattern**:
```csharp
// Parent (GameScreen)
public async Task StartConversation(string npcId, ConversationType type)
{
    CurrentConversationContext = await GameFacade.CreateConversationContext(npcId, type);
    if (CurrentConversationContext != null)
    {
        CurrentScreen = ScreenMode.Conversation;
        StateHasChanged();
    }
}

// Child (LocationContent)
[CascadingParameter] public GameScreenBase GameScreen { get; set; }

protected async Task OnNpcClick(string npcId)
{
    await GameScreen.StartConversation(npcId, ConversationType.Standard);
}
```

This architecture ensures:
- Simple, traceable data flow
- No race conditions
- Clear ownership of state
- Easy testing with mock contexts
- No complex event marshalling

*** CODE WRITING PRINCIPLES ***

*** Async/Await Philosophy (CRITICAL) ***
- **ALWAYS use async/await properly** - Never use .Wait(), .Result, or .GetAwaiter().GetResult()
- **NEVER block async code** - These patterns cause deadlocks and performance issues
- **NO Task.Run or parallel operations** - Keep everything sequential with async/await
- **If a method calls async code, it MUST be async** - Propagate async all the way up to the UI
- **NO synchronous wrappers for async methods** - Fix the callers to be async instead
- **NO deprecated methods or compatibility shims** - Delete old code, update all callers immediately

*** Error Handling Philosophy ***
- **Let exceptions bubble up** naturally for better error visibility
- NEVER THROW EXCEPTIONS
- NEVER USE TRY CATCH
- Prefer clear failures over hidden bugs

** Code Style Guidelines **

*** Code Style ***
- Self-descriptive code over excessive comments
- Comments for intent, not implementation
- **NO INLINE STYLES** - Always use separate CSS files (always scan for and read existing .css files first before creating new ones). Never use `<style>` blocks in Razor components

*** Anti-Defensive Programming Philosophy ***
- **Fail Fast**: Let exceptions bubble up naturally for clear debugging information
- **Minimal Try-Catch**: Only use try-catch when absolutely necessary for error recovery
- **No Excessive Null Checks**: Avoid defensive programming for things that should never be null
- **Assumption Validation**: It's fine to assume correctness for things that would fail during initialization and be caught in basic smoke testing

**Why**: Defensive programming hides bugs instead of revealing them. Clear exceptions with full stack traces are more valuable than swallowed errors.

**GENERAL PRINCIPLES**:
- **GAMEWORLD HAS NO DEPENDENCIES (CRITICAL)** - GameWorld is the single source of truth and must have NO dependencies on any services, managers, or external components. All dependencies flow INWARD towards GameWorld, never outward from it. GameWorld does NOT create any managers or services.
- **GAMEWORLD INITIALIZATION (CRITICAL)** - GameWorld MUST be created through a static GameWorldInitializer during startup. ServiceConfiguration MUST NOT use GetRequiredService or any DI service locator pattern to create GameWorld - this violates clean architecture and causes circular dependencies during prerendering. GameWorldInitializer must be a static class that can create GameWorld without needing dependency injection. This ensures clean initialization during startup without breaking ServerPrerendered mode or causing request hangs.
- **NAVIGATION HANDLER ARCHITECTURE (CRITICAL)** - GameUIBase (the root component at @page "/") is the ONLY navigation handler in the application. MainGameplayView is a regular component rendered by GameUIBase, NOT a navigation handler. NavigationService accepts registration via RegisterNavigationHandler() method. This pattern avoids circular dependencies while maintaining clean architecture. NO other components should implement INavigationHandler. This architectural decision prevents navigation conflicts and maintains a single point of control for all navigation operations.
- **ATTENTION SYSTEM ARCHITECTURE (CRITICAL)** - Attention is managed by TimeBlockAttentionManager and persists WITHIN time blocks (Dawn, Morning, Afternoon, Evening, Night, LateNight), NOT per conversation. GameFacade.StartConversationAsync MUST get attention from TimeBlockAttentionManager and pass it to the conversation context. This prevents the "infinite conversation exploit" where players could reset attention by starting new conversations. Attention refreshes only when the time block changes.
- **SINGLE SOURCE OF TRUTH FOR STATE** - Never duplicate state tracking across multiple objects. When you find duplicate state (e.g., location tracked in both Player and WorldState), identify which is used more frequently and make that the single source of truth. Other objects should delegate to it, not maintain their own copies.
- **never rename classes that already exist unless specifically ordered to (i.e. ConversationManager -> DeterministicConversationManager). Before creating classes, always check if classes with similar or overlapping functionality already exist**
- **NEVER use class inheritance/extensions** - Add helper methods to existing classes instead of creating subclasses
- **NEVER use interfaces, abstracts, or extensions** - Keep the code simple and direct. Use concrete classes only. No abstraction layers, no polymorphism, no fancy OOP patterns. Just direct, straightforward code.
- **UNDERSTAND BEFORE REMOVING** - Always understand the purpose of code before removing it. Determine if it's safe to remove or needs refactoring. Never assume code is redundant without understanding its context and dependencies.
- **READ ALL RELEVANT FILES BEFORE MODIFYING** - NEVER modify code without first reading ALL related files (models, repositories, managers, UI components). You MUST understand the complete data flow, types, and dependencies before making any changes. This prevents type mismatches and broken implementations.
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated
- You must run all tests and execute the game and do quick smoke tests before every commit
- **BUILD COMMANDS** - Always use `cd /mnt/c/git/wayfarer/src && dotnet build` to build the project. The pipe operator can cause issues with dotnet build output parsing. To check for errors: build first, then check the output.
- **Never keep legacy code for compatibility** - Delete it immediately and fix all callers
- **NEVER use suffixes like "New", "Revised", "V2", etc.** - Replace old implementations completely and use the correct final name immediately. Delete old code, don't leave it behind.
- **NO deprecated methods or backwards compatibility** - When changing a method signature, update ALL callers immediately. Never leave old versions around.
- **NEVER use Compile Remove in .csproj files** - This hides compilation errors and mistakes. Fix the code, rename files, or delete them entirely. Using Remove patterns in project files masks problems instead of solving them.
- **ALWAYS read files FULLY before making changes** - Never make assumptions about file contents. Read the entire file to understand context and avoid mistakes.
- **RENAME instead of DELETE/RECREATE** - When refactoring systems (e.g., Encounter → Conversation), rename files and classes to preserve git history and ensure complete transformation.
- **COMPLETE refactorings IMMEDIATELY** - Never leave systems half-renamed. If you start renaming Encounter to Conversation, finish ALL references before moving to other tasks.

*** GAME DESIGN PRINCIPLE: NO SPECIAL RULES (CRITICAL)

**"Special rules" are a design smell. When tempted to add special behavior, create new categorical mechanics instead.**

**Key Principle**: The game should have very few special rules. When you find yourself wanting to add special cases or unique behaviors, this indicates a need to enrich the existing systems rather than adding exceptions.

**📝 JSON VALIDATION BEST PRACTICES**
- **Always use case-insensitive property matching** - JSON files use camelCase while C# DTOs use PascalCase
- **Inherit from BaseValidator** - Provides TryGetPropertyCaseInsensitive helper method for robust validation
- **Test validators with actual JSON** - Don't assume field names match between JSON and DTOs

**Principles and Memories:**
- ALWAYS read the full file before editing
- game mechanical values, that could be changed during balancing, should be read from GameRules configuration file.
- strictly avoid defensive programming like checking for null values, try catch blocks, throwing exceptions, using defaults or fallback values and so on. this increases complexity of the code and hides errors. just let it fail and let the program crash fast

## CRITICAL REFACTORING RULES

  ### NO FALLBACKS
  - When implementing a subsystem, implement it COMPLETELY
  - NO placeholder methods
  - NO "will implement later" comments
  - NO partial functionality
  - If a method exists in old code, it MUST exist in new code IMMEDIATELY

  ### NO COMPATIBILITY LAYERS
  - Delete old code as soon as new code is ready
  - NO keeping both versions
  - NO switch statements choosing implementations
  - NO gradual migration
  - Cut over COMPLETELY or not at all

  ### NO TODOS
  - Every work packet produces COMPLETE, WORKING code
  - NO TODO comments in code
  - NO stub implementations
  - NO "temporary" solutions
  - Production-ready code from day one

  ### COMPLETE MIGRATION ONLY
  - When moving code, move ALL related code
  - Delete source immediately after migration
  - Update ALL references at once
  - NO partial migrations
  - NO leaving code in both places

**🔥 MANDATORY UI VERIFICATION STRATEGY 🔥**

When evaluating UI implementation, you MUST follow this EXACT process:

1. **OPEN THE MOCKUP HTML** 
   - Read EVERY element in the mockup
   - List ALL UI components shown (resources bar, headers, cards, buttons)
   - Note EXACT text and layout

2. **TAKE ACTUAL SCREENSHOTS**
   - Launch the game with Playwright
   - Navigate to the screen being evaluated
   - Take a screenshot with browser_take_screenshot
   - NEVER claim something works without a screenshot

3. **COMPARE LINE BY LINE**
   - Does the implementation have a resources bar? (Coins/Health/Hunger/Attention)
   - Are cards displayed as cards or as buttons?
   - Is the layout matching the mockup?
   - Are all data fields visible that should be visible?

4. **CHECK CORE DESIGN PRINCIPLES**
   - Exchange cards are JUST conversation cards (not special UI)
   - All costs/effects must be visible (Perfect Information principle)
   - Cards should look like cards, not buttons
   - Resources must be visible at all times

5. **BE BRUTALLY HONEST**
   - If it uses buttons instead of cards, it's WRONG
   - If resources aren't visible, it's BROKEN
   - If it doesn't match the mockup, it's NOT WORKING
   - Don't say "90% complete" when basic UI is missing
