/// <summary>
/// PARSE-TIME ONLY CATALOGUE
///
/// Defines 21 situation archetypes for procedural choice generation.
/// Creates learnable mechanical patterns players recognize and prepare for.
/// Uses strongly-typed enums for compile-time validation.
///
/// HIGHLANDER: ALL archetypes use ONE generation path.
/// RhythmPattern (Building/Crisis/Mixed) determines choice structure.
/// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
///
/// CATALOGUE PATTERN COMPLIANCE:
/// - Called ONLY from SceneTemplateParser at PARSE TIME
/// - NEVER called from facades, managers, or runtime code
/// - NEVER imported except in parser classes
/// - Generates archetype definitions that parser uses to create ChoiceTemplates
/// - Translation happens ONCE at game initialization
///
/// ARCHITECTURE:
/// Parser reads SituationArchetypeType enum from DTO → Calls GetArchetype() → Receives archetype structure
/// → Parser generates 4 ChoiceTemplates from archetype → Stores in SituationTemplate
/// → Runtime queries Scene.Situations (situations embedded in scenes), NO catalogue calls
///
/// Each archetype defines:
/// - Which stats are tested (learnable patterns)
/// - Cost structure (0/low/high)
/// - Challenge types
/// - Fallback penalties
/// </summary>
public static class SituationArchetypeCatalog
{
    /// <summary>
    /// Get archetype definition from strongly-typed enum.
    /// Called at parse time to generate choice structures.
    /// Compiler ensures exhaustiveness - no runtime unknown archetype errors.
    /// </summary>
    public static SituationArchetype GetArchetype(SituationArchetypeType archetypeType)
    {
        // COMPOSITION: Delegates to SituationArchetypeDefinitions for archetype creation
        return archetypeType switch
        {
            SituationArchetypeType.Confrontation => SituationArchetypeDefinitions.CreateConfrontation(),
            SituationArchetypeType.Negotiation => SituationArchetypeDefinitions.CreateNegotiation(),
            SituationArchetypeType.Investigation => SituationArchetypeDefinitions.CreateInvestigation(),
            SituationArchetypeType.SocialManeuvering => SituationArchetypeDefinitions.CreateSocialManeuvering(),
            SituationArchetypeType.Crisis => SituationArchetypeDefinitions.CreateCrisis(),
            SituationArchetypeType.ServiceTransaction => SituationArchetypeDefinitions.CreateServiceTransaction(),
            SituationArchetypeType.AccessControl => SituationArchetypeDefinitions.CreateAccessControl(),
            SituationArchetypeType.InformationGathering => SituationArchetypeDefinitions.CreateInformationGathering(),
            SituationArchetypeType.SkillDemonstration => SituationArchetypeDefinitions.CreateSkillDemonstration(),
            SituationArchetypeType.ReputationChallenge => SituationArchetypeDefinitions.CreateReputationChallenge(),
            SituationArchetypeType.EmergencyAid => SituationArchetypeDefinitions.CreateEmergencyAid(),
            SituationArchetypeType.AdministrativeProcedure => SituationArchetypeDefinitions.CreateAdministrativeProcedure(),
            SituationArchetypeType.TradeDispute => SituationArchetypeDefinitions.CreateTradeDispute(),
            SituationArchetypeType.CulturalFauxPas => SituationArchetypeDefinitions.CreateCulturalFauxPas(),
            SituationArchetypeType.Recruitment => SituationArchetypeDefinitions.CreateRecruitment(),
            SituationArchetypeType.RestPreparation => SituationArchetypeDefinitions.CreateRestPreparation(),
            SituationArchetypeType.EnteringPrivateSpace => SituationArchetypeDefinitions.CreateEnteringPrivateSpace(),
            SituationArchetypeType.DepartingPrivateSpace => SituationArchetypeDefinitions.CreateDepartingPrivateSpace(),
            SituationArchetypeType.ServiceNegotiation => SituationArchetypeDefinitions.CreateServiceNegotiation(),
            SituationArchetypeType.ContractNegotiation => SituationArchetypeDefinitions.CreateContractNegotiation(),
            SituationArchetypeType.ServiceExecutionRest => SituationArchetypeDefinitions.CreateServiceExecutionRest(),
            SituationArchetypeType.ServiceDeparture => SituationArchetypeDefinitions.CreateServiceDeparture(),
            SituationArchetypeType.MeditationAndReflection => SituationArchetypeDefinitions.CreateMeditationAndReflection(),
            SituationArchetypeType.LocalConversation => SituationArchetypeDefinitions.CreateLocalConversation(),
            SituationArchetypeType.StudyInLibrary => SituationArchetypeDefinitions.CreateStudyInLibrary(),
            _ => throw new InvalidOperationException($"Unhandled situation archetype type: {archetypeType}")
        };
    }

    /// <summary>
    /// CONTEXT-AWARE CHOICE GENERATION - HIGHLANDER COMPLIANT
    ///
    /// ALL archetypes use the SAME generation path. No routing, no special cases.
    /// RhythmPattern determines choice structure for ANY archetype.
    /// See arc42/08_crosscutting_concepts.md §8.26 (Sir Brante Rhythm Pattern)
    /// </summary>
    public static List<ChoiceTemplate> GenerateChoiceTemplatesWithContext(
        SituationArchetype archetype,
        string situationTemplateId,
        GenerationContext context)
    {
        // HIGHLANDER: Context is REQUIRED. Fail-fast if not provided.
        // Scale stat threshold by PowerDynamic (easier if dominant, harder if submissive)
        int scaledStatThreshold = context.Power switch
        {
            PowerDynamic.Dominant => archetype.StatThreshold - 2,
            PowerDynamic.Equal => archetype.StatThreshold,
            PowerDynamic.Submissive => archetype.StatThreshold + 2,
            _ => archetype.StatThreshold
        };

        // Scale coin cost by Quality (cheaper if basic, expensive if luxury)
        int scaledCoinCost = context.Quality switch
        {
            Quality.Basic => archetype.CoinCost - 3,
            Quality.Standard => archetype.CoinCost,
            Quality.Premium => archetype.CoinCost + 5,
            Quality.Luxury => archetype.CoinCost + 10,
            _ => archetype.CoinCost
        };

        // Scale coin reward by Quality (INCOME archetypes - better deals at higher quality venues)
        int scaledCoinReward = context.Quality switch
        {
            Quality.Basic => archetype.CoinReward - 2,
            Quality.Standard => archetype.CoinReward,
            Quality.Premium => archetype.CoinReward + 3,
            Quality.Luxury => archetype.CoinReward + 5,
            _ => archetype.CoinReward
        };

        // Adjust by NpcDemeanor for additional nuance
        if (context.NpcDemeanor == NPCDemeanor.Hostile)
        {
            scaledStatThreshold = scaledStatThreshold + 2;
        }
        else if (context.NpcDemeanor == NPCDemeanor.Friendly)
        {
            scaledStatThreshold = scaledStatThreshold - 2;
        }

        // DIFFICULTY SCALING: Further locations have harder requirements
        int difficultyAdjustment = context.LocationDifficulty switch
        {
            0 => 0,
            1 => 1,
            2 => 2,
            >= 3 => 3,
            _ => 0
        };
        scaledStatThreshold = scaledStatThreshold + difficultyAdjustment;

        // Also scale coin costs by difficulty (further locations = more expensive economy)
        scaledCoinCost = scaledCoinCost + (context.LocationDifficulty * 2);

        // Also scale coin rewards by difficulty (further locations = better paying contracts)
        scaledCoinReward = scaledCoinReward + (context.LocationDifficulty * 2);

        // Ensure minimum threshold of 1 (never negative)
        scaledStatThreshold = Math.Max(1, scaledStatThreshold);

        // Ensure minimum cost of 1 (never free or negative)
        scaledCoinCost = Math.Max(1, scaledCoinCost);

        // Ensure minimum reward of 1 if archetype has CoinReward (never 0 or negative for income archetypes)
        if (archetype.CoinReward > 0)
        {
            scaledCoinReward = Math.Max(1, scaledCoinReward);
        }

        // SIR BRANTE RHYTHM PATTERN: Same archetype produces different choices based on rhythm
        // Building = All positive (stat grants, no requirements)
        // Crisis = Damage mitigation (requirements gate avoiding penalty, fallback takes penalty)
        // Mixed = Standard trade-offs (current behavior)
        // See arc42/08_crosscutting_concepts.md §8.26
        RhythmPattern rhythm = context.Rhythm;

        return rhythm switch
        {
            RhythmPattern.Building => GenerateBuildingChoices(archetype, situationTemplateId),
            RhythmPattern.Crisis => GenerateCrisisChoices(archetype, situationTemplateId, scaledStatThreshold, scaledCoinCost, scaledCoinReward),
            RhythmPattern.Mixed => GenerateMixedChoices(archetype, situationTemplateId, scaledStatThreshold, scaledCoinCost, scaledCoinReward),
            _ => GenerateMixedChoices(archetype, situationTemplateId, scaledStatThreshold, scaledCoinCost, scaledCoinReward)
        };
    }

    /// <summary>
    /// Generate BUILDING rhythm choices - all positive outcomes, character formation.
    /// No requirements, choices GRANT stats instead of costing them.
    /// Used for: Tutorial A1, recovery periods, positive momentum scenes.
    ///
    /// RECOVERY ARCHETYPE HANDLING: Archetypes with Intensity=Recovery (ServiceExecutionRest, etc.)
    /// have None stats because they're about RESOURCE restoration, not stat development.
    /// Building rhythm generates resource recovery choices for these archetypes.
    /// </summary>
    private static List<ChoiceTemplate> GenerateBuildingChoices(
        SituationArchetype archetype,
        string situationTemplateId)
    {
        // Recovery archetypes get resource restoration instead of stat grants
        if (archetype.Intensity == ArchetypeIntensity.Recovery &&
            archetype.PrimaryStat == PlayerStatType.None)
        {
            return GenerateRecoveryChoices(archetype, situationTemplateId);
        }

        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        // Choice 1: Primary stat path - GRANTS primary stat (no requirement)
        // Uses "friendly" in ID for test compatibility with RapportBuildPlaythroughTest
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_friendly",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = GenerateBuildingPrimaryText(archetype),
            RequirementFormula = new CompoundRequirement(),
            Consequence = CreateStatGrantConsequence(archetype.PrimaryStat, 1),
            ActionType = ChoiceActionType.Instant
        });

        // Choice 2: Secondary stat path - GRANTS secondary stat (no requirement)
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_secondary",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = GenerateBuildingSecondaryText(archetype),
            RequirementFormula = new CompoundRequirement(),
            Consequence = CreateStatGrantConsequence(archetype.SecondaryStat, 1),
            ActionType = ChoiceActionType.Instant
        });

        // Choice 3: Exploration - GRANTS insight through curiosity (no requirement)
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_explore",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = "Explore and observe carefully",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { Insight = 1 },
            ActionType = ChoiceActionType.Instant
        });

        // Choice 4: Different flavor - GRANTS cunning (no requirement, always available)
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_cunning",
            PathType = ChoicePathType.Fallback,
            ActionTextTemplate = "Take time to think strategically",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { Cunning = 1 },
            ActionType = ChoiceActionType.Instant
        });

        return choices;
    }

    /// <summary>
    /// Generate choices for Recovery archetypes (ServiceExecutionRest, etc.)
    /// Resource restoration instead of stat grants - Health, Stamina, Focus recovery
    /// </summary>
    private static List<ChoiceTemplate> GenerateRecoveryChoices(
        SituationArchetype archetype,
        string situationTemplateId)
    {
        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        // Choice 1: Deep rest - full health restoration
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_rest_full",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = "Rest deeply and recover fully",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { Health = 10, Stamina = 5 },
            ActionType = ChoiceActionType.Instant
        });

        // Choice 2: Light rest with reflection - balanced recovery
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_rest_reflect",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = "Rest lightly and reflect on the day",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { Health = 5, Focus = 5 },
            ActionType = ChoiceActionType.Instant
        });

        // Choice 3: Active recovery - stamina focus
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_rest_active",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = "Stretch and prepare for tomorrow",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { Stamina = 10 },
            ActionType = ChoiceActionType.Instant
        });

        // Choice 4: Simple rest - basic recovery (fallback)
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_rest_simple",
            PathType = ChoicePathType.Fallback,
            ActionTextTemplate = "Simply rest",
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { Health = 3, Stamina = 3 },
            ActionType = ChoiceActionType.Instant
        });

        return choices;
    }

    /// <summary>
    /// Generate CRISIS rhythm choices - damage mitigation, stat gates avoid penalty.
    /// Requirements prevent penalty, fallback TAKES penalty.
    /// Used for: A3 crisis, high stakes moments, dramatic tension.
    /// INCOME ARCHETYPES: scaledCoinReward passed for consistency but not used in Crisis (no earning during crisis).
    /// </summary>
    private static List<ChoiceTemplate> GenerateCrisisChoices(
        SituationArchetype archetype,
        string situationTemplateId,
        int scaledStatThreshold,
        int scaledCoinCost,
        int scaledCoinReward)
    {
        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        // Crisis penalty based on archetype domain
        Consequence crisisPenalty = GetCrisisPenalty(archetype);

        // Choice 1: Stat-gated AVOIDANCE - meet threshold to AVOID penalty (no consequence)
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_stat",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = GenerateCrisisStatText(archetype),
            RequirementFormula = CreateStatRequirement(archetype, scaledStatThreshold),
            Consequence = new Consequence(), // Success = avoid penalty
            ActionType = ChoiceActionType.Instant
        });

        // Choice 2: Pay to AVOID penalty
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_money",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = GenerateCrisisMoneyText(archetype),
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { Coins = -scaledCoinCost }, // Pay to avoid
            ActionType = ChoiceActionType.Instant
        });

        // Choice 3: Challenge to MAYBE avoid penalty
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_challenge",
            PathType = ChoicePathType.Challenge,
            ActionTextTemplate = GenerateCrisisChallengeText(archetype),
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { Resolve = -archetype.ResolveCost },
            OnSuccessConsequence = new Consequence(), // Success = avoid penalty
            OnFailureConsequence = crisisPenalty, // Failure = take penalty
            ActionType = ChoiceActionType.StartChallenge,
            ChallengeId = null,
            ChallengeType = archetype.ChallengeType,
            ChallengeDeckName = archetype.ChallengeDeckName
        });

        // Choice 4: Fallback TAKES the penalty
        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_fallback",
            PathType = ChoicePathType.Fallback,
            ActionTextTemplate = GenerateCrisisFallbackText(archetype),
            RequirementFormula = new CompoundRequirement(),
            Consequence = crisisPenalty, // Fallback = take the penalty
            ActionType = ChoiceActionType.Instant
        });

        return choices;
    }

    /// <summary>
    /// Generate MIXED rhythm choices - standard trade-off gameplay.
    /// Requirements gate best outcome, fallback is poor but available.
    /// Used for: Normal gameplay, most procedural content.
    /// INCOME ARCHETYPES: When CoinReward > 0, Choice 2 generates income instead of expense.
    /// </summary>
    private static List<ChoiceTemplate> GenerateMixedChoices(
        SituationArchetype archetype,
        string situationTemplateId,
        int scaledStatThreshold,
        int scaledCoinCost,
        int scaledCoinReward)
    {
        List<ChoiceTemplate> choices = new List<ChoiceTemplate>();

        // Choice 1: Stat-gated best outcome
        // For INCOME archetypes: Best payment + flat bonus
        // For EXPENSE archetypes: Free access if stat met
        // DDR-007 COMPLIANT: Fixed flat bonus (+3), not percentage-based
        Consequence statGatedConsequence = archetype.CoinReward > 0
            ? new Consequence { Coins = scaledCoinReward + 3 } // INCOME: Best payment (reward + flat bonus)
            : new Consequence(); // EXPENSE: No cost if stat met

        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_stat",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = GenerateStatGatedActionText(archetype),
            RequirementFormula = CreateStatRequirement(archetype, scaledStatThreshold),
            Consequence = statGatedConsequence,
            ActionType = ChoiceActionType.Instant
        });

        // Choice 2: Money path
        // For INCOME archetypes: Accept standard contract terms (earn coins)
        // For EXPENSE archetypes: Pay to access (spend coins)
        Consequence moneyConsequence = archetype.CoinReward > 0
            ? new Consequence { Coins = scaledCoinReward } // INCOME: Standard payment
            : new Consequence { Coins = -scaledCoinCost }; // EXPENSE: Pay the cost

        string moneyActionText = archetype.CoinReward > 0
            ? GenerateIncomeActionText(archetype)
            : GenerateMoneyActionText(archetype);

        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_money",
            PathType = ChoicePathType.InstantSuccess,
            ActionTextTemplate = moneyActionText,
            RequirementFormula = new CompoundRequirement(),
            Consequence = moneyConsequence,
            ActionType = ChoiceActionType.Instant
        });

        Consequence challengeConsequence = archetype.ResolveCost > 0
            ? new Consequence { Resolve = -archetype.ResolveCost }
            : new Consequence();

        CompoundRequirement challengeReq = new CompoundRequirement();
        if (archetype.ResolveCost > 0)
        {
            OrPath resourcePath = new OrPath { Label = "Resource Requirements", ResolveRequired = 0 };
            challengeReq.OrPaths.Add(resourcePath);
        }

        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_challenge",
            PathType = ChoicePathType.Challenge,
            ActionTextTemplate = GenerateChallengeActionText(archetype),
            RequirementFormula = challengeReq,
            Consequence = challengeConsequence,
            ActionType = ChoiceActionType.StartChallenge,
            ChallengeId = null,
            ChallengeType = archetype.ChallengeType,
            ChallengeDeckName = archetype.ChallengeDeckName
        });

        choices.Add(new ChoiceTemplate
        {
            Id = $"{situationTemplateId}_fallback",
            PathType = ChoicePathType.Fallback,
            ActionTextTemplate = GenerateFallbackActionText(archetype),
            RequirementFormula = new CompoundRequirement(),
            Consequence = new Consequence { TimeSegments = archetype.FallbackTimeCost },
            ActionType = ChoiceActionType.Instant
        });

        return choices;
    }

    /// <summary>
    /// Get crisis penalty based on archetype domain.
    /// Physical domains = health loss, Mental = stamina loss, Social = coin loss.
    /// </summary>
    private static Consequence GetCrisisPenalty(SituationArchetype archetype)
    {
        return archetype.Domain switch
        {
            Domain.Authority => new Consequence { Health = -10 },
            Domain.Economic => new Consequence { Coins = -5 },
            Domain.Physical => new Consequence { Stamina = -10 },
            Domain.Social => new Consequence { Focus = -10 },
            Domain.Mental => new Consequence { Stamina = -10 },
            _ => new Consequence { Health = -10 }
        };
    }

    /// <summary>
    /// Create consequence that grants a single stat point.
    /// FAIL-FAST: Throws for None or unknown stats - archetypes must specify valid primary/secondary stats.
    /// </summary>
    private static Consequence CreateStatGrantConsequence(PlayerStatType stat, int amount)
    {
        return stat switch
        {
            PlayerStatType.Insight => new Consequence { Insight = amount },
            PlayerStatType.Rapport => new Consequence { Rapport = amount },
            PlayerStatType.Authority => new Consequence { Authority = amount },
            PlayerStatType.Diplomacy => new Consequence { Diplomacy = amount },
            PlayerStatType.Cunning => new Consequence { Cunning = amount },
            _ => throw new InvalidOperationException(
                $"Cannot create stat grant consequence for stat type '{stat}'. " +
                $"Archetypes must specify valid primary/secondary stats (Insight, Rapport, Authority, Diplomacy, or Cunning).")
        };
    }

    /// <summary>
    /// Generate action text for Building primary choice.
    /// </summary>
    private static string GenerateBuildingPrimaryText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Assert yourself confidently",
            SituationArchetypeType.Negotiation => "Engage diplomatically",
            SituationArchetypeType.Investigation => "Analyze the situation carefully",
            SituationArchetypeType.SocialManeuvering => "Build rapport warmly",
            SituationArchetypeType.MeditationAndReflection => "Focus your mind",
            SituationArchetypeType.LocalConversation => "Share a friendly word",
            SituationArchetypeType.StudyInLibrary => "Study with focused attention",
            _ => "Approach with your primary skill"
        };
    }

    /// <summary>
    /// Generate action text for Building secondary choice.
    /// </summary>
    private static string GenerateBuildingSecondaryText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Stand firm with resolve",
            SituationArchetypeType.Negotiation => "Connect personally",
            SituationArchetypeType.Investigation => "Use cunning to deduce",
            SituationArchetypeType.SocialManeuvering => "Navigate with diplomacy",
            SituationArchetypeType.MeditationAndReflection => "Let your thoughts settle",
            SituationArchetypeType.LocalConversation => "Listen with genuine interest",
            SituationArchetypeType.StudyInLibrary => "Explore with curiosity",
            _ => "Approach with your secondary skill"
        };
    }

    /// <summary>
    /// Generate action text for Crisis stat choice.
    /// </summary>
    private static string GenerateCrisisStatText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Take command to prevent disaster",
            SituationArchetypeType.Negotiation => "Defuse the situation diplomatically",
            SituationArchetypeType.Investigation => "Spot the danger before it's too late",
            SituationArchetypeType.SocialManeuvering => "Smooth things over quickly",
            _ => "Use expertise to avoid the worst"
        };
    }

    /// <summary>
    /// Generate action text for Crisis money choice.
    /// </summary>
    private static string GenerateCrisisMoneyText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Pay to make the problem go away",
            SituationArchetypeType.Negotiation => "Bribe your way out of trouble",
            SituationArchetypeType.Investigation => "Pay for urgent information",
            SituationArchetypeType.SocialManeuvering => "Offer compensation for the offense",
            _ => "Pay to escape the situation"
        };
    }

    /// <summary>
    /// Generate action text for Crisis challenge choice.
    /// </summary>
    private static string GenerateCrisisChallengeText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Fight desperately to survive",
            SituationArchetypeType.Negotiation => "Desperately try to reason",
            SituationArchetypeType.Investigation => "Race to solve the puzzle",
            SituationArchetypeType.SocialManeuvering => "Make a risky social gambit",
            _ => "Take a desperate chance"
        };
    }

    /// <summary>
    /// Generate action text for Crisis fallback choice.
    /// </summary>
    private static string GenerateCrisisFallbackText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Suffer the consequences",
            SituationArchetypeType.Negotiation => "Accept the harsh terms",
            SituationArchetypeType.Investigation => "Fail to prevent the problem",
            SituationArchetypeType.SocialManeuvering => "Face the social fallout",
            _ => "Accept the damage"
        };
    }

    /// <summary>
    /// Create stat requirement with scaled threshold.
    /// Threshold parameter allows universal property scaling.
    /// </summary>
    public static CompoundRequirement CreateStatRequirement(SituationArchetype archetype, int scaledThreshold)
    {
        CompoundRequirement requirement = new CompoundRequirement();

        OrPath primaryPath = CreateOrPathForStat(archetype.PrimaryStat, scaledThreshold);
        requirement.OrPaths.Add(primaryPath);

        if (archetype.SecondaryStat != archetype.PrimaryStat)
        {
            OrPath secondaryPath = CreateOrPathForStat(archetype.SecondaryStat, scaledThreshold);
            requirement.OrPaths.Add(secondaryPath);
        }

        return requirement;
    }

    /// <summary>
    /// Create an OrPath with the appropriate explicit stat property set.
    /// Uses Explicit Property Principle - each stat has its own named property.
    /// </summary>
    public static OrPath CreateOrPathForStat(PlayerStatType stat, int threshold)
    {
        OrPath path = new OrPath { Label = $"{stat} {threshold}+" };

        switch (stat)
        {
            case PlayerStatType.Insight:
                path.InsightRequired = threshold;
                break;
            case PlayerStatType.Rapport:
                path.RapportRequired = threshold;
                break;
            case PlayerStatType.Authority:
                path.AuthorityRequired = threshold;
                break;
            case PlayerStatType.Diplomacy:
                path.DiplomacyRequired = threshold;
                break;
            case PlayerStatType.Cunning:
                path.CunningRequired = threshold;
                break;
        }

        return path;
    }

    private static string GenerateStatGatedActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Assert authority and take command",
            SituationArchetypeType.Negotiation => "Negotiate favorable terms",
            SituationArchetypeType.Investigation => "Deduce the solution through analysis",
            SituationArchetypeType.SocialManeuvering => "Read the social dynamics and navigate skillfully",
            SituationArchetypeType.Crisis => "Take decisive action with expertise",
            SituationArchetypeType.ServiceTransaction => "Use your expertise",
            SituationArchetypeType.AccessControl => "Present credentials",
            SituationArchetypeType.InformationGathering => "Ask the right questions",
            SituationArchetypeType.SkillDemonstration => "Demonstrate your competence",
            SituationArchetypeType.ReputationChallenge => "Defend your honor",
            SituationArchetypeType.EmergencyAid => "Apply expert treatment",
            SituationArchetypeType.AdministrativeProcedure => "Navigate bureaucracy skillfully",
            SituationArchetypeType.TradeDispute => "Leverage your position",
            SituationArchetypeType.CulturalFauxPas => "Apologize gracefully",
            SituationArchetypeType.Recruitment => "Negotiate terms",
            SituationArchetypeType.RestPreparation => "Optimize rest conditions",
            SituationArchetypeType.EnteringPrivateSpace => "Thoroughly inspect and optimize the space",
            SituationArchetypeType.DepartingPrivateSpace => "Systematically prepare and check everything",
            _ => "Use your expertise"
        };
    }

    private static string GenerateMoneyActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Pay off the opposition",
            SituationArchetypeType.Negotiation => "Pay the premium price",
            SituationArchetypeType.Investigation => "Hire an expert or pay for information",
            SituationArchetypeType.SocialManeuvering => "Offer a generous gift",
            SituationArchetypeType.Crisis => "Pay for emergency solution",
            SituationArchetypeType.ServiceTransaction => "Pay the asking price",
            SituationArchetypeType.AccessControl => "Bribe the gatekeeper",
            SituationArchetypeType.InformationGathering => "Buy the information",
            SituationArchetypeType.SkillDemonstration => "Hire someone to vouch",
            SituationArchetypeType.ReputationChallenge => "Offer compensation",
            SituationArchetypeType.EmergencyAid => "Pay for premium care",
            SituationArchetypeType.AdministrativeProcedure => "Expedite with payment",
            SituationArchetypeType.TradeDispute => "Offer settlement",
            SituationArchetypeType.CulturalFauxPas => "Offer gift as amends",
            SituationArchetypeType.Recruitment => "Buy time",
            SituationArchetypeType.RestPreparation => "Use comfort items for better rest",
            SituationArchetypeType.EnteringPrivateSpace => "Request comfort amenities",
            SituationArchetypeType.DepartingPrivateSpace => "Leave generous gratuity for staff",
            _ => "Pay to resolve"
        };
    }

    /// <summary>
    /// Generate action text for INCOME archetypes (ContractNegotiation, etc.)
    /// Choice 2 for income archetypes - accepting standard terms to EARN coins.
    /// </summary>
    private static string GenerateIncomeActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.ContractNegotiation => "Accept the standard contract terms",
            _ => "Accept the offered payment"
        };
    }

    private static string GenerateChallengeActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Attempt a physical confrontation",
            SituationArchetypeType.Negotiation => "Engage in complex debate",
            SituationArchetypeType.Investigation => "Work through the puzzle systematically",
            SituationArchetypeType.SocialManeuvering => "Make a bold social gambit",
            SituationArchetypeType.Crisis => "Risk everything on a desperate gambit",
            SituationArchetypeType.ServiceTransaction => "Attempt to bargain",
            SituationArchetypeType.AccessControl => "Force your way through",
            SituationArchetypeType.InformationGathering => "Investigate on your own",
            SituationArchetypeType.SkillDemonstration => "Attempt without preparation",
            SituationArchetypeType.ReputationChallenge => "Challenge to duel",
            SituationArchetypeType.EmergencyAid => "Risk improvised treatment",
            SituationArchetypeType.AdministrativeProcedure => "Navigate red tape",
            SituationArchetypeType.TradeDispute => "Escalate to arbitration",
            SituationArchetypeType.CulturalFauxPas => "Defend your action",
            SituationArchetypeType.Recruitment => "Counter-offer boldly",
            SituationArchetypeType.RestPreparation => "Force yourself to relax despite anxiety",
            SituationArchetypeType.EnteringPrivateSpace => "Push through discomfort mentally",
            SituationArchetypeType.DepartingPrivateSpace => "Force yourself to leave promptly",
            _ => "Accept the challenge"
        };
    }

    private static string GenerateFallbackActionText(SituationArchetype archetype)
    {
        return archetype.Type switch
        {
            SituationArchetypeType.Confrontation => "Back down and submit",
            SituationArchetypeType.Negotiation => "Accept unfavorable terms",
            SituationArchetypeType.Investigation => "Give up and move on",
            SituationArchetypeType.SocialManeuvering => "Exit awkwardly",
            SituationArchetypeType.Crisis => "Flee the situation",
            SituationArchetypeType.ServiceTransaction => "Leave without service",
            SituationArchetypeType.AccessControl => "Turn back",
            SituationArchetypeType.InformationGathering => "Move on without answers",
            SituationArchetypeType.SkillDemonstration => "Admit lack of skill",
            SituationArchetypeType.ReputationChallenge => "Apologize and back down",
            SituationArchetypeType.EmergencyAid => "Do nothing",
            SituationArchetypeType.AdministrativeProcedure => "Abandon the process",
            SituationArchetypeType.TradeDispute => "Accept the loss",
            SituationArchetypeType.CulturalFauxPas => "Ignore and act oblivious",
            SituationArchetypeType.Recruitment => "Refuse bluntly",
            SituationArchetypeType.RestPreparation => "Collapse from exhaustion immediately",
            SituationArchetypeType.EnteringPrivateSpace => "Collapse immediately without preparation",
            SituationArchetypeType.DepartingPrivateSpace => "Rush out without proper preparation",
            _ => "Accept poor outcome"
        };
    }

}
