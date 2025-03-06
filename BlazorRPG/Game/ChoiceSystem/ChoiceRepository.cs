
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
        AddChoice("F-I-M", "Direct Assertion (+momentum, +dominance)", ApproachTypes.Force, FocusTypes.Information, EffectTypes.Momentum);
        AddChoice("F-I-P", "Forceful Interrogation (+pressure, ++dominance)", ApproachTypes.Force, FocusTypes.Information, EffectTypes.Pressure);
        AddChoice("F-R-M", "Dominant Engagement (+momentum, +dominance)", ApproachTypes.Force, FocusTypes.Relationship, EffectTypes.Momentum);
        AddChoice("F-R-P", "Intimidate Opposition (+pressure, ++dominance)", ApproachTypes.Force, FocusTypes.Relationship, EffectTypes.Pressure);
        AddChoice("F-P-M", "Physical Imposition (+momentum, +dominance)", ApproachTypes.Force, FocusTypes.Physical, EffectTypes.Momentum);
        AddChoice("F-P-P", "Overwhelming Maneuver (+pressure, ++dominance)", ApproachTypes.Force, FocusTypes.Physical, EffectTypes.Pressure);
        AddChoice("F-E-M", "Environmental Force (+momentum, +dominance)", ApproachTypes.Force, FocusTypes.Environment, EffectTypes.Momentum);
        AddChoice("F-E-P", "Forcefully Clear Path (+pressure, ++dominance)", ApproachTypes.Force, FocusTypes.Environment, EffectTypes.Pressure);
        AddChoice("F-Re-M", "Forceful Acquisition (+momentum, +dominance)", ApproachTypes.Force, FocusTypes.Resource, EffectTypes.Momentum);
        AddChoice("F-Re-P", "Demand Assets (+pressure, ++dominance)", ApproachTypes.Force, FocusTypes.Resource, EffectTypes.Pressure);

        // Finesse Approach Choices
        AddChoice("Fi-I-M", "Precise Assessment (+momentum, +precision)", ApproachTypes.Finesse, FocusTypes.Information, EffectTypes.Momentum);
        AddChoice("Fi-I-P", "Careful Examination (+pressure, ++precision)", ApproachTypes.Finesse, FocusTypes.Information, EffectTypes.Pressure);
        AddChoice("Fi-R-M", "Skillful Connection (+momentum, +precision)", ApproachTypes.Finesse, FocusTypes.Relationship, EffectTypes.Momentum);
        AddChoice("Fi-R-P", "Tactical Approach (+pressure, ++precision)", ApproachTypes.Finesse, FocusTypes.Relationship, EffectTypes.Pressure);
        AddChoice("Fi-P-M", "Precise Execution (+momentum, +precision)", ApproachTypes.Finesse, FocusTypes.Physical, EffectTypes.Momentum);
        AddChoice("Fi-P-P", "Measured Adjustment (+pressure, ++precision)", ApproachTypes.Finesse, FocusTypes.Physical, EffectTypes.Pressure);
        AddChoice("Fi-E-M", "Skillful Navigation (+momentum, +precision)", ApproachTypes.Finesse, FocusTypes.Environment, EffectTypes.Momentum);
        AddChoice("Fi-E-P", "Find Optimal Path (+pressure, ++precision)", ApproachTypes.Finesse, FocusTypes.Environment, EffectTypes.Pressure);
        AddChoice("Fi-Re-M", "Resource Optimization (+momentum, +precision)", ApproachTypes.Finesse, FocusTypes.Resource, EffectTypes.Momentum);
        AddChoice("Fi-Re-P", "Efficient Utilization (+pressure, ++precision)", ApproachTypes.Finesse, FocusTypes.Resource, EffectTypes.Pressure);

        // Wit Approach Choices
        AddChoice("W-I-M", "Analytical Insight (+momentum, +analysis)", ApproachTypes.Wit, FocusTypes.Information, EffectTypes.Momentum);
        AddChoice("W-I-P", "Logical Deconstruction (+pressure, ++analysis)", ApproachTypes.Wit, FocusTypes.Information, EffectTypes.Pressure);
        AddChoice("W-R-M", "Strategic Interaction (+momentum, +analysis)", ApproachTypes.Wit, FocusTypes.Relationship, EffectTypes.Momentum);
        AddChoice("W-R-P", "Intellectual Challenge (+pressure, ++analysis)", ApproachTypes.Wit, FocusTypes.Relationship, EffectTypes.Pressure);
        AddChoice("W-P-M", "Calculated Movement (+momentum, +analysis)", ApproachTypes.Wit, FocusTypes.Physical, EffectTypes.Momentum);
        AddChoice("W-P-P", "Exploit Vulnerability (+pressure, ++analysis)", ApproachTypes.Wit, FocusTypes.Physical, EffectTypes.Pressure);
        AddChoice("W-E-M", "Environmental Analysis (+momentum, +analysis)", ApproachTypes.Wit, FocusTypes.Environment, EffectTypes.Momentum);
        AddChoice("W-E-P", "Risk Assessment (+pressure, ++analysis)", ApproachTypes.Wit, FocusTypes.Environment, EffectTypes.Pressure);
        AddChoice("W-Re-M", "Strategic Allocation (+momentum, +analysis)", ApproachTypes.Wit, FocusTypes.Resource, EffectTypes.Momentum);
        AddChoice("W-Re-P", "Resource Evaluation (+pressure, ++analysis)", ApproachTypes.Wit, FocusTypes.Resource, EffectTypes.Pressure);

        // Charm Approach Choices
        AddChoice("C-I-M", "Persuasive Exchange (+momentum, +rapport)", ApproachTypes.Charm, FocusTypes.Information, EffectTypes.Momentum);
        AddChoice("C-I-P", "Diplomatic Inquiry (+pressure, ++rapport)", ApproachTypes.Charm, FocusTypes.Information, EffectTypes.Pressure);
        AddChoice("C-R-M", "Social Connection (+momentum, +rapport)", ApproachTypes.Charm, FocusTypes.Relationship, EffectTypes.Momentum);
        AddChoice("C-R-P", "Emotional Appeal (+pressure, ++rapport)", ApproachTypes.Charm, FocusTypes.Relationship, EffectTypes.Pressure);
        AddChoice("C-P-M", "Captivating Display (+momentum, +rapport)", ApproachTypes.Charm, FocusTypes.Physical, EffectTypes.Momentum);
        AddChoice("C-P-P", "Charismatic Flourish (+pressure, ++rapport)", ApproachTypes.Charm, FocusTypes.Physical, EffectTypes.Pressure);
        AddChoice("C-E-M", "Social Positioning (+momentum, +rapport)", ApproachTypes.Charm, FocusTypes.Environment, EffectTypes.Momentum);
        AddChoice("C-E-P", "Environmental Charm (+pressure, ++rapport)", ApproachTypes.Charm, FocusTypes.Environment, EffectTypes.Pressure);
        AddChoice("C-Re-M", "Advantageous Negotiation (+momentum, +rapport)", ApproachTypes.Charm, FocusTypes.Resource, EffectTypes.Momentum);
        AddChoice("C-Re-P", "Resource Solicitation (+pressure, ++rapport)", ApproachTypes.Charm, FocusTypes.Resource, EffectTypes.Pressure);

        // Stealth Approach Choices
        AddChoice("S-I-M", "Covert Intelligence (+momentum, +concealment)", ApproachTypes.Stealth, FocusTypes.Information, EffectTypes.Momentum);
        AddChoice("S-I-P", "Undetected Observation (+pressure, ++concealment)", ApproachTypes.Stealth, FocusTypes.Information, EffectTypes.Pressure);
        AddChoice("S-R-M", "Hidden Influence (+momentum, +concealment)", ApproachTypes.Stealth, FocusTypes.Relationship, EffectTypes.Momentum);
        AddChoice("S-R-P", "Subtle Manipulation (+pressure, ++concealment)", ApproachTypes.Stealth, FocusTypes.Relationship, EffectTypes.Pressure);
        AddChoice("S-P-M", "Concealed Movement (+momentum, +concealment)", ApproachTypes.Stealth, FocusTypes.Physical, EffectTypes.Momentum);
        AddChoice("S-P-P", "Evasive Action (+pressure, ++concealment)", ApproachTypes.Stealth, FocusTypes.Physical, EffectTypes.Pressure);
        AddChoice("S-E-M", "Stealthy Positioning (+momentum, +concealment)", ApproachTypes.Stealth, FocusTypes.Environment, EffectTypes.Momentum);
        AddChoice("S-E-P", "Covert Distraction (+pressure, ++concealment)", ApproachTypes.Stealth, FocusTypes.Environment, EffectTypes.Pressure);
        AddChoice("S-Re-M", "Discreet Acquisition (+momentum, +concealment)", ApproachTypes.Stealth, FocusTypes.Resource, EffectTypes.Momentum);
        AddChoice("S-Re-P", "Covert Appropriation (+pressure, ++concealment)", ApproachTypes.Stealth, FocusTypes.Resource, EffectTypes.Pressure);
    }

    private void AddChoice(string id, string name, ApproachTypes approach, FocusTypes focus, EffectTypes effect)
    {
        _choices[id] = new Choice(id, name, approach, focus, effect);
    }
}
