/// <summary>
/// Repository of all available choices in the game
/// </summary>
public class NarrativeChoiceRepository
{
    private readonly List<EncounterOption> narrativeChoices = new();

    public NarrativeChoiceRepository()
    {
        InitializeChoices();
    }

    private void InitializeChoices()
    {
        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildChoice(
            "imposing_stance",
            "Imposing Stance",
            "You adopt a powerful stance, projecting physical dominance and control.",
            SkillTypes.Strength,
            1,
            1,
            new List<string>() { "Physical", "Stance" }
        ));

        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildChoice(
            "intimidate",
            "Intimidate",
            "You intimidate your opponent, causing them to hesitate.",
            SkillTypes.Strength,
            1,
            1,
            new List<string>() { "Physical", "Intimidate" }
        ));

        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildChoice(
            "taunt",
            "Taunt",
            "You taunt your opponent, drawing their attention and ire.",
            SkillTypes.Strength,
            1,
            1,
            new List<string>() { "Physical", "Taunt" }
        ));

        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildChoice(
            "distract",
            "Distract",
            "You distract your opponent, causing them to lose focus.",
            SkillTypes.Strength,
            1,
            1,
            new List<string>() { "Physical", "Distract" }
        ));
    }

    public List<EncounterOption> GetAll()
    {
        return narrativeChoices.ToList();
    }

    public List<EncounterOption> GetForEncounter(EncounterState state)
    {
        return narrativeChoices.ToList();
    }
}