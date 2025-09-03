
// Card selection manager for UI
public class CardSelectionManager
{
    private readonly EmotionalState _currentState;
    private readonly HashSet<CardInstance> _selectedCards = new();

    public CardSelectionManager(EmotionalState currentState)
    {
        _currentState = currentState;
    }

    public void ToggleCard(CardInstance card)
    {
        if (_selectedCards.Contains(card))
            _selectedCards.Remove(card);
        else
            _selectedCards.Add(card);
    }

    public string GetSelectionDescription()
    {
        if (!_selectedCards.Any())
            return "No cards selected";

        int totalFocus = 0;
        foreach (CardInstance c in _selectedCards)
        {
            totalFocus += c.Focus; // No state modifiers in new system
        }

        List<string> cardNames = new List<string>();
        foreach (CardInstance c in _selectedCards)
        {
            cardNames.Add(c.Name);
        }

        return $"Playing {string.Join(", ", cardNames)} (focus: {totalFocus})";
    }

    public HashSet<CardInstance> GetSelectedCards()
    {
        return new HashSet<CardInstance>(_selectedCards);
    }
}