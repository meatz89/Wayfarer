/// <summary>
/// Manages the base approach and focus tags (0-5 scale)
/// </summary>
public class BaseTagSystem
{
    private Dictionary<ApproachTags, int> _ApproachTags;
    private Dictionary<FocusTags, int> _focusTags;

    public const int MinTagValue = 0;
    public const int MaxTagValue = 5;

    public static BaseTagSystem FromPreviousState(
        Dictionary<ApproachTags, int> previousApproachValues,
        Dictionary<FocusTags, int> previousFocusValues)
    {
        BaseTagSystem fromState = new BaseTagSystem();
        foreach (ApproachTags tag in previousApproachValues.Keys)
            fromState.SetEncounterStateTagValue(tag, previousApproachValues[tag]);

        foreach (FocusTags tag in previousFocusValues.Keys)
            fromState.SetFocusTagValue(tag, previousFocusValues[tag]);

        return fromState;
    }

    public BaseTagSystem()
    {
        _ApproachTags = new Dictionary<ApproachTags, int>();
        _focusTags = new Dictionary<FocusTags, int>();

        foreach (ApproachTags tag in Enum.GetValues(typeof(ApproachTags)))
            SetEncounterStateTagValue(tag, 0);

        foreach (FocusTags tag in Enum.GetValues(typeof(FocusTags)))
            SetFocusTagValue(tag, 0);
    }

    public Dictionary<ApproachTags, int> GetAllApproachTags() => _ApproachTags;
    public Dictionary<FocusTags, int> GetAllFocusTags() => _focusTags;
    public int GetEncounterStateTagValue(ApproachTags tag) => _ApproachTags[tag];
    public int GetFocusTagValue(FocusTags tag) => _focusTags[tag];

    public void ModifyEncounterStateTag(ApproachTags tag, int delta)
    {
        int newValue = _ApproachTags[tag] + delta;
        _ApproachTags[tag] = Math.Clamp(newValue, MinTagValue, MaxTagValue);
    }

    public void ModifyFocusTag(FocusTags tag, int delta)
    {
        int newValue = _focusTags[tag] + delta;
        _focusTags[tag] = Math.Clamp(newValue, MinTagValue, MaxTagValue);
    }

    public BaseTagSystem Clone()
    {
        BaseTagSystem clone = new BaseTagSystem();

        foreach (ApproachTags tag in Enum.GetValues(typeof(ApproachTags)))
            clone.SetEncounterStateTagValue(tag, GetEncounterStateTagValue(tag));

        foreach (FocusTags tag in Enum.GetValues(typeof(FocusTags)))
            clone.SetFocusTagValue(tag, GetFocusTagValue(tag));

        return clone;
    }

    // Set tag values directly (for clone initialization)
    public void SetEncounterStateTagValue(ApproachTags tag, int value)
    {
        _ApproachTags[tag] = Math.Clamp(value, MinTagValue, MaxTagValue);
    }

    public void SetFocusTagValue(FocusTags tag, int value)
    {
        _focusTags[tag] = Math.Clamp(value, MinTagValue, MaxTagValue);
    }
}
