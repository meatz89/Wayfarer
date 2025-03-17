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
        // Force + Relationship
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Command Respect",
            "Assert your authority to gain others' compliance",
            FocusTags.Information,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Escalate Demands",
            "Forcefully state your requirements with no room for negotiation",
            ApproachTags.Force,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1)
        ));

        // Force + Information
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Direct Questioning",
            "Interrogate firmly to extract crucial information",
            ApproachTags.Force,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Intimidating Interrogation",
            "Use implied threats to extract information",
            ApproachTags.Force,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1)
        ));

        // Force + Physical
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Display of Strength",
            "Demonstrate your physical prowess to impress or intimidate",
            ApproachTags.Force,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForEncounterState(ApproachTags.Concealment, -1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Aggressive Posturing",
            "Use your physical presence to threaten or intimidate",
            ApproachTags.Force,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1)
        ));

        // Force + Environment
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Control Territory",
            "Establish dominance over the physical space",
            ApproachTags.Force,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Force Through Obstacles",
            "Forcefully remove barriers in your way",
            ApproachTags.Force,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForEncounterState(ApproachTags.Precision, -1)
        ));

        // Force + Resource
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Seize Resources",
            "Take what you need through direct action",
            ApproachTags.Force,
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Dominance, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Forceful Requisition",
            "Demand resources with the implied threat of force",
            ApproachTags.Force,
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1)
        ));

        // Charm + Relationship
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Build Rapport",
            "Develop a positive connection with others",
            ApproachTags.Charm,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Express Vulnerability",
            "Share personal insights to build connection at the risk of exposure",
            ApproachTags.Charm,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1)
        ));

        // Charm + Information
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Friendly Inquiry",
            "Extract information through pleasant conversation",
            ApproachTags.Charm,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Deflecting Explanation",
            "Redirect a conversation using charm to avoid difficult topics",
            ApproachTags.Charm,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1)
        ));

        // Charm + Physical
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Graceful Display",
            "Impress others with physical charm or grace",
            ApproachTags.Charm,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForEncounterState(ApproachTags.Precision, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Hesitant Approach",
            "Use gentle physical presence to put others at ease",
            ApproachTags.Charm,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1)
        ));

        // Charm + Environment
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Create Ambiance",
            "Adjust the environment to create a favorable atmosphere",
            ApproachTags.Charm,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForEncounterState(ApproachTags.Precision, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Secluded Conversation",
            "Establish a private space for sensitive discussion",
            ApproachTags.Charm,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1)
        ));

        // Charm + Resource
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Negotiate Terms",
            "Charm others into favorable trade or agreements",
            ApproachTags.Charm,
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Overextend Resources",
            "Offer excessive resources to build goodwill at personal cost",
            ApproachTags.Charm,
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Rapport, 2)
        ));

        // Wit + Relationship
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Analyze Motives",
            "Understand others' true intentions and leverage them",
            ApproachTags.Wit,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Overthink Social Dynamics",
            "Spend time analyzing social situations at the risk of paralysis",
            ApproachTags.Wit,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1)
        ));

        // Wit + Information
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Process Evidence",
            "Expertly analyze available information",
            ApproachTags.Wit,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForEncounterState(ApproachTags.Precision, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Deep Investigation",
            "Delve deeply into analysis at the risk of losing focus",
            ApproachTags.Wit,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForEncounterState(ApproachTags.Precision, 1)
        ));

        // Wit + Physical
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Analyze Weaknesses",
            "Identify physical vulnerabilities to exploit",
            ApproachTags.Wit,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForEncounterState(ApproachTags.Dominance, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Complex Maneuvering",
            "Position yourself optimally through careful analysis",
            ApproachTags.Wit,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1)
        ));

        // Wit + Environment
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Environmental Assessment",
            "Thoroughly analyze surroundings for advantages",
            ApproachTags.Wit,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Complex Navigation",
            "Find a complex path through difficult terrain",
            ApproachTags.Wit,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1)
        ));

        // Wit + Resource
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Optimize Usage",
            "Find the most efficient use of available resources",
            ApproachTags.Wit,
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2),
            TagModification.ForEncounterState(ApproachTags.Precision, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Resource Analysis",
            "Spend time studying resources at the cost of immediate action",
            ApproachTags.Wit,
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Analysis, 2)
        ));

        // Finesse + Relationship
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Subtle Influence",
            "Subtly guide others' thinking without them noticing",
            ApproachTags.Finesse,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Calculated Social Risk",
            "Navigate delicate social situations with precision",
            ApproachTags.Finesse,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1)
        ));

        // Finesse + Information
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Precise Questioning",
            "Ask exactly the right questions to get needed information",
            ApproachTags.Finesse,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Read Between Lines",
            "Understand what's not being said through careful observation",
            ApproachTags.Finesse,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        // Finesse + Physical
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Precise Movement",
            "Execute actions with perfect accuracy",
            ApproachTags.Finesse,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Controlled Force",
            "Apply exactly the right amount of force needed",
            ApproachTags.Finesse,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Dominance, 1)
        ));

        // Finesse + Environment
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Environmental Manipulation",
            "Make subtle changes to the environment for advantage",
            ApproachTags.Finesse,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Circumvent Obstacles",
            "Find precise ways around barriers",
            ApproachTags.Finesse,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1)
        ));

        // Finesse + Resource
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Careful Allocation",
            "Apply resources with precision for maximum effect",
            ApproachTags.Finesse,
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Precision, 2),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Minimal Resource Use",
            "Accomplish goals using the least resources possible",
            ApproachTags.Finesse,
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        // Stealth + Relationship
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Observe Interactions",
            "Secretly watch how people relate to understand them",
            ApproachTags.Stealth,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Concealment, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Avoid Detection",
            "Remain unnoticed in social situations",
            ApproachTags.Stealth,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Concealment, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1)
        ));

        // Stealth + Information
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Eavesdrop",
            "Listen in on conversations without being noticed",
            ApproachTags.Stealth,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Concealment, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Hidden Research",
            "Investigate without leaving traces",
            ApproachTags.Stealth,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Concealment, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 2)
        ));

        // Stealth + Physical
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Silent Movement",
            "Move without making sound",
            ApproachTags.Stealth,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Concealment, 2),
            TagModification.ForEncounterState(ApproachTags.Precision, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Conceal Presence",
            "Hide your physical presence entirely",
            ApproachTags.Stealth,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Concealment, 2),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1)
        ));

        // Stealth + Environment
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Use Cover",
            "Utilize environmental features to hide",
            ApproachTags.Stealth,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Concealment, 2),
            TagModification.ForEncounterState(ApproachTags.Precision, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Create Diversion",
            "Set up a distraction in another location",
            ApproachTags.Stealth,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Concealment, 1),
            TagModification.ForEncounterState(ApproachTags.Dominance, -1)
        ));

        // Stealth + Resource
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Secret Acquisition",
            "Obtain resources without others noticing",
            ApproachTags.Stealth,
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Concealment, 1),
            TagModification.ForEncounterState(ApproachTags.Precision, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Hide Resources",
            "Conceal valuable items or information",
            ApproachTags.Stealth,
            FocusTags.Resource,
            TagModification.ForEncounterState(ApproachTags.Concealment, 2),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        // Transitional Choices
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Quiet Threat",
            "Intimidate someone without alerting others",
            ApproachTags.Force,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Tactical Force",
            "Apply force with calculated precision",
            ApproachTags.Force,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForEncounterState(ApproachTags.Precision, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Charming Diversion",
            "Use social skills to create a distraction",
            ApproachTags.Charm,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Analytical Conversation",
            "Guide a conversation to extract specific information",
            ApproachTags.Wit,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForEncounterState(ApproachTags.Analysis, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Precision Strike",
            "A carefully aimed action or pointed statement",
            ApproachTags.Finesse,
            FocusTags.Physical,
            TagModification.ForEncounterState(ApproachTags.Dominance, 1),
            TagModification.ForEncounterState(ApproachTags.Precision, 2)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Calculated Risk",
            "Use analytical insight to attempt a risky maneuver",
            ApproachTags.Wit,
            FocusTags.Environment,
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForEncounterState(ApproachTags.Concealment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "De-escalate Tension",
            "Calm a tense situation while maintaining authority",
            ApproachTags.Charm,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Rapport, 1),
            TagModification.ForEncounterState(ApproachTags.Dominance, -2)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Fade from Attention",
            "Socially extract yourself and become less noticeable",
            ApproachTags.Stealth,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Concealment, 2),
            TagModification.ForEncounterState(ApproachTags.Rapport, -1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Forceful Insight",
            "Aggressively point out a critical fact or realization",
            ApproachTags.Wit,
            FocusTags.Information,
            TagModification.ForEncounterState(ApproachTags.Analysis, 1),
            TagModification.ForEncounterState(ApproachTags.Dominance, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Precise Social Cues",
            "Carefully modulate your social behavior for maximum effect",
            ApproachTags.Finesse,
            FocusTags.Relationship,
            TagModification.ForEncounterState(ApproachTags.Precision, 1),
            TagModification.ForEncounterState(ApproachTags.Rapport, 1)
        ));
    }

    // Add a special choice for a specific location
    public void AddSpecialChoice(string locationName, SpecialChoice choice)
    {
        string key = $"{locationName}:{choice.Name}";
        _specialChoices[key] = choice;
    }

    // Get all standard choices
    public IReadOnlyList<IChoice> GetStandardChoices() => _standardChoices;

    // Get special choices for a specific location
    public IReadOnlyList<SpecialChoice> GetSpecialChoicesForLocation(string locationName, BaseTagSystem tagSystem)
    {
        return _specialChoices
            .Where(kv => kv.Key.StartsWith(locationName) && kv.Value.CanBeSelected(tagSystem))
            .Select(kv => kv.Value)
            .ToList();
    }

}
