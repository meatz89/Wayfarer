using System.Text.Json;

/// <summary>
/// Repository of all available choices in the game
/// </summary>
public class NarrativeChoiceRepository
{
    private readonly List<NarrativeChoice> narrativeChoices = new();

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
            Skills.Strength,
            1,
            1,
            new List<string>() { "Physical", "Stance" }
        ));

        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildChoice(
            "intimidate",
            "Intimidate",
            "You intimidate your opponent, causing them to hesitate.",
            Skills.Strength,
            1,
            1,
            new List<string>() { "Physical", "Intimidate" }
        ));

        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildChoice(
            "taunt",
            "Taunt",
            "You taunt your opponent, drawing their attention and ire.",
            Skills.Strength,
            1,
            1,
            new List<string>() { "Physical", "Taunt" }
        ));

        narrativeChoices.Add(NarrativeChoiceDefinitionFactory.BuildChoice(
            "distract",
            "Distract",
            "You distract your opponent, causing them to lose focus.",
            Skills.Strength,
            1,
            1,
            new List<string>() { "Physical", "Distract" }
        ));
    }

    public List<NarrativeChoice> GetAll()
    {
        return narrativeChoices.ToList();
    }

    public List<NarrativeChoice> GetForEncounter(EncounterState state)
    {
        return narrativeChoices.ToList();
    }
}