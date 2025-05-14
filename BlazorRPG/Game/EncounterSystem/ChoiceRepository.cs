/// <summary>
/// Repository of all available choices in the game
/// </summary>
public class ChoiceRepository
{
    private readonly List<CardDefinition> cards = new();

    public ChoiceRepository()
    {
        InitializeChoices();
    }

    private void InitializeChoices()
    {
        // KNIGHT CARDS (Dominance)

        // Tier 1
        cards.Add(CardDefinitionFactory.BuildCard(
            "imposing_stance",
            "Imposing Stance",
            "You adopt a powerful stance, projecting physical dominance and control.",
            1,
            EffectTypes.Momentum, 2,
            new EnvironmentalPropertyEffect(new() { Atmosphere.Rough }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement>()
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "show_of_force",
            "Show of Force",
            "You demonstrate your strength, intimidating others while establishing control over resources.",
            1,
            EffectTypes.Momentum, 2,
            new EnvironmentalPropertyEffect(new() { Population.Crowded }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement>()
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "stand_ground",
            "Stand Ground",
            "You plant your feet firmly, refusing to yield to pressure or intimidation.",
            1,
            EffectTypes.Pressure, 1,
            new EnvironmentalPropertyEffect(new() { Physical.Confined }, StrategicTagEffectType.DecreasePressure),
            new List<SkillRequirement>()
        ));

        // Tier 2
        cards.Add(CardDefinitionFactory.BuildCard(
            "battlefield_command",
            "Battlefield Command",
            "You take charge of the situation, directing others with firm authority.",
            2,
            EffectTypes.Momentum, 3,
            new EnvironmentalPropertyEffect(new() { Population.Crowded }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement> { new SkillRequirement(SkillTypes.Endurance, 1) }
        ));

        // Diplomat CARDS (Rapport)

        // Tier 1
        cards.Add(CardDefinitionFactory.BuildCard(
            "charming_words",
            "Charming Words",
            "You speak with genuine warmth and interest, making others feel valued and understood.",
            1,
            EffectTypes.Momentum, 2,
            new EnvironmentalPropertyEffect(new() { Population.Crowded }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement>()
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "smooth_tensions",
            "Smooth Tensions",
            "You defuse rising tension with well-chosen words and empathetic responses.",
            1,
            EffectTypes.Pressure, 1,
            new EnvironmentalPropertyEffect(new() { Atmosphere.Tense }, StrategicTagEffectType.DecreasePressure),
            new List<SkillRequirement>()
        ));

        // Tier 2
        cards.Add(CardDefinitionFactory.BuildCard(
            "social_leverage",
            "Rapport Leverage",
            "You skillfully use your connections and charm to gain access to valuable resources.",
            2,
            EffectTypes.Momentum, 3,
            new EnvironmentalPropertyEffect(new() { Atmosphere.Tense }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement> { new SkillRequirement(SkillTypes.Diplomacy, 1) }
        ));

        // SAGE CARDS (Analysis)

        // Tier 1
        cards.Add(CardDefinitionFactory.BuildCard(
            "critical_insight",
            "Critical Insight",
            "You quickly identify key patterns and connections others would miss.",
            1,
            EffectTypes.Momentum, 2,
            new EnvironmentalPropertyEffect(new() { Population.Quiet }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement>()
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "careful_consideration",
            "Careful Consideration",
            "You take time to thoroughly evaluate information, avoiding hasty conclusions.",
            1,
            EffectTypes.Pressure, 1,
            new EnvironmentalPropertyEffect(new() { Population.Scholarly }, StrategicTagEffectType.DecreasePressure),
            new List<SkillRequirement>()
        ));

        // Tier 2
        cards.Add(CardDefinitionFactory.BuildCard(
            "knowledge_application",
            "Knowledge Application",
            "You apply theoretical knowledge to practical problems with remarkable efficiency.",
            2,
            EffectTypes.Momentum, 3,
            new EnvironmentalPropertyEffect(new() { Physical.Hazardous }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement> { new SkillRequirement(SkillTypes.Insight, 1) }
        ));

        // FORESTER CARDS (Precision)

        // Tier 1
        cards.Add(CardDefinitionFactory.BuildCard(
            "precise_strike",
            "Precise Strike",
            "You execute a perfectly timed movement with flawless technique.",
            1,
            EffectTypes.Momentum, 2,
            new EnvironmentalPropertyEffect(new() { Illumination.Bright }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement>()
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "focused_observation",
            "Focused Observation",
            "You carefully examine your surroundings, noticing subtle details others would miss.",
            1,
            EffectTypes.Momentum, 2,
            new EnvironmentalPropertyEffect(new() { Physical.Expansive }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement>()
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "measured_movement",
            "Measured Movement",
            "You move with deliberate control, minimizing strain and risk of injury.",
            1,
            EffectTypes.Pressure, 1,
            new EnvironmentalPropertyEffect(new() { Physical.Hazardous }, StrategicTagEffectType.DecreasePressure),
            new List<SkillRequirement>()
        ));

        // Tier 2
        cards.Add(CardDefinitionFactory.BuildCard(
            "environmental_advantage",
            "Environmental Advantage",
            "You leverage precise knowledge of terrain features to gain tactical superiority.",
            2,
            EffectTypes.Momentum, 3,
            new EnvironmentalPropertyEffect(new() { Physical.Expansive }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement> { new SkillRequirement(SkillTypes.Charm, 1) }
        ));

        // Rogue CARDS (Concealment)

        // Tier 1
        cards.Add(CardDefinitionFactory.BuildCard(
            "hidden_advantage",
            "Hidden Advantage",
            "You use concealment to secure resources while remaining undetected.",
            1,
            EffectTypes.Momentum, 2,
            new EnvironmentalPropertyEffect(new() { Illumination.Roguey }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement>()
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "stealth_approach",
            "Precision Approach",
            "You move silently through the environment, avoiding detection entirely.",
            1,
            EffectTypes.Momentum, 2,
            new EnvironmentalPropertyEffect(new() { Illumination.Dark }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement>()
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "fade_away",
            "Fade Away",
            "You slip into the Rogues, removing yourself from immediate danger.",
            1,
            EffectTypes.Pressure, 1,
            new EnvironmentalPropertyEffect(new() { Population.Crowded }, StrategicTagEffectType.DecreasePressure),
            new List<SkillRequirement>()
        ));

        // Tier 2
        cards.Add(CardDefinitionFactory.BuildCard(
            "unseen_observer",
            "Unseen Observer",
            "You gather critical information while remaining completely undetected.",
            2,
            EffectTypes.Momentum, 3,
            new EnvironmentalPropertyEffect(new() { Population.Scholarly }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement> { new SkillRequirement(SkillTypes.Finesse, 1) }
        ));

        // Special cards
        cards.Add(CardDefinitionFactory.BuildCard(
            "pathfinder_eye",
            "Pathfinder's Eye",
            "You catch a subtle clue in the light, sharpening your analytical senses.",
            2,
            EffectTypes.Momentum, 3,
            new EnvironmentalPropertyEffect(new() { Physical.Expansive }, StrategicTagEffectType.IncreaseMomentum),
            new List<SkillRequirement> {
                new SkillRequirement (SkillTypes.Finesse, 2)
            }
        ));
    }

    public List<CardDefinition> GetAll()
    {
        return cards.ToList();
    }

    public List<CardDefinition> GetForEncounter(EncounterState state)
    {
        return cards.ToList();
    }

    public List<CardDefinition> GetCardsForLevel(int level, Professions archetype)
    {
        // Get all cards of appropriate tier
        List<CardDefinition> tierCards = cards.Where(x =>
        {
            return (int)x.Tier <= level;
        }).ToList();

        // Sort cards so that the archetype's preferred approach cards come first
        return tierCards.ToList();
    }
}