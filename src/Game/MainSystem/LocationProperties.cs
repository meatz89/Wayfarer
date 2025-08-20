using System.Collections.Generic;

namespace Wayfarer.Game
{
    // Minimal location property types to fix compilation
    // These are simplified from the old system

    public interface ILocationProperty
    {
        string GetDescription();
    }

    public class Population : ILocationProperty
    {
        public int Count { get; set; }
        public string Type { get; set; }

        public string GetDescription() => $"{Count} {Type}";
    }

    public class Atmosphere : ILocationProperty
    {
        public string Mood { get; set; }
        public string Activity { get; set; }

        public string GetDescription() => $"{Mood}, {Activity}";
    }

    public class Physical : ILocationProperty
    {
        public string Terrain { get; set; }
        public string Features { get; set; }

        public string GetDescription() => $"{Terrain} with {Features}";
    }

    public class Illumination : ILocationProperty
    {
        public string LightLevel { get; set; }
        public string Source { get; set; }

        public string GetDescription() => $"{LightLevel} from {Source}";
    }
}