public class ObservationCardDetail
{
    public ObservationCard Card { get; init; }
    public int HoursRemaining { get; init; }

    public ObservationCardDetail(ObservationCard card, int hoursRemaining)
    {
        Card = card;
        HoursRemaining = hoursRemaining;
    }
}