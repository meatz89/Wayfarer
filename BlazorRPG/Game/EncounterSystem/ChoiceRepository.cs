namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Repository of all available choices in the game
    /// </summary>
    public class ChoiceRepository
    {
        private readonly List<Choice> _standardChoices = new();
        private readonly Dictionary<string, SpecialChoice> _specialChoices = new();
        private readonly Dictionary<ApproachTypes, EmergencyChoice> _emergencyChoices = new();

        public ChoiceRepository()
        {
            InitializeStandardChoices();
            InitializeEmergencyChoices();
        }

        private void InitializeStandardChoices()
        {
            // Force + Relationship
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Command Respect",
                "Assert your authority to gain others' compliance",
                ApproachTypes.Force,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Dominance, 2),
                TagModification.ForApproach(ApproachTags.Rapport, -1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Escalate Demands",
                "Forcefully state your requirements with no room for negotiation",
                ApproachTypes.Force,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Dominance, 2),
                TagModification.ForApproach(ApproachTags.Rapport, -1)
            ));

            // Force + Information
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Direct Questioning",
                "Interrogate firmly to extract crucial information",
                ApproachTypes.Force,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Dominance, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Intimidating Interrogation",
                "Use implied threats to extract information",
                ApproachTypes.Force,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Dominance, 2),
                TagModification.ForApproach(ApproachTags.Rapport, -1)
            ));

            // Force + Physical
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Display of Strength",
                "Demonstrate your physical prowess to impress or intimidate",
                ApproachTypes.Force,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Dominance, 2),
                TagModification.ForApproach(ApproachTags.Concealment, -1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Aggressive Posturing",
                "Use your physical presence to threaten or intimidate",
                ApproachTypes.Force,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Dominance, 2),
                TagModification.ForApproach(ApproachTags.Rapport, -1)
            ));

            // Force + Environment
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Control Territory",
                "Establish dominance over the physical space",
                ApproachTypes.Force,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Dominance, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Force Through Obstacles",
                "Forcefully remove barriers in your way",
                ApproachTypes.Force,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Dominance, 2),
                TagModification.ForApproach(ApproachTags.Precision, -1)
            ));

            // Force + Resource
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Seize Resources",
                "Take what you need through direct action",
                ApproachTypes.Force,
                FocusTags.Resource,
                TagModification.ForApproach(ApproachTags.Dominance, 2),
                TagModification.ForApproach(ApproachTags.Rapport, -1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Forceful Requisition",
                "Demand resources with the implied threat of force",
                ApproachTypes.Force,
                FocusTags.Resource,
                TagModification.ForApproach(ApproachTags.Dominance, 1),
                TagModification.ForApproach(ApproachTags.Rapport, 1)
            ));

            // Charm + Relationship
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Build Rapport",
                "Develop a positive connection with others",
                ApproachTypes.Charm,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Rapport, 2),
                TagModification.ForApproach(ApproachTags.Dominance, -1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Express Vulnerability",
                "Share personal insights to build connection at the risk of exposure",
                ApproachTypes.Charm,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Rapport, 2),
                TagModification.ForApproach(ApproachTags.Dominance, -1)
            ));

            // Charm + Information
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Friendly Inquiry",
                "Extract information through pleasant conversation",
                ApproachTypes.Charm,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Rapport, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Deflecting Explanation",
                "Redirect a conversation using charm to avoid difficult topics",
                ApproachTypes.Charm,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Rapport, 2),
                TagModification.ForApproach(ApproachTags.Dominance, -1)
            ));

            // Charm + Physical
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Graceful Display",
                "Impress others with physical charm or grace",
                ApproachTypes.Charm,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Rapport, 1),
                TagModification.ForApproach(ApproachTags.Precision, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Hesitant Approach",
                "Use gentle physical presence to put others at ease",
                ApproachTypes.Charm,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Rapport, 2),
                TagModification.ForApproach(ApproachTags.Dominance, -1)
            ));

            // Charm + Environment
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Create Ambiance",
                "Adjust the environment to create a favorable atmosphere",
                ApproachTypes.Charm,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Rapport, 1),
                TagModification.ForApproach(ApproachTags.Precision, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Secluded Conversation",
                "Establish a private space for sensitive discussion",
                ApproachTypes.Charm,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Rapport, 2),
                TagModification.ForApproach(ApproachTags.Concealment, 1)
            ));

            // Charm + Resource
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Negotiate Terms",
                "Charm others into favorable trade or agreements",
                ApproachTypes.Charm,
                FocusTags.Resource,
                TagModification.ForApproach(ApproachTags.Rapport, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Overextend Resources",
                "Offer excessive resources to build goodwill at personal cost",
                ApproachTypes.Charm,
                FocusTags.Resource,
                TagModification.ForApproach(ApproachTags.Rapport, 2)
            ));

            // Wit + Relationship
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Analyze Motives",
                "Understand others' true intentions and leverage them",
                ApproachTypes.Wit,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Analysis, 2),
                TagModification.ForApproach(ApproachTags.Rapport, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Overthink Social Dynamics",
                "Spend time analyzing social situations at the risk of paralysis",
                ApproachTypes.Wit,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Analysis, 2),
                TagModification.ForApproach(ApproachTags.Rapport, 1)
            ));

            // Wit + Information
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Process Evidence",
                "Expertly analyze available information",
                ApproachTypes.Wit,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Analysis, 2),
                TagModification.ForApproach(ApproachTags.Precision, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Deep Investigation",
                "Delve deeply into analysis at the risk of losing focus",
                ApproachTypes.Wit,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Analysis, 2),
                TagModification.ForApproach(ApproachTags.Precision, 1)
            ));

            // Wit + Physical
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Analyze Weaknesses",
                "Identify physical vulnerabilities to exploit",
                ApproachTypes.Wit,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Analysis, 2),
                TagModification.ForApproach(ApproachTags.Dominance, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Complex Maneuvering",
                "Position yourself optimally through careful analysis",
                ApproachTypes.Wit,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Analysis, 1),
                TagModification.ForApproach(ApproachTags.Concealment, 1)
            ));

            // Wit + Environment
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Environmental Assessment",
                "Thoroughly analyze surroundings for advantages",
                ApproachTypes.Wit,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Analysis, 2),
                TagModification.ForApproach(ApproachTags.Concealment, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Complex Navigation",
                "Find a complex path through difficult terrain",
                ApproachTypes.Wit,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Analysis, 2),
                TagModification.ForApproach(ApproachTags.Concealment, 1)
            ));

            // Wit + Resource
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Optimize Usage",
                "Find the most efficient use of available resources",
                ApproachTypes.Wit,
                FocusTags.Resource,
                TagModification.ForApproach(ApproachTags.Analysis, 2),
                TagModification.ForApproach(ApproachTags.Precision, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Resource Analysis",
                "Spend time studying resources at the cost of immediate action",
                ApproachTypes.Wit,
                FocusTags.Resource,
                TagModification.ForApproach(ApproachTags.Analysis, 2)
            ));

            // Finesse + Relationship
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Subtle Influence",
                "Subtly guide others' thinking without them noticing",
                ApproachTypes.Finesse,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Precision, 1),
                TagModification.ForApproach(ApproachTags.Rapport, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Calculated Social Risk",
                "Navigate delicate social situations with precision",
                ApproachTypes.Finesse,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Precision, 1),
                TagModification.ForApproach(ApproachTags.Rapport, 1)
            ));

            // Finesse + Information
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Precise Questioning",
                "Ask exactly the right questions to get needed information",
                ApproachTypes.Finesse,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Precision, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Read Between Lines",
                "Understand what's not being said through careful observation",
                ApproachTypes.Finesse,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Precision, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            // Finesse + Physical
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Precise Movement",
                "Execute actions with perfect accuracy",
                ApproachTypes.Finesse,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Precision, 2),
                TagModification.ForApproach(ApproachTags.Concealment, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Controlled Force",
                "Apply exactly the right amount of force needed",
                ApproachTypes.Finesse,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Precision, 1),
                TagModification.ForApproach(ApproachTags.Dominance, 1)
            ));

            // Finesse + Environment
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Environmental Manipulation",
                "Make subtle changes to the environment for advantage",
                ApproachTypes.Finesse,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Precision, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Circumvent Obstacles",
                "Find precise ways around barriers",
                ApproachTypes.Finesse,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Precision, 1),
                TagModification.ForApproach(ApproachTags.Concealment, 1)
            ));

            // Finesse + Resource
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Careful Allocation",
                "Apply resources with precision for maximum effect",
                ApproachTypes.Finesse,
                FocusTags.Resource,
                TagModification.ForApproach(ApproachTags.Precision, 2),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Minimal Resource Use",
                "Accomplish goals using the least resources possible",
                ApproachTypes.Finesse,
                FocusTags.Resource,
                TagModification.ForApproach(ApproachTags.Precision, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            // Stealth + Relationship
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Observe Interactions",
                "Secretly watch how people relate to understand them",
                ApproachTypes.Stealth,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Concealment, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Avoid Detection",
                "Remain unnoticed in social situations",
                ApproachTypes.Stealth,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Concealment, 2),
                TagModification.ForApproach(ApproachTags.Rapport, -1)
            ));

            // Stealth + Information
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Eavesdrop",
                "Listen in on conversations without being noticed",
                ApproachTypes.Stealth,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Concealment, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Hidden Research",
                "Investigate without leaving traces",
                ApproachTypes.Stealth,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Concealment, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 2)
            ));

            // Stealth + Physical
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Silent Movement",
                "Move without making sound",
                ApproachTypes.Stealth,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Concealment, 2),
                TagModification.ForApproach(ApproachTags.Precision, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Conceal Presence",
                "Hide your physical presence entirely",
                ApproachTypes.Stealth,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Concealment, 2),
                TagModification.ForApproach(ApproachTags.Dominance, -1)
            ));

            // Stealth + Environment
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Use Cover",
                "Utilize environmental features to hide",
                ApproachTypes.Stealth,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Concealment, 2),
                TagModification.ForApproach(ApproachTags.Precision, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Create Diversion",
                "Set up a distraction in another location",
                ApproachTypes.Stealth,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Concealment, 1),
                TagModification.ForApproach(ApproachTags.Dominance, -1)
            ));

            // Stealth + Resource
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Secret Acquisition",
                "Obtain resources without others noticing",
                ApproachTypes.Stealth,
                FocusTags.Resource,
                TagModification.ForApproach(ApproachTags.Concealment, 1),
                TagModification.ForApproach(ApproachTags.Precision, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Hide Resources",
                "Conceal valuable items or information",
                ApproachTypes.Stealth,
                FocusTags.Resource,
                TagModification.ForApproach(ApproachTags.Concealment, 2),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            // Transitional Choices
            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Quiet Threat",
                "Intimidate someone without alerting others",
                ApproachTypes.Force,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Dominance, 1),
                TagModification.ForApproach(ApproachTags.Concealment, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Tactical Force",
                "Apply force with calculated precision",
                ApproachTypes.Force,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Dominance, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1),
                TagModification.ForApproach(ApproachTags.Precision, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Charming Diversion",
                "Use social skills to create a distraction",
                ApproachTypes.Charm,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Rapport, 1),
                TagModification.ForApproach(ApproachTags.Concealment, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Analytical Conversation",
                "Guide a conversation to extract specific information",
                ApproachTypes.Wit,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Rapport, 1),
                TagModification.ForApproach(ApproachTags.Analysis, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Precision Strike",
                "A carefully aimed action or pointed statement",
                ApproachTypes.Finesse,
                FocusTags.Physical,
                TagModification.ForApproach(ApproachTags.Dominance, 1),
                TagModification.ForApproach(ApproachTags.Precision, 2)
            ));

            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Calculated Risk",
                "Use analytical insight to attempt a risky maneuver",
                ApproachTypes.Wit,
                FocusTags.Environment,
                TagModification.ForApproach(ApproachTags.Analysis, 1),
                TagModification.ForApproach(ApproachTags.Concealment, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "De-escalate Tension",
                "Calm a tense situation while maintaining authority",
                ApproachTypes.Charm,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Rapport, 1),
                TagModification.ForApproach(ApproachTags.Dominance, -2)
            ));

            _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
                "Fade from Attention",
                "Socially extract yourself and become less noticeable",
                ApproachTypes.Stealth,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Concealment, 2),
                TagModification.ForApproach(ApproachTags.Rapport, -1)
            ));

            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Forceful Insight",
                "Aggressively point out a critical fact or realization",
                ApproachTypes.Wit,
                FocusTags.Information,
                TagModification.ForApproach(ApproachTags.Analysis, 1),
                TagModification.ForApproach(ApproachTags.Dominance, 1)
            ));

            _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
                "Precise Social Cues",
                "Carefully modulate your social behavior for maximum effect",
                ApproachTypes.Finesse,
                FocusTags.Relationship,
                TagModification.ForApproach(ApproachTags.Precision, 1),
                TagModification.ForApproach(ApproachTags.Rapport, 1)
            ));
        }

        private void InitializeEmergencyChoices()
        {
            _emergencyChoices[ApproachTypes.Charm] = new EmergencyChoice(
                "Desperate Appeal",
                "A last-ditch attempt to charm your way through",
                ApproachTypes.Charm,
                new[] { TagModification.ForApproach(ApproachTags.Rapport, 1) },
                ApproachTypes.Charm
            );

            _emergencyChoices[ApproachTypes.Force] = new EmergencyChoice(
                "Firm Stance",
                "Stand your ground when outmatched",
                ApproachTypes.Force,
                new[] { TagModification.ForApproach(ApproachTags.Dominance, 2) },
                ApproachTypes.Force
            );

            _emergencyChoices[ApproachTypes.Stealth] = new EmergencyChoice(
                "Quick Withdrawal",
                "Rapidly retreat from a difficult situation",
                ApproachTypes.Stealth,
                new[] { TagModification.ForApproach(ApproachTags.Concealment, 2) },
                ApproachTypes.Stealth
            );

            _emergencyChoices[ApproachTypes.Wit] = new EmergencyChoice(
                "Careful Analysis",
                "Quickly assess the situation when caught off guard",
                ApproachTypes.Wit,
                new[] { TagModification.ForApproach(ApproachTags.Analysis, 2) },
                ApproachTypes.Wit
            );

            _emergencyChoices[ApproachTypes.Finesse] = new EmergencyChoice(
                "Precise Adjustment",
                "Make a small but crucial adjustment to recover",
                ApproachTypes.Finesse,
                new[] { TagModification.ForApproach(ApproachTags.Precision, 2) },
                ApproachTypes.Finesse
            );
        }

        // Add a special choice for a specific location
        public void AddSpecialChoice(string locationName, SpecialChoice choice)
        {
            string key = $"{locationName}:{choice.Name}";
            _specialChoices[key] = choice;
        }

        // Get all standard choices
        public IReadOnlyList<Choice> GetStandardChoices() => _standardChoices;

        // Get special choices for a specific location
        public IReadOnlyList<SpecialChoice> GetSpecialChoicesForLocation(string locationName, BaseTagSystem tagSystem)
        {
            return _specialChoices
                .Where(kv => kv.Key.StartsWith(locationName) && kv.Value.CanBeSelected(tagSystem))
                .Select(kv => kv.Value)
                .ToList();
        }

        // Get emergency choice for a blocked approach
        public EmergencyChoice GetEmergencyChoice(ApproachTypes blockedApproach)
        {
            return _emergencyChoices.TryGetValue(blockedApproach, out EmergencyChoice? choice) ? choice : null;
        }
    }
}
