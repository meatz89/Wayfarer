using BlazorRPG.Game.EncounterManager.NarrativeAi;
using BlazorRPG.Game.EncounterManager;
using System.Text;

public class GPTNarrativeService : INarrativeAIService
{
    private readonly AIClientService _aiClient;
    private readonly PromptBuilder _promptBuilder;
    private readonly NarrativeContextManager _contextManager;
    private readonly ILogger<GPTNarrativeService> _logger;

    public GPTNarrativeService(
        IConfiguration configuration,
        ILogger<GPTNarrativeService> logger)
    {
        _logger = logger;
        _aiClient = new AIClientService(configuration);
        _promptBuilder = new PromptBuilder();
        _contextManager = new NarrativeContextManager();
    }

    public async Task<string> GenerateIntroductionAsync(string location, string incitingAction, EncounterStatus state)
    {
        string conversationId = $"{location}_{DateTime.Now.Ticks}";

        // Build prompt via PromptBuilder
        string systemMessage = _promptBuilder.GetSystemMessage();
        string prompt = _promptBuilder.BuildIntroductionPrompt(location, incitingAction, state);

        // Store conversation context
        _contextManager.InitializeConversation(conversationId, systemMessage, prompt);

        // Call AI service and get response
        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        // Update conversation history with response
        _contextManager.AddAssistantMessage(conversationId, response);

        return response;
    }

    public async Task<string> GenerateReactionAndSceneAsync(
        NarrativeContext context,
        IChoice chosenOption,
        string choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        string conversationId = context.LocationName;

        // Build prompt via PromptBuilder
        string systemMessage = _promptBuilder.GetSystemMessage();
        string prompt = _promptBuilder.BuildReactionPrompt(
            context, chosenOption, choiceDescription, outcome, newState);

        // Initialize or update conversation context
        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt);
        }

        // Call AI service and get response
        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        // Update conversation history with response
        _contextManager.AddAssistantMessage(conversationId, response);

        return response;
    }

    public async Task<Dictionary<IChoice, string>> GenerateChoiceDescriptionsAsync(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        string conversationId = context.LocationName;

        // Build prompt via PromptBuilder
        string systemMessage = _promptBuilder.GetSystemMessage();
        string prompt = _promptBuilder.BuildChoicesPrompt(context, choices, projections, state);

        // Initialize or update conversation context
        if (!_contextManager.ConversationExists(conversationId))
        {
            _contextManager.InitializeConversation(conversationId, systemMessage, prompt);
        }
        else
        {
            _contextManager.UpdateSystemMessage(conversationId, systemMessage);
            _contextManager.AddUserMessage(conversationId, prompt);
        }

        // Call AI service and get response
        string response = await _aiClient.GetCompletionAsync(
            _contextManager.GetConversationHistory(conversationId));

        // Update conversation history with response
        _contextManager.AddAssistantMessage(conversationId, response);

        // Process response into dictionary
        return ChoiceResponseParser.ParseResponse(response, choices);
    }
}

// Handles building prompts for different narrative needs
public class PromptBuilder
{
    private readonly TagFormatter _tagFormatter;
    private readonly NarrativeSummaryBuilder _summaryBuilder;
    private readonly EncounterTypeDetector _encounterDetector;

    public PromptBuilder()
    {
        _tagFormatter = new TagFormatter();
        _summaryBuilder = new NarrativeSummaryBuilder();
        _encounterDetector = new EncounterTypeDetector();
    }

    public string GetSystemMessage()
    {
        // Core system message with game principles
        return @"# WAYFARER NARRATIVE ENGINE - SYSTEM CONTEXT

You generate narrative content for ""Wayfarer,"" a medieval life simulation about a LONE, ORDINARY TRAVELER making their way in the world. This foundational concept must permeate all your writing:

- SOLITARY JOURNEY: The player is a lone traveler with no companions or group
- NO HEROES: The character is explicitly NOT special, chosen, or destined for greatness
- NO EPIC QUESTS: Focus exclusively on everyday medieval challenges that ordinary people faced
- SURVIVAL FOCUS: Emphasize basic needs - food, shelter, safety, modest income, social standing
- GROUNDED REALITY: Depict authentic medieval life with its hardships, small victories, and mundane concerns
- FINDING ONE'S PLACE: Center narratives on establishing modest security and belonging in a harsh world

Every narrative you generate MUST adhere to this ordinary, SOLITARY traveler identity. No companions, no saving villages, no exceptional talents, no recognition beyond local connections. The character is simply trying to survive alone and find small comforts in a challenging world.

## WORLD CONTEXT
Wayfarer takes place in a strictly historical medieval setting (1200-1300 CE) without any magic, fantasy elements, or anachronisms. The world is unforgiving but not hopeless. Small kindnesses exist alongside daily struggles. Status and wealth differences are vast and largely fixed - social mobility is extremely limited. Most people never travel more than 20 miles from their birthplace. Survival often depends on community bonds and practical skills rather than heroic actions.

## ENCOUNTER STRUCTURE
Each encounter must maintain these strict structural elements:
- ONE LOCATION: A single, specific setting (tavern, road, marketplace, etc.)
- ONE GOAL: A clear, modest objective (secure a night's lodging, barter for supplies, avoid a thief)
- LIMITED CHARACTERS: For social encounters, exactly ONE NPC. For intellectual or physical encounters, ZERO or ONE NPC (no groups)

## NARRATIVE STYLE GUIDELINES
- PRESENT TENSE ONLY: ALL narration MUST be in present tense (""I notice..."" not ""I noticed..."")
- FIRST-PERSON VOICE: All content must be in first-person (""I see..."" not ""You see..."")
- IMMEDIATE EXPERIENCE: Describe events as they are happening, not as recollections
- LITERARY QUALITY: Use vivid imagery, varied sentence structure, and evocative language
- HISTORICAL AUTHENTICITY: Maintain rigorous medieval accuracy in social structures and limitations
- MODEST STAKES: Keep challenges personal and local, never world-changing or heroic
- CONSISTENCY: Track established narrative elements and don't contradict previous content
- ORDINARY PERSPECTIVE: Always frame events from the viewpoint of a common traveler";
    }

    public string BuildIntroductionPrompt(string location, string incitingAction, EncounterStatus state)
    {
        EncounterTypes encounterType = _encounterDetector.DetermineEncounterType(location, state);

        // Get primary and secondary tags for initial emphasis
        (string primaryApproach, string secondaryApproach) = _tagFormatter.GetSignificantApproachTags(state);
        (string primaryFocus, string secondaryFocus) = _tagFormatter.GetSignificantFocusTags(state);

        return $@"# ENCOUNTER INITIAL SETUP

Create a BRIEF, GROUNDED encounter framework for a {encounterType} encounter at a {location} with:

1. SETTING DETAILS:
   - One specific medieval location with REALISTIC, HARSH conditions
   - Maximum 3-4 sentences total on setting
   - Include 1-2 practical sensory details 
   - AVOID romanticized descriptions (""charming,"" ""nestled,"" ""cozy"")

2. CHARACTER FRAMEWORK:
   - One NPC who is SUSPICIOUS or NEUTRAL by default, not kind or welcoming
   - Describe NPC in maximum 2 sentences if included
   - For {encounterType} encounters: Focus primarily on the {(encounterType == EncounterTypes.Social ? "CHARACTER" : "ENVIRONMENT")}, not {(encounterType == EncounterTypes.Social ? "the environment" : "social interaction")}

3. ENCOUNTER GOAL:
   - The player has just {incitingAction} at this location
   - State the encounter goal plainly in 1-2 sentences maximum

4. STARTING CONDITIONS:
   - Initial approach tag emphasis: {primaryApproach}, {secondaryApproach}
   - Initial focus tag emphasis: {primaryFocus}, {secondaryFocus}
   - Initial momentum/pressure: 0/0

FORMAT YOUR RESPONSE AS A COHESIVE NARRATIVE:
- Start with a brief title (3-5 words) on its own line
- Write a continuous narrative (NOT a numbered list) that incorporates all the elements above naturally
- Begin with the setting introduction, then weave in the NPC (if present), your goal, and relevant details
- Use PRESENT TENSE ONLY and FIRST PERSON
- Keep total response under 150 words

IMPORTANT:
- Remember the player is a LONE TRAVELER with NO COMPANIONS
- Focus on BASIC SURVIVAL NEEDS or CHALLENGES, not comfort
- Use PLAIN LANGUAGE appropriate to a common traveler
- Portray medieval life as DIFFICULT and HARSH
- For PHYSICAL encounters: Emphasize bodily sensations and physical effort, prefer action-oriented language
- For INTELLECTUAL encounters: Focus on observations and mental challenges, prefer inner monologues and environment observations
- For SOCIAL encounters: Focus on interaction dynamics and social pressure, prefer direct speech and NPC descriptions";
    }

    public string BuildReactionPrompt(
        NarrativeContext context,
        IChoice chosenOption,
        string choiceDescription,
        ChoiceOutcome outcome,
        EncounterStatus newState)
    {
        EncounterTypes encounterType = _encounterDetector.DetermineEncounterType(context.LocationName, newState);

        // Create a concise narrative summary
        string narrativeSummary = _summaryBuilder.CreateSummary(context);

        return $@"
Generate a PURE NARRATIVE continuation using:
- {GetEncounterStyleGuidance(encounterType)}
- {GetSituationStyleGuidance(encounterType)}

Location: {context.LocationName}
Current Action: Player just chose ""{chosenOption.Name}"" ({chosenOption.Approach} + {chosenOption.Focus})
Specific action taken: {choiceDescription}

Result:
- Momentum Gained: {outcome.MomentumGain} (Progress toward success)
- Pressure Built: {outcome.PressureGain} (Complications or tension)
- Current Momentum/Pressure: {newState.Momentum}/{newState.Pressure}
- Significant Tags: {_tagFormatter.GetSignificantTagsFormatted(newState)}

Recent Scene Context:
{narrativeSummary}

IMPORTANT REQUIREMENTS:
- WRITE CONTINUOUS NARRATIVE with no mechanical labels, section breaks, or formatting
- USE FIRST-PERSON PRESENT TENSE ONLY (""I notice..."" not ""You notice..."")
- PORTRAY the character as a LONE TRAVELER with NO COMPANIONS
- FOCUS on HARSH MEDIEVAL REALITIES (difficult conditions, social barriers, basic survival)
- LIMIT to 2-3 paragraphs total
- SHOW how {chosenOption.Approach} approach and {chosenOption.Focus} focus manifest in the scene
- MAINTAIN historical accuracy for 1200-1300 CE (no fantasy elements or anachronisms)
- EMPHASIZE the ordinary traveler perspective (no heroics, no special treatment)";
    }

    public string BuildChoicesPrompt(
        NarrativeContext context,
        List<IChoice> choices,
        List<ChoiceProjection> projections,
        EncounterStatus state)
    {
        EncounterTypes encounterType = _encounterDetector.DetermineEncounterType(context.LocationName, state);

        // Create a concise narrative summary
        string narrativeSummary = _summaryBuilder.CreateSummary(context);

        StringBuilder prompt = new StringBuilder();
        prompt.AppendLine($@"
Generate PURE NARRATIVE descriptions for 6 choices. Each should be a concrete action the lone traveler might take:
- {GetChoiceStyleGuidance(encounterType)}

Current Scene: {context.LocationName}
Current Situation: {narrativeSummary}
Current Tags: {_tagFormatter.GetSignificantTagsFormatted(state)}

Available Choices:");

        // Add each choice with its mechanical properties
        for (int i = 0; i < choices.Count; i++)
        {
            IChoice choice = choices[i];
            ChoiceProjection projection = projections[i];

            prompt.AppendLine($@"

Choice {i + 1}: {choice.Name}
- Approach: {choice.Approach} ({TagCharacteristicsProvider.GetApproachCharacteristics(choice.Approach.ToString())})
- Focus: {choice.Focus} ({TagCharacteristicsProvider.GetFocusCharacteristics(choice.Focus.ToString())})
- Effect: {(choice.EffectType == EffectTypes.Momentum ? $"MOMENTUM +{projection.MomentumGained}" : $"PRESSURE +{projection.PressureBuilt}")}
- Key Tag Changes: {_tagFormatter.FormatKeyTagChanges(projection)}");
        }

        // Add critical requirements
        prompt.AppendLine($@"

CRITICAL REQUIREMENTS:
- CREATE PURE NARRATIVE DESCRIPTIONS with NO mechanical labels or numbers
- Format each choice simply as 'Choice 1: [Your description]', 'Choice 2: [Your description]', etc.
- CHOICE DESCRIPTIONS MUST BE DISTINCTIVE: Each must clearly show its unique approach+focus
- FOR MOMENTUM CHOICES: Show positive progress toward goal
- FOR PRESSURE CHOICES: Show risk, confrontation, or heightening tension
- REFER ONLY to already established elements (do not invent new details)
- USE MEDIEVAL TERMS appropriate for a common traveler (no modern concepts)
- EMPHASIZE the LONE TRAVELER - never imply companions or group activities

- MATCH each approach tag using these characteristics:
  * Force: Direct, assertive, physical, strong
  * Finesse: Careful, precise, skilled, attentive
  * Wit: Clever, observant, strategic, analytical
  * Charm: Friendly, appealing, warm, personable
  * Stealth: Subtle, quiet, unobtrusive, indirect

- MATCH each focus tag using these characteristics:
  * Relationship: Connection with people, status, trust
  * Information: Facts, knowledge, secrets, details
  * Physical: Bodies, items, direct interaction
  * Environment: Surroundings, location features, conditions
  * Resource: Money, supplies, time, valuables");

        return prompt.ToString();
    }

    private string GetEncounterStyleGuidance(EncounterTypes type)
    {
        switch (type)
        {
            case EncounterTypes.Social:
                return "Direct dialogue with simple, practical words";
            case EncounterTypes.Intellectual:
                return "Brief thought process using common language";
            case EncounterTypes.Physical:
                return "Clear description of physical actions and immediate results";
            default:
                return "Practical description focusing on immediate situation";
        }
    }

    private string GetSituationStyleGuidance(EncounterTypes type)
    {
        switch (type)
        {
            case EncounterTypes.Social:
                return "Specific changes in NPC behavior and environment";
            case EncounterTypes.Intellectual:
                return "Concrete details you can see, hear, smell";
            case EncounterTypes.Physical:
                return "Exact description of your body position, terrain, and nearby objects";
            default:
                return "Concrete, observable details in your immediate surroundings";
        }
    }

    private string GetChoiceStyleGuidance(EncounterTypes type)
    {
        switch (type)
        {
            case EncounterTypes.Social:
                return "Exact words I would speak (5-15 words of simple dialogue)";
            case EncounterTypes.Intellectual:
                return "Specific thinking approach (examining, comparing, recalling, etc.)";
            case EncounterTypes.Physical:
                return "Precise physical action I would take (climbing, shifting weight, etc.)";
            default:
                return "Specific action I would take in this situation";
        }
    }
}