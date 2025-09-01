
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
            
        int totalWeight = 0;
        foreach (var c in _selectedCards)
        {
            totalWeight += c.GetEffectiveWeight(_currentState);
        }
        
        var cardNames = new List<string>();
        foreach (var c in _selectedCards)
        {
            cardNames.Add(c.Name);
        }
        
        return $"Playing {string.Join(", ", cardNames)} (weight: {totalWeight})";
    }
    
    public HashSet<CardInstance> GetSelectedCards()
    {
        return new HashSet<CardInstance>(_selectedCards);
    }
}