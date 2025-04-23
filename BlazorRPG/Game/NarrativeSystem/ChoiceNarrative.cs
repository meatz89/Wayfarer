// Main service class - slim coordinator
// Container for choice narrative elements
public class ChoiceNarrative
{
    public string ShorthandName { get; set; }
    public string FullDescription { get; set; }

    public ChoiceNarrative(string shorthandName, string fullDescription)
    {
        ShorthandName = shorthandName;
        FullDescription = fullDescription;
    }
}
