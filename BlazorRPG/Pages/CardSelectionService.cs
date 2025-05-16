public class CardSelectionService
{
    private ActionCardDefinition _selectedCard;

    public ActionCardDefinition SelectedCard
    {
        get => _selectedCard;
        set
        {
            if (_selectedCard != value)
            {
                _selectedCard = value;
                OnStateChanged?.Invoke();
            }
        }
    }

    public event Action OnStateChanged;

    public bool IsValidDropTarget(CardTypes requiredCardType)
    {
        if (SelectedCard == null)
            return false;

        return SelectedCard.Type == requiredCardType && !SelectedCard.IsExhausted;
    }

    public void Reset()
    {
        if (SelectedCard != null)
        {
            SelectedCard = null;
            // OnDragStateChanged is already called by the setter
        }
    }
}