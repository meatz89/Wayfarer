
/// <summary>
/// Provides methods to generate choices based on the current encounter state
/// </summary>
public class ChoiceGenerator
{
    private readonly Dictionary<string, Choice> _allChoices = new Dictionary<string, Choice>();

    public ChoiceGenerator()
    {
        InitializeChoices();
    }

    /// <summary>
    /// Generate 6 choices for the current encounter state according to the lookup tables
    /// </summary>
    public List<Choice> GenerateChoiceSet(EncounterState state)
    {
        // Determine which approach and focus tags to emphasize based on turn rotation
        DeterminePrimaryTags(state, out ApproachTypes primaryApproach, out FocusTypes primaryFocus);

        // Create the choice set
        List<Choice> choices = new List<Choice>();

        // Start with the two primary choices (momentum and pressure for primary approach+focus)
        AddPrimaryChoices(choices, primaryApproach, primaryFocus);

        // Determine next highest tags to use
        ApproachTypes nextApproach = GetNextHighestApproach(state, primaryApproach);
        FocusTypes nextFocus = GetNextHighestFocus(state, primaryFocus);

        // Add momentum and pressure choices for primary approach + next focus
        AddChoicesByTags(choices, primaryApproach, nextFocus, EffectTypes.Momentum);
        AddChoicesByTags(choices, primaryApproach, nextFocus, EffectTypes.Pressure);

        // Add momentum and pressure choices for next approach + primary focus
        AddChoicesByTags(choices, nextApproach, primaryFocus, EffectTypes.Momentum);
        AddChoicesByTags(choices, nextApproach, primaryFocus, EffectTypes.Pressure);

        // Ensure we meet diversity requirements
        EnsureDiversity(choices, state);

        // Ensure we have exactly 3 momentum and 3 pressure choices
        EnsureBalance(choices);

        return choices;
    }

    /// <summary>
    /// Determine the primary approach and focus tags based on turn rotation
    /// </summary>
    private void DeterminePrimaryTags(EncounterState state, out ApproachTypes primaryApproach, out FocusTypes primaryFocus)
    {
        // Get the ranked approach and focus tags
        List<ApproachTypes> rankedApproaches = GetRankedApproaches(state);
        List<FocusTypes> rankedFocuses = GetRankedFocuses(state);

        // Get the turn pattern (1-6, then repeats)
        int turnPattern = ((state.CurrentTurn - 1) % 6) + 1;

        // Select primary tags based on turn pattern
        switch (turnPattern)
        {
            case 1: // 1st highest approach + 1st highest focus
                primaryApproach = rankedApproaches[0];
                primaryFocus = rankedFocuses[0];
                break;
            case 2: // 2nd highest approach + 1st highest focus
                primaryApproach = rankedApproaches.Count > 1 ? rankedApproaches[1] : rankedApproaches[0];
                primaryFocus = rankedFocuses[0];
                break;
            case 3: // 1st highest approach + 2nd highest focus
                primaryApproach = rankedApproaches[0];
                primaryFocus = rankedFocuses.Count > 1 ? rankedFocuses[1] : rankedFocuses[0];
                break;
            case 4: // 1st highest approach + 3rd highest focus
                primaryApproach = rankedApproaches[0];
                primaryFocus = rankedFocuses.Count > 2 ? rankedFocuses[2] : rankedFocuses[0];
                break;
            case 5: // 3rd highest approach + 1st highest focus
                primaryApproach = rankedApproaches.Count > 2 ? rankedApproaches[2] : rankedApproaches[0];
                primaryFocus = rankedFocuses[0];
                break;
            case 6: // 2nd highest approach + 2nd highest focus
                primaryApproach = rankedApproaches.Count > 1 ? rankedApproaches[1] : rankedApproaches[0];
                primaryFocus = rankedFocuses.Count > 1 ? rankedFocuses[1] : rankedFocuses[0];
                break;
            default:
                primaryApproach = rankedApproaches[0];
                primaryFocus = rankedFocuses[0];
                break;
        }
    }

    /// <summary>
    /// Get approach tags ranked by value (highest to lowest)
    /// </summary>
    private List<ApproachTypes> GetRankedApproaches(EncounterState state)
    {
        return state.ApproachTags
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => GetApproachPriority(pair.Key))
            .Select(pair => pair.Key)
            .ToList();
    }

    /// <summary>
    /// Get focus tags ranked by value (highest to lowest)
    /// </summary>
    private List<FocusTypes> GetRankedFocuses(EncounterState state)
    {
        return state.FocusTags
            .OrderByDescending(pair => pair.Value)
            .ThenBy(pair => GetFocusPriority(pair.Key))
            .Select(pair => pair.Key)
            .ToList();
    }

    /// <summary>
    /// Get priority value for approach tag (used for tie-breaking)
    /// </summary>
    private int GetApproachPriority(ApproachTypes approach)
    {
        switch (approach)
        {
            case ApproachTypes.Force: return 1;
            case ApproachTypes.Charm: return 2;
            case ApproachTypes.Wit: return 3;
            case ApproachTypes.Finesse: return 4;
            case ApproachTypes.Stealth: return 5;
            default: return 99;
        }
    }

    /// <summary>
    /// Get priority value for focus tag (used for tie-breaking)
    /// </summary>
    private int GetFocusPriority(FocusTypes focus)
    {
        switch (focus)
        {
            case FocusTypes.Relationship: return 1;
            case FocusTypes.Information: return 2;
            case FocusTypes.Physical: return 3;
            case FocusTypes.Resource: return 4;
            case FocusTypes.Environment: return 5;
            default: return 99;
        }
    }

    /// <summary>
    /// Add primary momentum and pressure choices for the given approach and focus
    /// </summary>
    private void AddPrimaryChoices(List<Choice> choices, ApproachTypes approach, FocusTypes focus)
    {
        string momentumId = $"{GetApproachPrefix(approach)}-{GetFocusPrefix(focus)}-M";
        string pressureId = $"{GetApproachPrefix(approach)}-{GetFocusPrefix(focus)}-P";

        if (_allChoices.ContainsKey(momentumId))
            choices.Add(_allChoices[momentumId]);

        if (_allChoices.ContainsKey(pressureId))
            choices.Add(_allChoices[pressureId]);
    }

    /// <summary>
    /// Get next highest approach tag that contributes to diversity
    /// </summary>
    private ApproachTypes GetNextHighestApproach(EncounterState state, ApproachTypes primaryApproach)
    {
        List<ApproachTypes> ranked = GetRankedApproaches(state);

        // Find the highest ranked approach that isn't the primary
        foreach (ApproachTypes approach in ranked)
        {
            if (approach != primaryApproach)
                return approach;
        }

        // Fall back to primary if there's no alternative
        return primaryApproach;
    }

    /// <summary>
    /// Get next highest focus tag that contributes to diversity
    /// </summary>
    private FocusTypes GetNextHighestFocus(EncounterState state, FocusTypes primaryFocus)
    {
        List<FocusTypes> ranked = GetRankedFocuses(state);

        // Find the highest ranked focus that isn't the primary
        foreach (FocusTypes focus in ranked)
        {
            if (focus != primaryFocus)
                return focus;
        }

        // Fall back to primary if there's no alternative
        return primaryFocus;
    }

    /// <summary>
    /// Add a choice with the specified approach, focus, and effect type
    /// </summary>
    private void AddChoicesByTags(List<Choice> choices, ApproachTypes approach, FocusTypes focus, EffectTypes effectType)
    {
        string suffix = effectType == EffectTypes.Momentum ? "M" : "P";
        string id = $"{GetApproachPrefix(approach)}-{GetFocusPrefix(focus)}-{suffix}";

        if (_allChoices.ContainsKey(id) && !choices.Any(c => c.Id == id))
            choices.Add(_allChoices[id]);
    }

    /// <summary>
    /// Ensure the choices meet diversity requirements (3+ approach tags, 3+ focus tags)
    /// </summary>
    private void EnsureDiversity(List<Choice> choices, EncounterState state)
    {
        // Check approach diversity
        HashSet<ApproachTypes> approachesUsed = new HashSet<ApproachTypes>(choices.Select(c => c.Approach));
        if (approachesUsed.Count < 3)
        {
            // Need to add more approach diversity
            List<ApproachTypes> rankedApproaches = GetRankedApproaches(state);

            // Try to replace choices to increase approach diversity
            for (int i = 0; i < choices.Count && approachesUsed.Count < 3; i++)
            {
                Choice currentChoice = choices[i];

                // Try to find a replacement with the same effect type but different approach
                foreach (ApproachTypes approach in rankedApproaches)
                {
                    if (!approachesUsed.Contains(approach))
                    {
                        string suffix = currentChoice.EffectType == EffectTypes.Momentum ? "M" : "P";
                        string id = $"{GetApproachPrefix(approach)}-{GetFocusPrefix(currentChoice.Focus)}-{suffix}";

                        if (_allChoices.ContainsKey(id) && !choices.Any(c => c.Id == id))
                        {
                            choices[i] = _allChoices[id];
                            approachesUsed.Add(approach);
                            break;
                        }
                    }
                }
            }
        }

        // Check focus diversity
        HashSet<FocusTypes> focusesUsed = new HashSet<FocusTypes>(choices.Select(c => c.Focus));
        if (focusesUsed.Count < 3)
        {
            // Need to add more focus diversity
            List<FocusTypes> rankedFocuses = GetRankedFocuses(state);

            // Try to replace choices to increase focus diversity
            for (int i = 0; i < choices.Count && focusesUsed.Count < 3; i++)
            {
                Choice currentChoice = choices[i];

                // Try to find a replacement with the same effect type but different focus
                foreach (FocusTypes focus in rankedFocuses)
                {
                    if (!focusesUsed.Contains(focus))
                    {
                        string suffix = currentChoice.EffectType == EffectTypes.Momentum ? "M" : "P";
                        string id = $"{GetApproachPrefix(currentChoice.Approach)}-{GetFocusPrefix(focus)}-{suffix}";

                        if (_allChoices.ContainsKey(id) && !choices.Any(c => c.Id == id))
                        {
                            choices[i] = _allChoices[id];
                            focusesUsed.Add(focus);
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// Ensure we have exactly 3 momentum and 3 pressure choices
    /// </summary>
    private void EnsureBalance(List<Choice> choices)
    {
        int momentumCount = choices.Count(c => c.EffectType == EffectTypes.Momentum);

        // We need exactly 3 momentum and 3 pressure choices
        if (momentumCount < 3)
        {
            // Need to add more momentum choices
            int toAdd = 3 - momentumCount;
            int pressureCount = choices.Count(c => c.EffectType == EffectTypes.Pressure);

            // Convert pressure choices to momentum if we have too many
            if (pressureCount > 3)
            {
                for (int i = 0; i < choices.Count && toAdd > 0; i++)
                {
                    if (choices[i].EffectType == EffectTypes.Pressure)
                    {
                        string id = $"{GetApproachPrefix(choices[i].Approach)}-{GetFocusPrefix(choices[i].Focus)}-M";

                        if (_allChoices.ContainsKey(id) && !choices.Any(c => c.Id == id))
                        {
                            choices[i] = _allChoices[id];
                            toAdd--;
                        }
                    }
                }
            }
        }
        else if (momentumCount > 3)
        {
            // Need to add more pressure choices
            int toAdd = momentumCount - 3;

            // Convert momentum choices to pressure if we have too many
            for (int i = 0; i < choices.Count && toAdd > 0; i++)
            {
                if (choices[i].EffectType == EffectTypes.Momentum)
                {
                    string id = $"{GetApproachPrefix(choices[i].Approach)}-{GetFocusPrefix(choices[i].Focus)}-P";

                    if (_allChoices.ContainsKey(id) && !choices.Any(c => c.Id == id))
                    {
                        choices[i] = _allChoices[id];
                        toAdd--;
                    }
                }
            }
        }

        // Ensure we have exactly 6 choices
        if (choices.Count < 6)
        {
            // Add random choices that maintain 3/3 balance
            foreach (ApproachTypes approach in Enum.GetValues(typeof(ApproachTypes)))
            {
                foreach (FocusTypes focus in Enum.GetValues(typeof(FocusTypes)))
                {
                    if (choices.Count >= 6) break;

                    int momentumCurrentCount = choices.Count(c => c.EffectType == EffectTypes.Momentum);

                    if (momentumCurrentCount < 3)
                    {
                        string id = $"{GetApproachPrefix(approach)}-{GetFocusPrefix(focus)}-M";
                        if (_allChoices.ContainsKey(id) && !choices.Any(c => c.Id == id))
                            choices.Add(_allChoices[id]);
                    }
                    else
                    {
                        string id = $"{GetApproachPrefix(approach)}-{GetFocusPrefix(focus)}-P";
                        if (_allChoices.ContainsKey(id) && !choices.Any(c => c.Id == id))
                            choices.Add(_allChoices[id]);
                    }
                }
            }
        }

        // Truncate if we somehow ended up with more than 6
        if (choices.Count > 6)
        {
            choices = choices.Take(6).ToList();
        }
    }

    /// <summary>
    /// Get the prefix for an approach type used in choice IDs
    /// </summary>
    private string GetApproachPrefix(ApproachTypes approach)
    {
        switch (approach)
        {
            case ApproachTypes.Force: return "F";
            case ApproachTypes.Finesse: return "Fi";
            case ApproachTypes.Wit: return "W";
            case ApproachTypes.Charm: return "C";
            case ApproachTypes.Stealth: return "S";
            default: return "X";
        }
    }

    /// <summary>
    /// Get the prefix for a focus type used in choice IDs
    /// </summary>
    private string GetFocusPrefix(FocusTypes focus)
    {
        switch (focus)
        {
            case FocusTypes.Relationship: return "R";
            case FocusTypes.Information: return "I";
            case FocusTypes.Physical: return "P";
            case FocusTypes.Environment: return "E";
            case FocusTypes.Resource: return "Re";
            default: return "X";
        }
    }

    /// <summary>
    /// Initialize all 50 possible choices
    /// </summary>
    private void InitializeChoices()
    {
        // Force Approach Choices
        AddChoice("F-I-M", "Direct Interrogation", EffectTypes.Momentum, ApproachTypes.Force, FocusTypes.Information, "Forcefully demand answers to direct questions");
        AddChoice("F-I-P", "Intimidating Questions", EffectTypes.Pressure, ApproachTypes.Force, FocusTypes.Information, "Use intimidation to extract information");
        AddChoice("F-R-M", "Commanding Presence", EffectTypes.Momentum, ApproachTypes.Force, FocusTypes.Relationship, "Establish dominance in the social hierarchy");
        AddChoice("F-R-P", "Domineering Attitude", EffectTypes.Pressure, ApproachTypes.Force, FocusTypes.Relationship, "Force your will upon the other party");
        AddChoice("F-P-M", "Forceful Advance", EffectTypes.Momentum, ApproachTypes.Force, FocusTypes.Physical, "Take direct physical action with strength");
        AddChoice("F-P-P", "Overwhelming Force", EffectTypes.Pressure, ApproachTypes.Force, FocusTypes.Physical, "Use superior physical power to dominate");
        AddChoice("F-E-M", "Break Through Obstacle", EffectTypes.Momentum, ApproachTypes.Force, FocusTypes.Environment, "Overcome environmental challenges with raw power");
        AddChoice("F-E-P", "Brute Force Approach", EffectTypes.Pressure, ApproachTypes.Force, FocusTypes.Environment, "Smash through barriers with little finesse");
        AddChoice("F-Re-M", "Seize Resources", EffectTypes.Momentum, ApproachTypes.Force, FocusTypes.Resource, "Take what you need without hesitation");
        AddChoice("F-Re-P", "Demand Tribute", EffectTypes.Pressure, ApproachTypes.Force, FocusTypes.Resource, "Compel others to provide resources");

        // Finesse Approach Choices
        AddChoice("Fi-I-M", "Careful Analysis", EffectTypes.Momentum, ApproachTypes.Finesse, FocusTypes.Information, "Methodically examine details for insights");
        AddChoice("Fi-I-P", "Detailed Scrutiny", EffectTypes.Pressure, ApproachTypes.Finesse, FocusTypes.Information, "Meticulously investigate for hidden information");
        AddChoice("Fi-R-M", "Diplomatic Approach", EffectTypes.Momentum, ApproachTypes.Finesse, FocusTypes.Relationship, "Navigate social situations with grace and tact");
        AddChoice("Fi-R-P", "Cautious Negotiation", EffectTypes.Pressure, ApproachTypes.Finesse, FocusTypes.Relationship, "Carefully manage social dynamics");
        AddChoice("Fi-P-M", "Precise Movements", EffectTypes.Momentum, ApproachTypes.Finesse, FocusTypes.Physical, "Execute actions with perfect technique");
        AddChoice("Fi-P-P", "Careful Positioning", EffectTypes.Pressure, ApproachTypes.Finesse, FocusTypes.Physical, "Maintain optimal physical advantage");
        AddChoice("Fi-E-M", "Navigate Terrain", EffectTypes.Momentum, ApproachTypes.Finesse, FocusTypes.Environment, "Find the optimal path through challenging environments");
        AddChoice("Fi-E-P", "Find Alternative Path", EffectTypes.Pressure, ApproachTypes.Finesse, FocusTypes.Environment, "Discover hidden routes or approaches");
        AddChoice("Fi-Re-M", "Efficient Allocation", EffectTypes.Momentum, ApproachTypes.Finesse, FocusTypes.Resource, "Maximize the utility of available resources");
        AddChoice("Fi-Re-P", "Conserve Supplies", EffectTypes.Pressure, ApproachTypes.Finesse, FocusTypes.Resource, "Carefully manage limited resources");

        // Wit Approach Choices
        AddChoice("W-I-M", "Deduce Facts", EffectTypes.Momentum, ApproachTypes.Wit, FocusTypes.Information, "Use logic to uncover hidden truths");
        AddChoice("W-I-P", "Probe for Inconsistencies", EffectTypes.Pressure, ApproachTypes.Wit, FocusTypes.Information, "Identify contradictions in information");
        AddChoice("W-R-M", "Clever Persuasion", EffectTypes.Momentum, ApproachTypes.Wit, FocusTypes.Relationship, "Use intelligent reasoning to convince others");
        AddChoice("W-R-P", "Verbally Outmaneuver", EffectTypes.Pressure, ApproachTypes.Wit, FocusTypes.Relationship, "Win social exchanges through superior reasoning");
        AddChoice("W-P-M", "Tactical Planning", EffectTypes.Momentum, ApproachTypes.Wit, FocusTypes.Physical, "Devise intelligent strategies for physical action");
        AddChoice("W-P-P", "Exploit Weakness", EffectTypes.Pressure, ApproachTypes.Wit, FocusTypes.Physical, "Identify and target vulnerabilities");
        AddChoice("W-E-M", "Spot Environmental Advantage", EffectTypes.Momentum, ApproachTypes.Wit, FocusTypes.Environment, "Recognize useful features in surroundings");
        AddChoice("W-E-P", "Identify Hazard", EffectTypes.Pressure, ApproachTypes.Wit, FocusTypes.Environment, "Detect potential dangers in the environment");
        AddChoice("W-Re-M", "Strategic Bargaining", EffectTypes.Momentum, ApproachTypes.Wit, FocusTypes.Resource, "Negotiate favorable exchanges with clever tactics");
        AddChoice("W-Re-P", "Resource Assessment", EffectTypes.Pressure, ApproachTypes.Wit, FocusTypes.Resource, "Evaluate the true value and utility of resources");

        // Charm Approach Choices
        AddChoice("C-I-M", "Persuasive Questioning", EffectTypes.Momentum, ApproachTypes.Charm, FocusTypes.Information, "Extract information through likable demeanor");
        AddChoice("C-I-P", "Flattering Inquiry", EffectTypes.Pressure, ApproachTypes.Charm, FocusTypes.Information, "Use flattery to encourage information sharing");
        AddChoice("C-R-M", "Build Rapport", EffectTypes.Momentum, ApproachTypes.Charm, FocusTypes.Relationship, "Establish positive social connections");
        AddChoice("C-R-P", "Appeal to Emotions", EffectTypes.Pressure, ApproachTypes.Charm, FocusTypes.Relationship, "Manipulate feelings to achieve social goals");
        AddChoice("C-P-M", "Graceful Performance", EffectTypes.Momentum, ApproachTypes.Charm, FocusTypes.Physical, "Execute physical actions with charismatic flair");
        AddChoice("C-P-P", "Distracting Display", EffectTypes.Pressure, ApproachTypes.Charm, FocusTypes.Physical, "Use charm to divert attention from true intentions");
        AddChoice("C-E-M", "Set Comfortable Atmosphere", EffectTypes.Momentum, ApproachTypes.Charm, FocusTypes.Environment, "Create favorable social environment");
        AddChoice("C-E-P", "Social Flourish", EffectTypes.Pressure, ApproachTypes.Charm, FocusTypes.Environment, "Make dramatic gestures that alter the social space");
        AddChoice("C-Re-M", "Negotiate Better Terms", EffectTypes.Momentum, ApproachTypes.Charm, FocusTypes.Resource, "Use likability to secure better resource deals");
        AddChoice("C-Re-P", "Request Assistance", EffectTypes.Pressure, ApproachTypes.Charm, FocusTypes.Resource, "Convince others to share their resources");

        // Stealth Approach Choices
        AddChoice("S-I-M", "Eavesdrop Conversation", EffectTypes.Momentum, ApproachTypes.Stealth, FocusTypes.Information, "Secretly listen to gain information");
        AddChoice("S-I-P", "Observe Secretly", EffectTypes.Pressure, ApproachTypes.Stealth, FocusTypes.Information, "Watch unnoticed to gather intelligence");
        AddChoice("S-R-M", "Subtle Influence", EffectTypes.Momentum, ApproachTypes.Stealth, FocusTypes.Relationship, "Shape others' perceptions without their awareness");
        AddChoice("S-R-P", "Manipulate Indirectly", EffectTypes.Pressure, ApproachTypes.Stealth, FocusTypes.Relationship, "Pull social strings from behind the scenes");
        AddChoice("S-P-M", "Silent Movement", EffectTypes.Momentum, ApproachTypes.Stealth, FocusTypes.Physical, "Move without being detected");
        AddChoice("S-P-P", "Hidden Approach", EffectTypes.Pressure, ApproachTypes.Stealth, FocusTypes.Physical, "Conceal your physical presence");
        AddChoice("S-E-M", "Find Concealment", EffectTypes.Momentum, ApproachTypes.Stealth, FocusTypes.Environment, "Locate or create hiding places");
        AddChoice("S-E-P", "Create Diversion", EffectTypes.Pressure, ApproachTypes.Stealth, FocusTypes.Environment, "Distract attention away from your true purpose");
        AddChoice("S-Re-M", "Acquire Discreetly", EffectTypes.Momentum, ApproachTypes.Stealth, FocusTypes.Resource, "Obtain resources without drawing attention");
        AddChoice("S-Re-P", "Pilfer Resources", EffectTypes.Pressure, ApproachTypes.Stealth, FocusTypes.Resource, "Take resources through subterfuge");
    }

    /// <summary>
    /// Helper method to add a choice to the dictionary
    /// </summary>
    private void AddChoice(string id, string name, EffectTypes effectType, ApproachTypes approach, FocusTypes focus, string description)
    {
        _allChoices[id] = new Choice
        {
            Id = id,
            Name = name,
            EffectType = effectType,
            Approach = approach,
            Focus = focus,
            Description = description
        };
    }
}
