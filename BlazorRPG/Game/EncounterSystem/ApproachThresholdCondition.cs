namespace BlazorRPG.Game.EncounterManager
{
    public class ApproachThresholdCondition : ActivationCondition
    {
        public ApproachTags ApproachTag { get; }
        public int Threshold { get; }

        public ApproachThresholdCondition(ApproachTags approachTag, int threshold)
        {
            ApproachTag = approachTag;
            Threshold = threshold;
        }

        public override bool IsActive(BaseTagSystem tagSystem)
        {
            return tagSystem.GetApproachTagValue(ApproachTag) >= Threshold;
        }

        public override string GetDescription()
        {
            return $"Requires {ApproachTag} {Threshold}+";
        }
    }
}