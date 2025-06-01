The payload system (and thus payload IDs as dynamic lookup keys) is no longer needed.
 * This is because the Choice Template system directly defines the singular, specific mechanical effect for each outcome (success/failure).
 * Every single mechanical effect is a separate C# class.

This radically simplifies how mechanical consequences are determined and executed, making the templates even more central and self-contained. The AI's role is to choose a template, and that choice inherently dictates the precise, non-generic mechanical effect that will occur.
Let's construct the high-level concept overview reflecting this direct, class-based approach to mechanical effects, fully driven by the template definition.
Wayfarer: AI-Driven Narrative Encounters with Directly Enforced Template Mechanics and a Polling UI
The core concept for Wayfarer is a game system where an AI Game Master (AI GM) generates dynamic, narratively rich encounters. The system masterfully balances the AI's storytelling capabilities with a highly structured mechanical framework. This is primarily achieved through a Choice Template system, where each template selected by the AI directly dictates the single, specific C# class representing the mechanical effect for its success or failure outcome, alongside its input requirements.

Key Conceptual Pillars (Reflecting All Your Inputs, Especially the Latest Clarification on Effects):
 * AI-Powered Original Narrative Generation:
   * The AI GM is the central creative force, generating unique narrative beats, descriptive text for player choices, and distinct narrative outcomes for both success and failure. This narrative is originally crafted by the AI, guided by its interpretation of the selected template's structured definition (which is provided to it as JSON).
 * Backend State Management & Orchestration:
   * A robust C#-based backend (driven by a GameWorldManager and supported by an EncounterSystem) manages all game logic and the canonical GameWorld state.
   * GameWorld includes player data, world state, active encounter state, and a dedicated StreamingContentState for AI-generated text.
 * Polling Blazor UI (Decoupled Frontend):
   * The Blazor UI operates on a strict polling mechanism, querying the backend's GameState (via a snapshot from GameWorldManager) at regular intervals (e.g., every ~100ms) to display changes.
   * No direct backend-to-UI events or notifications are used for state updates; the UI pulls information.
 * Streaming AI Responses for Incremental UI Updates:
   * AI-generated text is streamed token-by-token into GameWorld.StreamingContentState.
   * The polling UI displays these incremental updates, creating a live text effect.
   * An AIStreamingService (or similar component within AIGameMaster) handles the streaming protocol.
 * Choice Templates: Self-Contained Packages of Mechanics & Conceptual Guidance (via JSON):
   * A catalog of ChoiceTemplate C# objects defines the available choice archetypes.
   * Each ChoiceTemplate contains:
     * A unique TemplateName (ID).
     * A StrategicPurpose and descriptive hints for AI understanding.
     * A Weight (hinting at desired frequency for AI selection).
     * Complex C# objects detailing InputMechanics (e.g., specific FocusCost, SkillCheckRequirement with concrete SCD and skill category), serialized to JSON for the AI.
     * A direct reference to the specific C# class (e.g., via System.Type or a specific enum mapping to a class) for the single mechanical effect upon success (e.g., Type SuccessEffectClass = typeof(SetStandardProgressFlagEffect);).
     * A direct reference to the specific C# class for the single mechanical effect upon failure (e.g., Type FailureEffectClass = typeof(ApplyMinorSetbackEffect);).
     * Conceptual descriptions of the output and success/failure consequences (e.g., ConceptualOutput, SuccessOutcomeNarrativeGuidance) to guide the AI's narrative generation around these fixed mechanical effects. These are also provided as JSON to the AI.
   * Templates do NOT contain generic strings for effects or lists of payload IDs. They point directly to the code that is the effect.
 * AI Autonomy in Template Selection & Narrative Instantiation (Mechanics are Template-Fixed):
   * For each encounter beat, the AI GM receives the entire catalog of all available ChoiceTemplate objects, with their input mechanics and conceptual outputs presented as embedded JSON structures, within its prompt.
   * The AI is instructed to select 3-4 appropriate templates.
   * For each selected template, the AI generates:
     * Original narrative text for the player choice.
     * Original narrative text for the success and failure outcomes.
     * The specific mechanical values for the choice's inputs (focusCost, skillOptions including SCD), which it must derive directly from the JSON definition of the chosen template's InputMechanics.
     * The AI must specify the templateUsed (by TemplateName) in its response.
     * The AI does NOT specify the mechanical outcome/effect. This is now entirely determined by the system looking up the SuccessEffectClass or FailureEffectClass associated with the templateUsed.
   * The game system does not pre-filter templates for the AI, nor does it validate or correct the AI's output for input mechanics (like focus cost or SCD). It trusts the AI to have correctly instantiated these from the template's JSON definition.
 * Structured AI Interaction & Response Processing:
   * The AIPromptBuilder provides the AI with game context and the full template catalog (as JSON objects defining inputs and conceptual outputs/guidance). Clear instructions are given on selecting templates, instantiating their input mechanics, generating original narrative, and structuring the JSON response (including templateUsed, focusCost, SCD).
   * The AIResponseProcessor parses the AI's JSON response. For each AI choice, it identifies the templateUsed.
 * Direct Execution of Template-Defined Effect Classes:
   * Core game elements (Focus Points, Skills, UESTs) are manipulated.
   * When a player's choice (based on an AI-selected template) is resolved (e.g., skill check succeeds or fails):
     * The system retrieves the ChoiceTemplate object corresponding to the templateUsed by the AI.
     * It then instantiates and executes the single, specific effect class (e.g., template.SuccessEffectClass.Apply(...) or template.FailureEffectClass.Apply(...)) associated with that template's outcome.
   * There is no PayloadRegistry for string ID lookups. The "payload" is now an actual C# effect class directly tied to the template definition.

This refined understanding places the Choice Template as an even more powerful and direct controller of game mechanics. The AI's role is to select these potent template packages, generate compelling original narrative around their fixed input mechanics and conceptual outcomes, and correctly specify the input mechanics in its response. The system then executes the specific, singular C# effect class hardwired to that template's success or failure path. This ensures absolute mechanical consistency per template choice.