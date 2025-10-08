
// Card selection manager for UI
public class CardSelectionManager
{
    private readonly ConnectionState _currentState;
    private readonly HashSet<CardInstance> _selectedCards = new();

    public CardSelectionManager(ConnectionState currentState)
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


}