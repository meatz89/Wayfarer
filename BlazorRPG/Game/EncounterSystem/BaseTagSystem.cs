namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Manages the base approach and focus tags (0-5 scale)
    /// </summary>
    public class BaseTagSystem
    {
        private Dictionary<EncounterStateTags, int> _encounterStateTags;
        private Dictionary<ApproachTags, int> _approachTags;
        private Dictionary<FocusTags, int> _focusTags;

        public const int MinTagValue = 0;
        public const int MaxTagValue = 5;

        public BaseTagSystem()
        {
            _encounterStateTags = new Dictionary<EncounterStateTags, int>();
            _approachTags = new Dictionary<ApproachTags, int>();
            _focusTags = new Dictionary<FocusTags, int>();

            // Initialize all tags to 0
            foreach (EncounterStateTags tag in Enum.GetValues(typeof(EncounterStateTags)))
                _encounterStateTags[tag] = 0;

            foreach (ApproachTags tag in Enum.GetValues(typeof(ApproachTags)))
                _approachTags[tag] = 0;

            foreach (FocusTags tag in Enum.GetValues(typeof(FocusTags)))
                _focusTags[tag] = 0;
        }

        public int GetEncounterStateTagValue(EncounterStateTags tag) => _encounterStateTags[tag];
        public int GetApproachTagValue(ApproachTags tag) => _approachTags[tag];
        public int GetFocusTagValue(FocusTags tag) => _focusTags[tag];

        public void ModifyEncounterStateTag(EncounterStateTags tag, int delta)
        {
            int newValue = _encounterStateTags[tag] + delta;
            _encounterStateTags[tag] = Math.Clamp(newValue, MinTagValue, MaxTagValue);
        }
        public void ModifyApproachTag(ApproachTags tag, int delta)
        {
            int newValue = _approachTags[tag] + delta;
            _approachTags[tag] = Math.Clamp(newValue, MinTagValue, MaxTagValue);
        }

        public void ModifyFocusTag(FocusTags tag, int delta)
        {
            int newValue = _focusTags[tag] + delta;
            _focusTags[tag] = Math.Clamp(newValue, MinTagValue, MaxTagValue);
        }

        public Dictionary<EncounterStateTags, int> GetAllEncounterStateTags() => _encounterStateTags;
        public Dictionary<ApproachTags, int> GetAllApproachTags() => _approachTags;
        public Dictionary<FocusTags, int> GetAllFocusTags() => _focusTags;

        public BaseTagSystem Clone()
        {
            BaseTagSystem clone = new BaseTagSystem();

            foreach (EncounterStateTags tag in Enum.GetValues(typeof(EncounterStateTags)))
                clone.SetEncounterStateTagValue(tag, GetEncounterStateTagValue(tag));

            foreach (ApproachTags tag in Enum.GetValues(typeof(ApproachTags)))
                clone.SetApproachTagValue(tag, GetApproachTagValue(tag));

            foreach (FocusTags tag in Enum.GetValues(typeof(FocusTags)))
                clone.SetFocusTagValue(tag, GetFocusTagValue(tag));

            return clone;
        }

        // Set tag values directly (for clone initialization)
        public void SetEncounterStateTagValue(EncounterStateTags tag, int value)
        {
            _encounterStateTags[tag] = Math.Clamp(value, MinTagValue, MaxTagValue);
        }

        private void SetApproachTagValue(ApproachTags tag, int value)
        {
            _approachTags[tag] = Math.Clamp(value, MinTagValue, MaxTagValue);
        }

        public void SetFocusTagValue(FocusTags tag, int value)
        {
            _focusTags[tag] = Math.Clamp(value, MinTagValue, MaxTagValue);
        }

    }
}