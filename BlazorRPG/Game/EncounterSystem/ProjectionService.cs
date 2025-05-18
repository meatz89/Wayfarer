public class ProjectionService
{
    private readonly Encounter _encounterInfo;
    private readonly PlayerState _playerState;

    public ProjectionService(
        Encounter encounterInfo,
        PlayerState playerState)
    {
        _encounterInfo = encounterInfo;
        _playerState = playerState;
    }

    public ChoiceProjection CreateChoiceProjection(
        EncounterOption choice,
        int currentMomentum,
        int currentPressure,
        int currentTurn)
    {
        ChoiceProjection projection = new ChoiceProjection(choice);

        int momentumChange = 0;
        int pressureChange = 0;

        CalculateSkillBonuses(choice, projection, ref momentumChange, ref pressureChange);

        return projection;
    }

    private void CalculateSkillBonuses(EncounterOption choice, ChoiceProjection projection, ref int momentumChange, ref int pressureChange)
    {
    }
}