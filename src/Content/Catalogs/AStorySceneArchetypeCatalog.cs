/// <summary>
/// ⚠️ PARSE-TIME ONLY CATALOGUE ⚠️
///
/// Generates A-Story (Main Story) multi-situation scene structures from archetype IDs.
/// Each archetype represents a reusable narrative beat for the never-ending main quest.
///
/// CATALOGUE PATTERN COMPLIANCE:
/// - Called ONLY from SceneTemplateParser at PARSE TIME (or via dynamic package generation)
/// - NEVER called from facades, managers, or runtime code directly
/// - Generates SceneArchetypeDefinition that parser embeds in SceneTemplate
/// - Translation happens ONCE per template generation
///
/// ARCHITECTURE:
/// JSON specifies aStoryArchetypeId → Parser calls catalogue → Receives SituationTemplates + SpawnRules
/// → Parser stores in SceneTemplate → Runtime queries GameWorld.SceneTemplates (NO catalogue calls)
///
/// A-STORY ARCHETYPES (Main story narrative beats):
/// Investigation: seek_audience, investigate_location, gather_testimony, analyze_clues
/// Confrontation: confront_antagonist, challenge_authority, demand_answers, expose_corruption
/// Social: meet_order_member, negotiate_alliance, social_infiltration, gain_trust
/// Discovery: discover_artifact, uncover_conspiracy, reveal_truth, find_passage
/// Crisis: urgent_decision, moral_crossroads, life_or_death, sacrifice_choice
///
/// Each archetype defines:
/// - Specific situation count and structure (2-4 situations)
/// - Situation archetypes (delegates to SituationArchetypeCatalog for choice generation)
/// - Transition rules (Linear, Conditional branching)
/// - Story-critical dependent resources
/// - Narrative hints for AI generation context
///
/// GUARANTEED PROGRESSION PATTERN:
/// ALL A-story archetypes MUST include guaranteed success paths:
/// - Every situation has at least one choice with no requirements
/// - Challenge choices spawn scenes on BOTH success and failure
/// - Final situation: ALL choices spawn same next A-scene (guaranteed forward progress)
/// </summary>
public static class AStorySceneArchetypeCatalog
{
    /// <summary>
    /// Generate A-Story scene archetype definition by ID
    /// Called at parse time (or dynamic package generation) to generate complete scene structure
    /// Throws InvalidDataException on unknown archetype ID (fail fast)
    /// </summary>
    public static SceneArchetypeDefinition Generate(
        string archetypeId,
        int tier,
        GenerationContext context)
    {
        return archetypeId?.ToLowerInvariant() switch
        {
            "seek_audience" => GenerateSeekAudience(tier, context),
            "investigate_location" => GenerateInvestigateLocation(tier, context),
            "gather_testimony" => GenerateGatherTestimony(tier, context),
            "confront_antagonist" => GenerateConfrontAntagonist(tier, context),
            "meet_order_member" => GenerateMeetOrderMember(tier, context),
            "discover_artifact" => GenerateDiscoverArtifact(tier, context),
            "uncover_conspiracy" => GenerateUncoverConspiracy(tier, context),
            "urgent_decision" => GenerateUrgentDecision(tier, context),
            "moral_crossroads" => GenerateMoralCrossroads(tier, context),
            "gain_trust" => GenerateGainTrust(tier, context),
            "challenge_authority" => GenerateChallengeAuthority(tier, context),
            "expose_corruption" => GenerateExposeCorruption(tier, context),
            "social_infiltration" => GenerateSocialInfiltration(tier, context),
            "reveal_truth" => GenerateRevealTruth(tier, context),
            "sacrifice_choice" => GenerateSacrificeChoice(tier, context),

            _ => throw new InvalidDataException($"Unknown A-Story archetype ID: '{archetypeId}'. Valid archetypes: seek_audience, investigate_location, gather_testimony, confront_antagonist, meet_order_member, discover_artifact, uncover_conspiracy, urgent_decision, moral_crossroads, gain_trust, challenge_authority, expose_corruption, social_infiltration, reveal_truth, sacrifice_choice")
        };
    }

    /// <summary>
    /// SEEK_AUDIENCE archetype
    ///
    /// FICTIONAL CONTEXT: Player seeks audience with authority figure or important NPC
    /// STORY PURPOSE: Gatekeeper challenge to access next story beat
    ///
    /// Situation Count: 2
    /// Pattern: Linear (negotiate_access → audience)
    ///
    /// Situation 1 - Negotiate Access: Convince gatekeeper to grant audience
    ///   - Archetype: negotiation (Diplomacy/coins/challenge/fallback)
    ///   - Guaranteed path: Fallback (wait patiently) always succeeds
    ///   - Success rewards: Unlock meeting location, advance to audience
    ///
    /// Situation 2 - Audience: Meet with authority figure
    ///   - Archetype: confrontation (Authority/Social challenge)
    ///   - Guaranteed path: Fallback (respectful submission) always succeeds
    ///   - Success rewards: Story revelation, next A-scene spawn
    ///
    /// Dependent Resources: Meeting chamber (generated location)
    ///
    /// GUARANTEED PROGRESSION: Both situations have fallback choices with no requirements
    /// FINAL SITUATION: ALL choices spawn next A-scene (guaranteed forward progress)
    /// </summary>
    private static SceneArchetypeDefinition GenerateSeekAudience(int tier, GenerationContext context)
    {
        string sceneId = "seek_audience";

        // SITUATION 1: NEGOTIATE ACCESS
        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype("negotiation");
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            negotiateArchetype,
            $"{sceneId}_negotiate",
            context);

        // Enrich with unlock rewards
        List<ChoiceTemplate> enrichedNegotiateChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in negotiateChoices)
        {
            ChoiceReward reward = choice.RewardTemplate ?? new ChoiceReward();

            // All successful paths unlock meeting chamber
            if (choice.PathType != ChoicePathType.Fallback)
            {
                reward.LocationsToUnlock = new List<string> { "generated:meeting_chamber" };
            }

            enrichedNegotiateChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = choice.RequirementFormula,
                CostTemplate = choice.CostTemplate,
                RewardTemplate = reward,
                ActionType = choice.ActionType,
                ChallengeType = choice.ChallengeType
            });
        }

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = $"{sceneId}_negotiate",
            Type = SituationType.Normal,
            ChoiceTemplates = enrichedNegotiateChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "formal",
                Theme = "access_negotiation",
                Context = "seeking_audience",
                Style = "diplomatic"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 2: AUDIENCE
        SituationArchetype audienceArchetype = SituationArchetypeCatalog.GetArchetype("confrontation");
        List<ChoiceTemplate> audienceChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            audienceArchetype,
            $"{sceneId}_audience",
            context);

        SituationTemplate audienceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_audience",
            Type = SituationType.Normal,
            ChoiceTemplates = audienceChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "tense",
                Theme = "authority_meeting",
                Context = "formal_audience",
                Style = "dramatic"
            },
            RequiredLocationId = "generated:meeting_chamber",
            RequiredNpcId = context.NpcId
        };

        // Linear spawn rules
        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_negotiate",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_negotiate",
                DestinationSituationId = $"{sceneId}_audience",
                Condition = TransitionCondition.Always
            }
        }
        };

        // Dependent resources
        DependentLocationSpec meetingChamber = new DependentLocationSpec
        {
            TemplateId = "meeting_chamber",
            NamePattern = "{NPCName}'s Chamber",
            DescriptionPattern = "A formal meeting space where important discussions take place.",
            VenueIdSource = VenueIdSource.SameAsBase,
            HexPlacement = HexPlacementStrategy.SameVenue,
            Properties = new List<string> { "indoor", "private", "formal" },
            IsLockedInitially = true,
            UnlockItemTemplateId = null,
            CanInvestigate = false
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        negotiateSituation,
        audienceSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec> { meetingChamber },
            DependentItems = new List<DependentItemSpec>()
        };
    }

    /// <summary>
    /// INVESTIGATE_LOCATION archetype
    ///
    /// FICTIONAL CONTEXT: Player investigates location for clues about the Order
    /// STORY PURPOSE: Procedural investigation advancing mystery
    ///
    /// Situation Count: 3
    /// Pattern: Linear (search → analyze → conclude)
    ///
    /// Classic investigation flow reusable for any location-based mystery beat.
    /// </summary>
    private static SceneArchetypeDefinition GenerateInvestigateLocation(int tier, GenerationContext context)
    {
        string sceneId = "investigate_location";

        // SITUATION 1: SEARCH LOCATION
        SituationArchetype searchArchetype = SituationArchetypeCatalog.GetArchetype("investigation");
        List<ChoiceTemplate> searchChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            searchArchetype,
            $"{sceneId}_search",
            context);

        // Enrich with evidence rewards
        List<ChoiceTemplate> enrichedSearchChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in searchChoices)
        {
            ChoiceReward reward = choice.RewardTemplate ?? new ChoiceReward();

            if (choice.PathType != ChoicePathType.Fallback)
            {
                reward.ItemIds = new List<string> { "generated:evidence" };
            }

            enrichedSearchChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = choice.RequirementFormula,
                CostTemplate = choice.CostTemplate,
                RewardTemplate = reward,
                ActionType = choice.ActionType,
                ChallengeType = choice.ChallengeType
            });
        }

        SituationTemplate searchSituation = new SituationTemplate
        {
            Id = $"{sceneId}_search",
            Type = SituationType.Normal,
            ChoiceTemplates = enrichedSearchChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "mysterious",
                Theme = "investigation",
                Context = "searching_clues",
                Style = "atmospheric"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = null
        };

        // SITUATION 2: ANALYZE EVIDENCE
        SituationArchetype analyzeArchetype = SituationArchetypeCatalog.GetArchetype("investigation");
        List<ChoiceTemplate> analyzeChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            analyzeArchetype,
            $"{sceneId}_analyze",
            context);

        SituationTemplate analyzeSituation = new SituationTemplate
        {
            Id = $"{sceneId}_analyze",
            Type = SituationType.Normal,
            ChoiceTemplates = analyzeChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "focused",
                Theme = "deduction",
                Context = "analyzing_evidence",
                Style = "cerebral"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = null
        };

        // SITUATION 3: CONCLUDE INVESTIGATION
        SituationArchetype concludeArchetype = SituationArchetypeCatalog.GetArchetype("investigation");
        List<ChoiceTemplate> concludeChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            concludeArchetype,
            $"{sceneId}_conclude",
            context);

        SituationTemplate concludeSituation = new SituationTemplate
        {
            Id = $"{sceneId}_conclude",
            Type = SituationType.Normal,
            ChoiceTemplates = concludeChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "resolute",
                Theme = "revelation",
                Context = "investigation_conclusion",
                Style = "climactic"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_search",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_search",
                DestinationSituationId = $"{sceneId}_analyze",
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_analyze",
                DestinationSituationId = $"{sceneId}_conclude",
                Condition = TransitionCondition.Always
            }
        }
        };

        DependentItemSpec evidence = new DependentItemSpec
        {
            TemplateId = "evidence",
            NamePattern = "Evidence Fragment",
            DescriptionPattern = "A clue you discovered during your investigation.",
            Categories = new List<ItemCategory> { ItemCategory.Documents },
            Weight = 1,
            BuyPrice = 0,
            SellPrice = 0,
            AddToInventoryOnCreation = true,
            SpawnLocationTemplateId = null
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        searchSituation,
        analyzeSituation,
        concludeSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),
            DependentItems = new List<DependentItemSpec> { evidence }
        };
    }

    /// <summary>
    /// GATHER_TESTIMONY archetype
    ///
    /// FICTIONAL CONTEXT: Player gathers testimony from witness/informant about Order activities
    /// STORY PURPOSE: Social information gathering advancing investigation
    ///
    /// Situation Count: 2
    /// Pattern: Linear (approach → interview)
    /// </summary>
    private static SceneArchetypeDefinition GenerateGatherTestimony(int tier, GenerationContext context)
    {
        string sceneId = "gather_testimony";

        // SITUATION 1: APPROACH WITNESS
        SituationArchetype approachArchetype = SituationArchetypeCatalog.GetArchetype("social_maneuvering");
        List<ChoiceTemplate> approachChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            approachArchetype,
            $"{sceneId}_approach",
            context);

        SituationTemplate approachSituation = new SituationTemplate
        {
            Id = $"{sceneId}_approach",
            Type = SituationType.Normal,
            ChoiceTemplates = approachChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "careful",
                Theme = "approach",
                Context = "gaining_confidence",
                Style = "subtle"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 2: INTERVIEW
        SituationArchetype interviewArchetype = SituationArchetypeCatalog.GetArchetype("negotiation");
        List<ChoiceTemplate> interviewChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            interviewArchetype,
            $"{sceneId}_interview",
            context);

        SituationTemplate interviewSituation = new SituationTemplate
        {
            Id = $"{sceneId}_interview",
            Type = SituationType.Normal,
            ChoiceTemplates = interviewChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "probing",
                Theme = "interrogation",
                Context = "gathering_testimony",
                Style = "investigative"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_approach",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_approach",
                DestinationSituationId = $"{sceneId}_interview",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        approachSituation,
        interviewSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),
            DependentItems = new List<DependentItemSpec>()
        };
    }

    /// <summary>
    /// CONFRONT_ANTAGONIST archetype
    ///
    /// FICTIONAL CONTEXT: Player confronts antagonist with evidence/accusation
    /// STORY PURPOSE: Dramatic confrontation advancing conflict
    ///
    /// Situation Count: 2
    /// Pattern: Linear (accusation → resolution)
    /// </summary>
    private static SceneArchetypeDefinition GenerateConfrontAntagonist(int tier, GenerationContext context)
    {
        string sceneId = "confront_antagonist";

        // SITUATION 1: ACCUSATION
        SituationArchetype accuseArchetype = SituationArchetypeCatalog.GetArchetype("confrontation");
        List<ChoiceTemplate> accuseChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            accuseArchetype,
            $"{sceneId}_accuse",
            context);

        SituationTemplate accuseSituation = new SituationTemplate
        {
            Id = $"{sceneId}_accuse",
            Type = SituationType.Normal,
            ChoiceTemplates = accuseChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "confrontational",
                Theme = "accusation",
                Context = "dramatic_confrontation",
                Style = "intense"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 2: RESOLUTION
        SituationArchetype resolveArchetype = SituationArchetypeCatalog.GetArchetype("crisis");
        List<ChoiceTemplate> resolveChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            resolveArchetype,
            $"{sceneId}_resolve",
            context);

        SituationTemplate resolveSituation = new SituationTemplate
        {
            Id = $"{sceneId}_resolve",
            Type = SituationType.Normal,
            ChoiceTemplates = resolveChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "decisive",
                Theme = "resolution",
                Context = "confrontation_outcome",
                Style = "climactic"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_accuse",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_accuse",
                DestinationSituationId = $"{sceneId}_resolve",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        accuseSituation,
        resolveSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),
            DependentItems = new List<DependentItemSpec>()
        };
    }

    /// <summary>
    /// MEET_ORDER_MEMBER archetype
    ///
    /// FICTIONAL CONTEXT: Player meets member of scattered Order, each knows one piece of mystery
    /// STORY PURPOSE: Incremental revelation, deepening pursuit
    ///
    /// Situation Count: 3
    /// Pattern: Linear (contact → negotiate_info → revelation)
    /// </summary>
    private static SceneArchetypeDefinition GenerateMeetOrderMember(int tier, GenerationContext context)
    {
        string sceneId = "meet_order_member";

        // SITUATION 1: INITIAL CONTACT
        SituationArchetype contactArchetype = SituationArchetypeCatalog.GetArchetype("social_maneuvering");
        List<ChoiceTemplate> contactChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            contactArchetype,
            $"{sceneId}_contact",
            context);

        SituationTemplate contactSituation = new SituationTemplate
        {
            Id = $"{sceneId}_contact",
            Type = SituationType.Normal,
            ChoiceTemplates = contactChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "cautious",
                Theme = "first_contact",
                Context = "meeting_order_member",
                Style = "mysterious"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 2: NEGOTIATE INFORMATION
        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype("negotiation");
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            negotiateArchetype,
            $"{sceneId}_negotiate",
            context);

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = $"{sceneId}_negotiate",
            Type = SituationType.Normal,
            ChoiceTemplates = negotiateChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "tense",
                Theme = "information_exchange",
                Context = "negotiating_knowledge",
                Style = "strategic"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 3: REVELATION
        SituationArchetype revelationArchetype = SituationArchetypeCatalog.GetArchetype("investigation");
        List<ChoiceTemplate> revelationChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            revelationArchetype,
            $"{sceneId}_revelation",
            context);

        SituationTemplate revelationSituation = new SituationTemplate
        {
            Id = $"{sceneId}_revelation",
            Type = SituationType.Normal,
            ChoiceTemplates = revelationChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "revelatory",
                Theme = "knowledge_gained",
                Context = "order_secret_revealed",
                Style = "impactful"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_contact",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_contact",
                DestinationSituationId = $"{sceneId}_negotiate",
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_negotiate",
                DestinationSituationId = $"{sceneId}_revelation",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        contactSituation,
        negotiateSituation,
        revelationSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),
            DependentItems = new List<DependentItemSpec>()
        };
    }

    /// <summary>
    /// DISCOVER_ARTIFACT archetype
    ///
    /// FICTIONAL CONTEXT: Player discovers Order artifact (relic, document, tool)
    /// STORY PURPOSE: Physical progression token, lore advancement
    ///
    /// Situation Count: 2
    /// Pattern: Linear (locate → acquire)
    /// </summary>
    private static SceneArchetypeDefinition GenerateDiscoverArtifact(int tier, GenerationContext context)
    {
        string sceneId = "discover_artifact";

        // SITUATION 1: LOCATE ARTIFACT
        SituationArchetype locateArchetype = SituationArchetypeCatalog.GetArchetype("investigation");
        List<ChoiceTemplate> locateChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            locateArchetype,
            $"{sceneId}_locate",
            context);

        SituationTemplate locateSituation = new SituationTemplate
        {
            Id = $"{sceneId}_locate",
            Type = SituationType.Normal,
            ChoiceTemplates = locateChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "anticipatory",
                Theme = "discovery",
                Context = "locating_artifact",
                Style = "atmospheric"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = null
        };

        // SITUATION 2: ACQUIRE ARTIFACT
        SituationArchetype acquireArchetype = SituationArchetypeCatalog.GetArchetype("crisis");
        List<ChoiceTemplate> acquireChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            acquireArchetype,
            $"{sceneId}_acquire",
            context);

        // Enrich with artifact reward
        List<ChoiceTemplate> enrichedAcquireChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in acquireChoices)
        {
            ChoiceReward reward = choice.RewardTemplate ?? new ChoiceReward();
            reward.ItemIds = new List<string> { "generated:order_artifact" };

            enrichedAcquireChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = choice.RequirementFormula,
                CostTemplate = choice.CostTemplate,
                RewardTemplate = reward,
                ActionType = choice.ActionType,
                ChallengeType = choice.ChallengeType
            });
        }

        SituationTemplate acquireSituation = new SituationTemplate
        {
            Id = $"{sceneId}_acquire",
            Type = SituationType.Normal,
            ChoiceTemplates = enrichedAcquireChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "triumphant",
                Theme = "acquisition",
                Context = "claiming_artifact",
                Style = "epic"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_locate",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_locate",
                DestinationSituationId = $"{sceneId}_acquire",
                Condition = TransitionCondition.Always
            }
        }
        };

        DependentItemSpec artifact = new DependentItemSpec
        {
            TemplateId = "order_artifact",
            NamePattern = "Order Artifact",
            DescriptionPattern = "An ancient artifact once belonging to the scattered Order.",
            Categories = new List<ItemCategory> { ItemCategory.Valuables },  // Use Valuables instead of removed Treasure
            Weight = 3,
            BuyPrice = 0,
            SellPrice = 100,
            AddToInventoryOnCreation = true,
            SpawnLocationTemplateId = null
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        locateSituation,
        acquireSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),
            DependentItems = new List<DependentItemSpec> { artifact }
        };
    }

    /// <summary>
    /// UNCOVER_CONSPIRACY archetype
    ///
    /// FICTIONAL CONTEXT: Player uncovers conspiracy related to Order's fall
    /// STORY PURPOSE: Major plot revelation, raising stakes
    ///
    /// Situation Count: 4
    /// Pattern: Linear (suspect → gather_proof → expose → consequence)
    /// </summary>
    private static SceneArchetypeDefinition GenerateUncoverConspiracy(int tier, GenerationContext context)
    {
        string sceneId = "uncover_conspiracy";

        // SITUATION 1: SUSPICION
        SituationArchetype suspectArchetype = SituationArchetypeCatalog.GetArchetype("investigation");
        List<ChoiceTemplate> suspectChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            suspectArchetype,
            $"{sceneId}_suspect",
            context);

        SituationTemplate suspectSituation = new SituationTemplate
        {
            Id = $"{sceneId}_suspect",
            Type = SituationType.Normal,
            ChoiceTemplates = suspectChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "suspicious",
                Theme = "conspiracy",
                Context = "initial_suspicion",
                Style = "tense"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = null
        };

        // SITUATION 2: GATHER PROOF
        SituationArchetype proofArchetype = SituationArchetypeCatalog.GetArchetype("investigation");
        List<ChoiceTemplate> proofChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            proofArchetype,
            $"{sceneId}_proof",
            context);

        SituationTemplate proofSituation = new SituationTemplate
        {
            Id = $"{sceneId}_proof",
            Type = SituationType.Normal,
            ChoiceTemplates = proofChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "determined",
                Theme = "evidence_gathering",
                Context = "proving_conspiracy",
                Style = "methodical"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = null
        };

        // SITUATION 3: EXPOSE
        SituationArchetype exposeArchetype = SituationArchetypeCatalog.GetArchetype("confrontation");
        List<ChoiceTemplate> exposeChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            exposeArchetype,
            $"{sceneId}_expose",
            context);

        SituationTemplate exposeSituation = new SituationTemplate
        {
            Id = $"{sceneId}_expose",
            Type = SituationType.Normal,
            ChoiceTemplates = exposeChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "dramatic",
                Theme = "revelation",
                Context = "exposing_conspiracy",
                Style = "climactic"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 4: CONSEQUENCE
        SituationArchetype consequenceArchetype = SituationArchetypeCatalog.GetArchetype("crisis");
        List<ChoiceTemplate> consequenceChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            consequenceArchetype,
            $"{sceneId}_consequence",
            context);

        SituationTemplate consequenceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_consequence",
            Type = SituationType.Normal,
            ChoiceTemplates = consequenceChoices,
            Priority = 70,
            NarrativeHints = new NarrativeHints
            {
                Tone = "grave",
                Theme = "consequence",
                Context = "conspiracy_aftermath",
                Style = "sobering"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_suspect",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_suspect",
                DestinationSituationId = $"{sceneId}_proof",
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_proof",
                DestinationSituationId = $"{sceneId}_expose",
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_expose",
                DestinationSituationId = $"{sceneId}_consequence",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        suspectSituation,
        proofSituation,
        exposeSituation,
        consequenceSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),
            DependentItems = new List<DependentItemSpec>()
        };
    }

    /// <summary>
    /// URGENT_DECISION archetype
    ///
    /// FICTIONAL CONTEXT: Player must make urgent time-pressured decision
    /// STORY PURPOSE: Crisis moment, testing player values
    ///
    /// Situation Count: 2
    /// Pattern: Linear (crisis → decision)
    /// </summary>
    private static SceneArchetypeDefinition GenerateUrgentDecision(int tier, GenerationContext context)
    {
        string sceneId = "urgent_decision";

        // SITUATION 1: CRISIS EMERGES
        SituationArchetype crisisArchetype = SituationArchetypeCatalog.GetArchetype("crisis");
        List<ChoiceTemplate> crisisChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            crisisArchetype,
            $"{sceneId}_crisis",
            context);

        SituationTemplate crisisSituation = new SituationTemplate
        {
            Id = $"{sceneId}_crisis",
            Type = SituationType.Crisis,
            ChoiceTemplates = crisisChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "urgent",
                Theme = "crisis",
                Context = "emergency",
                Style = "intense"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 2: DECISION
        SituationArchetype decisionArchetype = SituationArchetypeCatalog.GetArchetype("crisis");
        List<ChoiceTemplate> decisionChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            decisionArchetype,
            $"{sceneId}_decision",
            context);

        SituationTemplate decisionSituation = new SituationTemplate
        {
            Id = $"{sceneId}_decision",
            Type = SituationType.Crisis,
            ChoiceTemplates = decisionChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "desperate",
                Theme = "decision",
                Context = "urgent_choice",
                Style = "high_stakes"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_crisis",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_crisis",
                DestinationSituationId = $"{sceneId}_decision",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        crisisSituation,
        decisionSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),
            DependentItems = new List<DependentItemSpec>()
        };
    }

    /// <summary>
    /// MORAL_CROSSROADS archetype
    ///
    /// FICTIONAL CONTEXT: Player faces moral dilemma with lasting consequences
    /// STORY PURPOSE: Player agency, values expression
    ///
    /// Situation Count: 3
    /// Pattern: Linear (dilemma → choice → consequence)
    /// </summary>
    private static SceneArchetypeDefinition GenerateMoralCrossroads(int tier, GenerationContext context)
    {
        string sceneId = "moral_crossroads";

        // SITUATION 1: DILEMMA PRESENTED
        SituationArchetype dilemmaArchetype = SituationArchetypeCatalog.GetArchetype("social_maneuvering");
        List<ChoiceTemplate> dilemmaChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            dilemmaArchetype,
            $"{sceneId}_dilemma",
            context);

        SituationTemplate dilemmaSituation = new SituationTemplate
        {
            Id = $"{sceneId}_dilemma",
            Type = SituationType.Normal,
            ChoiceTemplates = dilemmaChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "conflicted",
                Theme = "moral_dilemma",
                Context = "ethical_choice",
                Style = "thoughtful"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 2: MORAL CHOICE
        SituationArchetype choiceArchetype = SituationArchetypeCatalog.GetArchetype("crisis");
        List<ChoiceTemplate> choiceChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            choiceArchetype,
            $"{sceneId}_choice",
            context);

        SituationTemplate choiceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_choice",
            Type = SituationType.Normal,
            ChoiceTemplates = choiceChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "weighty",
                Theme = "moral_stance",
                Context = "defining_moment",
                Style = "impactful"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        // SITUATION 3: CONSEQUENCE
        SituationArchetype consequenceArchetype = SituationArchetypeCatalog.GetArchetype("social_maneuvering");
        List<ChoiceTemplate> consequenceChoices = SituationArchetypeCatalog.GenerateChoiceTemplates(
            consequenceArchetype,
            $"{sceneId}_consequence",
            context);

        SituationTemplate consequenceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_consequence",
            Type = SituationType.Normal,
            ChoiceTemplates = consequenceChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints
            {
                Tone = "reflective",
                Theme = "consequence",
                Context = "moral_aftermath",
                Style = "somber"
            },
            RequiredLocationId = context.LocationId,
            RequiredNpcId = context.NpcId
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationId = $"{sceneId}_dilemma",
            Transitions = new List<SituationTransition>
        {
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_dilemma",
                DestinationSituationId = $"{sceneId}_choice",
                Condition = TransitionCondition.Always
            },
            new SituationTransition
            {
                SourceSituationId = $"{sceneId}_choice",
                DestinationSituationId = $"{sceneId}_consequence",
                Condition = TransitionCondition.Always
            }
        }
        };

        List<SituationTemplate> situations = new List<SituationTemplate>
    {
        dilemmaSituation,
        choiceSituation,
        consequenceSituation
    };

        // CRITICAL: Enrich final situation to spawn next A-scene (infinite progression)
        EnrichFinalSituationWithNextASceneSpawn(situations, context);

        return new SceneArchetypeDefinition
        {
            SituationTemplates = situations,
            SpawnRules = spawnRules,
            DependentLocations = new List<DependentLocationSpec>(),
            DependentItems = new List<DependentItemSpec>()
        };
    }

    // ===================================================================
    // INFINITE A-STORY PROGRESSION: Final Situation Enrichment
    // ===================================================================

    /// <summary>
    /// Enrich final situation to spawn next A-scene (CRITICAL for infinite progression)
    ///
    /// GUARANTEED PROGRESSION PATTERN:
    /// - Final situation ALL choices spawn next A-scene
    /// - Ensures forward progress regardless of player choices
    /// - Infinite A-story loop: A11 → A12 → A13 → ... → infinity
    ///
    /// Called after generating situation templates, before returning definition
    /// Modifies final situation's choice templates in-place
    /// </summary>
    private static void EnrichFinalSituationWithNextASceneSpawn(
        List<SituationTemplate> situations,
        GenerationContext context)
    {
        if (!context.AStorySequence.HasValue)
        {
            return; // Not an A-story scene, no enrichment needed
        }

        if (situations.Count == 0)
        {
            return; // No situations to enrich
        }

        // Final situation = last situation in list
        SituationTemplate finalSituation = situations[situations.Count - 1];

        // Next A-scene ID
        string nextASceneId = $"a_story_{context.AStorySequence.Value + 1}";

        // Enrich ALL choices with SceneSpawnReward
        List<ChoiceTemplate> enrichedChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in finalSituation.ChoiceTemplates)
        {
            ChoiceReward reward = choice.RewardTemplate ?? new ChoiceReward();

            // Add next A-scene spawn reward
            reward.ScenesToSpawn = new List<SceneSpawnReward>
        {
            new SceneSpawnReward
            {
                SceneTemplateId = nextASceneId,
                PlacementRelation = PlacementRelation.SameLocation
            }
        };

            enrichedChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = choice.RequirementFormula,
                CostTemplate = choice.CostTemplate,
                RewardTemplate = reward,
                ActionType = choice.ActionType,
                ChallengeType = choice.ChallengeType
            });
        }

        // Replace final situation's choices with enriched versions
        finalSituation.ChoiceTemplates.Clear();
        finalSituation.ChoiceTemplates.AddRange(enrichedChoices);

        Console.WriteLine($"[AStoryArchetype] Enriched final situation '{finalSituation.Id}' - ALL {enrichedChoices.Count} choices spawn next A-scene '{nextASceneId}'");
    }

    // Remaining archetypes follow same pattern - abbreviated for brevity
    // Each provides 2-4 situation linear/branching flows with guaranteed progression

    private static SceneArchetypeDefinition GenerateGainTrust(int tier, GenerationContext context)
    {
        // 2-situation social maneuvering + negotiation flow
        throw new NotImplementedException("Archetype pending full implementation");
    }

    private static SceneArchetypeDefinition GenerateChallengeAuthority(int tier, GenerationContext context)
    {
        // 2-situation confrontation + crisis flow
        throw new NotImplementedException("Archetype pending full implementation");
    }

    private static SceneArchetypeDefinition GenerateExposeCorruption(int tier, GenerationContext context)
    {
        // 3-situation investigation + confrontation + consequence flow
        throw new NotImplementedException("Archetype pending full implementation");
    }

    private static SceneArchetypeDefinition GenerateSocialInfiltration(int tier, GenerationContext context)
    {
        // 3-situation social maneuvering flow (approach + infiltrate + extract)
        throw new NotImplementedException("Archetype pending full implementation");
    }

    private static SceneArchetypeDefinition GenerateRevealTruth(int tier, GenerationContext context)
    {
        // 2-situation investigation + confrontation flow
        throw new NotImplementedException("Archetype pending full implementation");
    }

    private static SceneArchetypeDefinition GenerateSacrificeChoice(int tier, GenerationContext context)
    {
        // 3-situation crisis flow (stakes + decision + sacrifice)
        throw new NotImplementedException("Archetype pending full implementation");
    }
}
