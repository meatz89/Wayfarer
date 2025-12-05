/// <summary>
/// Encounter archetypes - meeting people and facing situations during the Frieren journey
/// These archetypes involve INTERPERSONAL DYNAMICS and CHALLENGES:
/// - MeetOrderMember: Making new contacts
/// - ConfrontAntagonist: Facing opposition
/// - UrgentDecision: Time-pressured choices
/// - MoralCrossroads: Ethical dilemmas
/// - QuietReflection: Solo contemplation (recovery)
/// - CasualEncounter: Friendly interactions (recovery)
/// - ScholarlyPursuit: Study and learning (recovery)
///
/// PARSE-TIME ONLY - Called from SceneArchetypeCatalog.Generate()
/// RhythmPattern determines choice generation, not archetype category
/// </summary>
internal static class EncounterArchetypes
{
    /// <summary>
    /// MEET_ORDER_MEMBER - Making new contacts
    /// Pattern: Linear (contact → negotiate → revelation)
    /// </summary>
    public static SceneArchetypeDefinition GenerateMeetOrderMember(GenerationContext context)
    {
        string sceneId = "meet_order_member";

        SituationArchetype contactArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.SocialManeuvering);
        List<ChoiceTemplate> contactChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            contactArchetype, $"{sceneId}_contact", context);

        SituationTemplate contactSituation = new SituationTemplate
        {
            Id = $"{sceneId}_contact",
            Type = SituationType.Normal,
            ChoiceTemplates = contactChoices,
            Priority = 100,
            NarrativeHints = new NarrativeHints { Tone = "cautious", Theme = "first_contact", Context = "meeting_order_member", Style = "mysterious" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype negotiateArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Negotiation);
        List<ChoiceTemplate> negotiateChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            negotiateArchetype, $"{sceneId}_negotiate", context);

        SituationTemplate negotiateSituation = new SituationTemplate
        {
            Id = $"{sceneId}_negotiate",
            Type = SituationType.Normal,
            ChoiceTemplates = negotiateChoices,
            Priority = 90,
            NarrativeHints = new NarrativeHints { Tone = "tense", Theme = "information_exchange", Context = "negotiating_knowledge", Style = "strategic" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype revelationArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Investigation);
        List<ChoiceTemplate> revelationChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            revelationArchetype, $"{sceneId}_revelation", context);

        SituationTemplate revelationSituation = new SituationTemplate
        {
            Id = $"{sceneId}_revelation",
            Type = SituationType.Normal,
            ChoiceTemplates = revelationChoices,
            Priority = 80,
            NarrativeHints = new NarrativeHints { Tone = "revelatory", Theme = "knowledge_gained", Context = "order_secret_revealed", Style = "impactful" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_contact"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { contactSituation, negotiateSituation, revelationSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// CONFRONT_ANTAGONIST - Facing opposition
    /// Pattern: Linear (accuse → resolve)
    /// </summary>
    public static SceneArchetypeDefinition GenerateConfrontAntagonist(GenerationContext context)
    {
        string sceneId = "confront_antagonist";

        SituationArchetype accuseArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Confrontation);
        List<ChoiceTemplate> accuseChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            accuseArchetype, $"{sceneId}_accuse", context);

        SituationTemplate accuseSituation = new SituationTemplate
        {
            Id = $"{sceneId}_accuse",
            Type = SituationType.Normal,
            ChoiceTemplates = accuseChoices,
            Priority = 100,
            Intensity = ArchetypeIntensity.Demanding,
            NarrativeHints = new NarrativeHints { Tone = "confrontational", Theme = "accusation", Context = "dramatic_confrontation", Style = "intense" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype resolveArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> resolveChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            resolveArchetype, $"{sceneId}_resolve", context);

        SituationTemplate resolveSituation = new SituationTemplate
        {
            Id = $"{sceneId}_resolve",
            Type = SituationType.Normal,
            ChoiceTemplates = resolveChoices,
            Priority = 90,
            Intensity = ArchetypeIntensity.Demanding,
            NarrativeHints = new NarrativeHints { Tone = "decisive", Theme = "resolution", Context = "confrontation_outcome", Style = "climactic" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_accuse"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { accuseSituation, resolveSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// URGENT_DECISION - Time-pressured choices
    /// Pattern: Linear (crisis → decision)
    /// </summary>
    public static SceneArchetypeDefinition GenerateUrgentDecision(GenerationContext context)
    {
        string sceneId = "urgent_decision";

        SituationArchetype crisisArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> crisisChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            crisisArchetype, $"{sceneId}_crisis", context);

        SituationTemplate crisisSituation = new SituationTemplate
        {
            Id = $"{sceneId}_crisis",
            Type = SituationType.Crisis,
            ChoiceTemplates = crisisChoices,
            Priority = 100,
            Intensity = ArchetypeIntensity.Demanding,
            NarrativeHints = new NarrativeHints { Tone = "urgent", Theme = "crisis", Context = "emergency", Style = "intense" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype decisionArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> decisionChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            decisionArchetype, $"{sceneId}_decision", context);

        SituationTemplate decisionSituation = new SituationTemplate
        {
            Id = $"{sceneId}_decision",
            Type = SituationType.Crisis,
            ChoiceTemplates = decisionChoices,
            Priority = 90,
            Intensity = ArchetypeIntensity.Demanding,
            NarrativeHints = new NarrativeHints { Tone = "desperate", Theme = "decision", Context = "urgent_choice", Style = "high_stakes" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_crisis"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { crisisSituation, decisionSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// MORAL_CROSSROADS - Ethical dilemmas
    /// Pattern: Linear (dilemma → choice → consequence)
    /// </summary>
    public static SceneArchetypeDefinition GenerateMoralCrossroads(GenerationContext context)
    {
        string sceneId = "moral_crossroads";

        SituationArchetype dilemmaArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.SocialManeuvering);
        List<ChoiceTemplate> dilemmaChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            dilemmaArchetype, $"{sceneId}_dilemma", context);

        SituationTemplate dilemmaSituation = new SituationTemplate
        {
            Id = $"{sceneId}_dilemma",
            Type = SituationType.Normal,
            ChoiceTemplates = dilemmaChoices,
            Priority = 100,
            Intensity = ArchetypeIntensity.Demanding,
            NarrativeHints = new NarrativeHints { Tone = "conflicted", Theme = "moral_dilemma", Context = "ethical_choice", Style = "thoughtful" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype choiceArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.Crisis);
        List<ChoiceTemplate> choiceChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            choiceArchetype, $"{sceneId}_choice", context);

        SituationTemplate choiceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_choice",
            Type = SituationType.Normal,
            ChoiceTemplates = choiceChoices,
            Priority = 90,
            Intensity = ArchetypeIntensity.Demanding,
            NarrativeHints = new NarrativeHints { Tone = "weighty", Theme = "moral_stance", Context = "defining_moment", Style = "impactful" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype consequenceArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.SocialManeuvering);
        List<ChoiceTemplate> consequenceChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            consequenceArchetype, $"{sceneId}_consequence", context);

        SituationTemplate consequenceSituation = new SituationTemplate
        {
            Id = $"{sceneId}_consequence",
            Type = SituationType.Normal,
            ChoiceTemplates = consequenceChoices,
            Priority = 80,
            Intensity = ArchetypeIntensity.Demanding,
            NarrativeHints = new NarrativeHints { Tone = "reflective", Theme = "consequence", Context = "moral_aftermath", Style = "somber" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_dilemma"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { dilemmaSituation, choiceSituation, consequenceSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// QUIET_REFLECTION - Solo contemplation (Recovery intensity)
    /// Pattern: Linear (settle → reflect)
    /// Building rhythm: All choices grant stats, no requirements
    /// </summary>
    public static SceneArchetypeDefinition GenerateQuietReflection(GenerationContext context)
    {
        string sceneId = "quiet_reflection";

        SituationArchetype settleArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.MeditationAndReflection);
        List<ChoiceTemplate> settleChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            settleArchetype, $"{sceneId}_settle", context);

        SituationTemplate settleSituation = new SituationTemplate
        {
            Id = $"{sceneId}_settle",
            Type = SituationType.Normal,
            ChoiceTemplates = settleChoices,
            Priority = 100,
            Intensity = ArchetypeIntensity.Recovery,
            NarrativeHints = new NarrativeHints { Tone = "calm", Theme = "settling", Context = "finding_peace", Style = "gentle" },
            LocationFilter = new PlacementFilter { PlacementType = PlacementType.Location, Activity = LocationActivity.Quiet, Safety = LocationSafety.Safe },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype reflectArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.MeditationAndReflection);
        List<ChoiceTemplate> reflectChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            reflectArchetype, $"{sceneId}_reflect", context);

        SituationTemplate reflectSituation = new SituationTemplate
        {
            Id = $"{sceneId}_reflect",
            Type = SituationType.Normal,
            ChoiceTemplates = reflectChoices,
            Priority = 90,
            Intensity = ArchetypeIntensity.Recovery,
            NarrativeHints = new NarrativeHints { Tone = "contemplative", Theme = "self_discovery", Context = "meditation", Style = "introspective" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_settle"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { settleSituation, reflectSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// CASUAL_ENCOUNTER - Friendly interactions (Recovery intensity)
    /// Pattern: Linear (encounter → converse)
    /// Building rhythm: Social stat grants, relationship building
    /// </summary>
    public static SceneArchetypeDefinition GenerateCasualEncounter(GenerationContext context)
    {
        string sceneId = "casual_encounter";

        SituationArchetype encounterArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.LocalConversation);
        List<ChoiceTemplate> encounterChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            encounterArchetype, $"{sceneId}_encounter", context);

        SituationTemplate encounterSituation = new SituationTemplate
        {
            Id = $"{sceneId}_encounter",
            Type = SituationType.Normal,
            ChoiceTemplates = encounterChoices,
            Priority = 100,
            Intensity = ArchetypeIntensity.Recovery,
            NarrativeHints = new NarrativeHints { Tone = "friendly", Theme = "meeting", Context = "casual_hello", Style = "warm" },
            LocationFilter = new PlacementFilter { PlacementType = PlacementType.Location, Activity = LocationActivity.Moderate, Safety = LocationSafety.Safe },
            NpcFilter = new PlacementFilter { PlacementType = PlacementType.NPC },
            RouteFilter = null
        };

        SituationArchetype converseArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.LocalConversation);
        List<ChoiceTemplate> converseChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            converseArchetype, $"{sceneId}_converse", context);

        SituationTemplate converseSituation = new SituationTemplate
        {
            Id = $"{sceneId}_converse",
            Type = SituationType.Normal,
            ChoiceTemplates = converseChoices,
            Priority = 90,
            Intensity = ArchetypeIntensity.Recovery,
            NarrativeHints = new NarrativeHints { Tone = "pleasant", Theme = "connection", Context = "friendly_chat", Style = "relaxed" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_encounter"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { encounterSituation, converseSituation },
            SpawnRules = spawnRules
        };
    }

    /// <summary>
    /// SCHOLARLY_PURSUIT - Study and learning (Recovery intensity)
    /// Pattern: Linear (browse → study)
    /// Building rhythm: Insight stat grants
    /// </summary>
    public static SceneArchetypeDefinition GenerateScholarlyPursuit(GenerationContext context)
    {
        string sceneId = "scholarly_pursuit";

        SituationArchetype browseArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.StudyInLibrary);
        List<ChoiceTemplate> browseChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            browseArchetype, $"{sceneId}_browse", context);

        SituationTemplate browseSituation = new SituationTemplate
        {
            Id = $"{sceneId}_browse",
            Type = SituationType.Normal,
            ChoiceTemplates = browseChoices,
            Priority = 100,
            Intensity = ArchetypeIntensity.Recovery,
            NarrativeHints = new NarrativeHints { Tone = "curious", Theme = "exploration", Context = "browsing_knowledge", Style = "intellectual" },
            LocationFilter = new PlacementFilter { PlacementType = PlacementType.Location, Purpose = LocationPurpose.Learning, Activity = LocationActivity.Quiet },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationArchetype studyArchetype = SituationArchetypeCatalog.GetArchetype(SituationArchetypeType.StudyInLibrary);
        List<ChoiceTemplate> studyChoices = SituationArchetypeCatalog.GenerateChoiceTemplatesWithContext(
            studyArchetype, $"{sceneId}_study", context);

        SituationTemplate studySituation = new SituationTemplate
        {
            Id = $"{sceneId}_study",
            Type = SituationType.Normal,
            ChoiceTemplates = studyChoices,
            Priority = 90,
            Intensity = ArchetypeIntensity.Recovery,
            NarrativeHints = new NarrativeHints { Tone = "focused", Theme = "learning", Context = "deep_study", Style = "scholarly" },
            LocationFilter = new PlacementFilter { Proximity = PlacementProximity.SameLocation },
            NpcFilter = null,
            RouteFilter = null
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Linear,
            InitialSituationTemplateId = $"{sceneId}_browse"
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { browseSituation, studySituation },
            SpawnRules = spawnRules
        };
    }
}
