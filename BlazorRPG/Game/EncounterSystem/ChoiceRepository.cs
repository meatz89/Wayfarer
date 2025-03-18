/// <summary>
/// Repository of all available choices in the game
/// </summary>
public class ChoiceRepository
{
    private readonly List<Choice> _standardChoices = new();
    private readonly Dictionary<string, SpecialChoice> _specialChoices = new();

    public ChoiceRepository()
    {
        InitializeStandardChoices();
    }

    private void InitializeStandardChoices()
    {
        // DOMINANCE-BASED OFFENSIVE CHOICES (MOMENTUM)
        // HIGH-COMMITMENT CHOICE: Enhanced benefit (+3) with greater cost (-2, -1)
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Overwhelming Force",
            "You unleash your full physical and authoritative might, completely dominating the situation at the cost of any subtlety or social nuance.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Dominance, 3),
            TagModification.ForEncounterState(ApproachTags.Evasion, -2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // MODIFIED WITH TRADEOFF: Sacrificing subtlety for dominance
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Command Presence",
            "You assert dominance in the interaction, your commanding tone making clear you expect obedience at the cost of building rapport.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Forceful Interrogation",
            "You press aggressively for answers, making evasion seem a worse option than compliance.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Control Territory",
            "You claim and secure key positions in the area, establishing clear dominance over the space.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Forceful Acquisition",
            "You assert rightful ownership through superior strength, making it clear these resources now serve your purpose.",
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // DOMINANCE-BASED DEFENSIVE CHOICES (PRESSURE)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Stand Ground",
            "You plant yourself firmly, refusing to yield an inch despite mounting pressure or challenges.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Establish Boundaries",
            "You decisively declare your limits and expectations, leaving no room for negotiation or misunderstanding.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Silence Doubts",
            "You decisively quash uncertainty with authoritative statements, preventing any spread of hesitation.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Secure Position",
            "You occupy and fortify defensible terrain, minimizing vulnerability to external threats.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        // MODIFIED WITH TRADEOFF: Becoming too controlling
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Absolute Control",
            "You take strict charge of critical supplies, ensuring they're distributed according to your judgment, reducing detailed analysis in favor of authority.",
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, -1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // RAPPORT-BASED OFFENSIVE CHOICES (MOMENTUM)
        // HIGH-COMMITMENT CHOICE: Enhanced benefit (+3) with greater cost (-2)
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Heart-to-Heart",
            "You create a moment of profound connection and authentic vulnerability that deeply resonates, but leaves you exposed and unable to maintain distance.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Rapport, 3),
            TagModification.ForEncounterState(ApproachTags.Evasion, -2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Friendly Inquiry",
            "You pose questions with such warmth that sharing information feels like a natural part of pleasant conversation.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // MODIFIED WITH TRADEOFF: Being too friendly makes precise execution difficult
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Team Spirit",
            "You inspire natural teamwork through positive connection, trading technical precision for enthusiastic cooperation.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForEncounterState(ApproachTags.Precision, -1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Create Ambiance",
            "You transform the atmosphere through your social presence, making everyone feel the environment is working in their favor.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Negotiate Terms",
            "You propose arrangements that genuinely benefit all parties, making resource allocation feel like a win-win scenario.",
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // RAPPORT-BASED DEFENSIVE CHOICES (PRESSURE)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Smooth Over",
            "You ease tensions with well-timed empathy and honest recognition of feelings, dissolving potential conflicts.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Reassuring Words",
            "You communicate critical information with a calming tone that alleviates fear while maintaining honesty.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // HIGH-COMMITMENT CHOICE: Enhanced benefit (+2) with cost (-1)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Disarming Vulnerability",
            "You lower all social defenses to create such genuine connection that even hostility dissolves, though it leaves you unable to analyze situations critically.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForEncounterState(ApproachTags.Analysis, -1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Create Safe Space",
            "You cultivate an atmosphere of mutual trust where everyone feels secure enough to lower their guards.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Fair Distribution",
            "You allocate resources with transparent fairness that acknowledges everyone's needs and contributions.",
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // ANALYSIS-BASED OFFENSIVE CHOICES (MOMENTUM)
        // HIGH-COMMITMENT CHOICE: Enhanced benefit (+3) with greater cost (-1, -1)
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Perfect Deduction",
            "You enter a state of complete analytical focus, making brilliant connections while becoming oblivious to social cues and physical surroundings.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Analysis, 3),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Systematic Approach",
            "You methodically break down a complex physical challenge into a series of manageable, efficient steps.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Assess Relationships",
            "You mentally map the social dynamics at play, identifying leverage points and influence patterns.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Environmental Analysis",
            "You carefully examine how your surroundings function, discovering hidden aspects that can be leveraged.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        // MODIFIED WITH TRADEOFF: Analysis over evasion
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Resource Optimization",
            "You quickly assess resource value and utility with such thoroughness that your intense scrutiny becomes noticeable to others.",
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForEncounterState(ApproachTags.Evasion, -1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // ANALYSIS-BASED DEFENSIVE CHOICES (PRESSURE)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Careful Consideration",
            "You pause to process all available information, avoiding hasty conclusions that could lead to error.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Calculate Risks",
            "You assess potential hazards and their probabilities, selecting the physical approach with optimal risk-benefit ratio.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // MODIFIED WITH TRADEOFF: Analysis over stealth
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Behavioral Analysis",
            "You deduce others' underlying motivations so thoroughly that your focused attention becomes noticeable, sacrificing some stealth.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForEncounterState(ApproachTags.Evasion, -1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Identify Safe Zones",
            "You locate areas within the environment that provide maximum protection or advantage with minimal exposure.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Efficient Resource Use",
            "You optimize resource allocation with careful planning, ensuring nothing is wasted and critical needs are met.",
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // PRECISION-BASED OFFENSIVE CHOICES (MOMENTUM)
        // HIGH-COMMITMENT CHOICE: Enhanced benefit (+3) with greater cost (-2)
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Surgical Precision",
            "You execute with such perfect technique and timing that it seems almost supernatural, but this level of focus blinds you to broader considerations.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Precision, 3),
            TagModification.ForEncounterState(ApproachTags.Analysis, -2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Targeted Question",
            "You formulate an incisive query that cuts directly to the heart of what you need to know, wasting no words.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Perfect Timing",
            "You intuitively identify the exact moment when your words or actions will have maximum impact on others.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Pinpoint Weakness",
            "Your keen eye instantly identifies the exact structural vulnerability that can be leveraged for advantage.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        // MODIFIED WITH TRADEOFF: Too precise to see alternatives
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Perfect Allocation",
            "You utilize resources with surgical precision, achieving exactly the intended outcome but becoming less adaptable to alternative approaches.",
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForEncounterState(ApproachTags.Analysis, -1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // PRECISION-BASED DEFENSIVE CHOICES (PRESSURE)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Measured Response",
            "You move with deliberate control and economy, each action calibrated to provide maximum protection with minimal exposure.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // HIGH-COMMITMENT CHOICE: Enhanced benefit (+2) with cost (-1)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Flawless Articulation",
            "You communicate with such perfect clarity and exactitude that misunderstanding becomes impossible, though social warmth suffers.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Targeted Reassurance",
            "You identify and address the exact concerns causing relational tension, without unnecessary emotional expenditure.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Minor Adjustment",
            "You make small but crucial modifications to your surroundings that significantly reduce environmental threats.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Careful Allocation",
            "You distribute limited resources with perfect efficiency, ensuring each allocation serves its exact intended purpose.",
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // EVASION-BASED OFFENSIVE CHOICES (MOMENTUM)
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Hidden Advantage",
            "You move with invisible purpose, positioning yourself to capitalize on opportunities others don't even perceive.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Evasion, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Gather Secrets",
            "You observe and absorb vital information others reveal unknowingly, collecting intelligence while appearing disinterested.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Evasion, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        // MODIFIED WITH TRADEOFF: Deception reduces authentic connection
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "False Persona",
            "You present a carefully crafted persona that conceals your true purpose while building advantageous but ultimately deceptive relationships.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Evasion, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // HIGH-COMMITMENT CHOICE: Enhanced benefit (+3) with greater cost (-2, -1)
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Become Shadow",
            "You vanish so completely into your surroundings that you practically cease to exist, sacrificing any ability to assert presence or maintain connections.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Evasion, 3),
            TagModification.ForEncounterState(ApproachTags.Dominance, -2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        // MODIFIED WITH TRADEOFF: Hiding vs forcing
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Hidden Resources",
            "You reveal concealed tools or supplies at the perfect moment, prioritizing stealth over a more dominant approach.",
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Evasion, 2),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // EVASION-BASED DEFENSIVE CHOICES (PRESSURE)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Fade Away",
            "You make yourself scarce at the critical moment, removing yourself from danger by seemingly vanishing.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        // HIGH-COMMITMENT CHOICE: Enhanced benefit (+2) with cost (-1)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Information Blackout",
            "You masterfully control all information about yourself, revealing nothing while learning everything, at the cost of forming genuine connections.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Evasion, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Maintain Privacy",
            "You establish subtle but effective boundaries that keep others from probing sensitive areas without seeming distant.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        // MODIFIED WITH TRADEOFF: Hiding vs analyzing
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Disappear",
            "You identify and utilize overlooked environmental features to vanish completely, relying on instinct rather than thorough analysis.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, -1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Hide Resources",
            "You secretively stash valuable resources where only you can access them, ensuring they remain available when needed.",
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // ADVANCED HYBRID CHOICES
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Tactical Force",
            "You apply force with calculated precision, sacrificing social graces for effective action.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Charming Insight",
            "You blend emotional intelligence with analytical observation, revealing insights that connect with others deeply.",
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForEncounterState(ApproachTags.Precision, -1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Precise Stealth",
            "You move with perfect control and silence, achieving the impossible without being detected.",
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Evasion, 1),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        // HIGH-COMMITMENT HYBRID: Enhanced benefit (+2/+2) with greater cost (-2, -1) 
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Calculated Vulnerability",
            "You create a perfectly calibrated emotional connection through analytical understanding of human psychology, sacrificing any pretense or self-protection.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForEncounterState(ApproachTags.Evasion, -2),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Forceful Precision",
            "You combine strength with perfect control, sacrificing stealth for effective controlled power.",
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Evasion, -1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));
    }

    public IReadOnlyList<IChoice> GetAllStandardChoices()
    {
        return _standardChoices.AsReadOnly();
    }

    public Choice GetChoiceByName(string name)
    {
        return _standardChoices.FirstOrDefault(c => c.Name == name);
    }

    public SpecialChoice GetSpecialChoiceByName(string name)
    {
        return _specialChoices.ContainsKey(name) ? _specialChoices[name] : null;
    }
}