namespace BlazorRPG.Game.EncounterManager
{
    public class FocusThresholdCondition : ActivationCondition
    {
        public FocusTags FocusTag { get; }
        public int Threshold { get; }

        public FocusThresholdCondition(FocusTags focusTag, int threshold)
        {
            FocusTag = focusTag;
            Threshold = threshold;
        }

        public override bool IsActive(BaseTagSystem tagSystem)
        {
            return tagSystem.GetFocusTagValue(FocusTag) >= Threshold;
        }

        public override string GetDescription()
        {
            return $"Requires {FocusTag} {Threshold}+";
        }
    }
}