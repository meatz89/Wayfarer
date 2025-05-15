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