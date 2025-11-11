
/// <summary>
/// Result of analyzing hex segment terrain and danger
/// Replaces value tuple (TerrainType dominantTerrain, int averageDanger)
/// </summary>
public class SegmentAnalysisResult
{
    public TerrainType DominantTerrain { get; init; }
    public int AverageDanger { get; init; }

    public SegmentAnalysisResult(TerrainType dominantTerrain, int averageDanger)
    {
        DominantTerrain = dominantTerrain;
        AverageDanger = averageDanger;
    }
}
