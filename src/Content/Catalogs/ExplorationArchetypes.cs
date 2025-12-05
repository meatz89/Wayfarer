/// <summary>
/// Exploration archetypes - finding things out during the Frieren journey
/// These archetypes involve DISCOVERY and INFORMATION GATHERING:
/// - SeekAudience: Gaining access to authority figures
/// - InvestigateLocation: Searching for clues
/// - GatherTestimony: Interviewing witnesses
/// - DiscoverArtifact: Finding important objects
/// - UncoverConspiracy: Revealing hidden plots
///
/// PARSE-TIME ONLY - Called from SceneArchetypeCatalog.Generate()
/// </summary>
internal static class ExplorationArchetypes
{
    /// <summary>
    /// SEEK_AUDIENCE - Gaining access to authority figures
    /// Pattern: Linear (negotiate_access → audience)
    /// </summary>
    public static SceneArchetypeDefinition GenerateSeekAudience(GenerationContext context)
    {
        string sceneId = "seek_audience";

        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Negotiation);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            negotiateArchetype, $"{sceneId}_negotiate", context);

        List<ChoiceTemplate> enrichedNegotiateChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in negotiateChoices)
        {
            Consequence consequence = choice.Consequence ?? new Consequence();
            enrichedNegotiateChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = choice.ActionTextTemplate,
                RequirementFormula = choice.RequirementFormula,
                Consequence = consequence,
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
            NarrativeHints = new NarrativeHints { Tone = "formal", Theme = "access_negotiation", Context = "seeking_audience", Style = "diplomatic" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        PlacementFilter meetingChamberFilter = new PlacementFilter
        {
            PlacementType = PlacementType.Location,
            Privacy = LocationPrivacy.Private,
            Safety = LocationSafety.Safe,
            Activity = LocationActivity.Quiet,
            Purpose = LocationPurpose.Governance,
            SelectionStrategy = PlacementSelectionStrategy.First
        };

        SituationArchetype audienceArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Confrontation);
        List<ChoiceTemplate> audienceChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            audienceArchetype, $"{sceneId}_audience", context);

        SituationTemplate audienceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_audience",
            Type = SituationType.Normal,
            ChoiceTemplates = audienceChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints { Tone = "tense", Theme = "authority_meeting", Context = "formal_audience", Style = "dramatic" },
            LocationFilter = meetingChamberFilter,
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_negotiate"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { negotiateSituation, audienceSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// INVESTIGATE_LOCATION - Searching for clues
    /// Pattern: Linear (search → analyze → conclude)
    /// </summary>
    public static SceneArchetypeDefinition GenerateInvestigateLocation(GenerationContext context)
    {
        string sceneId = "investigate_location";

        SituationArchetype searchArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> searchChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            searchArchetype, $"{sceneId}_search", context);

        SituationTemplate searchSituation = new SituationTemplate
        {
            Id = $"{sceneId}_search",
            Type = SituationType.Normal,
            ChoiceTemplates = searchChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints { Tone = "mysterious", Theme = "investigation", Context = "searching_clues", Style = "atmospheric" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype analyzeArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> analyzeChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            analyzeArchetype, $"{sceneId}_analyze", context);

        SituationTemplate analyzeSituation = new SituationTemplate
        {
            Id = $"{sceneId}_analyze",
            Type = SituationType.Normal,
            ChoiceTemplates = analyzeChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints { Tone = "focused", Theme = "deduction", Context = "analyzing_evidence", Style = "cerebral" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype concludeArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> concludeChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            concludeArchetype, $"{sceneId}_conclude", context);

        SituationTemplate concludeSituation = new SituationTemplate
        {
            Id = $"{sceneId}_conclude",
            Type = SituationType.Normal,
            ChoiceTemplates = concludeChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints { Tone = "resolute", Theme = "revelation", Context = "investigation_conclusion", Style = "climactic" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_search"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { searchSituation, analyzeSituation, concludeSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// GATHER_TESTIMONY - Interviewing witnesses
    /// Pattern: Linear (approach → interview)
    /// </summary>
    public static SceneArchetypeDefinition GenerateGatherTestimony(GenerationContext context)
    {
        string sceneId = "gather_testimony";

        SituationArchetype approachArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.SocialManeuvering);
        List<ChoiceTemplate> approachChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            approachArchetype, $"{sceneId}_approach", context);

        SituationTemplate approachSituation = new SituationTemplate
        {
            Id = $"{sceneId}_approach",
            Type = SituationType.Normal,
            ChoiceTemplates = approachChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints { Tone = "careful", Theme = "approach", Context = "gaining_confidence", Style = "subtle" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype interviewArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Negotiation);
        List<ChoiceTemplate> interviewChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            interviewArchetype, $"{sceneId}_interview", context);

        SituationTemplate interviewSituation = new SituationTemplate
        {
            Id = $"{sceneId}_interview",
            Type = SituationType.Normal,
            ChoiceTemplates = interviewChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints { Tone = "probing", Theme = "interrogation", Context = "gathering_testimony", Style = "investigative" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_approach"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { approachSituation, interviewSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// DISCOVER_ARTIFACT - Finding important objects
    /// Pattern: Linear (locate → acquire)
    /// </summary>
    public static SceneArchetypeDefinition GenerateDiscoverArtifact(GenerationContext context)
    {
        string sceneId = "discover_artifact";

        SituationArchetype locateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> locateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            locateArchetype, $"{sceneId}_locate", context);

        SituationTemplate locateSituation = new SituationTemplate
        {
            Id = $"{sceneId}_locate",
            Type = SituationType.Normal,
            ChoiceTemplates = locateChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints { Tone = "anticipatory", Theme = "discovery", Context = "locating_artifact", Style = "atmospheric" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype acquireArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> acquireChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            acquireArchetype, $"{sceneId}_acquire", context);

        SituationTemplate acquireSituation = new SituationTemplate
        {
            Id = $"{sceneId}_acquire",
            Type = SituationType.Normal,
            ChoiceTemplates = acquireChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints { Tone = "triumphant", Theme = "acquisition", Context = "claiming_artifact", Style = "epic" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_locate"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { locateSituation, acquireSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// UNCOVER_CONSPIRACY - Revealing hidden plots
    /// Pattern: Linear (suspect → proof → expose → consequence)
    /// </summary>
    public static SceneArchetypeDefinition GenerateUncoverConspiracy(GenerationContext context)
    {
        string sceneId = "uncover_conspiracy";

        SituationArchetype suspectArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> suspectChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            suspectArchetype, $"{sceneId}_suspect", context);

        SituationTemplate suspectSituation = new SituationTemplate
        {
            Id = $"{sceneId}_suspect",
            Type = SituationType.Normal,
            ChoiceTemplates = suspectChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints { Tone = "suspicious", Theme = "conspiracy", Context = "initial_suspicion", Style = "tense" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype proofArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> proofChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            proofArchetype, $"{sceneId}_proof", context);

        SituationTemplate proofSituation = new SituationTemplate
        {
            Id = $"{sceneId}_proof",
            Type = SituationType.Normal,
            ChoiceTemplates = proofChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints { Tone = "determined", Theme = "evidence_gathering", Context = "proving_conspiracy", Style = "methodical" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype exposeArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Confrontation);
        List<ChoiceTemplate> exposeChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            exposeArchetype, $"{sceneId}_expose", context);

        SituationTemplate exposeSituation = new SituationTemplate
        {
            Id = $"{sceneId}_expose",
            Type = SituationType.Normal,
            ChoiceTemplates = exposeChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints { Tone = "dramatic", Theme = "revelation", Context = "exposing_conspiracy", Style = "climactic" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype consequenceArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> consequenceChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            consequenceArchetype, $"{sceneId}_consequence", context);

        SituationTemplate consequenceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_consequence",
            Type = SituationType.Normal,
            ChoiceTemplates = consequenceChoices,
            Priority = 70,
            NarrativeHints = new NarrativeHints { Tone = "grave", Theme = "consequence", Context = "conspiracy_aftermath", Style = "sobering" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_suspect"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { suspectSituation, proofSituation, exposeSituation, consequenceSituation },
            SpawnRules = spawnRules
        };
    }
}
