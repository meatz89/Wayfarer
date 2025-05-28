public record UserActionOption
(
    string ActionName,
    bool IsDisabled,
    LocationAction locationAction,
    string LocationId,
    string LocationSpot,
    string Character,
    int LocationDifficulty,
    string DisabledReason,
    string ApproachId
)
{
    public SkillCard SelectedCard { get; set; }
}
