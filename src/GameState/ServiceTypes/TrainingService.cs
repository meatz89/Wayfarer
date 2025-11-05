using Wayfarer.Content.Catalogues;
using Wayfarer.GameState.Enums;

namespace Wayfarer.GameState;

public class TrainingService : ServiceType
{
    public override string Id => "training";

    public override string DisplayName => "Training";

    public override ChoiceReward GenerateRewards(int tier)
    {
        return new ChoiceReward
        {
            TimeSegments = 4
        };
    }

    public override SceneArchetypeDefinition GenerateMultiSituationArc(
        int tier,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        return GenerateSingleSituation(tier, contextNPC, contextLocation, contextPlayer);
    }

    public override SceneArchetypeDefinition GenerateSingleSituation(
        int tier,
        NPC contextNPC,
        Location contextLocation,
        Player contextPlayer)
    {
        string situationId = $"{Id}_situation";

        SituationArchetype archetype = SituationArchetypeCatalog.GetArchetype(Id);
        List<ChoiceTemplate> choices = SituationArchetypeCatalog.GenerateChoiceTemplates(archetype, situationId);

        SituationTemplate situation = new SituationTemplate
        {
            Id = situationId,
            Name = DisplayName,
            Type = SituationType.Normal,
            NarrativeTemplate = null,
            ChoiceTemplates = choices,
            Priority = 100,
            NarrativeHints = new NarrativeHints
            {
                Tone = "instructive",
                Theme = "skill_development",
                Context = Id
            },
            RequiredLocationId = contextNPC?.Location?.Id,
            RequiredNpcId = contextNPC?.ID
        };

        SituationSpawnRules spawnRules = new SituationSpawnRules
        {
            Pattern = SpawnPattern.Standalone,
            InitialSituationId = situationId,
            Transitions = new List<SituationTransition>()
        };

        return new SceneArchetypeDefinition
        {
            SituationTemplates = new List<SituationTemplate> { situation },
            SpawnRules = spawnRules
        };
    }
}
