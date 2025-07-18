
    public class Track
    {
        public string Title { get; set; }
        public string FilePath { get; set; }
        public TimeSpan Duration { get; set; }
        public List<string> Tags { get; set; }

        public Track(string title, string filePath, TimeSpan duration, List<string> tags)
        {
            Title = title;
            FilePath = filePath;
            Duration = duration;
            Tags = tags;
        }
    }