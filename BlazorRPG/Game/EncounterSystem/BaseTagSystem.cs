namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Manages the base approach and focus tags (0-5 scale)
    /// </summary>
    public class BaseTagSystem
    {
        private Dictionary<EncounterStateTags, int> _approachTags;
        private Dictionary<FocusTags, int> _focusTags;

        public const int MinTagValue = 0;
        public const int MaxTagValue = 5;

        public BaseTagSystem()
        {
            _approachTags = new Dictionary<EncounterStateTags, int>();
            _focusTags = new Dictionary<FocusTags, int>();

            // Initialize all tags to 0
            foreach (EncounterStateTags tag in Enum.GetValues(typeof(EncounterStateTags)))
                _approachTags[tag] = 0;

            foreach (FocusTags tag in Enum.GetValues(typeof(FocusTags)))
                _focusTags[tag] = 0;
        }

        public int GetApproachTagValue(EncounterStateTags tag) => _approachTags[tag];
        public int GetFocusTagValue(FocusTags tag) => _focusTags[tag];

        public void ModifyApproachTag(EncounterStateTags tag, int delta)
        {
            int newValue = _approachTags[tag] + delta;
            _approachTags[tag] = Math.Clamp(newValue, MinTagValue, MaxTagValue);
        }

        public void ModifyFocusTag(FocusTags tag, int delta)
        {
            int newValue = _focusTags[tag] + delta;
            _focusTags[tag] = Math.Clamp(newValue, MinTagValue, MaxTagValue);
        }

        public IReadOnlyDictionary<EncounterStateTags, int> GetAllApproachTags() => _approachTags;
        public IReadOnlyDictionary<FocusTags, int> GetAllFocusTags() => _focusTags;

        public BaseTagSystem Clone()
        {
            BaseTagSystem clone = new BaseTagSystem();

            foreach (EncounterStateTags tag in Enum.GetValues(typeof(EncounterStateTags)))
                clone.SetApproachTagValue(tag, GetApproachTagValue(tag));

            foreach (FocusTags tag in Enum.GetValues(typeof(FocusTags)))
                clone.SetFocusTagValue(tag, GetFocusTagValue(tag));

            return clone;
        }

        // Set tag values directly (for clone initialization)
        public void SetApproachTagValue(EncounterStateTags tag, int value)
        {
            _approachTags[tag] = Math.Clamp(value, MinTagValue, MaxTagValue);
        }

        public void SetFocusTagValue(FocusTags tag, int value)
        {
            _focusTags[tag] = Math.Clamp(value, MinTagValue, MaxTagValue);
        }
    }
}