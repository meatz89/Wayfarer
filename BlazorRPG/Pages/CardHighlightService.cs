public class CardHighlightService
{
    private bool isHighlightModeActive = false;
    private SkillCategories _targetCardType = SkillCategories.Physical;

    public bool IsHighlightModeActive
    {
        get
        {
            return isHighlightModeActive;
        }
    }

    public SkillCategories TargetCardType
    {
        get
        {
            return _targetCardType;
        }
    }

    public event Action OnHighlightModeChanged;

    public HighlightMode _highlightMode = HighlightMode.Refresh;

    public void ActivateHighlightMode(SkillCategories cardType, HighlightMode highlightMode)
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

    public bool ShouldHighlightCard(SkillCard card)
    {
        if (!isHighlightModeActive)
            return false;

        return card.Category == _targetCardType &&
            (_highlightMode == HighlightMode.Refresh ? card.IsExhausted : !card.IsExhausted);
    }
}

public enum HighlightMode
{
    Highlight = 0,
    Refresh = 1,
}