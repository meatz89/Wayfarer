public class DragDropService
{
    private ActionCardDefinition _draggedCard;

    public ActionCardDefinition DraggedCard
    {
        get => _draggedCard;
        set
        {
            if (_draggedCard != value)
            {
                _draggedCard = value;
                OnDragStateChanged?.Invoke();
            }
        }
    }

    public event Action OnDragStateChanged;

    public bool IsValidDropTarget(CardTypes requiredCardType)
    {
        if (DraggedCard == null)
            return false;

        return DraggedCard.Type == requiredCardType && !DraggedCard.IsExhausted;
    }

    public void Reset()
    {
        if (DraggedCard != null)
        {
            DraggedCard = null;
            // OnDragStateChanged is already called by the setter
        }
    }
}