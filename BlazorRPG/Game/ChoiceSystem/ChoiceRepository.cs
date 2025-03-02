
/// <summary>
/// Repository of all possible choices in the system
/// </summary>
public class ChoiceRepository
{
    private readonly Dictionary<string, Choice> _choices = new Dictionary<string, Choice>();

    public ChoiceRepository()
    {
        InitializeChoices();
    }

    public Choice GetChoice(string id)
    {
        return _choices.ContainsKey(id) ? _choices[id] : null;
    }

    public IEnumerable<Choice> GetAllChoices()
    {
        return _choices.Values;
    }

    public IEnumerable<Choice> GetChoicesByApproach(ApproachTypes approach)
    {
        return _choices.Values.Where(c => c.ApproachType == approach);
    }

    public IEnumerable<Choice> GetChoicesByFocus(FocusTypes focus)
    {
        return _choices.Values.Where(c => c.FocusType == focus);
    }

    public IEnumerable<Choice> GetChoicesByEffect(EffectTypes effect)
    {
        return _choices.Values.Where(c => c.EffectType == effect);
    }

    public Choice GetChoice(ApproachTypes approach, FocusTypes focus, EffectTypes effect)
    {
        string prefix = GetApproachPrefix(approach);
        string focusSuffix = GetFocusSuffix(focus);
        string effectSuffix = effect == EffectTypes.Momentum ? "M" : "P";

        string id = $"{prefix}-{focusSuffix}-{effectSuffix}";

        return GetChoice(id);
    }

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

    private string GetFocusSuffix(FocusTypes focus)
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

    private void InitializeChoices()
    {
        // Force Approach Choices
        AddChoice("F-I-M", "Direct Questioning", ApproachTypes.Force, FocusTypes.Information, EffectTypes.Momentum);
        AddChoice("F-I-P", "Intimidating Questions", ApproachTypes.Force, FocusTypes.Information, EffectTypes.Pressure);
        AddChoice("F-R-M", "Commanding Presence", ApproachTypes.Force, FocusTypes.Relationship, EffectTypes.Momentum);
        AddChoice("F-R-P", "Domineering Attitude", ApproachTypes.Force, FocusTypes.Relationship, EffectTypes.Pressure);
        AddChoice("F-P-M", "Forceful Advance", ApproachTypes.Force, FocusTypes.Physical, EffectTypes.Momentum);
        AddChoice("F-P-P", "Overwhelming Force", ApproachTypes.Force, FocusTypes.Physical, EffectTypes.Pressure);
        AddChoice("F-E-M", "Overcome Barrier", ApproachTypes.Force, FocusTypes.Environment, EffectTypes.Momentum);
        AddChoice("F-E-P", "Brute Force Approach", ApproachTypes.Force, FocusTypes.Environment, EffectTypes.Pressure);
        AddChoice("F-Re-M", "Seize Resources", ApproachTypes.Force, FocusTypes.Resource, EffectTypes.Momentum);
        AddChoice("F-Re-P", "Demand Resources", ApproachTypes.Force, FocusTypes.Resource, EffectTypes.Pressure);

        // Finesse Approach Choices
        AddChoice("Fi-I-M", "Careful Analysis", ApproachTypes.Finesse, FocusTypes.Information, EffectTypes.Momentum);
        AddChoice("Fi-I-P", "Detailed Scrutiny", ApproachTypes.Finesse, FocusTypes.Information, EffectTypes.Pressure);
        AddChoice("Fi-R-M", "Diplomatic Approach", ApproachTypes.Finesse, FocusTypes.Relationship, EffectTypes.Momentum);
        AddChoice("Fi-R-P", "Cautious Negotiation", ApproachTypes.Finesse, FocusTypes.Relationship, EffectTypes.Pressure);
        AddChoice("Fi-P-M", "Precise Movements", ApproachTypes.Finesse, FocusTypes.Physical, EffectTypes.Momentum);
        AddChoice("Fi-P-P", "Careful Positioning", ApproachTypes.Finesse, FocusTypes.Physical, EffectTypes.Pressure);
        AddChoice("Fi-E-M", "Navigate Environment", ApproachTypes.Finesse, FocusTypes.Environment, EffectTypes.Momentum);
        AddChoice("Fi-E-P", "Find Alternative Path", ApproachTypes.Finesse, FocusTypes.Environment, EffectTypes.Pressure);
        AddChoice("Fi-Re-M", "Efficient Allocation", ApproachTypes.Finesse, FocusTypes.Resource, EffectTypes.Momentum);
        AddChoice("Fi-Re-P", "Conserve Resources", ApproachTypes.Finesse, FocusTypes.Resource, EffectTypes.Pressure);

        // Wit Approach Choices
        AddChoice("W-I-M", "Deduce Facts", ApproachTypes.Wit, FocusTypes.Information, EffectTypes.Momentum);
        AddChoice("W-I-P", "Probe for Inconsistencies", ApproachTypes.Wit, FocusTypes.Information, EffectTypes.Pressure);
        AddChoice("W-R-M", "Clever Persuasion", ApproachTypes.Wit, FocusTypes.Relationship, EffectTypes.Momentum);
        AddChoice("W-R-P", "Outmaneuver Intellectually", ApproachTypes.Wit, FocusTypes.Relationship, EffectTypes.Pressure);
        AddChoice("W-P-M", "Tactical Planning", ApproachTypes.Wit, FocusTypes.Physical, EffectTypes.Momentum);
        AddChoice("W-P-P", "Exploit Weakness", ApproachTypes.Wit, FocusTypes.Physical, EffectTypes.Pressure);
        AddChoice("W-E-M", "Identify Advantage", ApproachTypes.Wit, FocusTypes.Environment, EffectTypes.Momentum);
        AddChoice("W-E-P", "Identify Hazard", ApproachTypes.Wit, FocusTypes.Environment, EffectTypes.Pressure);
        AddChoice("W-Re-M", "Strategic Resource Use", ApproachTypes.Wit, FocusTypes.Resource, EffectTypes.Momentum);
        AddChoice("W-Re-P", "Resource Assessment", ApproachTypes.Wit, FocusTypes.Resource, EffectTypes.Pressure);

        // Charm Approach Choices
        AddChoice("C-I-M", "Persuasive Questioning", ApproachTypes.Charm, FocusTypes.Information, EffectTypes.Momentum);
        AddChoice("C-I-P", "Flattering Inquiry", ApproachTypes.Charm, FocusTypes.Information, EffectTypes.Pressure);
        AddChoice("C-R-M", "Build Rapport", ApproachTypes.Charm, FocusTypes.Relationship, EffectTypes.Momentum);
        AddChoice("C-R-P", "Appeal to Emotions", ApproachTypes.Charm, FocusTypes.Relationship, EffectTypes.Pressure);
        AddChoice("C-P-M", "Graceful Execution", ApproachTypes.Charm, FocusTypes.Physical, EffectTypes.Momentum);
        AddChoice("C-P-P", "Distracting Behavior", ApproachTypes.Charm, FocusTypes.Physical, EffectTypes.Pressure);
        AddChoice("C-E-M", "Create Favorable Conditions", ApproachTypes.Charm, FocusTypes.Environment, EffectTypes.Momentum);
        AddChoice("C-E-P", "Dramatic Gesture", ApproachTypes.Charm, FocusTypes.Environment, EffectTypes.Pressure);
        AddChoice("C-Re-M", "Negotiate Better Terms", ApproachTypes.Charm, FocusTypes.Resource, EffectTypes.Momentum);
        AddChoice("C-Re-P", "Request Assistance", ApproachTypes.Charm, FocusTypes.Resource, EffectTypes.Pressure);

        // Stealth Approach Choices
        AddChoice("S-I-M", "Gather Information Secretly", ApproachTypes.Stealth, FocusTypes.Information, EffectTypes.Momentum);
        AddChoice("S-I-P", "Observe Secretly", ApproachTypes.Stealth, FocusTypes.Information, EffectTypes.Pressure);
        AddChoice("S-R-M", "Subtle Influence", ApproachTypes.Stealth, FocusTypes.Relationship, EffectTypes.Momentum);
        AddChoice("S-R-P", "Manipulate Indirectly", ApproachTypes.Stealth, FocusTypes.Relationship, EffectTypes.Pressure);
        AddChoice("S-P-M", "Move Unnoticed", ApproachTypes.Stealth, FocusTypes.Physical, EffectTypes.Momentum);
        AddChoice("S-P-P", "Hidden Approach", ApproachTypes.Stealth, FocusTypes.Physical, EffectTypes.Pressure);
        AddChoice("S-E-M", "Find Concealment", ApproachTypes.Stealth, FocusTypes.Environment, EffectTypes.Momentum);
        AddChoice("S-E-P", "Create Diversion", ApproachTypes.Stealth, FocusTypes.Environment, EffectTypes.Pressure);
        AddChoice("S-Re-M", "Acquire Discreetly", ApproachTypes.Stealth, FocusTypes.Resource, EffectTypes.Momentum);
        AddChoice("S-Re-P", "Take Resources Covertly", ApproachTypes.Stealth, FocusTypes.Resource, EffectTypes.Pressure);
    }

    private void AddChoice(string id, string name, ApproachTypes approach, FocusTypes focus, EffectTypes effect)
    {
        _choices[id] = new Choice(id, name, approach, focus, effect);
    }
}
