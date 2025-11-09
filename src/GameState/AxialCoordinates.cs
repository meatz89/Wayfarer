using System;

/// <summary>
/// Axial coordinate system for hexagonal grid
/// Uses Q (column) and R (row) coordinates
/// Provides hex distance calculation, neighbor traversal, and direction determination
///
/// Coordinate System:
///   Q increases horizontally (right = positive)
///   R increases diagonally down-left (down-left = positive)
///   S (derived) = -Q - R (ensures Q + R + S = 0)
///
/// Neighbor Directions (clockwise from top):
///   0: (+0, -1) North
///   1: (+1, -1) Northeast
///   2: (+1, +0) Southeast
///   3: (+0, +1) South
///   4: (-1, +1) Southwest
///   5: (-1, +0) Northwest
/// </summary>
public struct AxialCoordinates : IEquatable<AxialCoordinates>
{
public int Q { get; init; }
public int R { get; init; }

// Derived cube coordinate S (for distance calculations)
public int S => -Q - R;

public AxialCoordinates(int q, int r)
{
    Q = q;
    R = r;
}

// Hex neighbor offsets (axial coordinates)
private static readonly HexDirectionOffset[] NeighborOffsets = new[]
{
    new HexDirectionOffset( 0, -1), // North
    new HexDirectionOffset(+1, -1), // Northeast
    new HexDirectionOffset(+1,  0), // Southeast
    new HexDirectionOffset( 0, +1), // South
    new HexDirectionOffset(-1, +1), // Southwest
    new HexDirectionOffset(-1,  0)  // Northwest
};

/// <summary>
/// Get all 6 neighboring hex coordinates
/// </summary>
public AxialCoordinates[] GetNeighbors()
{
    AxialCoordinates[] neighbors = new AxialCoordinates[6];
    for (int i = 0; i < 6; i++)
    {
        neighbors[i] = new AxialCoordinates(
            Q + NeighborOffsets[i].Q,
            R + NeighborOffsets[i].R
        );
    }
    return neighbors;
}

/// <summary>
/// Get neighbor in specific direction (0-5, clockwise from north)
/// </summary>
public AxialCoordinates GetNeighbor(int direction)
{
    if (direction < 0 || direction > 5)
        throw new ArgumentException($"Direction must be 0-5, got {direction}");

    HexDirectionOffset offset = NeighborOffsets[direction];
    return new AxialCoordinates(Q + offset.Q, R + offset.R);
}

/// <summary>
/// Calculate Manhattan distance between two hexes (minimum number of hex steps)
/// </summary>
public int DistanceTo(AxialCoordinates other)
{
    // Cube distance formula: (|dq| + |dr| + |ds|) / 2
    int dq = Math.Abs(Q - other.Q);
    int dr = Math.Abs(R - other.R);
    int ds = Math.Abs(S - other.S);
    return (dq + dr + ds) / 2;
}

/// <summary>
/// Determine if two hexes are neighbors (distance = 1)
/// </summary>
public bool IsNeighbor(AxialCoordinates other)
{
    return DistanceTo(other) == 1;
}

/// <summary>
/// Get direction from this hex to neighbor (0-5), returns -1 if not neighbors
/// </summary>
public int GetDirectionTo(AxialCoordinates neighbor)
{
    if (!IsNeighbor(neighbor))
        return -1;

    int dq = neighbor.Q - Q;
    int dr = neighbor.R - R;

    for (int i = 0; i < 6; i++)
    {
        if (NeighborOffsets[i].Q == dq && NeighborOffsets[i].R == dr)
            return i;
    }

    return -1;
}

// Equality and hash code for dictionary lookups
public bool Equals(AxialCoordinates other)
{
    return Q == other.Q && R == other.R;
}

public override bool Equals(object obj)
{
    return obj is AxialCoordinates other && Equals(other);
}

public override int GetHashCode()
{
    return HashCode.Combine(Q, R);
}

public static bool operator ==(AxialCoordinates left, AxialCoordinates right)
{
    return left.Equals(right);
}

public static bool operator !=(AxialCoordinates left, AxialCoordinates right)
{
    return !left.Equals(right);
}

public override string ToString()
{
    return $"({Q}, {R})";
}

// Deconstruction support for pattern matching
public void Deconstruct(out int q, out int r)
{
    q = Q;
    r = R;
}
}
