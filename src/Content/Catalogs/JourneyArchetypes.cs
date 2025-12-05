/// <summary>
/// Journey archetypes - the logistics of the Frieren infinite journey
/// These archetypes form the FRAMEWORK around which story beats occur:
/// - DeliveryContract: How journeys begin (economic entry point)
/// - RouteSegmentTravel: How journeys proceed (movement with obstacles)
/// - InnLodging: How journeys pause (rest and recovery)
/// - ConsequenceReflection: How journeys transform (processing outcomes)
///
/// PARSE-TIME ONLY - Called from SceneArchetypeCatalog.Generate()
/// </summary>
internal static class JourneyArchetypes
{
    /// <summary>
    /// DELIVERY_CONTRACT - How journeys begin
    /// Pattern: Linear (offer → negotiation)
    /// </summary>
    public static SceneArchetypeDefinition GenerateDeliveryContract(GenerationContext context)
    {
        string sceneId = "delivery_contract";
        string offerSitId = $"{sceneId}_offer";
        string negotiateSitId = $"{sceneId}_negotiate";

        SituationTemplate offerSituation = new SituationTemplate
        {
            Id = offerSitId,
            Name = "Delivery Opportunity",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = new List<ChoiceTemplate>
            {
                new ChoiceTemplate
                {
                    Id = $"{offerSitId}_accept",
                    PathType = ChoicePathType.InstantSuccess,
                    ActionTextTemplate = "Accept the opportunity",
                    RequirementFormula = new CompoundRequirement(),
                    Consequence = new Consequence(),
                    ActionType = ChoiceActionType.Instant
                },
                new ChoiceTemplate
                {
                    Id = $"{offerSitId}_decline",
                    PathType = ChoicePathType.Fallback,
                    ActionTextTemplate = "Not right now",
                    RequirementFormula = new CompoundRequirement(),
                    Consequence = new Consequence(),
                    ActionType = ChoiceActionType.Instant
                }
            },
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "opportunistic",
                Theme = "contract_offer",
                Context = "delivery_opportunity",
                Style = "direct"
            },
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Purpose = LocationPurpose.Commerce
            },
            NpcFilter = new PlacementFilter
            {
                PlacementType = PlacementType.NPC,
                Profession = Professions.Merchant
            },
            RouteFilter = null
        };

        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ContractNegotiation);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            negotiateArchetype,
            negotiateSitId,
            context);

        List<ChoiceTemplate> enrichedNegotiateChoices = new List<ChoiceTemplate>();
        foreach (ChoiceTemplate choice in negotiateChoices)
        {
            string enrichedActionText = choice.PathType == ChoicePathType.Fallback
                ? "Back out of the deal"
                : choice.ActionTextTemplate;

            enrichedNegotiateChoices.Add(new ChoiceTemplate
            {
                Id = choice.Id,
                PathType = choice.PathType,
                ActionTextTemplate = enrichedActionText,
                RequirementFormula = choice.RequirementFormula,
                Consequence = choice.Consequence,
                OnSuccessConsequence = choice.OnSuccessConsequence,
                OnFailureConsequence = choice.OnFailureConsequence,
                ActionType = choice.ActionType,
                ChallengeId = choice.ChallengeId,
                ChallengeType = choice.ChallengeType,
                ChallengeDeckName = choice.ChallengeDeckName,
                NavigationPayload = choice.NavigationPayload
            });
        }

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Name = "Contract Terms",
            Type = SituationType.Normal,
            SystemType = negotiateArchetype.ChallengeType,
            NarrativeTemplate = null,
            ChoiceTemplates = enrichedNegotiateChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints
            {
                Tone = "transactional",
                Theme = "negotiation",
                Context = "contract_terms",
                Style = "businesslike"
            },
            LocationFilter = new PlacementFilter
            {
                PlacementType = PlacementType.Location,
                Purpose = LocationPurpose.Commerce
            },
            NpcFilter = new PlacementFilter
            {
                PlacementType = PlacementType.NPC,
                Profession = Professions.Merchant
            },
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = offerSitId
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { offerSituation, negotiateSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// ROUTE_SEGMENT_TRAVEL - How journeys proceed
    /// Pattern: Linear (3 obstacles → approach → arrival)
    /// </summary>
    public static SceneArchetypeDefinition GenerateRouteSegmentTravel(GenerationContext context)
    {
        string sceneId = "route_segment_travel";
        string obstacle1SitId = $"{sceneId}_obstacle1";
        string obstacle2SitId = $"{sceneId}_obstacle2";
        string obstacle3SitId = $"{sceneId}_obstacle3";
        string approachSitId = $"{sceneId}_approach";
        string arrivalSitId = $"{sceneId}_arrival";

        SituationArchetype obstacle1Archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Confrontation);
        List<ChoiceTemplate> obstacle1Choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            obstacle1Archetype, obstacle1SitId, context);

        SituationTemplate obstacle1Situation = new SituationTemplate
        {
            Id = obstacle1SitId,
            Name = "Forest Obstacle",
            Type = SituationType.Normal,
            SystemType = obstacle1Archetype.ChallengeType,
            NarrativeTemplate = null,
            ChoiceTemplates = obstacle1Choices,
            Priority = 100,
            NarrativeHints = new NarrativeHints { Tone = "challenging", Theme = "physical_obstacle", Context = "route_travel", Style = "action" },
            RouteFilter = new PlacementFilter { PlacementType = PlacementType.Route, SegmentIndex = 0 },
            LocationFilter = new PlacementFilter { PlacementType = PlacementType.Location, Proximity = PlacementProximity.SameLocation },
            NpcFilter = null
        };

        SituationArchetype obstacle2Archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> obstacle2Choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            obstacle2Archetype, obstacle2SitId, context);

        SituationTemplate obstacle2Situation = new SituationTemplate
        {
            Id = obstacle2SitId,
            Name = "River Crossing",
            Type = SituationType.Normal,
            SystemType = obstacle2Archetype.ChallengeType,
            NarrativeTemplate = null,
            ChoiceTemplates = obstacle2Choices,
            Priority = 90,
            NarrativeHints = new NarrativeHints { Tone = "analytical", Theme = "mental_challenge", Context = "route_travel", Style = "thoughtful" },
            RouteFilter = new PlacementFilter { PlacementType = PlacementType.Route, SegmentIndex = 1 },
            LocationFilter = new PlacementFilter { PlacementType = PlacementType.Location, Proximity = PlacementProximity.SameLocation },
            NpcFilter = null
        };

        SituationArchetype obstacle3Archetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.SocialManeuvering);
        List<ChoiceTemplate> obstacle3Choices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            obstacle3Archetype, obstacle3SitId, context);

        SituationTemplate obstacle3Situation = new SituationTemplate
        {
            Id = obstacle3SitId,
            Name = "Checkpoint Guard",
            Type = SituationType.Normal,
            SystemType = obstacle3Archetype.ChallengeType,
            NarrativeTemplate = null,
            ChoiceTemplates = obstacle3Choices,
            Priority = 80,
            NarrativeHints = new NarrativeHints { Tone = "transactional", Theme = "social_negotiation", Context = "route_travel", Style = "diplomatic" },
            RouteFilter = new PlacementFilter { PlacementType = PlacementType.Route, SegmentIndex = 2 },
            LocationFilter = new PlacementFilter { PlacementType = PlacementType.Location, Proximity = PlacementProximity.SameLocation },
            NpcFilter = null
        };

        SituationArchetype approachArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.RestPreparation);
        List<ChoiceTemplate> approachChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            approachArchetype, approachSitId, context);

        SituationTemplate approachSituation = new SituationTemplate
        {
            Id = approachSitId,
            Name = "Final Approach",
            Type = SituationType.Normal,
            SystemType = approachArchetype.ChallengeType,
            NarrativeTemplate = null,
            ChoiceTemplates = approachChoices,
            Priority = 70,
            NarrativeHints = new NarrativeHints { Tone = "anticipatory", Theme = "arrival", Context = "route_travel", Style = "forward-looking" },
            RouteFilter = new PlacementFilter { PlacementType = PlacementType.Route, SegmentIndex = 3 },
            LocationFilter = new PlacementFilter { PlacementType = PlacementType.Location, Proximity = PlacementProximity.SameLocation },
            NpcFilter = null
        };

        SituationArchetype arrivalArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceTransaction);
        List<ChoiceTemplate> arrivalChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            arrivalArchetype, arrivalSitId, context);

        SituationTemplate arrivalSituation = new SituationTemplate
        {
            Id = arrivalSitId,
            Name = "Delivery Complete",
            Type = SituationType.Normal,
            SystemType = arrivalArchetype.ChallengeType,
            NarrativeTemplate = null,
            ChoiceTemplates = arrivalChoices,
            Priority = 60,
            NarrativeHints = new NarrativeHints { Tone = "conclusive", Theme = "completion", Context = "delivery_success", Style = "satisfying" },
            LocationFilter = new PlacementFilter { PlacementType = PlacementType.Location, Proximity = PlacementProximity.RouteDestination, Purpose = LocationPurpose.Commerce },
            NpcFilter = new PlacementFilter { PlacementType = PlacementType.NPC, Profession = Professions.Merchant },
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = obstacle1SitId
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { obstacle1Situation, obstacle2Situation, obstacle3Situation, approachSituation, arrivalSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// INN_LODGING - How journeys pause
    /// Pattern: Linear (negotiate → rest → depart)
    /// </summary>
    public static SceneArchetypeDefinition GenerateInnLodging(GenerationContext context)
    {
        string sceneId = "inn_lodging";
        string negotiateSitId = $"{sceneId}_negotiate";
        string restSitId = $"{sceneId}_rest";
        string departSitId = $"{sceneId}_depart";

        DependentResourceCatalog.DependentResources resources =
            DependentResourceCatalog.GenerateForActivity(ServiceActivityType.Lodging);
        PlacementFilter serviceLocationFilter = resources.LocationFilter;

        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceNegotiation);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            negotiateArchetype, negotiateSitId, context);

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = negotiateSitId,
            Name = "Secure Lodging",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social,
            NarrativeTemplate = null,
            ChoiceTemplates = negotiateChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints { Tone = "welcoming", Theme = "first_impressions", Context = "securing_lodging", Style = "approachable" },
            LocationFilter = new PlacementFilter { PlacementType = PlacementType.Location, Purpose = LocationPurpose.Commerce },
            NpcFilter = new PlacementFilter { PlacementType = PlacementType.NPC, Profession = Professions.Innkeeper },
            RouteFilter = null
        };

        SituationArchetype restArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceExecutionRest);
        List<ChoiceTemplate> restChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            restArchetype, restSitId, context);

        SituationTemplate restSituation = new SituationTemplate
        {
            Id = restSitId,
            Name = "Evening in Room",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Mental,
            NarrativeTemplate = null,
            ChoiceTemplates = restChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints { Tone = "contemplative", Theme = "preparation", Context = "evening_choices", Style = "introspective" },
            LocationFilter = serviceLocationFilter,
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype departArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.ServiceDeparture);
        List<ChoiceTemplate> departChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            departArchetype, departSitId, context);

        SituationTemplate departSituation = new SituationTemplate
        {
            Id = departSitId,
            Name = "Morning Departure",
            Type = SituationType.Normal,
            SystemType = TacticalSystemType.Social,
            NarrativeTemplate = null,
            ChoiceTemplates = departChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints { Tone = "forward-looking", Theme = "departure", Context = "morning_departure", Style = "optimistic" },
            LocationFilter = serviceLocationFilter,
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = negotiateSitId
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { negotiateSituation, restSituation, departSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// CONSEQUENCE_REFLECTION - How journeys transform
    /// Pattern: Standalone
    /// </summary>
    public static SceneArchetypeDefinition GenerateConsequenceReflection(GenerationContext context)
    {
        string situationId = "consequence_reflection";

        SituationArchetype reflectionArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> reflectionChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            reflectionArchetype, situationId, context);

        SituationTemplate reflectionSituation = new SituationTemplate
        {
            Id = situationId,
            Name = "Morning Reflection",
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = reflectionChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints { Tone = "regretful", Theme = "consequence", Context = "morning_after", Style = "somber" },
            LocationFilter = new PlacementFilter { PlacementType = PlacementType.Location },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Standalone,
            InitialSituationTemplateId = situationId
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { reflectionSituation },
            SpawnRules = spawnRules
        };
    }
}
