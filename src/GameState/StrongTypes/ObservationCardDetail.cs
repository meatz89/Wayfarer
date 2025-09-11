public class ObservationCardDetail
{
    public ObservationCard Card { get; init; }
    public int SegmentsRemaining { get; init; }

    public ObservationCardDetail(ObservationCard card, int segmentsRemaining)
    {
        Card = card;
        SegmentsRemaining = segmentsRemaining;
    }
}