public class CardHighlightService
{
    private bool isHighlightModeActive = false;
    private CardTypes _targetCardType = CardTypes.Physical;

    public bool IsHighlightModeActive => isHighlightModeActive;
    public CardTypes TargetCardType => _targetCardType;

    public event Action OnHighlightModeChanged;

    public HighlightMode _highlightMode = HighlightMode.OnlyExhausted;

    public void ActivateHighlightMode(CardTypes cardType, HighlightMode highlightMode)
    {
        _highlightMode = highlightMode;
        _targetCardType = cardType;
        isHighlightModeActive = true;
        OnHighlightModeChanged?.Invoke();
    }

    public void DeactivateHighlightMode()
    {
        isHighlightModeActive = false;
        OnHighlightModeChanged?.Invoke();
    }

    public bool ShouldHighlightCard(ActionCardDefinition card)
    {
        if (!isHighlightModeActive)
            return false;

        return card.Type == _targetCardType && 
            (_highlightMode == HighlightMode.OnlyExhausted ? card.IsExhausted : !card.IsExhausted);
    }
}

public enum HighlightMode
{
    OnlyExhausted,
    OnlyAvailable, // Not exhausted
}