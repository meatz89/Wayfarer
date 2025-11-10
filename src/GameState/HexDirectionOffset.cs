/// <summary>
/// Offset for hex neighbor directions in axial coordinates
/// Replaces value tuple (int q, int r)
/// </summary>
public class HexDirectionOffset
{
public int Q { get; init; }
public int R { get; init; }

public HexDirectionOffset(int q, int r)
{
    Q = q;
    R = r;
}
}
