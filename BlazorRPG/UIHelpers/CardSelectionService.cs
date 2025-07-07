public class CardSelectionService
{
    private SkillCard _selectedCard;

    public SkillCard SelectedCard
    {
        get
        {
            return _selectedCard;
        }

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

    public bool IsValidDropTarget(SkillCategories requiredCardType)
    {
        if (SelectedCard == null)
            return false;

        return SelectedCard.Category == requiredCardType && !SelectedCard.IsExhausted;
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