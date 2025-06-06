public partial class EncounterChoiceTooltipBase
{
    protected class EffectItem
    {
        public int Value { get; set; }
        public string Description { get; set; }
        public bool IsTokenEffect { get; set; }
        public bool IsProgressEffect { get; set; }
        public bool IsFocusEffect { get; set; }
    }
}