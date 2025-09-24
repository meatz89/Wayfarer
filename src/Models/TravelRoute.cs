namespace Wayfarer.Models
{
    /// <summary>
    /// Strongly-typed representation of a travel route between two locations.
    /// Replaces tuple usage in TravelTimeCalculator for route keys.
    /// </summary>
    public record TravelRoute
    {
        /// <summary>
        /// The starting location ID
        /// </summary>
        public string FromLocationId { get; init; }

        /// <summary>
        /// The destination location ID
        /// </summary>
        public string ToLocationId { get; init; }

        /// <summary>
        /// Create a travel route
        /// </summary>
        public TravelRoute(string fromLocationId, string toLocationId)
        {
            FromLocationId = fromLocationId;
            ToLocationId = toLocationId;
        }

        /// <summary>
        /// Override GetHashCode for dictionary usage
        /// </summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(FromLocationId, ToLocationId);
        }

        /// <summary>
        /// String representation for debugging
        /// </summary>
        public override string ToString()
        {
            return $"{FromLocationId} -> {ToLocationId}";
        }
    }
}