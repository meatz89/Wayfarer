# CLAUDE.md

**⚠️ MANDATORY: READ THE ENTIRE CLAUDE.MD FILE BEFORE WRITING TO IT ⚠️**

**⚠️ CRITICAL: ALWAYS READ THE FULL FILE BEFORE MODIFYING IT ⚠️**
**NEVER make changes to a file without reading it completely first. This is non-negotiable.**

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

### CODE WRITING PRINCIPLES

**TRANSFORMATION APPROACH**:
- **RENAME AND RECONTEXTUALIZE** - Don't wrap new functionality in old classes, rename them to reflect new purpose
- **DELETE LEGACY CODE ENTIRELY** - Remove old contract system, reputation system, favor system completely
- **NO COMPATIBILITY LAYERS** - Clean break from old mechanics to new queue/token system
- **FRESH TEST SUITE** - Delete old tests and write new ones for queue/token mechanics

**GENERAL PRINCIPLES**:
- **never rename classes that already exist unless specifically ordered to (i.e. ConversationManager -> DeterministicConversationManager). Before creating classes, always check if classes with similar or overlapping functionality already exist**
- **NEVER use class inheritance/extensions** - Add helper methods to existing classes instead of creating subclasses
- **UNDERSTAND BEFORE REMOVING** - Always understand the purpose of code before removing it. Determine if it's safe to remove or needs refactoring. Never assume code is redundant without understanding its context and dependencies.
- **READ ALL RELEVANT FILES BEFORE MODIFYING** - NEVER modify code without first reading ALL related files (models, repositories, managers, UI components). You MUST understand the complete data flow, types, and dependencies before making any changes. This prevents type mismatches and broken implementations.
- Do not leave comments in code that are not TODOs or SERIOUSLY IMPORTANT
- After each change, run the tests to check for broken functionality. Never commit while tests are failing
- **ALWAYS write unit tests confirming errors before fixing them** - This ensures the bug is properly understood and the fix is validated
- You must run all tests and execute the game and do quick smoke tests before every commit
- **Never keep legacy code for compatibility**
- **NEVER use suffixes like "New", "Revised", "V2", etc.** - Replace old implementations completely and use the correct final name immediately. Delete old code, don't leave it behind.
- **NEVER use Compile Remove in .csproj files** - This hides compilation errors and mistakes. Fix the code, rename files, or delete them entirely. Using Remove patterns in project files masks problems instead of solving them.
- **ALWAYS read files FULLY before making changes** - Never make assumptions about file contents. Read the entire file to understand context and avoid mistakes.
- **RENAME instead of DELETE/RECREATE** - When refactoring systems (e.g., Encounter → Conversation), rename files and classes to preserve git history and ensure complete transformation.
- **COMPLETE refactorings IMMEDIATELY** - Never leave systems half-renamed. If you start renaming Encounter to Conversation, finish ALL references before moving to other tasks.

### GAME DESIGN PRINCIPLE: NO SPECIAL RULES (CRITICAL)

**"Special rules" are a design smell. When tempted to add special behavior, create new categorical mechanics instead.**

**Key Principle**: The game should have very few special rules. When you find yourself wanting to add special cases or unique behaviors, this indicates a need to enrich the existing systems rather than adding exceptions.

**Examples of the Wrong Approach:**
- ❌ "Patron letters always go to position 1" - Special rule
- ❌ "Noble letters can't be refused" - Special exception
- ❌ "Shadow NPCs offer illegal work when player is desperate" - Conditional special case

**The Right Approach - Categorical Mechanics:**
- ✅ Create a "Leverage" system where debt affects ALL letter positions
- ✅ Add "Obligation" mechanics that modify ALL queue behaviors
- ✅ Use "Desperation" as a player state that affects ALL NPC interactions

**Design Process:**
1. **Identify the special case** - "I want patron letters to be high priority"
2. **Ask why** - "Because the patron has power over the player"
3. **Generalize the concept** - "Power dynamics affect letter priority"
4. **Create categorical system** - "Leverage system: debt creates priority for ALL NPCs"
5. **Special case becomes regular case** - "Patron starts with high leverage due to employment debt"

**Benefits:**
- Emergent gameplay from system interactions
- Players discover strategies rather than memorizing exceptions
- Code remains clean and general
- New content automatically inherits system behaviors