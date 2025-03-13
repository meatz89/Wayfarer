namespace BlazorRPG.Game.EncounterManager
{
    /// <summary>
    /// Represents a tag modification to be applied when a choice is made
    /// </summary>
    public class EncounterStateModification
    {
        public enum TagTypes { Approach, Focus }

        public TagTypes Type { get; }
        public object Tag { get; } // Either ApproachTags or FocusTags
        public int Delta { get; }

        private EncounterStateModification(TagTypes type, object tag, int delta)
        {
            Type = type;
            Tag = tag;
            Delta = delta;
        }

        public static EncounterStateModification ForApproach(EncounterStateTags tag, int delta)
        {
            return new EncounterStateModification(TagTypes.Approach, tag, delta);
        }

        public static EncounterStateModification ForFocus(FocusTags tag, int delta)
        {
            return new EncounterStateModification(TagTypes.Focus, tag, delta);
        }

        public override string ToString()
        {
            return $"{Type} ({Delta} to {Tag})";
        }
    }
}