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
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Display of Force",
            "You demonstrate your physical power and authority to intimidate others.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Command Attention",
            "You take control of the conversation through sheer force of personality.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Forceful Interrogation",
            "You demand answers, making it clear that evasion will not be tolerated.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Control Territory",
            "You secure key positions in the area, establishing dominance over the physical space.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Forceful Acquisition",
            "You seize resources through superior strength, making it clear they now belong to you.",
            FocusTags.Resource,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 2),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // DOMINANCE-BASED DEFENSIVE CHOICES (PRESSURE)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Stand Ground",
            "You refuse to be intimidated, standing tall and maintaining your position despite challenges.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Establish Boundaries",
            "You clearly establish what you will and won't tolerate, setting firm expectations.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Silence Doubts",
            "You shut down skepticism with authoritative statements, preventing uncertainty from spreading.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Secure Position",
            "You take control of defensible terrain, reducing vulnerability to attacks or surprises.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Control Resources",
            "You guard important supplies, ensuring they remain secure and available when needed.",
            FocusTags.Resource,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // RAPPORT-BASED OFFENSIVE CHOICES (MOMENTUM)
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Charming Words",
            "You engage with warmth and genuine interest, making others feel valued and understood.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Friendly Inquiry",
            "You ask questions with warmth and interest that encourages others to share information.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Coordinated Effort",
            "You foster teamwork that makes physical coordination feel natural and effortless.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Create Ambiance",
            "You transform the atmosphere through your social presence, making the environment work in your favor.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Negotiate Terms",
            "You propose a mutually beneficial arrangement regarding resources or payment.",
            FocusTags.Resource,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 2),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // RAPPORT-BASED DEFENSIVE CHOICES (PRESSURE)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Smooth Over",
            "You defuse tension with well-chosen words and genuine empathy, easing strained relationships.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Reassuring Words",
            "You share information in a calming manner that alleviates fears and reduces pressure.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Gentle Approach",
            "Your non-threatening body language helps others relax their guard around you.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Create Safe Space",
            "You cultivate an atmosphere where everyone feels secure enough to let their guard down.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Fair Distribution",
            "You ensure everyone feels fairly treated in how resources are shared or allocated.",
            FocusTags.Resource,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // ANALYSIS-BASED OFFENSIVE CHOICES (MOMENTUM)
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Analytical Insight",
            "You identify critical connections between seemingly unrelated pieces of information.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Systematic Approach",
            "You develop a step-by-step methodology to tackle the complex problem efficiently.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Assess Relationships",
            "You methodically map out the relationships and power dynamics at play in the social situation.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Environmental Analysis",
            "You critically examine how the environment contains clues relevant to your investigation.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Resource Evaluation",
            "You assess which resources and assets will be most valuable in the current context.",
            FocusTags.Resource,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 2),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // ANALYSIS-BASED DEFENSIVE CHOICES (PRESSURE)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Careful Consideration",
            "You consider all available information before acting, avoiding potential missteps.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Calculate Risks",
            "You assess potential risks and pitfalls before taking action, avoiding mistakes.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Understand Motivations",
            "You discern underlying motivations, helping you avoid unnecessary social conflicts.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Identify Safe Zones",
            "You locate areas of safety or advantage within the environment, reducing vulnerability.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Efficient Resource Use",
            "You conserve energy and supplies through careful planning, ensuring nothing is wasted.",
            FocusTags.Resource,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // PRECISION-BASED OFFENSIVE CHOICES (MOMENTUM)
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Precise Strike",
            "You execute a perfectly timed movement with flawless technique for maximum effect.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Targeted Question",
            "You ask the perfect question that gets directly to the heart of what you need to know.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Perfect Timing",
            "You choose the perfect moment to make your request or share your thoughts.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Pinpoint Weakness",
            "You locate the exact structural weakness or advantage in the environment to exploit.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Exact Measurement",
            "You use resources with perfect economy, achieving maximum effect with minimum waste.",
            FocusTags.Resource,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 2),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // PRECISION-BASED DEFENSIVE CHOICES (PRESSURE)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Measured Response",
            "You move with deliberate control, minimizing strain and risk of injury.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Clear Communication",
            "You express yourself with perfect clarity, preventing misunderstandings that could cause complications.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Targeted Reassurance",
            "You address the exact concerns that are causing tension in the relationship.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Minor Adjustment",
            "You make small but crucial adjustments to the environment that reduce hazards.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Careful Allocation",
            "You distribute resources with perfect efficiency, ensuring nothing is wasted or lacking.",
            FocusTags.Resource,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // CONCEALMENT-BASED OFFENSIVE CHOICES (MOMENTUM)
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Hidden Advantage",
            "You move stealthily, positioning yourself for an advantageous approach.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 2),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Gather Secrets",
            "You listen more than you speak, gathering valuable information others don't realize they're sharing.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 2),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Veiled Intentions",
            "You maintain a carefully crafted persona, revealing only what serves your purposes.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 2),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Blend With Surroundings",
            "You use the environment to mask your presence, becoming nearly invisible.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 2),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Secret Resources",
            "You have hidden tools or supplies that can be deployed at the perfect moment.",
            FocusTags.Resource,
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 2),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // CONCEALMENT-BASED DEFENSIVE CHOICES (PRESSURE)
        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Fade Away",
            "You slip into the shadows, removing yourself from immediate danger.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Conceal Weaknesses",
            "You carefully control what information about yourself is revealed, hiding vulnerabilities.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Maintain Privacy",
            "You establish comfortable boundaries, preventing others from prying into sensitive areas.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Find Cover",
            "You locate physical cover or shelter that protects you from immediate threats.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Hide Resources",
            "You conceal valuable resources, ensuring they remain available when truly needed.",
            FocusTags.Resource,
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 1),
            TagModification.ForFocus(FocusTags.Resource, 1)
        ));

        // ADVANCED HYBRID CHOICES
        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Tactical Force",
            "You apply force with calculated precision, maximizing impact while minimizing wasted effort.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 1),
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Physical, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Charming Insight",
            "You blend emotional intelligence with analytical thinking, gaining insights others miss.",
            FocusTags.Information,
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 1),
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 1),
            TagModification.ForFocus(FocusTags.Information, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreateMomentumChoice(
            "Precise Stealth",
            "You move with perfect control and silence, achieving the impossible without being detected.",
            FocusTags.Environment,
            TagModification.ForEncounterState(EncounterStateTags.Precision, 1),
            TagModification.ForEncounterState(EncounterStateTags.Concealment, 1),
            TagModification.ForFocus(FocusTags.Environment, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Calculated Charm",
            "You use a carefully calibrated approach to social interactions, defusing tensions through strategic empathy.",
            FocusTags.Relationship,
            TagModification.ForEncounterState(EncounterStateTags.Analysis, 1),
            TagModification.ForEncounterState(EncounterStateTags.Rapport, 1),
            TagModification.ForFocus(FocusTags.Relationship, 1)
        ));

        _standardChoices.Add(ChoiceFactory.CreatePressureChoice(
            "Forceful Precision",
            "You combine strength with perfect control, creating an imposing presence that deters threats.",
            FocusTags.Physical,
            TagModification.ForEncounterState(EncounterStateTags.Dominance, 1),
            TagModification.ForEncounterState(EncounterStateTags.Precision, 1),
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