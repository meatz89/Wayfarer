[Serializable]
public class CompletionMessage4o
{
    public string role;
    public List<CompletionMessageContent4o> content;

    private const string NewLine = "\r\n";

    public override string ToString()
    {
        string s = string.Empty;
        foreach (CompletionMessageContent4o item in content)
        {
            s += $"{role}: {item.text}";
            s += NewLine;
        }
        return s;
    }
}


[Serializable]
public class CompletionMessageContent4o
{
    public string type;
    public string text;
}


[Serializable]
public class Completion4oModel
{
    public string model;
    public List<CompletionMessage4o> messages;
    public object response_format;
    public double temperature;
    public int max_completion_tokens;
    public double top_p;
    public double frequency_penalty;
    public double presence_penalty;
}