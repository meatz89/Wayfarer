using System.Text.Json;

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
        cards.Add(CardDefinitionFactory.BuildCard(
            "imposing_stance",
            "Imposing Stance",
            "You adopt a powerful stance, projecting physical dominance and control.",
            CardTypes.Physical,
            Skills.Strength,
            1,
            1,
            new List<string>() { "Physical", "Stance" }
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "intimidate",
            "Intimidate",
            "You intimidate your opponent, causing them to hesitate.",
            CardTypes.Physical,
            Skills.Strength,
            1,
            1,
            new List<string>() { "Physical", "Intimidate" }
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "taunt",
            "Taunt",
            "You taunt your opponent, drawing their attention and ire.",
            CardTypes.Physical,
            Skills.Strength,
            1,
            1,
            new List<string>() { "Physical", "Taunt" }
        ));

        cards.Add(CardDefinitionFactory.BuildCard(
            "distract",
            "Distract",
            "You distract your opponent, causing them to lose focus.",
            CardTypes.Physical,
            Skills.Strength,
            1,
            1,
            new List<string>() { "Physical", "Distract" }
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
}